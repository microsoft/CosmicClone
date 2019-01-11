// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
using logger = CosmosCloneCommon.Utility.CloneLogger;

namespace CosmosCloneCommon.Utility
{
   
    public static class CloneSettings
    {
        public static bool CopyStoredProcedures { get; set; }
        public static bool CopyUDFs { get; set; }
        public static bool CopyTriggers { get; set; }
        public static bool CopyDocuments { get; set; }
        public static bool CopyIndexingPolicy { get; set; }
        public static bool CopyPartitionKey { get; set; }
        public static int ReadBatchSize { get; set; }
        public static int WriteBatchSize { get; private set; }
        public static bool EnableTextLogging { get; set; }
        public static bool ScrubbingRequired { get; set; }
        
        public static int SourceOfferThroughputRUs { get; set; }
        public static int TargetMigrationOfferThroughputRUs { get; set; }
        public static int TargetRestOfferThroughputRUs { get; set; }
        public static CosmosCollectionValues SourceSettings { get; set; }
        public static CosmosCollectionValues TargetSettings { get; set; }



        static CloneSettings()
        {
            ConfigurationManager.RefreshSection("appSettings");

            CopyStoredProcedures = bool.Parse(ConfigurationManager.AppSettings["CopyStoredProcedures"]);
            CopyUDFs = bool.Parse(ConfigurationManager.AppSettings["CopyUDFs"]);
            CopyTriggers = bool.Parse(ConfigurationManager.AppSettings["CopyTriggers"]);
            CopyDocuments = bool.Parse(ConfigurationManager.AppSettings["CopyDocuments"]);
            CopyIndexingPolicy = bool.Parse(ConfigurationManager.AppSettings["CopyIndexingPolicy"]);
            CopyPartitionKey = bool.Parse(ConfigurationManager.AppSettings["CopyPartitionKey"]);
            var value = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            ReadBatchSize = int.Parse(ConfigurationManager.AppSettings["ReadBatchSize"].ToString());
            WriteBatchSize = int.Parse(ConfigurationManager.AppSettings["WriteBatchCount"]);
            EnableTextLogging = bool.Parse(ConfigurationManager.AppSettings["EnableTextLogging"]);
            SourceOfferThroughputRUs = int.Parse(ConfigurationManager.AppSettings["SourceOfferThroughputRUs"]);
            TargetMigrationOfferThroughputRUs = int.Parse(ConfigurationManager.AppSettings["TargetMigrationOfferThroughputRUs"]);
            TargetRestOfferThroughputRUs = int.Parse(ConfigurationManager.AppSettings["TargetRestOfferThroughputRUs"]);
            ScrubbingRequired = bool.Parse(ConfigurationManager.AppSettings["ScrubbingRequired"]);
            var sourceConfigs = GetConfigurationSection("SourceCosmosDBSettings"); 
            SourceSettings = new CosmosCollectionValues()
            {
                EndpointUrl = sourceConfigs["EndpointUrl"],
                AccessKey = sourceConfigs["AccessKey"],
                DatabaseName = sourceConfigs["DatabaseName"],
                CollectionName = sourceConfigs["CollectionName"]
                //OfferThroughputRUs = int.Parse(sourceConfigs["OfferThroughputRUs"])
            };

            var targetConfigs = GetConfigurationSection("TargetCosmosDBSettings");
            TargetSettings = new CosmosCollectionValues()
            {
                EndpointUrl = targetConfigs["EndpointUrl"],
                AccessKey = targetConfigs["AccessKey"],
                DatabaseName = targetConfigs["DatabaseName"],
                CollectionName = targetConfigs["CollectionName"]
               // OfferThroughputRUs = int.Parse(sourceConfigs["OfferThroughputRUs"])
            };
        }

        public static NameValueCollection GetConfigurationSection(string sectionName)
        {
            var appSettings = ConfigurationManager.GetSection(sectionName) as NameValueCollection;
            if (appSettings.Count == 0)
            {
                logger.LogInfo($"Application Settings are not defined for {sectionName}");
            }
            return appSettings;
        }
        public static string AppSettings(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }

    public class CosmosCollectionValues
    {
        public string EndpointUrl { get; set; }
        public string AccessKey { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
       
    }
}
