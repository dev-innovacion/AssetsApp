using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rivka.Files
{

    /// <summary>
    ///     This class allows to work with a json section
    /// </summary>
    public class DataJson
    {
        protected JObject dataObject;

        /// <summary>
        ///     Allows to initializes this datajson with a section of the json
        /// </summary>
        /// <param name="JsonSection">The section of the json's string to work</param>
        public DataJson( String jsonSection ) {
            try
            {
                dataObject = JsonConvert.DeserializeObject<JObject>(jsonSection);
            }
            catch (Exception e) {
                //dataObject = null;
                dataObject = new JObject();
                //throw e;
            }
        }

        /// <summary>
        ///     Allows to initializes this datajson with a section of the json
        /// </summary>
        /// <param name="JsonSection">The section of the json in a jObject to work</param>
        public DataJson(JObject jsonSection)
        {
            try
            {
                dataObject = jsonSection;
            }
            catch (Exception e)
            {
                //dataObject = null;
                dataObject = new JObject();
                //throw e;
            }
        }

        public String ToString() {
            return JsonConvert.SerializeObject(dataObject);
        }

        /// <summary>
        ///     Allows to use index with this class
        /// </summary>
        /// <param name="key">The index to search for</param>
        /// <author>Luis Gonzalo Quijada Romero</author>
        /// <returns></returns>
        public dynamic this[String key]{
            get{
                try
                {
                    JObject subObject = (JObject)dataObject[key];
                    if (subObject == null) {
                        dataObject.Add(key, new JObject());
                        subObject = (JObject)dataObject[key];
                    }
                    return new DataJson(subObject);
                }
                catch (Exception e) {
                    if (dataObject != null)
                    {
                       
                        return dataObject[key].ToString();
                    }
                    else {
                        dataObject = new JObject();
                        dataObject.Add(key,"");
                        return new DataJson(dataObject);
                    }
                }
            }

            set{
                try
                {
                    Type type = value.GetType();
                    if (type == typeof(int[]) || type == typeof(string[]) || type == typeof(char[]))
                    {
                        dataObject[key] = new JArray(value);
                        return;
                    }
                    dataObject[key] = value;
                }catch(Exception e){
                    //unknow type
                }
            }
        }
    }
}