using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Processes.Models
{
    public class RulesTable : MongoModel 
    {
        public RulesTable() : base("ProcessRules")
        {
        }
    }
}