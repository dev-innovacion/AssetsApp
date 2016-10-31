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

namespace RivkaAreas.List.Models
{

    [Authorize]
    public class ListTable
    {
        private MongoCollection collection;
        private MongoConection conection;

        /// <summary>
        ///     Initializes the model
        /// </summary>
        /// <param name="collectionname">
        ///     the mongo's collections where the data is
        /// </param>
        public ListTable()
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection("Lists");
        }

        /// <summary>
        ///     This method returns the collection's document with the specified id
        /// </summary>
        /// <param name="id">
        ///     It's the document's id we are looking for
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the document with the specified id
        /// </returns>
        public BsonDocument getRow(String id)
        {
            try //trying to get the document
            {
                Object documentObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(id));
                BsonDocument document = documentObject.ToBsonDocument();
                document.Set("_id", document.GetElement("_id").Value.ToString());
                return document;
            }
            catch (Exception e)
            { //can not get the document
                return null;
            }
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
        public String saveRow(String jsonString, String id = null)
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

            if (!isValidDocument(doc)) //is the document valid?
                return null;

            if (id != null) //is this an update?
            {
                doc.Set("_id", new ObjectId(id)); //setting the user's id to the document
                try
                {
                    doc.Set("createdDate", this.getRow(id).GetElement("createdDate").Value); //the createdDate must not be modified
                }
                catch (Exception e) {
                    doc.Set("createdDate", DateTime.Now.ToString()); //setting the createdDate
                }
            }
            else //is a new user?
            {
                doc.Set("createdDate", DateTime.Now.ToString()); //setting the createdDate
            }
            doc.Set("lastModified", DateTime.Now.ToString()); //either it is a new user or an update we set the lastmodDate to this Day
            collection.Save(doc);
            return doc["_id"].ToString();
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
            var cursor = collection.FindAllAs(typeof(BsonDocument)).SetSortOrder(SortBy.Ascending("name")); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //setting each document in an array
            {
                if (isValidDocument(document)) //is this document valid?
                {
                    try
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                        documents.Add(document); //if it's valid add it to the list
                    }catch(Exception e){ /*Ignored*/ }
                }
                else
                {
                    try //if the document is not valid delete it, if it can be deleted
                    {
                        deleteRow(document.GetElement("_id").Value.ToString());
                    }
                    catch (Exception e) { /*Ignored*/ }
                }
            }

            return documents;
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
            var cursor = collection.FindAs(typeof(BsonDocument), query); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //putting the docuemnts into a list
            {
                if (isValidDocument(document)) //firts we have to check if this document has a valid structure
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
            //try //trying to validate the document if an exception occurs the document is not valid
            //{
            //    if (document == null) return false; //the document can not be null
            //    List<string> keys = new List<string>();
            //    foreach (BsonElement element in document) //creating an array with every key in the document
            //    {
            //        keys.Add(element.Name);
            //    }
            //    if (staticFields.Except(keys).ToList().Count() != 0) //checks if there are staticFields that are not in the document
            //        return false;
            //    ProfileTable profileTable = new ProfileTable();
            //    if (profileTable.getRow(document.GetElement("profileId").Value.ToString()) == null) //the profileID gived exists?
            //        return false;
            //    return true;
            //}
            //catch (Exception e)
            //{
                return true;
            //}
        }
    }
}