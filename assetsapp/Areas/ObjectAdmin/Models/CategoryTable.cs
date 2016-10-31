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
    public class CategoryTable : MongoModel
    {
        public CategoryTable(): base("Categories") 
        { 
        }
        public string GetCatByText(string texto)
        {
            IMongoQuery query;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            //Query needed to get the result
            if (texto != "")
            {
                BsonRegularExpression match = new BsonRegularExpression("/" + texto + "/i");
                listqueries.Add(Query.Matches("name", match));

            }
            query = Query.Or(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("Categories")
                .Join("Users", "Creator", "_id", "name=>Creator");



            return Join.Find(query);
        }
    }

}