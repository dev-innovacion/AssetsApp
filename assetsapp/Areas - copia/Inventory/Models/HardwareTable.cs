using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace RivkaAreas.Inventory.Models
{
    public class HardwareTable : MongoModel
    {
        public HardwareTable() : base ("Hardware")
        { 
        }

        public string GetHardware(string idHardware = null)
        {
            JoinCollections Join = new JoinCollections();

            Join.Select("Hardware")
                .Join("HardwareReference","hardware_reference","_id","name");

            if (idHardware != null)
            {
                var query = Query.EQ("_id", new ObjectId(idHardware));
                return Join.Find(query);
            }

            return Join.Find();
        }
    }
}