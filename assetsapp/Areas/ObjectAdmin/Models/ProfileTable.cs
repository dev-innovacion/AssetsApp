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

namespace RivkaAreas.ObjectAdmin.Models
{
    public class ProfileTable: MongoModel
    {
        
        public ProfileTable(string table = "MovementProfiles"):base(table) {
        }

        public List<BsonDocument> getRows() {
            var cursor = collection.FindAllAs( typeof(BsonDocument) ).SetSortOrder( SortBy.Ascending("name"));
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) {
                documents.Add(document);
            }
            return documents;
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
        public void removeField(String fieldID)
        {
            String currentDocumentID;
            //List<BsonDocument> documentsList = get("customFields.fields.fieldID", fieldID); //each profile that has the field
            var query = Query.And(Query.EQ("customFields.fields.fieldID", fieldID));
            var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending("name"));
            List<BsonDocument> documentsList = new List<BsonDocument>();
            foreach (BsonDocument document in cursor)
            {
                document.Set("_id", document.GetElement("_id").Value.ToString());
                documentsList.Add(document);
            }
            
            foreach (BsonDocument document in documentsList)
            {
                BsonArray tabsObject = (BsonArray)document.GetElement("customFields").Value;
                foreach (BsonDocument tab in tabsObject)
                {
                    BsonArray fields = (BsonArray)tab.GetElement("fields").Value;
                    foreach (BsonDocument field in fields)
                    {
                        if (field["fieldID"] == fieldID)
                        {
                            fields.Remove(field); //we found it!!!!
                            break;
                        }
                    }
                }
                currentDocumentID = document.GetElement("_id").Value.ToString();
                document.Remove("_id");
                SaveRow(document.ToJson(), currentDocumentID);
            }
        }     

        /*public List<BsonDocument> get(string key, string value)
        {
            var query = Query.And(Query.EQ(key, value));
            var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending("name"));
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor)
            {
                documents.Add(document);
            }

            return documents;
        }*/

        public BsonDocument getRow(String objectId) {
            Object result = collection.FindOneByIdAs( typeof(BsonDocument), new BsonObjectId( objectId) );
            return result.ToBsonDocument();
        }

    }
}