using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Drawing;
using System.Globalization;
using Rivka.Db;
using Rivka.Db.MongoDb;
using System.Web;
using MongoDB.Driver.Linq;

namespace RivkaAreas.Inventory.Models
{
    public class ObjectTable : MongoModel
    {
        public ObjectTable() : base ("ReferenceObjects")
        { 
        }
        public string GetObjectsRefTable(List<string> varLocations)
        {
            //Query needed to get the result
            string[] arraylocations = varLocations.ToArray();
            BsonArray bsonarray = new BsonArray();

            for (int i = 0; i < arraylocations.Length; i++)
            {
                bsonarray.Add(arraylocations[i]);
            }
            var query = Query.And(Query.In("location", bsonarray), Query.NE("system_status", false));
            if (varLocations.Count() == 0)
                query = Query.And(Query.NE("system_status", false));


            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                 .Join("Locations", "location", "_id", "name => location_name,number=>number, parent=>conjunto,profileId=>profileId")
                 .Join("ReferenceObjects", "objectReference", "_id", "name => objectReference_name,profileFields => extra");


            return Join.Find(query);
        }
    }
}