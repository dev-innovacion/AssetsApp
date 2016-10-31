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

namespace RivkaAreas.Tags.Models
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

    }
}