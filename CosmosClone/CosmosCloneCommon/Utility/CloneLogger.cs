// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CosmosCloneCommon.Model;

namespace CosmosCloneCommon.Utility
{
    public static class CloneLogger
    {
        static CloneLogger()
        {
            _logBuilder = new StringBuilder("Collection Copy log");
        }
        static StringBuilder _logBuilder;

        public static string FullLog
        {
            get
            {
               return _logBuilder.ToString();
            }
        }

        public static string GetFullLog()
        {
            return _logBuilder.ToString();
        }

        public static void LogInfo(string info)
        {
            Console.WriteLine(info);
            _logBuilder.Append("\n"+info);
        }

        public static void LogError(string s)
        {
            LogInfo("Error Occurred");
            LogInfo(s);
        }

        public static void LogError(Exception e)
        {
            LogInfo("LogError");
            Exception baseException = e.GetBaseException();
            LogInfo($"Error: {e.Message}, Message: {baseException.Message}");
        }

        public static void LogScrubRulesInformation(List<ScrubRule> scrubRules)
        {
            if (scrubRules!=null && scrubRules.Count>0 && CloneSettings.ScrubbingRequired)
            { 
                long totalRecords=0;
                foreach (var rule in scrubRules)
                {
                    LogInfo($"Rule Id: {rule.RuleId}. Attribute: {rule.PropertyName}");
                    LogInfo($"Rule filter:{(string.IsNullOrEmpty(rule.FilterCondition) ? "None":rule.FilterCondition)}");
                    LogInfo($"Rule Type: {rule.Type.ToString()}");
                    LogInfo($"Records by filter: {rule.RecordsByFilter}. Records updated: {rule.RecordsUpdated}");
                    totalRecords += rule.RecordsUpdated;
                }
                LogInfo($"Total records scrubbed: {totalRecords}");
            }
        }
    }
}
