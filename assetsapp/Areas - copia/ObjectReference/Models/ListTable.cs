using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.ObjectReference.Models
{
    public class ListTable : MongoModel
    {
        public ListTable()
            : base("Lists") 
        { 
        }
    }
}