using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Rivka.Db.MongoDb;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Rivka.Error;
using MongoDB.Bson;

namespace RivkaAreas.Reports.Models
{
    public class NotificationReport : MongoModel
    {
        public NotificationReport() :
            base("Notifications") { }


        public string GetNotifications( string filter )
        {
            JArray modules = new JArray();

            try
            {
                modules = JsonConvert.DeserializeObject<JArray>(filter);
            }
            catch(Exception e)
            {
                Error.Log(e, "Trying to get filter modules");
            }

            List<IMongoQuery> queries = new List<IMongoQuery>();

            foreach (JObject module in modules)
            {
                BsonArray values = new BsonArray();

                foreach(string v in module["v"])
                {
                    values.Add(v);
                }

                //TODO: fix this from controller or view...
                string mod = module["mod"].ToString().Split(new char[] { '_' })[0];

                IMongoQuery moduleQuery = Query.And(Query.EQ("module", mod), Query.In("type", values));
                queries.Add(moduleQuery);
            }

            IMongoQuery mainQuery = Query.Or(queries);

            //Get All matched rows, descending and just 10 rows
            MongoCursor cursor = collection.FindAs(typeof(BsonDocument), mainQuery).SetSortOrder(SortBy.Descending("CreatedTimeStamp")).SetLimit(10);

            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor)
            {
                document.Set("_id", document.GetElement("_id").Value.ToString());
                try
                {
                    document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                }
                catch (Exception ex)
                {
                    Error.Log(ex, "Creating time stamp");
                }
                documents.Add(document);
            }
            return documents.ToJson();


        }

    }
}
