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
namespace RivkaAreas.Reports.Models
{
    public class UsersReport : MongoModel
    {
        //
        // GET: /Reports/UsersReport/
        private MongoCollection collection;
        private MongoConection conection;
        private MongoCollection collection1;
        private MongoConection conection1;
      
        public  UsersReport(string table):base("Users") {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
           // collection.EnsureIndex(IndexKeys.Ascending("_id"),IndexOptions.SetUnique(true)); 
          
        }
        public String getUsersByLocations(List<string> locations = null)
        {

            try
            {
                //Query needed to get the result
               
                BsonArray bsonarray = new BsonArray();

               foreach(string item in locations)
                {
                    bsonarray.Add(item);
                }
               var query = Query.And(Query.In("userLocations.id", bsonarray));
               

                JoinCollections Join = new JoinCollections();
                Join.Select("Users")
                    .Join("Profiles", "profileId", "_id", "name=>profilename");


                return Join.Find(query);

               
            }
            catch
            {
                return null;
            }
        }

        public String GetRowsReportUser(List<string> profile = null,Dictionary<string,string> fields=null,Int64 start=0,Int64 end=0,List<string> locations=null)
        {
           

            var orderfield = "name";
            BsonArray locsbson = new BsonArray();
            try
            {
                if (locations != null)
                {
                    foreach (string loc in locations)
                    {
                        locsbson.Add(loc);
                    }
                }
            }
            catch { }
        
           

            try
            {
              
                string[] arrayprofile = profile.ToArray();
                List<string> cols = new List<string>();
                if (fields == null || fields.Count()==0)
                {
                    cols.Add("user");
                    cols.Add("name");
                    cols.Add("lastname");
                    cols.Add("profileId");
                    cols.Add("email");
                    cols.Add("CreatedTimeStamp");
                }
                else
                {
                    foreach (var x in fields)
                    {
                        cols.Add(x.Key);
                    }
                    if (!fields.Keys.Contains("profileId"))
                    {
                        cols.Add("profileId");
                    }
                    cols.Add("CreatedTimeStamp");
                }
                BsonArray bsonarray = new BsonArray();

                for (int i = 0; i < arrayprofile.Length; i++)
                {
                    bsonarray.Add(arrayprofile[i]);
                }

                string[] arrayfields = cols.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();
                if (profile != null && profile.Count>0)
                {
                    var query = Query.And(Query.In("profileId", bsonarray), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                   
                    if (locsbson.Count() > 0)
                    {
                        query = Query.And(Query.In("profileId", bsonarray), Query.In("userLocations.id", locsbson), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                   
                    }
                    
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
                }
                else
                {
                    var query = Query.And( Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

                    var cursor = collection.FindAs(typeof(BsonDocument), query).SetFields(arrayfields).SetSortOrder(SortBy.Ascending(orderfield));
                    foreach (BsonDocument document in cursor)
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                        try { 
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex)
                        {

                        }
                        documents.Add(document);
                    }

                }
                return documents.ToJson();
            }
            catch (Exception e)
            {
            //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }
    }
}
