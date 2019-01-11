// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CosmosCloneCommon.Utility
{
    public class CloneSerializer
    {
        public static string XMLSerialize<T>(T dataToSerialize)
        {
            try
            {
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stringwriter, dataToSerialize);
                return stringwriter.ToString();
            }
            catch
            {
                throw;
            }
        }

        public static T XMLDeserialize<T>(string xmlText)
        {
            try
            {
                var stringReader = new System.IO.StringReader(xmlText);
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
            catch
            {
                throw;
            }
        }
    }
}
