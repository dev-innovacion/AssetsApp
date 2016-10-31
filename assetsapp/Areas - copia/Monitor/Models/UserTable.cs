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
using Rivka.Security;

namespace RivkaAreas.Monitor.Models
{

    /// <summary>
    ///     This class abstracts the user's collection
    /// </summary>
    public class UserTable : MongoModel
    {
     
       
        //staticFieds contains the required fields, each document must have this values
        static public List<String> staticFields = new List<String>() { "user", "pwd", "imgext", "email", "name", "lastname", "profileId", "profileFields" };

        /// <summary>
        ///     Intilializes the model, sets the collection to users
        /// </summary>
        public UserTable():base("Users")
        {
        }

        public BsonDocument Login(string nombre, string password)
        {
            var query = Query.And(
                Query.EQ("user", nombre));
            object cursor = collection.FindOneAs(typeof(BsonDocument), query);

            BsonDocument doc = cursor.ToBsonDocument();

            if (doc != null)
            {
                if (!HashPassword.ValidatePassword(password, doc["pwd"].ToString())) {
                    return null;
                }
                return doc;
            }
            else
            {
                query = Query.And(
                    Query.EQ("email", nombre));
                cursor = collection.FindOneAs(typeof(BsonDocument), query);

                doc = cursor.ToBsonDocument();
                
                if(doc!=null)
                {
                    if (!HashPassword.ValidatePassword(password, doc["pwd"].ToString()))
                    {
                        return null;
                    }
                    return doc;
                }

                return null;

            }
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
        public BsonDocument getRow(string objectId)
        {
            Object resultObject = null;
            try //trying to get the document, if an exception occurs there is not such document
            {
                resultObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
            }
            catch (Exception e) { /*ignored*/ }
            BsonDocument result = resultObject.ToBsonDocument();

            try
            {
                result.Set("_id", result.GetElement("_id").Value.ToString());
            }
            catch (Exception e) { }
            try
            {
                result.Set("CreatedTimeStamp", result.GetElement("CreatedTimeStamp").Value.ToString());
            }
            catch (Exception ex)
            {

            } 
            if (!isValidDocument(result)) //if the document has not a correct structure, delete it and return no document
            { 
                return null;
            }
            return result;
        }

        public String getRowString(string objectId)
        {
            Object resultObject = null;
            try //trying to get the document, if an exception occurs there is not such document
            {
                resultObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
            }
            catch (Exception e) { /*ignored*/ }
            BsonDocument result = resultObject.ToBsonDocument();
            try
            {
                result.Set("_id", result.GetElement("_id").Value.ToString());
                try{
               result.Set("CreatedTimeStamp", result.GetElement("CreatedTimeStamp").Value.ToString());
                  }
                    catch (Exception ex)
                    {

                    } 
            }
            catch (Exception e)
            {
                return null;
            }
            if (result != null)
                return result.ToJson();
            else return null;
        }
        /// <summary>
        ///     Gets the customer array realated to a user
        /// </summary>
        /// <returns></returns>
        public string getCustomersArray(string idUser)
        {
            BsonJavaScript map = "";
            BsonJavaScript reduce = "";


            collection.MapReduce(map,reduce);

            return "";
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
        public List<BsonDocument> getRows()
        {
            //).SetSortOrder(SortBy.Ascending("user")
            var cursor = collection.FindAllAs(typeof(BsonDocument)); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //setting each document in an array
            {
                if (isValidDocument(document)) //is this document valid?
                {
                    try
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                        try { 
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    catch (Exception e) { }
                    documents.Add(document); //if it's valid add it to the list
                }
            }

            return documents;
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

            if (id != null) //is this an update?
            {
                doc.Set("_id", new ObjectId(id)); //setting the user's id to the document
              
                doc.Set("CreatedDate", this.getRow(id).GetElement("CreatedDate").Value ); //the createdDate must not be modified
            }
            else //is a new user?
            {
                doc.Set("CreatedTimeStamp",Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")));
                doc.Set("CreatedDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
       
                  //setting the createdDate
            }
            doc.Set("LastmodDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //either it is a new user or an update we set the lastmodDate to this Day
            collection.Save(doc);
            return doc["_id"].ToString();
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
        }
            catch (Exception e) { /*Ignored*/ }
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
            var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending("user")); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //putting the docuemnts into a list
            {
                if(isValidDocument(document)) //firts we have to check if this document has a valid structure
                    try
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                    }
                    catch (Exception e) { }
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
            return true;    
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
                ProfileTable profileTable = new ProfileTable();
                if (profileTable.getRow(document.GetElement("profileId").Value.ToString()) == null) //the profileID gived exists?
                return false;
            return true;
            }
            catch (Exception e) {
                return false;
            }
        }

        /// <summary>
        ///     Saves the relation between User and Customers
        /// </summary>
        /// <param name="customerList">
        ///     A string array of customers id. e.g.(["1","2"])
        /// </param>
        /// <param name="id_user"></param>
        public void saveRelation(string customerList, string id_user)
        {
            customerList = customerList.Replace("\"", "'");
            try
            {
                //Creating the Mogo function to call
                string evalFunction = "UpdateUserRelation('"+id_user+"', "+customerList+")";
                BsonJavaScript UpdateFuncion = new BsonJavaScript(evalFunction);

                //Calling the mongo function created
                string result = conection.getDataBase().Eval(UpdateFuncion).ToString();
            }
            catch(Exception e){/**/}
        }


        public void removeRelation(string id_user, string id_customer)
        {
            try
            {
                var query = Query.EQ("_id", new ObjectId(id_user));

                collection.Update(query, Update.Pull("customers", id_customer));
            }
            catch(Exception e){
            }
        }
    }
}