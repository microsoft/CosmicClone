// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


//using CollectionMigrator.Model;
using logger = CosmosCloneCommon.Utility.CloneLogger;

namespace CloneConsoleRun.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using CosmosCloneCommon.Utility;
    using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    public class SampleDBCreator
    {
        #region declare variables
        protected int WriteBatchSize = 10000;
        protected int ReadDelaybetweenRequestsInMs = 2000;
        protected int maxtestDocumentCount = 50000;
        protected bool IsFixedCollection = true;
        protected CosmosSampleDBHelper cosmosHelper;
        protected CosmosBulkImporter cosmosBulkImporter;
        protected DocumentClient sampleClient;
        protected DocumentCollection sampleCollection;
        #endregion

        public SampleDBCreator()
        {
            cosmosHelper = new CosmosSampleDBHelper();
            cosmosBulkImporter = new CosmosBulkImporter();            
        }

        public async Task InitializeMigration()
        {
            logger.LogInfo("Inside Initialize migration for SampleDBCreator. ");
            sampleClient = cosmosHelper.GetSampleDocumentDbClient();
            sampleCollection = await cosmosHelper.CreateSampleDocumentCollection(sampleClient, this.IsFixedCollection);
            await cosmosBulkImporter.InitializeBulkExecutor(sampleClient, sampleCollection);
        }
        public async Task<bool> Start()
        {
            await InitializeMigration();
            await CreateUploadTestDataInbatches();
            return true;
        }

        protected List<dynamic> GetCommonEntitiesinBatch()
        {
            List<dynamic> entities = new List<dynamic>();
            for(int i=0; i<this.WriteBatchSize; i++)
            {
                entities.Add(EntityV2.getRandomEntity());
            }
            return entities;
        }

        public async Task<bool> CreateUploadTestDataInbatches()
        {
            #region batchVariables
            //initialize Batch Process variables                    
            int batchCount = 0;
            int totalUploaded = 0;
            var badEntities = new List<Object>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            #endregion
            while (totalUploaded < maxtestDocumentCount)
            {
                batchCount++;
                logger.LogInfo("Begin Sample Db creation");

                List<dynamic> entities = GetCommonEntitiesinBatch();
                BulkImportResponse uploadResponse = new BulkImportResponse();
                if (entities.Any())
                {
                    uploadResponse = await cosmosBulkImporter.BulkSendToNewCollection<dynamic>(entities);
                }
                badEntities = uploadResponse.BadInputDocuments;
                //summary.totalRecordsSent += uploadResponse.NumberOfDocumentsImported;
                totalUploaded += entities.Count();

                logger.LogInfo($"Summary of Batch {batchCount} records retrieved {entities.Count()}. Records Uploaded: {uploadResponse.NumberOfDocumentsImported}");
                //logger.LogInfo($"Total records retrieved {summary.totalRecordsRetrieved}. Total records uploaded {summary.totalRecordsSent}");
                logger.LogInfo($"Time elapsed : {stopwatch.Elapsed} ");
            }
            stopwatch.Stop();
            logger.LogInfo("Completed Sample DB creation.");
            return true;
        }
    }
}
