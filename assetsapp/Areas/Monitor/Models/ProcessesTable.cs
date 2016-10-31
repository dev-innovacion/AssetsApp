using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Rivka.Db.MongoDb;
using Rivka.Db;


namespace RivkaAreas.Monitor.Models
{
    public class ProcessesTable : MongoModel
    {
        /// <summary>
        ///     Initializes the model
        /// </summary>
        /// <param name="collectionname">
        ///     the mongo's collections where the data is
        /// </param>
        public ProcessesTable() : base("Processes") { }

    }
}