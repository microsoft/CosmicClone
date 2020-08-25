// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Diagnostics;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.Documents.Linq;
using CosmosCloneCommon.Utility;
using CosmosCloneCommon.Model;
using Newtonsoft.Json;
using logger = CosmosCloneCommon.Utility.CloneLogger;
using Newtonsoft.Json.Linq;

namespace CosmosCloneCommon.Migrator
{
    public class DocumentMigrator
    {
        #region declare variables
        protected int ReadDelaybetweenRequestsInMs = 2000;
        
        protected CosmosDBHelper cosmosHelper;
        protected CosmosBulkImporter cosmosBulkImporter;
        protected DocumentClient sourceClient;
        
        protected DocumentClient targetClient;

        protected DocumentCollection sourceCollection;
        protected DocumentCollection targetCollection;
        protected ObjectScrubber objectScrubber;
        //protected static List<ScrubRule> scrubRules;
        protected List<ScrubRule> noFilterScrubRules;
        protected List<ScrubRule> filteredScrubRules;


        protected IQueryable<dynamic> SourceCommonDataFetchQuery;

        public static List<ScrubRule> scrubRules;
        public static long TotalRecordsRetrieved { get; set; }
        public static long TotalRecordsSent { get; set; }
        public static long TotalRecordsInSource { get; set; }
        public static bool IsCodeMigrationComplete { get; set; }
        protected static bool IsInitialized { get; set; }

        public static int ScrubPercentProgress
        {
            get
            {
                if (scrubRules != null && scrubRules.Count > 0)
                {
                    int totalRules = scrubRules.Count();
                    int noFilterRuleCompleteCount = scrubRules.Where(x => x.IsComplete == true && string.IsNullOrEmpty(x.FilterCondition)).ToList().Count();
                    int filterRuleCompleteCount = 0;
                    if (DataScrubMigrator.scrubRules != null && DataScrubMigrator.scrubRules.Count > 0)
                    {
                        filterRuleCompleteCount = DataScrubMigrator.scrubRules.Where(x => x.IsComplete == true).ToList().Count();
                    }

                    var completedRules = scrubRules.Where(x => x.IsComplete == true).ToList().Count();
                    int percent = (int)((noFilterRuleCompleteCount + filterRuleCompleteCount) * 100 / scrubRules.Count());
                    return percent;
                }
                else if (IsInitialized) return 100;
                else return 0;
            }            
        }

        #endregion

