using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RivkaConf.Conection
{

    /// <summary>
    ///     This class allows to configure the system database conection
    /// </summary>
    class DataBaseConfig
    {

        /// <summary>
        ///     This method allows to the system modules to know the database config, PUT THE DATABASE SYSTEM CONFIG HERE
        /// </summary>
        /// 
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// 
        /// <mongoRequiredData>
        ///     type : (MongoDB) specifies this conections as a mongo database conection.
        ///     host : the ip where the database is hosted.
        ///     user : the user's name for the database authetication.
        ///     password : the user's password for the database authentication
        ///     database : the database name
        /// </mongoRequiredData>
        /// 
        /// <returns>
        ///     Returns the database configuration to the caller module
        /// </returns>
        public static Dictionary<String, String> getDataBaseConfig()
        {
            Dictionary<String, String> dataBaseConection = new Dictionary<string, string>();

            String path = HttpContext.Current.Server.MapPath("/App_Data/database.conf");
            StreamReader objReader = new StreamReader(path);
            String sLine = objReader.ReadLine();

            JObject dbdata = JsonConvert.DeserializeObject<JObject>(sLine);
                
            dataBaseConection.Add("type", dbdata["type"].ToString());
            dataBaseConection.Add("host", dbdata["host"].ToString());
            dataBaseConection.Add("user", dbdata["user"].ToString());
            dataBaseConection.Add("password", dbdata["password"].ToString());
            dataBaseConection.Add("database", dbdata["database"].ToString());

            return dataBaseConection;
        }
    }
}