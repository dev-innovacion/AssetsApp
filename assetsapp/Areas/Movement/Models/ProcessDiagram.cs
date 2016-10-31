using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Rivka.Db;
using Rivka.Db.MongoDb;
using MongoDB.Driver.Builders;

namespace RivkaAreas.Movement.Models
{
    public class ProcessDiagram : MongoModel
    {
        public ProcessDiagram() : base("ProcessesDiagram"){}

        /// <summary>
        ///     Checks if the process is set in some diagram
        /// </summary>
        /// <param name="idProcess"></param>
        /// <returns></returns>
        public bool IsInDiagram(string idProcess)
        {
            IMongoQuery myQuery = Query.Or(Query.EQ("connections.shape.source", idProcess), Query.EQ("connections.shape.target", idProcess));
            long isInDiagram = collection.FindAs(typeof(BsonDocument), myQuery).Count();

            if (isInDiagram > 0)
                return true;
            else
                return false;
        }

    }
}