        public DocumentMigrator()
        {
            cosmosHelper = new CosmosDBHelper();
            cosmosBulkImporter = new CosmosBulkImporter();
            objectScrubber = new ObjectScrubber();
        }
        public async Task<bool> StartCopy(List<ScrubRule> scrubRules = null)
        {
            IsCodeMigrationComplete = false;
            DocumentMigrator.scrubRules = scrubRules;

            await InitializeMigration();
            if (CloneSettings.CopyDocuments)
            {
                await ReadUploadInBatches((IDocumentQuery<dynamic>)SourceCommonDataFetchQuery);
            }
            else
            {
                //scrub documents for rules without filters(no where condition)
                //This is also part of copy documents code hence this is included here when copydocuments is turned off
                if (CloneSettings.ScrubbingRequired && noFilterScrubRules != null && noFilterScrubRules.Count > 0)
                {
                    var dcs = new DataScrubMigrator();
                    var result = await dcs.StartScrub(noFilterScrubRules);
                }
            }
            
            if (CloneSettings.ScrubbingRequired && filteredScrubRules != null && filteredScrubRules.Count > 0)
            {
                var dcs = new DataScrubMigrator();
                var result = await dcs.StartScrub(filteredScrubRules);
            }
            
            logger.LogScrubRulesInformation(DocumentMigrator.scrubRules);

            if (CloneSettings.CopyStoredProcedures) { await CopyStoredProcedures(); }
            if (CloneSettings.CopyUDFs) { await CopyUDFs(); }
            if (CloneSettings.CopyTriggers) { await CopyTriggers(); }
            IsCodeMigrationComplete = true;

            return true;
        }
        public async Task InitializeMigration()
        {
            logger.LogInfo("Begin Document Migration.");
            logger.LogInfo($"Source Database: {CloneSettings.SourceSettings.DatabaseName} Source Collection: {CloneSettings.SourceSettings.CollectionName}");
            logger.LogInfo($"Target Database: {CloneSettings.TargetSettings.DatabaseName} Target Collection: {CloneSettings.TargetSettings.CollectionName}");

            IsInitialized = true;
            sourceClient = cosmosHelper.GetSourceDocumentDbClient();
            sourceCollection = await cosmosHelper.GetSourceDocumentCollection(sourceClient);

            targetClient = cosmosHelper.GetTargetDocumentDbClient();
            var indexPolicy = (CloneSettings.CopyIndexingPolicy)? sourceCollection.IndexingPolicy : new IndexingPolicy();
            targetCollection = await cosmosHelper.CreateTargetDocumentCollection(targetClient, indexPolicy, sourceCollection.PartitionKey);         

            if (CloneSettings.CopyDocuments)
            {
                TotalRecordsInSource = cosmosHelper.GetSourceRecordCount();
                logger.LogInfo($"Total records in Source: {TotalRecordsInSource} ");
                SourceCommonDataFetchQuery = cosmosHelper.GetSourceEntityDocumentQuery<dynamic>(sourceClient, CloneSettings.SourceSettings.SelectQuery, CloneSettings.ReadBatchSize);
                await cosmosBulkImporter.InitializeBulkExecutor(targetClient, targetCollection);
            }
            else
            {
                logger.LogInfo("Document Migration is disabled through configuration. ");
            }

            if (CloneSettings.ScrubbingRequired && scrubRules != null && scrubRules.Count > 0)
            {
                noFilterScrubRules = new List<ScrubRule>();
                filteredScrubRules = new List<ScrubRule>();
                foreach (var sRule in scrubRules)
                {
                    if(string.IsNullOrEmpty(sRule.FilterCondition))
                    {
                        sRule.RecordsByFilter = TotalRecordsInSource;
                        noFilterScrubRules.Add(sRule);
                    }
                    else
                    {
                        filteredScrubRules.Add(sRule);
                        //DataScrubMigrator.scrubRules.Add(sRule);
                    }
                }
            }
        }
        
        public async Task ReadUploadInBatches(IDocumentQuery<dynamic> query) 
        {

            #region batchVariables
            //initialize Batch Process variables                    
            int batchCount = 0;
            TotalRecordsRetrieved = 0;
            TotalRecordsSent = 0;
            var badEntities = new List<Object>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            #endregion
            while (query.HasMoreResults)
            {
                batchCount++;
                logger.LogInfo($"BatchNumber : {batchCount} begins ");
                List<dynamic> entities = await GetCommonEntitiesinBatch(query);
                TotalRecordsRetrieved += entities.Count();
                List<object> objEntities = new List<object>();
                objEntities.AddRange((IEnumerable<object>)entities);
                List<string> strEntities = new List<string>();
                foreach (var obj in objEntities)
                {
                    strEntities.Add(JsonConvert.SerializeObject(obj));
                }

                BulkImportResponse uploadResponse = new BulkImportResponse();
                var scrubbedEntities = strEntities;
                if (entities.Any())
                {
                    if( noFilterScrubRules == null || noFilterScrubRules.Count==0)
                    {                        
                        uploadResponse = await cosmosBulkImporter.BulkSendToNewCollection<dynamic>(entities);
                    }
                    else
                    {
                        var jEntities = new List<JToken>();                        
                        foreach (var sRule in noFilterScrubRules)
                        {
                            jEntities = objectScrubber.ScrubObjectList(scrubbedEntities, sRule);
                            var nentities = new List<string>();
                            foreach (var jobj in jEntities)
                            {
                                nentities.Add(JsonConvert.SerializeObject(jobj));
                            }
                            scrubbedEntities = nentities;
                            sRule.RecordsUpdated += jEntities.Count;
                        }
                        var objDocuments = jEntities.Cast<Object>().ToList();
                        uploadResponse = await cosmosBulkImporter.BulkSendToNewCollection<dynamic>(objDocuments);
                    }                    
                }
                else
                {
                    logger.LogInfo("No Entities retrieved from query");
                    continue;
                }
                badEntities = uploadResponse.BadInputDocuments;
                TotalRecordsSent += uploadResponse.NumberOfDocumentsImported;

                logger.LogInfo($"Summary of Batch {batchCount} records retrieved {entities.Count()}. Records Uploaded: {uploadResponse.NumberOfDocumentsImported}");
                logger.LogInfo($"Total records retrieved {TotalRecordsRetrieved}. Total records uploaded {TotalRecordsSent}");
                logger.LogInfo($"Time elapsed : {stopwatch.Elapsed} ");
            }
            SetCompleteOnNoFilterRules();
            stopwatch.Stop();
            logger.LogInfo("Document Migration completed");
        }

