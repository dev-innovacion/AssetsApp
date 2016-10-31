using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rivka.Db
{

    /// <summary>
    ///     This class allows to configure the conection with the mongodb database
    /// </summary>
    /// 
    /// <author>
    ///     Luis Gonzalo Quijada Romero
    /// </author>
    public abstract class Conection
    {
        protected String host;
        protected String user;
        protected String password;
        protected String database;

        /// <summary>
        ///     This method allows to get the database conection
        /// </summary>
        /// 
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// 
        /// <returns>
        ///     The database conection
        /// </returns>
        public static Conection getConection( Dictionary<string,string> dbConf = null ) {

            if( dbConf == null){
                dbConf = RivkaConf.Conection.DataBaseConfig.getDataBaseConfig(); //getting the system database configuration
            }
            string dbType = dbConf["type"]; //getting the database type

            switch (dbType) { 
                case "MongoDB":
                    return new Rivka.Db.MongoDb.MongoConection( dbConf );
            }
            return null;
        }
    }
}