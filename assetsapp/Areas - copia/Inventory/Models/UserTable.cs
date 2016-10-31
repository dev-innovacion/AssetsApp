using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using System.Text.RegularExpressions;
namespace RivkaAreas.Inventory.Models
{
    public class UserTable : MongoModel
    {
        public UserTable() : base("Users") 
        { 
        }
        public string getByProfile(string profile)
        {
            //Query needed to get the result
          var  query= Query.Matches("name", BsonRegularExpression.Create(new Regex(profile)));

            JoinCollections Join = new JoinCollections();
            Join.Select("Profiles")
                .Join("Users", "Creator", "_id", "name=>Creator");
                


            return Join.Find(query);
        }
    }

}