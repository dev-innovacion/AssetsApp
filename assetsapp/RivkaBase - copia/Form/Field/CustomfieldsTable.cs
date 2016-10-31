using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.IO;
using System.Reflection;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

using Rivka.User;
using Rivka.Db;
using Rivka.Db.MongoDb;
using Rivka.Api;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rivka.Form.Field
{
    [Authorize]
    public class CustomFieldsTable : Rivka.Db.MongoDb.MongoModel
    {

        protected Agent agent = null; //used to send data to the HTK admin site
        public CustomFieldsTable(String collectionName) : base(collectionName){}

        /// <summary>
        ///     This method initializes the agent
        /// </summary>
        /// <author>Quijada Romero Luis Gonzalo</author>
        private void initAgent()
        {
            if (agent == null)
            {
                String path = HttpContext.Current.Server.MapPath("/App_Data/com-agent.conf");
                StreamReader objReader = new StreamReader(path);
                String sLine = objReader.ReadLine();

                JObject agentConf = JsonConvert.DeserializeObject<JObject>(sLine);
                agent = new Agent(agentConf["url"].ToString(), agentConf["user"].ToString(), agentConf["password"].ToString());
            }
        }

        /// <summary>
        /// Delete a table row by ID
        /// </summary>
        /// <param name="ID">Id of the customfield</param>
        /// <author>Galaviz Alejos Luis Angel</author>
        public void deleteRows(string ID)
        {
            var query = Query.EQ("_id", new ObjectId(ID));
            
            //CategoryTable categoryTable = new CategoryTable();
            //categoryTable.removeField(ID); //does this collection need to be updated nestly?
            var cursor = collection.Remove(query);
        }

        /// <summary>
        /// Save the row if there is no id, if the id is null, it updates the register
        /// </summary>
        /// <param name="fc">Any custom field as an object(Type "object" is used to use reflection)</param>
        /// <author>Galaviz Alejos Luis Angel</author>
        /// <modifications>
        ///     -Returns the id of the recently saved document / Gonzalo Quijada
        ///     -sends message to the agent to store a copy
        /// </modifications>
        public String saveCustomField(object fc)
        {
            String docId = null; //id before saving the document
            BsonDocument doc = new BsonDocument();
            var props = fc.GetType().GetProperties();
            foreach (var property in props)
            {
                //TODO:Ignores the list types
                if (property.GetValue(fc, null) is IList)
                {
                    continue;
                }

                //The id requires a special check, because his type is ObjectID
                if (property.Name != "_id")
                {
                    //This property is set by the Type of the customField manually
                    if (property.Name == "type")
                    {
                        //Sets the CustomFieldType manually
                        doc.Add(new BsonElement(property.Name, BsonValue.Create(fc.GetType().Name)));
                    }
                    else
                    {
                        //Set the other properties
                        doc.Add(new BsonElement(property.Name, BsonValue.Create(property.GetValue(fc, null))));
                    }
                }
                else
                {
                    //Reads the ID, if its null, does an insert, if is not, does an update
                    string _id = property.GetValue(fc, null).ToString();

                    /****************  Send coppy to the admin  ****************************/
                    if (!string.IsNullOrEmpty(_id)) //it's an update
                    {
                        docId = _id;
                        //Sete the property
                        doc.Add(new BsonElement(property.Name, new ObjectId(_id)));
                    }
                }
            }

            collection.Save(doc);

            //send copy to the agent
            String id = doc["_id"].ToString();
            doc.Remove("_id");
            try {
                JObject document = JsonConvert.DeserializeObject<JObject>(doc.ToString());
                document.Add("client-name", "cinepolis");
                document.Add("innerCollection", "field");
                document.Add("auxId", docId);
                document.Add("idInClient", id);
                String jsonData = JsonConvert.SerializeObject(document).Replace("\"", "'");
                initAgent();
                String jsonAgent = "{'action':'spreadToFather','collection':'ObjectReference','document':" + jsonData + ",'Source':'cinepolis'}";
                agent.sendMessage(jsonAgent);
                    }
            catch { }
            return id;
        }

        /// <summary>
        /// Check if customfield exist on the table, by his name, to do validation corresponding on no name repetition
        /// </summary>
        /// <param name="name">Name of the custom Field</param>
        /// <returns>True if exist, false if it doesnt</returns>
        /// <author>Galaviz Alejos Luis Angel</author>
        public bool CustomFieldExist(string name)
        {
            var field = collection.FindOneAs<BsonDocument>(Query.EQ("name", name));
            if (field != null)
            {
                return true;
            }
            return false;
        }

    }
}