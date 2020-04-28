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

namespace CosmosCloneCommon.Utility
{
    public class CosmosDBHelper
    {
        private int OfferThroughput = 1000;
        private ConnectionPolicy ConnectionPolicy;

        public CosmosDBHelper()
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

        public ValidationResult TestSourceConnection()
        {
            var result = new ValidationResult();
            DocumentClient sourceDocumentClient;
            try
            {
                sourceDocumentClient = new DocumentClient(new Uri(CloneSettings.SourceSettings.EndpointUrl), CloneSettings.SourceSettings.AccessKey, ConnectionPolicy);

            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                result.IsSuccess = false;
                result.Message = "Unable to connect to Source. Check your input url and key are accurate. If Firewall security is enabled for your database please add the ip address of the current machine.";
                return result;
            }
            try
            {
                var cosmosDBURI = UriFactory.CreateDocumentCollectionUri(CloneSettings.SourceSettings.DatabaseName, CloneSettings.SourceSettings.CollectionName);
                //var sourceDatabase = await sourceDocumentClient.ReadDatabaseAsync(Database);
                var sourceCollection = sourceDocumentClient.ReadDocumentCollectionAsync(cosmosDBURI);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                result.IsSuccess = false;
                result.Message = "Incorrect DatabaseName or Collection. Check your input DatabaseName and Collection are accurate.";
                return result;
            }
            result.IsSuccess = true;
            result.Message = "OK";
            return result;
        }

        public ValidationResult TestTargetConnection()
        {
            var result = new ValidationResult();
            DocumentClient targetDocumentClient;
            try
            {
                targetDocumentClient = new DocumentClient(new Uri(CloneSettings.TargetSettings.EndpointUrl), CloneSettings.TargetSettings.AccessKey, ConnectionPolicy);

            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                result.IsSuccess = false;
                result.Message = "Unable to connect to Target. Check your input url and key are accurate. If Firewall security is enabled for your database please add the ip address of the current machine.";
                return result;
            }
            try
            {
                var cosmosDBURI = UriFactory.CreateDatabaseUri(CloneSettings.TargetSettings.DatabaseName);
                var sourceDatabase = targetDocumentClient.ReadDatabaseAsync(cosmosDBURI);
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                result.IsSuccess = false;
                result.Message = "Incorrect DatabaseName .Check whether the input DatabaseName is accurate.";
                return result;
            }
            result.IsSuccess = true;
            result.Message = "OK";
            return result;
        }

