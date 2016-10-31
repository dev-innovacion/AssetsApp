using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Drawing;
using Rivka.Db.MongoDb;
using Newtonsoft.Json;
using Rivka.Db;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
namespace RivkaAreas.LogBook.Models
{
    public class LogBook : MongoModel
    {
        //staticFieds contains the required fields, each document must have this values

        public LogBook()
            : base("LogBook")
        {

        }

    }
}