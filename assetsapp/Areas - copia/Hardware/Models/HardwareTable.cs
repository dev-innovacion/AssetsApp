using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MongoDB.Driver.Builders;
using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Hardware.Models
{
    public class HardwareTable : MongoModel
    {
        public HardwareTable() : base ("Hardware")
        { 
        }

        public string GetFullHardware()
        {
            JoinCollections Join = new JoinCollections();

            Join.Select("Hardware")
                .Join("HardwareReference", "hardware_reference", "_id", "name => hardwareName, ext => image");


            return Join.Find();
        }

        public string GetLocatedHardware(string id_location)
        {
            var query = Query.EQ("location_id", id_location);

            JoinCollections Join = new JoinCollections();
            Join.Select("Hardware")
                .Join("HardwareReference", "hardware_reference", "_id", "name => hardwareName, ext => image");


            return Join.Find(query);
        }
    }
}