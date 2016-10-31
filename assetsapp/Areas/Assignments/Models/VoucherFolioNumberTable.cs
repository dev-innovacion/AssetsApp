using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Rivka.Db.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RivkaAreas.Assignments.Models
{
    public class VoucherFolioNumberTable : MongoModel
    {
        static public List<String> staticFields = new List<String>() { "employee", "folionumber" };
        /// <summary>
        ///     Intilializes the model, sets the collection to users
        /// </summary>
        public VoucherFolioNumberTable()
            : base("VoucherFolioNumber")
        {
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
        /// 

        public String saveRow(String jsonString)
        {
            BsonDocument doc;


            try //trying to parse the jsonString into a bsondocument
            {
                doc = BsonDocument.Parse(jsonString);
                doc.Set("CreatedTimeStamp", Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")));
                doc.Set("CreatedDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            }
            catch (Exception e)
            { //if it's not a valid jsonString, the user can't be saved
                return null;
            }

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
        public BsonDocument get(string key, string value)
        {
            var query = Query.And(Query.EQ(key, value)); //creating the query
            var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending("employee")); //getting the collection's cursor
            BsonDocument documents = new BsonDocument();
            foreach (BsonDocument document in cursor) //putting the docuemnts into a list
            {
                    try
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                    }
                    catch (Exception e) { }
                documents.Add(document);
            }

            return documents;
        }

        // Get last Row
        public BsonDocument getLastRow()
        {
            var sortBy = SortBy.Descending("CreatedDate");
            var cursor = collection.FindAllAs(typeof(BsonDocument)).SetSortOrder(sortBy).SetLimit(1);;
            
           

           // return collectionLog.FindAs<BsonDocument>(query).SetSortOrder(sortby).SetLimit(1);

           //var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(sortBy).SetLimit(1); //getting the collection's cursor

            //var query = Query.And(Query.EQ(key, value)); //creating the query
            //var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Descending("folionumber")); //getting the collection's cursor
            BsonDocument documents = new BsonDocument();
            foreach (BsonDocument document in cursor) //putting the docuemnts into a list
            {
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
            var cursor = collection.FindAllAs(typeof(BsonDocument)).SetSortOrder(SortBy.Descending("folionumber")); //getting the collection's cursor
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) //setting each document in an array
            {
                try
                {
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                                   }
                catch (Exception e) { }
                documents.Add(document); //if it's valid add it to the list
            }

            return documents;
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
        public BsonDocument getRowsSingle()
        {
            //).SetSortOrder(SortBy.Ascending("user")
            var cursor = collection.FindAllAs(typeof(BsonDocument)).SetSortOrder(SortBy.Descending("folionumber")); //getting the collection's cursor
           BsonDocument documents = new BsonDocument();
            foreach (BsonDocument document in cursor) //setting each document in an array
            {
                try
                {
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                }
                catch (Exception e) { }
                documents.Add(document); //if it's valid add it to the list
            }

            return documents;
        }
    }
}