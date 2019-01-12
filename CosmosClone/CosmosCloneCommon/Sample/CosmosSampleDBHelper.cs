// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using CosmosCloneCommon.Model;
using logger = CosmosCloneCommon.Utility.CloneLogger;
using CosmosCloneCommon.Utility;

namespace CosmosCloneCommon.Sample
{
    public class CosmosSampleDBHelper
    {        
        private ConnectionPolicy ConnectionPolicy;
        public CosmosSampleDBHelper()
        {
            ConnectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp,
                RetryOptions = new RetryOptions()
            };
            this.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 20;
            this.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 600;
        }
        
        public DocumentClient GetSampleDocumentDbClient()
        {
            try
            {
                var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SampleCosmosDBSettings");
                string SourceEndpointUrl = SourceCosmosDBSettings["EndpointUrl"];
                string SourceAccessKey = SourceCosmosDBSettings["AccessKey"];
                var sourceDocumentClient = new DocumentClient(new Uri(SourceEndpointUrl), SourceAccessKey, ConnectionPolicy);
                return sourceDocumentClient;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public async Task<DocumentCollection> CreateSampleDocumentCollection(DocumentClient sampleClient, bool IsFixedCollection = false)
        {
            try
            {
                var sampleCosmosDBSettings = CloneSettings.GetConfigurationSection("SampleCosmosDBSettings");
                string sampleDatabaseName = sampleCosmosDBSettings["DatabaseName"]; ;
                string sampleCollectionName = sampleCosmosDBSettings["CollectionName"];
                int offerThroughput = 1000;
                int.TryParse(sampleCosmosDBSettings["OfferThroughputRUs"], out offerThroughput);
                await sampleClient.CreateDatabaseIfNotExistsAsync(new Database { Id = sampleDatabaseName });
                DocumentCollection newDocumentCollection;
                if (!IsFixedCollection)
                {
                    var partitionKeyDefinition = new PartitionKeyDefinition();
                    partitionKeyDefinition.Paths.Add("/CompositeName");
                    newDocumentCollection = (DocumentCollection)await sampleClient.CreateDocumentCollectionIfNotExistsAsync
                                            (UriFactory.CreateDatabaseUri(sampleDatabaseName),
                                            new DocumentCollection { Id = sampleCollectionName, PartitionKey = partitionKeyDefinition },
                                            new RequestOptions { OfferThroughput = offerThroughput });
                }
                else
                {
                    //no partition key if it is a fixed collection
                    newDocumentCollection = (DocumentCollection)await sampleClient.CreateDocumentCollectionIfNotExistsAsync
                                           (UriFactory.CreateDatabaseUri(sampleDatabaseName),
                                           new DocumentCollection { Id = sampleCollectionName},
                                           new RequestOptions { OfferThroughput = offerThroughput });
                }
                return newDocumentCollection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public async Task<DocumentCollection> CreateTargetDocumentCollection(DocumentClient targetClient, IndexingPolicy indexingPolicy, PartitionKeyDefinition partitionKeyDefinition)
        {
            try
            {
                //var targetCosmosDBSettings = CloneSettings.GetConfigurationSection("TargetCosmosDBSettings");
                string targetDatabaseName = CloneSettings.TargetSettings.DatabaseName;
                string targetCollectionName = CloneSettings.TargetSettings.CollectionName;

                await targetClient.CreateDatabaseIfNotExistsAsync(new Database { Id = targetDatabaseName });
                DocumentCollection newDocumentCollection;
                if (partitionKeyDefinition != null && partitionKeyDefinition.Paths.Count>0)
                {
                    if(CloneSettings.CopyPartitionKey)
                    { 
                    // Partition key exists in Source (Unlimited Storage)
                    newDocumentCollection = (DocumentCollection)await targetClient.CreateDocumentCollectionIfNotExistsAsync
                                        (UriFactory.CreateDatabaseUri(targetDatabaseName),
                                        new DocumentCollection { Id = targetCollectionName, PartitionKey = partitionKeyDefinition, IndexingPolicy = indexingPolicy },
                                        new RequestOptions { OfferEnableRUPerMinuteThroughput = true, OfferThroughput = CloneSettings.TargetMigrationOfferThroughputRUs });
                    }
                    else
                    {
                    newDocumentCollection = (DocumentCollection)await targetClient.CreateDocumentCollectionIfNotExistsAsync
                                         (UriFactory.CreateDatabaseUri(targetDatabaseName),
                                         new DocumentCollection { Id = targetCollectionName,  IndexingPolicy = indexingPolicy },
                                         new RequestOptions { OfferEnableRUPerMinuteThroughput = true, OfferThroughput = CloneSettings.TargetMigrationOfferThroughputRUs });
                    }
                }
                else
                {   //no partition key set in source (Fixed storage)
                    newDocumentCollection = (DocumentCollection)await targetClient.CreateDocumentCollectionIfNotExistsAsync
                                       (UriFactory.CreateDatabaseUri(targetDatabaseName),
                                       new DocumentCollection { Id = targetCollectionName, IndexingPolicy = indexingPolicy },
                                       new RequestOptions { OfferEnableRUPerMinuteThroughput = true, OfferThroughput = CloneSettings.TargetMigrationOfferThroughputRUs });
                }
                logger.LogInfo($"SuccessFully Created Target. Database: {targetDatabaseName} Collection:{targetCollectionName}");
                return newDocumentCollection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }
   
    }
}