        public async Task TestOffers()
        {
            //sourceClient = cosmosHelper.GetSourceDocumentDbClient();
            //sourceCollection = await cosmosHelper.GetSourceDocumentCollection(sourceClient);
            var setCorrect = await cosmosHelper.SetTargetRestOfferThroughput();
        }
        public bool SetCompleteOnNoFilterRules()
        {
            if (scrubRules != null && scrubRules.Count > 0)
            {
                foreach (var sRule in scrubRules)
                {
                    if (string.IsNullOrEmpty(sRule.FilterCondition))
                    {
                        sRule.IsComplete = true;
                    }
                }
            }
            if (this.noFilterScrubRules != null && this.noFilterScrubRules.Count > 0)
            {
                foreach (var sRule in this.noFilterScrubRules)
                {
                    if (string.IsNullOrEmpty(sRule.FilterCondition))
                    {
                        sRule.IsComplete = true;
                    }
                }
            }
            return true;
        }


        protected async Task<List<dynamic>> GetCommonEntitiesinBatch(IDocumentQuery<dynamic> query)
        {
            List<dynamic> entities = new List<dynamic>();
            int attempts = 0;
            try
            {
                while (query.HasMoreResults && entities.Count < CloneSettings.WriteBatchSize)
                {                    
                    attempts++;
                    var prevRecordCount = entities.Count;
                    var res = await query.ExecuteNextAsync();
                    entities.AddRange((IEnumerable<dynamic>)res);
                    logger.LogInfo($"Records retrieved from source: {entities.Count - prevRecordCount}");
                }
                logger.LogInfo($"Total Records retrieved from Source {entities.Count}");
                return entities;
            }
            catch (Exception ex)
            {
                logger.LogInfo("Exception while fetching Entities from source. Will Increase delay time and continue");
                logger.LogError(ex);
                ReadDelaybetweenRequestsInMs += 2000;//increased delay
                System.Threading.Thread.Sleep(ReadDelaybetweenRequestsInMs);
                return entities;
            }
        }

        private async Task CopyTriggers()
        {
            try
            {
                logger.LogInfo("-----------------------------------------------");
                logger.LogInfo("Begin CopyTriggers");
                FeedOptions feedOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
                var requestOptions = new RequestOptions { OfferEnableRUPerMinuteThroughput = true };
                var triggerFeedResponse = await sourceClient.ReadTriggerFeedAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.SourceSettings.DatabaseName, CloneSettings.SourceSettings.CollectionName), feedOptions);
                var triggerList = triggerFeedResponse.ToList();
                logger.LogInfo($"Triggers retrieved from source {triggerList.Count}");
                //summary.totalRecordsRetrieved += triggerList.Count;

