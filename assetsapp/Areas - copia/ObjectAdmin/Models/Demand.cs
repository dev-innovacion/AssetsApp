using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db.MongoDb;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

namespace RivkaAreas.ObjectAdmin.Models
{
    public class Demand : MongoModel
    {
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonProperty("folio")]
        public string folio { get; set; }

        [JsonProperty("movement")]
        public string movement { get; set; }

        [JsonProperty("objectreal")]
        public string objectreal { get; set; }

        [JsonProperty("total")]
        public string total { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }


        //staticFieds contains the required fields, each document must have this values
        static public List<String> staticFields = new List<String>() {"_id", "folio", "movement", "object","total","status" };

        public Demand(string table = "Demand")
            : base(table)
        {
        }

        /// <summary>
        ///     Gets the join between many collections
        /// </summary>
        /// <returns> An array with the resulting search </returns>
        /// 
        public String GetValidObjects(List<string> idobjs = null)
        {


            var orderfield = "folio";


            try
            {


                BsonArray bsonarrayids = new BsonArray();

                foreach (string item in idobjs)
                {
                    bsonarrayids.Add(item);
                }

                JoinCollections Join = new JoinCollections();
                Join.Select("Demand")
                .Join("Locations", "objects.location", "_id", "name=>nameLocation")
                .Join("Users", "Creator", "_id", "name=>nameUser")
                .Join("MovementProfiles", "movement", "_id", "name=>nameMovement");

                IMongoQuery query = null;
                if (idobjs != null && idobjs.Count > 0)
                {
                    query = Query.And(Query.In("objects.id", bsonarrayids), Query.LT("status", 6));

                }
                else
                {
                    return null;
                }

                return Join.Find(query);
            }
            catch (Exception e)
            {
                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }
        public string GetDemandTableByFolio(string folio)
        {
            if (folio != null)
            {
                IMongoQuery query;
                query = Query.And(Query.EQ("folio", folio));
                JoinCollections Join = new JoinCollections();
                Join.Select("Demand")
                    .Join("MovementProfiles", "movement", "_id", " name=>typeMovement, typeMovement=>typeStatus")
                    .Join("Locations", "location", "_id", "name => location")
                    .Join("ReferenceObjects", "object", "_id", "name=>object");
                   

                return Join.Find(query);
            }
            return null;
        }

        public string GetProfileTemporalDemand()
        {
            
                IMongoQuery query;
                query = Query.And(Query.EQ("temporal", true));
                JoinCollections Join = new JoinCollections();
                Join.Select("MovementProfiles");
              //  .Join("Demand", "_id", "movement", "_id=>demandid ");
                return Join.Find(query);
            
            return null;
        }


        public string GetDemandTable(string varMovement, string varStatus, string Creator = null,bool admin=false)
        {
            Dictionary<string, BsonArray> statusArray = new Dictionary<string, BsonArray>();
            statusArray.Add("1", new BsonArray(new int[] { 1 }));
            statusArray.Add("2", new BsonArray(new int[] { 2 }));
            statusArray.Add("3", new BsonArray(new int[] { 1, 2, 3, 4, 5 }));
            statusArray.Add("4", new BsonArray(new int[] { 4 }));
            statusArray.Add("5", new BsonArray(new int[] { 5 }));
            statusArray.Add("6", new BsonArray(new int[] { 6 }));
            statusArray.Add("7", new BsonArray(new int[] { 7 }));
            statusArray.Add("8", new BsonArray(new int[] { 8 }));
            statusArray.Add("9", new BsonArray(new int[] { 9 }));

            //Query needed to get the result
            
            IMongoQuery mainQuery = null;
            IMongoQuery singleQuery = null;

            IMongoQuery query;

            if (varMovement == "null")
            {
                if (varStatus == null || varStatus == "null")
                {
                    query = Query.And(Query.EQ("status", varStatus)); //normal query when varStatus is equal to null
                }
                else
                {
                    query = Query.And(Query.In("status", statusArray[varStatus])); //
                }
            }
            else if (varStatus == "null")
            {
                query = Query.And(Query.EQ("movement", varMovement));
            }
            else
            {
                try
                {
                    query = Query.And(Query.EQ("movement", varMovement), Query.In("status", statusArray[varStatus]));
                }
                catch(Exception e)
                {
                    query = Query.And(Query.EQ("movement", varMovement), Query.EQ("status", varStatus));
                }
            }

            if (Creator != null && admin==false)
            {
                singleQuery = Query.EQ("Creator", Creator);
                mainQuery = Query.And(singleQuery, query);
            }
            else
            {
                mainQuery = query;
            }

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "temporal,name=>movement , name=>typeMovement, typeMovement=>typeStatus")
                .Join("Locations", "location", "_id", "name => location")
                .Join("ReferenceObjects", "object", "_id", "name=>object")
                .Join("Users", "Creator", "_id", "name=>Creator");

            if(varMovement=="null" && varStatus=="null")
                return Join.Find(singleQuery);

            return Join.Find(mainQuery);
        }

