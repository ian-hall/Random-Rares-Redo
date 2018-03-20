using System;
using System.Collections.Generic;
using System.Net;
using System.Json;
using System.IO;

namespace RaresBot
{
    class RareGood
    {
        public string Item { get; set; }
        public string System { get; set; }
        public string Port { get; set; }
        public int Dst { get; set; }
        public int Alloc { get; set; }
        public int Cost { get; set; }
        public bool Permit { get; set; }
        public bool Illeg { get; set; }
        public int ID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public RareGood(JsonValue json, string item)
        {
            //Need to null check on alloc and cost
            string checkAlloc = json["alloc"];
            string checkCost = json["cost"];
            this.Item = item;
            this.System = json["system"];
            this.Port = json["port"];
            this.Dst = json["dst"];
            this.Alloc = (checkAlloc != "") ? int.Parse(checkAlloc) : 0;
            this.Cost = (checkCost != "") ? int.Parse(checkCost) : 0;
            this.Permit = json["permit"];
            this.Illeg = json["illeg"];
            this.ID = json["port_id"];
            this.X = json["x"];
            this.Y = json["y"];
            this.Z = json["z"];
        }

        public static List<RareGood> LoadRares(string path, bool isRemote)
        {
            var allRares = new List<RareGood>();
            if( !isRemote )
            {
                using (var jsonStream = File.OpenRead(path))
                {
                    var raresArray = (JsonObject)JsonObject.Load(jsonStream);
                    foreach (var item in raresArray.Keys)
                    {
                        var itemInfo = raresArray[item];
                        allRares.Add(new RareGood(itemInfo, item));
                    }
                }
            }
            else
            {
                var webRequest = WebRequest.Create(path);
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var jsonStream = new StreamReader(content))
                {
                    var raresArray = (JsonObject)JsonObject.Load(jsonStream);
                    foreach (var item in raresArray.Keys)
                    {
                        var itemInfo = raresArray[item];
                        allRares.Add(new RareGood(itemInfo, item));
                    }
                }
            }

            return allRares;
        }

        override
        public string ToString()
        {
            return String.Format("{0} sold in {1} @ {2} ({3}cr)", this.Item, this.System, this.Port, this.Cost);
        }
    }
}
