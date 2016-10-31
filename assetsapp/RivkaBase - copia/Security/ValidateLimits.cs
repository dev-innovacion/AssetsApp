using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Db.MongoDb;
using MongoDB.Driver;
using Rivka.Files;
using System.Net;

namespace Rivka.Security
{

    public class ValidateLimits
    {
        protected MongoModel dbMod;
     //   protected MongoModel dbLimits;
        protected DataFileManager Filelimits;
        public ValidateLimits()
        {
          //  dbLimits = new MongoModel("Limits");
            string relativepath = "/App_Data/system.conf";
            string absolutedpath = HttpContext.Current.Server.MapPath(relativepath);
            
            Filelimits = new DataFileManager(absolutedpath,"juanin");
        }

        public string validate(String nameCollection, String key = null, String value = null, String mod = null, int numitems = 0, bool filter = false, int getavailables = 0)
        {
            try
            {
                return "true";
                dbMod = new MongoModel(nameCollection);
                String result = "";
                if (key == null || value == null)
                {
                    result = dbMod.GetRows();
                }
                else if (key != null && value != null)
                {
                    if (filter == true)
                    {
                        result = dbMod.Get(key, value);
                    }
                    else
                    {
                        result = dbMod.GetRows();


                    }
                }
                JArray resultja = JsonConvert.DeserializeObject<JArray>(result);
             //   String resultlimit = dbLimits.GetRows();
             //   JArray limitsja = JsonConvert.DeserializeObject<JArray>(resultlimit);
                JObject limitsjo = new JObject();
                int counttotals = 0;
                if ((key != null) && filter == false)
                {
                    foreach (JObject item in resultja)
                    {
                        int num = 0;
                        int.TryParse(item[key].ToString(), out num);
                        counttotals = counttotals + num;
                    }
                }
                else
                {
                    counttotals = resultja.Count();
                }

              /*  foreach (JObject item in limitsja)
                {

                    limitsjo = item;
                }
                */
              //  int limit = Convert.ToInt32(limitsjo[mod].ToString());
                int limit = Convert.ToInt32(Filelimits["clientInfo"][mod].ToString());
                if (limit < 0)
                {
                    return "true";
                }
                int total = counttotals;
                int stock = numitems + total;
                int availables = limit - stock;
                int ac = limit - total;
                if (ac < 0)
                {
                    ac = 0;

                }
                if (availables < 0)
                {
                    availables = 0;

                }

                if (stock <= limit)
                {
                    return "true";
                }
                else
                {
                    if (getavailables == 1)
                    {
                        return ac.ToString();
                    }
                    if (numitems > 1)
                    {
                        return "Su solicitud sobrepasa el limite,solo puede crear  " + ac + " ";
                    }
                    return "Ha llegado a su limite permitido,no puede crear más ";
                }


            }
            catch (Exception ex)
            {

                return "false";
            }


        }
    }
}