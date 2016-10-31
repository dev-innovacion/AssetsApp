using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;
using MongoDB.Driver.Builders;
using Newtonsoft.Json.Linq;
using MongoDB.Driver;

namespace RivkaAreas.ObjectAdmin.Models
{
    public class AdjudicatingTable : MongoModel
    {
        public AdjudicatingTable()
            : base("Adjudicating") 
        { 
        }

        public string GetDictaminadorTable(JArray lista)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            List<IMongoQuery> listqueries2 = new List<IMongoQuery>();

            //Query needed to get the result
            foreach(JObject elem in lista){
                listqueries.Add(Query.EQ("location.value", elem["location"].ToString()));
            //    listqueries.Add(Query.EQ("type.value", elem["assetType"].ToString()));
                listqueries2.Add(Query.And(listqueries));
            }
            
            query = Query.Or(listqueries2);

            JoinCollections Join = new JoinCollections();
            Join.Select("Adjudicating")
                .Join("Users", "Creator", "_id", "name=>Creator");

            return Join.Find(query);
        }
    }
}