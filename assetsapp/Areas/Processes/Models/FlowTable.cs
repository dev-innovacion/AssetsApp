using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rivka.Db.MongoDb;
using MongoDB.Bson;

namespace RivkaAreas.Processes.Models
{
    public class FlowTable : MongoModel 
    {
        public FlowTable()
            : base("FlowRules")
        {
            
        }

        public void UpdateTargets(String idFlow, String idtarget)
        {
            try
            {
                //Creating the Mogo function to call
                string evalFunction = "db.FlowRules.update(" +
                    "{ _id: ObjectId('" + idFlow + "') }," +
                    "{ $addToSet: { targets: '" + idtarget + "' } }," +
                    "{ multi: true}" +
                    ");";

                BsonJavaScript UpdateFuncion = new BsonJavaScript(evalFunction);

                //Calling the mongo function created
                string result = conection.getDataBase().Eval(UpdateFuncion).ToString();
            }
            catch (Exception e) {/**/}
        }
    }
}