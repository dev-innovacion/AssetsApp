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

namespace RivkaAreas.ObjectReference.Models
{
    [Authorize]
    public class LocationTable : MongoModel
    {
        private MongoCollection collection;
        private MongoConection conection;

        /// <summary>
        ///     Initializes the model
        /// </summary>
        /// <param name="collectionname">
        ///     the mongo's collections where the data is
        /// </param>
        public LocationTable() : base("Locations")
        {

        }

        public string GetLocationsTable(string vartypeuser, string vartype, string varRegion, string varConjunto, List<string> locations = null)
        {
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            IMongoQuery query = null;

            BsonArray bsonarrayids = new BsonArray();
            if (locations != null)
            {
                foreach (string item in locations)
                {
                    bsonarrayids.Add(new ObjectId(item));
                }
            }


            //Query needed to get the result
            if (vartype == "")
            {
                listqueries.Add(Query.NE("profileId", varRegion));
                listqueries.Add(Query.NE("profileId", varConjunto));
                if (locations != null)
                    listqueries.Add(Query.In("_id", bsonarrayids));
            }
            else if (vartype == "Region")
            {
                listqueries.Add(Query.EQ("profileId", varRegion));
                if (locations != null)
                    listqueries.Add(Query.In("_id", bsonarrayids));

            }
            else if (vartype == "Conjunto")
            {
                listqueries.Add(Query.EQ("profileId", varConjunto));
                if (locations != null)
                    listqueries.Add(Query.In("_id", bsonarrayids));
            }

            query = Query.And(listqueries);

            JoinCollections Join = new JoinCollections();
            Join.Select("Locations")
                .Join("Users", "responsable", "_id", "name=>nameResponsable");

            return Join.Find(query);
        }
    }
}