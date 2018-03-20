using System;
using System.Linq;
using System.IO;
using System.Json;
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using System.Net;

namespace RaresBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var localPath = @"../../../rares.json";
            //var allRares = RareGood.LoadRares(localPath, false);

            var remotePath = @"http://edtools.ddns.net/rares.json";
            var allRares = RareGood.LoadRares(remotePath, true);


            foreach (var rare in allRares)
            {
                Console.WriteLine(rare);
            }

            var allPorts = allRares.Select(item => item.Port);
            var allItems = allRares.Select(item => item.Item);
            var portsBreakDown = BreakDown(allPorts);
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
        /// Converts a IEnumerable of strings into a list of lists of strings, based on the number of spaces in the string
        /// EX: This Is A String would return a list with 4 lists -> [[This][Is][A][String]]
        /// </summary>
        public static List<List<string>> BreakDown(IEnumerable<string> startingVals)
        {
            var retVal = new List<List<string>>();
            var firstList = startingVals.Select(s => s.Split().First());
            retVal.Add(firstList.ToList());
            var rest = startingVals.Where(s => s.Split().Skip(1).Count() >= 1).Select(s => s.Split().Skip(1));
            while ( rest.Count() != 0 )
            {
                var currList = rest.Select(s => s.First()).ToList();
                retVal.Add(currList);
                rest = rest.Where(s => s.Skip(1).Count() >= 1).Select(s => s.Skip(1));
            }
            return retVal;
        }
    }
}
