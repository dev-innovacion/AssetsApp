using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Monitor.Models
{
    public class ObjectRealMonitorTable : MongoModel
    {
        public ObjectRealMonitorTable()
            : base("ObjectRealMonitor")
        { 
        }
    }
}