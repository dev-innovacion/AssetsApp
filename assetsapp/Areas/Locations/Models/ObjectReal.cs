using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db.MongoDb;
using Newtonsoft.Json;

namespace RivkaAreas.Locations.Models
{
    public class ObjectReal : MongoModel
    {
        //staticFieds contains the required fields, each document must have this values
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        static public List<String> staticFields = new List<String>() {"_id", "name" };

        public ObjectReal(string table = "ObjectReal")
            : base(table)
        {

        }
    }
}