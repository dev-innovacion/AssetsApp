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
    public class CategoryTable: MongoModel
    {
        /// <summary>
        ///     Initializes the models
        /// </summary>
        public CategoryTable(): base("Categories") {
        }

        /// <summary>
        ///     Allows to get all the documents in the categories collection
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a list with all the documents in the categories collection 
        ///     or null if there are no documents
        /// </returns>
        public String getRows() {
            try
            {
                var cursor = collection.FindAllAs(typeof(BsonDocument)).SetSortOrder(SortBy.Ascending("name"));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    documents.Add(document);
                }
                return documents.ToJson();
            }
            catch (Exception e) {
                return null;
            }
        }

        /// <summary>
        ///     This method allows to get a category document
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
        public String getRow(String objectId) {
            try
            {
                BsonDocument result = null;
                Object resultAux = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
                result = resultAux.ToBsonDocument();
                result.Set("_id", result.GetValue("_id").ToString());
                return result.ToJson();
            }
            catch (Exception e) {
                return null;
            }
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
        public String get(String key, String value)
        {
            try
            {
                var query = Query.And(Query.EQ(key, value));
                var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending("name"));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    // if (isValidDocument(document))
                    document.Set( "_id", document.GetElement("_id").Value.ToString());
                    documents.Add(document);
                }

                return documents.ToJson();
            }
            catch (Exception e) {
                return null;
            }
        }


        /// <summary>
        ///     Allows to create or modigy a category
        /// </summary>
        /// <param name="jsonString">
        ///     The category json string with the document's information
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
        public String saveRow(String jsonString, String id = null)
        {
            BsonDocument doc;
            try //does the jsonString have a json correct format?
            {
                doc = BsonDocument.Parse(jsonString);
            }
            catch (Exception e) {
                return null;
            }

            if (id != null && id != "")
            {
                doc.Set("_id", new ObjectId(id));
                BsonDocument lastData = BsonDocument.Parse(this.getRow(id));
                try
                {
                    doc.Set("CreatedDate", lastData.GetElement("CreatedDate").Value);
                }
                catch(Exception e){
                    doc.Set("CreatedDate", DateTime.Now.ToString()); //if created date is not set let set it to the current date
                }

                try
                {
                    doc.Set("Creator", lastData.GetElement("Creator").Value);
                }
                catch (Exception e) {
                    doc.Set("Creator", HttpContext.Current.Session["_id"].ToString()); //if creator is not set let's set it to the current user's id
                }
            }
            else
            {
                doc.Set("CreatedDate", DateTime.Now.ToString());
                doc.Set("Creator", HttpContext.Current.Session["_id"].ToString());
            }
            doc.Set("LastmodDate", DateTime.Now.ToString());

            collection.Save(doc);
            return doc["_id"].ToString();
        }

    }
}