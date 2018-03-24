using System;
using System.Linq;
using System.IO;
using System.Json;
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using System.Net;

namespace RaresBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            var localPath = @"../../../rares.json";
            var allRares = RareGood.LoadRares(localPath, false);

            var remotePath = @"http://edtools.ddns.net/rares.json";
            //var allRares = RareGood.LoadRares(remotePath, true);


            foreach (var rare in allRares)
            {
                Console.WriteLine(rare);
            }

            var allPorts = allRares.Select(item => item.Port);
            var portsBreakDown = BreakDown(allPorts);
            //Special case: remove "de" from the 2nd list.Z
            portsBreakDown[1].Remove("de");
            Console.WriteLine(GetPortName(portsBreakDown));

            var allItems = allRares.Select(item => item.Item);
            var itemsBreakDown = BreakDown(allItems);

            //Console.WriteLine(allPorts);
            //var testing = SeqModule.Windowed(3, allPorts);
            //foreach(var whatever in testing)
            //{
            //    whatever.Any(x => { Console.Write("{0} ", x); return false; });
            //    Console.WriteLine();
            //}
        }

        /// <summary>
        /// Going to eventually generate a silly name for an Elite Dangerous spaceport.
        /// When reading in the list of lists, I am going to assume there are at least 2 lists.
        /// First list can be ignored and the program will (almost) always use something from it.
        /// RNG will then decide how much is added from the other lists.
        /// The "de" that was removed could be added somewhere also probably.
        /// literally bespoke handcrafted markov chains because reinventing wheels is cool+good (its not)
        /// 
        /// IDEA: ps[0] has mostly proper names
        ///       ps[1]-ps[3] has mostly types (port, orbital, station etc)
        ///       probably have a chance to start with either a word from 0 or 1, mostly from 0
        ///       then have a chance to add 1-3 more words depending on RNG
        /// </summary>
        public static string GetPortName(List<List<string>> portStuff)
        {
            var sb = new System.Text.StringBuilder();
            //Chance array to decide how long the port name will be
            //upperval should maybe be higher to let upper vals be picked more??
            var chances = new List<double>();
            var upperVal = 100.0;
            var total = portStuff.Skip(1).Sum(l => l.Count) * 1.0;
            chances.Add((portStuff[1].Count / total) * upperVal);
            for(int i = 2; i < portStuff.Count; i++ )
            {
                var temp = (portStuff[i].Count / total) * upperVal;
                chances.Add(temp + chances.Last());
            }
            
            //maybe fudge this a little so we have a better chance to go longer or do some kind of avg thing
            var rng = new Random();
            //plz remember to delete the loop
            for( int i = 0; i < 100; i++ )
            {
                var randVal = rng.NextDouble() * upperVal;
                var chosen = chances.First(x => x >= randVal);

                switch (chances.IndexOf(chosen))
                {
                    case 0:
                        var l1 = portStuff[0].Count;
                        var l2 = portStuff[1].Count;
                        sb.AppendFormat("{0} ", portStuff[0][rng.Next(l1 - 1)]);
                        sb.AppendLine(portStuff[1][rng.Next(l2 - 1)]);
                        break;
                    case 1:
                        sb.AppendLine("de' whatever");
                        break;
                    case 2:
                    default: //in case the number of lists in portStuff changes and i forget to change this stuff
                        sb.AppendLine("idk some kind of weird thing");
                        break;
                }
            }



            return sb.ToString();
        }

        /// <summary>
        /// Converts a IEnumerable of strings into a list of lists of strings, based on the number of spaces in the string
        /// EX: This Is A String would return a list with 4 lists -> [[This][Is][A][String]]
        /// Also removes duplicates from the list
        /// </summary>
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
        /// Returns the unique elements of a IEnumerable of strings
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static List<string> GetUniqueStrings(IEnumerable<string> l)
        {
            return l.GroupBy(s => s)
                    .ToDictionary(group => group.Key, group => group.Count())
                    .Select(kvp => kvp.Key)
                    .ToList();
        }
    }
}
