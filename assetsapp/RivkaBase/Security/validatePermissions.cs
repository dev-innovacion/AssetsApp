using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rivka.Security
{
    public class validatePermissions
    {

        public validatePermissions()
        {

        }
        public List<string> Tolistpermissions(String permission, String dataPermissions = null)
        {
            var datos = "";
            List<string> listdata = new List<string>();
            try
            {
                if (dataPermissions != null)
                {
                    datos = dataPermissions;
                    JObject allp2 = JsonConvert.DeserializeObject<JObject>(datos);

                    if (allp2[permission]["grant"].Count() > 0)
                    {
                        foreach (string x in allp2[permission]["grant"])
                        {
                          
                                listdata.Add(x);
                            
                        }
                    }

                    return listdata;
                }
                return listdata;

            }
            catch (Exception ex) { return listdata; }
        }
        public bool getpermissions(String permission, String type,String dataPermissions=null)
        {
            var datos = "";
            try
            {
                if (dataPermissions != null)
                {
                    datos = dataPermissions;
                    JObject allp2 = JsonConvert.DeserializeObject<JObject>(datos);

                    if (allp2[permission]["grant"].Count() > 0)
                    {
                        foreach (string x in allp2[permission]["grant"])
                        {
                            if (x.Contains(type))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                return false;

            }
            catch (Exception ex) { return false; }
        }
    }
}