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
namespace RivkaAreas.ObjectAdmin.Models
{
    public class ObjectReal : MongoModel
    {
        //staticFieds contains the required fields, each document must have this values
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        static public List<String> staticFields = new List<String>() {"_id", "name" };

        public ObjectReal(string table = "ObjectReal")
            : base(table)
        {

        }
        public bool isValidDocument(BsonDocument document)
        {
            return true;
         
        }
        public String SaveRowFalse(String jsonString, String id = null)
        {
            BsonDocument doc;

            try //trying to parse the jsonString into a bsondocument
            {
                doc = BsonDocument.Parse(jsonString);
            }
            catch (Exception e)
            { //if it's not a valid jsonString, the user can't be saved
                return null;
            }

            /*if (!isValidDocument(doc)) //is the document valid?
                return null;*/

            if (id != null && id != "" && id != "null") //is this an update?
            {
                doc.Set("_id", new ObjectId(id)); //setting the user's id to the document
                try
                {
                    doc.Set("CreatedDate", BsonDocument.Parse(GetRow(id)).GetElement("CreatedDate").Value); //the createdDate must not be modified
                }
                catch (Exception e)
                {
                    doc.Set("CreatedDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //setting the createdDate
                }

                try
                {
                    doc.Set("Creator", BsonDocument.Parse(GetRow(id)).GetElement("Creator").Value);
                }
                catch (Exception e)
                {
                    try
                    {
                        doc.Set("Creator", HttpContext.Current.Session["_id"].ToString());
                    }
                    catch (Exception e2)
                    {
                        //do not set the creator
                    }
                }
            }
            else //is a new user?
            {
                doc.Set("CreatedDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //setting the createdDate
                doc.Set("CreatedTimeStamp", Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")));
                try
                {
                    doc.Set("Creator", HttpContext.Current.Session["_id"].ToString());
                }
                catch (Exception ex)
                {

                }
                doc.Set("system_status", false);
            }
            doc.Set("LastmodDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //either it is a new user or an update we set the lastmodDate to this Day
            collection.Save(doc);
            return doc["_id"].ToString();
        }
        public String getRow(string objectId)
        {
            Object resultObject = null;
            try //trying to get the document, if an exception occurs there is not such document
            {
                resultObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
                BsonDocument result = resultObject.ToBsonDocument();
                result.Set("_id", result.GetElement("_id").Value.ToString());

                if (!isValidDocument(result)) //if the document has not a correct structure, delete it and return no document
                {
                    deleteRow(objectId);
                    return null;
                }

                return result.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public void deleteRow(String rowID)
        {
            try
            {
                var query = Query.And(Query.EQ("_id", new ObjectId(rowID)));
                collection.Remove(query);
            }
            catch (Exception e) { /*Ignored*/ }
        }

        public string GetObjectsReferences() {

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
        public string GetSubObjects(string locationId = null,bool filter=false)
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
                string type = "true";
                if (filter)
                {
                    try
                    {
                        type = document.GetElement("system_status").Value.ToString().ToLower();
                    }
                    catch { }
                }
                if (filter && type == "false")
                {

                }
                else
                {
                    documents.Add(document);
                }
               
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
        public string GetDemandByObj(List<string> idsact,int type=0)
        {

            BsonArray bsonarray = new BsonArray();
            BsonArray bsonstatus=new BsonArray();
            bsonstatus.Add(6);
            bsonstatus.Add(7);
            foreach(string x in idsact){
                try{

                    bsonarray.Add(x);
                }catch{


                }
            }
            //Get only active objects
           
            IMongoQuery query = Query.And(Query.In("status", bsonstatus), Query.In("objects.id", bsonarray));
            if(type==1){
                query = Query.And(Query.NotIn("status", bsonstatus), Query.In("objects.id", bsonarray));
            }
            

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "name=>namemov");


            return Join.Find(query);
        }

        public string GetObjects(string varLocation, string varReference=null,bool filter=false)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            //Query needed to get the result
            if (varLocation != null || varLocation == "")
            {
                listqueries.Add(Query.EQ("location", varLocation));
            }
            if (varReference != null || varReference=="") {
                listqueries.Add(Query.EQ("objectReference", varReference));
            }
            if(filter==true)
                listqueries.Add(Query.EQ("system_status", false));
            //Get only active objects
          //  listqueries.Add(Query.EQ("system_status", true));

            query = Query.And(listqueries);
            
            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object,marca=>marca,modelo=>modelo,ext=>ext, profileFields=>profileFields, parentCategory=>parentCategory")
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
        public String getMMO(List<string> list,int type=0)
        {
            BsonArray  bson = new BsonArray();
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
                .Join("ReferenceObjects", "objectReference", "_id", "marca,modelo,object_id,profileFields,parentCategory,assetType")
                .Join("Users", "Creator", "_id", "name=>Creator")
                .Join("Locations", "location", "_id", "name=>location");
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
        public String getRefObj(List<string> list)
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
            Join.Select("ReferenceObjects")
               .Join("Categories", "parentCategory", "_id", "name=>nameassetType")
              .Join("Users", "Creator", "_id", "name=>Creator");

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
      /*  public string GetObjectsByText(string texto, List<BsonValue> lista)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            //Query needed to get the result
            if (texto != "")
            {
                BsonRegularExpression match = new BsonRegularExpression("/"+texto+"/i");
                listqueries.Add(Query.Matches("name", match));
                listqueries.Add(Query.Matches("EPC", match));
                listqueries.Add(Query.Matches("location", match));
                listqueries.Add(Query.In("objectReference", lista));
            }
            query = Query.Or(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext, profileFields, parentCategory")
                .Join("Users", "Creator", "_id", "name=>Creator")
                .Join("Locations", "location", "_id", "name=>location");


            return Join.Find(query);
        }*/
        public string GetObjectsByText(string texto, List<BsonValue> lista, List<string> locationsid, List<string> usersid, List<string> assettype, List<string> refobjsids)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
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
                listqueries.Add(Query.In("location", locationsbson));
                listqueries.Add(Query.Matches("serie", match));
                listqueries.Add(Query.Matches("object_id", match));
                listqueries.Add(Query.Matches("marca", match));
                listqueries.Add(Query.Matches("quantity", match));
                listqueries.Add(Query.Matches("modelo", match));
                listqueries.Add(Query.Matches("price", match));
                listqueries.Add(Query.Matches("label", match));
                listqueries.Add(Query.Matches("assetType", match));
                listqueries.Add(Query.In("objectReference", lista));
                listqueries.Add(Query.In("objectReference", refobjbson));
                listqueries.Add(Query.In("Creator", usersbson));
                listqueries.Add(Query.In("assetType", categbson));
            }
            query = Query.Or(listqueries);

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

    }
}