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
    public class InventoryReport : MongoModel
    {
        private MongoCollection collection;
        private MongoConection conection;
        public InventoryReport(string table)
            : base("Inventory")
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
        }

        public String GetRowsReportInventory(Dictionary<string, string> fields = null,List<string> locs=null, Int64 start = 0, Int64 end = 0)
        {

            var orderfield = "name";



            try
            {

                List<string> cols = new List<string>();
                if (fields == null || fields.Count() == 0)
                {

                    cols.Add("name");


                    cols.Add("location");
                    cols.Add("hardware");
                    cols.Add("profile");
                    cols.Add("CreatedTimeStamp");
                }
                else
                {
                    foreach (var x in fields)
                    {
                        cols.Add(x.Key);
                    }
                    if (!fields.Keys.Contains("location"))
                    {
                        cols.Add("location");
                    }
                    if (!fields.Keys.Contains("hardware"))
                    {
                        cols.Add("hardware");
                    }
                    if (!fields.Keys.Contains("profile"))
                    {
                        cols.Add("profile");
                    }

                    cols.Add("CreatedTimeStamp");
                }

                string[] arrayfields = cols.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();
                BsonArray locs1 = new BsonArray();
                foreach (string lo in locs)
                {
                    locs1.Add(lo);
                }

                JoinCollections Join = new JoinCollections();
                Join.Select("Inventory")
                .Join("Locations", "location", "_id", "name=>locationname")
                .Join("Users", "Creator", "_id", "name=>Creator")
                 .Join("Profiles", "profile", "_id", "name=>profile");
               

                var query = Query.And(Query.In("location",locs1), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

             /*   var cursor = collection.FindAs(typeof(BsonDocument), query).SetFields(arrayfields).SetSortOrder(SortBy.Ascending(orderfield));
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


                return documents.ToJson();*/
                 return Join.Find(query);
                 }
            catch (Exception e)
            {
                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
           
        }

    }
}
