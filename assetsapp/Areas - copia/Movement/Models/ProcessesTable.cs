using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Movement.Models
{
    public class ProcessesTable: MongoModel
    {
        public ProcessesTable():base("Processes")
        {
        }
    }
}