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

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Tags.Models
{

    /// <summary>
    ///     This class abstracts the ReferenceObject's collection
    /// </summary>
    public class ObjectTable : MongoModel
    {

        //staticFieds contains the required fields, each document must have this values
        //static public List<String> staticFields = new List<String>() { "user", "pwd", "imgext", "email", "name", "lastname", "profileId", "profileFields" };

        /// <summary>
        ///     Intilializes the model, sets the collection to users
        /// </summary>
        public ObjectTable(string table = "ReferenceObjects") : base(table)
        {
        }

        /// <summary>
        ///     This method looks for a document with the specified Id
        /// </summary>
        /// <param name="objectId">
        ///     The document's id we are looking fo
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns document with this id
        /// </returns>
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
            catch (Exception e) {
                return null;
            }
        }

        /// <summary>
        ///     This method returns the whole collection's documents
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     returns the collection's documments
        /// </returns>
        public String getRows()
        {
            var cursor = collection.FindAllAs(typeof(BsonDocument)); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //setting each document in an array
            {
                if (isValidDocument(document)) //is this document valid?
                {
                    documents.Add(document); //if it's valid add it to the list
            }
                else {
                    try //if the document is not valid delete it, if it can be deleted
                    {
                        deleteRow(document.GetElement("_id").Value.ToString());
                    }
                    catch (Exception e) { /*Ignored*/ } 
                }
            }

            return documents.ToJson();
        }

        /// <summary>
        ///     This method allows to create or update an user if an id is gived
        /// </summary>
        /// <param name="jsonString">
        ///     the document's json without _id
        /// </param>
        /// <param name="id">
        ///     the document's id
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the id of the saved document
        /// </returns>
        public String saveRow(String jsonString, string id = null)
        {
            BsonDocument doc;
            
            try //trying to parse the jsonString into a bsondocument
            {
                doc = BsonDocument.Parse(jsonString);
            }catch( Exception e ){ //if it's not a valid jsonString, the user can't be saved
                return null;
            }

            if (!isValidDocument(doc)) //is the document valid?
                return null;

            if (id != null && id != "" && id != "null") //is this an update?
            {
                doc.Set("_id", new ObjectId(id)); //setting the user's id to the document
                try
                {
                    doc.Set("CreatedDate", BsonDocument.Parse(getRow(id)).GetElement("CreatedDate").Value); //the createdDate must not be modified
                }
                catch (Exception e) {
                    doc.Set("CreatedDate", DateTime.Now.ToString()); //setting the createdDate
                }

                try
                {
                    doc.Set("Creator", BsonDocument.Parse(getRow(id)).GetElement("Creator").Value);
                }
                catch (Exception e) {
                    doc.Set("Creator", HttpContext.Current.Session["_id"].ToString());
                }
            }
            else //is a new user?
            {
                doc.Set("CreatedDate", DateTime.Now.ToString() ); //setting the createdDate
                doc.Set("Creator", HttpContext.Current.Session["_id"].ToString());
            }
            doc.Set("LastmodDate", DateTime.Now.ToString() ); //either it is a new user or an update we set the lastmodDate to this Day
            collection.Save(doc);
            return doc["_id"].ToString();
        }

        /// <summary>
        /// Changes the User´s profileId
        /// </summary>
        /// <param name="user">
        /// Receive an User object with the _id and the new profileId
        /// </param>
        //public void updateProfile(dynamic user)
        //{
        //    var query = Query.And(
        //        Query.EQ("_id", new ObjectId(user._id))
        //    );
        //    var update = Update.Set("profileId",user.profileId);
        //    collection.Update(query,update);
        //}

        /// <summary>
        ///     This method deletes a document from the db
        /// </summary>
        /// <param name="rowID">
        ///     the document's id to delete
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        public void deleteRow(String rowID)
        {
            try
            {
                var query = Query.And(Query.EQ("_id", new ObjectId(rowID)));
                collection.Remove(query);
            }catch (Exception e) { /*Ignored*/ }
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
        public String get(String key, String value)
        {
            var query = Query.And(Query.EQ(key, value)); //creating the query
            var cursor = collection.FindAs(typeof(BsonDocument), query); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //putting the docuemnts into a list
            {
                if (isValidDocument(document)) //firts we have to check if this document has a valid structure
                    document.Set( "_id", document.GetElement("_id").Value.ToString());
                    documents.Add(document);
            }

            return documents.ToJson();
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
            return true;
            //try //trying to validate the document if an exception occurs the document is not valid
            //{
            //    if (document == null) return false; //the document can not be null
            //List<string> keys = new List<string>();
            //    foreach (BsonElement element in document) //creating an array with every key in the document
            //    {
            //        keys.Add(element.Name);
            //}
            //    if (staticFields.Except(keys).ToList().Count() != 0) //checks if there are staticFields that are not in the document
            //        return false;
            //    ProfileTable profileTable = new ProfileTable();
            //    if (profileTable.getRow(document.GetElement("profileId").Value.ToString()) == null) //the profileID gived exists?
            //    return false;
            //return true;
            //}
            //catch (Exception e) {
            //    return false;
            //}
        }

        public String getEPC(String idObjRef, String numEtiq){
            String newepc="";

            if (idObjRef != null && numEtiq != null) {

                string queryFunction = "generateEPC ( '" + idObjRef + "', " + numEtiq + " )";
                BsonJavaScript JoinFunction = new BsonJavaScript(queryFunction);

                //Calling the stores Mongo Function
                MongoConection conection = (MongoConection)Conection.getConection();
                BsonDocument result = conection.getDataBase().Eval(JoinFunction).AsBsonDocument;

                newepc = result.ToJson();
            
            }
            return newepc;
        }

        public String NewObjectEPC(String idObjRef, String IdClient) {
            String result = "";

            if (idObjRef != null && IdClient != null)
            {

                string queryFunction = "NewObjectBaseEPC ( '" + IdClient + "', '" + idObjRef + "' )";
                BsonJavaScript JoinFunction = new BsonJavaScript(queryFunction);

                //Calling the stores Mongo Function
                MongoConection conection = (MongoConection)Conection.getConection();
                result = conection.getDataBase().Eval(JoinFunction).AsString;

            }
            return result;
        }
    }
}