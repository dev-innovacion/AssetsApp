using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MongoDB.Driver.Builders;
using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Hardware.Models
{
    public class HardwareReferenceTable : MongoModel
    {
        public HardwareReferenceTable()
            : base("HardwareReference")
        { 
        }
    }
}