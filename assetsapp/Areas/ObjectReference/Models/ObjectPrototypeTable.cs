using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.ObjectReference.Models
{
    public class ObjectPrototypeTable
    {
        private MongoCollection collection;
        private MongoConection conection;

        /// <summary>
        ///     Initializes the models
        /// </summary>
        public ObjectPrototypeTable()
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection("ObjectPrototype");
        }

        /// <summary>
        ///     Allows to get all the documents in the collection
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a list with all the documents in the collection 
        ///     or null if there are no documents
        /// </returns>
        public List<BsonDocument> getRows()
        {
            try
            {
                var cursor = collection.FindAllAs(typeof(BsonDocument)).SetSortOrder(SortBy.Ascending("parentCategory"));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    documents.Add(document);
                }
                return documents;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     This method allows to get a collection's document
        /// </summary>
        /// <param name="objectId">
        ///     The document's id we want to get
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the document with the specified id
        /// </returns>
        public BsonDocument getRow(String objectId)
        {
            Object result;
            try
            {
                result = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
            }
            catch (Exception e)
            {
                return null;
            }
            return result.ToBsonDocument();
        }

        /// <summary>
        ///     This method allows to get a collection's document by its parentCategory
        /// </summary>
        /// <param name="parentCategory">
        ///     The object's parentCategory id
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the document with the specified parentCategory
        /// </returns>
        public BsonDocument getRowByCategory(String parentCategory)
        {
            Object result;
            try
            {
                var query = Query.And(Query.EQ("parentCategory", parentCategory));
                result = collection.FindOneAs(typeof(BsonDocument), query);
            }
            catch (Exception e)
            {
                return null;
            }
            return result.ToBsonDocument();
        }

        /// <summary>
        ///     This method allows to remove a field from every profile that contains it
        /// </summary>
        /// <param name="fieldID">
        ///     It's the field's id which we are going to search for
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        //public void removeField(String fieldID)
        //{
        //    String currentDocumentID;
        //    List<BsonDocument> documentsList = get("customFields.fields.fieldID", fieldID); //each profile that has the field
        //    foreach (BsonDocument document in documentsList)
        //    {
        //        BsonArray tabsObject = (BsonArray)document.GetElement("customFields").Value;
        //        foreach (BsonDocument tab in tabsObject)
        //        {
        //            BsonArray fields = (BsonArray)tab.GetElement("fields").Value;
        //            foreach (BsonDocument field in fields)
        //            {
        //                if (field["fieldID"] == fieldID)
        //                {
        //                    fields.Remove(field); //we found it!!!!
        //                    break;
        //                }
        //            }
        //        }
        //        currentDocumentID = document.GetElement("_id").Value.ToString();
        //        document.Remove("_id");
        //        saveRow(document.ToJson(), currentDocumentID);
        //    }
        //}


        /// <summary>
        ///     This method allows to remove a document
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <param name="idProfile">
        ///     The document's id we want to delete
        /// </param>
        public void deleteRow(String idProfile)
        {
            var query = Query.And(Query.EQ("_id", new ObjectId(idProfile)));
            try
            {
                collection.Remove(query);
            }
            catch (Exception e) { /*ignored*/}
        }

        /// <summary>
        ///     This method allows to remove a document giving its parentCategory
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <param name="parentCategory">
        ///     The document's parentCategory id we want to delete
        /// </param>
        public void deleteRowByParent(String parentCategory)
        {
            var query = Query.And(Query.EQ("parentCategory", parentCategory));
            try
            {
                collection.Remove(query);
            }
            catch (Exception e) { /*ignored*/}
        }

        /// <summary>
        ///     Allows to get a set of documents that satisfy the condition key = value
        /// </summary>
        /// <param name="key">
        ///     The collection key to check
        /// </param>
        /// <param name="value">
        ///     The value which the documents must have
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a set of documents where key = value
        /// </returns>
        public List<BsonDocument> get(string key, string value)
        {
            try
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
            catch (Exception e)
            {
                return null;
            }
        }


        /// <summary>
        ///     Allows to create or modigy a document
        /// </summary>
        /// <param name="jsonString">
        ///     The document's json string with the document's information
        /// </param>
        /// <param name="id">
        ///     The modifying document's id
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
            try //does the jsonString have a json correct format?
            {
                doc = BsonDocument.Parse(jsonString);
            }
            catch (Exception e)
            {
                return null;
            }
            if (id != null)
            {
                doc.Set("_id", new ObjectId(id));
                try
                {
                    doc.Set("CreatedDate", this.getRow(id).GetElement("CreatedDate").Value);
                }
                catch (Exception e) { /*Ignored*/ }

            }
            else
            {
                doc.Set("CreatedDate", DateTime.Now.ToString());
            }
            doc.Set("LastmodDate", DateTime.Now.ToString());

            collection.Save(doc);
            return doc["_id"].ToString();
        }

    }
}