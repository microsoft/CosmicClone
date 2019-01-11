// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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

        public static string getFullLog()
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
    }
}
