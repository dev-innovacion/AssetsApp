using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Monitor.Models
{
    public class DiagramTable : MongoModel 
    {
        public DiagramTable() : base("ProcessesDiagram")
        {

        }
    }
}