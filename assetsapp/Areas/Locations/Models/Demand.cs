using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db.MongoDb;
using Newtonsoft.Json;
using MongoDB.Driver.Builders;

namespace RivkaAreas.Locations.Models
{
    public class Demand : MongoModel
    {
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonProperty("folio")]
        public string folio { get; set; }

        [JsonProperty("movement")]
        public string movement { get; set; }

        [JsonProperty("objectreal")]
        public string objectreal { get; set; }

        [JsonProperty("total")]
        public string total { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }


        //staticFieds contains the required fields, each document must have this values
        static public List<String> staticFields = new List<String>() {"_id", "folio", "movement", "object","total","status" };

        public Demand(string table = "Demand")
            : base(table)
        {
        }

        /// <summary>
        ///     Gets the join between many collections
        /// </summary>
        /// <returns> An array with the resulting search </returns>
        public string GetDemandTable(string varMovement, string varStatus)
        {
            //Query needed to get the result
            var query = Query.And(Query.EQ("movement", varMovement), Query.EQ("status", varStatus));
            if(varMovement=="null")
                query = Query.And(Query.EQ("status", varStatus));
            if(varStatus=="null")
                query = Query.And(Query.EQ("movement", varMovement));

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "name=>movement , name=>typeMovement")
                .Join("Locations", "location", "_id", "name => location")
                .Join("ReferenceObjects", "object", "_id", "name=>object")
                .Join("Users", "Creator", "_id", "name=>Creator");

            if(varMovement=="null" && varStatus=="null")
                return Join.Find();

            return Join.Find(query);
        }

        

    }
}