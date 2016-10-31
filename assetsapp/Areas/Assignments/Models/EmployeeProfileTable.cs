using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Assignments.Models
{
    public class EmployeeProfileTable : MongoModel
    {
        private MongoCollection collection;
        private MongoConection conection;

        public EmployeeProfileTable(string table = "EmployeeProfiles")
            : base("EmployeeProfiles")
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
        }

       
        public BsonDocument getRow(String objectId)
        {
            Object result;
            try
            {
                result = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
            }
            catch (Exception e)
            {
                return null;
            }
            return result.ToBsonDocument();
        }

      

    }
}