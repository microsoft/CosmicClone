// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using logger = CosmosCloneCommon.Utility.CloneLogger;
using CosmosCloneCommon.Utility;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosCloneCommon.Migrator
{
    public class CodeMigrator
        {
            #region declare variables
            private CosmosDBHelper cosmosHelper;
            private CosmosBulkImporter cosmosBulkImporter;
            protected DocumentClient sourceClient;
            protected DocumentClient targetClient;
            protected DocumentCollection sourceCollection;
            protected DocumentCollection targetCollection;
            private string SourceEndpointUrl;
            private string SourceAccessKey;
            private string sourceDatabaseName;
            private string sourceCollectionName;

            private string TargetEndpointUrl;
            private string TargetAccessKey;
            private string TargetDatabaseName;
            private string TargetCollectionName;
            //private EntitySummary summary;

            #endregion

            public CodeMigrator()
            {
                //initialize settings and other utilities
                var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
                SourceEndpointUrl = CloneSettings.SourceSettings.EndpointUrl; ;
                SourceAccessKey = CloneSettings.SourceSettings.AccessKey;
                sourceDatabaseName = CloneSettings.SourceSettings.DatabaseName;
                sourceCollectionName = CloneSettings.SourceSettings.CollectionName;

                //var TargetCosmosDBSettings = CloneSettings.GetConfigurationSection("TargetCosmosDBSettings");
                TargetEndpointUrl = CloneSettings.TargetSettings.EndpointUrl;
                TargetAccessKey = CloneSettings.TargetSettings.AccessKey;
                TargetDatabaseName = CloneSettings.TargetSettings.DatabaseName;
                TargetCollectionName = CloneSettings.TargetSettings.CollectionName;

                cosmosHelper = new CosmosDBHelper();
                cosmosBulkImporter = new CosmosBulkImporter();
                //summary = new EntitySummary();
                //summary.EntityType = "DBCode";
            }
            private async Task Initialize()
            {
                logger.LogInfo("-----------------------------------------------");
                logger.LogInfo("Begin CosmosDBCodeMigrator");

                sourceClient = cosmosHelper.GetSourceDocumentDbClient();
                sourceCollection = await cosmosHelper.GetSourceDocumentCollection(sourceClient);

                targetClient = cosmosHelper.GetTargetDocumentDbClient();
                targetCollection = await cosmosHelper.CreateTargetDocumentCollection(targetClient, sourceCollection.IndexingPolicy, sourceCollection.PartitionKey);
            }

            public async Task<bool> StartCopy()
            {
                await Initialize();
                if (CloneSettings.CopyStoredProcedures) { await CopyStoredProcedures(); }
                if (CloneSettings.CopyUDFs) { await CopyUDFs(); }
                if (CloneSettings.CopyTriggers) { await CopyTriggers(); }

                //summary.isMigrationComplete = true;
                logger.LogInfo("CosmosDBCodeMigrator End");
                logger.LogInfo("-----------------------------------------------");
                //return summary;
                return true;
            }

            private async Task CopyTriggers()
            {
                try
                {
                    logger.LogInfo("-----------------------------------------------");
                    logger.LogInfo("Begin CopyTriggers");
                    FeedOptions feedOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
                    var requestOptions = new RequestOptions { OfferEnableRUPerMinuteThroughput = true };
                    var triggerFeedResponse = await sourceClient.ReadTriggerFeedAsync(UriFactory.CreateDocumentCollectionUri(sourceDatabaseName, sourceCollectionName), feedOptions);
                    var triggerList = triggerFeedResponse.ToList();
                    logger.LogInfo($"Triggers retrieved from source {triggerList.Count}");
                    //summary.totalRecordsRetrieved += triggerList.Count;

                    var targetResponse = await targetClient.ReadTriggerFeedAsync(UriFactory.CreateDocumentCollectionUri(TargetDatabaseName, TargetCollectionName), feedOptions);
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
                        await targetClient.CreateTriggerAsync(UriFactory.CreateDocumentCollectionUri(TargetDatabaseName, TargetCollectionName), trigger, requestOptions);
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
                    var udfFeedResponse = await sourceClient.ReadUserDefinedFunctionFeedAsync(UriFactory.CreateDocumentCollectionUri(sourceDatabaseName, sourceCollectionName), feedOptions);
                    var udfList = udfFeedResponse.ToList<UserDefinedFunction>();
                    logger.LogInfo($"UDFs retrieved from source {udfList.Count}");
                    //summary.totalRecordsRetrieved += udfList.Count;

                    var targetResponse = await targetClient.ReadUserDefinedFunctionFeedAsync(UriFactory.CreateDocumentCollectionUri(TargetDatabaseName, TargetCollectionName), feedOptions);
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
                        await targetClient.CreateUserDefinedFunctionAsync(UriFactory.CreateDocumentCollectionUri(TargetDatabaseName, TargetCollectionName), udf, requestOptions);
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
                    var sourceResponse = await sourceClient.ReadStoredProcedureFeedAsync(UriFactory.CreateDocumentCollectionUri(sourceDatabaseName, sourceCollectionName), feedOptions);
                    var splist = sourceResponse.ToList<StoredProcedure>();
                    logger.LogInfo($"StoredProcedures retrieved from source {splist.Count}");
                    //summary.totalRecordsRetrieved += splist.Count;

                    var targetResponse = await targetClient.ReadStoredProcedureFeedAsync(UriFactory.CreateDocumentCollectionUri(TargetDatabaseName, TargetCollectionName), feedOptions);
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
                        await targetClient.CreateStoredProcedureAsync(UriFactory.CreateDocumentCollectionUri(TargetDatabaseName, TargetCollectionName), sp, requestOptions);
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
