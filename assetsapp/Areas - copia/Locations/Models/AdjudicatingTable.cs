using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Locations.Models
{
    public class AdjudicatingTable: MongoModel
    {
        public AdjudicatingTable()
            : base("Adjudicating") 
        { 
        }
    }
}