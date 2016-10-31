using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.User.Models
{
    public class CustomerTable
    {
        private MongoCollection collection;
        private MongoConection conection;

        /// <summary>
        ///     Initializes the model and sets the Customer collection
        /// </summary>
        public CustomerTable()
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection("Customers");
        }

        /// <summary>
        ///     Gets a list of the Customers related to a User
        /// </summary>
        /// <param name="CustomersRelated">
        ///     A json array with the Customers id
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        /// <author>
        ///     Juan Bautista
        /// </author>
        public string GetCustomerRelated(string CustomersRelated)
        {
            List<string> Customers = JsonConvert.DeserializeObject<List<string>>(CustomersRelated);
            List<BsonValue> CustomersBson = new List<BsonValue>();

            int i = 0;
            foreach (BsonValue Customer in Customers)
            {
                CustomersBson.Add(new BsonObjectId(Customer.ToString()));
            }

            var query = Query.In("_id", CustomersBson);
            var result = collection.FindAs(typeof(BsonDocument), query);

            List<BsonDocument> newResult = new List<BsonDocument>();

            foreach (BsonDocument customer in result)
            {
                BsonDocument newCustomer = new BsonDocument();

                newCustomer.Set("id", customer.GetElement("_id").Value.ToString());
                newCustomer.Set("name", customer.GetElement("name").Value.ToString());

                newResult.Add(newCustomer);
            }

            //collection.MapReduce(

            return newResult.ToJson();
        }

        public List<BsonDocument> getRows()
        {
            var cursor = collection.FindAllAs(typeof(BsonDocument)).SetSortOrder(SortBy.Ascending("name"));
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach (BsonDocument document in cursor)
            {
                documents.Add(document);
            }
            return documents;
        }

    }
}