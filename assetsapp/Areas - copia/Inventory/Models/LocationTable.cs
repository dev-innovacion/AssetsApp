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

namespace RivkaAreas.Inventory.Models
{
    public class LocationTable : MongoModel
    {
        public LocationTable() : base ("Locations")
        { 
        }
        public String get(String key, String value)
        {
            try
            {
                var query = Query.And(Query.EQ(key, value));
                var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending("name"));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    // if (isValidDocument(document))
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex) { }
                    documents.Add(document);
                }

                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}