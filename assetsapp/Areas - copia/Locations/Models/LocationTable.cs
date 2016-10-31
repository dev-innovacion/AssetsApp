using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Locations.Models
{
    [Authorize]
    public class LocationTable:MongoModel
    {
         //staticFieds contains the required fields, each document must have this values
        static public List<String> staticFields = new List<String>() { "name", "parent","tipo", "setupname", "setup", "profileId", "profileFields" };


        /// <summary>
        ///     Initializes the model
        /// </summary>
        /// <param name="collectionname">
        ///     the mongo's collections where the data is
        /// </param>
        public LocationTable()
            : base("Locations")
        {
        }


        /// <summary>
        /// Changes the User´s profileId
        /// </summary>
        /// <param name="user">
        /// Receive an User object with the _id and the new profileId
        /// </param>
        public void updateProfile(dynamic user)
        {
            var query = Query.And(
                Query.EQ("_id", new ObjectId(user._id))
            );
            var update = Update.Set("profileId",user.profileId);
            collection.Update(query,update);
        }

        /// <summary>
        ///     This function generates a query, and returns all coincidences where key = value
        /// </summary>
        /// <param name="key">
        ///     The collection's key value in the db
        /// </param>
        /// <param name="value">
        ///     The collection's value in the db
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author> 
        /// <returns></returns>
        public List<BsonDocument> get(string key, string value)
        {
            var query = Query.And(Query.EQ(key, value)); //creating the query
            var cursor = collection.FindAs(typeof(BsonDocument), query); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //putting the docuemnts into a list
            {
                if (isValidDocument(document)) //firts we have to check if this document has a valid structure
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    documents.Add(document);
            }
            return documents;
        }


        /// <summary>
        ///     This method validates a document
        /// </summary>
        /// <param name="document">
        ///     The document that we want to validate
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a boolean, is this document valid?
        /// </returns>
        public bool isValidDocument(BsonDocument document)
        {
            try //trying to validate the document if an exception occurs the document is not valid
            {
                if (document == null) return false; //the document can not be null
            List<string> keys = new List<string>();
                foreach (BsonElement element in document) //creating an array with every key in the document
                {
                    keys.Add(element.Name);
            }
                if (staticFields.Except(keys).ToList().Count() != 0) //checks if there are staticFields that are not in the document
                    return false;
            return true;
            }
            catch (Exception e) {
                return false;
            }
        }

        public int GetNumObjects(String idlocation)
        {
            int result = 0;
            BsonArray objs = new BsonArray();
            if (idlocation == null || idlocation=="")
            {
                idlocation = "null";
            }

            string queryFunction = "getSubObjects ('" + idlocation + "' )";
            BsonJavaScript JoinFunction = new BsonJavaScript(queryFunction);

            //Calling the stores Mongo Function
            MongoConection conection = (MongoConection)Conection.getConection();
            objs = conection.getDataBase().Eval(JoinFunction).AsBsonArray;

            result = objs.Count;

            return result;
        }

        public string GetLocationsTable(string vartypeuser,string vartype, string varRegion, string varConjunto,string varSubLocation, List<string> locations=null)
        {
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            IMongoQuery query = null;

            BsonArray bsonarrayids = new BsonArray();
            if (locations != null) {
                foreach (string item in locations)
                {
                    bsonarrayids.Add(new ObjectId(item));
                }
            }
            

            //Query needed to get the result
            if (vartype == "")
            {
                listqueries.Add(Query.NE("profileId", varRegion));
                listqueries.Add(Query.NE("profileId", varConjunto));
                listqueries.Add(Query.NE("profileId", varSubLocation));
                if(locations!=null)
                    listqueries.Add(Query.In("_id", bsonarrayids));
            }
            
            else if(vartype=="Region"){
                listqueries.Add(Query.EQ("profileId", varRegion));
                if (locations != null)
                    listqueries.Add(Query.In("_id", bsonarrayids));
                
            }
            else if (vartype == "Conjunto")
            {
                listqueries.Add(Query.EQ("profileId", varConjunto));
                if (locations != null)
                    listqueries.Add(Query.In("_id", bsonarrayids));
            }
          
            query = Query.And(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("Locations")
                .Join("Users", "responsable", "_id", "name=>nameResponsable");

            return Join.Find(query);
        }

        public string GetLocations(String profileid, String parent)
        {
            IMongoQuery query = null;
            query = Query.And(Query.EQ("profileId", profileid), Query.EQ("parent", parent));
            JoinCollections Join = new JoinCollections();
            Join.Select("Locations");

            return Join.Find(query);
        }

        public string ExistLocation(String fisrt, string valueF, string secound, string valueS) {
            var query = Query.And(Query.EQ(fisrt, valueF), Query.EQ(secound, valueS));
            JoinCollections Join = new JoinCollections();
            Join.Select("Locations");

            return Join.Find(query);
        }
    }
}