        public DocumentClient GetSourceDocumentDbClient()
        {
            try
            {                
                string SourceEndpointUrl = CloneSettings.SourceSettings.EndpointUrl;
                string SourceAccessKey = CloneSettings.SourceSettings.AccessKey;
                var sourceDocumentClient = new DocumentClient(new Uri(SourceEndpointUrl), SourceAccessKey, ConnectionPolicy);                
                return sourceDocumentClient;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public DocumentClient GetTargetDocumentDbClient()
        {
            try
            {
                var newConnectionPolicy = new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp,
                    RetryOptions = new RetryOptions()
                };
                newConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 20;
                newConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 600;

                string targetEndpointUrl = CloneSettings.TargetSettings.EndpointUrl;
                string targetAccessKey = CloneSettings.TargetSettings.AccessKey;
                OfferThroughput = CloneSettings.TargetMigrationOfferThroughputRUs;
                var targetDocumentClient = new DocumentClient(new Uri(targetEndpointUrl), targetAccessKey, newConnectionPolicy);
                return targetDocumentClient;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public DocumentClient GetTargetDocumentDbClient(ConnectionPolicy connectionPolicy)
        {
            try
            {
                var newConnectionPolicy = new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp,
                    RetryOptions = new RetryOptions()
                };
                newConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 20;
                newConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 600;

                string targetEndpointUrl = CloneSettings.TargetSettings.EndpointUrl;
                string targetAccessKey = CloneSettings.TargetSettings.AccessKey;
                OfferThroughput = CloneSettings.TargetMigrationOfferThroughputRUs;
                var targetDocumentClient = new DocumentClient(new Uri(targetEndpointUrl), targetAccessKey, newConnectionPolicy);
                return targetDocumentClient;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public async Task<DocumentCollection> GetTargetDocumentCollection(DocumentClient targetClient)
        {
            try
            {
                //var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
                string DatabaseName = CloneSettings.TargetSettings.DatabaseName;
                string CollectionName = CloneSettings.TargetSettings.CollectionName;

                var cosmosDBURI = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                //sourceClient.ReadDocumentCollectionAsync
                var targetCollection = (DocumentCollection)await targetClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName), new RequestOptions { OfferEnableRUPerMinuteThroughput = true, OfferThroughput = CloneSettings.TargetMigrationOfferThroughputRUs });
                //targetClient.ReadDocumentCollectionAsync()
                //var sourceCollection = (DocumentCollection)await sourceClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(sourceDatabaseName), new DocumentCollection { Id = sourceCollectionName });
                return targetCollection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public async Task<DocumentCollection> GetTargetDocumentCollection_v2(DocumentClient targetClient)
        {
            try
            {
                //var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
                string DatabaseName = CloneSettings.TargetSettings.DatabaseName;
                string CollectionName = CloneSettings.TargetSettings.CollectionName;

                var cosmosDBURI = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                //sourceClient.ReadDocumentCollectionAsync
                var targetCollection = (DocumentCollection)await targetClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName), new RequestOptions { OfferEnableRUPerMinuteThroughput = true, OfferThroughput = CloneSettings.TargetMigrationOfferThroughputRUs });
                //targetClient.ReadDocumentCollectionAsync()
                //var sourceCollection = (DocumentCollection)await sourceClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(sourceDatabaseName), new DocumentCollection { Id = sourceCollectionName });
                return targetCollection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public async Task<DocumentCollection> GetSourceDocumentCollection(DocumentClient sourceClient)
        {
            try
            {
                //var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
                string sourceDatabaseName = CloneSettings.SourceSettings.DatabaseName;
                string sourceCollectionName = CloneSettings.SourceSettings.CollectionName;

                var cosmosDBURI = UriFactory.CreateDocumentCollectionUri(sourceDatabaseName, sourceCollectionName);
                var sourceCollection = (DocumentCollection)await sourceClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(sourceDatabaseName), new DocumentCollection { Id = sourceCollectionName });
                return sourceCollection;
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

        public IQueryable<T> GetSourceEntityDocumentQuery<T>(DocumentClient sourceClient, int batchSize = -1)
        {
            try
            {
                //var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
                string sourceDatabaseName = CloneSettings.SourceSettings.DatabaseName;
                string sourceCollectionName = CloneSettings.SourceSettings.CollectionName;
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = batchSize, EnableCrossPartitionQuery = true };
                string EntityDataQuery = $"SELECT * FROM c";
                var documentQuery = sourceClient.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(sourceDatabaseName, sourceCollectionName), EntityDataQuery, queryOptions);
                return documentQuery;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public IQueryable<T> GetScrubDataDocumentQuery<T>(DocumentClient targetClient,string filterCondition, int batchSize = -1)
        {
            try
            {
                //var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
                string DatabaseName = CloneSettings.TargetSettings.DatabaseName;
                string CollectionName = CloneSettings.TargetSettings.CollectionName;
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = batchSize, EnableCrossPartitionQuery = true };
                string scrubDataQuery;

                if (string.IsNullOrEmpty(filterCondition))
                {
                    scrubDataQuery = "SELECT * FROM c";
                }
                else
                {
                    scrubDataQuery = "SELECT * FROM c" + " where " + filterCondition;
                }
                
                var documentQuery = targetClient.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName), scrubDataQuery, queryOptions);
                return documentQuery;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                throw;
            }
        }

        public async Task<bool> SetTargetRestOfferThroughput()
        {
            using (var client = GetTargetDocumentDbClient())
            {
                var collection =  this.GetTargetDocumentCollection(client);
                
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
                //var Ioffer = cosmosClient.CreateOfferQuery(queryOptions);
                //var offer = Ioffer.AsEnumerable().SingleOrDefault();
                Offer offer = client.CreateOfferQuery()
                                 .Where(r => r.ResourceLink == collection.Result.SelfLink)
                                 .AsEnumerable()
                                 .SingleOrDefault();
                offer = new OfferV2(offer, CloneSettings.TargetRestOfferThroughputRUs);
                await client.ReplaceOfferAsync(offer);
                //.Where(r => r.ResourceLink == collection.)
                //.AsEnumerable()
                //.SingleOrDefault();
            }
                return true;
        }
        public bool CheckSourceReadability()
        {
            string sourceDatabaseName = CloneSettings.SourceSettings.DatabaseName;
            string sourceCollectionName = CloneSettings.SourceSettings.CollectionName;
            string topOneRecordQuery = "SELECT TOP 1 * FROM c";
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            //long sourceTotalRecordCount, targetTotalRecordCount;
            try { 
                using (var cosmosClient = GetSourceDocumentDbClient())
                {
                    var document = cosmosClient.CreateDocumentQuery<dynamic>(
                                        UriFactory.CreateDocumentCollectionUri(sourceDatabaseName, sourceCollectionName), topOneRecordQuery, queryOptions)
                                        .AsEnumerable().First();
                    if (document != null)
                        return true;
                }
            }
            catch(Exception ex)
            {
                logger.LogInfo("Exception during CheckSource Readability");
                logger.LogError(ex);
            }
            return false;
        }
        
        public long GetFilterRecordCount(string filterCondition)
        {
            //var TargetCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
            string targetDatabaseName = CloneSettings.TargetSettings.DatabaseName;
            string targetCollectionName = CloneSettings.TargetSettings.CollectionName;            

            string totalCountQuery = "SELECT VALUE COUNT(1) FROM c";
            if(!string.IsNullOrEmpty(filterCondition))
            {
                totalCountQuery = "SELECT VALUE COUNT(1) FROM c WHERE "+ filterCondition;
            }
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            long totalRecordCount;
            using (var cosmosClient = GetTargetDocumentDbClient())
            {
                totalRecordCount = cosmosClient.CreateDocumentQuery<long>(
                                    UriFactory.CreateDocumentCollectionUri(targetDatabaseName, targetCollectionName), totalCountQuery, queryOptions)
                                    .AsEnumerable().First();
            }
            return totalRecordCount;
        }

        public bool CompareRecordCount()
        {
            //var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
            string sourceDatabaseName = CloneSettings.SourceSettings.DatabaseName;
            string sourceCollectionName = CloneSettings.SourceSettings.CollectionName;

            //var TargetCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
            string targetDatabaseName = CloneSettings.TargetSettings.DatabaseName;
            string targetCollectionName = CloneSettings.TargetSettings.CollectionName;

            string totalCountQuery = "SELECT VALUE COUNT(1) FROM c";
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            long sourceTotalRecordCount, targetTotalRecordCount;
            using (var cosmosClient = GetSourceDocumentDbClient())
            {
                sourceTotalRecordCount = cosmosClient.CreateDocumentQuery<long>(
                                    UriFactory.CreateDocumentCollectionUri(sourceDatabaseName, sourceCollectionName), totalCountQuery, queryOptions)
                                    .AsEnumerable().First();
            }
            using (var cosmosClient = GetTargetDocumentDbClient())
            {
                targetTotalRecordCount = cosmosClient.CreateDocumentQuery<long>(
                                    UriFactory.CreateDocumentCollectionUri(targetDatabaseName, targetCollectionName), totalCountQuery, queryOptions)
                                    .AsEnumerable().First();
            }
            return (sourceTotalRecordCount == targetTotalRecordCount) ? true: false;
        }

        public long GetSourceRecordCount()
        {
            //var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
            string sourceDatabaseName = CloneSettings.SourceSettings.DatabaseName;
            string sourceCollectionName = CloneSettings.SourceSettings.CollectionName;

            string totalCountQuery = "SELECT VALUE COUNT(1) FROM c";
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            long sourceTotalRecordCount;
            using (var cosmosClient = GetSourceDocumentDbClient())
            {
                sourceTotalRecordCount = cosmosClient.CreateDocumentQuery<long>(
                                    UriFactory.CreateDocumentCollectionUri(sourceDatabaseName, sourceCollectionName), totalCountQuery, queryOptions)
                                    .AsEnumerable().First();
            }
            return sourceTotalRecordCount;
        }

        public long GetRecordCountToScrub(ScrubRule scrubbingRule)
        {
            //var SourceCosmosDBSettings = CloneSettings.GetConfigurationSection("SourceCosmosDBSettings");
            string DatabaseName = CloneSettings.TargetSettings.DatabaseName;
            string CollectionName = CloneSettings.TargetSettings.CollectionName;
            string totalCountQuery;

            if (string.IsNullOrEmpty(scrubbingRule.FilterCondition))
            {
                totalCountQuery = "SELECT VALUE COUNT(1) FROM c";
            }
            else
            {
                totalCountQuery = "SELECT VALUE COUNT(1) FROM c" + " where " + scrubbingRule.FilterCondition;
            }
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            long sourceTotalRecordCount;
            using (var cosmosClient = GetTargetDocumentDbClient())
            {
                sourceTotalRecordCount = cosmosClient.CreateDocumentQuery<long>(
                                    UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName), totalCountQuery, queryOptions)
                                    .AsEnumerable().First();
            }
            return sourceTotalRecordCount;
        }
   
    }
}
