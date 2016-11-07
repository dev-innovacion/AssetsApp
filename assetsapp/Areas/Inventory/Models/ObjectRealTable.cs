using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Inventory.Models
{
    public class ObjectRealTable : MongoModel
    {
        public ObjectRealTable() : base ("ObjectReal")
        { 
        }
    }
}