using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db;
using Rivka.Db.MongoDb;
namespace RivkaAreas.Tags.Models
{
    public class UserProfileTable : MongoModel
    {
        public UserProfileTable()
            : base("Profiles")
        { 
        
        }
    }

}