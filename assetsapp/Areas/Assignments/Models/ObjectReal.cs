using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Drawing;
using Rivka.Db.MongoDb;
using Newtonsoft.Json;
using Rivka.Db;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
namespace RivkaAreas.Assignments.Models
{
    public class ObjectReal : MongoModel
    {
        //staticFieds contains the required fields, each document must have this values
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        static public List<String> staticFields = new List<String>() { "_id", "name" };

        public ObjectReal(string table = "ObjectReal")
            : base(table)
        {

        }
        public bool isValidDocument(BsonDocument document)
        {
            return true;

        }

        public String getRow(string objectId, string AssignMentId)
        {
            Object resultObject = null;
            try //trying to get the document, if an exception occurs there is not such document
            {
                resultObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
                BsonDocument result = resultObject.ToBsonDocument();
                result.Set("_id", result.GetElement("_id").Value.ToString());
                result.Remove("CreatedTimeStamp");
                if (AssignMentId != "")
                {
                    result.Add("AssignMentId", AssignMentId);
                }
                return result.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public BsonDocument getbasonRow(string objectId)
        {
            Object resultObject = null;
            try //trying to get the document, if an exception occurs there is not such document
            {
                resultObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
                BsonDocument result = resultObject.ToBsonDocument();
                result.Set("_id", result.GetElement("_id").Value.ToString());
                result.Remove("CreatedTimeStamp");

                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }


        public string GetObjectsReferences()
        {

            JoinCollections Join = new JoinCollections();
            Join.Select("ReferenceObjects")
                .Join("ObjectReal", "_id", "objectReference", "object=>object");


            return Join.Find();
        }

        /// <summary>
        /// Returns all objects from a location and the locations whitin
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public string GetSubObjects(string locationId = null)
        {
            locationId = (locationId != null && locationId != "") ? locationId : "";

            string queryFunction = "getSubObjects ( \"" + locationId + "\")";
            BsonJavaScript SubFunction = new BsonJavaScript(queryFunction);

            //Calling the stores Mongo Function
            MongoConection conection = (MongoConection)Conection.getConection();
            BsonArray result = conection.getDataBase().Eval(SubFunction).AsBsonArray;
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in result)
            {
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

        public string GetdemandFolio(string objid = null)
        {
            objid = (objid != null && objid != "") ? objid : "";

            string queryFunction = "getDemandOfObject ( \"" + objid + "\")";
            BsonJavaScript SubFunction = new BsonJavaScript(queryFunction);

            //Calling the stores Mongo Function
            MongoConection conection = (MongoConection)Conection.getConection();
            String result = conection.getDataBase().Eval(SubFunction).AsString;

            return result;
        }

        public string GetAlldemandsFolio(string objid = null)
        {
            objid = (objid != null && objid != "") ? objid : "";

            string queryFunction = "getAllDemandsOfObject ( \"" + objid + "\")";
            BsonJavaScript SubFunction = new BsonJavaScript(queryFunction);

            //Calling the stores Mongo Function
            MongoConection conection = (MongoConection)Conection.getConection();
            String result = conection.getDataBase().Eval(SubFunction).AsString;

            return result;
        }

        public string GetSubObjectsDemand(string locationId = null)
        {
            locationId = (locationId != null && locationId != "") ? locationId : "";

            string queryFunction = "getSubObjectsDemand ( \"" + locationId + "\")";
            BsonJavaScript SubFunction = new BsonJavaScript(queryFunction);

            //Calling the stores Mongo Function
            MongoConection conection = (MongoConection)Conection.getConection();
            BsonArray result = conection.getDataBase().Eval(SubFunction).AsBsonArray;
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in result)
            {
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
        public string GetDemandByObj(List<string> idsact, int type = 0)
        {

            BsonArray bsonarray = new BsonArray();
            BsonArray bsonstatus = new BsonArray();
            bsonstatus.Add(6);
            bsonstatus.Add(7);
            foreach (string x in idsact)
            {
                try
                {

                    bsonarray.Add(x);
                }
                catch
                {


                }
            }
            //Get only active objects

            IMongoQuery query = Query.And(Query.In("status", bsonstatus), Query.In("objects.id", bsonarray));
            if (type == 1)
            {
                query = Query.And(Query.NotIn("status", bsonstatus), Query.In("objects.id", bsonarray));
            }


            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "name=>namemov");


            return Join.Find(query);
        }

        public string GetObjects(string varLocation, string varReference = null)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            //Query needed to get the result
            if (varLocation != null || varLocation == "")
            {
                listqueries.Add(Query.EQ("location", varLocation));
            }
            if (varReference != null || varReference == "")
            {
                listqueries.Add(Query.EQ("objectReference", varReference));
            }

            //Get only active objects
            //  listqueries.Add(Query.EQ("system_status", true));

            query = Query.And(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext, profileFields, parentCategory")
                .Join("Users", "Creator", "_id", "name=>nameCreator, lastname=>lastnameCreator")
                .Join("Locations", "location", "_id", "name=>namelocation");


            return Join.Find(query);
        }
        public String Validserie(List<string> numbers)
        {
            BsonArray series = new BsonArray();
            foreach (string num in numbers)
            {
                series.Add(num);
            }

            IMongoQuery query = Query.And(Query.In("serie", series), Query.NE("system_status", false));
            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
               .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext, profileFields, parentCategory")
               .Join("Users", "Creator", "_id", "name=>Creator")
               .Join("Locations", "location", "_id", "name=>location");
            return Join.Find(query);
        }
        public String getMMO(List<string> list, int type = 0)
        {
            BsonArray bson = new BsonArray();
            foreach (string element in list)
            {
                try
                {
                    bson.Add(new BsonObjectId(element));
                }
                catch { }
            }

            IMongoQuery query = Query.And(Query.In("_id", bson));
            JoinCollections Join = new JoinCollections();
            if (type == 0)
            {
                Join.Select("ReferenceObjects")
                   .Join("Users", "Creator", "_id", "name=>username");
            }
            else
            {
                Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "marca,modelo,object_id,profileFields, parentCategory,assetType")
                .Join("Users", "Creator", "_id", "name=>Creator")
                .Join("Locations", "location", "_id", "name=>namelocation");
            }
            return Join.Find(query);
        }
        public String getParents(List<string> list)
        {
            BsonArray bson = new BsonArray();
            foreach (string element in list)
            {
                try
                {
                    bson.Add(new BsonObjectId(element));
                }
                catch { }
            }

            IMongoQuery query = Query.And(Query.In("_id", bson), Query.NE("system_status", false));
            JoinCollections Join = new JoinCollections();
            Join.Select("Locations")
               .Join("Locations", "parent", "_id", "name=>nameparent");

            return Join.Find(query);
        }

        //*****************************
        public String getLocations(List<string> list)
        {
            BsonArray bson = new BsonArray();
            foreach (string element in list)
            {
                try
                {
                    bson.Add(new BsonObjectId(element));
                }
                catch { }
            }

            IMongoQuery query = Query.And(Query.In("_id", bson), Query.NE("system_status", false));
            JoinCollections Join = new JoinCollections();
            Join.Select("Locations")
               .Join("Locations", "parent", "_id", "name=>nameparent");

            return Join.Find(query);
        }

        public String getCategory(List<string> list)
        {
            BsonArray bson = new BsonArray();
            foreach (string element in list)
            {
                try
                {
                    bson.Add(new BsonObjectId(element));
                }
                catch { }
            }

            IMongoQuery query = Query.And(Query.In("_id", bson), Query.NE("system_status", false));
            JoinCollections Join = new JoinCollections();
            Join.Select("ReferenceObjects")
               .Join("Categories", "parentCategory", "_id", "name=>namecategory");

            return Join.Find(query);
        }
        public String ValidserieDemands(List<string> numbers)
        {
            BsonArray series = new BsonArray();
            BsonInt64 num1 = 7;
            foreach (string num in numbers)
            {
                series.Add(num);
            }

            IMongoQuery query = Query.And(Query.In("objects.serie", series), Query.NE("status", num1), Query.NE("system_status", false));
            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
               .Join("ReferenceObjects", "objects.objectReference", "_id", "name=>objectref");

            return Join.Find(query);
        }

        public string GetObjectsByText(string texto, List<BsonValue> lista, List<string> locationsid, List<string> usersid, List<string> assettype, List<string> refobjsids)
        {
            IMongoQuery query, query2;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            List<IMongoQuery> listqueries2 = new List<IMongoQuery>();
            //Query needed to get the result
            BsonArray locationsbson = new BsonArray();
            BsonArray usersbson = new BsonArray();
            BsonArray categbson = new BsonArray();
            BsonArray refobjbson = new BsonArray();

            try
            {
                foreach (string id in locationsid)
                {
                    locationsbson.Add(id);
                }

            }
            catch { }
            try
            {
                foreach (string id in refobjsids)
                {
                    refobjbson.Add(id);
                }

            }
            catch { }

            try
            {
                foreach (string id in usersid)
                {
                    usersbson.Add(id);
                }

            }
            catch { }
            try
            {
                foreach (string name in assettype)
                {
                    categbson.Add(name);
                }

            }
            catch { }

            if (texto != "")
            {
                BsonRegularExpression match = new BsonRegularExpression("/" + texto + "/i");
                listqueries.Add(Query.Matches("name", match));
                listqueries.Add(Query.Matches("EPC", match));
                listqueries.Add(Query.Matches("EPC_conflicto", match));
                listqueries.Add(Query.Matches("status", match));
                listqueries.Add(Query.Matches("serie", match));
                listqueries.Add(Query.Matches("object_id", match));
                listqueries.Add(Query.Matches("id_registro", match));
                listqueries.Add(Query.Matches("marca", match));
                listqueries.Add(Query.Matches("quantity", match));
                listqueries.Add(Query.Matches("modelo", match));
                listqueries.Add(Query.Matches("price", match));
                listqueries.Add(Query.Matches("label", match));
                listqueries.Add(Query.Matches("assetType", match));
            }
            if (lista.Count() > 0)
            {
                listqueries.Add(Query.In("objectReference", lista));
            }
            if (refobjbson.Count() > 0)
            {
                listqueries.Add(Query.In("objectReference", refobjbson));
            }
            if (usersbson.Count() > 0)
            {
                listqueries.Add(Query.In("Creator", usersbson));
            }
            if (categbson.Count() > 0)
            {
                listqueries.Add(Query.In("assetType", categbson));
            }


            query2 = Query.Or(listqueries);
            listqueries2.Add(query2);
            if (locationsbson.Count() > 0)
            {
                listqueries2.Add(Query.In("location", locationsbson));
            }


            query = Query.And(listqueries2);

            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext, profileFields, parentCategory")
                .Join("Users", "Creator", "_id", "name=>Creator,lastname=>lastname")
                .Join("Locations", "location", "_id", "name=>location");


            return Join.Find(query);
        }
        public string GetRefObjByText(string texto)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            //Query needed to get the result
            if (texto != "")
            {
                BsonRegularExpression match = new BsonRegularExpression("/" + texto + "/i");
                listqueries.Add(Query.Matches("name", match));

            }
            query = Query.Or(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("ReferenceObjects")
                .Join("Users", "Creator", "_id", "name=>Creator");



            return Join.Find(query);
        }

        public string GetButacas(String idlocation)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            //Query needed to get the result

            BsonRegularExpression match = new BsonRegularExpression("/Butaca/i");
            listqueries.Add(Query.Matches("name", match));
            listqueries.Add(Query.GT("quantity", "1"));
            query = Query.And(Query.EQ("location", idlocation), Query.Or(listqueries));

            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object");
            return Join.Find(query);
        }

        public string GetIdUnico()
        {
            string queryFunction = "getIDOne()";
            BsonJavaScript SubFunction = new BsonJavaScript(queryFunction);

            //Calling the stores Mongo Function
            MongoConection conection = (MongoConection)Conection.getConection();
            double result = conection.getDataBase().Eval(SubFunction).AsDouble;

            return result.ToString();
        }

        public string FilterObjectsByText(string texto, string locationId)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();

            if (texto != "")
            {
                BsonRegularExpression match = new BsonRegularExpression("/" + texto + "/i");
                listqueries.Add(Query.Matches("name", match));
                listqueries.Add(Query.Matches("EPC", match));
                listqueries.Add(Query.Matches("EPC_conflicto", match));
                listqueries.Add(Query.Matches("status", match));
                listqueries.Add(Query.Matches("serie", match));
                listqueries.Add(Query.Matches("object_id", match));
                listqueries.Add(Query.Matches("id_registro", match));
                listqueries.Add(Query.Matches("marca", match));
                listqueries.Add(Query.Matches("quantity", match));
                listqueries.Add(Query.Matches("modelo", match));
                listqueries.Add(Query.Matches("price", match));
                listqueries.Add(Query.Matches("label", match));
                listqueries.Add(Query.Matches("assetType", match));
            }
            if (locationId != null && locationId != "" && locationId != "null")
            {


                //  Query.And(Query.EQ("location", locationId));
                listqueries.Add(Query.And(Query.EQ("location", locationId)));
            }

            query = Query.Or(listqueries);

            //  query = Query.And(listqueries2);


            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext, profileFields, parentCategory")
                .Join("Users", "Creator", "_id", "name=>Creator,lastname=>lastname")
                .Join("Locations", "location", "_id", "name=>location");


            return Join.Find(query);
        }

        public List<BsonDocument> get(string key, string value)
        {

        

            var query = Query.And(Query.EQ(key, value));
            var cursor = collection.FindAs(typeof(BsonDocument), query);
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor)
            {
                // if (isValidDocument(document)) 
                documents.Add(document);
            }

            return documents;
        }

        public BsonDocument getActivo(string key, string value)
        {



            var query = Query.And(Query.EQ(key, value));
            var cursor = collection.FindAs(typeof(BsonDocument), query);
            BsonDocument documents = new BsonDocument();
            foreach (BsonDocument document in cursor)
            {
                // if (isValidDocument(document)) 
                documents.Add(document);
            }

            return documents;
        }

    }
}