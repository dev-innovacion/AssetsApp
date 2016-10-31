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
namespace RivkaAreas.Reports.Models
{
    public class ObjectsRefReport : MongoModel
    {
        private MongoCollection collection;
        private MongoConection conection;
        public ObjectsRefReport(string table)
            : base("ReferenceObjects")
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
        }

        public String GetRowsReportRf(Dictionary<string, string> fields = null, Int64 start = 0, Int64 end = 0)
        {

            var orderfield = "name";



            try
            {

                List<string> cols = new List<string>();
                if (fields == null || fields.Count() == 0)
                {

                    cols.Add("name");


                    cols.Add("parentCategory");
                    cols.Add("CreatedTimeStamp");
                }
                else
                {
                    foreach (var x in fields)
                    {
                        cols.Add(x.Key);
                    }
                    if (!fields.Keys.Contains("parentCategory"))
                    {
                        cols.Add("parentCategory");
                    }

                    cols.Add("CreatedTimeStamp");
                }

                string[] arrayfields = cols.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();


                var query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

                JoinCollections Join = new JoinCollections();
                Join.Select("ReferenceObjects")
                .Join("Categories", "parentCategory", "_id", "name=>nameCategory");

                //var cursor = collection.FindAs(typeof(BsonDocument), query).SetFields(arrayfields).SetSortOrder(SortBy.Ascending(orderfield));
                //foreach (BsonDocument document in cursor)
                //{
                //    document.Set("_id", document.GetElement("_id").Value.ToString());
                //    try
                //    {

                //        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //    documents.Add(document);
                //}


                return Join.Find(query);
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}

