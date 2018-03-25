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
            var localPath = @"../../../rares.json";
            var allRares = RareGood.LoadRares(localPath, false);

            //var remotePath = @"http://edtools.ddns.net/rares.json";
            //var allRares = RareGood.LoadRares(remotePath, true);

            var allPorts = allRares.Select(item => item.Port);
            var portsBreakDown = BreakDown(allPorts);
            //Special case: remove "de" from the 2nd list.
            portsBreakDown[1].Remove("de");
            //for( int i = 0; i < 100; i++ )
            //{
            //    Console.WriteLine(GetPortName(portsBreakDown));
            //}

            var allItems = allRares.Select(item => item.Item);
            var itemsBreakDown = BreakDown(allItems);
            //special case: remove "De" and "of" from 2nd list
            //also "Of" from 3rd list
            //also "42" from 4th list
            itemsBreakDown[1].Remove("De");
            itemsBreakDown[1].Remove("of");
            itemsBreakDown[2].Remove("Of");
            itemsBreakDown[3].Remove("42");
            for( int i = 0; i < 100; i++ )
            {
                Console.WriteLine(GetItemName(itemsBreakDown));
            }
        }

        /// <summary>
        /// Going to eventually generate a silly name for an Elite Dangerous spaceport.
        /// </summary>
        /// literally bespoke handcrafted markov chains because reinventing wheels is cool+good (its not)
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
                sb.AppendFormat("{0} ", portStuff[0][rng.Next(portStuff[0].Count - 1)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count - 1)]);
            }
            //almost port name, ps[1-3] + ps[0]
            else if(typeVal <= 0.80)
            {
                sb.AppendFormat("{0} ", combinedNames[rng.Next(combinedNames.Count - 1)]);
                sb.AppendFormat("{0}", portStuff[0][rng.Next(portStuff[0].Count - 1)]);
            }
            //fancy port name, ps[0]/ps[1]+ "de" + ps[0] + ps[1-3]
            else if (typeVal <= 0.90)
            {
                var temp = rng.NextDouble();
                if( temp <= 0.50 )
                {
                    sb.AppendFormat("{0} ", portStuff[0][rng.Next(portStuff[0].Count - 1)]);
                }
                else
                {
                    sb.AppendFormat("{0} ", portStuff[1][rng.Next(portStuff[1].Count - 1)]);
                }
                sb.Append("de ");
                sb.AppendFormat("{0} ", portStuff[0][rng.Next(portStuff[0].Count - 1)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count - 1)]);
            }
            //OwO
            else
            {
                sb.AppendFormat("{0}-{1} ", portStuff[0][rng.Next(portStuff[0].Count - 1)], portStuff[0][rng.Next(portStuff[0].Count - 1)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count - 1)]);

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
            if (typeVal <= 0.30)
            {
                sb.AppendFormat("{0} ", itemStuff[0][rng.Next(itemStuff[0].Count - 1)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count - 1)]);
            }
            //is[0] + extraNames + combinedNames
            else if (typeVal <= 0.60)
            {
                sb.AppendFormat("{0} ", itemStuff[0][rng.Next(itemStuff[0].Count - 1)]);
                sb.AppendFormat("{0} ", extraNames[rng.Next(extraNames.Count - 1)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count - 1)]);
            }
            //is[0] + combinedNames + "of" + extraNames
            else if (typeVal <= 0.80)
            {
                sb.AppendFormat("{0} ", itemStuff[0][rng.Next(itemStuff[0].Count - 1)]);
                sb.AppendFormat("{0} of ", combinedNames[rng.Next(combinedNames.Count - 1)]);
                sb.AppendFormat("{0}", extraNames[rng.Next(extraNames.Count - 1)]);
                
            }
            //OwO
            else
            {
                sb.AppendFormat("\t{0}-{1} ", itemStuff[0][rng.Next(itemStuff[0].Count - 1)], itemStuff[0][rng.Next(itemStuff[0].Count - 1)]);
                sb.AppendFormat("{0}", combinedNames[rng.Next(combinedNames.Count - 1)]);
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
    }
}
