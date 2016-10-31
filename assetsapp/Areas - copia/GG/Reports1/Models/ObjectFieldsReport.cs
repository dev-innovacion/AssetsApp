using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Drawing;
using System.Globalization;
using Rivka.Db;
using Rivka.Db.MongoDb;
using System.Web;
using MongoDB.Driver.Linq;


namespace RivkaAreas.Reports.Models
{
    public class ObjectFieldsReport : MongoModel
    {
        //
        // GET: /Reports/DemandReport/

        private MongoCollection collection;
        private MongoConection conection;
        private MongoCollection collection1;
        private MongoConection conection1;
        private MongoConection conection2;


        public ObjectFieldsReport(string table)
            : base("ObjectFields")
        {
            conection = (MongoConection)Conection.getConection();
            conection1 = (MongoConection)Conection.getConection();
            conection2 = (MongoConection)Conection.getConection();

            collection = conection.getCollection(table);
           // collection.EnsureIndex(IndexKeys.Ascending("_id"), IndexOptions.SetUnique(true));

        }
        public String Mapreduced(Dictionary<string, string> fields, List<string> movement = null, List<string> objs = null, List<string> locations = null, List<string> users = null, Int64 start = 0, Int64 end = 0)
        {
            var orderfield = "folio";


            try
            {
                List<string> cols = new List<string>();
                foreach (var x in fields)
                {
                    cols.Add(x.Key);
                }
                cols.Add("CreatedTimeStamp");
                string[] arrayfields = cols.ToArray();
                string[] arraymov = movement.ToArray();
                string[] arrayobjs = objs.ToArray();
                string[] arraylocations = locations.ToArray();
                string[] arrayusers = users.ToArray();

                BsonArray bsonarraymov = new BsonArray();
                BsonArray bsonarrayobjs = new BsonArray();
                BsonArray bsonarrayloc = new BsonArray();
                BsonArray bsonarrayusers = new BsonArray();
                for (int i = 0; i < arraymov.Length; i++)
                {
                    bsonarraymov.Add(arraymov[i]);
                }
                for (int i = 0; i < arrayobjs.Length; i++)
                {
                    bsonarrayobjs.Add(arrayobjs[i]);
                } for (int i = 0; i < arraylocations.Length; i++)
                {
                    bsonarrayloc.Add(arraylocations[i]);
                } for (int i = 0; i < arrayusers.Length; i++)
                {
                    bsonarrayusers.Add(arrayusers[i]);
                }


                List<BsonDocument> documents = new List<BsonDocument>();

                var query = Query.And(Query.In("movement", bsonarraymov), Query.In("object", bsonarrayobjs), Query.In("location", bsonarrayloc), Query.In("Creator", bsonarrayusers), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

                /*Realiza la conexion */
                MongoCollection Demand = conection.getCollection("Demand");
                MongoCollection Movements = conection1.getCollection("MovementProfiles");
                MongoCollection Locations = conection2.getCollection("Locations");

                var Movementsfunction = @"function() {
                var key = this._id.valueOf();
                var value={movement:this.name};
                 emit(key,value);
                   }";
                var Locationsfunction = @"function() {
                var key = this._id.valueOf();
                var value={location:this.name};
                 emit(key,value);
                   }";
                var Demandfunction = @"function() {
                    var key = this.movement;
                     var value = {id:this._id.valueOf(),folio:this.folio,object:this.object,total:this.total,status:this.status,movementFields:this.movementFields,CreatedDate:this.CreatedDate,CreatedTimeStamp:this.CreatedTimeStamp,Creator:this.Creator,system_status:this.system_status};
                     emit(key,value);
                
                   }";
                var Demandfunction2 = @"function() {
                    var key = this.location;
                     var value = {id:this._id.valueOf(),folio:this.folio,object:this.object,total:this.total,status:this.status,movementFields:this.movementFields,CreatedDate:this.CreatedDate,CreatedTimeStamp:this.CreatedTimeStamp,Creator:this.Creator,system_status:this.system_status};
                     emit(key,value);
                
                   }";

