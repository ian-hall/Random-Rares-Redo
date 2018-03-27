using System;
using System.Linq;
using System.IO;
using System.Json;
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using System.Net;
using Tweetinvi;

namespace RaresBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            var localPath = @"rares.json";
            var allRares = RareGood.LoadRares(localPath, false);

            //var secretsPath = @"secrets.txt";
            //var ck = "";
            //var cs = "";
            //var ak = "";
            //var aks = "";
            //using (var secrets = new StreamReader(secretsPath))
            //{
            //    ck = secrets.ReadLine();
            //    cs = secrets.ReadLine();
            //    ak = secrets.ReadLine();
            //    aks = secrets.ReadLine();
            //}
            //Auth.SetUserCredentials(ck, cs, ak, aks);

            //var remotePath = @"http://edtools.ddns.net/rares.json";
            //var allRares = RareGood.LoadRares(remotePath, true);

            var allPorts = allRares.Select(item => item.Port);
            var portsBreakDown = BreakDown(allPorts);
            //Special case: remove "de" from the 2nd list.
            portsBreakDown[1].Remove("de");

            var allItems = allRares.Select(item => item.Item);
            var itemsBreakDown = BreakDown(allItems);
            //special case: remove "De" and "of" from 2nd list
            //also "Of" from 3rd list
            //also "42" from 4th list
            itemsBreakDown[1].Remove("De");
            itemsBreakDown[1].Remove("of");
            itemsBreakDown[2].Remove("Of");
            itemsBreakDown[3].Remove("42");

            for( int i = 0; i < 1000; i++ )
            {
                var port = GetPortName(portsBreakDown);
                var rare = GetItemName(itemsBreakDown);
                var rareStr = GetTweetString(port, rare);
                Console.WriteLine(rareStr);
            }
            //Tweet.PublishTweet(rareStr);
        }

        /// <summary>
        /// Going to eventually generate a silly name for an Elite Dangerous spaceport.
        /// </summary>
        /// literally bespoke handcrafted markov chains because i hate myself
        /// 
        /// IDEA: ps[0] has mostly proper names
        ///       ps[1]-ps[3] has mostly types (port, orbital, station etc)
        ///       probably have a chance to start with either a word from 0 or 1, mostly from 0
        ///       then have a chance to add 1-3 more words depending on RNG
        /// This is gonna break if the port names change ever since lol magic numbers
        public static string GetPortName(List<List<string>> portStuff)
        {
            var sb = new System.Text.StringBuilder();
            var rng = new Random();
            var typeVal = rng.NextDouble();

            var combinedNames = new List<string>();
            foreach(var s in portStuff[1])
            {
                combinedNames.Add(s);
            }
            foreach (var s in portStuff[2])
            {
                combinedNames.Add(s);
            }
            foreach (var s in portStuff[3])
            {
                combinedNames.Add(s);
            }
            combinedNames = GetUniqueStrings(combinedNames);


            //basic port name, ps[0] + ps[1-3]
            if (typeVal <= 0.40 )
            {
                sb.AppendFormat("{0} ", portStuff[0][rng.Next(portStuff[0].Count)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count)]);
            }
            //almost port name, ps[1-3] + ps[0]
            else if(typeVal <= 0.80)
            {
                sb.AppendFormat("{0} ", combinedNames[rng.Next(combinedNames.Count)]);
                sb.AppendFormat("{0}", portStuff[0][rng.Next(portStuff[0].Count)]);
            }
            //fancy port name, ps[0]/ps[1]+ "de" + ps[0] + ps[1-3]
            else if (typeVal <= 0.90)
            {
                var temp = rng.NextDouble();
                if( temp <= 0.50 )
                {
                    sb.AppendFormat("{0} ", portStuff[0][rng.Next(portStuff[0].Count)]);
                }
                else
                {
                    sb.AppendFormat("{0} ", portStuff[1][rng.Next(portStuff[1].Count)]);
                }
                sb.Append("de ");
                sb.AppendFormat("{0} ", portStuff[0][rng.Next(portStuff[0].Count)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count)]);
            }
            //OwO
            else
            {
                sb.AppendFormat("{0}-{1} ", portStuff[0][rng.Next(portStuff[0].Count)], portStuff[0][rng.Next(portStuff[0].Count)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count)]);

            }
            return sb.ToString();
        }

        /// <summary>
        /// gets a silly elite dangerous rare item name
        /// </summary>
        /// is[0] is proper names
        /// is[1-3] are types of items and modifiers and stuff
        public static string GetItemName(List<List<string>> itemStuff)
        {
            var sb = new System.Text.StringBuilder();
            var rng = new Random();
            var typeVal = rng.NextDouble();

            var combinedNames = new List<string>();
            foreach (var s in itemStuff[2])
            {
                combinedNames.Add(s);
            }
            foreach (var s in itemStuff[3])
            {
                combinedNames.Add(s);
            }
            combinedNames = GetUniqueStrings(combinedNames);
            var plurals = itemStuff[1].Where(s => s[s.Length - 1] == 's').ToList();
            foreach (var s in plurals)
            {
                combinedNames.Add(s);
            }
            combinedNames = GetUniqueStrings(combinedNames);


            var extraNames = itemStuff[1].Where(s => s[s.Length - 1] != 's').ToList();


            //basic item name, is[0] + combinedNames
            if (typeVal <= 0.25)
            {
                sb.AppendFormat("{0} ", itemStuff[0][rng.Next(itemStuff[0].Count)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count)]);
            }
            //is[0] + extraNames + combinedNames
            else if (typeVal <= 0.50)
            {
                sb.AppendFormat("{0} ", itemStuff[0][rng.Next(itemStuff[0].Count)]);
                sb.AppendFormat("{0} ", extraNames[rng.Next(extraNames.Count)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count)]);
            }
            //{is[0] +} combinedNames + "of" + extraNames
            else if (typeVal <= 0.70)
            {
                if( rng.NextDouble() <= 0.75)
                {
                    sb.AppendFormat("{0} ", itemStuff[0][rng.Next(itemStuff[0].Count)]);
                }
                if( rng.NextDouble() <= 0.50)
                {
                    sb.AppendFormat("{0} of ", combinedNames[rng.Next(combinedNames.Count)]);
                }
                else
                {
                    sb.AppendFormat("{0}-de-", combinedNames[rng.Next(combinedNames.Count)]);
                }
                sb.AppendFormat("{0}", extraNames[rng.Next(extraNames.Count)]);
                
            }
            //#yolo
            else if( typeVal <= 0.85 )
            {
                sb.AppendFormat("{0} ", itemStuff[1][rng.Next(itemStuff[1].Count)]);
                sb.AppendFormat("{0} ", itemStuff[0][rng.Next(itemStuff[0].Count)]);
                if(rng.NextDouble() <= 0.50)
                {
                    sb.AppendFormat("{0} ", extraNames[rng.Next(extraNames.Count)]);
                }
                else
                {
                    sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count)]);
                }                
            }
            //~special items~
            else
            {
                if (rng.NextDouble() <= 0.50)
                {
                    sb.AppendFormat("{0} ", extraNames[rng.Next(extraNames.Count)]);
                }
                else
                {
                    sb.AppendFormat("{0} ", itemStuff[0][rng.Next(itemStuff[0].Count)]);
                }
                var firstItem = combinedNames[rng.Next(combinedNames.Count)];
                var secondItem = combinedNames[rng.Next(combinedNames.Count)];
                while(secondItem == firstItem)
                {
                    secondItem = combinedNames[rng.Next(combinedNames.Count)];
                }
                sb.AppendFormat("{0}2{1}", firstItem, secondItem);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a IEnumerable of strings into a list of lists of strings, based on the number of spaces in the strings.
        /// </summary>
        /// EX: This Is A String would return a list with 4 lists -> [[This][Is][A][String]]
        /// Also removes duplicates from the list
        public static List<List<string>> BreakDown(IEnumerable<string> startingVals)
        {
            var retVal = new List<List<string>>();
            //Gets the unique values in the list
            var firstList = startingVals.Select(s => s.Split().First());
            retVal.Add(GetUniqueStrings(firstList));
            var rest = startingVals.Where(s => s.Split().Skip(1).Count() >= 1).Select(s => s.Split().Skip(1));
            while ( rest.Count() != 0 )
            {
                var currList = rest.Select(s => s.First());
                retVal.Add(GetUniqueStrings(currList));
                rest = rest.Where(s => s.Skip(1).Count() >= 1).Select(s => s.Skip(1));
            }
            return retVal;
        }
        /// <summary>
        /// Returns the unique elements of a IEnumerable of strings.
        /// </summary>
        public static List<string> GetUniqueStrings(IEnumerable<string> l)
        {
            return l.GroupBy(s => s)
                    .ToDictionary(group => group.Key, group => group.Count())
                    .Select(kvp => kvp.Key)
                    .ToList();
        }
        /// <summary>
        /// Returns a string with the given portName and itemName.
        /// </summary>
        public static string GetTweetString(string portName, string itemName)
        {
            //Want to pad these so the illegal/permit responses are very rare
            var portFirst = new List<string>() { "Finally made it to {0} to pick up some {1}",
                                                 "On my way to {0} to get some {1}",
                                                 "Oopsie woopsie, didn't have a permit for {0}, can't buy {1}",
                                                 "Heading to {0} to get {1}"
                                               };
            var itemFirst = new List<string>() { "Just got some {0} from {1}",
                                                 "Ran out of {0}, heading over to {1} to get some",
                                                 "Found out {0} was illegal after leaving {1}, oops"
                                               };

            var rng = new Random();
            if(rng.NextDouble() <= 0.60)
            {
                var choice = rng.Next(portFirst.Count);
                return String.Format(portFirst[choice], portName, itemName);
            }
            else
            {
                var choice = rng.Next(itemFirst.Count);
                return String.Format(itemFirst[choice], itemName, portName);
            }
        }
    }
}
