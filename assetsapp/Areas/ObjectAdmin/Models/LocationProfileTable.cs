using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.ObjectAdmin.Models
{
    public class LocationProfileTable : MongoModel
    {

        public LocationProfileTable()
            : base("LocationProfiles")
        {
           
        }

        public void deleteProfile(String idProfile)
        {
            var query = Query.And(Query.EQ("_id", new ObjectId(idProfile)));
            try
            {
                collection.Remove(query);
            }
            catch (Exception e) { /*ignored*/}
        }

     
    }
}