                var reducefunction = @"function (key, values) {
               var reduced = {id:'',folio:'',movement:'',location:'',object:'',total:'',status:'',movementFields:'',CreatedDate:'',CreatedTimeStamp:'',Creator:'',system_status:''};
                var index=0;
              values.forEach(function(value){
              
          
                 
                    if(value.movement != null && reduced.movement ==''){
                    
                    reduced.movement = value.movement;
                  
                    }
                    if(value.location != null && reduced.location == ''){
                    
                    reduced.location = value.location;
                  
                    }
                   
                    if(value.id != null){
                       reduced.id= value.id;
                       reduced.folio = value.folio;
                      
                        reduced.object = value.object;
                        reduced.total = value.total;
                        reduced.status = value.status;
                        reduced.movementFields = value.movementFields;
                        reduced.CreatedDate = value.CreatedDate;
                        reduced.CreatedTimeStamp = value.CreatedTimeStamp;
                        reduced.Creator = value.Creator;
                        reduced.system_status = value.system_status;

                     
                    }
           
            });
           
            return reduced;
          }";

                /*Establece las propiedades a seguir del mapreduce */
                MapReduceOptionsBuilder options = new MapReduceOptionsBuilder();
                options.SetOutput(MapReduceOutput.Inline);
                options.SetOutput(MapReduceOutput.Reduce("result", true));



                /* Realiza los mapreduce sobre el mismo resultado */
                MapReduceResult result = Movements.MapReduce(Movementsfunction, reducefunction, options);
                // result = Locations.MapReduce(Locationsfunction, reducefunction, options);

                result = Demand.MapReduce(Demandfunction, reducefunction, options);
                //    result = Demand.MapReduce(Demandfunction2, reducefunction, options);


                /* resultado en json */
                string results = result.GetResults().ToJson();
                return results;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public String GetRowsReportObjectFields(Dictionary<string, string> fields, List<string> movement = null, List<string> objs = null, List<string> locations = null, List<string> users = null, Int64 start = 0, Int64 end = 0)
        {


            var orderfield = "folio";


            try
            {
                List<string> cols = new List<string>();
                foreach (var x in fields)
                {
                    cols.Add(x.Key);
                }
                cols.Add("CreatedTimeStamp");
                string[] arrayfields = cols.ToArray();
                string[] arraymov = movement.ToArray();
                string[] arrayobjs = objs.ToArray();
                string[] arraylocations = locations.ToArray();
                string[] arrayusers = users.ToArray();

                BsonArray bsonarraymov = new BsonArray();
                BsonArray bsonarrayobjs = new BsonArray();
                BsonArray bsonarrayloc = new BsonArray();
                BsonArray bsonarrayusers = new BsonArray();
                for (int i = 0; i < arraymov.Length; i++)
                {
                    bsonarraymov.Add(arraymov[i]);
                }
                for (int i = 0; i < arrayobjs.Length; i++)
                {
                    bsonarrayobjs.Add(arrayobjs[i]);
                } for (int i = 0; i < arraylocations.Length; i++)
                {
                    bsonarrayloc.Add(arraylocations[i]);
                } for (int i = 0; i < arrayusers.Length; i++)
                {
                    bsonarrayusers.Add(arrayusers[i]);
                }


                List<BsonDocument> documents = new List<BsonDocument>();
                if (movement != null && movement.Count > 0)
                {
                    var query = Query.And(Query.In("movement", bsonarraymov), Query.In("object", bsonarrayobjs), Query.In("location", bsonarrayloc), Query.In("Creator", bsonarrayusers), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                    var cursor = collection.FindAs(typeof(BsonDocument), query).SetFields(arrayfields).SetSortOrder(SortBy.Ascending(orderfield));


                    foreach (BsonDocument document in cursor)
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                        try
                        {
                            document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex)
                        {

                        }
                        documents.Add(document);
                    }
                }
                else
                {
                    var query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

                    var cursor = collection.FindAs(typeof(BsonDocument), query).SetFields(arrayfields).SetSortOrder(SortBy.Ascending(orderfield));
                    foreach (BsonDocument document in cursor)
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                        try
                        {
                            document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex)
                        {

                        }
                        documents.Add(document);
                    }

                }
                return documents.ToJson();
            }
            catch (Exception e)
            {
                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }

    }
}

