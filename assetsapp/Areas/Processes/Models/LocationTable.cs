using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Processes.Models
{
    public class LocationTable : MongoModel 
    {
        public LocationTable() : base("Locations")
        {

        }
    }
}