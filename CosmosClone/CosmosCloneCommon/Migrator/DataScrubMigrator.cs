// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Diagnostics;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.Documents.Linq;
using CosmosCloneCommon.Utility;
using CosmosCloneCommon.Model;
using logger = CosmosCloneCommon.Utility.CloneLogger;
using Newtonsoft.Json.Linq;

namespace CosmosCloneCommon.Migrator
{
    public class DataScrubMigrator
    {
        #region declare variables
        protected int ReadDelaybetweenRequestsInMs = 2000;
        protected CosmosDBHelper cosmosHelper;
        protected CosmosBulkImporter cosmosBulkImporter;
        protected DocumentClient targetClient;
        protected DocumentCollection targetCollection;
        //protected ScrubRule scrubRule;
        protected IQueryable<string> ScrubDataFetchQuery;

        public static List<ScrubRule> scrubRules;
        public long TotalRecordsRetrieved { get; set; }
        public long TotalRecordsScrubbed { get; set; }
        public long TotalRecordsToScrub { get; set; }
        #endregion
        public DataScrubMigrator()
        {
            cosmosHelper = new CosmosDBHelper();
            cosmosBulkImporter = new CosmosBulkImporter();
        }
        public async Task<bool> StartScrub(List<ScrubRule> scrubRules)
        {
            DataScrubMigrator.scrubRules = scrubRules;

            if (!CloneSettings.ScrubbingRequired)
            {
                logger.LogInfo("No Scrubbing required");
                return false;
            }
            await InitializeMigration();
            //group by filtered Rules
            var distinctFilters = scrubRules.Select(o => o.FilterCondition).Distinct();
            //get distinct filterConditions
            //foreach filterCondition obtain set of rules and send at once
            foreach (var filterCondition in distinctFilters)
            {

                logger.LogInfo($"Initialize process for scrub rule on filter {filterCondition}");
                var sRules = scrubRules.Where(o => o.FilterCondition.Equals(filterCondition)).ToList();
                logger.LogInfo($"Scrub rules found {sRules.Count}");
                long filterRecordCount = cosmosHelper.GetFilterRecordCount(filterCondition);
                ScrubDataFetchQuery = cosmosHelper.GetScrubDataDocumentQuery<string>(targetClient, filterCondition, CloneSettings.ReadBatchSize);
                await ReadUploadInbatches((IDocumentQuery<string>)ScrubDataFetchQuery, sRules);

                foreach(var srule in DataScrubMigrator.scrubRules)
                {
                    if(srule.FilterCondition.Equals(filterCondition))
                    {
                        srule.IsComplete = true;
                        srule.RecordsByFilter = filterRecordCount;
                    }
                }
            }

            return true;
        }
        public async Task InitializeMigration()
        {
            logger.LogInfo("Initialize data scrubbing");
            targetClient = cosmosHelper.GetTargetDocumentDbClient();
            targetCollection = await cosmosHelper.GetTargetDocumentCollection(targetClient);
            await cosmosBulkImporter.InitializeBulkExecutor(targetClient, targetCollection);
        }      

        public async Task ReadUploadInbatches(IDocumentQuery<string> query, List<ScrubRule> scrubRules)
        {
            #region batchVariables
            //initialize Batch Process variables                    
            int batchCount = 0;
            TotalRecordsRetrieved = 0;
            TotalRecordsScrubbed = 0;
            var badEntities = new List<Object>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var objScrubber = new ObjectScrubber();
            #endregion
            while (query.HasMoreResults)
            {
                batchCount++;
                logger.LogInfo($"BatchNumber : {batchCount} begins ");
                List<string> entities = await GetCommonStringEntitiesinBatch(query);
                TotalRecordsRetrieved += entities.Count();
                BulkImportResponse uploadResponse = new BulkImportResponse();
                var scrubbedEntities = entities;
                if (entities.Any())
                {
                    var jEntities = new List<JToken>();
                    foreach (var scrubRule in scrubRules)
                    {
                        jEntities = objScrubber.ScrubObjectList(scrubbedEntities, scrubRule);
                        var nentities = new List<string>();
                        foreach (var jobj in jEntities)
                        {
                            nentities.Add(JsonConvert.SerializeObject(jobj));
                        }
                        scrubbedEntities = nentities;
                        scrubRule.RecordsUpdated += jEntities.Count;
                    }                       
                    var objEntities = jEntities.Cast<Object>().ToList();
                    try
                    {
                        uploadResponse = await cosmosBulkImporter.BulkSendToNewCollection<dynamic>(objEntities);
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(ex);
                        throw (ex);                        
                    }                                    
                }
                badEntities = uploadResponse.BadInputDocuments;
                TotalRecordsScrubbed += uploadResponse.NumberOfDocumentsImported;

                logger.LogInfo($"Summary of Batch {batchCount} records retrieved {entities.Count()}. Records Uploaded: {uploadResponse.NumberOfDocumentsImported}");
                logger.LogInfo($"Total records retrieved {TotalRecordsRetrieved}. Total records uploaded {TotalRecordsScrubbed}");
                logger.LogInfo($"Time elapsed : {stopwatch.Elapsed} ");
            }

            stopwatch.Stop();
            logger.LogInfo("Document Scrubbing completed");
        }

        protected async Task<List<string>> GetCommonStringEntitiesinBatch(IDocumentQuery<string> query)
        {
            List<string> entities = new List<string>();
            List<object> objEntities = new List<object>();
            int attempts = 0;
            try
            {
                while (query.HasMoreResults && objEntities.Count < CloneSettings.WriteBatchSize)
                {
                    attempts++;
                    var prevRecordCount = entities.Count;
                    var res = await query.ExecuteNextAsync();
                    objEntities.AddRange((IEnumerable<object>)res);
                    logger.LogInfo($"Records retrieved from source: {objEntities.Count - prevRecordCount}");
                }
                foreach(var obj in objEntities)
                {                   
                    entities.Add(JsonConvert.SerializeObject(obj));                 
                }
                logger.LogInfo($"Total Records retrieved from Source {entities.Count}");
                return entities;
            }
            catch (Exception ex)
            {
                logger.LogInfo("Exception while fetching Entities from source. Will Increase delay time and continue");
                logger.LogError(ex);
                ReadDelaybetweenRequestsInMs += 2000;
                System.Threading.Thread.Sleep(ReadDelaybetweenRequestsInMs);
                return entities;
            }

        }
    }
}
