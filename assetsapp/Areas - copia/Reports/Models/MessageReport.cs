using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Drawing;
using System.Globalization;
using Rivka.Db;
using Rivka.Db.MongoDb;
using System.Web;

namespace RivkaAreas.Reports.Models
{
    public class MessageReport : MongoModel
    {
        private MongoCollection collection;
        private MongoConection conection;
        public  MessageReport(string table):base("Messages") {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
        }
       
    }
}
