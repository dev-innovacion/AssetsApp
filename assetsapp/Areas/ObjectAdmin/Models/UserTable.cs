using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
 
using MongoDB.Driver.Builders;
namespace RivkaAreas.ObjectAdmin.Models
{
    public class UserTable : MongoModel
    {
        public UserTable() : base("Users") 
        { 
        }

        public BsonDocument Login(string nombre, string password)
        {
            var query = Query.And(
                Query.EQ("user", nombre),
                Query.EQ("pwd", password));
            object cursor = collection.FindOneAs(typeof(BsonDocument), query);

            BsonDocument doc = cursor.ToBsonDocument();

            if (doc != null)
            {
                return doc;
            }
            else
            {
                query = Query.And(
                    Query.EQ("email", nombre),
                    Query.EQ("pwd", password));
                cursor = collection.FindOneAs(typeof(BsonDocument), query);

                doc = cursor.ToBsonDocument();

                if (doc != null)
                {
                    return doc;
                }

                return null;

            }
        }

        public void UpdateObjects(String iduser, String idobj)
        {
            try
            {
                //Creating the Mogo function to call
                string evalFunction = "db.Users.update(" +
                    "{ _id: ObjectId('" + iduser + "') }," +
                    "{ $addToSet: { objects: '" + idobj + "' } }," +
                    "{ multi: true}" +
                    ");";

                BsonJavaScript UpdateFuncion = new BsonJavaScript(evalFunction);

                //Calling the mongo function created
                string result = conection.getDataBase().Eval(UpdateFuncion).ToString();
            }
            catch (Exception e) {/**/}
        }
        public String getUsersByProfile(List<string> profileid)
        {
            BsonArray idprofiles = new BsonArray();
            foreach (string idp in profileid)
            {
                idprofiles.Add(idp);
            }

           IMongoQuery query = Query.And(Query.In("profileId", idprofiles), Query.NE("system_status", false));
            JoinCollections Join = new JoinCollections();
            Join.Select("Users")
                .Join("Profiles", "profileId", "_id", "name=>nameprofile");
              
            return Join.Find(query);
        }
        public string GetUserByText(string texto)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            //Query needed to get the result
            if (texto != "")
            {
                List<string> texts = new List<string>();
                try
                {
                    texts = texto.Split(' ').ToList();
                }
                catch { }
                BsonRegularExpression match = new BsonRegularExpression("/" + texto + "/i");
                listqueries.Add(Query.Matches("name", match));
                if (texts.Count > 0)
                {
                    foreach (string text in texts)
                    {
                        BsonRegularExpression match1 = new BsonRegularExpression("/" + text + "/i");
                        listqueries.Add(Query.Matches("name", match1));
                        listqueries.Add(Query.Matches("user", match1));
                        listqueries.Add(Query.Matches("last", match1));
                    }

                }



            }
            query = Query.Or(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("Users")
                .Join("Profiles", "profileId", "_id", "name=>profileId");



            return Join.Find(query);
        }
    }

}