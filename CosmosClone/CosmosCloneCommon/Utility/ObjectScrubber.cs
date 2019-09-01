// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using CosmosCloneCommon.Model;

namespace CosmosCloneCommon.Utility
{
    public class ObjectScrubber
    {
        public List<JToken> ScrubObjectList(List<string> srcList, ScrubRule scrubRule)
        {
            //var scrubbedObjects = new List<string>();
            var scrubbedObjects = new List<JToken>();
            var propNames = scrubRule.PropertyName.Split('.').ToList();
            if(scrubRule.Type == RuleType.NullValue || scrubRule.Type == RuleType.SingleValue)
            {
                foreach (var strObj in srcList)
                {
                    try
                    {
                        JToken jToken = GetUpdatedJsonArrayValue((JToken)JObject.Parse(strObj), propNames, scrubRule.UpdateValue);
                        scrubbedObjects.Add(jToken);
                    }
                    catch(Exception ex)
                    {
                        CloneLogger.LogInfo("Log failed");
                        CloneLogger.LogError(ex);
                        throw ;
                    }
                   
                }
            }
            else if(scrubRule.Type == RuleType.Shuffle)
            {
                //get all similar values
                var propertyValues = new List<JToken>();
                foreach (var strObj in srcList)
                {
                    try
                    {
                        List<JToken> jTokenList = new List<JToken>();
                        GetPropertyValues((JToken)JObject.Parse(strObj), propNames, ref jTokenList);
                        propertyValues.AddRange(jTokenList);
                    }
                    catch (Exception ex)
                    {
                        CloneLogger.LogInfo("Log failed");
                        CloneLogger.LogError(ex);
                        throw ;
                    }
                   
                }
                
                var shuffledTokens = RandomNumberGenerator.Shuffle(propertyValues);
                var shuffledTokenQ = new Queue<JToken>(shuffledTokens);

                foreach (var strObj in srcList)
                {
                    try
                    {
                        JToken jToken = GetDocumentShuffledToken((JToken)JObject.Parse(strObj), propNames, ref shuffledTokenQ);
                        scrubbedObjects.Add(jToken);
                    }
                    catch (Exception ex)
                    {
                        CloneLogger.LogInfo("Log failed");
                        CloneLogger.LogError(ex);
                        throw ;
                    }
                }
            }
            else
            {
                foreach (var strObj in srcList)
                {
                    scrubbedObjects.Add((JToken)strObj);
                }
            }
            
            return scrubbedObjects;
        }