                var targetResponse = await targetClient.ReadTriggerFeedAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.TargetSettings.DatabaseName, CloneSettings.TargetSettings.CollectionName), feedOptions);
                var targetTriggerList = targetResponse.ToList();
                logger.LogInfo($"Triggers already in target {targetTriggerList.Count}");
                var targetTriggerIds = new HashSet<string>();
                targetTriggerList.ForEach(sp => targetTriggerIds.Add(sp.Id));

                foreach (var trigger in triggerList)
                {
                    if (targetTriggerIds.Contains(trigger.Id))
                    {
                        logger.LogInfo($"Trigger {trigger.Id} already Exists in destination DB");
                        continue;
                    }
                    logger.LogInfo($"Create Trigger {trigger.Id} start");
                    await targetClient.CreateTriggerAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.TargetSettings.DatabaseName, CloneSettings.TargetSettings.CollectionName), trigger, requestOptions);
                    logger.LogInfo($"Create Trigger {trigger.Id} complete");
                    //summary.totalRecordsSent++;
                }
                logger.LogInfo("Copy Triggers end.");
                logger.LogInfo("");
            }
            catch (Exception ex)
            {
                logger.LogInfo("Exception while CopyTriggers");
                logger.LogError(ex);
            }
        }

        private async Task CopyUDFs()
        {
            try
            {
                logger.LogInfo("-----------------------------------------------");
                logger.LogInfo("Begin CopyUDFs");
                FeedOptions feedOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
                var udfFeedResponse = await sourceClient.ReadUserDefinedFunctionFeedAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.SourceSettings.DatabaseName, CloneSettings.SourceSettings.CollectionName), feedOptions);
                var udfList = udfFeedResponse.ToList<UserDefinedFunction>();
                logger.LogInfo($"UDFs retrieved from source {udfList.Count}");
                //summary.totalRecordsRetrieved += udfList.Count;

                var targetResponse = await targetClient.ReadUserDefinedFunctionFeedAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.TargetSettings.DatabaseName, CloneSettings.TargetSettings.CollectionName), feedOptions);
                var targetUdfList = targetResponse.ToList();
                logger.LogInfo($"Triggers already in target {targetUdfList.Count}");
                var targetUDFIds = new HashSet<string>();
                targetUdfList.ForEach(sp => targetUDFIds.Add(sp.Id));

                var requestOptions = new RequestOptions { OfferEnableRUPerMinuteThroughput = true };
                foreach (var udf in udfList)
                {
                    if (targetUDFIds.Contains(udf.Id))
                    {
                        logger.LogInfo($"UDF {udf.Id} already Exists in destination DB");
                        continue;
                    }
                    logger.LogInfo($"Create Trigger {udf.Id} start");
                    await targetClient.CreateUserDefinedFunctionAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.TargetSettings.DatabaseName, CloneSettings.TargetSettings.CollectionName), udf, requestOptions);
                    logger.LogInfo($"Create Trigger {udf.Id} complete");
                    //summary.totalRecordsSent++;
                }
                logger.LogInfo("CopyUDFs end.");
                logger.LogInfo("");
            }
            catch (Exception ex)
            {
                logger.LogInfo("Exception while CopyUDFs");
                logger.LogError(ex);
            }
        }
        private async Task CopyStoredProcedures()
        {
            try
            {
                logger.LogInfo("-----------------------------------------------");
                logger.LogInfo("Begin CopyStoredProcedures");
                FeedOptions feedOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
                var requestOptions = new RequestOptions { OfferEnableRUPerMinuteThroughput = true };
                var sourceResponse = await sourceClient.ReadStoredProcedureFeedAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.SourceSettings.DatabaseName, CloneSettings.SourceSettings.CollectionName), feedOptions);
                var splist = sourceResponse.ToList<StoredProcedure>();
                logger.LogInfo($"StoredProcedures retrieved from source {splist.Count}");
                //summary.totalRecordsRetrieved += splist.Count;

                var targetResponse = await targetClient.ReadStoredProcedureFeedAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.TargetSettings.DatabaseName, CloneSettings.TargetSettings.CollectionName), feedOptions);
                var targetSPList = targetResponse.ToList();
                logger.LogInfo($"StoredProcedures already retrieved in target {targetSPList.Count}");
                var targetSPIds = new HashSet<string>();
                targetSPList.ForEach(sp => targetSPIds.Add(sp.Id));

                foreach (var sp in splist)
                {
                    if (targetSPIds.Contains(sp.Id))
                    {
                        logger.LogInfo($"StoredProcedure {sp.Id} already Exists in destination DB");
                        continue;
                    }
                    logger.LogInfo($"Create StoredProcedure {sp.Id} start");
                    await targetClient.CreateStoredProcedureAsync(UriFactory.CreateDocumentCollectionUri(CloneSettings.TargetSettings.DatabaseName, CloneSettings.TargetSettings.CollectionName), sp, requestOptions);
                    logger.LogInfo($"Create StoredProcedure {sp.Id} complete");
                    //summary.totalRecordsSent++;
                }
                logger.LogInfo("CopyStoredProcedures end.");
            }
            catch (Exception ex)
            {
                logger.LogInfo("Exception while CopyStoredProcedures");
                logger.LogError(ex);
                logger.LogInfo("");
            }
        }

    }
}
