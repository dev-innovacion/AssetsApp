using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.ObjectAdmin.Models
{
    [Authorize]
    public class LocationTable : MongoModel
    {
        /// <summary>
        ///     Initializes the model
        /// </summary>
        /// <param name="collectionname">
        ///     the mongo's collections where the data is
        /// </param>
        public LocationTable() : base("Locations")
        {

        }

        public string GetLocations(String profileid, String parent)
        {
            IMongoQuery query = null;
            query = Query.And(Query.EQ("profileId", profileid), Query.EQ("parent", parent));
            JoinCollections Join = new JoinCollections();
            Join.Select("Locations");

            return Join.Find(query);
        }
        public string GetLocationsByText(string texto)
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
            Join.Select("Locations")
                .Join("Users", "Creator", "_id", "name=>Creator");



            return Join.Find(query);
        }

    }
}