        public List<JToken> GetPropertyValues(JToken token, List<string> propNames, ref List<JToken> jTokenList)
        {            
            if(jTokenList == null)
            {
                jTokenList = new List<JToken>();
            }
            if (token == null || token.Type == JTokenType.Null) return jTokenList;

            bool isLeaflevel = false;

            if(propNames.Count > 1)
            {
                if (propNames.Count == 2) isLeaflevel = true;
                var currentProperty = propNames[1];

                if (token.Type == JTokenType.Array)
                {
                    var jArray = (JArray)token;
                    for (int k = 0; k < jArray.Count; k++)
                    {
                        if (isLeaflevel == true)
                        {
                            if (jArray[k][currentProperty] != null && jArray[k][currentProperty].Type != JTokenType.Null)
                            {
                                jTokenList.Add(jArray[k][currentProperty]);
                            }
                            else
                            {
                                jTokenList.Add(null);//In future, to retain null feature modify this to conditional
                            }
                            continue;
                        }
                        else
                        {
                            GetPropertyValues(jArray[k], propNames.GetRange(1, propNames.Count - 1), ref jTokenList);
                            continue;
                        }
                    }
                }
                else
                {
                    var jObj = (JObject)token;
                    if (isLeaflevel == true)
                    {
                        if (jObj[currentProperty] != null)
                        {
                            jTokenList.Add(jObj[currentProperty]);
                        }
                        else
                        {
                            jTokenList.Add(null);//In future, to retain null feature modify this to conditional
                        }
                    }
                    else
                    {
                        GetPropertyValues((JToken)jObj[currentProperty], propNames.GetRange(1, propNames.Count - 1), ref jTokenList);
                    }
                }

            } 
            return jTokenList;
        }
        public JToken GetDocumentShuffledToken(JToken token, List<string> propNames, ref Queue<JToken> tokenQ)
        {
            if (token == null || token.Type == JTokenType.Null) return null;

            JToken jTokenResult = token;//just to initialize
            bool isLeaflevel = false;
            if (propNames.Count > 1)
            {
                if (propNames.Count == 2) isLeaflevel = true;

                var currentProperty = propNames[1];

                if (token.Type == JTokenType.Array)
                {
                    var jArray = (JArray)token;
                    for (int k = 0; k < jArray.Count; k++)
                    {
                        if (isLeaflevel == true)
                        {
                            if (jArray[k][currentProperty] != null && jArray[k][currentProperty].Type != JTokenType.Null)
                            {
                                jArray[k][currentProperty] = tokenQ.Dequeue();
                            }
                            continue;
                        }
                        else
                        {
                            jArray[k] = GetDocumentShuffledToken(jArray[k], propNames.GetRange(1, propNames.Count - 1), ref tokenQ);
                            continue;
                        }
                    }
                    var str2 = jArray.ToString();
                    jTokenResult = (JToken)jArray;
                }
                else
                {
                    var jObj = (JObject)token;
                    if (isLeaflevel == true)
                    {
                        if (jObj[currentProperty] != null)
                        {
                            jObj[currentProperty] = tokenQ.Dequeue();
                        }
                    }
                    else
                    {
                        jObj[currentProperty] = GetDocumentShuffledToken((JToken)jObj[currentProperty], propNames.GetRange(1, propNames.Count - 1), ref tokenQ);
                    }
                    var str3 = jObj.ToString();
                    jTokenResult = (JToken)jObj;
                }
            }               
            if (jTokenResult == null)
            {
                jTokenResult = token;
            }
            return jTokenResult;
        }

        public JToken GetUpdatedJsonArrayValue(JToken token, List<string> propNames, string overwritevalue)
        {
            if (token == null || token.Type == JTokenType.Null) return null;

            JToken jTokenResult=token;//just to initialize
            bool isLeaflevel = false;

            if (propNames.Count > 1)
            {
                if (propNames.Count == 2) isLeaflevel = true;

                var currentProperty = propNames[1];

                if (token.Type == JTokenType.Array)
                {
                    var jArray = (JArray)token;
                    for (int k = 0; k < jArray.Count; k++)
                    {
                        if (isLeaflevel == true)
                        {
                            if (jArray[k][currentProperty] != null && jArray[k][currentProperty].Type != JTokenType.Null)
                            {
                                jArray[k][currentProperty] = overwritevalue;
                            }
                            continue;
                        }
                        else
                        {
                            if (jArray[k] != null && jArray[k][currentProperty].Type != JTokenType.Null)
                            {
                                jArray[k] = GetUpdatedJsonArrayValue(jArray[k], propNames.GetRange(1, propNames.Count - 1), overwritevalue);
                                continue;
                            }
                            //else return null;
                        }
                    }
                    var str2 = jArray.ToString();
                    jTokenResult = (JToken)jArray;
                }
                else
                {
                    var jObj = (JObject)token;
                    if (isLeaflevel == true)
                    {
                        if (jObj[currentProperty] != null && jObj[currentProperty].Type != JTokenType.Null)
                        {
                            jObj[currentProperty] = overwritevalue;
                        }
                    }
                    else
                    {
                        if (jObj[currentProperty] != null && jObj[currentProperty].Type != JTokenType.Null)
                        {
                            jObj[currentProperty] = GetUpdatedJsonArrayValue((JToken)jObj[currentProperty], propNames.GetRange(1, propNames.Count - 1), overwritevalue);
                        }
                        //else return null;
                    }
                    var str3 = jObj.ToString();
                    jTokenResult = (JToken)jObj;
                }
            }

            if(jTokenResult == null)
            {
                jTokenResult = token;
            }
            return jTokenResult;
        }                
    }
}
