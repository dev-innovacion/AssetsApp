using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rivka.Db;
using Rivka.Db.MongoDb;
using Rivka.Error;

namespace RivkaAreas.Rule
{

    /// <summary>
    ///     Allows to check the rules 
    /// </summary>
    public class RulesChecker
    {

        protected static MongoModel objectsModel = new MongoModel("ObjectReal");
        protected static MongoModel RefobjectsModel = new MongoModel("ReferenceObjects");
        protected static MongoModel locationModel = new MongoModel("Locations");
        protected static MongoModel locationruleModel = new MongoModel("LocationRules");
        protected static MongoModel flowruleModel = new MongoModel("FlowRules");
        protected static MongoModel processruleModel = new MongoModel("ProcessRules");

        /// <summary>
        ///     Allows to check if an object can go to a location
        /// </summary>
        /// <param name="objectId">The object's id to check</param>
        /// <param name="locationId">The location where the object is going to be sent</param>
        /// <author>Luis Gonzalo Quijada Romero</author>
        /// <returns>A booleand that represents if the movement is allowed</returns>
        /// 
        public static Boolean isValidToLocation( String objectId, String locationId ) {
            int flag = 0;
            JObject objectRef = new JObject();
            JObject objectReal = new JObject();
            String idobj = "";
            String objectRealString = objectsModel.GetRow(objectId);
            if (objectRealString == null) {
                objectRealString = RefobjectsModel.GetRow(objectId);
                
                //An object must have a ReferenceObject 
                // * Check this Exception *
                if (objectRealString == null)
                {
                    Error.Log(new Exception(), "Object without Id_Reference found");
                    return true;
                }

                objectRef = JsonConvert.DeserializeObject<JObject>(objectRealString);
                idobj=objectId;
            }
            else
            {
                objectReal = JsonConvert.DeserializeObject<JObject>(objectRealString);
                idobj = objectReal["objectReference"].ToString();
            }

            //
            // TODO :: Validar si locationId es null
            //
            String locationString = locationModel.GetRow(locationId);
            JObject location = JsonConvert.DeserializeObject<JObject>(locationString);

            //check 
            String rulestring = locationruleModel.Get("IdLocation", location["_id"].ToString());
            if (rulestring == null) return true;
            JArray rule = JsonConvert.DeserializeObject<JArray>(rulestring);
            bool band = true;
            foreach (JObject objrule in rule) {
                JArray rls = JsonConvert.DeserializeObject<JArray>(objrule["RefObjects"].ToString());
                
                foreach (String obj in rls)
                {
                    if (obj == idobj)
                    {
                        flag = 1;
                    }
                }
                if (flag == 1)
                {
                    if (objrule["Type"].ToString() == "Permitidos")
                    {
                        band = true;
                    }
                    if (objrule["Type"].ToString() == "Denegados")
                    {
                        band = false;
                    }
                }
                else
                {

                    if (objrule["Type"].ToString() == "Permitidos")
                    {
                        band = false;
                    }
                    if (objrule["Type"].ToString() == "Denegados")
                    {
                        band = true;
                    }
                }
            }

            

            return band;
        }

        public static Boolean isValidProcessFlow(String Source, String Target)
        {
            String ruleString = flowruleModel.Get("source",Source);
            JArray rulearray = new JArray();
            bool band = true;
            if (ruleString != null)
            {
                rulearray = JsonConvert.DeserializeObject<JArray>(ruleString);
                band = false;
                foreach(JObject obj in rulearray){
                    JArray targets = JsonConvert.DeserializeObject<JArray>(obj["targets"].ToString());
                    foreach (string cad in targets)
                    {
                        if (cad == Target) band = true;
                    }
                }
            }
            else
            {
                band = true;
            }
            return band;
        }

        /// <summary>
        ///     Allows to check if an object can pass through a hardware
        /// </summary>
        /// <param name="objectId">The object's id to check</param>
        /// <param name="hardwareId">The hardware's id where the object was detected</param>
        /// <author>Luis Gonzalo Quijada Romero</author>
        /// <returns></returns>
        /// 
        public static Boolean isValidToHardware( String objectId, String hardwareId ) {

            return true;
        }
    }
}