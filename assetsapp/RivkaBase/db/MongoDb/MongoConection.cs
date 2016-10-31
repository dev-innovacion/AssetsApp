using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Rivka.Db.MongoDb
{

    /// <summary>
    ///     This class allows to configure the conection with the mongodb database
    /// </summary>
    /// 
    /// <author>
    ///     Luis Gonzalo Quijada Romero
    /// </author>
    public class MongoConection : Conection
    {
        private MongoServer conection;
        private MongoDatabase mongoDB;
        private MongoCollection mongoColection;


        public MongoConection( Dictionary<string,string> dbConfig )
        {
            //LAN
            host = dbConfig["host"];
            user = dbConfig["user"];
            password = dbConfig["password"];
            database = dbConfig["database"];
        }

        /// <summary>
        ///     This method allows to get a conection to the mongo database
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     The conection objecto to the database
        /// </returns>
        public MongoServer getConection( Boolean authAgainstAdmin = false )
        {
            if (conection == null)
            { //does the conection exist already?

                String conectionString = "mongodb://" + user + ":" + password + "@" + host + "/";
                if (!authAgainstAdmin)
                {
                    conectionString += database;
                }
                else {
                    conectionString += "admin";
                }
                conection = new MongoClient(conectionString).GetServer();
                try
                {
                    conection.Connect();
                }
                catch(Exception e){}
            }
            return conection;
        }

        /// <summary>
        ///     This method allows to get a reference to the mongo database
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     A reference to the mongo database 
        /// </returns>
        public MongoDatabase getDataBase( Boolean authAgainstAdmin = false )
        {

            if (conection == null)
            { //is there a conection setted?
                getConection(authAgainstAdmin);
            }
            if (mongoDB == null)
            { //the database reference does not exist?
                mongoDB = conection.GetDatabase(database);
            }
            return mongoDB;
        }

        /// <summary>
        ///     This method allows to get a reference to the mongo database collection
        /// </summary>
        /// <param name="collectionName">
        ///     The collection's name in the mongo database
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a reference to the mongo database's collection
        /// </returns>
        public MongoCollection getCollection( String collectionName, Boolean authAgainstAdmin = false )
        {
            if (mongoDB == null)
            { //is the database reference already setted?
                getDataBase(authAgainstAdmin); //if is not created, then we create it
            }
            if (mongoColection == null)
            {
                mongoColection = mongoDB.GetCollection(collectionName);
            }
            return mongoColection;
        }

    }
}