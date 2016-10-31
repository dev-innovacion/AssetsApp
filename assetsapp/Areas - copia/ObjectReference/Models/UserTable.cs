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

namespace RivkaAreas.ObjectReference.Models
{

    /// <summary>
    ///     This class abstracts the user's collection
    /// </summary>
    public class UserTable : MongoModel
    {

        private MongoCollection collection;
        private MongoConection conection;

        //staticFieds contains the required fields, each document must have this values
        static public List<String> staticFields = new List<String>() { "user", "pwd", "imgext", "email", "name", "lastname", "profileId", "profileFields" };

        /// <summary>
        ///     Intilializes the model, sets the collection to users
        /// </summary>
        public UserTable()
            : base("Users")
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
            }
            catch (Exception e) { /*ignored*/ }
            BsonDocument result = resultObject.ToBsonDocument();
            try
            {
                result.Set("_id", result.GetElement("_id").Value.ToString());
            }catch(Exception e){
                return null;
            }
            if (result != null)
                return result.ToJson();
            else return null;
        }
    }
}