using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Rivka.Db.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RivkaAreas.Employee.Models
{ 
    public class EmployeeTable : MongoModel
    {
          //staticFieds contains the required fields, each document must have this values
      static public List<String> staticFields = new List<String>() {"name","lastname",  "lastname", "employee", "type", "profileid", "imgext" };

        /// <summary>
        ///     Intilializes the model, sets the collection to users
        /// </summary>
        public EmployeeTable()
            : base("Employee")
        {
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
                        try
                        {
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
            }
            catch (Exception e)
            { //if it's not a valid jsonString, the user can't be saved
                return null;
            }

            //if (!isValidDocument(doc)) //is the document valid?
            //    return null;

            if (id != null) //is this an update?
            {
               doc.Set("_id", new ObjectId(id)); //setting the user's id to the document

               doc.Set("CreatedDate", this.getRow(id).GetElement("CreatedDate").Value); //the createdDate must not be modified
            }
            else //is a new user?
            {
                doc.Set("CreatedTimeStamp", Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")));
                doc.Set("CreatedDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                //setting the createdDate
            }
            doc.Set("LastmodDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //either it is a new user or an update we set the lastmodDate to this Day
            collection.Save(doc);
            return doc["_id"].ToString();
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
           
            return result;
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
            var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending("employee")); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //putting the docuemnts into a list
            {
                if (isValidDocument(document)) //firts we have to check if this document has a valid structure
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
            catch (Exception e)
            {
                return false;
            }
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
        /// Changes the User´s profileId
        /// </summary>
        /// <param name="user">
        /// Receive an User object with the _id and the new profileId
        /// </param>
        public void updateProfile(dynamic employee)
        {
            var query = Query.And(
                Query.EQ("_id", new ObjectId(employee._id))
            );
            var update = Update.Set("profileId", employee.profileId);
            collection.Update(query, update);
        }

    }
}