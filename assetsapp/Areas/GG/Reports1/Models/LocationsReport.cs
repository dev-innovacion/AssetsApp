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
    public class LocationsReport : MongoModel
    {
        private MongoCollection collection;
        private MongoConection conection;
        public LocationsReport(string table)
            : base("Locations")
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
        }
     
        public String GetRowsFilterProfile(string field)
        {

            var orderfield = "name";



            try
            {



                  BsonArray bsonarray = new BsonArray();
                  List<BsonDocument> documents = new List<BsonDocument>();
                  
                 try
                    {
                        bsonarray.Add(field);
                    }
                    catch (Exception e) { }
             

                var query = Query.And(Query.In("name", bsonarray));

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
        public String GetRowsFilterbyConjunt(List<string> idslocation)
        {

            var orderfield = "name";



            try
            {



                string[] arrayfields = idslocation.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();
                BsonArray bsonarray = new BsonArray();

                for (int i = 0; i < arrayfields.Length; i++)
                {
                    try
                    {
                        bsonarray.Add( arrayfields[i] );
                    }
                    catch (Exception e) { }
                }

                var query = Query.And(Query.In("profileId", bsonarray));

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
     
        public String GetRowsFilter(List<string> idslocation)
        {

            var orderfield = "name";



            try
            {



                string[] arrayfields = idslocation.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();
                BsonArray bsonarray = new BsonArray();

                for (int i = 0; i < arrayfields.Length; i++)
                {
                    try
                    {
                        bsonarray.Add(new BsonObjectId(arrayfields[i]));
                    }
                    catch(Exception e){}
                }

                var query = Query.And(Query.In("_id", bsonarray));

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
        public String GetChildrens(List<string> idslocation)
        {

            var orderfield = "name";



            try
            {



                string[] arrayfields = idslocation.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();
                BsonArray bsonarray = new BsonArray();

                for (int i = 0; i < arrayfields.Length; i++)
                {
                    try
                    {
                        bsonarray.Add(arrayfields[i]);
                    }
                    catch (Exception e) { }
                }

                var query = Query.And(Query.In("parent", bsonarray));

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
        public string Getparents(List<string> varLocations)
        {
            //Query needed to get the result
            string[] arraylocations = varLocations.ToArray();
            BsonArray bsonarray = new BsonArray();

            for (int i = 0; i < arraylocations.Length; i++)
            {
                bsonarray.Add(new BsonObjectId(arraylocations[i]));
            }
            var query = Query.And(Query.In("_id", bsonarray));
            if (varLocations.Count() == 0)
                query = Query.And(Query.EQ("system_status", "true"));


            JoinCollections Join = new JoinCollections();
            Join.Select("Locations")
                .Join("Locations", "parent", "_id", "name=>conjuntoname,number=>conjuntonumber");


            return Join.Find(query);
        }
        public String GetRowsReportLoc(Dictionary<string, string> fields = null, Int64 start = 0, Int64 end = 0,List<string> locations=null)
        {

            var orderfield = "name";



            try
            {
                BsonArray locsidbson = new BsonArray();
                if (locations != null)
                {
                    foreach (string loc in locations)
                    {
                        try
                        {

                            locsidbson.Add(new BsonObjectId(loc));
                        }
                        catch
                        {

                        }
                    }
                }
                List<string> cols = new List<string>();
                if (fields == null || fields.Count() == 0)
                {

                    cols.Add("name");


                    cols.Add("profileId");
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

                string[] arrayfields = cols.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();




                var query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                    if(locsidbson.Count()>0){
                        query=Query.And(Query.In("_id",locsidbson),Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
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


                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}

