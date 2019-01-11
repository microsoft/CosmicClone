// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosCloneCommon.Model
{
    public class ScrubRule
    {
        public int RuleId { get; set; }
        public string FilterCondition { get; set; }
        public string PropertyName { get; set; }
        public string UpdateValue { get; set; }        

        public RuleType? Type { get; set; }

        private int RecordsByFilter { get; set; }
        private int RecordsUpdated { get; set; }
        private int CountNullAttributes { get; set; }
        public bool IsComplete { get; set; }

        public ScrubRule() { }
        public ScrubRule(string filterCondition, string propertyName, RuleType type, string updateValue, int ruleId)
        {          

            this.FilterCondition = filterCondition;
            this.PropertyName = propertyName;
            this.UpdateValue = updateValue;
            this.Type = type;
            this.RuleId = ruleId;
            this.IsComplete = false;
        }
    }
    public enum RuleType { SingleValue, NullValue, Shuffle };//Can add random later if required.
       

}
