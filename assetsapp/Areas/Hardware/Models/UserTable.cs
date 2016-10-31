using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace RivkaAreas.Hardware.Models
{
    public class UserTable : MongoModel
    {
        public UserTable() : base("Users") 
        { 
        }

    }

}