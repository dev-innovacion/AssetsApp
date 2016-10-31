using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.ObjectAdmin.Models
{
    public class ProcessesTable : MongoModel
    {
        /// <summary>
        ///     Initializes the mode
        /// </summary>
        /// <param name="collectionname">
        ///     the mongo's collections where the data is
        /// </param>
        public ProcessesTable() : base("Processes") { }

    }
}