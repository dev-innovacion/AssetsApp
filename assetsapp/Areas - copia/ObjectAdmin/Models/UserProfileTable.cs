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
    public class UserProfileTable : MongoModel
    {
     

        public UserProfileTable():base("Profiles") {

        }

        public List<BsonDocument> getRows() {
            var cursor = collection.FindAllAs( typeof(BsonDocument) ).SetSortOrder( SortBy.Ascending("name"));
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor) {
                documents.Add(document);
            }
            return documents;
        }

        public BsonDocument getRow(String objectId) {
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
        ///     This method allows to remove a field from every profile that contains it
        /// </summary>
        /// <param name="fieldID">
        ///     It's the field's id which we are going to search for
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        public void removeField(String fieldID) {
            String currentDocumentID;
            List<BsonDocument> documentsList = get("customFields.fields.fieldID", fieldID); //each profile that has the field
            foreach( BsonDocument document in documentsList){
                BsonArray tabsObject = (BsonArray)document.GetElement("customFields").Value;
                foreach (BsonDocument tab in tabsObject) {
                    BsonArray fields = (BsonArray)tab.GetElement("fields").Value;
                    foreach ( BsonDocument field in fields ) {
                        if (field["fieldID"] == fieldID ) {
                            fields.Remove( field ); //we found it!!!!
                            break;
                        }
                    }
                }
                currentDocumentID =  document.GetElement("_id").Value.ToString();
                document.Remove("_id");
                saveRow( document.ToJson(), currentDocumentID );
            }
        }

        public void deleteProfile(String idProfile)
        {
            var query = Query.And(Query.EQ("_id", new ObjectId(idProfile)));
            try
            {
                collection.Remove(query);
            }
            catch (Exception e) { /*ignored*/}
        }

        public List<BsonDocument> get(string key, string value)
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
        public String getProfiles(List<string> namesprofiles)
        {
            BsonArray names = new BsonArray();
            foreach (string num in namesprofiles)
            {
                names.Add(num);
            }

            IMongoQuery query = Query.And(Query.In("name", names), Query.NE("system_status", false));
            JoinCollections Join = new JoinCollections();
            Join.Select("Profiles")
               .Join("Users", "Creator", "_id", "name=>username");
               
            return Join.Find(query);
        }
        public String getId(String objectId)
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
            catch (Exception e)
            {
                return null;
            }
            if (result != null)
                return result.ToJson();
            else return null;
        }
        public String saveRow(String jsonString, string id = null)
        {
            BsonDocument doc = BsonDocument.Parse(jsonString);
            if (id != null)
            {
                doc.Set("_id", new ObjectId(id));
                try
                {
                    doc.Set("CreatedDate", this.getRow(id).GetElement("CreatedDate").Value);
                }
                catch(Exception e){ /*Ignored*/ }
                
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