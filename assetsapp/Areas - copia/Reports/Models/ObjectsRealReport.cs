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
    public class ObjectsRealReport : MongoModel
    {
        //
        // GET: /Reports/UsersReport/
        private MongoCollection collection;
        private MongoConection conection;
        private MongoCollection collection1;
        private MongoConection conection1;

        public ObjectsRealReport(string table): base("ObjectReal")
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
          //  collection.EnsureIndex(IndexKeys.Ascending("_id"), IndexOptions.SetUnique(true));

        }
       
        public string GetObjectsRealTable(List<string> varLocations)
        {
            //Query needed to get the result
            string[] arraylocations = varLocations.ToArray();
            BsonArray bsonarray = new BsonArray();
         
            for (int i = 0; i < arraylocations.Length; i++)
            {
                bsonarray.Add(arraylocations[i]);
            }
            var query = Query.And(Query.In("location", bsonarray));
            if (varLocations.Count() ==0)
                query = Query.And(Query.EQ("system_status", "true"));
           

            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "profileFields=>customfields")
                .Join("Locations", "location", "_id", "name => location")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object");
               

            return Join.Find(query);
        }
        public string GetbyCustom(string key, List<string> datas, string Collection)
        {
            //Query needed to get the result
            BsonArray datasarray = new BsonArray();
            foreach (string data in datas)
            {
                try
                {
                    if (key == "_id")
                    {
                        datasarray.Add(new BsonObjectId(data));
                    }
                    else
                    {
                        datasarray.Add(data);
                    }
                }
                catch { }
            }
            var query = Query.And(Query.In(key, datasarray));

            JoinCollections Join = new JoinCollections();
            Join.Select(Collection)
                .Join("Users", "Creator", "_id", "name=>user_name");


            return Join.Find(query);
        }
        public string GetCategories()
        {
            //Query needed to get the result

            var query = Query.NE("system_status", false);
               
            JoinCollections Join = new JoinCollections();
            Join.Select("Categories")
                   .Join("Users", "Creator", "_id", "name=>Creator");


            return Join.Find(query);
        }
        public string GetRowJoin(string id)
        {
            //Query needed to get the result
            var  query = Query.And(Query.EQ("_id", new BsonObjectId(id)));


            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                 .Join("ObjectReal", "objects.id", "_id", "name => objects.name")
                 .Join("Locations", "objects.location", "_id", "name => objects.location")
                 .Join("Locations", "objects.locationDestiny", "_id", "name => objects.locationDestiny")
                 .Join("Locations", "objects.conjuntoDestiny", "_id", "name => objects.conjuntoDestiny")
                 .Join("Locations", "objects.conjunto", "_id", "name => objects.conjunto")
                .Join("ReferenceObjects", "objects.objectReference", "_id", "name=>objects.objectReference,object_id=>objects.id_articulo,parentCategory=>parentCategory");


            return Join.Find(query);
        }

        public String GetRowsReportObjectsReal(List<string> profile = null, Dictionary<string, string> fields = null, Int64 start = 0, Int64 end = 0,string id=null)
        {


            var orderfield = "name";




            try
            {  

                string[] arrayprofile = profile.ToArray();
                List<string> cols = new List<string>();
                if (fields == null || fields.Count() == 0)
                {
                    cols.Add("name");
                    cols.Add("location");
                   
                    cols.Add("EPC");
                    cols.Add("serie");
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
                    cols.Add("CreatedTimeStamp");
                }
                BsonArray bsonarray = new BsonArray();

                for (int i = 0; i < arrayprofile.Length; i++)
                {
                    bsonarray.Add(arrayprofile[i]);
                }

                string[] arrayfields = cols.ToArray();
                List<BsonDocument> documents = new List<BsonDocument>();
                var query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));


                if (id != null)
                {
                  BsonValue idbson = new BsonObjectId(id);
                  query = Query.And(Query.EQ("_id",idbson));

                }
               
                if (profile != null && profile.Count > 0)
                {
                    if (id != null)
                    {
                        BsonValue idbson = new BsonObjectId(id);
                        query = Query.And(Query.EQ("_id",idbson));
              
                    }
                    else
                    {
                        query = Query.And(Query.In("location", bsonarray), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                    }
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
                    }*/
                }
                else
                {
                    
                   /* var cursor = collection.FindAs(typeof(BsonDocument), query).SetFields(arrayfields).SetSortOrder(SortBy.Ascending(orderfield));
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
                    }*/

                }
                JoinCollections Join = new JoinCollections();
                Join.Select("ObjectReal")
                     .Join("Locations", "location", "_id", "parent=>parent")
                     .Join("ReferenceObjects", "objectReference", "_id", "name=>name,depreciacion=>depreciacion,parentCategory=>parentCategory,marca=>marca,modelo=>modelo,perfil=>perfil,department=>department,proveedor=>proveedor,object_id=>object_id_ref");
                     //.Join("Deparments", "department", "idDep", "name =>department");
                     


                return Join.Find(query);
                //return documents.ToJson();
            }
            catch (Exception e)
            {
                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }
    }
}

