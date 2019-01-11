// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using logger = CosmosCloneCommon.Utility.CloneLogger;

namespace CosmosCloneCommon.Utility
{
    public class CosmosBulkImporter
    {
        private IBulkExecutor bulkExecutor;
        private static readonly ConnectionPolicy ConnectionPolicy = new ConnectionPolicy
        {
            ConnectionMode = ConnectionMode.Direct,
            ConnectionProtocol = Protocol.Tcp
        };
        public CosmosBulkImporter()
        {
            //var TargetCosmosDBSettings = CloneSettings.GetConfigurationSection("TargetCosmosDBSettings");
            var TargetEndpointUrl = CloneSettings.TargetSettings.EndpointUrl;
            var TargetAccessKey = CloneSettings.TargetSettings.AccessKey;
            var TargetDatabaseName = CloneSettings.TargetSettings.DatabaseName;
            var TargetCollectionName = CloneSettings.TargetSettings.CollectionName;
        }

        public async Task InitializeBulkExecutor(DocumentClient targetClient, DocumentCollection targetCollection)
        {
            logger.LogInfo("Inside InitializeBulkExecutor ");
            // Set retry options high for initialization (default values).
            targetClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            targetClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

            bulkExecutor = new BulkExecutor(targetClient, targetCollection);
            await bulkExecutor.InitializeAsync();
            // Set retries to 0 to pass control to bulk executor.
            targetClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            targetClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;
        }
        public async Task<BulkImportResponse> BulkSendToNewCollection<T>(List<T> entityList)
        {
            BulkImportResponse bulkImportResponse = null;
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            int attempts = 0;
            var objList = entityList.Cast<Object>();
            do
            {
                bulkImportResponse = await bulkExecutor.BulkImportAsync(
                    documents: objList,
                    enableUpsert: true,
                    disableAutomaticIdGeneration: true,
                    maxConcurrencyPerPartitionKeyRange: null,
                    maxInMemorySortingBatchSize: null,
                    cancellationToken: token);
                attempts++;
            } while (bulkImportResponse.NumberOfDocumentsImported < entityList.Count() && attempts <= 5);

            var badDocumentList = bulkImportResponse.BadInputDocuments;

            #region log bulk Summary
            logger.LogInfo(String.Format("\n Batch Upload completed "));
            logger.LogInfo("--------------------------------------------------------------------- ");
            logger.LogInfo(String.Format("Inserted {0} docs @ {1} writes/s, {2} RU/s in {3} sec",
                bulkImportResponse.NumberOfDocumentsImported,
                Math.Round(bulkImportResponse.NumberOfDocumentsImported / bulkImportResponse.TotalTimeTaken.TotalSeconds, 2),
                Math.Round(bulkImportResponse.TotalRequestUnitsConsumed / bulkImportResponse.TotalTimeTaken.TotalSeconds, 2),
                bulkImportResponse.TotalTimeTaken.TotalSeconds));
            logger.LogInfo(String.Format("Average RU consumption per document: {0}",
                Math.Round(bulkImportResponse.TotalRequestUnitsConsumed / bulkImportResponse.NumberOfDocumentsImported, 2)));

            if (badDocumentList != null && badDocumentList.Count > 0)
            {
                logger.LogInfo($"bad Documents detected {badDocumentList.Count}");
            }

            logger.LogInfo("---------------------------------------------------------------------\n ");
            #endregion

            return bulkImportResponse;
        }

    }
}
