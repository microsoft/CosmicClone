// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace CosmosCloneCommon.Model
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public static class RandomNumberGenerator
    {
        private static readonly Random _random = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
        
        public static int GetNext(int maxValue)
        {
            return _random.Next(0,maxValue);
        }
        public static int GetNext()
        {
            return _random.Next(1, 99999999);
        }
        public static string GetRandomEntityType()
        {
            switch (RandomNumberGenerator.GetNext(5))
            {
                case 1:
                    return "Individual";
                case 2:
                    return "Organization";
                case 3:
                    return "Supplier";
                case 4:
                    return "Partner";
                case 0:
                    return "External";
                default:
                    return "External";
            }
        }

        //A simple implementation of Fisher yates algorithm for shuffling
        public static List<T> Shuffle<T>(List<T> list)
        {
            int n = list.Count-1;
            while (n > 1)
            {
                n--;                
                int k = RandomNumberGenerator.GetNext(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}