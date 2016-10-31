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
    public class ProfileReport : MongoModel
    {
        private MongoCollection collection;
        private MongoConection conection;
        public  ProfileReport(string table):base("Profiles") {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
        }

        public String GetRowsReportProfile( Dictionary<string, string> fields = null, Int64 start = 0, Int64 end = 0)
        {

            var orderfield = "name";



            try
            {

                List<string> cols = new List<string>();
                if (fields == null || fields.Count()==0)
                {
                    
                    cols.Add("name");


                   
                    cols.Add("CreatedTimeStamp");
                    cols.Add("CreatedDate");
                }
                else
                {
                    foreach (var x in fields)
                    {
                        cols.Add(x.Key);
                    }
                  
                    cols.Add("CreatedTimeStamp");
                }

                string[] arrayfields = cols.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();

              
                    var query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

                    var cursor = collection.FindAs(typeof(BsonDocument), query).SetFields(arrayfields).SetSortOrder(SortBy.Ascending(orderfield));
                    foreach (BsonDocument document in cursor)
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                        try
                        {
                           
                            document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex)
                        {

                        }
                        documents.Add(document);
                    }

                
                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public String GetRowsFilter(List<string> idprofiles)
        {

            var orderfield = "name";



            try
            {

             

                string[] arrayfields = idprofiles.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();
                BsonArray bsonarray = new BsonArray();

                for (int i = 0; i < arrayfields.Length; i++)
                {
                    bsonarray.Add(new BsonObjectId(arrayfields[i]));
                }

                var query = Query.And(Query.In("_id",bsonarray));
              
                var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending(orderfield));
                foreach (BsonDocument document in cursor)
                {
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {

                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
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
