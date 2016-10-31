using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Processes.Models
{
    public class HardwareReferenceTable : MongoModel
    {
        public HardwareReferenceTable()
            : base("HardwareReference")
        { 
        
        }
    }
}