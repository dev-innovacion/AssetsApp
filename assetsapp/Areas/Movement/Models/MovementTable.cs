using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Movement.Models
{
    public class MovementTable: MongoModel
    {
        public MovementTable():base("MovementProfiles")
        {
        }
        /// <summary>
        /// Changes the Customer´s profileId
        /// </summary>
        /// <param name="customer">
        /// Receive an User object with the _id and the new profileId
        /// </param>
        public void updateProfile(dynamic customer)
        {
            var query = Query.And(
                Query.EQ("_id", new ObjectId(customer._id))
            );
            var update = Update.Set("profileId", customer.profileId);
            collection.Update(query, update);
        }
    }
}