        public string GetAutorizations2(string varMovement, string varStatus, String key, String value1 = "")
        {
            Dictionary<string, BsonArray> statusArray = new Dictionary<string, BsonArray>();
            statusArray.Add("1", new BsonArray(new int[] {1}));
            statusArray.Add("2", new BsonArray(new int[] { 2 }));
            statusArray.Add("3", new BsonArray(new int[] { 1, 2, 3, 4, 5 }));
            statusArray.Add("4", new BsonArray(new int[] { 4 }));
            statusArray.Add("5", new BsonArray(new int[] { 5 }));
            statusArray.Add("6", new BsonArray(new int[] { 6 }));
            statusArray.Add("7", new BsonArray(new int[] { 7 }));
            statusArray.Add("8", new BsonArray(new int[] { 8 }));
            statusArray.Add("9", new BsonArray(new int[] { 9 }));

            //Query needed to get the result
            IMongoQuery mainQuery = null;
            IMongoQuery singleQuery = null;

            IMongoQuery query;

            if (varMovement == "null")
            {
                if (varStatus == null || varStatus == "null")
                {
                    query = Query.And(Query.EQ("status", varStatus)); //normal query when varStatus is equal to null
                }
                else
                {
                    query = Query.And(Query.In("status", statusArray[varStatus])); //
                }
            }
            else if (varStatus == "null")
            {
                query = Query.And(Query.EQ("movement", varMovement));
            }
            else
            {
                try
                {
                    query = Query.And(Query.EQ("movement", varMovement), Query.In("status", statusArray[varStatus]));
                }
                catch (Exception e)
                {
                    query = Query.And(Query.EQ("movement", varMovement), Query.EQ("status", varStatus));
                }
            }
            if (value1 != "")
            {
                singleQuery = Query.EQ(key, value1);
                mainQuery = Query.And(singleQuery, query);
            }
            else
            {
                mainQuery = query;
            }

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "temporal,name=>namemovement, typeMovement=>typeStatus")
                .Join("Users", "Creator", "_id", "name=>CreatorName, lastname => CreatorLastName");

            if (varMovement == "null" && varStatus == "null")
                return Join.Find(singleQuery);

            return Join.Find(mainQuery);
        }

        /// <summary>
        ///     Get the authorizations related to a user
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetAutorizations(String key, String value1="", int value2=0)
        {
            IMongoQuery query=null ;
            if(value1!=""){
                query=Query.EQ(key, value1);
            }
            if (value2 != 0) {
                query = Query.EQ(key, value2);
            }
            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "temporal,name=>namemovement, typeMovement=>typeStatus")
                .Join("Users", "Creator", "_id", "name=>CreatorName, lastname => CreatorLastName");

            return Join.Find(query);
        }

        public string GetdemandsInProfiles(List<String> profiles)
        {
            BsonArray bsonarraymovs = new BsonArray();

            foreach (string item in profiles)
            {
                bsonarraymovs.Add(item);
            }
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            IMongoQuery query = null;
            listqueries.Add(Query.In("movement", bsonarraymovs));

            query = Query.And(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "temporal,name=>namemovement, typeMovement=>typeStatus")
                .Join("Users", "Creator", "_id", "name=>CreatorName, lastname => CreatorLastName");

            return Join.Find(query);
        }

        public string GetAutorizations3(List<String> subusers, List<String> movs)
        {
            BsonArray bsonarrayids = new BsonArray();
            BsonArray bsonarraymovs = new BsonArray();
            foreach (string item in subusers)
            {
                bsonarrayids.Add(item);
            }
            foreach (string item in movs)
            {
                bsonarraymovs.Add(item);
            }
            List<IMongoQuery> listqueries=new List<IMongoQuery>();
            IMongoQuery query = null;
            listqueries.Add(Query.In("Creator", bsonarrayids));
           // listqueries.Add(Query.EQ("status", 6));
            listqueries.Add(Query.In("movement", bsonarraymovs));

            query = Query.And(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "temporal,name=>namemovement, typeMovement=>typeStatus")
                .Join("Users", "Creator", "_id", "name=>CreatorName, lastname => CreatorLastName");

            return Join.Find(query);
        }

        public string GetWitnesses()
        {
            
            JoinCollections Join = new JoinCollections();
            Join.Select("witnesses")
                .Join("Users", "user", "_id", "name=>CreatorName, lastname => CreatorLastName");

            return Join.Find();
        }

        public String UpdateRowStatus(String field, int data, String id)
        {
            BsonDocument updatedData = new BsonDocument();
            try
            {
                id = id.Trim();
                UpdateBuilder update = new UpdateBuilder();
                try
                {
                    //updatedData = BsonDocument.Parse(data);
                    update = Update.Set(field, data);
                }
                catch (Exception e)
                {
                    
                }
                if (id != null && id != "")
                {
                    var query = Query.EQ("_id", new ObjectId(id));
                    var lastMod = Update.Set(
                        "LastmodDate",
                        DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                    );
                    //Update data
                    collection.Update(query, update);
                    collection.Update(query, lastMod);
                }
            }
            catch (Exception e)
            {
               // Error.Error.Log(e, "Updating data");
            }
            return "";
        }

    }
}