// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosCloneCommon;
using CosmosCloneCommon.Migrator;
using CosmosCloneCommon.Model;
using CosmosCloneCommon.Sample;
using CosmosCloneCommon.Utility;
using logger = CosmosCloneCommon.Utility.CloneLogger;
using Newtonsoft.Json;

namespace CloneConsoleRun
{
    class Program
    {
        //Use the console for test of common utils before integration with the UI
        public static void Main(string[] args)
        {
            try
            {
                logger.LogInfo("Begin Clone Execution");

                //Update the app.config settings in the console project to run the below directly
                //var documentMigrator = new CosmosCloneCommon.Migrator.DocumentMigrator();
                //documentMigrator.StartCopy().Wait();
                TestCosmosScrubbing();

                //logger.LogInfo("Begin Code migration");
                //var codeMigrator = new CosmosCloneCommon.Migrator.CodeMigrator();
                //codeMigrator.StartCopy().Wait();

                //var sampleMigrator = new CosmosCloneCommon.Sample.SampleDBCreator();
                //sampleMigrator.Start().Wait();
                //documentMigrator.StartCopy().Wait();

            }
            catch (Exception e)
            {
                //added ip address
                logger.LogError(e);
                Console.ReadKey();
            }
            finally
            {
                //logger.Close();
                logger.LogInfo("Cosmos Collection cloning complete");
                Console.ReadKey();
                Console.ReadLine();
            }
        }

        public static void TestCosmosScrubbing()
        {
            var tcs = new DataScrubMigrator();
            // ScrubRule rule1 = new ScrubRule("c.id=\"a5e66b8b-a57c-4788-a194-58d3735a9854\"", "c.CompositeName",  RuleType.Singlevalue, "Test overwritten Value xyz",1);
            //ScrubRule rule2 = new ScrubRule("", "c.SuperKeys.KeyValue",  RuleType.Shuffle, "",2);
            //ScrubRule rule3 = new ScrubRule("", "c.EntityValue.Name", RuleType.Shuffle, "", 3);
            //ScrubRule rule4 = new ScrubRule("", "c.EntityValue.Description", RuleType.Singlevalue, "OverWrite Filtered rule Description", 4);
            //ScrubRule rule3 = new ScrubRule("c.id=\"1402e84d-e034-45f9-8064-3ab174119e4f\"", "c.EntityValue.Name", RuleType.Singlevalue,"Test overwritten EntityName", 3);
            var scrubRules = new List<ScrubRule>();
            //scrubRules.Add(rule2);
            //scrubRules.Add(rule3);
            //scrubRules.Add(rule4);            
            //ScrubRule rule5 = new ScrubRule("c.EntityType=\"External\"", "c.EmailAddress", RuleType.Singlevalue, "unknown@unknown.com", 4);
            ScrubRule rule6 = new ScrubRule("c.id=\"2826d281-3a8b-4408-b064-efff26e26119\"", "c.EmailAddress", RuleType.SingleValue, "unknown@unknown.com", 4);
            scrubRules.Add(rule6);
            var documentMigrator = new CosmosCloneCommon.Migrator.DocumentMigrator();
            documentMigrator.StartCopy(scrubRules).Wait();
            //var result = tcs.StartScrub(scrubRules);
        }    


        public static async Task TestCollections()
        {
            logger.LogInfo("TestConnections");
            var dbhelper = new CosmosDBHelper();
            var vResult = await dbhelper.TestSourceConnection();
            var vResult2 = await dbhelper.TestTargetConnection();
        }

     
    }
}
