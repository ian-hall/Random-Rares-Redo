using System;
using System.Linq;
using System.IO;
using System.Json;

namespace RaresBot
{
    class Program
    {
        static void Main(string[] args)
        {
            //var allStuff = JsonConvert.DeserializeObject<EDRareStation>(File.ReadAllText("../../../rares.json"));
            using (var jsonStream = File.OpenRead("../../../rares.json"))
            {
                var raresArray = (JsonObject)JsonObject.Load(jsonStream);
                foreach( var item in raresArray.Keys)
                {
                    var itemInfo = raresArray[item];
                    Console.WriteLine(new EDRareStation(itemInfo, item));
                }
            }
            Console.ReadLine();
        }
    }
}
