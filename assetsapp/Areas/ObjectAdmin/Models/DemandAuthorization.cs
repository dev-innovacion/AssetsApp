using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db.MongoDb;
using Newtonsoft.Json;

namespace RivkaAreas.ObjectAdmin.Models
{
    public class DemandAuthorization : MongoModel
    {
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonProperty("demand")]
        public string demand { get; set; }

        [JsonProperty("user")]
        public string user { get; set; }


        //staticFieds contains the required fields, each document must have this values
        static public List<String> staticFields = new List<String>() {"_id", "demand", "user" };

        public DemandAuthorization(string table = "DemandAuthorization") : base(table)
        {
        }
    }
}