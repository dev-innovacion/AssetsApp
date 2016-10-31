using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.ObjectAdmin.Models
{
    public class ObjectFieldsTable : MongoModel
    {
        public ObjectFieldsTable()
            : base("ObjectFields")
        {

        }
    }
}