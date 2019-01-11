// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosCloneCommon.Model
{
    public static class RandomNumberGenerator
    {
        private static Random _random;
        static RandomNumberGenerator()
        {
            _random = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
            _random.Next();
        }
        public static int getNext(int maxValue)
        {
            _random.Next();
            _random.Next(0, maxValue);
            return _random.Next(0,maxValue);
        }
        public static int getNext()
        {
            return _random.Next(1, 99999999);
        }
        public static string GetRandomEntityType()
        {
            string returnvalue;
            switch (RandomNumberGenerator.getNext() % 5)
            {
                case 1:
                    returnvalue = "Individual"; break;
                case 2:
                    returnvalue = "Organization"; break;
                case 3:
                    returnvalue = "Supplier"; break;
                case 4:
                    returnvalue = "Partner"; break;
                case 0:
                    returnvalue = "External"; break;
                default:
                    returnvalue = "External"; break;
            }
            return returnvalue;
        }

        //A simple implementation of Fisher yates algorithm for shuffling
        public static List<T> Shuffle<T>(List<T> list)
        {
            int n = list.Count-1;
            while (n > 1)
            {
                n--;                
                int k = RandomNumberGenerator.getNext(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
