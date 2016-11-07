using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Db.MongoDb;
using RivkaAreas.Reports.Models;
using System.Drawing;
using MongoDB.Driver;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using iTextSharp.text.html.simpleparser;
using System.IO;
using System.util;
using System.Net;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using Rivka.Security;
using Rivka.Error;
using System.Reflection;
using System.Text;
using RivkaAreas.Message.Controllers;
using Rivka.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace RivkaAreas.Reports.Controllers
{
     [Authorize]
    public class ReportsController : Controller
    {
        //
        // GET: /Reports/Reports/
        protected MongoModel profilesdb = new MongoModel("Profiles");
        protected CreateExcelDoc Exceldoc;
      
        protected MongoModel semaphoredb = new MongoModel("Semaphore");
        protected MongoModel messagesdb = new MongoModel("Messages");
        protected MongoModel usermessagesdb = new MongoModel("UserMessage");
        protected UsersReport Userdb = new UsersReport("Users");
        protected ObjectsRealReport ObjectsRealdb = new ObjectsRealReport("ObjectReal");
        protected HwReport Hwdb = new HwReport("Hardware");
        protected MongoModel HwCat = new MongoModel("HardwareCategories");
        protected ProfileReport Profiledb = new ProfileReport("Profiles");
        protected ProccessReport Proccessdb = new ProccessReport("Processes");
        protected ObjectsRefReport Objrefdb = new ObjectsRefReport("ReferenceObjects");
        protected LocationsReport locationsdb = new LocationsReport("Locations");
        protected MovementReport Movementdb = new MovementReport("MovementProfiles");
        protected InventoryReport Inventorydb = new InventoryReport("Inventory");
        protected MongoModel locationdb = new MongoModel("Locations");
        protected MongoModel listdb = new MongoModel("Lists");
        protected MongoModel alertsdb = new MongoModel("Alerts");
        protected MongoModel Categoriesdb = new MongoModel("Categories");
        protected MongoModel locationsProfilesdb = new MongoModel("LocationProfiles");
        protected getReports Reportsdb = new getReports("Reports");
        protected MongoModel Dashboard = new MongoModel("Dashboard");
        protected DemandReport demanddb = new DemandReport("Demand");
        protected ObjectFieldsReport ObjectFieldsdb = new ObjectFieldsReport("ObjectFields");
        protected NotificationReport Notifications = new NotificationReport();
        protected validatePermissions validatepermissions = new validatePermissions();
        protected MessageController messagesC = new MessageController();
        public class headgraph
        {
            public Dictionary<int, string> Head { get; set; }
            public List<int> listA { get; set; }
            public string range { get; set; }
            public headgraph(Dictionary<int, string> head, List<int> array, string type)
            {
                Head = head;
                listA = array;
                range = type;
            }
        }
        public Dictionary<int, string> generateDict()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(1, "A");
            dict.Add(2, "B");
            dict.Add(3, "C");
            dict.Add(4, "D");
            dict.Add(5, "E");
            dict.Add(6, "F");
            dict.Add(7, "G");
            dict.Add(8, "H");
            dict.Add(9, "I");
            dict.Add(10, "J");
            dict.Add(11, "K");
            dict.Add(12, "L");
            dict.Add(13, "M");
            dict.Add(14, "N");
            dict.Add(15, "O");
            dict.Add(16, "P");
            dict.Add(17, "Q");
            dict.Add(18, "R");
            dict.Add(19, "S");
            dict.Add(20, "T");
            dict.Add(21, "U");
            dict.Add(22, "V");
            dict.Add(23, "W");
            dict.Add(24, "X");
            dict.Add(25, "Y");
            dict.Add(26, "Z");
            dict.Add(27, "AA");
            dict.Add(28, "AB");
            dict.Add(29, "AC");
            dict.Add(30, "AD");
            dict.Add(31, "AE");
            dict.Add(32, "AF");
            dict.Add(33, "AG");
            dict.Add(34, "AH");
            dict.Add(35, "AI");
            dict.Add(36, "AJ");
            dict.Add(37, "AK");
            dict.Add(38, "AL");
            dict.Add(39, "AM");
            dict.Add(40, "AN");
            dict.Add(41, "AO");
            dict.Add(42, "AP");
            dict.Add(43, "AQ");
            dict.Add(44, "AR");
            dict.Add(45, "AS");
            dict.Add(46, "AT");
            dict.Add(47, "AU");
            dict.Add(48, "AV");
            dict.Add(49, "AW");
            dict.Add(50, "AX");
            dict.Add(51, "AY");
            dict.Add(52, "AZ");
            dict.Add(53, "BA");
            dict.Add(54, "BB");
            dict.Add(55, "BC");
            dict.Add(56, "BD");
            dict.Add(57, "BE");
            dict.Add(58, "BF");
            dict.Add(59, "BG");
            dict.Add(60, "BH");
            dict.Add(61, "BI");
            dict.Add(62, "BJ");
            dict.Add(63, "BK");
            dict.Add(64, "BL");
            dict.Add(65, "BM");
            dict.Add(66, "BN");
            dict.Add(67, "BO");
            dict.Add(68, "BP");
            dict.Add(69, "BQ");
            dict.Add(70, "BR");
            dict.Add(71, "BS");
            dict.Add(72, "BT");
            dict.Add(73, "BU");
            dict.Add(74, "BV");
            dict.Add(75, "BW");
            dict.Add(76, "BX");
            dict.Add(77, "BY");
            dict.Add(78, "BZ");
            dict.Add(79, "CA");
            dict.Add(80, "CB");
            dict.Add(81, "CC");
            return dict;
        }
        /// <summary>
        ///     Returns a Graph with the parameters for the report
        /// </summary>
        /// <param name="filter">
        ///     All filters report needed
        /// </param>
        /// <returns></returns>
        public JsonResult GeneralReport(string filter)
        {
            JObject fil = JsonConvert.DeserializeObject<JObject>(filter);
            JObject result = new JObject();
            
            string objsarray = "";
            if (fil["module"].ToString() == "ObjectReal")
                objsarray = Reportsdb.GetObjectsTable(fil);
            if (fil["module"].ToString() == "Locations")
                objsarray = Reportsdb.GetLocationsTable(fil);
            if (fil["module"].ToString() == "Users")
                objsarray = Reportsdb.GetUsersTable(fil);
            if (fil["module"].ToString() == "Demand")
                objsarray = Reportsdb.GetMovementsTable(fil);
            if (fil["module"].ToString() == "Notifications")
            {
                JsonResult response = Json("[]");

                try
                {
                    string notifications = Notifications.GetNotifications(fil["filters"]["modules"].ToString());

                    response = Json(notifications);
                }
                catch (Exception e)
                {
                    Error.Log(e, "Getting last notifications");
                }

                return response;
            }

            JArray objs = JsonConvert.DeserializeObject<JArray>(objsarray);
            JArray contadores = new JArray();

            JArray valors = new JArray();
            if (fil["type"].ToString() == "summary")
            {
                return Json(JsonConvert.SerializeObject(objs.Count));
            }
            JArray cad = JsonConvert.DeserializeObject<JArray>(fil["date"].ToString());
            JObject ejem = new JObject();
            JObject objfilter = JsonConvert.DeserializeObject<JObject>(fil["filters"].ToString());
            ejem["unidad"] = cad[1].ToString();
            if (fil["graphtype"].ToString() == "combochart" || fil["graphtype"].ToString() == "linechart")
            {
                JArray obj1 = new JArray();
                JArray obj2 = new JArray();
                JArray obj3 = new JArray();
                JArray obj4 = new JArray();
                JArray obj5 = new JArray();

                JObject c1 = new JObject();
                JObject c2 = new JObject();
                JObject c3 = new JObject();
                JObject c4 = new JObject();
                JObject c5 = new JObject();

                double num = 0;

                int cont1 = 0, cont2 = 0, cont3 = 0, cont4 = 0, cont5 = 0;

                double.TryParse(cad[0].ToString(), out num);


                if (cad[1].ToString() == "days")
                {
                    obj1.Add(DateTime.Now.AddDays(-num / 5));
                    c1["fecha"] = DateTime.Now.AddDays(-num / 5);
                    obj2.Add(DateTime.Now.AddDays(-2 * num / 5));
                    c2["fecha"] = DateTime.Now.AddDays(-2 * num / 5);
                    obj3.Add(DateTime.Now.AddDays(-3 * num / 5));
                    c3["fecha"] = DateTime.Now.AddDays(-3 * num / 5);
                    obj4.Add(DateTime.Now.AddDays(-4 * num / 5));
                    c4["fecha"] = DateTime.Now.AddDays(-4 * num / 5);
                    obj5.Add(DateTime.Now.AddDays(-num));
                    c5["fecha"] = DateTime.Now.AddDays(-num);
                }
                if (cad[1].ToString() == "minutes")
                {
                    obj1.Add(DateTime.Now.AddMinutes(-num / 5));
                    c1["fecha"] = DateTime.Now.AddMinutes(-num / 5);
                    obj2.Add(DateTime.Now.AddMinutes(-2 * num / 5));
                    c2["fecha"] = DateTime.Now.AddMinutes(-2 * num / 5);
                    obj3.Add(DateTime.Now.AddMinutes(-3 * num / 5));
                    c3["fecha"] = DateTime.Now.AddMinutes(-3 * num / 5);
                    obj4.Add(DateTime.Now.AddMinutes(-4 * num / 5));
                    c4["fecha"] = DateTime.Now.AddMinutes(-4 * num / 5);
                    obj5.Add(DateTime.Now.AddMinutes(-num));
                    c5["fecha"] = DateTime.Now.AddMinutes(-num);
                }
                if (cad[1].ToString() == "hours")
                {
                    obj1.Add(DateTime.Now.AddHours(-num / 5));
                    c1["fecha"] = DateTime.Now.AddHours(-num / 5);
                    obj2.Add(DateTime.Now.AddHours(-2 * num / 5));
                    c2["fecha"] = DateTime.Now.AddHours(-2 * num / 5);
                    obj3.Add(DateTime.Now.AddHours(-3 * num / 5));
                    c3["fecha"] = DateTime.Now.AddHours(-3 * num / 5);
                    obj4.Add(DateTime.Now.AddHours(-4 * num / 5));
                    c4["fecha"] = DateTime.Now.AddHours(-4 * num / 5);
                    obj5.Add(DateTime.Now.AddHours(-num));
                    c5["fecha"] = DateTime.Now.AddHours(-num);
                }


                foreach (JObject obj in objs)
                {

                    if (obj[objfilter["group"].ToString()].ToString() == "")
                    {
                        ejem["NA"] = "NA";
                    }
                    else
                        ejem[obj[objfilter["group"].ToString()].ToString()] = obj[objfilter["group"].ToString()].ToString();

                    int year, month, day, hour, minute, second;
                    int.TryParse(obj["CreatedTimeStamp"].ToString().Substring(0, 4), out year);
                    int.TryParse(obj["CreatedTimeStamp"].ToString().Substring(4, 2), out month);
                    int.TryParse(obj["CreatedTimeStamp"].ToString().Substring(6, 2), out day);
                    int.TryParse(obj["CreatedTimeStamp"].ToString().Substring(8, 2), out hour);
                    int.TryParse(obj["CreatedTimeStamp"].ToString().Substring(10, 2), out minute);
                    int.TryParse(obj["CreatedTimeStamp"].ToString().Substring(12, 2), out second);
                    DateTime fecha = new DateTime(year, month, day, hour, minute, second);
                    // DateTime.TryParse(, out fecha2);
                    if (fecha.CompareTo(DateTime.Parse(obj1[0].ToString())) > 0)
                    {
                        try
                        {
                            int.TryParse(c1[obj[objfilter["group"].ToString()].ToString()].ToString(), out cont1);
                            cont1++;
                            c1[obj[objfilter["group"].ToString()].ToString()] = cont1;
                        }
                        catch (Exception ex)
                        {
                            c1[obj[objfilter["group"].ToString()].ToString()] = 1;
                        }
                    }
                    if (fecha.CompareTo(DateTime.Parse(obj2[0].ToString())) > 0 && fecha.CompareTo(DateTime.Parse(obj1[0].ToString())) < 0)
                    {
                        try
                        {
                            int.TryParse(c2[obj[objfilter["group"].ToString()].ToString()].ToString(), out cont2);
                            cont2++;
                            c2[obj[objfilter["group"].ToString()].ToString()] = cont2;
                        }
                        catch (Exception ex)
                        {
                            c2[obj[objfilter["group"].ToString()].ToString()] = 1;
                        }
                    }
                    if (fecha.CompareTo(DateTime.Parse(obj3[0].ToString())) > 0 && fecha.CompareTo(DateTime.Parse(obj2[0].ToString())) < 0)
                    {
                        try
                        {
                            int.TryParse(c3[obj[objfilter["group"].ToString()].ToString()].ToString(), out cont3);
                            cont3++;
                            c3[obj[objfilter["group"].ToString()].ToString()] = cont3;
                        }
                        catch (Exception ex)
                        {
                            c3[obj[objfilter["group"].ToString()].ToString()] = 1;
                        }
                        
                    }
                    if (fecha.CompareTo(DateTime.Parse(obj4[0].ToString())) > 0 && fecha.CompareTo(DateTime.Parse(obj3[0].ToString())) < 0)
                    {
                        try
                        {
                            int.TryParse(c4[obj[objfilter["group"].ToString()].ToString()].ToString(), out cont4);
                            cont4++;
                            c4[obj[objfilter["group"].ToString()].ToString()] = cont4;
                        }
                        catch (Exception ex)
                        {
                            c4[obj[objfilter["group"].ToString()].ToString()] = 1;
                        }
                    }
                    if (fecha.CompareTo(DateTime.Parse(obj5[0].ToString())) > 0 && fecha.CompareTo(DateTime.Parse(obj4[0].ToString())) < 0)
                    {
                        try
                        {
                            int.TryParse(c5[obj[objfilter["group"].ToString()].ToString()].ToString(), out cont5);
                            cont5++;
                            c5[obj[objfilter["group"].ToString()].ToString()] = cont5;
                        }
                        catch (Exception ex)
                        {
                            c5[obj[objfilter["group"].ToString()].ToString()] = 1;
                        }
                    }
                }

                if (objs.Count == 0)
                {
                    if (fil["module"].ToString() == "ObjectReal")
                        ejem["nada"] = "Activos";
                    if (fil["module"].ToString() == "Locations")
                        ejem["nada"] = "Ubicaciones";
                    if (fil["module"].ToString() == "Users")
                        ejem["nada"] = "Usuarios";
                    if (fil["module"].ToString() == "Demand")
                        ejem["nada"] = "Movimientos";
                }

                foreach (KeyValuePair<String, JToken> token in ejem)
                {
                    valors.Add(token.Value);
                }
                contadores.Add(valors);
                JToken valor;
                foreach (KeyValuePair<String, JToken> token in ejem)
                {
                    if (token.Key != "unidad")
                    {
                        if (c1.TryGetValue(token.Value.ToString(), out valor))
                        {
                            obj1.Add(c1[token.Value.ToString()]);
                        }
                        else
                            obj1.Add(0);
                    }
                    else
                    {
                        if (ejem["unidad"].ToString() == "days")
                        {
                            obj1[0] = DateTime.Parse(obj1[0].ToString()).ToShortDateString();
                        }
                        if (ejem["unidad"].ToString() == "hours")
                        {
                            obj1[0] = DateTime.Parse(obj1[0].ToString()).ToShortTimeString();
                        }
                        if (ejem["unidad"].ToString() == "minutes")
                        {
                            obj1[0] = DateTime.Parse(obj1[0].ToString()).ToShortTimeString();
                        }
                    }
                        
                }

                foreach (KeyValuePair<String, JToken> token in ejem)
                {
                    if (token.Key != "unidad")
                    {
                        if (c2.TryGetValue(token.Value.ToString(), out valor))
                        {
                            obj2.Add(c2[token.Value.ToString()]);
                        }
                        else
                            obj2.Add(0);
                    }
                    else
                    {
                        if (ejem["unidad"].ToString() == "days")
                        {
                            obj2[0] = DateTime.Parse(obj2[0].ToString()).ToShortDateString();
                        }
                        if (ejem["unidad"].ToString() == "hours")
                        {
                            obj2[0] = DateTime.Parse(obj2[0].ToString()).ToShortTimeString();
                        }
                        if (ejem["unidad"].ToString() == "minutes")
                        {
                            obj2[0] = DateTime.Parse(obj2[0].ToString()).ToShortTimeString();
                        }
                    }
                }

                foreach (KeyValuePair<String, JToken> token in ejem)
                {
                    if (token.Key != "unidad")
                    {
                        if (c3.TryGetValue(token.Value.ToString(), out valor))
                        {
                            obj3.Add(c3[token.Value.ToString()]);
                        }
                        else
                            obj3.Add(0);
                    }
                    else
                    {
                        if (ejem["unidad"].ToString() == "days")
                        {
                            obj3[0] = DateTime.Parse(obj3[0].ToString()).ToShortDateString();
                        }
                        if (ejem["unidad"].ToString() == "hours")
                        {
                            obj3[0] = DateTime.Parse(obj3[0].ToString()).ToShortTimeString();
                        }
                        if (ejem["unidad"].ToString() == "minutes")
                        {
                            obj3[0] = DateTime.Parse(obj3[0].ToString()).ToShortTimeString();
                        }
                    }

                }

                foreach (KeyValuePair<String, JToken> token in ejem)
                {
                    if (token.Key != "unidad")
                    {
                        if (c4.TryGetValue(token.Value.ToString(), out valor))
                        {
                            obj4.Add(c4[token.Value.ToString()]);
                        }
                        else
                            obj4.Add(0);
                    }
                    else
                    {
                        if (ejem["unidad"].ToString() == "days")
                        {
                            obj4[0] = DateTime.Parse(obj4[0].ToString()).ToShortDateString();
                        }
                        if (ejem["unidad"].ToString() == "hours")
                        {
                            obj4[0] = DateTime.Parse(obj4[0].ToString()).ToShortTimeString();
                        }
                        if (ejem["unidad"].ToString() == "minutes")
                        {
                            obj4[0] = DateTime.Parse(obj4[0].ToString()).ToShortTimeString();
                        }
                    }

                }

                foreach (KeyValuePair<String, JToken> token in ejem)
                {
                    if (token.Key != "unidad")
                    {
                        if (c5.TryGetValue(token.Value.ToString(), out valor))
                        {
                            obj5.Add(c5[token.Value.ToString()]);
                        }
                        else
                            obj5.Add(0);
                    }
                    else
                    {
                        if (ejem["unidad"].ToString() == "days")
                        {
                            obj5[0] = DateTime.Parse(obj5[0].ToString()).ToShortDateString();
                        }
                        if (ejem["unidad"].ToString() == "hours")
                        {
                            obj5[0] = DateTime.Parse(obj5[0].ToString()).ToShortTimeString();
                        }
                        if (ejem["unidad"].ToString() == "minutes")
                        {
                            obj5[0] = DateTime.Parse(obj5[0].ToString()).ToShortTimeString();
                        }
                    }
                }
                contadores.Add(obj5);
                contadores.Add(obj4);
                contadores.Add(obj3);
                contadores.Add(obj2);
                contadores.Add(obj1);

            }
            if (fil["graphtype"].ToString() == "piechart")
            {
                JArray obj1 = new JArray();
                JObject c1 = new JObject();
                double num = 0;

                int cont1 = 0;

                obj1.Add(objfilter["group"].ToString());

                double.TryParse(cad[0].ToString(), out num);

                if (cad[1].ToString() == "days")
                {
                    obj1.Add(DateTime.Now.AddDays(-num).ToShortDateString());
                    c1["fecha"] = DateTime.Now.AddDays(-num);
                }
                if (cad[1].ToString() == "minutes")
                {
                    obj1.Add(DateTime.Now.AddMinutes(-num));
                    c1["fecha"] = DateTime.Now.AddMinutes(-num);
                }
                if (cad[1].ToString() == "hours")
                {
                    obj1.Add(DateTime.Now.AddHours(-num));
                    c1["fecha"] = DateTime.Now.AddHours(-num);
                }

                contadores.Add(obj1);

                foreach (JObject obj in objs)
                {

                    ejem[obj[objfilter["group"].ToString()].ToString()] = obj[objfilter["group"].ToString()].ToString();
                    try
                    {
                        int.TryParse(c1[obj[objfilter["group"].ToString()].ToString()].ToString(), out cont1);
                        cont1++;
                        c1[obj[objfilter["group"].ToString()].ToString()] = cont1;
                    }
                    catch (Exception ex)
                    {
                        c1[obj[objfilter["group"].ToString()].ToString()] = 1;
                    }
                    
                }

                JToken valor;
                foreach (KeyValuePair<String, JToken> token in ejem)
                {
                    if (token.Key != "unidad")
                    {
                        JArray o1 = new JArray();
                        o1.Add(token.Value.ToString());
                        if (c1.TryGetValue(token.Value.ToString(), out valor))
                        {
                            o1.Add(c1[token.Value.ToString()]);
                        }
                        else
                            o1.Add(0);
                        contadores.Add(o1);
                    }
                }

            }

            
            return Json(JsonConvert.SerializeObject(contadores));
        }
        public ActionResult MovementTemporalReport(string id, string datestart, string dateend)
        {
            // var types = Assembly.GetCallingAssembly().GetTypes();
            Dictionary<string, string> obj = new Dictionary<string, string>();
            Dictionary<string, string> movement = new Dictionary<string, string>();
            Dictionary<string, string> status = new Dictionary<string, string>();

            Dictionary<string, string> reports = new Dictionary<string, string>();
            /*  if (Request.Cookies["_id2"] != null)
           {
               Session["_id"] = Request.Cookies["_id2"].Value;
           }*/
            // string idreport=Session["_id"].ToString();
            string getobjs = demanddb.GetRows();
            JArray objsja = JsonConvert.DeserializeObject<JArray>(getobjs);
            string getmovement = Movementdb.GetRows();
            JArray movementja = JsonConvert.DeserializeObject<JArray>(getmovement);
            //  string getstatus = demanddb.GetRows();
            //  JArray statusja = JsonConvert.DeserializeObject<JArray>(getstatus);
            try
            {
                obj = objsja.ToDictionary(x => (string)x["_id"], x => (string)x["folio"]);
            }
            catch (Exception ex) { }
            try
            {
                movement = movementja.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
            }
            catch (Exception ex) { }
            /*  foreach (JObject item1 in usersja)
              {
                  users.Add(item1["_id"].ToString(), item1["name"].ToString());
              }
              foreach (JObject item1 in locationja)
              {
                  locations.Add(item1["_id"].ToString(), item1["name"].ToString());
              }
              foreach (JObject item1 in objsja)
              {
                  obj.Add(item1["_id"].ToString(), item1["name"].ToString());
              }
             foreach (JObject item1 in statusja)
              {
                  if (!status.Keys.Contains(item1["status"].ToString()))
                      status.Add(item1["status"].ToString(), item1["status"].ToString());
              }
              foreach (JObject item1 in movementja)
              {
                  movement.Add(item1["_id"].ToString(), item1["name"].ToString());
              }*/
            string idreport = id; string type = "MovementMaster";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;
            ViewData["obj"] = obj;
            ViewData["movement"] = movement;
            //  ViewData["status"] = status;
            return View();

        }
      

        public ActionResult MovementMasterReport(string id, string datestart, string dateend)
        {
            try
            {
                string permission = Session["Permissions"].ToString();
            }
            catch (Exception ex)
            {
                if (Request.Cookies["permissions"] != null)
                {
                    Session["Permissions"] = Request.Cookies["permissions"].Value;

                }

            }

            String dataPermissions = Session["Permissions"].ToString();
            List<string> allowreports = validatepermissions.Tolistpermissions("selectreports", dataPermissions);
            if (!allowreports.Contains("a") && !allowreports.Contains("u"))
            {
                return null;
            }
            // var types = Assembly.GetCallingAssembly().GetTypes();
             Dictionary<string, string> obj = new Dictionary<string, string>();
            Dictionary<string, string> movement = new Dictionary<string, string>();
            Dictionary<string, string> status = new Dictionary<string, string>();

            Dictionary<string, string> reports = new Dictionary<string, string>();
            Dictionary<string, string> users = new Dictionary<string, string>();
            Dictionary<string, string> locs = new Dictionary<string, string>();
         
           if (Request.Cookies["_id2"] != null)
           {
               Session["_id"] = Request.Cookies["_id2"].Value;
           }
          string iduserd=Session["_id"].ToString();
            List<string> validlocations = new List<string>();
            List<string> locsvalid = new List<string>();
            bool alls = false;
            try
            {
                JObject thisuser = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(iduserd));
                locsvalid = (from loc in thisuser["userLocations"].Children() select (string)loc["id"]).ToList();
                if (locsvalid.Contains("undefined") || locsvalid.Contains("null"))
                {
                    alls = true;

                }
                else
                {


                    JArray getconjuntos = JsonConvert.DeserializeObject<JArray>(locationsdb.GetRowsFilter(locsvalid));
                    validlocations = (from lo in getconjuntos select (string)lo["_id"]).ToList();
                    try
                    {
                        List<string> regions = (from lo in getconjuntos where (string)lo["parent"] == "null" select (string)lo["_id"]).ToList();
                        JArray getchildrenreg = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(regions));
                        List<string> childrenlocs = (from ch in getchildrenreg select (string)ch["_id"]).ToList();
                        validlocations.AddRange(childrenlocs);

                    }
                    catch
                    {

                    }
                }

            }
            catch
            {

            }

            string getobjs = demanddb.GetRows();
            JArray objsja = JsonConvert.DeserializeObject<JArray>(getobjs);
            string getmovement = Movementdb.GetRows();
            JArray movementja = JsonConvert.DeserializeObject<JArray>(getmovement);
            string geusers = Userdb.GetRows();
            JArray userja = JsonConvert.DeserializeObject<JArray>(geusers);
             string gelocp = locationsProfilesdb.GetRows();
            JArray locpja = JsonConvert.DeserializeObject<JArray>(gelocp);
            List<string> profiles=(from prof in locpja.Children() where (string)prof["name"]=="Conjunto" select (string)prof["_id"] ).ToList();
            
            
            string gelocs = locationsdb.GetRowsFilterbyConjunt(profiles);
            try {
            JArray locsja = JsonConvert.DeserializeObject<JArray>(gelocs);
           
           
                foreach (JObject item in locsja)
                {
                    try
                    {
                        if (alls)
                        {
                            locs.Add(item["_id"].ToString(), item["name"].ToString());
                        }
                        else
                        {
                            if (validlocations.Contains(item["_id"].ToString()))
                            {
                                locs.Add(item["_id"].ToString(), item["name"].ToString());
                            }
                        }
                    }
                    catch
                    {

                    }

                }
            
          
            }
            catch (Exception ex) { }
         
            //  string getstatus = demanddb.GetRows();
            //  JArray statusja = JsonConvert.DeserializeObject<JArray>(getstatus);
             try
            {
                users = userja.ToDictionary(x => (string)x["_id"], x => (string)x["user"]);
            }
            catch (Exception ex) { }
         
            try
            {
                obj = objsja.ToDictionary(x => (string)x["_id"], x => (string)x["folio"]);
            }
            catch (Exception ex) { }
            try
            {
            movement = movementja.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
               }
            catch (Exception ex) { }
            /*  foreach (JObject item1 in usersja)
              {
                  users.Add(item1["_id"].ToString(), item1["name"].ToString());
              }
              foreach (JObject item1 in locationja)
              {
                  locations.Add(item1["_id"].ToString(), item1["name"].ToString());
              }
              foreach (JObject item1 in objsja)
              {
                  obj.Add(item1["_id"].ToString(), item1["name"].ToString());
              }
             foreach (JObject item1 in statusja)
              {
                  if (!status.Keys.Contains(item1["status"].ToString()))
                      status.Add(item1["status"].ToString(), item1["status"].ToString());
              }
              foreach (JObject item1 in movementja)
              {
                  movement.Add(item1["_id"].ToString(), item1["name"].ToString());
              }*/
            string idreport = id; string type = "MovementMaster";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;
             ViewData["obj"] = obj;
            ViewData["movement"] = movement;
            ViewData["users"] = users;
            ViewData["locs"] = locs;
            //  ViewData["status"] = status;
            return View();

        }
        public ActionResult MovementHistoryReport(string id, string datestart, string dateend)
        {
            // var types = Assembly.GetCallingAssembly().GetTypes();
            Dictionary<string, string> users = new Dictionary<string, string>();
             Dictionary<string, string> movement = new Dictionary<string, string>();
           
            Dictionary<string, string> reports = new Dictionary<string, string>();
            /*  if (Request.Cookies["_id2"] != null)
           {
               Session["_id"] = Request.Cookies["_id2"].Value;
           }*/
            // string idreport=Session["_id"].ToString();
            string getusers = Userdb.GetRows();
            JArray usersja = JsonConvert.DeserializeObject<JArray>(getusers);
               string getmovement = Movementdb.GetRows();
            JArray movementja = JsonConvert.DeserializeObject<JArray>(getmovement);
            //  string getstatus = demanddb.GetRows();
            //  JArray statusja = JsonConvert.DeserializeObject<JArray>(getstatus);
            users = usersja.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
              movement = movementja.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);

            
              
            string idreport = id; string type = "Custom";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;
            ViewData["user"] = users;
             ViewData["movement"] = movement;
            //  ViewData["status"] = status;
            return View();

        }
    
        public ActionResult CustomReport(string id, string datestart, string dateend)
        {
           // var types = Assembly.GetCallingAssembly().GetTypes();
           Dictionary<string, string> users = new Dictionary<string, string>();
            Dictionary<string, string> locations = new Dictionary<string, string>();
            Dictionary<string, string> obj = new Dictionary<string, string>();
            Dictionary<string, string> movement = new Dictionary<string, string>();
            Dictionary<string, string> status = new Dictionary<string, string>();

            Dictionary<string, string> reports = new Dictionary<string, string>();
            /*  if (Request.Cookies["_id2"] != null)
           {
               Session["_id"] = Request.Cookies["_id2"].Value;
           }*/
            // string idreport=Session["_id"].ToString();
            string getusers = Userdb.GetRows();
            JArray usersja = JsonConvert.DeserializeObject<JArray>(getusers);
            string getlocations = locationsdb.GetRows();
            JArray locationja = JsonConvert.DeserializeObject<JArray>(getlocations);
            string getobjs = Objrefdb.GetRows();
            JArray objsja = JsonConvert.DeserializeObject<JArray>(getobjs);
            string getmovement = Movementdb.GetRows();
            JArray movementja = JsonConvert.DeserializeObject<JArray>(getmovement);
          //  string getstatus = demanddb.GetRows();
          //  JArray statusja = JsonConvert.DeserializeObject<JArray>(getstatus);
             users = usersja.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
             locations = locationja.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
             obj = objsja.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
             movement = movementja.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
              
          /*  foreach (JObject item1 in usersja)
            {
                users.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            foreach (JObject item1 in locationja)
            {
                locations.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            foreach (JObject item1 in objsja)
            {
                obj.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
           foreach (JObject item1 in statusja)
            {
                if (!status.Keys.Contains(item1["status"].ToString()))
                    status.Add(item1["status"].ToString(), item1["status"].ToString());
            }
            foreach (JObject item1 in movementja)
            {
                movement.Add(item1["_id"].ToString(), item1["name"].ToString());
            }*/
            string idreport = id; string type = "Custom";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;
            ViewData["user"] = users;
            ViewData["locations"] = locations;
            ViewData["obj"] = obj;
            ViewData["movement"] = movement;
          //  ViewData["status"] = status;
            return View();

        }
        public ActionResult Index()
        {
           try
          {
              string permission = Session["Permissions"].ToString();
          }
          catch (Exception ex)
          {
              if (Request.Cookies["permissions"] != null)
              {
                  Session["Permissions"] = Request.Cookies["permissions"].Value;

              }

          }
           try
           {
               string permissionclient = Session["PermissionsClient"].ToString();
           }
           catch (Exception ex)
           {
               if (Request.Cookies["permissionsclient"] != null)
               {
                   Session["PermissionsClient"] = Request.Cookies["permissionsclient"].Value;

               }

           }
           String dataPermissions = Session["Permissions"].ToString();
        
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
          //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("reports", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("reports", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                List<string> allowreports = validatepermissions.Tolistpermissions("selectreports", dataPermissions);
                ViewData["allowreports"] = allowreports;
              
                Dictionary<string, string> reports = new Dictionary<string, string>();

                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string idreport = Session["_id"].ToString();

                try
                {
                    string getreports = Reportsdb.GetRowsReports(idreport);
                    JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

                    foreach (JObject item1 in reportsja)
                    {
                        reports.Add(item1["_id"].ToString(), item1["name"].ToString());
                    }
                }
                catch (Exception ex) { }
                ViewData["reports"] = reports;
                return View();
            }
            else
            {
                return Redirect("~/Home");
            }
        }
        public ActionResult MessageReport(string id, string datestart, string dateend)
        {

            return View();
        
        }

        [OutputCache(Duration = 600, VaryByParam = "id")] 
     
        public ActionResult MovementReport(string id, string datestart, string dateend)
        {
            Dictionary<string, string> reports = new Dictionary<string, string>();
                /*  if (Request.Cookies["_id2"] != null)
          {
              Session["_id"] = Request.Cookies["_id2"].Value;
          }*/
            // string idreport=Session["_id"].ToString();
            string idreport = id;
            string type = "Movimientos";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;

            return View();

        }
           [OutputCache(Duration = 600, VaryByParam = "id")] 
     
        public ActionResult ProcessesReport(string id, string datestart, string dateend)
        {
            Dictionary<string, string> reports = new Dictionary<string, string>();
            /*  if (Request.Cookies["_id2"] != null)
           {
               Session["_id"] = Request.Cookies["_id2"].Value;
           }*/
            // string idreport=Session["_id"].ToString();
            string idreport = id; string type = "Procesos";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;

            return View();

        }
           [OutputCache(Duration = 600, VaryByParam = "id")] 
     
        public ActionResult LocationsReport(string id, string datestart, string dateend)
        {
            Dictionary<string, string> reports = new Dictionary<string, string>();
            /*  if (Request.Cookies["_id2"] != null)
          {
              Session["_id"] = Request.Cookies["_id2"].Value;
          }*/
            // string idreport=Session["_id"].ToString();
            string idreport = id;
            string type = "Ubicaciones";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;
            return View();

        }
          [OutputCache(Duration = 600, VaryByParam = "id")] 
     
      
         
     
        public ActionResult ObjectsRefReport(string id, string datestart, string dateend)
        {
            Dictionary<string, string> reports = new Dictionary<string, string>();
            /*  if (Request.Cookies["_id2"] != null)
            {
                Session["_id"] = Request.Cookies["_id2"].Value;
            }*/
            // string idreport=Session["_id"].ToString();
            string idreport = id;
            string type = "Objetos_Ref";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;

            return View();

        }
         // [OutputCache(Duration = 600, VaryByParam = "id")] 

          [OutputCache(Duration = 600, VaryByParam = "id")]

          public ActionResult HwReport(string id, string datestart, string dateend)
          {
              Dictionary<string, string> reports = new Dictionary<string, string>();
              /*  if (Request.Cookies["_id2"] != null)
              {
                  Session["_id"] = Request.Cookies["_id2"].Value;
              }*/
              // string idreport=Session["_id"].ToString();
              string idreport = id; string type = "Hardwares";
              try
              {
                  string getreports = Reportsdb.GetRowsReports(idreport, type);
                  JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

                  foreach (JObject item1 in reportsja)
                  {
                      reports.Add(item1["_id"].ToString(), item1["name"].ToString());
                  }
                  ViewData["reports"] = reports;
              }
              catch { }
              return View();

          }

          public JArray getCatalogLocations(List<string> locations)
          {
              JArray result=new JArray();
              try
              {
                 string idconjuntprof =(JsonConvert.DeserializeObject<JArray>(locationsProfilesdb.Get("name", "Conjunto")).First as JObject)["_id"].ToString() ;
                 string idlocationprof =(JsonConvert.DeserializeObject<JArray>( locationsProfilesdb.Get("name", "Ubicacion")).First as JObject)["_id"].ToString() ;
                 string idsublocationprof = (JsonConvert.DeserializeObject<JArray>(locationsProfilesdb.Get("name", "Sub-Ubicaciones")).First as JObject)["_id"].ToString();
                 JArray alllocations = JsonConvert.DeserializeObject<JArray>(ObjectsRealdb.GetbyCustom("_id", locations, "Locations"));
              //   JArray conjuntsja = (from conj in alllocations where (string)conj["profileId"] == idconjuntprof select conj) as JArray;
             //  JArray locationsja = (from loc in alllocations where (string)loc["profileId"] == idlocationprof select loc) as JArray;
             //  JArray sublocationsja = (from sub in alllocations where (string)sub["profileId"] == idsublocationprof select sub) as JArray;
                 JArray conjuntsja = new JArray();
                  foreach (JObject conjx in alllocations)
                 {
                     try
                     {
                         if (conjx["profileId"].ToString() == idconjuntprof)
                         {
                             conjuntsja.Add(conjx);
                         }
                     }
                     catch { }
                 }
                 foreach (JObject conjitem in conjuntsja)
                 {
                     JObject subrow = new JObject();
                     subrow.Add("id", conjitem["_id"].ToString());
                     subrow.Add("name", conjitem["name"].ToString());
                     try
                     {
                     subrow.Add("number", conjitem["number"].ToString());
                     }
                     catch {
                     subrow.Add("number","NA");
                     }
                     
                     
                     try
                     {

                        
                         try
                         {
                           // JArray filterlocations = (from loc in alllocations where (string)loc["parent"] == conjitem["_id"].ToString() select loc) as JArray;
                             JArray filterlocations = new JArray();
                             foreach (JObject locx in alllocations)
                             {
                                 try
                                 {
                                     if (locx["parent"].ToString() == conjitem["_id"].ToString())
                                     {
                                         filterlocations.Add(locx);
                                     }
                                 }
                                 catch { }
                             }
                             int countlocations = 0;
                            foreach (JObject locitem in filterlocations)
                            {
                                JObject subrowloc = new JObject();
                                subrowloc.Add("id", locitem["_id"].ToString());
                                subrowloc.Add("name", locitem["name"].ToString());
                                try
                                {
                                    subrowloc.Add("number", locitem["number"].ToString());
                                }
                                catch
                                {
                                    subrowloc.Add("number", "NA");
                                }
                     
                     
                                try
                                {
                                  //  JArray filtersublocations = (from sub in alllocations where (string)locitem["parent"] == locitem["_id"].ToString() select sub) as JArray;
                                    JArray filtersublocations = new JArray();
                                    foreach (JObject sublocx in alllocations)
                                    {
                                        try
                                        {
                                            if (sublocx["parent"].ToString() == locitem["_id"].ToString())
                                            {
                                                filtersublocations.Add(sublocx);
                                            }
                                        }
                                        catch { }
                                    }
                                    int countsub = 0;
                                    foreach (JObject subitem in filtersublocations)
                                    {
                                        try
                                        {
                                            JObject row = new JObject();
                                            JObject subrowsub = new JObject();
                                            subrowsub.Add("id", subitem["_id"].ToString());
                                            subrowsub.Add("name", subitem["name"].ToString());
                                            try
                                            {
                                                subrowsub.Add("number", subitem["number"].ToString());
                                            }
                                            catch
                                            {
                                                subrowsub.Add("number", "NA");
                                            }
                                            row.Add("conjunto", subrow);
                                            row.Add("ubicacion", subrowloc);
                                            row.Add("sububicacion", subrowsub);
                                            result.Add(row);
                                            countsub++;
                                            countlocations++;
                                        }
                                        catch { }
                                    }
                                    if (countsub == 0)
                                    {
                                        JObject row = new JObject();
                                        JObject subrowsub = new JObject();
                                        subrowsub.Add("id", "0");
                                        subrowsub.Add("name", "NA");

                                        row.Add("conjunto", subrow);
                                        row.Add("ubicacion", subrowloc);
                                        row.Add("sububicacion", subrowsub);
                                        result.Add(row);
                                        countlocations++;
                                    }
                                }
                                catch { }
                            }

                            if (countlocations == 0)
                            {
                                JObject row = new JObject();
                                JObject subrowsub = new JObject();
                                JObject subrowloc = new JObject();
                                subrowsub.Add("id", "0");
                                subrowsub.Add("name", "NA");
                                subrowloc.Add("id", "0");
                                subrowloc.Add("name", "NA");

                                row.Add("conjunto", subrow);
                                row.Add("ubicacion", subrowloc);
                                row.Add("sububicacion", subrowsub);
                                result.Add(row);
                            }
                         }
                         catch
                         {

                         }
                            
                        

                     }
                     catch
                     {

                     }
                 }
                  
                  
               return result;
              }
              catch
              {
                  return result; 
              }
          }
          public Dictionary<string, string> getLocationsValids()
          {

            try
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();
                List<string> validlocations = new List<string>();
                List<string> locsvalid = new List<string>();
                bool alls = false;
                try
                {
                    JObject thisuser = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(useridx));
                    locsvalid = (from loc in thisuser["userLocations"].Children() select (string)loc["id"]).ToList();
                    if (locsvalid.Contains("undefined") || locsvalid.Contains("null"))
                    {
                        alls = true;

                    }
                    else
                    {


                        JArray getconjuntos = JsonConvert.DeserializeObject<JArray>(locationsdb.GetRowsFilter(locsvalid));
                        validlocations = (from lo in getconjuntos select (string)lo["_id"]).ToList();
                        try
                        {
                            List<string> regions = (from lo in getconjuntos where (string)lo["parent"] == "null" select (string)lo["_id"]).ToList();
                            JArray getchildrenreg = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(regions));
                            List<string> childrenlocs = (from ch in getchildrenreg select (string)ch["_id"]).ToList();
                            validlocations.AddRange(childrenlocs);

                        }
                        catch
                        {

                        }

                    }

                }
                catch
                {
                   
                }

                Dictionary<string, string> profiles = new Dictionary<string, string>();

                string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
                JArray conjuntja = new JArray();
                string idprof = "";
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch { }

                string getprofiles = locationsdb.Get("profileId", idprof);

                JArray profilesja = new JArray();
                try
                {
                    profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                }
                catch (Exception ex) { }

                foreach (JObject item in profilesja)
                {
                    try
                    {
                        if (alls)
                        {
                            profiles.Add(item["_id"].ToString(), item["name"].ToString());
                        }
                        else
                        {
                            if (validlocations.Contains(item["_id"].ToString()))
                            {
                                profiles.Add(item["_id"].ToString(), item["name"].ToString());
                            }
                        }
                    }
                    catch (Exception ex) { continue; }
                }

                return profiles;
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
        
           [OutputCache(Duration = 600, VaryByParam = "id")] 
     
        public ActionResult InventoryReport(string id, string datestart, string dateend)
        {
            Dictionary<string, string> reports = new Dictionary<string, string>();
            /*  if (Request.Cookies["_id2"] != null)
           {
               Session["_id"] = Request.Cookies["_id2"].Value;
           }*/
            // string idreport=Session["_id"].ToString();
            try
            {
                try
                {
                    string permission = Session["Permissions"].ToString();
                }
                catch (Exception ex)
                {
                    if (Request.Cookies["permissions"] != null)
                    {
                        Session["Permissions"] = Request.Cookies["permissions"].Value;

                    }

                }

                String dataPermissions = Session["Permissions"].ToString();
                List<string> allowreports = validatepermissions.Tolistpermissions("selectreports", dataPermissions);
                if (!allowreports.Contains("a") && !allowreports.Contains("d"))
                {
                    return null;
                }
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();
                List<string> validlocations = new List<string>();
                List<string> locsvalid = new List<string>();
                bool alls = false;
                try
                {
                    JObject thisuser = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(useridx));
                    locsvalid = (from loc in thisuser["userLocations"].Children() select (string)loc["id"]).ToList();
                    if (locsvalid.Contains("undefined") || locsvalid.Contains("null"))
                    {
                        alls = true;

                    }
                    else
                    {


                        JArray getconjuntos = JsonConvert.DeserializeObject<JArray>(locationsdb.GetRowsFilter(locsvalid));
                        validlocations = (from lo in getconjuntos select (string)lo["_id"]).ToList();
                        try
                        {
                            List<string> regions = (from lo in getconjuntos where (string)lo["parent"] == "null" select (string)lo["_id"]).ToList();
                            JArray getchildrenreg = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(regions));
                            List<string> childrenlocs = (from ch in getchildrenreg select (string)ch["_id"]).ToList();
                            validlocations.AddRange(childrenlocs);

                        }
                        catch
                        {

                        }

                    }

                }
                catch
                {

                }
                Dictionary<string, string> customfieldsdic = new Dictionary<string, string>();
                Dictionary<string, string> profiles = new Dictionary<string, string>();

                string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
                JArray conjuntja = new JArray();
                string idprof = "";
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch{ }

                string getprofiles = locationsdb.Get("profileId", idprof);

                JArray profilesja = new JArray();
                try
                {
                    profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                }
                catch (Exception ex) { }
                string idreport = id; string type = "Inventarios";
                string getreports = Reportsdb.GetRowsReports(idreport, type);
                JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

                foreach (JObject item1 in reportsja)
                {
                    reports.Add(item1["_id"].ToString(), item1["name"].ToString());
                }
                foreach (JObject item in profilesja)
                {
                    try
                    {
                        if (alls)
                        {
                            profiles.Add(item["_id"].ToString(), item["name"].ToString());
                        }
                        else
                        {
                            if (validlocations.Contains(item["_id"].ToString()))
                            {
                                profiles.Add(item["_id"].ToString(), item["name"].ToString());
                            }
                        }
                    }
                    catch (Exception ex) { continue; }
                }
                ViewData["reports"] = reports;
                ViewData["locations"] = profiles;
                return View();
            }
            catch
            {
                return null;
            }

        }
     

        public string exp(JArray result, string nameprofile, string numtot, Dictionary<string, string> datacols, string[] datesarray, string namereports, string typefilter)
        {
         
            try
            {

                JArray tableja = result;
           string filter = nameprofile;
           int headres = 3;
                if (filter != "") { headres = 4; }
                string numtotal = numtot;
          string namereport = namereports;
                string[] getdates = datesarray;
                Dictionary<string, string> head = datacols;
              
                
                HttpContext context = System.Web.HttpContext.Current;

                System.IO.StringWriter stringWrite = new StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);

               // StringReader reader = new StringReader(inputhtml);

                //Create PDF document
                string file = DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                string pdfurl = Server.MapPath("~") + "\\Uploads\\Reports\\" + file;
                string relativepath = "\\Uploads\\Reports\\";
                string absoluteurl = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absoluteurl))
                {
                    System.IO.Directory.Delete(absoluteurl, true);
                   // System.IO.Directory.CreateDirectory(absoluteurl);
                }
                
                if (!System.IO.Directory.Exists(absoluteurl))
                {
                    
                    System.IO.Directory.CreateDirectory(absoluteurl);
                }
                Document doc = new Document(PageSize.A4);
                HTMLWorker parser = new HTMLWorker(doc);
                try
                {

                PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Reports/" + file,
                    
                FileMode.Create));
                }
                catch (Exception ex)
                {
                    if (!System.IO.Directory.Exists(absoluteurl))
                    {

                        System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Reports/" + file,

               FileMode.Create));
              
                }
                doc.Open();
                iTextSharp.text.Image imgpfd = iTextSharp.text.Image.GetInstance(Server.MapPath("~") + "/Content/Images/newLogoLogin.png"); //Dirreccion a la imagen que se hace referencia
               // imgpfd.SetAbsolutePosition(0,0); //Posicion en el eje carteciano de X y Y
                imgpfd.ScaleAbsolute(100, 50);//Ancho y altura de la imagen
                doc.Add(imgpfd);
                doc.Add(new Paragraph("\n"));
                Paragraph paragraph = new Paragraph();
                  paragraph.Alignment = Element.ALIGN_RIGHT;
                  paragraph.Font = FontFactory.GetFont("Arial", 9);
                  paragraph.Add("");
                /********************************************************************************/
                var interfaceProps = new Dictionary<string, Object>();
                //    var ih = new ImageHander() { BaseUri = Request.Url.ToString() };

                //  interfaceProps.Add(HTMLWorker.IMG_PROVIDER, ih);
                // table resumen
                PdfPTable tableres = new PdfPTable(headres);
                PdfPCell cellres = new PdfPCell(new Phrase("Resumen"));
                cellres.Colspan = headres;
                cellres.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                tableres.AddCell(cellres);


                PdfPCell cellhres = new PdfPCell(new Phrase("#" + namereport));
                cellhres.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
                 tableres.AddCell(cellhres);
                if (filter != "")
                {
                 cellhres = new PdfPCell(new Phrase(typefilter));
                 cellhres.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
                 tableres.AddCell(cellhres);
                }
                cellhres = new PdfPCell(new Phrase("Fecha Inicial"));
                cellhres.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
                 tableres.AddCell(cellhres);
                 cellhres = new PdfPCell(new Phrase("Fecha Final"));
                 cellhres.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
                 tableres.AddCell(cellhres);

                  
                tableres.AddCell(numtotal);
                if (filter != "") { tableres.AddCell(filter); }
                tableres.AddCell(getdates[0]);
                tableres.AddCell(getdates[1]);
             
                doc.Add(tableres);
                
                //table report
                PdfPTable table = new PdfPTable(head.Count());
                PdfPCell cell = new PdfPCell(new Phrase("Reporte de " + namereport));
                cell.Colspan = head.Count();
                cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                table.AddCell(cell);

                foreach (var th in head)
                {
                    PdfPCell cellh = new PdfPCell(new Phrase(th.Value));
                    cellh.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
                   
                    table.AddCell(cellh);

                }
                foreach (JObject item in tableja)
                {
                   
                     foreach (var x in head)
                    {
                      

                            table.AddCell(item[x.Key].ToString());
                       
                       
                    }
                }
                doc.Add(new Paragraph("\n"));
                doc.Add(table);
             /*   foreach (IElement element in HTMLWorker.ParseToList(
                new StringReader(inputhtml), null))
                {
                    doc.Add(element);
                }*/
                doc.Close();

                System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Reports/" + file);


             //  Downloadpdf(file);
                return toDownload.Name;
              //  Response.End();
                //SPFile file = listItem.File;
               
              
              //  Response.TransmitFile(Server.MapPath("~") + "/App_Data/" + file);
              //  Response.Redirect(Server.MapPath("~") + "/App_Data/" + file);
                // Write the file to the Response
               
             //   Response.Flush();
             //    Response.End();
           /*     WebClient myClient = new WebClient();
                string basefile = Path.GetFileName(file);
                myClient.DownloadFile(url, file);*/

               //  File.Delete(myfile.FullName);
              //  System.IO.File.Delete(Server.MapPath("~") + "/App_Data/" + file);
               
               
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public string exp1fail(String result, String namefilter, String numtot, String datacols, String datesarray, string namereports, string typefilters, string graph, int rotate = 0)
        {

            try
            {

                /*   byte[] toEncodeAsBytes= System.Text.ASCIIEncoding.ASCII.GetBytes(graph);
                   string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
                  // byte[] bytes = Convert.FromBase64String(returnValue);*/
                string results = result.ToString();
                JArray resultja = JsonConvert.DeserializeObject<JArray>(results);

                string cols = datacols.ToString();
                JArray colsja = JsonConvert.DeserializeObject<JArray>(cols);

                string datesx = datesarray.ToString();
                JObject datesja = JsonConvert.DeserializeObject<JObject>(datesx);
                List<string> listdatesx = new List<string>();

                listdatesx.Add(datesja["start"].ToString());
                listdatesx.Add(datesja["end"].ToString());

                System.Drawing.Image bmpReturn = null;

                var base64Data = Regex.Match(graph, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                var binData = Convert.FromBase64String(base64Data);
                var stream = new MemoryStream(binData);


                bmpReturn = System.Drawing.Image.FromStream(stream);

                JArray tableja = resultja;

                string filter = "";
                try { filter = namefilter; }
                catch (Exception ex) { };
                int headres = 3;
                if (filter != "") { headres = 4; }
                string numtotal = numtot;
                string namereport = namereports;
                string[] getdates = listdatesx.ToArray();
                // Dictionary<string, string> head =(Dictionary<string,string>) Session["headuser"];
                Dictionary<string, string> head = new Dictionary<string, string>();
                JObject getfirst = tableja.First() as JObject;

                foreach (JObject x in colsja)
                {
                    try
                    {
                        JToken jt;
                        try
                        {
                            if (x["key"].ToString().ToLower() == "detalle")
                            {
                                continue;
                            }
                        }
                        catch { }
                        if (getfirst.TryGetValue(x["key"].ToString(), out jt))
                        {
                            if (getfirst[x["key"].ToString()].GetType().Name.ToString() == "JValue")
                                head.Add(x["key"].ToString(), x["value"].ToString());
                        }
                        else
                        {
                            head.Add(x["key"].ToString(), x["value"].ToString());
                        }
                    }
                    catch { }
                }
                string typefilter = typefilters;
                HttpContext context = System.Web.HttpContext.Current;

                System.IO.StringWriter stringWrite = new StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);

                // StringReader reader = new StringReader(inputhtml);

                //Create PDF document
                string file = DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                string pdfurl = Server.MapPath("~") + "\\Uploads\\Reports\\" + file;
                string relativepath = "\\Uploads\\Reports\\";
                string absoluteurl = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absoluteurl))
                {
                    try
                    {
                        System.IO.Directory.Delete(absoluteurl, true);
                    }
                    catch (Exception ex) { }
                    // System.IO.Directory.CreateDirectory(absoluteurl);
                }

                if (!System.IO.Directory.Exists(absoluteurl))
                {

                    System.IO.Directory.CreateDirectory(absoluteurl);
                }
                Document doc = new Document(PageSize.A4);
                if (rotate == 1)
                {
                    //  doc = new Document(PageSize.LEGAL.Rotate());
                    var pgSize = new iTextSharp.text.Rectangle(3000, 2000);
                    doc = new Document(pgSize.Rotate());
                }

                //doc.SetPageSize(PageSize.A4.Rotate());
                HTMLWorker parser = new HTMLWorker(doc);
                try
                {

                    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Reports/" + file,

                    FileMode.Create));
                }
                catch (Exception ex)
                {
                    if (!System.IO.Directory.Exists(absoluteurl))
                    {

                        System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Reports/" + file,

               FileMode.Create));

                }
                doc.Open();
                iTextSharp.text.Image imgpfd = iTextSharp.text.Image.GetInstance(Server.MapPath("~") + "/Uploads/Images/Design/Logo/52e95ab907719e0d40637d96635711672855466839.png"); //Dirreccion a la imagen que se hace referencia
                // imgpfd.SetAbsolutePosition(0,0); //Posicion en el eje carteciano de X y Y
                imgpfd.ScaleAbsolute(100, 50);//Ancho y altura de la imagen
                doc.Add(imgpfd);
                doc.Add(new Paragraph("\n"));
                Paragraph paragraph = new Paragraph();
                paragraph.Alignment = Element.ALIGN_RIGHT;
                paragraph.Font = FontFactory.GetFont("Arial", 9);
                paragraph.Add("");
                /********************************************************************************/
                var interfaceProps = new Dictionary<string, Object>();
                //    var ih = new ImageHander() { BaseUri = Request.Url.ToString() };

                //  interfaceProps.Add(HTMLWorker.IMG_PROVIDER, ih);
                // table resumen
                PdfPTable tableres = new PdfPTable(headres);
                if (rotate == 1)
                    tableres.WidthPercentage = 100;
                var TextFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Color.WHITE);
                var TextHeads = FontFactory.GetFont("Arial", 12, iTextSharp.text.Color.WHITE);
                var TextRow = FontFactory.GetFont("Arial", 10, iTextSharp.text.Color.BLACK);
                var Textdate = FontFactory.GetFont("Arial", 10, iTextSharp.text.Color.BLACK);

                int border = 0;
                var titleResumen = new Chunk("Resumen", TextFont);


                PdfPCell cellres = new PdfPCell(new Phrase(titleResumen));
                cellres.BackgroundColor = new iTextSharp.text.Color(24, 116, 205);

                cellres.Colspan = headres;
                cellres.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                tableres.AddCell(cellres);

                var headtitle = new Chunk("#" + namereport, TextHeads);
                PdfPCell cellhres = new PdfPCell(new Phrase(headtitle));
                cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);
                cellhres.BorderWidthRight = 0;
                tableres.AddCell(cellhres);
                if (filter != "")
                {
                    headtitle = new Chunk(typefilter, TextHeads);

                    cellhres = new PdfPCell(new Phrase(headtitle));
                    cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);

                    cellhres.BorderWidthRight = 0;
                    cellhres.BorderWidthLeft = 0;
                    tableres.AddCell(cellhres);
                }
                headtitle = new Chunk("Fecha Inicial", TextHeads);

                cellhres = new PdfPCell(new Phrase(headtitle));
                cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);

                cellhres.BorderWidthRight = 0;
                cellhres.BorderWidthLeft = 0;
                tableres.AddCell(cellhres);
                headtitle = new Chunk("Fecha Final", TextHeads);

                cellhres = new PdfPCell(new Phrase(headtitle));
                cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);

                cellhres.BorderWidthLeft = 0;

                tableres.AddCell(cellhres);

                headtitle = new Chunk(numtotal, TextRow);
                cellhres = new PdfPCell(new Phrase(headtitle));
                cellhres.BorderWidthRight = 0;

                tableres.AddCell(cellhres);
                if (filter != "")
                {

                    headtitle = new Chunk(filter, TextRow);
                    cellhres = new PdfPCell(new Phrase(headtitle));
                    cellhres.BorderWidthRight = 0;
                    cellhres.BorderWidthLeft = 0;
                    tableres.AddCell(cellhres);
                }
                headtitle = new Chunk(getdates[0], TextRow);
                cellhres = new PdfPCell(new Phrase(headtitle));

                cellhres.BorderWidthRight = 0;
                cellhres.BorderWidthLeft = 0;
                tableres.AddCell(cellhres);
                headtitle = new Chunk(getdates[1], TextRow);
                cellhres = new PdfPCell(new Phrase(headtitle));
                cellhres.BorderWidthLeft = 0;
                tableres.AddCell(cellhres);


                doc.Add(tableres);
                doc.Add(new Paragraph("\n"));

                var datetime = new Chunk("                   Generado el : " + DateTime.Now.ToString("dd/MM/yyyy"), Textdate);

                doc.Add(new Paragraph(datetime));
                //table report
                var titlereport = new Chunk("Reporte de " + namereport, TextFont);

                PdfPTable table = new PdfPTable(head.Count());
                if (rotate == 1)
                    table.WidthPercentage = 100;
                PdfPCell cell = new PdfPCell(new Phrase(titlereport));
                cell.BackgroundColor = new iTextSharp.text.Color(24, 116, 205);
                cell.Colspan = head.Count();
                cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                table.AddCell(cell);

                foreach (var th in head)
                {



                    headtitle = new Chunk(th.Value, TextHeads);

                    PdfPCell cellh = new PdfPCell(new Phrase(headtitle));
                    cellh.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);

                    if (border == 0)
                    {
                        cellh.BorderWidthRight = 0;



                    }
                    else
                    {

                        if (border + 1 < head.Count())
                        {
                            cellh.BorderWidthLeft = 0;
                            cellh.BorderWidthRight = 0;
                        }
                        else
                        {
                            cellh.BorderWidthLeft = 0;

                        }
                    }
                    table.AddCell(cellh);
                    border++;

                }

                int rowcolor = 0;
                int index = 0;
                foreach (JObject item in tableja)
                {
                    border = 0;
                    index = 0;
                    foreach (var x in head)
                    {
                        try
                        {
                            var rowtitle = new Chunk("", TextRow);
                            try
                            {
                                if (item[x.Key].GetType().Name.ToString() == "JValue")
                                    rowtitle = new Chunk(item[x.Key].ToString(), TextRow);
                            }
                            catch (Exception ex)
                            {

                            }

                            PdfPCell cellrow = new PdfPCell(new Phrase(rowtitle));
                            if ((rowcolor % 2) == 0)
                            {
                                cellrow.BackgroundColor = new iTextSharp.text.Color(201, 201, 201);

                            }
                            else
                            {


                            }
                            if (border == 0)
                            {
                                cellrow.BorderWidthRight = 0;
                                cellrow.BorderWidthTop = 0;
                                cellrow.BorderWidthBottom = 0;

                                border++;
                            }
                            else
                            {

                                if (index + 1 < head.Count())
                                {
                                    cellrow.Border = 0;
                                }
                                else
                                {
                                    cellrow.BorderWidthLeft = 0;
                                    cellrow.BorderWidthTop = 0;
                                    cellrow.BorderWidthBottom = 0;
                                }


                            }

                            if (rowcolor + 1 == tableja.Count())
                            {
                                cellrow.BorderWidthBottom = 1;
                            }
                            table.AddCell(cellrow);

                            index++;
                        }
                        catch (Exception ex) { continue; }
                    }
                    rowcolor++;

                }
                doc.Add(new Paragraph("\n"));
                doc.Add(table);
                /*   foreach (IElement element in HTMLWorker.ParseToList(
                   new StringReader(inputhtml), null))
                   {
                       doc.Add(element);
                   }*/
                iTextSharp.text.Image imggraph = iTextSharp.text.Image.GetInstance(bmpReturn, iTextSharp.text.Color.WHITE, false); //Dirreccion a la imagen que se hace referencia
                // imgpfd.SetAbsolutePosition(0,0); //Posicion en el eje carteciano de X y Y
                imggraph.ScaleAbsolute(500, 300);//Ancho y altura de la imagen
                imggraph.Alignment = 1;
                doc.Add(imggraph);
                // doc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                doc.Close();

                System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Reports/" + file);


                //  Downloadpdf(file);
                return toDownload.Name;
                //  Response.End();
                //SPFile file = listItem.File;


                //  Response.TransmitFile(Server.MapPath("~") + "/App_Data/" + file);
                //  Response.Redirect(Server.MapPath("~") + "/App_Data/" + file);
                // Write the file to the Response

                //   Response.Flush();
                //    Response.End();
                /*     WebClient myClient = new WebClient();
                     string basefile = Path.GetFileName(file);
                     myClient.DownloadFile(url, file);*/

                //  File.Delete(myfile.FullName);
                //  System.IO.File.Delete(Server.MapPath("~") + "/App_Data/" + file);


            }
            catch (Exception ex)
            {
                return null;
            }

        }
       
        public string exp1(String result, String namefilter, String numtot, String datacols, String datesarray, string namereports, string typefilters, string graph,int rotate=0)
        {

            try
            {

             /*   byte[] toEncodeAsBytes= System.Text.ASCIIEncoding.ASCII.GetBytes(graph);
                string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
               // byte[] bytes = Convert.FromBase64String(returnValue);*/
                string results = result.ToString();
                JArray resultja = JsonConvert.DeserializeObject<JArray>(results);

                string cols = datacols.ToString();
                JArray colsja = JsonConvert.DeserializeObject<JArray>(cols);

                string datesx = datesarray.ToString();
                JObject datesja = JsonConvert.DeserializeObject<JObject>(datesx);
                List<string> listdatesx = new List<string>();
               
                    listdatesx.Add(datesja["start"].ToString());
                      listdatesx.Add(datesja["end"].ToString());
               
                System.Drawing.Image bmpReturn = null;

             var base64Data = Regex.Match(graph, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
             var binData = Convert.FromBase64String(base64Data);
             var stream = new MemoryStream(binData);
           
               
             bmpReturn = System.Drawing.Image.FromStream(stream);

                JArray tableja = resultja;

                string filter = "";
                try { filter = namefilter; }
                catch (Exception ex) { };
                int headres = 3;
                if (filter != "") { headres = 4; }
                string numtotal = numtot;
                string namereport = namereports;
                string[] getdates = listdatesx.ToArray();
               // Dictionary<string, string> head =(Dictionary<string,string>) Session["headuser"];
                Dictionary<string, string> head = new Dictionary<string, string>();
                JObject getfirst = tableja.First() as JObject;
                foreach (JObject x in colsja)
                {
                    try
                    {
                        JToken jt;
                        if (getfirst.TryGetValue(x["key"].ToString(), out jt))
                        {
                            if (getfirst[x["key"].ToString()].GetType().Name.ToString() == "JValue")
                                head.Add(x["key"].ToString(), x["value"].ToString());
                        }
                        else
                        {
                            head.Add(x["key"].ToString(), x["value"].ToString());
                        }
                    }
                    catch { }
                }
                string typefilter = typefilters;
                HttpContext context = System.Web.HttpContext.Current;

                System.IO.StringWriter stringWrite = new StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);

                // StringReader reader = new StringReader(inputhtml);

                //Create PDF document
                string file = DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                string pdfurl = Server.MapPath("~") + "\\Uploads\\Reports\\" + file;
                string relativepath = "\\Uploads\\Reports\\";
                string absoluteurl = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absoluteurl))
                {
                    try
                    {
                        System.IO.Directory.Delete(absoluteurl, true);
                    }
                    catch (Exception ex) { }
                    // System.IO.Directory.CreateDirectory(absoluteurl);
                }

                if (!System.IO.Directory.Exists(absoluteurl))
                {

                    System.IO.Directory.CreateDirectory(absoluteurl);
                }
                Document doc = new Document(PageSize.A4);
                if (rotate == 1)
                {
                    //  doc = new Document(PageSize.LEGAL.Rotate());
                    var pgSize = new iTextSharp.text.Rectangle(3000, 2000);
                    doc = new Document(pgSize.Rotate());
                }

                HTMLWorker parser = new HTMLWorker(doc);
                try
                {

                    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Reports/" + file,

                    FileMode.Create));
                }
                catch (Exception ex)
                {
                    if (!System.IO.Directory.Exists(absoluteurl))
                    {

                        System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Reports/" + file,

               FileMode.Create));

                }
                doc.Open();
                iTextSharp.text.Image imgpfd = iTextSharp.text.Image.GetInstance(Server.MapPath("~") + "/Content/Images/newLogoLogin.png"); //Dirreccion a la imagen que se hace referencia
                // imgpfd.SetAbsolutePosition(0,0); //Posicion en el eje carteciano de X y Y
                imgpfd.ScaleAbsolute(100, 50);//Ancho y altura de la imagen
                doc.Add(imgpfd);
                doc.Add(new Paragraph("\n"));
                Paragraph paragraph = new Paragraph();
                paragraph.Alignment = Element.ALIGN_RIGHT;
                paragraph.Font = FontFactory.GetFont("Arial", 9);
                paragraph.Add("");
                /********************************************************************************/
                var interfaceProps = new Dictionary<string, Object>();
                //    var ih = new ImageHander() { BaseUri = Request.Url.ToString() };

                //  interfaceProps.Add(HTMLWorker.IMG_PROVIDER, ih);
                // table resumen
                PdfPTable tableres = new PdfPTable(headres);
                var TextFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Color.WHITE);
                var TextHeads = FontFactory.GetFont("Arial", 12, iTextSharp.text.Color.WHITE);
                var TextRow = FontFactory.GetFont("Arial", 10, iTextSharp.text.Color.BLACK);
                var Textdate = FontFactory.GetFont("Arial", 10, iTextSharp.text.Color.BLACK);
               
                int border = 0;
                var titleResumen = new Chunk("Resumen", TextFont);
               
                
                PdfPCell cellres = new PdfPCell(new Phrase(titleResumen));
                cellres.BackgroundColor = new iTextSharp.text.Color(24, 116, 205);
               
                cellres.Colspan = headres;
                cellres.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                tableres.AddCell(cellres);

                var headtitle = new Chunk("#" + namereport, TextHeads);
               PdfPCell cellhres = new PdfPCell(new Phrase(headtitle));
               cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);
               cellhres.BorderWidthRight = 0;
                tableres.AddCell(cellhres);
                if (filter != "")
                {
                    headtitle = new Chunk(typefilter, TextHeads);
           
                    cellhres = new PdfPCell(new Phrase(headtitle));
                    cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);
                   
                    cellhres.BorderWidthRight = 0;
                    cellhres.BorderWidthLeft = 0;
                    tableres.AddCell(cellhres);
                }
                headtitle = new Chunk("Fecha Inicial", TextHeads);
           
                cellhres = new PdfPCell(new Phrase(headtitle));
                cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);
               
                cellhres.BorderWidthRight = 0;
                cellhres.BorderWidthLeft = 0;
                tableres.AddCell(cellhres);
                headtitle = new Chunk("Fecha Final", TextHeads);
           
                cellhres = new PdfPCell(new Phrase(headtitle));
                cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);
               
                cellhres.BorderWidthLeft = 0;
            
                tableres.AddCell(cellhres);

                headtitle = new Chunk(numtotal, TextRow);
               cellhres = new PdfPCell(new Phrase(headtitle));
               cellhres.BorderWidthRight = 0;
              
               tableres.AddCell(cellhres);
                if (filter != "")
                {

                    headtitle = new Chunk(filter, TextRow);
                    cellhres = new PdfPCell(new Phrase(headtitle));
                    cellhres.BorderWidthRight = 0;
                    cellhres.BorderWidthLeft = 0;
                    tableres.AddCell(cellhres);
                    }
                headtitle = new Chunk(getdates[0], TextRow);
                cellhres = new PdfPCell(new Phrase(headtitle));
               
                cellhres.BorderWidthRight = 0;
                cellhres.BorderWidthLeft = 0;
                tableres.AddCell(cellhres);
                headtitle = new Chunk(getdates[1], TextRow);
                cellhres = new PdfPCell(new Phrase(headtitle));
                  cellhres.BorderWidthLeft = 0;
                tableres.AddCell(cellhres);
                
               
                doc.Add(tableres);
                doc.Add(new Paragraph("\n"));
              
                var datetime = new Chunk("                   Generado el : " + DateTime.Now.ToString("dd/MM/yyyy"), Textdate);

                doc.Add(new Paragraph(datetime));
                //table report
                var titlereport = new Chunk("Reporte de " + namereport, TextFont);
              
                PdfPTable table = new PdfPTable(head.Count());
            
                PdfPCell cell = new PdfPCell(new Phrase(titlereport));
                cell.BackgroundColor = new iTextSharp.text.Color(24, 116, 205);
                cell.Colspan = head.Count();
                cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                table.AddCell(cell);
               
                foreach (var th in head)
                {
                   

                   
                    headtitle = new Chunk(th.Value, TextHeads);
           
                    PdfPCell cellh = new PdfPCell(new Phrase(headtitle));
                    cellh.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);

                    if (border == 0)
                    {
                        cellh.BorderWidthRight = 0;


                       
                    }
                    else
                    {

                        if (border + 1 < head.Count())
                        {
                            cellh.BorderWidthLeft = 0;
                            cellh.BorderWidthRight = 0;
                        }
                        else
                        {
                            cellh.BorderWidthLeft = 0;
                           
                        }
                    }
                    table.AddCell(cellh);
                    border++;
                    
                }
               
               int rowcolor = 0;
                int index = 0;
                foreach (JObject item in tableja)
                {
                    border = 0;
                    index = 0;
                    foreach (var x in head)
                    {
                        try
                        {
                            var rowtitle = new Chunk("", TextRow); 
                            try
                            {
                                if(item[x.Key].GetType().Name.ToString()=="JValue")
                                 rowtitle = new Chunk(item[x.Key].ToString(), TextRow);
                            }
                            catch (Exception ex)
                            {

                            }

                        PdfPCell cellrow = new PdfPCell(new Phrase(rowtitle));
                        if ((rowcolor % 2) == 0)
                        {
                            cellrow.BackgroundColor = new iTextSharp.text.Color(201, 201, 201);
                           
                        }
                        else
                        {
                           

                        }
                        if (border == 0)
                        {
                            cellrow.BorderWidthRight = 0;
                                cellrow.BorderWidthTop = 0;
                            cellrow.BorderWidthBottom = 0;
                          
                            border++;
                        }
                        else
                        {

                                if (index + 1 < head.Count())
                                {
                                cellrow.Border = 0;
                            }
                            else
                            {
                                cellrow.BorderWidthLeft = 0;
                                cellrow.BorderWidthTop = 0;
                                cellrow.BorderWidthBottom = 0;
                            }
                           

                        }

                            if (rowcolor + 1 == tableja.Count())
                        {
                            cellrow.BorderWidthBottom = 1;
                        }
                        table.AddCell(cellrow);

                       index++;
                    }
                        catch (Exception ex) { continue; }
                    }
                    rowcolor++;
                    
                }
                doc.Add(new Paragraph("\n"));
                doc.Add(table);
                /*   foreach (IElement element in HTMLWorker.ParseToList(
                   new StringReader(inputhtml), null))
                   {
                       doc.Add(element);
                   }*/
                iTextSharp.text.Image imggraph = iTextSharp.text.Image.GetInstance(bmpReturn, iTextSharp.text.Color.WHITE, false); //Dirreccion a la imagen que se hace referencia
                // imgpfd.SetAbsolutePosition(0,0); //Posicion en el eje carteciano de X y Y
                imggraph.ScaleAbsolute(500, 300);//Ancho y altura de la imagen
                 imggraph.Alignment = 1;
                doc.Add(imggraph);
               
               
                doc.Close();

                System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Reports/" + file);


                //  Downloadpdf(file);
                return toDownload.Name;
                //  Response.End();
                //SPFile file = listItem.File;


                //  Response.TransmitFile(Server.MapPath("~") + "/App_Data/" + file);
                //  Response.Redirect(Server.MapPath("~") + "/App_Data/" + file);
                // Write the file to the Response

                //   Response.Flush();
                //    Response.End();
                /*     WebClient myClient = new WebClient();
                     string basefile = Path.GetFileName(file);
                     myClient.DownloadFile(url, file);*/

                //  File.Delete(myfile.FullName);
                //  System.IO.File.Delete(Server.MapPath("~") + "/App_Data/" + file);


            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public string ExportDataSet(String result, String namefilter, String numtot, String datacols, String datesarray, string namereports, string typefilters, string graph)
        {

            try
            {

                string results = result.ToString();
                JArray resultja = JsonConvert.DeserializeObject<JArray>(results);

                string cols = datacols.ToString();
                JArray colsja = JsonConvert.DeserializeObject<JArray>(cols);

                string datesx = datesarray.ToString();
                JObject datesja = JsonConvert.DeserializeObject<JObject>(datesx);
                List<string> listdatesx = new List<string>();

                listdatesx.Add(datesja["start"].ToString());
                listdatesx.Add(datesja["end"].ToString());

                System.Drawing.Image bmpReturn = null;

                var base64Data = Regex.Match(graph, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                var binData = Convert.FromBase64String(base64Data);
                var stream = new MemoryStream(binData);


                bmpReturn = System.Drawing.Image.FromStream(stream);

                JArray tableja = resultja;

                string filter = "";
                try { filter = namefilter; }
                catch (Exception ex) { };
                int headres = 3;
                if (filter != "") { headres = 4; }
                string numtotal = numtot;
                string namereport = namereports;
                string[] getdates = listdatesx.ToArray();
                // Dictionary<string, string> head =(Dictionary<string,string>) Session["headuser"];
                Dictionary<string, string> head = new Dictionary<string, string>();

                JObject getfirst = tableja.First() as JObject;
                for (int i = 0; i < colsja.Count(); i++)
                {
                    try
                    {
                        JToken jt;
                        try
                        {
                            if (colsja[i]["key"].ToString().ToLower() == "detalle")
                            {
                                continue;
                            }
                        }
                        catch { }
                        if (getfirst.TryGetValue(colsja[i]["key"].ToString(), out jt))
                        {
                            if (getfirst[colsja[i]["key"].ToString()].GetType().Name.ToString() == "JValue")
                                head.Add(colsja[i]["key"].ToString(), colsja[i]["value"].ToString());
                        }
                        else
                        {
                            head.Add(colsja[i]["key"].ToString(), colsja[i]["value"].ToString());
                        }
                    }
                    catch { }
                }
                //foreach (JObject x in colsja)
                //{

                //}
                string typefilter = typefilters;
                HttpContext context = System.Web.HttpContext.Current;

                System.IO.StringWriter stringWrite = new StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);

                // StringReader reader = new StringReader(inputhtml);

                //Create PDF document
                string file = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                string file1 = DateTime.Now.ToString("yyyyMMddHHmmss") + "test.xlsx";

                string pdfurl = Server.MapPath("~") + "\\Uploads\\Reports\\" + file;
                string pdfurl1 = Server.MapPath("~") + "\\Uploads\\Reports\\" + file1;

                string relativepath = "\\Uploads\\Reports\\";
                string absoluteurl = Server.MapPath(relativepath);
                float[] widths = new float[] { 1f, 2f };
                if (System.IO.Directory.Exists(absoluteurl))
                {
                    try
                    {
                        //  System.IO.Directory.Delete(absoluteurl, true);
                        // System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (!System.IO.Directory.Exists(absoluteurl))
                {

                    System.IO.Directory.CreateDirectory(absoluteurl);
                }



                Exceldoc = new CreateExcelDoc();

                int row = 2;
                List<string> headja = new List<string>();
                headja.Add(namereport);
                if (filter != "")
                    headja.Add(typefilter);

                headja.Add("fecha Inicial");
                headja.Add("fecha Final");
                Dictionary<int, string> dictindex = generateDict();

                for (int i = 0; i < headja.Count(); i++)
                {
                    try
                    {
                        int colindex = headja.IndexOf(headja[i]) + 1;
                        Exceldoc.createHeaders(row, colindex, headja[i].ToString(), String.Format("{0}{1}", dictindex[colindex], row), String.Format("{0}{1}", dictindex[colindex], row), 1, "FB", true, 60, "");
                    }
                    catch
                    {

                    }
                }
                //foreach (string head2 in headja)
                //{

                //}
                row++;

                try
                {
                    int colindex = 1;
                    List<string> dataset = new List<string>();
                    dataset.Add(numtotal.ToString());
                    if (filter != "")
                        dataset.Add(filter);
                    dataset.Add(getdates[0]);
                    dataset.Add(getdates[1]);

                    for (int i = 0; i < dataset.Count(); i++)
                    {
                        try
                        {

                            Exceldoc.addData(row, colindex, dataset[i].ToString(), String.Format("{0}{1}", dictindex[colindex], row), String.Format("{0}{1}", dictindex[colindex], row), "#,##0");

                        }
                        catch { }
                        colindex++;

                    }
                    //foreach (var col in dataset)
                    //{

                    //}
                    row++;
                }
                catch { row++; }


                row++;
                int headcount = 1;

                foreach (var x in head)
                {
                    try
                    {
                        int colindex3 = headcount;
                        Exceldoc.createHeaders(row, colindex3, x.Value.ToString(), String.Format("{0}{1}", dictindex[colindex3], row), String.Format("{0}{1}", dictindex[colindex3], row), 2, "FB", true, 60, "");
                    }
                    catch
                    {

                    }
                    headcount++;
                };
                row++;

                // table report   

                int minrow = row;
                int mincol = 1;
                int maxrow = row;
                int maxcol = head.Count();
                List<List<string>> bilist = new List<List<string>>();
                int indexarr = 0;
                for (int i = 0; i < tableja.Count(); i++)
                {
                    int colindex2 = 1;
                    List<string> listdat = new List<string>();
                    int r = 0;
                    foreach (var col in head)
                    {
                        try
                        {
                            // Exceldoc.addData(row, colindex2, item[col.Key].ToString(), String.Format("{0}{1}", dictindex[colindex2], row), String.Format("{0}{1}", dictindex[colindex2], row), "#,##0");
                            listdat.Add(tableja[i][col.Key].ToString());

                        }
                        catch { listdat.Add(""); }
                        colindex2++;
                    }
                    bilist.Add(listdat);
                    row++;

                }
                //foreach (var item in tableja)
                //{

                //}

                string[,] biarray = new string[bilist.Count(), head.Count()];
                int w = 0;
                for (int i = 0; i < bilist.Count(); i++)
                {
                    int z = 0;
                    try
                    {
                        for (int j = 0; j < bilist[i].Count(); j++)
                        {
                            try
                            {
                                biarray[w, z] = bilist[i][j];
                            }
                            catch
                            {

                            }
                            z++;
                        }
                    }
                    catch
                    {

                    }
                    w++;
                }
                //foreach (List<string> lis in bilist)
                //{

                //}
                maxrow = row;
                row++;

                Exceldoc.addFast(minrow, mincol, maxrow, maxcol, biarray);



                string filex = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                string relativepathx = "\\Uploads\\Reports\\";
                string absoluteurlx = Server.MapPath(relativepathx);
                string excelurlx = absoluteurl + file;
                Exceldoc.SetPicture();
                string resultx = Exceldoc.saveDoc(excelurlx, absoluteurlx);


                return resultx.Split('\\').Last();

                //  Downloadpdf(file);

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
       
        public string ExportDataSetold(String result, String namefilter, String numtot, String datacols, String datesarray, string namereports, string typefilters, string graph)
        {



            string results = result.ToString();
            JArray resultja = JsonConvert.DeserializeObject<JArray>(results);

            string cols = datacols.ToString();
            JArray colsja = JsonConvert.DeserializeObject<JArray>(cols);

            string datesx = datesarray.ToString();
            JObject datesja = JsonConvert.DeserializeObject<JObject>(datesx);
            List<string> listdatesx = new List<string>();

            listdatesx.Add(datesja["start"].ToString());
            listdatesx.Add(datesja["end"].ToString());

            System.Drawing.Image bmpReturn = null;

            var base64Data = Regex.Match(graph, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            var binData = Convert.FromBase64String(base64Data);
            var stream = new MemoryStream(binData);


            bmpReturn = System.Drawing.Image.FromStream(stream);

            JArray tableja = resultja;

            string filter = "";
            try { filter = namefilter; }
            catch (Exception ex) { };
            int headres = 3;
            if (filter != "") { headres = 4; }
            string numtotal = numtot;
            string namereport = namereports;
            string[] getdates = listdatesx.ToArray();
            // Dictionary<string, string> head =(Dictionary<string,string>) Session["headuser"];
            Dictionary<string, string> head = new Dictionary<string, string>();

            foreach (JObject x in colsja)
            {
                head.Add(x["key"].ToString(), x["value"].ToString());
            }
            string typefilter = typefilters;
            HttpContext context = System.Web.HttpContext.Current;

            System.IO.StringWriter stringWrite = new StringWriter();
            System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);

            // StringReader reader = new StringReader(inputhtml);

            //Create PDF document
            string file = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string file1 = DateTime.Now.ToString("yyyyMMddHHmmss") + "test.xlsx";
           
            string pdfurl = Server.MapPath("~") + "\\Uploads\\Reports\\" + file;
            string pdfurl1 = Server.MapPath("~") + "\\Uploads\\Reports\\" + file1;
           
            string relativepath = "\\Uploads\\Reports\\";
            string absoluteurl = Server.MapPath(relativepath);

            if (System.IO.Directory.Exists(absoluteurl))
            {
                try
                {
                  //  System.IO.Directory.Delete(absoluteurl, true);
                    // System.IO.Directory.CreateDirectory(absoluteurl);
                }
                catch (Exception ex)
                {
                   
                }
            }

            if (!System.IO.Directory.Exists(absoluteurl))
            {

                System.IO.Directory.CreateDirectory(absoluteurl);
            }


              try
                {

                   var workbooktest = SpreadsheetDocument.Create(pdfurl1, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                    workbooktest.Close(); 
              }
                catch (Exception ex)
                {
                    if (!System.IO.Directory.Exists(absoluteurl))
                    {

                        System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                   

                }

           
            
            
            
            
            
            using (var workbook = SpreadsheetDocument.Create(pdfurl, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

         
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                    sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = file };
                    sheets.Append(sheet);

                 //table resumen
                   
                    DocumentFormat.OpenXml.Spreadsheet.Row headerRes = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        DocumentFormat.OpenXml.Spreadsheet.Cell cellres = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cellres.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cellres.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(namereport);
                        BackgroundColor backgroundColor = new BackgroundColor() { Indexed = (UInt32Value)64U };
                      
                        //  cellres.Append(borders1);
                          
                           headerRes.AppendChild(cellres);
                        if (filter != "")
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cellres1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cellres1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cellres1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(typefilter);
                            //cellres1.Append(borders1);
                            headerRes.AppendChild(cellres1);
                        }
                        DocumentFormat.OpenXml.Spreadsheet.Cell cellres2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cellres2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cellres2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("Fecha Inicial");
                       // cellres2.Append(borders1);
                        headerRes.AppendChild(cellres2);

                        DocumentFormat.OpenXml.Spreadsheet.Cell cellres3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cellres3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cellres3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("Fecha Final");
                       // cellres3.Append(borders1);
                        headerRes.AppendChild(cellres3);
                        
                        sheetData.AppendChild(headerRes);
                          DocumentFormat.OpenXml.Spreadsheet.Row Resdata = new DocumentFormat.OpenXml.Spreadsheet.Row();

                         DocumentFormat.OpenXml.Spreadsheet.Cell celldata = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                         celldata.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                         celldata.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(numtotal);
                        // celldata.Append(borders1);
                            
                         Resdata.AppendChild(celldata);

                           if (filter != "")
                           {
                               DocumentFormat.OpenXml.Spreadsheet.Cell celldata1 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                               celldata1.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                               celldata1.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(filter);
                        //       celldata1.Append(borders1);
                        
                               Resdata.AppendChild(celldata1);
                           }

                           DocumentFormat.OpenXml.Spreadsheet.Cell celldata2 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                           celldata2.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                           celldata2.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(getdates[0]);
                          // celldata2.Append(borders1);
                        
                            Resdata.AppendChild(celldata2);

                           DocumentFormat.OpenXml.Spreadsheet.Cell celldata3 = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                           celldata3.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                           celldata3.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(getdates[1]);
                          // celldata3.Append(borders1);
                        
                            Resdata.AppendChild(celldata3);

                           sheetData.AppendChild(Resdata);
                     
                           DocumentFormat.OpenXml.Spreadsheet.Row Resdata1 = new DocumentFormat.OpenXml.Spreadsheet.Row();
                DocumentFormat.OpenXml.Spreadsheet.Cell cellclear = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                           cellclear.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                           cellclear.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue("");
                           Resdata1.AppendChild(cellclear);

                           sheetData.AppendChild(Resdata1);
                         
                
                
                   
                    
                // table report   

                            DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                    List<String> columns = new List<string>();
                    foreach (var x in head)
                    {
                        columns.Add(x.Value);
                        
                       DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(x.Value);
                      //  cell.Append(borders1);
                        
                        headerRow.AppendChild(cell);
                    }


                    sheetData.AppendChild(headerRow);
                     foreach (var item in tableja)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (var col in head)
                        {
                            try
                            {
                                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                                cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(item[col.Key].ToString()); //

                                newRow.AppendChild(cell);
                            }
                            catch (Exception ex) { continue; }
                            //  cell.Append(borders1);
                      
                        }

                        sheetData.AppendChild(newRow);
                    
                    }

                       
                    workbook.Close();
            }
            
            System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Reports/" + file);


            //  Downloadpdf(file);
            return toDownload.Name;
         
        }
       
        public void Downloadpdf(string file)
        {
//            System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Reports" + file);



            System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Reports/" + file);

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + toDownload.Name);
            Response.AddHeader("Content-Length", toDownload.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.TransmitFile(toDownload.FullName);
            Response.Flush();
           Response.End();
             
        }
           [OutputCache(Duration = 600, VaryByParam = "id")] 
     
        public ActionResult ProfileReport(string id, string datestart, string dateend)
        {
            Dictionary<string, string> reports = new Dictionary<string, string>();
            /*  if (Request.Cookies["_id2"] != null)
           {
               Session["_id"] = Request.Cookies["_id2"].Value;
           }*/
            // string idreport=Session["_id"].ToString();
            string idreport = id;
            string type = "Perfiles";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }
            ViewData["reports"] = reports;
            return View();

        }
        public string getImage(string input)
        {
            if (input == null)
                return string.Empty;
            string tempInput = input;
            string pattern = @"<img(.|\n)+?>";
            string src = string.Empty;
            HttpContext context = System.Web.HttpContext.Current;

            //Change the relative URL's to absolute URL's for an image, if any in the HTML code.
            foreach (Match m in Regex.Matches(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline |

            RegexOptions.RightToLeft))
            {
                if (m.Success)
                {
                    string tempM = m.Value;
                    string pattern1 = "src=[\'|\"](.+?)[\'|\"]";
                    Regex reImg = new Regex(pattern1, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    Match mImg = reImg.Match(m.Value);

                    if (mImg.Success)
                    {
                        src = mImg.Value.ToLower().Replace("src=", "").Replace("\"", "");

                        if (src.ToLower().Contains("http://") == false)
                        {
                            //Insert new URL in img tag
                            src = "src=\"" + context.Request.Url.Scheme + "://" +
                            context.Request.Url.Authority + src + "\"";
                            try
                            {
                                tempM = tempM.Remove(mImg.Index, mImg.Length);
                                tempM = tempM.Insert(mImg.Index, src);

                                //insert new url img tag in whole html code
                                tempInput = tempInput.Remove(m.Index, m.Length);
                                tempInput = tempInput.Insert(m.Index, tempM);
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
            }
            return tempInput;
        }

        public JsonResult GetReport(string id)
        {
            try
            {
                string getreports = Reportsdb.GetRow(id);
                JObject reportsja = JsonConvert.DeserializeObject<JObject>(getreports);

                string profile = "";
                try
                {
                    profile = reportsja["filter"].ToString();
                }
                catch (Exception ex) { }
                string cols = reportsja["fields"].ToString();
               
                string startdate = reportsja["start_date"].ToString();
                string enddate = reportsja["end_date"].ToString();
                string typereport = reportsja["CategoryReport"].ToString();
               
                string movements = "[]";
                try
                {
                    movements = reportsja["movements"].ToString();
                }
                catch (Exception ex) { }
                string objects = "[]";
                try
                {
                    objects = reportsja["objects"].ToString();
                }
                catch (Exception ex) { }
                string locations = "[]";
                try
                {
                    locations = reportsja["locations"].ToString();
                }
                catch (Exception ex) { }
                string users = "[]";
                try
                {
                    users = reportsja["users"].ToString();
                }
                catch (Exception ex) { }
                /*  String jsonData = "{'profiles':" + profile + ",'cols':" + cols + ",'start_date':'" + startdate
                 + "','end_date':'" + enddate + "'}";*/

                JObject result = new JObject();
                result.Add("profiles", profile);
                result.Add("movements", movements);
                result.Add("objects", objects);
                result.Add("locations", locations);
                result.Add("users", users);
                result.Add("cols", cols);
                result.Add("startdate", startdate);
                result.Add("enddate", enddate);
                result.Add("CategoryReport", typereport);
                return Json(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //  [OutputCache(Duration =600, VaryByParam = "id")] 
     
        public ActionResult UserReport(string id, string datestart, string dateend)
        {


            List<string> dates = new List<string>();
            dates.Add(datestart);
            dates.Add(dateend);
            ViewData["dates"] = dates;
            Dictionary<string, string> profiles = new Dictionary<string, string>();
            string getprofiles = profilesdb.GetRows();
            JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
            Dictionary<string, string> reports = new Dictionary<string, string>();
          /*  if (Request.Cookies["_id2"] != null)
            {
                Session["_id"] = Request.Cookies["_id2"].Value;
            }*/
           // string idreport=Session["_id"].ToString();
            string idreport = id;
            string type = "Usuarios";
            string getreports = Reportsdb.GetRowsReports(idreport, type);
            JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

            foreach (JObject item1 in reportsja)
            {
                reports.Add(item1["_id"].ToString(), item1["name"].ToString());
            }


            foreach (JObject item in profilesja)
            {

                profiles.Add(item["_id"].ToString(), item["name"].ToString());
                
            }

            ViewData["reports"] = reports;
            ViewData["locations"] = getLocationsValids();
            return View(profiles);
        }
        public ActionResult ObjectsRealReport2(string id, string datestart, string dateend)
        {

            try
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();
                List<string> validlocations = new List<string>();
                List<string> locsvalid = new List<string>();
                bool alls = false;
                try
                {
                    JObject thisuser = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(useridx));
                    locsvalid = (from loc in thisuser["userLocations"].Children() select (string)loc["id"]).ToList();
                    if (locsvalid.Contains("undefined") || locsvalid.Contains("null"))
                    {
                        alls = true;

                    }
                    else
                    {


                        JArray getconjuntos = JsonConvert.DeserializeObject<JArray>(locationsdb.GetRowsFilter(locsvalid));
                        validlocations = (from lo in getconjuntos select (string)lo["_id"]).ToList();
                        try
                        {
                            List<string> regions = (from lo in getconjuntos where (string)lo["parent"] == "null" select (string)lo["_id"]).ToList();
                            JArray getchildrenreg = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(regions));
                            List<string> childrenlocs = (from ch in getchildrenreg select (string)ch["_id"]).ToList();
                            validlocations.AddRange(childrenlocs);

                        }
                        catch
                        {

                        }

                    }

                }
                catch
                {

                }
                List<string> dates = new List<string>();
                dates.Add(datestart);
                dates.Add(dateend);
                ViewData["dates"] = dates;
                Dictionary<string, string> customfieldsdic = new Dictionary<string, string>();
                Dictionary<string, string> profiles = new Dictionary<string, string>();

                string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
                JArray conjuntja = new JArray();
                string idprof = "";
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }
                string getprofiles = locationsdb.Get("profileId", idprof);
                string customfields = ObjectFieldsdb.GetRows();
                JArray profilesja = new JArray();
                try
                {
                    profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                }
                catch (Exception ex) { }
                JArray customfieldsja = new JArray();
                try
                {
                    customfieldsja = JsonConvert.DeserializeObject<JArray>(customfields);
                }
                catch (Exception ex) { }
                Dictionary<string, string> reports = new Dictionary<string, string>();
                /*  if (Request.Cookies["_id2"] != null)
                  {
                      Session["_id"] = Request.Cookies["_id2"].Value;
                  }*/
                // string idreport=Session["_id"].ToString();
                string idreport = id;
                string type = "Objetos Reales";
                string getreports = Reportsdb.GetRowsReports(idreport, type);
                JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

                foreach (JObject item1 in reportsja)
                {
                    try
                    {
                        reports.Add(item1["name"].ToString(), item1["label"].ToString());
                    }
                    catch (Exception ex) { continue; }
                }


                foreach (JObject item in profilesja)
                {
                    try
                    {
                        if (alls)
                        {
                            profiles.Add(item["_id"].ToString(), item["name"].ToString());
                        }
                        else
                        {
                            if (validlocations.Contains(item["_id"].ToString()))
                            {
                                profiles.Add(item["_id"].ToString(), item["name"].ToString());
                            }
                        }
                    }
                    catch (Exception ex) { continue; }
                }

                foreach (JObject item in customfieldsja)
                {
                    try
                    {
                        customfieldsdic.Add(item["name"].ToString(), item["label"].ToString());
                    }
                    catch (Exception ex) { continue; }
                }

                ViewData["reports"] = reports;
                ViewData["customfields"] = customfieldsdic;
                return View(profiles);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ActionResult ObjectsRealReport(string id, string datestart, string dateend)
        {

            try
            {
                try
                {
                    string permission = Session["Permissions"].ToString();
                }
                catch (Exception ex)
                {
                    if (Request.Cookies["permissions"] != null)
                    {
                        Session["Permissions"] = Request.Cookies["permissions"].Value;

                    }

                }

                String dataPermissions = Session["Permissions"].ToString();
                List<string> allowreports = validatepermissions.Tolistpermissions("selectreports", dataPermissions);
                if (!allowreports.Contains("a") && !allowreports.Contains("c"))
                {
                    return null;
                }
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();
                List<string> validlocations = new List<string>();
                List<string> locsvalid = new List<string>();
                bool alls = false;
                try
                {
                    JObject thisuser = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(useridx));
                    locsvalid = (from loc in thisuser["userLocations"].Children() select (string)loc["id"]).ToList();
                    if (locsvalid.Contains("undefined") || locsvalid.Contains("null")) {
                        alls = true;

                    }
                    else {


                        JArray getconjuntos = JsonConvert.DeserializeObject<JArray>(locationsdb.GetRowsFilter(locsvalid));
                        validlocations = (from lo in getconjuntos select (string)lo["_id"]).ToList();
                        try
                        {
                            List<string> regions = (from lo in getconjuntos where (string)lo["parent"]=="null" select (string)lo["_id"]).ToList();
                            JArray getchildrenreg = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(regions));
                            List<string> childrenlocs = (from ch in getchildrenreg select (string)ch["_id"]).ToList();
                            validlocations.AddRange(childrenlocs);
                            
                        }
                        catch
                        {

                        }
                       
                    }
                
                }
                catch
                {

                }
                List<string> dates = new List<string>();
                dates.Add(datestart);
                dates.Add(dateend);
                ViewData["dates"] = dates;
                Dictionary<string, string> customfieldsdic = new Dictionary<string, string>();
                Dictionary<string, string> profiles = new Dictionary<string, string>();

                string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
                JArray conjuntja = new JArray();
                string idprof = "";
                try
                {  
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }
                string getprofiles = locationsdb.Get("profileId", idprof);
                string customfields = ObjectFieldsdb.GetRows();
                JArray profilesja = new JArray();
                try
                {
                    profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                }
                catch (Exception ex) { }
                JArray customfieldsja = new JArray();
                try
                {
                    customfieldsja = JsonConvert.DeserializeObject<JArray>(customfields);
                }
                catch (Exception ex) { }
                Dictionary<string, string> reports = new Dictionary<string, string>();
                /*  if (Request.Cookies["_id2"] != null)
                  {
                      Session["_id"] = Request.Cookies["_id2"].Value;
                  }*/
                // string idreport=Session["_id"].ToString();
                string idreport = id;
                string type = "Objetos Reales";
                string getreports = Reportsdb.GetRowsReports(idreport, type);
                JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

                foreach (JObject item1 in reportsja)
                {
                    try
                    {
                        reports.Add(item1["name"].ToString(), item1["label"].ToString());
                    }
                    catch (Exception ex) { continue; }
                }


                foreach (JObject item in profilesja)
                {
                    try
                    {
                        if (alls)
                        {
                            profiles.Add(item["_id"].ToString(), item["name"].ToString());
                        }
                        else
                        {
                            if (validlocations.Contains(item["_id"].ToString()))
                            {
                                profiles.Add(item["_id"].ToString(), item["name"].ToString());
                            }
                        }
                    }
                    catch (Exception ex) { continue; }
                }

                foreach (JObject item in customfieldsja)
                {
                    try
                    {
                        customfieldsdic.Add(item["name"].ToString(), item["label"].ToString());
                    }
                    catch (Exception ex) { continue; }
                }

                ViewData["reports"] = reports;
                ViewData["customfields"] = customfieldsdic;
                return View(profiles);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult GenerateMessageReport(string profile, string startdate, string enddate, string col)
        {
            return View();
        
        
        }

        public ActionResult GenerateProfileReport(string startdate, string enddate, string col)
        {


            try
            {
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                int numhw = 0;
                int index = 0;

              


                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {

                    if (x["data"].ToString() == "users")
                    {
                        index++;
                        valuex = x["value"].ToString();

                        datax = x["data"].ToString();
                    }
                    else
                    {
                        
                             datacols.Add(x["data"].ToString(), x["value"].ToString());
                   
                    }
                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = Profiledb.GetRowsReportProfile(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                string getuser = Userdb.GetRows();
                JArray usersja = JsonConvert.DeserializeObject<JArray>(getuser);
               
                

                    // }
                   
                  
                    // }

                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                    int numusers = 0;

                    foreach (JObject x in usersja)
                    {

                        if (x["profileId"].ToString() == item["_id"].ToString())
                        {
                            int val = 0;
                            int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                            int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                            if (month.ToString().Substring(0, 1) == "0")
                            {

                                month = Convert.ToInt16(month.ToString().Substring(1));

                            }
                            int[] arraylinex = aux.ToArray(); 
                            if (graph.TryGetValue(item["name"].ToString(), out val))
                            {
                                graph[item["name"].ToString()] = graph[item["name"].ToString()] + 1;
                                arraylinex = auxgraph[item["name"].ToString()];
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["name"].ToString()] = arraylinex;
                            }
                            else
                            {

                                graph.Add(item["name"].ToString(), 1);
                               
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["name"].ToString()] = arraylinex;
                            }
                    
                            numusers++;
                        }

                    }
                  

                    // }
                   // item.Add("\Users':'"+numusers+"'");
                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"users\":\'" + numusers + "', \"CreatedDate\":\'" + item["CreatedDate"] + "' }");
                      
                    result.Add(jobjectnew);
                    numhw++;
                    // }
                    }
                    catch (Exception ex)
                    {


                }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numprofiles"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
              /*  string rut = exp(result, "", numhw.ToString(), datacols, datesarray, "Perfiles", "");
                ViewData["url"] = rut;*/
              
                 Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;


                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
        public headgraph Getdatagraph(int years, string year1, string year2)
        {
            List<int> aux = new List<int>();
            Dictionary<int, string> headm = new Dictionary<int, string>();
            string type = "";
            if (years == 0)
            {
                headm.Add(1, "Ene"); headm.Add(2, "Feb"); headm.Add(3, "Mar"); headm.Add(4, "Abril"); headm.Add(5, "Mayo");
                headm.Add(6, "Jun"); headm.Add(7, "Jul"); headm.Add(8, "Agost"); headm.Add(9, "Sept"); headm.Add(10, "Oct");
                headm.Add(11, "Nov"); headm.Add(12, "Dic");

                for (int i = 0; i < 12; i++)
                {

                    aux.Add(0);
                }
                // arrayline = aux.ToArray();
                type = "Meses";
            }
            else if (years > 0 && years < 3)
            {
                for (int i = Convert.ToInt16(year1); i <= Convert.ToInt16(year2); i++)
                {
                    string key1 = Convert.ToString(i) + "1";
                    string key2 = Convert.ToString(i) + "2";
                    string key3 = Convert.ToString(i) + "3";
                    headm.Add(Convert.ToInt16(key1), i + "-1");
                    headm.Add(Convert.ToInt16(key2), i + "-2");
                    headm.Add(Convert.ToInt16(key3), i + "-3");
                    aux.Add(0);
                    aux.Add(0);
                    aux.Add(0);
                }



                type = "Cuatrimestres";
              //  arrayline = aux.ToArray();
            }
            else if (years > 2 && years < 10)
            {

                for (int i = Convert.ToInt16(year1); i <= Convert.ToInt16(year2) + 1; i++)
                {

                    headm.Add(i, i.ToString());

                    aux.Add(0);
                }

                type = "Años";
            }

            headgraph data = new headgraph(headm, aux, type);

            return data;
          
        }

        public ActionResult GenerateProcessesReport(string startdate, string enddate, string col)
        {

           
            try
            {
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                int numhw = 0;
                int index = 0;
                int[] arrayline;



                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {

                   

                        datacols.Add(x["data"].ToString(), x["value"].ToString());

                   
                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
               
              
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

              //end graphlinetime

                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = Proccessdb.GetRowsReportProcesses(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                //timegraph
                Dictionary<string, int> graph = new Dictionary<string, int>();
                 Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                // }


                // }
                
                         
                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {

                    try
                    {
                    int numi = 0;
                    int numusers = 0;
                    string min = "";
                    string max = "";
                    try
                    {
                        min = item["min_duration"]["duration"].ToString();
                        min = min + " " + item["min_duration"]["type"].ToString();
                      
                    }
                    catch (Exception ex)
                    {
                        min = "Ilimitado";
                    }
                    try
                    {
                        max = item["max_duration"]["duration"].ToString();
                        max = max + " " + item["max_duration"]["type"].ToString();
                   
                    }
                    catch (Exception ex)
                    {
                        max = "Ilimitado";
                    }

                  
                            int val = 0;

                            int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                            int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));
                         
                            if (month.ToString().Substring(0, 1) == "0")
                            {

                                month = Convert.ToInt16(month.ToString().Substring(1));

                            }
                            int[] arraylinex = aux.ToArray(); 

                            if (graph.TryGetValue(item["status"].ToString(), out val))
                            {
                               
                                graph[item["status"].ToString()] = graph[item["status"].ToString()] + 1;
                                arraylinex = auxgraph[item["status"].ToString()];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["status"].ToString()] = arraylinex;


                            }
                            else
                            {
                                graph.Add(item["status"].ToString(), 1);
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["status"].ToString()] = arraylinex;
                                 

                            }
                                        
                      
                        numusers++;
                        

                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"status\":\'" + item["status"] + "', \"min_duration\":\'" + min + "', \"max_duration\":\'" + max + "', \"CreatedDate\":\'" + item["CreatedDate"] + "' }");

                    result.Add(jobjectnew);
                    numhw++;
                 
                    }
                    catch (Exception ex)
                    {


                  }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
               
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
        public ActionResult GenerateMovementReport(string startdate, string enddate, string col)
        {


            try
            {
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                int numhw = 0;
                int index = 0;
                int[] arrayline;



                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {



                    datacols.Add(x["data"].ToString(), x["value"].ToString());


                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);


                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime

                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = Movementdb.GetRowsReportMov(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                //timegraph
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                // }


                // }


                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {

                    try
                    {
                        int numi = 0;
                        int numusers = 0;

                        string processesrow = Proccessdb.GetRow(item["processes"].ToString());
                        JObject proja = JsonConvert.DeserializeObject<JObject>(processesrow);

                        string categoryName = proja["name"].ToString();
                        item["processes"] = proja["name"].ToString();
                        int val = 0;

                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();

                        if (graph.TryGetValue(categoryName, out val))
                        {

                            graph[categoryName] = graph[categoryName] + 1;
                            arraylinex = auxgraph[categoryName];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[categoryName] = arraylinex;


                        }
                        else
                        {
                            graph.Add(categoryName, 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[categoryName] = arraylinex;


                        }


                        numusers++;


                      //  JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"processes\":\'" + categoryName + "',\"CreatedDate\":\'" + item["CreatedDate"] + "' }");

                        result.Add(item);
                        numhw++;

                    }
                    catch (Exception ex)
                    {


                    }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
             
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }

        public ActionResult GenerateObjectsRefReport(string startdate, string enddate, string col)
        {


            try
            {

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                int numhw = 0;
                int index = 0;
                int[] arrayline;



                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {



                    datacols.Add(x["data"].ToString(), x["value"].ToString());


                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);


                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime

                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = Objrefdb.GetRowsReportRf(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                //timegraph
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                // }


                // }


                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {

                    try
                    {
                    int numi = 0;
                    int numusers = 0;

                    //string categoryrow = Categoriesdb.GetRow(item["parentCategory"].ToString());
                    //JObject catja = JsonConvert.DeserializeObject<JObject>(categoryrow);

                    string categoryName = "NA";
                    try
                    {
                      categoryName= item["nameCategory"].ToString();
                    }
                    catch { }
                    int val = 0;

                   // int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                   // int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));
                    int month = 0;
                    int year = 0;
                    try
                    {
                        string size = item["CreatedDate"].ToString().Split(' ').First();

                            month = Convert.ToInt16(size.Split('/')[0]);
                            year = Convert.ToInt16(size.Split('/')[2]);
                       
                    }
                    catch {
                    
                    }
                    if (month.ToString().Substring(0, 1) == "0")
                    {

                        month = Convert.ToInt16(month.ToString().Substring(1));

                    }
                    int[] arraylinex = aux.ToArray();

                    if (graph.TryGetValue(categoryName, out val))
                    {

                        graph[categoryName] = graph[categoryName] + 1;
                        arraylinex = auxgraph[categoryName];
                        arraylinex = getgraph(years, arraylinex, month, year, headm);
                        auxgraph[categoryName] = arraylinex;


                    }
                    else
                    {
                        graph.Add(categoryName, 1);
                        arraylinex = getgraph(years, arraylinex, month, year, headm);
                        auxgraph[categoryName] = arraylinex;


                    }


                    numusers++;


                    JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":\"" + item["name"].ToString() + "\", \"parentCategory\":\"" + categoryName + "\",\"CreatedDate\":\"" + item["CreatedDate"] + "\" }");

                    result.Add(jobjectnew);
                    numhw++;

                    }
                    catch (Exception ex)
                    {


                }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
              
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }


        public ActionResult GenerateLocationsReport(string startdate, string enddate, string col)
        {


            try
            {

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                int numhw = 0;
                int index = 0;
                int[] arrayline;
                List<string> locslist = new List<string>();
                try
                {
                    Dictionary<string, string> listlocsvalid = getLocationsValids();
                    foreach (string loc in listlocsvalid.Keys)
                    {
                        locslist.Add(loc);
                    }
                    try
                    {
                        JArray regions = JsonConvert.DeserializeObject<JArray>(locationsdb.Getparents(locslist));
                        foreach (JObject jor in regions)
                        {
                            if (!locslist.Contains(jor["parent"].ToString()))
                            {
                                locslist.Add(jor["parent"].ToString());
                            }
                        }
                    }
                    catch { 
                    
                    }
                    List<string> childrenslocations = new List<string>();
                    try
                    {
                        JArray childrens = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(locslist));
                        childrenslocations = (from child in childrens select (string)child["_id"]).ToList();
                    }
                    catch {

                    }

                    locslist.AddRange(childrenslocations);
                }
                catch
                {

                }

                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {



                    datacols.Add(x["data"].ToString(), x["value"].ToString());


                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);


                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime

                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = locationsdb.GetRowsReportLoc(datacols, start, end, locslist);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                //timegraph
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                // }


                // }


                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {

                    try
                    {

                    int numi = 0;
                    int numusers = 0;

                    string profilerow = locationsProfilesdb.GetRow(item["profileId"].ToString());
                    JObject proja = JsonConvert.DeserializeObject<JObject>(profilerow);

                    string categoryName = proja["name"].ToString();
                    int val = 0;

                    int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                    int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                    if (month.ToString().Substring(0, 1) == "0")
                    {

                        month = Convert.ToInt16(month.ToString().Substring(1));

                    }
                    int[] arraylinex = aux.ToArray();

                    if (graph.TryGetValue(categoryName, out val))
                    {

                        graph[categoryName] = graph[categoryName] + 1;
                        arraylinex = auxgraph[categoryName];
                        arraylinex = getgraph(years, arraylinex, month, year, headm);
                        auxgraph[categoryName] = arraylinex;


                    }
                    else
                    {
                        graph.Add(categoryName, 1);
                        arraylinex = getgraph(years, arraylinex, month, year, headm);
                        auxgraph[categoryName] = arraylinex;


                    }


                    numusers++;

                    categoryName = (categoryName == "Conjunto") ? "Oficina" : categoryName;
                    JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"profileId\":\'" + categoryName + "', \"tipo\":\'" + item["tipo"] + "',\"CreatedDate\":\'" + item["CreatedDate"] + "' }");

                    result.Add(jobjectnew);
                    numhw++;

                    }
                    catch (Exception ex)
                    {


                }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
              
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }

        public int[] getgraph(int years, int[] arraylinex, int month, int year, Dictionary<int, string> headm)
        {
            int q = 0;
            if (years == 0)
            {


                arraylinex[month - 1] = arraylinex[month - 1] + 1;
               
                return arraylinex;
            }
            if (years > 0 && years < 3)
            {

                if (month < 5) { q = 1; }
                if (month > 4 && month < 9) { q = 2; }
                if (month > 8) { q = 3; }

                int getindex = 0;
                int e = Convert.ToInt16(Convert.ToString(year) + Convert.ToString(q));

                foreach (var x in headm)
                {
                    if (x.Key == e)
                    {
                        string v = Convert.ToString(year + "-" + q);
                        if (x.Value == v)
                        {
                            arraylinex[getindex] = arraylinex[getindex] + 1;
                            return arraylinex;
                        }


                    }
                    getindex++;
                }


            }
            if (years > 2 && years < 10)
            {


                int getindex = 0;

                foreach (var x in headm)
                {
                    if (x.Key == year)
                    {

                        arraylinex[getindex] = arraylinex[getindex] + 1;
                      
                        return arraylinex;



                    }
                    getindex++;

                }

            }
            return arraylinex;
        }
       
        public ActionResult GenerateInventoryReport(string startdate, string enddate, string col,string locs)
        {


            try
            {

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                int numhw = 0;


               

                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                JArray locsja = JsonConvert.DeserializeObject<JArray>(locs);
                Dictionary<string, string> datacols = new Dictionary<string, string>();
                List<string> locslist = new List<string>();
                foreach (JObject x in cols)
                {

                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }
                foreach (JObject loc in locsja)
                {
                    try
                    {
                        locslist.Add(loc["data"].ToString());
                    }
                    catch { }
                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                Dictionary<string, JObject> unumber = new Dictionary<string, JObject>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                List<string> childrenlocs = new List<string>();
                try
                {
                    JArray locschildren = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(locslist));
                    childrenlocs = (from ch in locschildren select (string)ch["_id"]).ToList();
                    locslist.AddRange(childrenlocs);
                }
                catch { }
                //fin sublucations
                List<string> sublocations = new List<string>();
                try
                {
                    JArray childrens = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(childrenlocs));
                    sublocations = (from child in childrens select (string)child["_id"]).ToList();
                    locslist.AddRange(sublocations);

                }
                catch { }
                //end find sublocations
                string gethw = Inventorydb.GetRowsReportInventory(datacols,locslist, start, end);
                JArray hwsja = new JArray();
                try { hwsja = JsonConvert.DeserializeObject<JArray>(gethw); }
                catch { }
                JArray result = new JArray();
                

                List<string> locationsresult = new List<string>();
                if (hwsja.Count > 0)
                {
                    try
                    {
                        locationsresult = (from hw in hwsja select (string)hw["location"]).ToList();
                        JArray resultnum = JsonConvert.DeserializeObject<JArray>(locationsdb.Getparents(locationsresult));
                        foreach (JObject jo in resultnum)
                        {
                            try
                            {
                                unumber.Add(jo["_id"].ToString(), jo);
                            }
                            catch { }

                        }
                    }
                    catch
                    {

                    }
                }

                Dictionary<string, int> graph = new Dictionary<string, int>();
                //timegraph
              
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                //
                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                    string v = "";
                    //    if (datacols.TryGetValue("profileId",out v)){
                    string value = "";
             
                  /*  string getlocationname = locationdb.GetRow(item["location"].ToString());
                    JObject locationja = JsonConvert.DeserializeObject<JObject>(getlocationname);
                    string getprofile = profilesdb.GetRow(item["profile"].ToString());
                    JObject profilejaa = JsonConvert.DeserializeObject<JObject>(getprofile);
                 /*   string gethardware = Hwdb.GetRow(item["hardware"].ToString());
                    JObject hardwarejaa = JsonConvert.DeserializeObject<JObject>(gethardware);
                  
                   
                        item["location"] = locationja["name"].ToString();
                       item["profile"]=profilejaa["name"].ToString();
                  //     item["hardware"] = hardwarejaa["name"].ToString();*/
                    try
                    {
                        JObject jn;
                        JToken nnn;
                        if(unumber.TryGetValue(item["location"].ToString(),out jn)){
                             if(jn.TryGetValue("conjuntoname",out nnn)){
                                item.Add("conjuntoname", jn["conjuntoname"].ToString());
                            }
                            else
                            {
                                item.Add("conjuntoname", "");
                            }
                            if (jn.TryGetValue("conjuntonumber", out nnn))
                            {
                                item.Add("conjuntonumber", jn["conjuntonumber"].ToString());
                            }
                            else
                            {
                                item.Add("conjuntonumber", "");
                            }
                        }
                        item["location"] = item["locationname"].ToString();
                    }
                    catch
                    {

                    }


                    int val = 0;
                    // times
                    int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                    int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                    if (month.ToString().Substring(0, 1) == "0")
                    {

                        month = Convert.ToInt16(month.ToString().Substring(1));

                    }
                    int[] arraylinex = aux.ToArray();
                    //


                    if (graph.TryGetValue(item["location"].ToString(), out val))
                    {
                        graph[item["location"].ToString()] = graph[item["location"].ToString()] + 1;
                        arraylinex = auxgraph[item["location"].ToString()];
                        arraylinex = getgraph(years, arraylinex, month, year, headm);
                        auxgraph[item["location"].ToString()] = arraylinex;
                    }
                    else
                    {

                        graph.Add(item["location"].ToString(), 1);
                        arraylinex = getgraph(years, arraylinex, month, year, headm);
                        auxgraph[item["location"].ToString()] = arraylinex;
                    }


                    // }
                    result.Add(item);
                    numhw++;
                    // }
                    }
                    catch (Exception ex)
                    {


                }
                }



                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
             
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
    
       /* public ActionResult GenerateHwReport(string type, string startdate, string enddate, string col)
        {


            try
            {
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                int numhw = 0;
              
               
                if (type == "0") { ViewData["smart"] = "Todos"; }
                else if (type == "1") { ViewData["smart"] = "Inteligentes"; }
                 else if (type == "2") { ViewData["smart"] = "No Inteligentes"; }
               

                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                Dictionary<string, string> datacols = new Dictionary<string, string>();
                foreach (JObject x in cols)
                {

                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                string gethw = Hwdb.GetRowsReportHw(type, datacols, start, end);
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();
                 
                Dictionary<string, int> graph = new Dictionary<string, int>();
                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                    string v = "";
                    //    if (datacols.TryGetValue("profileId",out v)){
                    string value = "";

                        if (item["smart"].ToString().ToLower() == "false")
                    {

                        item["smart"] = "No Inteligente";

                        }
                        else
                        {
                            item["smart"] = "Inteligente";


                    }

                        int val = 0;
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray(); 
                        if (graph.TryGetValue(item["smart"].ToString(), out val))
                        {
                            graph[item["smart"].ToString()] = graph[item["smart"].ToString()] + 1;
                            arraylinex = auxgraph[item["smart"].ToString()];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["smart"].ToString()] = arraylinex;
                        }
                        else
                        {

                            graph.Add(item["smart"].ToString(), 1);
                           
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["smart"].ToString()] = arraylinex;
                        }
                    

                    // }
                    result.Add(item);
                    numhw++;
                    // }
                    }
                    catch (Exception ex)
                    {


                }
                }
              
                
                
                ViewData["numusers"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
       */
        public ActionResult GenerateHwReport(string type, string startdate, string enddate, string col)
        {


            try
            {
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                int numhw = 0;


                if (type == "0") { ViewData["smart"] = "Todos"; }
                else if (type == "1") { ViewData["smart"] = "Inteligentes"; }
                else if (type == "2") { ViewData["smart"] = "No Inteligentes"; }


                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                Dictionary<string, string> datacols = new Dictionary<string, string>();
                foreach (JObject x in cols)
                {

                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();

                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                // string gethw = Hwdb.GetRowsReportHw(type, datacols, start, end);
                Dictionary<string, string> catdict = new Dictionary<string, string>();
                try
                {
                    JArray cate = JsonConvert.DeserializeObject<JArray>(HwCat.GetRows());
                    catdict = cate.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                }
                catch { }
                string gethw = Hwdb.GetRows();
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end)
                        {
                            string v = "";
                            try
                            {
                                item["hardware_reference"] = catdict[item["hardware_reference"].ToString()];
                            }
                            catch
                            {
                                try { item["hardware_reference"] = ""; }
                                catch { }

                            }
                            //    if (datacols.TryGetValue("profileId",out v)){
                            string value = "";

                            /*     if (item["smart"].ToString().ToLower() == "false")
                             {

                                 item["smart"] = "No Inteligente";

                                 }
                                 else
                                 {
                                     item["smart"] = "Inteligente";


                             }*/

                            int val = 0;
                            try
                            {
                                int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                                int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                                if (month.ToString().Substring(0, 1) == "0")
                                {

                                    month = Convert.ToInt16(month.ToString().Substring(1));

                                }
                                int[] arraylinex = aux.ToArray();

                                if (graph.TryGetValue(item["smart"].ToString(), out val))
                                {
                                    graph[item["smart"].ToString()] = graph[item["smart"].ToString()] + 1;
                                    arraylinex = auxgraph[item["smart"].ToString()];
                                    arraylinex = getgraph(years, arraylinex, month, year, headm);
                                    auxgraph[item["smart"].ToString()] = arraylinex;
                                }
                                else
                                {

                                    graph.Add(item["smart"].ToString(), 1);

                                    arraylinex = getgraph(years, arraylinex, month, year, headm);
                                    auxgraph[item["smart"].ToString()] = arraylinex;
                                }
                            }
                            catch { }

                            // }
                            result.Add(item);
                            numhw++;
                            // }
                        }
                    }
                    catch (Exception ex)
                    {


                    }
                }



                ViewData["numusers"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
       
         
         public string SaveReportCustom(string namereport, string movements, string objects, string locations, string users, string startdate, string enddate, string type)
        {
            try
            {
                Dictionary<string, string> datacols = new Dictionary<string, string>(); 
                datacols.Add("folio", "folio");
                    datacols.Add("object", "objetos");
                    datacols.Add("location", "ubicacion");
                    
                   
                    datacols.Add("movement", "movimiento");
                    datacols.Add("Creator", "Creador");
                    datacols.Add("CreatedDate", "Fecha");

                    JArray movementja = JsonConvert.DeserializeObject<JArray>(movements);
                    JArray objectsja = JsonConvert.DeserializeObject<JArray>(objects);
                    JArray locationsja = JsonConvert.DeserializeObject<JArray>(locations);
                    JArray usersja = JsonConvert.DeserializeObject<JArray>(users);
                JArray cols = new JArray();
                foreach (var item in datacols)
                {
                    JObject colfolio = new JObject();
                    colfolio.Add("data", item.Key);
                    colfolio.Add("value", item.Value);

                    cols.Add(colfolio);
                }
                String jsonData = "{'UserId':'" + Session["_id"].ToString() + "','name':'" + namereport + "','CategoryReport':'" + type + "','fields':" + cols + ",'movements':" + movementja + ",'objects':" + objectsja + ",'locations':" + locationsja + ",'users':" + usersja + ",'start_date':'" + startdate
                   + "','end_date':'" + enddate + "'}";

                    string Id = "";

                    Id = Reportsdb.SaveRow(jsonData, Id);

               
                var url = "";
                switch (type)
                {
                    case "Usuarios":
                        {
                            url = Url.Action("UserReport", "Reports");
                            break;
                        }
                    case "Perfiles":
                        {
                            url = Url.Action("ProfileReport", "Reports");
                            break;
                        }
                    case "Procesos":
                        {
                            url = Url.Action("ProcessesReport", "Reports");
                            break;
                        }
                    case "Objetos_Ref":
                        {
                            url = Url.Action("ObjectsRefReport", "Reports");
                            break;
                        }
                    case "Movimientos":
                        {
                            url = Url.Action("MovementReport", "Reports");
                            break;
                        }
                    case "Ubicaciones":
                        {
                            url = Url.Action("LocationsReport", "Reports");
                            break;
                        }
                    case "Inventarios":
                        {
                            url = Url.Action("InventoryReport", "Reports");
                            break;
                        }
                    case "Hardwares":
                        {
                            url = Url.Action("HwReport", "Reports");
                            break;
                        }

                    case "Movements":
                        {
                            url = Url.Action("CustomReport", "Reports");
                            break;
                        }

                }
                HttpResponse.RemoveOutputCacheItem(url);

                return "Guardado Correctamente";

            }
            catch (Exception ex)
            {
                return "Error Al Guardar";
            }

        }
        public string SaveReport(string namereport, string filter, string startdate, string enddate, string col, string type)
        {
            try
            {
                
               

                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                if (type != "Usuarios" && type != "Objetos_Reales")
                {
                    string filterx = filter;
                    String jsonData = "{'UserId':'" + Session["_id"].ToString() + "','name':'" + namereport + "','CategoryReport':'" + type + "','fields':" + cols + ",'filter':'" + filterx + "','start_date':'" + startdate
                   + "','end_date':'" + enddate + "'}";

                    string Id = "";

                    Id = Reportsdb.SaveRow(jsonData, Id);
             
                }
                else
                {

                    JArray filterx = JsonConvert.DeserializeObject<JArray>(filter);

                    String jsonData = "{'UserId':'" + Session["_id"].ToString() + "','name':'" + namereport + "','CategoryReport':'" + type + "','fields':" + cols + ",'filter':" + filterx + ",'start_date':'" + startdate
                    + "','end_date':'" + enddate + "'}";

                    string Id = "";

                    Id = Reportsdb.SaveRow(jsonData, Id);
                }
                var url = "";
                switch (type)
               {
                    case "Usuarios":
                        {
                            url = Url.Action("UserReport", "Reports");
                             break;
                        }
                    case "Objetos_Reales":
                        {
                            url = Url.Action("ObjectsRealReport", "Reports");
                            break;
                        }
                    case "Perfiles":
                        {
                            url = Url.Action("ProfileReport", "Reports");
                            break;
                        }
                    case "Procesos":
                        {
                            url = Url.Action("ProcessesReport", "Reports");
                            break;
                        }
                    case "Objetos_Ref":
                        {
                            url = Url.Action("ObjectsRefReport", "Reports");
                            break;
                        }
                    case "Movimientos":
                        {
                             url = Url.Action("MovementReport", "Reports");
                            break;
                        }
                    case "Ubicaciones":
                        {
                             url = Url.Action("LocationsReport", "Reports");
                            break;
                        }
                    case "Inventarios":
                        {
                             url = Url.Action("InventoryReport", "Reports");
                            break;
                        }
                    case "Hardwares":
                        {
                            url = Url.Action("HwReport", "Reports");
                            break;
                        }

                }
                HttpResponse.RemoveOutputCacheItem(url);

                return "Guardado Correctamente";

            }
            catch (Exception ex)
            {
                return "Error Al Guardar";
            }

        }
        public ActionResult GenerateUserReport(string profile, string startdate, string enddate, string col,string locations)
        {
            try
            {
                int numusers = 0;
                string nameprofile = "";
                
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                JArray profilesx = JsonConvert.DeserializeObject<JArray>(profile);
                JArray locationsja = new JArray();
                List<string> locslist = new List<string>();
                try
                {
                  locationsja = JsonConvert.DeserializeObject<JArray>(locations);
                  if (locationsja.Count() == 0)
                  {
                      Dictionary<string, string> listvalidlocs = getLocationsValids();
                      foreach (string idl in listvalidlocs.Keys)
                      {
                          locslist.Add(idl);
                      }

                  }
                  
                }
                catch { }
                List<string> filterprofile = new List<string>();

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }



                Dictionary<string, string> datacols = new Dictionary<string, string>();
               
                foreach (JObject x in profilesx)
                {
                    filterprofile.Add(x["data"].ToString());
                }

                foreach (JObject x in cols)
                {

                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }
                foreach(JObject loc in locationsja)
                {
                    locslist.Add(loc["data"].ToString());
                }
                //fin sublucations
                List<string> sublocations = new List<string>();
                try
                {
                    JArray childrens = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(locslist));
                    sublocations = (from child in childrens select (string)child["_id"]).ToList();
                    
                       
                }
                catch { }
                //end find sublocations
                try
                {
                    JArray regions = JsonConvert.DeserializeObject<JArray>(locationsdb.Getparents(locslist));
                    foreach (JObject jor in regions)
                    {
                        if (!locslist.Contains(jor["parent"].ToString()))
                        {
                            locslist.Add(jor["parent"].ToString());
                        }
                    }
                }
                catch { }
                locslist.AddRange(sublocations);
                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);
                
                }
                string[] datesarray = dates.ToArray();
                Dictionary<string, string> profiles = new Dictionary<string, string>();
                ViewData["dates"] = dates;
                if (profile == "0")
                {
                    string getprofiles = profilesdb.GetRows();
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());
                        nameprofile = "Todos";
                    }
                }
                else
                {
                 /*   string getprofiles = profilesdb.GetRow(profile);
                   JObject profilesja = JsonConvert.DeserializeObject<JObject>(getprofiles);

                    profiles.Add(profilesja["_id"].ToString(), profilesja["name"].ToString());
                    nameprofile = profilesja["name"].ToString();*/
            
                     string getprofiles = Profiledb.GetRowsFilter(filterprofile);
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    nameprofile = "";
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());
                       

                       
                        nameprofile += x["name"] + ",";
                      

                     
                    }
                }
                if (nameprofile.Length > 50)
                {

                    nameprofile = nameprofile.Substring(0, 47);
                    nameprofile += "...";
                }

                string getuser = Userdb.GetRowsReportUser(filterprofile, datacols, start, end,locslist);
                JArray usersja = JsonConvert.DeserializeObject<JArray>(getuser);
                JArray result = new JArray();
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                foreach (JObject item in usersja)
                {
               //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        string v = "";
                    //    if (datacols.TryGetValue("profileId",out v)){
                            string value = "";
                            if (profiles.TryGetValue(item["profileId"].ToString(), out value))
                            {
                                item["profileId"] = value;

                                int val = 0;
                                int month = Convert.ToInt16(item["CreatedDate"].ToString().Substring(3, 2));
                                int year = Convert.ToInt16(item["CreatedDate"].ToString().Substring(6, 4));

                                if (month.ToString().Substring(0, 1) == "0")
                                {

                                    month = Convert.ToInt16(month.ToString().Substring(1));

                                }
                                int[] arraylinex = aux.ToArray(); 
                                if (graph.TryGetValue(value, out val))
                                {
                                    graph[value] = graph[value] + 1;
                                    arraylinex = auxgraph[item["profileId"].ToString()];
                                    arraylinex = getgraph(years, arraylinex, month, year, headm);
                                    auxgraph[item["profileId"].ToString()] = arraylinex;
                                }
                                else
                                {

                                    graph.Add(value, 1);
                                   
                                    arraylinex = getgraph(years, arraylinex, month, year, headm);
                                    auxgraph[item["profileId"].ToString()] = arraylinex;
                                }
                            }

                       // }
                        result.Add(item);
                        numusers++;
                   // }
                    }
                    catch (Exception ex)
                    {


                }
                }

                ViewData["profiles"] = nameprofile;
                ViewData["numusers"] = numusers.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
              /*  string rut = exp(result, nameprofile, numusers.ToString(), datacols, datesarray, "Usuarios", "Perfil(es)");
                ViewData["url"] = rut;
                this.Session["headuser"] = datacols;
                this.Session["userpdf"] = result;
                this.Session["filtername"] = nameprofile;
                this.Session["numtotal"] = numusers.ToString();
                this.Session["getdates"] = datesarray;
                this.Session["namereport"] = "Usuarios";
                this.Session["typefilter"] = "Perfil(es)";*/
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();
                
                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();
               
                 headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);
                    
                    auxarray.Add(w, x.Value);
                    
                    w++;
                }

                w = 0;
                
                foreach (var x in graphmult)
                {

                    
                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                           listrange.Add(a[w]);

                           }
                      
                    graphend.Add(x.Key, listrange.ToArray());
                      w++;
                      }
               
                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }
        }
        public ActionResult GenerateObjectsRealReport(string profile, string startdate, string enddate, string col, string customfields)
        {
            try
            {
                int numusers = 0;
                string nameprofile = "";

                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                JArray profilesx = JsonConvert.DeserializeObject<JArray>(profile);
                JArray customfieldsx = new JArray();
                try
                {
                    customfieldsx = JsonConvert.DeserializeObject<JArray>(customfields);
                }
                catch (Exception ex) { }
                List<string> filterprofile = new List<string>();
                Dictionary<string, string> customfieldslist = new Dictionary<string, string>();
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }



                Dictionary<string, string> datacols = new Dictionary<string, string>();

                foreach (JObject x in profilesx)
                {
                    filterprofile.Add(x["data"].ToString());
                }
                foreach (JObject x in customfieldsx)
                {
                    customfieldslist.Add(x["data"].ToString(), x["value"].ToString());
                }

                foreach (JObject x in cols)
                {
                    try
                    {
                       
                            datacols.Add(x["data"].ToString(), x["value"].ToString());
                    }
                    catch
                    {

                    }
                }
                DateTime datenow1 = DateTime.Now;
                int yearnow = datenow1.Year;
                int monthnow = datenow1.Month;
                int period = datenow1.Year - 2006;

              
                
                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                Dictionary<string, string> profiles = new Dictionary<string, string>();
                Dictionary<string, string> unumber = new Dictionary<string, string>();
                Dictionary<string, string> childrendict = new Dictionary<string, string>();
                ViewData["dates"] = dates;
                if (profile == "0")
                {
                    string getprofiles = locationsdb.GetRows();
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());
                        nameprofile = "Todos";
                    }
                }
                else
                {
                    /*   string getprofiles = profilesdb.GetRow(profile);
                      JObject profilesja = JsonConvert.DeserializeObject<JObject>(getprofiles);

                       profiles.Add(profilesja["_id"].ToString(), profilesja["name"].ToString());
                       nameprofile = profilesja["name"].ToString();*/

                    string getprofiles = locationsdb.GetRowsFilter(filterprofile);
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    nameprofile = "";

                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());

                        try
                        {
                            unumber.Add(x["_id"].ToString(), x["number"].ToString());
                        }
                        catch
                        {

                        }


                        nameprofile += x["name"] + ",";



                    }
                }
                if (nameprofile.Length > 50)
                {

                    nameprofile = nameprofile.Substring(0, 47);
                    nameprofile += "...";
                }
                List<string> childrenslocations = new List<string>();
                try
                {
                    JArray childrens = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(filterprofile));
                    childrenslocations = (from child in childrens select (string)child["_id"]).ToList();
                    childrendict = childrens.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                }
                catch { }
                //find sublucations
                try
                {
                    JArray childrens = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(childrenslocations));
                    List<string> childrenslocations2 = (from child in childrens select (string)child["_id"]).ToList();
                    childrenslocations.AddRange(childrenslocations2);
                   Dictionary<string,string> childrendict2 = childrens.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                    foreach(var item in childrendict2)
                    {
                        try
                        {
                            childrendict.Add(item.Key, item.Value);
                        }
                        catch { }
                    }
                }
                catch { }
                try
                {
                    childrenslocations.AddRange(filterprofile);
                }
                catch { }
                //end find sublocations
                string getuser = ObjectsRealdb.GetRowsReportObjectsReal(childrenslocations, datacols, start, end);
                JArray usersja = JsonConvert.DeserializeObject<JArray>(getuser);
                string joinresult = ObjectsRealdb.GetObjectsRealTable(filterprofile);
                JArray joinresultja = JsonConvert.DeserializeObject<JArray>(joinresult);

                JArray result = new JArray();
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();
                // Dictionary<string, JObject> listprovdict = new Dictionary<string, JObject>();
                // Dictionary<string, JObject> listdepartmentdict = new Dictionary<string, JObject>();
                JObject listprov = new JObject();
                JObject listdepart = new JObject();
                try
                {
                    listprov = JsonConvert.DeserializeObject<JArray>(listdb.Get("name", "proveedores")).First() as JObject;
                    //listprovdict = listprov.ToDictionary(x=>(string)x["_id"].ToString(), x=>(JObject)x);
                }
                catch
                {

                }
                try
                {
                    listdepart = JsonConvert.DeserializeObject<JArray>(listdb.Get("name", "departments")).First() as JObject;
                    // listdepartmentdict = listdepart.ToDictionary(x => (string)x["_id"].ToString(), x => (JObject)x);
                }
                catch
                {

                }
                Dictionary<string, JObject> listrefobj = new Dictionary<string, JObject>();

              /*  try
                {
                    List<string> listref = (from obj in usersja select (string)obj["objectReference"]).ToList();
                    JArray jaref = JsonConvert.DeserializeObject<JArray>(ObjectsRealdb.GetbyCustom("_id", listref, "ReferenceObjects"));

                    foreach (JObject item in jaref)
                    {
                        try
                        {
                            listrefobj.Add(item["_id"].ToString(), item);
                        }
                        catch { }
                    }
                }
                catch { }*/
                JArray cataloglocations = getCatalogLocations(childrenslocations);
                JArray categoriesja = new JArray();
                Dictionary<string, string> categoriesdict = new Dictionary<string, string>();
                try
                {
                    categoriesja = JsonConvert.DeserializeObject<JArray>(Categoriesdb.GetRows());
                    foreach (JObject cat in categoriesja)
                    {
                        try
                        {
                            categoriesdict.Add(cat["_id"].ToString(), cat["name"].ToString());
                        }
                        catch { }
                    }
                }
                catch { }
                foreach(JObject item in usersja)
                {

                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    JToken tokn;
                    try
                    {
                        if (!item.TryGetValue("assetsType", out tokn))
                        {
                            item.Add("assetsType", "");
                        }
                        string cat="";
                        if (categoriesdict.TryGetValue(item["parentCategory"].ToString(), out cat))
                        {
                            item["assetsType"] = cat;
                        }
                    }
                    catch
                    {

                    }
                    if (!item.TryGetValue("number", out tokn))
                    {
                        item.Add("number", "NA");
                    }

                    try
                    {
                        if (item.TryGetValue("system_status", out tokn))
                        {
                            item["system_status"] = (item["system_status"].ToString().ToLower() == "true") ? "Activo" : "Baja";
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (!item.TryGetValue("quantity", out tokn))
                        {
                            item.Add("quantity", "1");
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("department", out tokn))
                        {

                            foreach (JObject unorder in listdepart["elements"]["unorder"])
                            {

                                JToken untk;
                                if (unorder.TryGetValue(item["department"].ToString(), out untk))
                                {
                                    item["department"] = untk.ToString();
                                    break;
                                }


                            }



                        }
                        {

                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("vale", out tokn))
                        {
                            if (item["vale"].ToString().Length > 0)
                            {
                                item["vale"] = "<a href='/Uploads/Images/ObjectsFiles/" + item["vale"].ToString() + "' title='" + item["vale"].ToString() + "' target='_blank' >  Ver Archivo  </a>";
                            }
                            else
                            {
                                item["vale"] ="sin archivo";
                            }
                        }
                        else
                        {
                            item.Add("vale", "sin archivo");
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("filefactura", out tokn))
                        {
                            if (item["filefactura"].ToString().Length > 0)
                            {
                                item["filefactura"] = "<a href='/Uploads/Images/ObjectsFiles/" + item["filefactura"].ToString() + "' title='" + item["filefactura"].ToString() + "' target='_blank' >  Ver Archivo  </a>";
                            }
                            else
                            {
                                item["filefactura"] = "sin archivo";
                            }
                        }
                        else
                        {
                            item.Add("filefactura", "sin archivo");
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("objectfile", out tokn))
                        {
                            if (item["objectfile"].ToString().Length > 0)
                            {
                                item["objectfile"] = "<a href='/Uploads/Images/ObjectsFiles/" + item["objectfile"].ToString() + "' title='" + item["objectfile"].ToString() + "' target='_blank' >  Ver Archivo  </a>";
                            }
                            else
                            {
                                item["objectfile"] = "sin archivo";
                            }
                        }
                        else
                        {
                            item.Add("objectfile", "sin archivo");
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("proveedor", out tokn))
                        {

                            foreach (JObject order in listprov["elements"]["order"])
                            {
                                JToken untk;
                                if (order.TryGetValue(item["proveedor"].ToString(), out untk))
                                {
                                    item["proveedor"] = untk.ToString();
                                    break;
                                }
                            }
                            {

                            }
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        string unumberconj;

                        if (item.TryGetValue("parent", out tokn))
                        {
                            if (unumber.TryGetValue(item["parent"].ToString(), out unumberconj))
                            {
                                if (item.TryGetValue("number", out tokn))
                                {
                                    item["number"] = unumberconj;
                                }
                                else
                                {
                                    item.Add("number", unumberconj);
                                }

                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            // item.Add("parent", "");
                        }
                    }
                    catch { }
                    try
                    {
                        string conjunt;

                        if (item.TryGetValue("parent", out tokn))
                        {
                            if (profiles.TryGetValue(item["parent"].ToString(), out conjunt))
                            {
                                item["parent"] = conjunt;
                            }
                            else
                            {
                                item["parent"] = "";
                            }
                        }
                        else
                        {
                            item.Add("parent", "");
                        }
                    }
                    catch { }

                    try
                    {
                        foreach (var fields in customfieldslist)
                        {
                            bool stop = false;
                            foreach (JObject jointable in joinresultja)
                            {
                                string idact = jointable["_id"].ToString();
                                if (idact == item["_id"].ToString())
                                {
                                    foreach (JProperty joincustoms in jointable["customfields"])
                                    {
                                        string namekey = joincustoms.Name;
                                        namekey = namekey.Replace("_HTKField", "");
                                        if (namekey == fields.Key && idact == item["_id"].ToString())
                                        {
                                            try
                                            {
                                                item.Add(fields.Key, joincustoms.Value);
                                                stop = true;
                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                continue;
                                            }
                                        }

                                    }
                                    if (stop == false)
                                    {
                                        try
                                        {
                                            item.Add(fields.Key, "");
                                        }
                                        catch (Exception ex)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";
                        string aux1 = item["location"].ToString();
                        try
                        {
                            try
                            {
                                foreach (JObject ls in cataloglocations)
                                {
                                    try
                                    {
                                        if (ls["conjunto"]["id"].ToString() == item["location"].ToString())
                                        {
                                            item["location"] = ls["conjunto"]["name"].ToString();
                                            item["parent"] = ls["conjunto"]["name"].ToString();
                                            item["number"] = ls["conjunto"]["number"].ToString();
                                            break;
                                        }
                                        if (ls["ubicacion"]["id"].ToString() == item["location"].ToString())
                                        {
                                            item["location"] = ls["ubicacion"]["name"].ToString();
                                            item["parent"] = ls["conjunto"]["name"].ToString();
                                            item["number"] = ls["conjunto"]["number"].ToString();
                                            break;
                                        }
                                        if (ls["sububicacion"]["id"].ToString() == item["location"].ToString())
                                        {
                                            item["location"] = ls["sububicacion"]["name"].ToString();
                                            item["parent"] = ls["conjunto"]["name"].ToString();
                                            item["number"] = ls["conjunto"]["number"].ToString();
                                            break;
                                        }
                                    
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            catch { }
                        }
                        catch { }
                        if (childrendict.TryGetValue(aux1, out value))
                        {
                           // item["location"] = value;

                            int val = 0;
                            int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                            int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                            if (month.ToString().Substring(0, 1) == "0")
                            {

                                month = Convert.ToInt16(month.ToString().Substring(1));

                            }
                            int[] arraylinex = aux.ToArray();
                            if (graph.TryGetValue(value, out val))
                            {
                                graph[value] = graph[value] + 1;
                                arraylinex = auxgraph[item["location"].ToString()];
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["location"].ToString()] = arraylinex;
                            }
                            else
                            {

                                graph.Add(value, 1);

                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["location"].ToString()] = arraylinex;
                            }
                        }

                        // }
                        result.Add(item);
                        numusers++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }

                ViewData["profiles"] = nameprofile;
                ViewData["numusers"] = numusers.ToString();
                ViewData["dates"] = datesarray;
                foreach (JObject x in customfieldsx)
                {
                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                /*  string rut = exp(result, nameprofile, numusers.ToString(), datacols, datesarray, "Usuarios", "Perfil(es)");
                  ViewData["url"] = rut;
                  this.Session["headuser"] = datacols;
                  this.Session["userpdf"] = result;
                  this.Session["filtername"] = nameprofile;
                  this.Session["numtotal"] = numusers.ToString();
                  this.Session["getdates"] = datesarray;
                  this.Session["namereport"] = "Usuarios";
                  this.Session["typefilter"] = "Perfil(es)";*/
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }
        }
        public ActionResult GenerateObjectsRealReport3(string profile, string startdate, string enddate, string col, string customfields,string idreport=null)
        {
            try
            {
                int numusers = 0;
                string nameprofile = "";

                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                JArray profilesx = JsonConvert.DeserializeObject<JArray>(profile);
                JArray customfieldsx = new JArray();
                try
                {
                    customfieldsx = JsonConvert.DeserializeObject<JArray>(customfields);
                }
                catch (Exception ex) { }
                List<string> filterprofile = new List<string>();
                Dictionary<string, string> customfieldslist = new Dictionary<string, string>();
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }



                Dictionary<string, string> datacols = new Dictionary<string, string>();

                foreach (JObject x in profilesx)
                {
                    filterprofile.Add(x["data"].ToString());
                }
                foreach (JObject x in customfieldsx)
                {
                    customfieldslist.Add(x["data"].ToString(), x["value"].ToString());
                }

                foreach (JObject x in cols)
                {
                    try
                    {
                        if (x["data"].ToString() != "price" && x["data"].ToString() != "location" && x["data"].ToString() != "parent" && x["data"].ToString() != "number")
                            datacols.Add(x["data"].ToString(), x["value"].ToString());
                    }
                    catch
                    {

                    }
                }
                DateTime datenow1 = DateTime.Now;
                int yearnow = datenow1.Year;
                int monthnow = datenow1.Month;
                int period = datenow1.Year - 2006;

                datacols.Add("depreciacion", "% de Dep.");
                datacols.Add("date", "Fecha de Adq.");
                datacols.Add("datenow", "Fecha de Referencia");
                datacols.Add("price", "M.O.I.");
                datacols.Add("months", "Meses de Uso");
                datacols.Add("depcont", "Dep. Contable");
                datacols.Add("indexdate", "Indice Fecha de Adq.");
                datacols.Add("indexdate2", "Indice 1/2 Periodo Uso");
                datacols.Add("factor", "Factor de Act.");
                datacols.Add("depact", "Dep Act.");
              /*  for (int i = 0; i < period; i++)
                {
                    datacols.Add("year-" + Convert.ToString(2007 + i), Convert.ToString(2007 + i));
                }*/
                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                Dictionary<string, string> profiles = new Dictionary<string, string>();
                Dictionary<string, string> unumber = new Dictionary<string, string>();
                Dictionary<string, string> childrendict = new Dictionary<string, string>();
                ViewData["dates"] = dates;
                if (profile == "0")
                {
                    string getprofiles = locationsdb.GetRows();
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());
                        nameprofile = "Todos";
                    }
                }
                else
                {
                    /*   string getprofiles = profilesdb.GetRow(profile);
                      JObject profilesja = JsonConvert.DeserializeObject<JObject>(getprofiles);

                       profiles.Add(profilesja["_id"].ToString(), profilesja["name"].ToString());
                       nameprofile = profilesja["name"].ToString();*/

                    string getprofiles = locationsdb.GetRowsFilter(filterprofile);
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    nameprofile = "";

                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());

                        try
                        {
                            unumber.Add(x["_id"].ToString(), x["number"].ToString());
                        }
                        catch
                        {

                        }


                        nameprofile += x["name"] + ",";



                    }
                }
                if (nameprofile.Length > 50)
                {

                    nameprofile = nameprofile.Substring(0, 47);
                    nameprofile += "...";
                }
                List<string> childrenslocations = new List<string>();
                try
                {
                    JArray childrens = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(filterprofile));
                    childrenslocations = (from child in childrens select (string)child["_id"]).ToList();
                    childrendict = childrens.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                }
                catch { }
                try
                {
                    childrenslocations.AddRange(filterprofile);
                }
                catch { }
                string getuser = ObjectsRealdb.GetRowsReportObjectsReal(childrenslocations, datacols, start, end,idreport);
                JArray usersja = JsonConvert.DeserializeObject<JArray>(getuser);
                string joinresult = ObjectsRealdb.GetObjectsRealTable(filterprofile);
                JArray joinresultja = JsonConvert.DeserializeObject<JArray>(joinresult);

                JArray result = new JArray();
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();
                // Dictionary<string, JObject> listprovdict = new Dictionary<string, JObject>();
                // Dictionary<string, JObject> listdepartmentdict = new Dictionary<string, JObject>();
                JObject listprov = new JObject();
                JObject listdepart = new JObject();
                try
                {
                    listprov = JsonConvert.DeserializeObject<JArray>(listdb.Get("name", "proveedores")).First() as JObject;
                    //listprovdict = listprov.ToDictionary(x=>(string)x["_id"].ToString(), x=>(JObject)x);
                }
                catch
                {

                }
                try
                {
                    listdepart = JsonConvert.DeserializeObject<JArray>(listdb.Get("name", "departments")).First() as JObject;
                    // listdepartmentdict = listdepart.ToDictionary(x => (string)x["_id"].ToString(), x => (JObject)x);
                }
                catch
                {

                }
                Dictionary<string, string> listdict = new Dictionary<string, string>();
                try
                {
                    JObject listdep = JsonConvert.DeserializeObject<JArray>(listdb.Get("name", "depreciacion")).FirstOrDefault() as JObject;

                    foreach (var element in listdep["elements"]["unorder"])
                    {
                        try
                        {
                            foreach (JProperty item in element)
                            {
                                try
                                {
                                    listdict.Add(item.Name.ToString(), item.Value.ToString());
                                }
                                catch { }
                            }
                        }
                        catch
                        {

                        }

                    }
                }
                catch { }
                Dictionary<string, string> catdep = new Dictionary<string, string>();
                try
                {
                    String categoriesjson = ObjectsRealdb.GetCategories();
                    JArray categsx = JsonConvert.DeserializeObject<JArray>(categoriesjson);
                    catdep = categsx.ToDictionary(x => (string)x["_id"], x => (string)x["depreciacion"]);
                }
                catch { }              
                foreach (JObject itemorigin in usersja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    string yearref = "2007";
                    try
                    {
                        yearref = itemorigin["date"].ToString().Split('-')[0];
                    }
                    catch { }
                    string monthref = "01";
                    try
                    {
                        monthref = itemorigin["date"].ToString().Split('-')[1];
                    }
                    catch { }
                    string dayref = "01";
                    try
                    {
                        dayref = itemorigin["date"].ToString().Split('-')[2];
                    }
                    catch { }
                               
                 DateTime dateref = DateTime.ParseExact(dayref+"/"+monthref+"/"+yearref+" 00:00:00", "dd/MM/yyyy HH:mm:ss", null);
                 int firsmonth = 0;
                 try
                 {
                     JToken tokn;
                     if (!itemorigin.TryGetValue("depreciacion", out tokn))
                         itemorigin.Add("depreciacion", "0");
                     itemorigin["depreciacion"] = catdep[itemorigin["parentCategory"].ToString()];
                 }
                 catch
                 {
                     itemorigin["depreciacion"] = "0";
                 }
                    while(true){

                       

                  JObject item = new JObject(itemorigin);
                    JToken tokn;
                    DateTime dateinit;
                    DateTime datenow;
                    try
                    {
                        
                        item.Add("months", "0");
                        item.Add("depcont", "0");
                        item.Add("indexdate", "0");
                        item.Add("indexdate2", "0");
                        item.Add("factor", "0");
                        item.Add("depact", "0");
                        item.Add("datenow", dateref.Year.ToString()+"/"+dateref.Month.ToString());
                        int month1 = 0;
                        int year = 0;
                        if (item.TryGetValue("date", out tokn))
                        {
                            try
                            {
                                string dateb = item["date"].ToString();
                                string dateyear = item["date"].ToString().Split('-')[0];
                                string datemonth = item["date"].ToString().Split('-')[1];
                                string dateday = item["date"].ToString().Split('-')[2];
                                string datefull = dateday + "/" + datemonth + "/" + dateyear + " 00:00:00";
                                datenow = dateref;
                                dateinit = DateTime.ParseExact(datefull, "dd/MM/yyyy HH:mm:ss", null);
                                month1 = dateinit.Month;
                                year = dateinit.Year;

                                decimal diferences = 0;
                                
                                
                                     diferences = (datenow.Month - dateinit.Month) + 12 * (datenow.Year - dateinit.Year);

                                    if (datenow.Day < dateinit.Day)
                                    {
                                        diferences--;
                                    }
                                    if (diferences < 0)
                                    {
                                        DateTime now = DateTime.Now;
                                        if (now.Year == dateref.Year && now.Month == dateref.Month)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                             
                                            dateref = dateref.AddMonths(1);
                                           
                                        }
                                        
                                    }
                                    item["months"] = diferences.ToString();
                                //var ressultdates = datenow - dateinit;
                               
                            }
                            catch
                            {
                                item["months"] = "0";
                            }
                            try
                            {
                                if (Convert.ToInt16(item["months"].ToString()) <= 0)
                                {
                                    dateref = dateref.AddMonths(1);
                                    continue;
                                }
                            }
                            catch { }
                            if (item.TryGetValue("price", out tokn))
                            {

                                try
                                {
                                    decimal price = 0;
                                    decimal dep = 0;
                                    int months = 0;
                                    string pricevalid = "";
                                    try
                                    {

                                        try
                                        {
                                            foreach (char c in item["price"].ToString())
                                            {
                                                string cs = c.ToString();
                                                if (Regex.IsMatch(cs, "[0-9]") || cs == ".")
                                                {
                                                    if (cs == ".")
                                                        cs = ".";
                                                    pricevalid += cs;
                                                }


                                            }
                                            // price = Convert.ToDecimal(pricevalid,null);
                                            var style = System.Globalization.NumberStyles.AllowDecimalPoint;
                                            var culture = System.Globalization.CultureInfo.CreateSpecificCulture("es-ES");

                                            // if (Decimal.TryParse(pricevalid, out price)) { }
                                        }
                                        catch
                                        {

                                        }



                                        price = Convert.ToDecimal(pricevalid.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    catch { }
                                    try
                                    {
                                        // item["depreciacion"] = item["depreciacion"].ToString().Replace(".", ",");
                                        dep = Convert.ToDecimal(item["depreciacion"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                       // dep = dep / 100;
                                    }
                                    catch { }
                                    try { months = Convert.ToInt16(item["months"].ToString()); }
                                    catch { }
                                    decimal depcontresult = Math.Round((price * dep * months) / 12, 4);
                                    // item["depcont"] = depcontresult.ToString().Replace(".", ",");
                                    item["depcont"] = depcontresult.ToString();
                                }
                                catch
                                {

                                }

                            }
                            try
                            {


                                //find in the list year and month
                                // string indexresult = (Convert.ToDecimal(listdict[year + "/" + month1].Replace(".", ",")) / 100).ToString(); ;
                                string indexresult = (Convert.ToDecimal(listdict[year + "/" + month1], System.Globalization.CultureInfo.InvariantCulture)).ToString(); ;

                                item["indexdate"] = indexresult;

                            }
                            catch
                            {

                            }
                            try
                            {
                               // decimal indexdate = (12 - month1) / 2;
                               // int roundindex = Convert.ToInt16(Math.Round(indexdate, MidpointRounding.ToEven));
                                //  string indexdate2result =(Convert.ToDecimal(listdict[yearnow + "/" + roundindex].Replace(".", ","))/100).ToString();
                                yearnow = dateref.Year;
                                decimal indexdate = (12 - (month1 - 1)) / 2;
                                int roundindex = Convert.ToInt16(Math.Round(indexdate, MidpointRounding.ToEven));
                                decimal monthhalf = (month1 - 1) + roundindex;
                                string indexdate2result = "0.00";
                                if (yearnow > year)
                                    monthhalf = 6;
                              // if(yearnow<DateTime.Now.Year)
                                   indexdate2result = (Convert.ToDecimal(listdict[yearnow + "/" + monthhalf], System.Globalization.CultureInfo.InvariantCulture)).ToString();


                                item["indexdate2"] = indexdate2result;

                            }
                            catch
                            {

                            }
                            try
                            {



                                decimal resultfact = Convert.ToDecimal(item["indexdate2"].ToString(), System.Globalization.CultureInfo.InvariantCulture) / Convert.ToDecimal(item["indexdate"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                item["factor"] = Math.Round(resultfact, 4).ToString();//.Replace(".", ",");
                            }
                            catch
                            {

                            }
                            try
                            {
                                decimal resultact = Math.Round((Convert.ToDecimal(item["depcont"].ToString(), System.Globalization.CultureInfo.InvariantCulture) * Convert.ToDecimal(item["factor"].ToString(), System.Globalization.CultureInfo.InvariantCulture)), 4);
                                item["depact"] = Math.Round(resultact, 4).ToString();//.Replace(".", ",");
                            }
                            catch
                            {

                            }
                            try
                            {

                             

                               /* for (int i = 0; i < period; i++)
                                {
                                    try
                                    {
                                        int ind = 2007 + i;
                                        int resultyear = (ind) - year;
                                        string use = "";
                                        DateTime daynow = DateTime.Now;
                                        datenow = DateTime.Now;
                                        string dateyear = item["date"].ToString().Split('-')[0];
                                        string datemonth = item["date"].ToString().Split('-')[1];
                                        string dateday = item["date"].ToString().Split('-')[2];
                                        string datefull = dateday + "/" + datemonth + "/" + dateyear + " 00:00:00";
                                        dateinit = DateTime.ParseExact(datefull, "dd/MM/yyyy HH:mm:ss", null);


                                        if (ind == year)
                                        {

                                            if (year == yearnow)
                                            {
                                                use = (monthnow - month1).ToString();
                                                if (datenow.Day < dateinit.Day)
                                                {
                                                    use = ((monthnow - month1) - 1).ToString();
                                                }
                                            }
                                            else
                                            {
                                                use = (12 - month1).ToString();
                                            }


                                        }
                                        else
                                            if (ind < year)
                                            {

                                                use = "0";
                                            }
                                            else
                                            {
                                                if (ind == yearnow)
                                                {
                                                    use = (monthnow - 1).ToString();
                                                }
                                                else
                                                {
                                                    use = "12";
                                                }
                                            }

                                        item["year-" + ind.ToString()] = use;
                                    }
                                    catch
                                    {

                                    }

                                }*/
                            }
                            catch { }

                        }
                        else
                        {
                            item["depcont"] = "";
                            item["factor"] = "";
                            item["indexdate"] = "";
                            item["indexdate2"] = "";
                            item["depact"] = "";
                        }
                        try
                        {
                            /*  item["depcont"] = item["depcont"].ToString().Replace(",", ".");
                              item["factor"] = item["factor"].ToString().Replace(",", ".");
                              item["indexdate"] = item["indexdate"].ToString().Replace(",", ".");
                              item["indexdate2"] = item["indexdate2"].ToString().Replace(",", ".");
                              item["depact"] = item["depact"].ToString().Replace(",", ".");*/
                        }
                        catch { }
                    }
                    catch
                    {

                    }

                    try
                    {
                        if (item.TryGetValue("system_status", out tokn))
                        {
                            item["system_status"] = (item["system_status"].ToString().ToLower() == "true") ? "Activo" : "Baja";
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (!item.TryGetValue("quantity", out tokn))
                        {
                            item.Add("quantity", "1");
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("department", out tokn))
                        {

                            foreach (JObject unorder in listdepart["elements"]["unorder"])
                            {

                                JToken untk;
                                if (unorder.TryGetValue(item["department"].ToString(), out untk))
                                {
                                    item["department"] = untk.ToString();
                                    break;
                                }


                            }



                        }
                        {

                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("proveedor", out tokn))
                        {

                            foreach (JObject order in listprov["elements"]["order"])
                            {
                                JToken untk;
                                if (order.TryGetValue(item["proveedor"].ToString(), out untk))
                                {
                                    item["proveedor"] = untk.ToString();
                                    break;
                                }
                            }
                            {

                            }
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        string unumberconj;

                        if (item.TryGetValue("parent", out tokn))
                        {
                            if (unumber.TryGetValue(item["parent"].ToString(), out unumberconj))
                            {
                                if (item.TryGetValue("number", out tokn))
                                {
                                    item["number"] = unumberconj;
                                }
                                else
                                {
                                    item.Add("number", unumberconj);
                                }

                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            // item.Add("parent", "");
                        }
                    }
                    catch { }
                    try
                    {
                        string conjunt;

                        if (item.TryGetValue("parent", out tokn))
                        {
                            if (profiles.TryGetValue(item["parent"].ToString(), out conjunt))
                            {
                                item["parent"] = conjunt;
                            }
                            else
                            {
                                item["parent"] = "";
                            }
                        }
                        else
                        {
                            item.Add("parent", "");
                        }
                    }
                    catch { }

                    try
                    {
                        foreach (var fields in customfieldslist)
                        {
                            bool stop = false;
                            foreach (JObject jointable in joinresultja)
                            {
                                string idact = jointable["_id"].ToString();
                                if (idact == item["_id"].ToString())
                                {
                                    foreach (JProperty joincustoms in jointable["customfields"])
                                    {
                                        string namekey = joincustoms.Name;
                                        namekey = namekey.Replace("_HTKField", "");
                                        if (namekey == fields.Key && idact == item["_id"].ToString())
                                        {
                                            try
                                            {
                                                item.Add(fields.Key, joincustoms.Value);
                                                stop = true;
                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                continue;
                                            }
                                        }

                                    }
                                    if (stop == false)
                                    {
                                        try
                                        {
                                            item.Add(fields.Key, "");
                                        }
                                        catch (Exception ex)
                                        {
                                            continue;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";
                        if (childrendict.TryGetValue(item["location"].ToString(), out value))
                        {
                            item["location"] = value;

                            int val = 0;
                            int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                            int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                            if (month.ToString().Substring(0, 1) == "0")
                            {

                                month = Convert.ToInt16(month.ToString().Substring(1));

                            }
                            int[] arraylinex = aux.ToArray();
                            if (graph.TryGetValue(value, out val))
                            {
                                graph[value] = graph[value] + 1;
                                arraylinex = auxgraph[item["location"].ToString()];
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["location"].ToString()] = arraylinex;
                            }
                            else
                            {

                                graph.Add(value, 1);

                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["location"].ToString()] = arraylinex;
                            }
                        }

                        // }
                        try
                        {
                            foreach (var colsx in datacols)
                            {
                                if (!item.TryGetValue(colsx.Key, out tokn))
                                {
                                    item.Add(colsx.Key," ");
                                }
                            }
                        }
                        catch { }
                       

                       result.Add(item);
                        
                       
                        numusers++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                       DateTime now2 = DateTime.Now;
                       if (now2.Year == dateref.Year && now2.Month == dateref.Month)
                       {
                           break;
                       }
                       else
                       {
                          dateref= dateref.AddMonths(1);
                       }
                   }
                }

                ViewData["profiles"] = nameprofile;
                ViewData["numusers"] = numusers.ToString();
                ViewData["dates"] = datesarray;
                foreach (JObject x in customfieldsx)
                {
                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                /*  string rut = exp(result, nameprofile, numusers.ToString(), datacols, datesarray, "Usuarios", "Perfil(es)");
                  ViewData["url"] = rut;
                  this.Session["headuser"] = datacols;
                  this.Session["userpdf"] = result;
                  this.Session["filtername"] = nameprofile;
                  this.Session["numtotal"] = numusers.ToString();
                  this.Session["getdates"] = datesarray;
                  this.Session["namereport"] = "Usuarios";
                  this.Session["typefilter"] = "Perfil(es)";*/
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                ViewBag.title = "Reporte de Depreciación de Activos Detallado";
                if (idreport != null)
                {
                    return PartialView("GenerateObjectsRealdetail", result);
                }
                else
                {
                    return PartialView("GenerateObjectsRealReport2", result);
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public ActionResult GenerateObjectsRealReport2(string profile, string startdate, string enddate, string col, string customfields,int typereport=0,string idreport=null)
        {
            try
            {
                try
                {
                    ViewData["profilereport"] = profile;
                    ViewData["startdatereport"] = startdate;
                    ViewData["enddatereport"] = enddate;
                    ViewData["colreport"] = col;
                    ViewData["customfieldsreport"] = customfields;
                  

                }
                catch { }
                int numusers = 0;
                string nameprofile = "";

                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                JArray profilesx = JsonConvert.DeserializeObject<JArray>(profile);
                JArray customfieldsx = new JArray();
                try
                {
                    customfieldsx = JsonConvert.DeserializeObject<JArray>(customfields);
                }
                catch (Exception ex) { }
                List<string> filterprofile = new List<string>();
                Dictionary<string, string> customfieldslist = new Dictionary<string, string>();
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }



                Dictionary<string, string> datacols = new Dictionary<string, string>();

                foreach (JObject x in profilesx)
                {
                    filterprofile.Add(x["data"].ToString());
                }
                foreach (JObject x in customfieldsx)
                {
                    customfieldslist.Add(x["data"].ToString(), x["value"].ToString());
                }

                datacols.Add("detail", "Detalle");
                foreach (JObject x in cols)
                {
                    try
                    {
                        if (x["data"].ToString() != "price")
                            datacols.Add(x["data"].ToString(), x["value"].ToString());
                    }
                    catch
                    {

                    }
                }
                DateTime datenow1 = DateTime.Now;
                int yearnow = datenow1.Year;
                int monthnow = datenow1.Month;
                int period = datenow1.Year - 2006;
 
                    datacols.Add("depreciacion", "% de Dep.");
                    datacols.Add("date", "Fecha de Adq.");
                    datacols.Add("price", "M.O.I");
                    datacols.Add("months", "Meses de Uso");
                    datacols.Add("depcont", "Dep. Contable");
                    datacols.Add("indexdate", "Indice Fecha de Adq.");
                    datacols.Add("indexdate2", "Indice 1/2 Periodo Uso");
                    datacols.Add("factor", "Factor de Act.");
                    datacols.Add("depact", "Dep Act.");
               
                
                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                Dictionary<string, string> profiles = new Dictionary<string, string>();
                Dictionary<string, string> unumber = new Dictionary<string, string>();
                Dictionary<string, string> childrendict = new Dictionary<string, string>();
                ViewData["dates"] = dates;
                if (profile == "0")
                {
                    string getprofiles = locationsdb.GetRows();
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());
                        nameprofile = "Todos";
                    }
                }
                else
                {
                    /*   string getprofiles = profilesdb.GetRow(profile);
                      JObject profilesja = JsonConvert.DeserializeObject<JObject>(getprofiles);

                       profiles.Add(profilesja["_id"].ToString(), profilesja["name"].ToString());
                       nameprofile = profilesja["name"].ToString();*/

                    string getprofiles = locationsdb.GetRowsFilter(filterprofile);
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    nameprofile = "";
                  
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());

                        try
                        {
                            unumber.Add(x["_id"].ToString(), x["number"].ToString());
                        }
                        catch
                        {

                        }


                        nameprofile += x["name"] + ",";



                    }
                }
                if (nameprofile.Length > 50)
                {

                    nameprofile = nameprofile.Substring(0, 47);
                    nameprofile += "...";
                }
                List<string> childrenslocations = new List<string>();
                try
                {
                    JArray childrens = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(filterprofile));
                    childrenslocations = (from child in childrens select (string)child["_id"]).ToList();
                    childrendict = childrens.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                }
                catch { }

                //fin sublucations
                try
                {
                    JArray childrens = JsonConvert.DeserializeObject<JArray>(locationsdb.GetChildrens(childrenslocations));
                    List<string> childrenslocations2 = (from child in childrens select (string)child["_id"]).ToList();
                    childrenslocations.AddRange(childrenslocations2);
                    Dictionary<string, string> childrendict2 = childrens.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                    foreach (var item in childrendict2)
                    {
                        try
                        {
                            childrendict.Add(item.Key, item.Value);
                        }
                        catch { }
                    }
                }
                catch { }
                try
                {
                    childrenslocations.AddRange(filterprofile);
                }
                catch { }
                //end find sublocations
                string getuser = ObjectsRealdb.GetRowsReportObjectsReal(childrenslocations, datacols, start, end,idreport);
                JArray usersja = JsonConvert.DeserializeObject<JArray>(getuser);
                string joinresult = ObjectsRealdb.GetObjectsRealTable(filterprofile);
                JArray joinresultja = JsonConvert.DeserializeObject<JArray>(joinresult);
               
                JArray result = new JArray();
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();
               // Dictionary<string, JObject> listprovdict = new Dictionary<string, JObject>();
               // Dictionary<string, JObject> listdepartmentdict = new Dictionary<string, JObject>();
                JObject listprov = new JObject();
                JObject listdepart = new JObject();
                try
                {
                    listprov = JsonConvert.DeserializeObject<JArray>(listdb.Get("name", "proveedores")).First() as JObject;
                    //listprovdict = listprov.ToDictionary(x=>(string)x["_id"].ToString(), x=>(JObject)x);
                }
                catch
                {

                }
                try
                {
                     listdepart = JsonConvert.DeserializeObject<JArray>(listdb.Get("name", "departments")).First() as JObject;
                   // listdepartmentdict = listdepart.ToDictionary(x => (string)x["_id"].ToString(), x => (JObject)x);
                }
                catch
                {

                }
                Dictionary<string, string> listdict = new Dictionary<string, string>();
                try
                {
                    JObject listdep = JsonConvert.DeserializeObject<JArray>(listdb.Get("name", "depreciacion")).FirstOrDefault() as JObject;

                    foreach (var element in listdep["elements"]["unorder"])
                    {
                        try
                        {
                            foreach (JProperty item in element)
                            {
                                try
                                {
                                    listdict.Add(item.Name.ToString(), item.Value.ToString());
                                }
                                catch { }
                            }
                        }
                        catch
                        {

                        }

                    }
                }
                catch { }
                Dictionary<string, string> catdep = new Dictionary<string, string>();
                try
                {
                    String categoriesjson = ObjectsRealdb.GetCategories();
                    JArray categsx = JsonConvert.DeserializeObject<JArray>(categoriesjson);
                    catdep = categsx.ToDictionary(x => (string)x["_id"], x => (string)x["depreciacion"]);
                }
                catch { }
                JArray cataloglocations = getCatalogLocations(childrenslocations);
                //new code
                int yearinitvalue = 2007;
                int yearbefore = 2006;
               
                if (typereport == 1)
                {
                    try
                    {
                        JObject dataobject = usersja.First() as JObject;
                        yearinitvalue = Convert.ToInt16(dataobject["date"].ToString().Split('-')[0]);
                        yearbefore = yearinitvalue - 1;
                    }
                    catch { }
                    period = datenow1.Year - yearbefore;
                    for (int i = 0; i < period; i++)
                    {
                        datacols.Add("year-" + Convert.ToString(yearinitvalue + i), Convert.ToString(yearinitvalue + i));
                    }
                }
                else
                {
                    datacols.Add("plus", "Acción");
                }
                decimal globaldep = 0;
                decimal globalprice = 0;
                string globalindex = "0";
                foreach (JObject item in usersja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    JToken tokn;
                    DateTime dateinit;
                    DateTime datenow;
                    try
                    {
                        if (!item.TryGetValue("depreciacion", out tokn))
                            item.Add("depreciacion", "0");
                        item["depreciacion"] = catdep[item["parentCategory"].ToString()];
                        
                    }
                    catch {
                        item["depreciacion"] = "0";
                    }
                    try
                    {
                        item.Add("months", "0");
                        item.Add("depcont","0");
                        item.Add("indexdate", "0");
                        item.Add("indexdate2", "0");
                        item.Add("factor", "0");
                        item.Add("depact", "0");
                       
                        int month1 = 0;
                        int year = 0;
                        if (item.TryGetValue("date", out tokn))
                        {
                            try
                            {
                                string dateb = item["date"].ToString();
                                string dateyear = item["date"].ToString().Split('-')[0];
                                string datemonth = item["date"].ToString().Split('-')[1];
                                string dateday = item["date"].ToString().Split('-')[2];
                                string datefull = dateday + "/" + datemonth + "/" + dateyear+" 00:00:00";
                                datenow = DateTime.Now;
                                dateinit = DateTime.ParseExact(datefull, "dd/MM/yyyy HH:mm:ss", null);
                                month1 = dateinit.Month;
                                year = dateinit.Year;
                                decimal diferences = Math.Abs((datenow.Month - dateinit.Month) + 12 * (datenow.Year - dateinit.Year));

                                if (datenow.Day < dateinit.Day)
                                {
                                    diferences--;
                                }

                                item["months"] = diferences.ToString();
                            }
                            catch
                            {
                                item["months"] = "0";
                            }
                            if (item.TryGetValue("price", out tokn))
                            {

                                try
                                {
                                    decimal price = 0;
                                    decimal dep = 0;
                                    int months = 0;
                                    string pricevalid="";
                                    try {

                                        try {
                                            foreach (char c in item["price"].ToString())
                                            {
                                                string cs = c.ToString();
                                                if (Regex.IsMatch(cs, "[0-9]") || cs == ".")
                                                {
                                                    if (cs == ".")
                                                        cs = ".";
                                                    pricevalid += cs;
                                                }

                                                
                                            }
                                           // price = Convert.ToDecimal(pricevalid,null);
                                           var style =System.Globalization.NumberStyles.AllowDecimalPoint;
                                           var culture =System.Globalization.CultureInfo.CreateSpecificCulture("es-ES");
                                           
                                           // if (Decimal.TryParse(pricevalid, out price)) { }
                                            }catch{

                                            }



                                        price = Convert.ToDecimal(pricevalid.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                        globalprice = price;
                                    }
                                    catch { }
                                    try {
                                       // item["depreciacion"] = item["depreciacion"].ToString().Replace(".", ",");
                                        dep = Convert.ToDecimal(item["depreciacion"].ToString(),System.Globalization.CultureInfo.InvariantCulture);
                                        //dep = dep / 100;
                                        globaldep = dep;
                                     }
                                    catch { }
                                    try { months = Convert.ToInt16(item["months"].ToString()); }
                                    catch { }
                                    decimal depcontresult = Math.Round((price * dep * months) /12, 4) ;
                                   // item["depcont"] = depcontresult.ToString().Replace(".", ",");
                                    item["depcont"] = depcontresult.ToString();
                                }
                                catch
                                {
                                   
                                }

                            }
                            try
                            {
                                
                              
                                //find in the list year and month
                               // string indexresult = (Convert.ToDecimal(listdict[year + "/" + month1].Replace(".", ",")) / 100).ToString(); ;
                                string indexresult = (Convert.ToDecimal(listdict[year + "/" + month1], System.Globalization.CultureInfo.InvariantCulture)).ToString(); ;
                               
                                item["indexdate"] = indexresult;
                                globalindex = indexresult;
                               
                            }
                            catch
                            {

                            }
                            try
                            {

                               // decimal indexdate = (12 - month1) / 2;
                                decimal indexdate = (12 - (month1-1))/2;
                                int roundindex =Convert.ToInt16( Math.Round(indexdate,MidpointRounding.ToEven));
                                decimal monthhalf = (month1 - 1) + roundindex;
                                string indexdate2result = "0.00";
                                if(yearnow>year)
                                     indexdate2result = (Convert.ToDecimal(listdict[year + "/" + monthhalf], System.Globalization.CultureInfo.InvariantCulture)).ToString();
                               
                                    
                                item["indexdate2"] = indexdate2result;
                               
                            }
                            catch
                            {
                                
                            }
                            try
                            {


                                
                                decimal resultfact = Convert.ToDecimal(item["indexdate2"].ToString(),System.Globalization.CultureInfo.InvariantCulture) / Convert.ToDecimal(item["indexdate"].ToString(),System.Globalization.CultureInfo.InvariantCulture);
                                item["factor"] = Math.Round(resultfact,4).ToString();//.Replace(".", ",");
                            }
                            catch
                            {
                               
                            }
                            try
                            {
                                decimal resultact = Math.Round(( Convert.ToDecimal(item["depcont"].ToString(),System.Globalization.CultureInfo.InvariantCulture) * Convert.ToDecimal(item["factor"].ToString(),System.Globalization.CultureInfo.InvariantCulture)), 4);
                                 item["depact"] = resultact.ToString();//.Replace(".", ",");
                            }
                            catch
                            {

                            }
                            try
                            {
                                if (typereport == 1)
                                {
                                    int countmonths = 0;
                                    for (int i = 0; i < period; i++)
                                    {
                                        try
                                        {

                                            int ind = yearinitvalue + i;
                                            int resultyear = (ind) - year;
                                            string use = "";
                                            DateTime daynow = DateTime.Now;
                                            datenow = DateTime.Now;
                                            string dateyear = item["date"].ToString().Split('-')[0];
                                            string datemonth = item["date"].ToString().Split('-')[1];
                                            string dateday = item["date"].ToString().Split('-')[2];
                                            string datefull = dateday + "/" + datemonth + "/" + dateyear + " 00:00:00";
                                            dateinit = DateTime.ParseExact(datefull, "dd/MM/yyyy HH:mm:ss", null);
                                            //new code


                                            //end new code

                                            if (ind == year)
                                            {

                                                if (year == yearnow)
                                                {
                                                    use = (monthnow - month1).ToString();
                                                    if (datenow.Day < dateinit.Day)
                                                    {
                                                        use = ((monthnow - month1) - 1).ToString();
                                                    }
                                                }
                                                else
                                                {
                                                    use = (12 - month1).ToString();
                                                }


                                            }
                                            else
                                                if (ind < year)
                                                {

                                                    use = "0";
                                                }
                                                else
                                                {
                                                    if (ind == yearnow)
                                                    {
                                                        use = (monthnow - 1).ToString();
                                                    }
                                                    else
                                                    {
                                                        use = "12";
                                                    }
                                                }
                                            countmonths = countmonths + Convert.ToInt16(use);
                                            int monthuse = countmonths;
                                           
                                           
                                            decimal depcontresult = 0;
                                            try
                                            {
                                                depcontresult = Math.Round((globalprice * globaldep * monthuse) / 12, 4);
                                            }
                                            catch { }
                                            string indexdate2result = "0.00";
                                            try
                                            {
                                                decimal indexdate = (12 - (month1 - 1)) / 2;
                                                int roundindex = Convert.ToInt16(Math.Round(indexdate, MidpointRounding.ToEven));
                                                decimal monthhalf = (month1 - 1) + roundindex;
                                              
                                                if (ind > year)
                                                    monthhalf = 6;
                                                if (ind < DateTime.Now.Year)
                                                    indexdate2result = (Convert.ToDecimal(listdict[ind + "/" + monthhalf], System.Globalization.CultureInfo.InvariantCulture)).ToString();
                                            }
                                            catch
                                            {

                                            }
                                            decimal factor1 = 0;
                                            try
                                            {
                                                decimal resultfact1 = Convert.ToDecimal(indexdate2result, System.Globalization.CultureInfo.InvariantCulture) / Convert.ToDecimal(globalindex, System.Globalization.CultureInfo.InvariantCulture);
                                                factor1 = Math.Round(resultfact1, 4);//.Replace(".", ",");
                           
                                            }
                                            catch { }
                                            decimal depact = Math.Round(depcontresult * factor1, 4);
                                            item["year-" + ind.ToString()] = depact.ToString();
                                        }
                                        catch
                                        {

                                        }

                                    }
                                }
                            }
                            catch { }

                        }
                        else
                        {
                            item["depcont"] = "";
                            item["factor"] = "";
                            item["indexdate"] = "";
                            item["indexdate2"] = "";
                            item["depact"] = "";
                        }
                        try
                        {
                          /*  item["depcont"] = item["depcont"].ToString().Replace(",", ".");
                            item["factor"] = item["factor"].ToString().Replace(",", ".");
                            item["indexdate"] = item["indexdate"].ToString().Replace(",", ".");
                            item["indexdate2"] = item["indexdate2"].ToString().Replace(",", ".");
                            item["depact"] = item["depact"].ToString().Replace(",", ".");*/
                        }
                        catch { }
                    }
                    catch
                    {

                    }

                    try
                    {
                        if (item.TryGetValue("system_status", out tokn))
                        {
                            item["system_status"] = (item["system_status"].ToString().ToLower() == "true") ? "Activo" : "Baja";
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (!item.TryGetValue("quantity", out tokn))
                        {
                            item.Add("quantity", "1");
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("department", out tokn))
                        {

                            foreach (JObject unorder in listdepart["elements"]["unorder"])
                                    {
                                        
                                            JToken untk;
                                            if (unorder.TryGetValue(item["department"].ToString(), out untk))
                                            {
                                                item["department"] = untk.ToString();
                                                break;
                                            }
                                        
                                       
                                    }

                     

                        }
                        {

                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        if (item.TryGetValue("proveedor", out tokn))
                        {

                            foreach (JObject order in listprov["elements"]["order"])
                            {
                                 JToken untk;
                                        if (order.TryGetValue(item["proveedor"].ToString(), out untk))
                                        {
                                            item["proveedor"] = untk.ToString();
                                            break;
                                        }
                              }
                            {

                            }
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        string unumberconj;

                        if (item.TryGetValue("parent", out tokn))
                        {
                            if (unumber.TryGetValue(item["parent"].ToString(), out unumberconj))
                            {
                                if (item.TryGetValue("number", out tokn))
                                {
                                    item["number"] = unumberconj;
                                }
                                else
                                {
                                    item.Add("number", unumberconj);
                                }

                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            // item.Add("parent", "");
                        }
                    }
                    catch { }
                    try
                    {
                        string conjunt;
                     
                        if (item.TryGetValue("parent", out tokn))
                        {
                            if (profiles.TryGetValue(item["parent"].ToString(), out conjunt))
                            {
                                item["parent"] = conjunt;
                            }
                            else
                            {
                                item["parent"] = "";
                            }
                        }
                        else
                        {
                            item.Add("parent", "");
                        }
                    }
                    catch { }
                   
                    try
                    {
                        foreach (var fields in customfieldslist)
                        {
                            bool stop = false;
                            foreach (JObject jointable in joinresultja)
                            {
                                string idact = jointable["_id"].ToString();
                                if (idact == item["_id"].ToString())
                                {
                                foreach (JProperty joincustoms in jointable["customfields"])
                                {
                                    string namekey = joincustoms.Name;
                                        namekey = namekey.Replace("_HTKField", "");
                                        if (namekey == fields.Key && idact == item["_id"].ToString())
                                    {
                                        try
                                        {
                                            item.Add(fields.Key, joincustoms.Value);
                                            stop = true;
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            continue;
                                        }
                                    }
                                   
                                }
                                if (stop == false)
                                {
                                    try
                                    {
                                        item.Add(fields.Key, "");
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            }
                            }
                        }
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";
                        string aux1 = item["location"].ToString();
                        try
                        {
                            try
                            {
                                foreach (JObject ls in cataloglocations)
                                {
                                    try
                                    {
                                        if (ls["conjunto"]["id"].ToString() == item["location"].ToString())
                                        {
                                            item["location"] = ls["conjunto"]["name"].ToString();
                                            item["parent"] = ls["conjunto"]["name"].ToString();
                                            item["number"] = ls["conjunto"]["number"].ToString();
                                            break;
                                        }
                                        if (ls["ubicacion"]["id"].ToString() == item["location"].ToString())
                                        {
                                            item["location"] = ls["ubicacion"]["name"].ToString();
                                            item["parent"] = ls["conjunto"]["name"].ToString();
                                            item["number"] = ls["conjunto"]["number"].ToString();
                                            break;
                                        }
                                        if (ls["sububicacion"]["id"].ToString() == item["location"].ToString())
                                        {
                                            item["location"] = ls["sububicacion"]["name"].ToString();
                                            item["parent"] = ls["conjunto"]["name"].ToString();
                                            item["number"] = ls["conjunto"]["number"].ToString();
                                            break;
                                        }

                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            catch { }
                        }
                        catch { }
                        if (childrendict.TryGetValue(aux1, out value))
                        {
                           // item["location"] = value;

                            int val = 0;
                            int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                            int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                            if (month.ToString().Substring(0, 1) == "0")
                            {

                                month = Convert.ToInt16(month.ToString().Substring(1));

                            }
                            int[] arraylinex = aux.ToArray();
                            if (graph.TryGetValue(value, out val))
                            {
                                graph[value] = graph[value] + 1;
                                arraylinex = auxgraph[item["location"].ToString()];
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["location"].ToString()] = arraylinex;
                            }
                            else
                            {

                                graph.Add(value, 1);

                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["location"].ToString()] = arraylinex;
                            }
                        }
                          try
                        {
                            foreach (var colsx in datacols)
                            {
                                if (!item.TryGetValue(colsx.Key, out tokn))
                                {
                                    item.Add(colsx.Key," ");
                                }
                            }
                        }
                        catch { }
                        // }
                        result.Add(item);
                        numusers++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }

                ViewData["profiles"] = nameprofile;
                ViewData["numusers"] = numusers.ToString();
                ViewData["dates"] = datesarray;
                foreach (JObject x in customfieldsx)
                {
                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                /*  string rut = exp(result, nameprofile, numusers.ToString(), datacols, datesarray, "Usuarios", "Perfil(es)");
                  ViewData["url"] = rut;
                  this.Session["headuser"] = datacols;
                  this.Session["userpdf"] = result;
                  this.Session["filtername"] = nameprofile;
                  this.Session["numtotal"] = numusers.ToString();
                  this.Session["getdates"] = datesarray;
                  this.Session["namereport"] = "Usuarios";
                  this.Session["typefilter"] = "Perfil(es)";*/
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                ViewBag.title = "Reporte de Depreciación de Activos";
                if (typereport == 0)
                {
                    return View(result);
                }
                else
                {
                    return View("GenerateObjectsRealplus",result);
                }
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public ActionResult GenerateReport(string data)
        {

            try
            {
                List<string> arrayoptions = new List<string>();
                string options = data.ToString();
                JArray array = JsonConvert.DeserializeObject<JArray>(options);

                JArray resultlist = new JArray();
                Dictionary<string, string> listoption = new Dictionary<string, string>();
                foreach (JObject item in array)
                {


                   
                        MongoModel usersModel = new MongoModel(item["data"].ToString());
                        string result = usersModel.GetRows();

                        JArray arrayresult = JsonConvert.DeserializeObject<JArray>(result);
                    JObject obj = JsonConvert.DeserializeObject<JObject>("{\'" + item["data"].ToString() + "':" + arrayresult + "}");
                    listoption.Add(item["data"].ToString(), item["value"].ToString());
                        resultlist.Add(obj);
                   

                }


                ViewData["options"] = listoption;
                return View(resultlist);
            }
            catch (Exception ex)
            {
                return null;
            }
            }
        public ActionResult GenerateObjectsRealWidget(string profile, string startdate, string enddate, string col, string typegraph = null, string id = null)
        {
            try
            {

                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                int numusers = 0;
                string nameprofile = "";
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                JArray profilesx = JsonConvert.DeserializeObject<JArray>(profile);

                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                String jsonDashboard = "{'userId':'" + useridx + "','profile':" + profilesx + ",'start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateObjectsRealWidget','category': 'Objetos_Reales'}";






                List<string> filterprofile = new List<string>();





                Dictionary<string, string> datacols = new Dictionary<string, string>();

                foreach (JObject x in profilesx)
                {
                    filterprofile.Add(x["data"].ToString());
                }

                foreach (JObject x in cols)
                {

                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                Dictionary<string, string> profiles = new Dictionary<string, string>();
                ViewData["dates"] = dates;
                if (profile == "0")
                {
                    string getprofiles = locationsdb.GetRows();
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());
                        nameprofile = "Todos";
                    }
                }
                else
                {

                    string getprofiles = locationsdb.GetRowsFilter(filterprofile);
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    nameprofile = "";
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());

                        nameprofile += x["name"] + ",";

                    }
                }
                if (nameprofile.Length > 50)
                {
                    nameprofile = nameprofile.Substring(0, 47);
                    nameprofile += "...";
                }

                string getuser = ObjectsRealdb.GetRowsReportObjectsReal(filterprofile, datacols, start, end);
                JArray usersja = JsonConvert.DeserializeObject<JArray>(getuser);
                JArray result = new JArray();
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                foreach (JObject item in usersja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";
                        if (profiles.TryGetValue(item["location"].ToString(), out value))
                        {
                            item["location"] = value;

                            int val = 0;
                            int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                            int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                            if (month.ToString().Substring(0, 1) == "0")
                            {

                                month = Convert.ToInt16(month.ToString().Substring(1));

                            }
                            int[] arraylinex = aux.ToArray();
                            if (graph.TryGetValue(value, out val))
                            {
                                graph[value] = graph[value] + 1;
                                arraylinex = auxgraph[item["location"].ToString()];
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["location"].ToString()] = arraylinex;
                            }
                            else
                            {

                                graph.Add(value, 1);

                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["location"].ToString()] = arraylinex;
                            }
                        }

                        // }
                        result.Add(item);
                        numusers++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }

                ViewData["profiles"] = nameprofile;
                ViewData["numusers"] = numusers.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {
                    headgraphmult.Add(x.Key);
                    auxarray.Add(w, x.Value);
                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {
                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {
                        int[] a = y.Value;
                        listrange.Add(a[w]);
                    }
                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                //save dashboard perfil
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                //
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }
        }
       
        public ActionResult GenerateUserWidget(string profile, string startdate, string enddate, string col, string typegraph = null, string id = null)
        {
            try
            {

                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                int numusers = 0;
                string nameprofile = "";
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
                JArray profilesx = JsonConvert.DeserializeObject<JArray>(profile);
              
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();
               

                String jsonDashboard = "{'userId':'" + useridx + "','profile':" + profilesx + ",'start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateUserWidget','category': 'Usuarios'}";
             





                 List<string> filterprofile = new List<string>();





                Dictionary<string, string> datacols = new Dictionary<string, string>();

                foreach (JObject x in profilesx)
                {
                    filterprofile.Add(x["data"].ToString());
                }

                foreach (JObject x in cols)
                {

                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }
                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                Dictionary<string, string> profiles = new Dictionary<string, string>();
                ViewData["dates"] = dates;
                if (profile == "0")
                {
                    string getprofiles = profilesdb.GetRows();
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());
                        nameprofile = "Todos";
                    }
                }
                else
                {
                   
                    string getprofiles = Profiledb.GetRowsFilter(filterprofile);
                    JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofiles);
                    nameprofile = "";
                    foreach (var x in profilesja)
                    {
                        profiles.Add(x["_id"].ToString(), x["name"].ToString());

                        nameprofile += x["name"] + ",";

                    }
                }
                if (nameprofile.Length > 50)
                {
                    nameprofile = nameprofile.Substring(0, 47);
                    nameprofile += "...";
                }

                string getuser = Userdb.GetRowsReportUser(filterprofile, datacols, start, end);
                JArray usersja = JsonConvert.DeserializeObject<JArray>(getuser);
                JArray result = new JArray();
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                foreach (JObject item in usersja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";
                        if (profiles.TryGetValue(item["profileId"].ToString(), out value))
                        {
                            item["profileId"] = value;

                            int val = 0;
                            int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                            int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                            if (month.ToString().Substring(0, 1) == "0")
                            {

                                month = Convert.ToInt16(month.ToString().Substring(1));

                            }
                            int[] arraylinex = aux.ToArray();
                            if (graph.TryGetValue(value, out val))
                            {
                                graph[value] = graph[value] + 1;
                                arraylinex = auxgraph[item["profileId"].ToString()];
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["profileId"].ToString()] = arraylinex;
                            }
                            else
                            {

                                graph.Add(value, 1);

                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[item["profileId"].ToString()] = arraylinex;
                            }
                        }

                        // }
                        result.Add(item);
                        numusers++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }

                ViewData["profiles"] = nameprofile;
                ViewData["numusers"] = numusers.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {
                    headgraphmult.Add(x.Key);
                    auxarray.Add(w, x.Value);
                     w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {
                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {
                        int[] a = y.Value;
                        listrange.Add(a[w]);
                    }
                   graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                //save dashboard perfil
                string Id = "";
              


                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                //
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }
        }
        public ActionResult GenerateProfileWidget(string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                int numhw = 0;
                int index = 0;
                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);
              
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateProfileWidget','category': 'Perfiles'}";
             

                ViewData["typegraph"] = typegraph;

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }

                 Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {

                    if (x["data"].ToString() == "users")
                    {
                        index++;
                        valuex = x["value"].ToString();

                        datax = x["data"].ToString();
                    }
                    else
                    {

                        datacols.Add(x["data"].ToString(), x["value"].ToString());

                    }
                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = Profiledb.GetRowsReportProfile(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                string getuser = Userdb.GetRows();
                JArray usersja = JsonConvert.DeserializeObject<JArray>(getuser);



                // }


                // }

                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        int numusers = 0;

                        foreach (JObject x in usersja)
                        {

                            if (x["profileId"].ToString() == item["_id"].ToString())
                            {
                                int val = 0;
                                int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                                int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                                if (month.ToString().Substring(0, 1) == "0")
                                {

                                    month = Convert.ToInt16(month.ToString().Substring(1));

                                }
                                int[] arraylinex = aux.ToArray();
                                if (graph.TryGetValue(item["name"].ToString(), out val))
                                {
                                    graph[item["name"].ToString()] = graph[item["name"].ToString()] + 1;
                                    arraylinex = auxgraph[item["name"].ToString()];
                                    arraylinex = getgraph(years, arraylinex, month, year, headm);
                                    auxgraph[item["name"].ToString()] = arraylinex;
                                }
                                else
                                {

                                    graph.Add(item["name"].ToString(), 1);

                                    arraylinex = getgraph(years, arraylinex, month, year, headm);
                                    auxgraph[item["name"].ToString()] = arraylinex;
                                }

                                numusers++;
                            }

                        }


                        // }
                        // item.Add("\Users':'"+numusers+"'");
                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"users\":\'" + numusers + "', \"CreatedDate\":\'" + item["CreatedDate"] + "' }");

                        result.Add(jobjectnew);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numprofiles"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                /*  string rut = exp(result, "", numhw.ToString(), datacols, datesarray, "Perfiles", "");
                  ViewData["url"] = rut;*/

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                //save dashboard perfil
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                //

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
       /* public ActionResult GenerateInventoryWidget(string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                int numhw = 0;

                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;

                JArray cols = JsonConvert.DeserializeObject<JArray>(col);

                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateInventoryWidget','category': 'Inventarios'}";
             

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }

                 Dictionary<string, string> datacols = new Dictionary<string, string>();
                foreach (JObject x in cols)
                {

                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string gethw = Inventorydb.GetRowsReportInventory(datacols, start, end);
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                //timegraph

                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                //
                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";

                        string getlocationname = locationdb.GetRow(item["location"].ToString());
                        JObject locationja = JsonConvert.DeserializeObject<JObject>(getlocationname);
                        string getprofile = profilesdb.GetRow(item["profile"].ToString());
                        JObject profilejaa = JsonConvert.DeserializeObject<JObject>(getprofile);
                        string gethardware = Hwdb.GetRow(item["hardware"].ToString());
                        JObject hardwarejaa = JsonConvert.DeserializeObject<JObject>(gethardware);


                        item["location"] = locationja["name"].ToString();
                        item["profile"] = profilejaa["name"].ToString();
                        item["hardware"] = hardwarejaa["name"].ToString();

                        int val = 0;
                        // times
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();
                        //


                        if (graph.TryGetValue(item["location"].ToString(), out val))
                        {
                            graph[item["location"].ToString()] = graph[item["location"].ToString()] + 1;
                            arraylinex = auxgraph[item["location"].ToString()];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["location"].ToString()] = arraylinex;
                        }
                        else
                        {

                            graph.Add(item["location"].ToString(), 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["location"].ToString()] = arraylinex;
                        }


                        // }
                        result.Add(item);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }



                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                //save dashboard perfil
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                //
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }*/

        public ActionResult GenerateHwWidget(string type, string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                int numhw = 0;
                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);

                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateHwWidget','category': 'Hardwares'}";
             

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                if (type == "0") { ViewData["smart"] = "Todos"; }
                else if (type == "1") { ViewData["smart"] = "Inteligentes"; }
                else if (type == "2") { ViewData["smart"] = "No Inteligentes"; }


                  Dictionary<string, string> datacols = new Dictionary<string, string>();
                foreach (JObject x in cols)
                {

                    datacols.Add(x["data"].ToString(), x["value"].ToString());
                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);
                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();

                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;
                //end graphlinetime
                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                string gethw = Hwdb.GetRowsReportHw(type, datacols, start, end);
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";

                        if (item["smart"].ToString().ToLower() == "false")
                        {

                            item["smart"] = "No Inteligente";

                        }
                        else
                        {
                            item["smart"] = "Inteligente";


                        }

                        int val = 0;
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();
                        if (graph.TryGetValue(item["smart"].ToString(), out val))
                        {
                            graph[item["smart"].ToString()] = graph[item["smart"].ToString()] + 1;
                            arraylinex = auxgraph[item["smart"].ToString()];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["smart"].ToString()] = arraylinex;
                        }
                        else
                        {

                            graph.Add(item["smart"].ToString(), 1);

                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["smart"].ToString()] = arraylinex;
                        }


                        // }
                        result.Add(item);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }



                ViewData["numusers"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                //save dashboard perfil
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                //
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
        public ActionResult GenerateProcessesWidget(string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                int numhw = 0;
                int index = 0;
                int[] arrayline;
                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);

                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateProcessesWidget','category': 'Procesos'}";
             

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }
                 Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {



                    datacols.Add(x["data"].ToString(), x["value"].ToString());


                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);


                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime

                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = Proccessdb.GetRowsReportProcesses(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                //timegraph
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                // }


                // }


                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {

                    try
                    {
                        int numi = 0;
                        int numusers = 0;
                        string min = "";
                        string max = "";
                        try
                        {
                            min = item["min_duration"]["duration"].ToString();
                            min = min + " " + item["min_duration"]["type"].ToString();

                        }
                        catch (Exception ex)
                        {
                            min = "Ilimitado";
                        }
                        try
                        {
                            max = item["max_duration"]["duration"].ToString();
                            max = max + " " + item["max_duration"]["type"].ToString();

                        }
                        catch (Exception ex)
                        {
                            max = "Ilimitado";
                        }


                        int val = 0;

                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();

                        if (graph.TryGetValue(item["status"].ToString(), out val))
                        {

                            graph[item["status"].ToString()] = graph[item["status"].ToString()] + 1;
                            arraylinex = auxgraph[item["status"].ToString()];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["status"].ToString()] = arraylinex;


                        }
                        else
                        {
                            graph.Add(item["status"].ToString(), 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["status"].ToString()] = arraylinex;


                        }


                        numusers++;


                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"status\":\'" + item["status"] + "', \"min_duration\":\'" + min + "', \"max_duration\":\'" + max + "', \"CreatedDate\":\'" + item["CreatedDate"] + "' }");

                        result.Add(jobjectnew);
                        numhw++;

                    }
                    catch (Exception ex)
                    {


                    }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                //save dashboard perfil
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
        public ActionResult GenerateMovementWidget(string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                int numhw = 0;
                int index = 0;
                int[] arrayline;
                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);

                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateMovementWidget','category': 'Movimientos'}";
             

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }

                 Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {



                    datacols.Add(x["data"].ToString(), x["value"].ToString());


                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);


                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime

                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = Movementdb.GetRowsReportMov(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                //timegraph
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                // }


                // }


                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {

                    try
                    {
                        int numi = 0;
                        int numusers = 0;

                        string processesrow = Proccessdb.GetRow(item["processes"].ToString());
                        JObject proja = JsonConvert.DeserializeObject<JObject>(processesrow);

                        string categoryName = proja["name"].ToString();
                        item["processes"] = proja["name"].ToString();
                        int val = 0;

                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();

                        if (graph.TryGetValue(categoryName, out val))
                        {

                            graph[categoryName] = graph[categoryName] + 1;
                            arraylinex = auxgraph[categoryName];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[categoryName] = arraylinex;


                        }
                        else
                        {
                            graph.Add(categoryName, 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[categoryName] = arraylinex;


                        }


                        numusers++;


                        //  JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"processes\":\'" + categoryName + "',\"CreatedDate\":\'" + item["CreatedDate"] + "' }");

                        result.Add(item);
                        numhw++;

                    }
                    catch (Exception ex)
                    {


                    }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                //save dashboard perfil
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                //
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }

        public ActionResult GenerateObjectsRefWidget(string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                int numhw = 0;
                int index = 0;
                int[] arrayline;

                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);

                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateObjectsRefWidget','category': 'Objetos_Ref'}";
             

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }

                Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {



                    datacols.Add(x["data"].ToString(), x["value"].ToString());


                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);


                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime

                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = Objrefdb.GetRowsReportRf(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                //timegraph
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                // }


                // }


                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {

                    try
                    {
                        int numi = 0;
                        int numusers = 0;

                        string categoryrow = Categoriesdb.GetRow(item["parentCategory"].ToString());
                        JObject catja = JsonConvert.DeserializeObject<JObject>(categoryrow);

                        string categoryName = catja["name"].ToString();
                        int val = 0;

                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();

                        if (graph.TryGetValue(categoryName, out val))
                        {

                            graph[categoryName] = graph[categoryName] + 1;
                            arraylinex = auxgraph[categoryName];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[categoryName] = arraylinex;


                        }
                        else
                        {
                            graph.Add(categoryName, 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[categoryName] = arraylinex;


                        }


                        numusers++;


                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"parentCategory\":\'" + categoryName + "',\"CreatedDate\":\'" + item["CreatedDate"] + "' }");

                        result.Add(jobjectnew);
                        numhw++;

                    }
                    catch (Exception ex)
                    {


                    }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                //save dashboard perfil
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                return View(result);

            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
        public ActionResult GenerateNotWidget(string profile, string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                  String jsonDashboard = "{'userId':'" + useridx + "','profile':'" + profile + "','start_date':'','end_date':'','fields':'','graph':'" + typegraph + "','urlaction': 'GenerateNotWidget','category': '" + typegraph + "'}";
             

                string Id = "";




                if (id == null)
                {
                   // Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

               
                ViewData["idwidget"] = Id;

                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult GenerateInvWidget(string profile, string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();

                 String jsonDashboard = "{'userId':'" + useridx + "','profile':'" + profile + "','start_date':'','end_date':'','fields':'','graph':'" + typegraph + "','urlaction': 'GenerateInvWidget','category': '" + typegraph + "'}";
             



                string Id = "";




                if (id == null)
                {
                   // Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }


                ViewData["idwidget"] = Id;

                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

         public ActionResult GenerateMapWidget(string profile, string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


             try
             {
                 if (Request.Cookies["_id2"] != null)
                 {
                     Session["_id"] = Request.Cookies["_id2"].Value;
                 }
                 string useridx = Session["_id"].ToString();

                 String jsonDashboard = "{'userId':'" + useridx + "','profile':'" + profile + "','start_date':'','end_date':'','fields':'','graph':'" + typegraph + "','urlaction': 'GenerateMapWidget','category': '" + typegraph + "'}";
             

                   string Id = "";

              


                 if (id == null)
                 {
                   //  Id = Dashboard.SaveRow(jsonDashboard, Id);
                 }
                 else
                 {
                     Id = id;
                 }

                 string[] dataw = { profile, typegraph };
                 ViewData["idwidget"] = Id;

                 return View(dataw);
             }
             catch (Exception ex)
             {
                 return null;
             }
         }
        public ActionResult GenerateLocationsWidget(string startdate, string enddate, string col, string typegraph = null, string id = null)
        {


            try
            {
                int numhw = 0;
                int index = 0;
                int[] arrayline;

                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                JArray cols = JsonConvert.DeserializeObject<JArray>(col);

                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','start_date':'" + startdate + "','end_date':'" + enddate
               + "','fields':" + cols + ",'graph':'" + typegraph + "','urlaction': 'GenerateLocationsWidget','category': 'Ubicaciones'}";
             

                if (startdate == "Indefinida" || startdate == null)
                {
                    startdate = "01/01/1900";

                }
                if (enddate == "Indefinida" || enddate == null)
                {
                    enddate = "01/01/3000";
                }

                Dictionary<string, string> datacols = new Dictionary<string, string>();
                string valuex = "";
                string datax = "";
                foreach (JObject x in cols)
                {



                    datacols.Add(x["data"].ToString(), x["value"].ToString());


                }

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);


                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime

                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;

                string getprofile = locationsdb.GetRowsReportLoc(datacols, start, end);
                JArray profilesja = JsonConvert.DeserializeObject<JArray>(getprofile);
                JArray result = new JArray();

                //timegraph
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                // }


                // }


                foreach (JObject item in profilesja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {

                    try
                    {

                        int numi = 0;
                        int numusers = 0;

                        string profilerow = locationsProfilesdb.GetRow(item["profileId"].ToString());
                        JObject proja = JsonConvert.DeserializeObject<JObject>(profilerow);

                        string categoryName = proja["name"].ToString();
                        int val = 0;

                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();

                        if (graph.TryGetValue(categoryName, out val))
                        {

                            graph[categoryName] = graph[categoryName] + 1;
                            arraylinex = auxgraph[categoryName];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[categoryName] = arraylinex;


                        }
                        else
                        {
                            graph.Add(categoryName, 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[categoryName] = arraylinex;


                        }


                        numusers++;


                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"name\":'" + item["name"].ToString() + "', \"profileId\":\'" + categoryName + "', \"tipo\":\'" + item["tipo"] + "',\"CreatedDate\":\'" + item["CreatedDate"] + "' }");

                        result.Add(jobjectnew);
                        numhw++;

                    }
                    catch (Exception ex)
                    {


                    }
                }

                if (index != 0)
                {
                    datacols.Add(datax, valuex);
                }

                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                   //save dashboard perfil
                string Id = "";
              


                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                //

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
        public ActionResult GenerateMovementMasterWidget(string startdate, string enddate, string movements = null, string objects = null, string status = null, string typegraph = null, string id = null)
        {
            if (startdate == "Indefinida" || startdate == null)
            {
                startdate = "01/01/1900";

            }
            if (enddate == "Indefinida" || enddate == null)
            {
                enddate = "01/01/3000";
            }
            //    string mapreduce = demanddb.Mapreduced();
            try
            {
                int numhw = 0;
                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();




                List<string> listmovements = new List<string>();
                List<string> listobjects = new List<string>();
                 List<int> liststatus = new List<int>();
                JArray movementja = JsonConvert.DeserializeObject<JArray>(movements);
                JArray objectsja = JsonConvert.DeserializeObject<JArray>(objects);
                 JArray statusja = JsonConvert.DeserializeObject<JArray>(status);
                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','movements':" + movementja + ",'objects':" + objectsja + ",'start_date':'" + startdate + "','end_date':'" + enddate
              + "','fields':[],'graph':'" + typegraph + "','urlaction': 'GenerateCustomWidget','category': 'Movements'}";


                foreach (JObject x in movementja)
                {
                    listmovements.Add(x["id"].ToString());
                }
                foreach (JObject x in objectsja)
                {
                    listobjects.Add(x["id"].ToString());
                }
              
                foreach (JObject x in statusja)
                {
                    if (x["id"].ToString() == "3")
                    {
                        for (int i = 1; i < 6; i++)
                        {

                            liststatus.Add(i);
                        }

                    }
                    else
                    {
                        liststatus.Add(Convert.ToInt16(x["id"].ToString()));
                    }
                }


                // JArray cols = JsonConvert.DeserializeObject<JArray>(movements);
                Dictionary<string, string> datacols = new Dictionary<string, string>();

                datacols.Add("folio", "id Solicitud");
                datacols.Add("object", "Activo");
                datacols.Add("location", "Ubicacion");


                datacols.Add("movement", "Descripcion");
                datacols.Add("status", "Tipo de Solicitud");
                datacols.Add("Creator", "Solicitada Por");
                datacols.Add("CreatedDate", "Fecha de Solicitud");



                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                //string mapreduce = demanddb.Mapreduced(datacols, listmovements, listobjects, listlocations, listusers, start, end);

                string gethw = demanddb.GetRowsReportMasterDemand(datacols, listmovements, listobjects, liststatus, start, end);
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                //timegraph

                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                string allslocations = locationdb.GetRows();
                string allsobjects = Objrefdb.GetRows();
                JArray allobjsjaa = JsonConvert.DeserializeObject<JArray>(allsobjects);
                JArray alllocjaa = JsonConvert.DeserializeObject<JArray>(allslocations);
                var dictionaryobjs = allobjsjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                var dictionarylocs = alllocjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                Dictionary<int, string> statusdict = new Dictionary<int, string>();
                statusdict.Add(1, "Dictamen Pendiente");
                statusdict.Add(2, "Con Dictamen y En Espera de Datos Adicionales");
                statusdict.Add(3, "Con Dictamen, Datos Adicionales  y En Espera de Autorizaciones");
                statusdict.Add(4, "Con Dictamen, Datos Adicionales, Autorizaciones y En Espera de Comprobantes");
                statusdict.Add(5, "Con Dictamen, Datos Adicionales, Autorizaciones,Comprobantes y En Espera del VOBO");
                statusdict.Add(6, "Solicitud Autorizada y Aplicada");
                statusdict.Add(7, "Denegada");

                //
                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        List<string> locationslist = new List<string>();
                        List<string> objlist = new List<string>();
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";

                        StringBuilder objectsall = new StringBuilder();
                        StringBuilder locsall = new StringBuilder();
                        foreach (var arrayobject in item["objects"])
                        {
                            string locsname = dictionarylocs[arrayobject["location"].ToString()];
                            string objsname = dictionaryobjs[arrayobject["objectReference"].ToString()];
                            if (!locationslist.Contains(locsname))
                            {
                                locationslist.Add(locsname);
                                locsall.Append(locsname + ",");//+Environment.NewLine
                            }
                            if (!objlist.Contains(objsname))
                            {
                                objectsall.Append(objsname + ",");
                                objlist.Add(objsname);

                            }
                        }



                        try
                        {
                            if (item["deleteType"].ToString() == "planeada" || item["deleteType"].ToString() == "no planeada")
                            {
                                item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                            }
                        }
                        catch (Exception ex)
                        {
                            switch (item["status"].ToString())
                            {
                                case "6":
                                    item["status"] = statusdict[6];
                                    break;
                                case "7":
                                    item["status"] = statusdict[7];
                                    break;
                                default:
                                    item["status"] = "Pendiente";
                                    break;
                            }
                        }

                        item["location"] = locsall.ToString();
                        item["movement"] = item["nameMovement"].ToString();
                        item["Creator"] = item["nameUser"].ToString();
                        //  item["object"] = objsjaa["name"].ToString();
                        item["object"] = objectsall.ToString();


                        int val = 0;
                        // times
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();
                        //


                        if (graph.TryGetValue(item["location"].ToString(), out val))
                        {
                            graph[item["location"].ToString()] = graph[item["location"].ToString()] + 1;
                            arraylinex = auxgraph[item["location"].ToString()];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["location"].ToString()] = arraylinex;
                        }
                        else
                        {

                            graph.Add(item["location"].ToString(), 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["location"].ToString()] = arraylinex;
                        }


                        // }
                        result.Add(item);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }



                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
        public ActionResult GenerateCustomWidget(string startdate, string enddate, string movements = null, string objects = null, string locations = null, string users = null, string status = null, string typegraph = null, string id = null)
        {
            if (startdate == "Indefinida" || startdate == null)
            {
                startdate = "01/01/1900";

            }
            if (enddate == "Indefinida" || enddate == null)
            {
                enddate = "01/01/3000";
            }
            //    string mapreduce = demanddb.Mapreduced();
            try
            {
                int numhw = 0;
                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();




                List<string> listmovements = new List<string>();
                List<string> listobjects = new List<string>();
                List<string> listlocations = new List<string>();
                List<string> listusers = new List<string>();
                List<int> liststatus = new List<int>();
                JArray movementja = JsonConvert.DeserializeObject<JArray>(movements);
                JArray objectsja = JsonConvert.DeserializeObject<JArray>(objects);
                JArray locationsja = JsonConvert.DeserializeObject<JArray>(locations);
                JArray usersja = JsonConvert.DeserializeObject<JArray>(users);
                JArray statusja = JsonConvert.DeserializeObject<JArray>(status);
                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','movements':" + movementja + ",'objects':" + objectsja + ",'locations':" + locationsja + ",'users':" + usersja + ",'start_date':'" + startdate + "','end_date':'" + enddate
              + "','fields':[],'graph':'" + typegraph + "','urlaction': 'GenerateCustomWidget','category': 'Movements'}";


                foreach (JObject x in movementja)
                {
                    listmovements.Add(x["id"].ToString());
                }
                foreach (JObject x in objectsja)
                {
                    listobjects.Add(x["id"].ToString());
                }
                foreach (JObject x in locationsja)
                {
                    listlocations.Add(x["id"].ToString());
                }
                foreach (JObject x in usersja)
                {
                    listusers.Add(x["id"].ToString());
                }
                foreach (JObject x in statusja)
                {
                    if (x["id"].ToString() == "3")
                    {
                        for (int i = 1; i < 6; i++)
                        {

                            liststatus.Add(i);
                        }

                    }
                    else
                    {
                        liststatus.Add(Convert.ToInt16(x["id"].ToString()));
                    }
                }


                // JArray cols = JsonConvert.DeserializeObject<JArray>(movements);
                Dictionary<string, string> datacols = new Dictionary<string, string>();

                datacols.Add("folio", "id Solicitud");
                datacols.Add("object", "Activo");
                datacols.Add("location", "Ubicacion");


                datacols.Add("movement", "Descripcion");
                datacols.Add("status", "Tipo de Solicitud");
                datacols.Add("Creator", "Solicitada Por");
                datacols.Add("CreatedDate", "Fecha de Solicitud");



                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                //string mapreduce = demanddb.Mapreduced(datacols, listmovements, listobjects, listlocations, listusers, start, end);

                string gethw = demanddb.GetRowsReportDemand(datacols, listmovements, listobjects, listlocations, listusers, liststatus, start, end);
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                //timegraph

                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                string allslocations = locationdb.GetRows();
                string allsobjects = Objrefdb.GetRows();
                JArray allobjsjaa = JsonConvert.DeserializeObject<JArray>(allsobjects);
                JArray alllocjaa = JsonConvert.DeserializeObject<JArray>(allslocations);
                var dictionaryobjs = allobjsjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                var dictionarylocs = alllocjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                Dictionary<int, string> statusdict = new Dictionary<int, string>();
                statusdict.Add(1, "Dictamen Pendiente");
                statusdict.Add(2, "Con Dictamen y En Espera de Datos Adicionales");
                statusdict.Add(3, "Con Dictamen, Datos Adicionales  y En Espera de Autorizaciones");
                statusdict.Add(4, "Con Dictamen, Datos Adicionales, Autorizaciones y En Espera de Comprobantes");
                statusdict.Add(5, "Con Dictamen, Datos Adicionales, Autorizaciones,Comprobantes y En Espera del VOBO");
                statusdict.Add(6, "Solicitud Autorizada y Aplicada");
                statusdict.Add(7, "Denegada");

                //
                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        List<string> locationslist = new List<string>();
                        List<string> objlist = new List<string>();
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";

                        StringBuilder objectsall = new StringBuilder();
                        StringBuilder locsall = new StringBuilder();
                        foreach (var arrayobject in item["objects"])
                        {
                            string locsname = dictionarylocs[arrayobject["location"].ToString()];
                            string objsname = dictionaryobjs[arrayobject["objectReference"].ToString()];
                            if (!locationslist.Contains(locsname))
                            {
                                locationslist.Add(locsname);
                                locsall.Append(locsname + ",");//+Environment.NewLine
                            }
                            if (!objlist.Contains(objsname))
                            {
                                objectsall.Append(objsname + ",");
                                objlist.Add(objsname);

                            }
                        }



                        try
                        {
                            if (item["deleteType"].ToString() == "planeada" || item["deleteType"].ToString() == "no planeada")
                            {
                                item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                            }
                        }
                        catch (Exception ex)
                        {
                            switch (item["status"].ToString())
                            {
                                case "6":
                                    item["status"] = statusdict[6];
                                    break;
                                case "7":
                                    item["status"] = statusdict[7];
                                    break;
                                default:
                                    item["status"] = "Pendiente";
                                    break;
                            }
                        }

                        item["location"] = locsall.ToString();
                        item["movement"] = item["nameMovement"].ToString();
                        item["Creator"] = item["nameUser"].ToString();
                        //  item["object"] = objsjaa["name"].ToString();
                        item["object"] = objectsall.ToString();


                        int val = 0;
                        // times
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();
                        //


                        if (graph.TryGetValue(item["location"].ToString(), out val))
                        {
                            graph[item["location"].ToString()] = graph[item["location"].ToString()] + 1;
                            arraylinex = auxgraph[item["location"].ToString()];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["location"].ToString()] = arraylinex;
                        }
                        else
                        {

                            graph.Add(item["location"].ToString(), 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["location"].ToString()] = arraylinex;
                        }


                        // }
                        result.Add(item);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }



                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
    
        public ActionResult GenerateMovementHistoryWidget(string startdate, string enddate, string movements = null, string users = null, string typegraph = null, string id = null)
        {
            if (startdate == "Indefinida" || startdate == null)
            {
                startdate = "01/01/1900";

            }
            if (enddate == "Indefinida" || enddate == null)
            {
                enddate = "01/01/3000";
            }
            //    string mapreduce = demanddb.Mapreduced();
            try
            {
                int numhw = 0;
                if (typegraph == null)
                {
                    typegraph = "piegoo";
                }
                ViewData["typegraph"] = typegraph;
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string useridx = Session["_id"].ToString();


               

                List<string> listmovements = new List<string>();
                 List<string> listusers = new List<string>();
                   JArray movementja = JsonConvert.DeserializeObject<JArray>(movements);
                   JArray usersja = JsonConvert.DeserializeObject<JArray>(users);
                String jsonDashboard = "{'userId':'" + useridx + "','profile':'','movements':" + movementja + ",'users':" + usersja + ",'start_date':'" + startdate + "','end_date':'" + enddate
              + "','fields':[],'graph':'" + typegraph + "','urlaction': 'GenerateCustomWidget','category': 'MovementHistory'}";
             

                foreach (JObject x in movementja)
                {
                    listmovements.Add(x["id"].ToString());
                }
               
                foreach (JObject x in usersja)
                {
                    listusers.Add(x["id"].ToString());
                }
             


                // JArray cols = JsonConvert.DeserializeObject<JArray>(movements);
                Dictionary<string, string> datacols = new Dictionary<string, string>();

                datacols.Add("folio", "id Solicitud");
                datacols.Add("object", "Activo");
                datacols.Add("location", "Conjunto");


                datacols.Add("movement", "Descripcion");
                datacols.Add("status", "Tipo de Solicitud");
                datacols.Add("Creator", "Solicitada Por");
                datacols.Add("CreatedDate", "Fecha de Solicitud");



                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                //string mapreduce = demanddb.Mapreduced(datacols, listmovements, listobjects, listlocations, listusers, start, end);

                string gethw = demanddb.GetRowsReportHistoryDemand(datacols, listmovements, listusers, start, end);
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                //timegraph

                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();

                string allslocations = locationdb.GetRows();
                string allsobjects = Objrefdb.GetRows();
                JArray allobjsjaa = JsonConvert.DeserializeObject<JArray>(allsobjects);
                JArray alllocjaa = JsonConvert.DeserializeObject<JArray>(allslocations);
                var dictionaryobjs = allobjsjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                var dictionarylocs = alllocjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                Dictionary<int, string> statusdict = new Dictionary<int, string>();
                statusdict.Add(1, "Dictamen Pendiente");
                statusdict.Add(2, "Con Dictamen y En Espera de Datos Adicionales");
                statusdict.Add(3, "Con Dictamen, Datos Adicionales  y En Espera de Autorizaciones");
                statusdict.Add(4, "Con Dictamen, Datos Adicionales, Autorizaciones y En Espera de Comprobantes");
                statusdict.Add(5, "Con Dictamen, Datos Adicionales, Autorizaciones,Comprobantes y En Espera del VOBO");
                statusdict.Add(6, "Solicitud Autorizada y Aplicada");
                statusdict.Add(7, "Denegada");

                //
                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        List<string> locationslist = new List<string>();
                        List<string> objlist = new List<string>();
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";

                            StringBuilder objectsall = new StringBuilder();
                        StringBuilder locsall = new StringBuilder();
                        foreach (var arrayobject in item["objects"])
                        {
                                    string locsname = dictionarylocs[arrayobject["location"].ToString()];
                            string objsname = dictionaryobjs[arrayobject["objectReference"].ToString()];
                                if (!locationslist.Contains(locsname))
                            {
                                locationslist.Add(locsname);
                                locsall.Append(locsname + ",");//+Environment.NewLine
                            }
                            if (!objlist.Contains(objsname))
                            {
                                objectsall.Append(objsname + ",");
                                objlist.Add(objsname);

                            }
                        }



                        try
                        {
                            if (item["deleteType"].ToString() == "planeada" || item["deleteType"].ToString() == "no planeada")
                            {
                                item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                            }
                        }
                        catch (Exception ex)
                        {
                            switch (item["status"].ToString())
                            {
                                case "6":
                                    item["status"] = statusdict[6];
                                    break;
                                case "7":
                                    item["status"] = statusdict[7];
                                    break;
                                default:
                                    item["status"] = "Pendiente";
                                    break;
                            }
                        }

                        item["location"] = locsall.ToString();
                        item["movement"] = item["nameMovement"].ToString();
                        item["Creator"] = item["nameUser"].ToString();
                        //  item["object"] = objsjaa["name"].ToString();
                        item["object"] = objectsall.ToString();


                        int val = 0;
                        // times
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();
                        //


                        if (graph.TryGetValue(item["location"].ToString(), out val))
                        {
                            graph[item["location"].ToString()] = graph[item["location"].ToString()] + 1;
                            arraylinex = auxgraph[item["location"].ToString()];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["location"].ToString()] = arraylinex;
                        }
                        else
                        {

                            graph.Add(item["location"].ToString(), 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[item["location"].ToString()] = arraylinex;
                        }


                        // }
                        result.Add(item);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }



                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
                string Id = "";



                if (id == null)
                {
                    Id = Dashboard.SaveRow(jsonDashboard, Id);
                }
                else
                {
                    Id = id;
                }

                ViewData["idwidget"] = Id;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
    
        public void deleteWidget(string id)
        {

            Dashboard.DeleteRowPhysical(id);
       
        }
        public string checkFilters(String data,string type="0")
        {
            try
            {
                JArray dataja = JsonConvert.DeserializeObject<JArray>(data);
                string movprofiles = Movementdb.GetRows();
                JArray movprofilesja = JsonConvert.DeserializeObject<JArray>(movprofiles);
                
                string typemov="limit";
             /*   foreach (JObject item in dataja)
                {   
                    string type = (from mov in movprofilesja where (string)mov["_id"] == item["id"].ToString() select (string)mov["typeMovement"]).First();
                  
                    if(type=="delete"){
                        typemov="alls";
                        break;
                      }
                     
                }*/

                var type1 = (from mov in movprofilesja where dataja.Children()["id"].Contains(mov["_id"]) select mov["typeMovement"].Value<string>()).ToList();
                if (type1.Contains("delete"))
                {
                    if (type == "1")
                    {
                        return "limit";
                    }
                    typemov = "alls";
                }
                return typemov;
             
            }
            catch (Exception ex) {
                
                return "alls";
            
            }


        }
        public ActionResult GenerateMovementTemporalReport(string startdate, string enddate, string movements = null, string objects = null, string status = null, string dict = null, string done = null, string mp = null, string destroy = null, string auto = null, string vobo = null, string photo = null, string extrafilter = null)
        {

            if (startdate == "Indefinida" || startdate == null)
            {
                startdate = "01/01/1900";

            }
            if (enddate == "Indefinida" || enddate == null)
            {
                enddate = "01/01/3000";
            }
            //    string mapreduce = demanddb.Mapreduced();
            try
            {
                int numhw = 0;

                List<string> listmovements = new List<string>();
                List<string> listobjects = new List<string>();
                List<int> liststatus = new List<int>();
                JArray movementja = JsonConvert.DeserializeObject<dynamic>(movements);
                JArray objectsja = JsonConvert.DeserializeObject<JArray>(objects);
                JArray statusja = JsonConvert.DeserializeObject<JArray>(status);
                //   var url = movementja.Select(m => new { id = (string)m["id"] });
                listmovements = (from mov in movementja select (string)mov["id"]).ToList();
                listobjects = (from obj in objectsja select (string)obj["id"]).ToList();
                // listmovements = url.Where(p => p.Name == "id").Select(p => p.Value).ToList<string>();*/
                // listmovements = url;
                /*   listobjects = objectsja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   listlocations = locationsja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   listusers = usersja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   liststatus = statusja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                */

                foreach (JObject x in statusja)
                {
                    if (x["id"].ToString() == "3")
                    {
                        for (int i = 1; i < 6; i++)
                        {

                            liststatus.Add(i);
                        }

                    }
                    else
                    {
                        liststatus.Add(Convert.ToInt16(x["id"].ToString()));
                    }
                }


                // JArray cols = JsonConvert.DeserializeObject<JArray>(movements);
                Dictionary<string, string> datacols = new Dictionary<string, string>();

                datacols.Add("folio", "id Solicitud");
                datacols.Add("object", "Activo");
                datacols.Add("location", "Ubicacion");


                datacols.Add("movement", "Descripcion");
                datacols.Add("status", "Tipo de Solicitud");
                datacols.Add("Creator", "Solicitada Por");
                datacols.Add("CreatedDate", "Fecha de Solicitud");
                if (extrafilter == "alls")
                {
                    datacols.Add("dctFolio", "Dictamen");
                    datacols.Add("receiptFile", "Acta de Hecho");
                    datacols.Add("actFolio", "Acta de Destrucción");
                    datacols.Add("vobo", "Visto Bueno");
                    datacols.Add("autouser", "Autorizador(es)");
                    datacols.Add("images", "Imagen(es)");

                }
                else
                {
                    datacols.Add("vobo", "Visto Bueno");
                    datacols.Add("autouser", "Autorizador(es)");

                }


                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                //string mapreduce = demanddb.Mapreduced(datacols, listmovements, listobjects, listlocations, listusers, start, end);

                Task<string> task1 = Task<string>.Factory.StartNew(() => locationdb.GetRows());
                Task<string> task2 = Task<string>.Factory.StartNew(() => Objrefdb.GetRows());
                Task<string> task3 = Task<string>.Factory.StartNew(() => semaphoredb.GetRows());
                JArray result = new JArray();
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();
                Task.WaitAll(task1, task2, task3);
                string gethw = demanddb.GetRowsReportMasterDemand(datacols, listmovements, listobjects, liststatus, start, end, dict, done, mp, destroy, auto, vobo, photo);
                string allslocations = task1.Result;
                string allsobjects = task2.Result;
                string allssemaphores = task3.Result;

                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray allobjsjaa = JsonConvert.DeserializeObject<JArray>(allsobjects);
                JArray alllocjaa = JsonConvert.DeserializeObject<JArray>(allslocations);
                JArray alllsemaphorejaa = JsonConvert.DeserializeObject<JArray>(allssemaphores);
                var dictionaryobjs = allobjsjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                var dictionarylocs = alllocjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                Dictionary<int, string> statusdict = new Dictionary<int, string>();
                statusdict.Add(1, "Dictamen Pendiente");
                statusdict.Add(2, "Con Dictamen y En Espera de Datos Adicionales");
                statusdict.Add(3, "Con Dictamen, Datos Adicionales  y En Espera de Autorizaciones");
                statusdict.Add(4, "Con Dictamen, Datos Adicionales, Autorizaciones y En Espera de Comprobantes");
                statusdict.Add(5, "Con Dictamen, Datos Adicionales, Autorizaciones,Comprobantes y En Espera del VOBO");
                statusdict.Add(6, "Solicitud Autorizada y Aplicada");
                statusdict.Add(7, "Denegada");
                Dictionary<string, string> semaphorosvalues = new Dictionary<string, string>();
                Task<string>[] tasks = null;

                try
                {
                    var noapprovedList = (from mov in hwsja where mov["authorizations"].Children()["approved"].Contains("0") && (string)mov["status"] == "3" select new { id = (string)mov["_id"], iduser = (from xs in mov["authorizations"].Children() where (string)xs["approved"] == "0" select xs["id_user"].Value<string>()).ToList(), date = (string)mov["CreatedDate"], type = (string)mov["typemov"], namemov = (string)mov["nameMovement"], folio = (string)mov["folio"] }).ToList();
                    var semaphores1 = (from item in alllsemaphorejaa select new { color = (string)item["color"], days = (int)item["days"], type = (string)item["typeMovement"] }).ToList();
                    tasks = new Task<string>[noapprovedList.Count()];
                    foreach (var item in noapprovedList)
                    {
                        
                        DateTime datenow = DateTime.Now;
                        DateTime dateCreated = Convert.ToDateTime(item.date.Substring(0, 10));
                        TimeSpan time = datenow - dateCreated;
                        int diferences = time.Days;
                        var values = 0;
                        bool mailsend = false;
                        foreach (var sem in semaphores1)
                        {
                            if (diferences >= sem.days && item.type == sem.type)
                            {
                                if (sem.days > values)
                                {
                                    semaphorosvalues.Add(item.id, sem.color);
                                    mailsend = true;
                                }
                            }
                        }
                        if (mailsend)
                        {
                            try
                            {
                                JArray recipients = new JArray();
                                JArray attachments = new JArray();
                                string namemov = item.namemov;
                                foreach (string userid in item.iduser)
                                {
                                    recipients.Add(userid);
                                }
                                string to = JsonConvert.SerializeObject(recipients);
                                string attach = JsonConvert.SerializeObject(attachments);
                                tasks[noapprovedList.IndexOf(item)] = Task.Factory.StartNew(() => messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema"));
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                catch (Exception ex) { }
                ViewData["test"] = semaphorosvalues;
                ViewData["semaphores"] = semaphorosvalues;
                Parallel.ForEach(hwsja, itemtoken =>
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {

                        JObject item = itemtoken.Value<JObject>();

                        List<string> locationslist = new List<string>();
                        List<string> objlist = new List<string>();
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";
                        //   var semaphores = (from mov in item["authorizations"] where (string)mov["approved"] == "1" select new { id = (string)mov["_id"], iduser = (string)mov["id_user"], date = (string)mov["CreatedDate"] }).ToList();
                        /* string getobjs = Objrefdb.GetRow(item["objects"]["objectReference"].ToString());
                         JObject objsjaa = JsonConvert.DeserializeObject<JObject>(getobjs);*/
                        StringBuilder objectsall = new StringBuilder();
                        StringBuilder locsall = new StringBuilder();
                        foreach (var arrayobject in item["objects"])
                        {
                            string locsname = dictionarylocs[arrayobject["location"].ToString()];
                            string objsname = dictionaryobjs[arrayobject["objectReference"].ToString()];
                            if (!locationslist.Contains(locsname))
                            {
                                locationslist.Add(locsname);

                            }
                            if (!objlist.Contains(objsname))
                            {
                                objlist.Add(objsname);


                            }
                        }
                        objectsall.Append(String.Join(",", objlist));
                        locsall.Append(String.Join(",", locationslist));


                        try
                        {
                            if (item["deleteType"].ToString() == "planeada")
                            {
                                item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                            }
                        }
                        catch (Exception ex)
                        {
                            switch (item["status"].ToString())
                            {
                                case "6":
                                    item["status"] = statusdict[6];
                                    break;
                                case "7":
                                    item["status"] = statusdict[7];
                                    break;
                                default:
                                    item["status"] = "Pendiente";
                                    break;
                            }
                        }

                        item["location"] = locsall.ToString();
                        item["movement"] = item["nameMovement"].ToString();
                        item["Creator"] = item["nameUser"].ToString();
                        item["object"] = objectsall.ToString();
                        item.Add("images", "sin Imagen");
                        item.Add("autouser", "sin Autorizador(es)");
                        item.Add("vobo", "sin Visto Bueno");
                        try
                        {
                            List<string> imageslist = item["objects"].Children()["images"].Values<string>().ToList();
                            if (imageslist.Count() > 0)
                            {
                                if (photo == "2")
                                {

                                    return;
                                }
                                item["images"] = String.Join(",", imageslist);
                            }
                            else if (photo == "1")
                            {
                                return;
                            }

                        }
                        catch (Exception ex) { }
                        try
                        {
                            List<string> userautolist = item["authorizations"].Children()["user"].Values<string>().ToList();
                            if (userautolist.Count() > 0)
                                item["autouser"] = String.Join(",", userautolist);

                        }
                        catch (Exception ex)
                        {

                        }
                        try
                        {
                            List<string> vobolist = item["approval"].Children()["user"].Values<string>().ToList();
                            if (vobolist.Count() > 0)
                                item["vobo"] = String.Join(",", vobolist);

                        }
                        catch (Exception ex)
                        {

                        }
                        int val = 0;
                        // times
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();
                        //

                        foreach (string locationame in locationslist)
                        {
                            if (graph.TryGetValue(locationame, out val))
                            {
                                graph[locationame] = graph[locationame] + 1;
                                arraylinex = auxgraph[locationame];
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[locationame] = arraylinex;
                            }
                            else
                            {

                                graph.Add(locationame, 1);
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[locationame] = arraylinex;
                            }
                        }


                        result.Add(item);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                });

                tasks = (from t in tasks where t != null select t).ToArray();
                // Task.WaitAll(tasks);
                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                Dictionary<string, string> semaforos = new Dictionary<string, string>();
                 semaforos.Add("54bc26126e57600c28db7ebd", "#ff0000");

              
                
                 Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;
               
                ViewData["semaforos"] = semaforos;
                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }

        public ActionResult getDetail(string iddemand)
        {
            try
            {
                String demand = ObjectsRealdb.GetRowJoin(iddemand);
                JObject demandja = JsonConvert.DeserializeObject<JArray>(demand).First() as JObject;
               int i=0;
               JArray result = new JArray();
               string type = "";
               JObject extradata = new JObject();
                foreach (JObject item in demandja["objects"])
                {
                    try
                    {
                        String obj = ObjectsRealdb.GetRow(item["id"].ToString());
                        String locs = locationdb.GetRow(item["location"].ToString());
                        JObject locsjo = new JObject();
                        JObject objjo = new JObject();
                        try {   locsjo = JsonConvert.DeserializeObject<JObject>(locs); }
                        catch (Exception ex) { 
                        
                        }
                        try {    objjo = JsonConvert.DeserializeObject<JObject>(obj);
                        }catch (Exception ex) {
                        
                        }

                        JToken xyz;
                        try
                        {
                            item.Add("name", "");
                        }
                        catch { }
                        if (item.TryGetValue("name", out xyz))
                        {
                           
                            try
                            {

                                JObject objname = JsonConvert.DeserializeObject<JObject>(Objrefdb.GetRow(item["objectReference"].ToString()));
                                item["name"] = objname["name"].ToString();
                            }
                            catch { }
                        }
                        else
                        {
                            
                        }
                        if (locsjo.TryGetValue("name", out xyz))
                        {
                           // demandja["objects"].ElementAt(i)["location"] = locsjo["name"].ToString();
                            item["location"]= locsjo["name"].ToString();
                        }
                        else
                        {
                            
                        }
                        if (objjo.TryGetValue("status", out xyz))
                        {
                            item.Add("status", objjo["status"].ToString());

                        }
                        else
                        {
                            item.Add("status","");
                         }
                        if (objjo.TryGetValue("EPC", out xyz))
                        {
                            item.Add("EPC", objjo["EPC"].ToString());

                        }
                        else
                        {
                            item.Add("EPC", "Sin Epc");
                        }
                       
                        if (objjo.TryGetValue("serie", out xyz))
                        {
                            if (!item.TryGetValue("serie", out xyz))
                            {
                                item.Add("serie", objjo["serie"].ToString());
                            }
                            else
                            {
                                item["serie"] = objjo["serie"].ToString();
                            }
                        }
                        else
                        {
                            if (!item.TryGetValue("serie", out xyz))
                            {
                                item.Add("serie", "");
                            }
                            
                        }
                        if (!item.TryGetValue("object_id", out xyz))
                            item.Add("object_id", "");
                        if (!item.TryGetValue("price", out xyz))
                            item.Add("price", "");
                        if (!item.TryGetValue("marca", out xyz))
                            item.Add("marca", "");
                        if (!item.TryGetValue("modelo", out xyz))
                            item.Add("modelo", "");
                        if (!item.TryGetValue("quantity", out xyz))
                            item.Add("quantity", "");
                        if (item.TryGetValue("locationDestiny", out xyz))
                        {
                            type = "transferencia";
                            try
                            {
                                JObject locdest = JsonConvert.DeserializeObject<JObject>(locationdb.GetRow(item["locationDestiny"].ToString()));
                                item["locationDestiny"] = locdest["name"].ToString();
                            }
                            catch { }
                        }else{
                            item.Add("locationDestiny", "");
                        }
                        if (item.TryGetValue("conjuntoDestiny", out xyz))
                        {
                            type = "transferencia";
                            try
                            {
                                JObject locconj = JsonConvert.DeserializeObject<JObject>(locationdb.GetRow(item["conjuntoDestiny"].ToString()));
                                item["conjuntoDestiny"] = locconj["name"].ToString();
                            }
                            catch { }
                        }
                        else
                        {
                            item.Add("conjuntoDestiny", "");
                        }
                        if (item.TryGetValue("conjunto", out xyz))
                        {
                            
                            try
                            {
                                JObject locconj = JsonConvert.DeserializeObject<JObject>(locationdb.GetRow(item["conjunto"].ToString()));
                                item["conjunto"] = locconj["name"].ToString();
                            }
                            catch { }
                        }
                        else
                        {
                            item.Add("conjunto", "");
                        }
                       
                        if (!item.TryGetValue("location", out xyz))
                            item.Add("location", "");
                        if (!item.TryGetValue("conjunto", out xyz))
                            item.Add("conjunto", "");
                    
                        result.Add(item);

                       
                    }
                    catch (Exception ex)
                    {   
                    }
                    i++;
                }
                try
                {
                    JToken xyz;
                    if (demandja.TryGetValue("AuthorizedDate", out xyz))
                    {
                        extradata.Add("AuthorizedDate", demandja["AuthorizedDate"].ToString());
                    }
                    else
                    {
                        extradata.Add("AuthorizedDate", "");
                    }
                    if (demandja.TryGetValue("ApprovedDate", out xyz))
                    {
                        extradata.Add("ApprovedDate", demandja["ApprovedDate"].ToString());
                    }
                    else
                    {
                        extradata.Add("ApprovedDate", "");
                    }
                    if (demandja.TryGetValue("authorizations", out xyz))
                    {
                        List<string> namesauto = new List<string>();

                        foreach (var aut in demandja["authorizations"])
                        {
                            string nameauto = "";
                            string lastname = "";
                            try
                            {

                                try
                                {
                                    nameauto = aut["name"].ToString();
                                    lastname = aut["lastname"].ToString();

                                }
                                catch
                                {

                                }

                                namesauto.Add(nameauto + " " + lastname);
                            }
                            catch
                            {

                            }
                        }
                        if (namesauto.Count() > 0)
                        {

                            extradata.Add("authorizations", String.Join(",", namesauto));
                        }
                        else
                        {
                            extradata.Add("authorizations", "Sin Autorizadores!!");
                        }

                    }
                    else
                    {
                        extradata.Add("authorizations", "Sin Autorizadores");
                    }

                    if (demandja.TryGetValue("approval", out xyz))
                    {
                        List<string> namesvobo = new List<string>();

                        foreach (var aut in demandja["approval"])
                        {
                            string namevobo = "";
                            string lastname = "";
                            try
                            {

                                try
                                {
                                    namevobo = aut["name"].ToString();
                                    lastname = aut["lastname"].ToString();

                                }
                                catch
                                {

                                }

                                namesvobo.Add(namevobo + " " + lastname);
                            }
                            catch
                            {

                            }
                        }
                        if (namesvobo.Count() > 0)
                        {

                            extradata.Add("approval", String.Join(",", namesvobo));
                        }
                        else
                        {
                            extradata.Add("approval", "Sin Visto Bueno!!");
                        }

                    }
                    else
                    {
                        extradata.Add("approval", "Sin Visto Bueno");
                    }
                }
                catch { }
                ViewBag.type = type;
                ViewData["extras"] = extradata;
                return View(result);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string sendalerts()
        {

            try
            {
                String demands = demanddb.GetByStatus("status", 3);
                String alerts = alertsdb.GetRows();
                String semaphores = semaphoredb.GetRows();
                JArray demandsja = new JArray();
                JArray semaphoresja = new JArray();
                JArray alertsja = new JArray();
                try
                {
                    demandsja = JsonConvert.DeserializeObject<JArray>(demands);
                }
                catch (Exception ex)
                {
                }
                try
                {
                    semaphoresja = JsonConvert.DeserializeObject<JArray>(semaphores);

                }
                catch (Exception ex)
                {

                }
                try
                {
                    alertsja = JsonConvert.DeserializeObject<JArray>(alerts);

                }
                catch (Exception ex)
                {

                }
                Task<string>[] tasks = null;
                
                try
                {
                    var noapprovedList = (from mov in demandsja where mov["authorizations"].Children()["approved"].Contains("0") && (string)mov["status"] == "3" select new { id = (string)mov["_id"], iduser = (from xs in mov["authorizations"].Children() where (string)xs["approved"] == "0" select xs["id_user"].Value<string>()).ToList(), date = (string)mov["CreatedDate"], type = (string)mov["typemov"], namemov = (string)mov["nameMovement"], folio = (string)mov["folio"] }).ToList();
                    var semaphores1 = (from item in semaphoresja select new { color = (string)item["color"], days = (int)item["days"], type = (string)item["typeMovement"] }).ToList();
                    var alertslist = (from alr in alertsja.Children() select new { user = (string)alr["to"], demand = (string)alr["demand"] }).ToList();
                    tasks = new Task<string>[noapprovedList.Count()];
                 //   List<string> listn = (from ap in noapprovedList join al in alertslist on ap.id equals al.demand select (string)al.demand).ToList();
                   // Dictionary<string, string> alertsusers = alertslist.ToDictionary(x => (string)x.user, x => (string)x.demand);
                             
                    foreach (var item in noapprovedList)
                    {
                        DateTime datenow = DateTime.Now;
                        DateTime dateCreated = DateTime.ParseExact(item.date.Substring(0, 10), "dd/MM/yyyy", null);
                        TimeSpan time = datenow - dateCreated;
                        int diferences = time.Days;
                        var values = 0;
                        bool mailsend = false;
                        List<string> listusers = new List<string>();
                        foreach (var sem in semaphores1)
                        {
                            if (diferences >= sem.days && item.type == sem.type)
                            {
                                  foreach (string userx in item.iduser)
                                {
                                    var results=alertslist.Where(x=>x.user==userx && x.demand==item.id).Select(x=>x).ToList();
                                    if (results.Count()==0)
                                    {
                                        listusers.Add(userx);
                                        mailsend = true;
                                    }

                                }
                            }


                        }

                        if (mailsend)
                        {
                            try
                            {
                                JArray recipients = new JArray();
                                JArray attachments = new JArray();
                                string namemov = item.namemov;
                                foreach (string userid in listusers.Distinct())
                                {
                                    recipients.Add(userid);
                                }
                                string to = JsonConvert.SerializeObject(recipients);
                                string attach = JsonConvert.SerializeObject(attachments);

                                //  messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema");
                           
                                Task<string>[] tasksave = null;
                                tasks[noapprovedList.IndexOf(item)] = Task.Factory.StartNew(() => messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema"));
                                tasksave = new Task<string>[listusers.Distinct().Count()];
                                int ind = 0;
                                foreach (string destiny in listusers.Distinct())
                                {
                                    JObject alertdata = new JObject();
                                    alertdata.Add("to", destiny);
                                    alertdata.Add("demand", item.id);
                                    alertdata.Add("from", "Sisteme");
                                    alertdata.Add("subject", "Urgente:Solicitud con folio #" + item.folio + ",esta pendiente de autorizar");
                                    alertdata.Add("msg", "La Solicitud de " + namemov + " con folio #" + item.folio + ",esta pendiente de autorizar,favor de Autorizarla lo antes posible!!");
                                    String alertjson = JsonConvert.SerializeObject(alertdata);
                                    tasksave[ind] = Task.Factory.StartNew(() => alertsdb.SaveRow(alertjson));
                                }
                                Task.WaitAll(tasksave);
                            }
                            catch (Exception ex) { }
                        }



                    }

                }
                catch (Exception ex) { return null; }
                return "true";
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ActionResult GenerateMovementMasterReport(string startdate, string enddate, string movements = null, string objects = null, string status = null, string dict = null, string done = null, string mp = null, string destroy = null, string auto = null, string vobo = null, string photo = null, string extrafilter=null,string users=null,string locs=null)
        {

            if (startdate == "Indefinida" || startdate == null)
            {
                startdate = "01/01/1900";

            }
            if (enddate == "Indefinida" || enddate == null)
            {
                enddate = "01/01/3000";
            }
            //    string mapreduce = demanddb.Mapreduced();
            try
            {
                int numhw = 0;

                List<string> listmovements = new List<string>();
                List<string> listobjects = new List<string>();
                 List<int> liststatus = new List<int>();
                 List<string> listusers = new List<string>();
                 List<string> listlocs = new List<string>();
                JArray movementja = JsonConvert.DeserializeObject<dynamic>(movements);
                JArray objectsja = JsonConvert.DeserializeObject<JArray>(objects);
                 JArray statusja = JsonConvert.DeserializeObject<JArray>(status);
                 JArray usersja = JsonConvert.DeserializeObject<JArray>(users);
                 JArray locsja = JsonConvert.DeserializeObject<JArray>(locs);
                //   var url = movementja.Select(m => new { id = (string)m["id"] });
                 if (locsja.Count == 0)
                 {
                     string locsx = locationdb.GetRows();
                     locsja = JsonConvert.DeserializeObject<JArray>(locsx);
                     listlocs = (from loc in locsja select (string)loc["_id"]).ToList();

                 }
                 else
                 {
                     listlocs = (from loc in locsja select (string)loc["id"]).ToList();

                 }
                listmovements = (from mov in movementja select (string)mov["id"]).ToList();
                listobjects = (from obj in objectsja select (string)obj["id"]).ToList();
                listusers = (from user in usersja select (string)user["id"]).ToList();
               
                 // listmovements = url.Where(p => p.Name == "id").Select(p => p.Value).ToList<string>();*/
                // listmovements = url;
                /*   listobjects = objectsja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   listlocations = locationsja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   listusers = usersja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   liststatus = statusja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                */
              
                foreach (JObject x in statusja)
                {
                    if (x["id"].ToString() == "3")
                    {
                        for (int i = 1; i < 6; i++)
                        {

                            liststatus.Add(i);
                        }

                    }
                    else
                    {
                        liststatus.Add(Convert.ToInt16(x["id"].ToString()));
                    }
                }


                // JArray cols = JsonConvert.DeserializeObject<JArray>(movements);
                Dictionary<string, string> datacols = new Dictionary<string, string>();

                datacols.Add("folio", "id_Solicitud(folio)");
                datacols.Add("object", "Descripcion_activo");
                datacols.Add("location", "Descripcion_Ubicacion");


                datacols.Add("movement", "Descripcion");
                datacols.Add("status", "Estatus_Solicitud");
                
               
                datacols.Add("Creator", "Solicitada Por");
                datacols.Add("CreatedDate", "Fecha_de_Solicitud");
                if (extrafilter == "alls")
                {
                    datacols.Add("deleteType", "Tipo_De_Baja");
                    datacols.Add("destinyOptions", "Destino de_Solicitud");
                   
                   
                   
                     //datacols.Add("actFolio", "Acta de Destrucción");
                    datacols.Add("vobo", "Visto_Bueno");
                    datacols.Add("autouser", "Autorizador(es)");
                    datacols.Add("images", "Ver_Imagen(es)");
                    datacols.Add("objects", "detalle");
                    datacols.Add("report", "Reporte Generado");
                }
                else
                {
                    datacols.Add("vobo", "Visto_Bueno");
                    datacols.Add("autouser", "Autorizador(es)");
                    datacols.Add("objects", "detalle");

                }
                datacols.Add("autodeny", "Aut. Rechazada por:");
                datacols.Add("vobodeny", "V.O.B.O Denegado por:");
                datacols.Add("Deniers", "Rechazada por:");
                datacols.Add("DenyDate", "Fecha de Rechazo");
                datacols.Add("receiptFile", "Comprobantes");
                 
                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);


                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                //string mapreduce = demanddb.Mapreduced(datacols, listmovements, listobjects, listlocations, listusers, start, end);
            
                Task<string> task1 = Task<string>.Factory.StartNew(() => locationdb.GetRows());
                Task<string> task2 = Task<string>.Factory.StartNew(() => Objrefdb.GetRows());
             //   Task<string> task3 = Task<string>.Factory.StartNew(() => semaphoredb.GetRows());
                JArray result = new JArray();
                Dictionary<string, int> graph = new Dictionary<string, int>();
                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();
                
                 string gethw = demanddb.GetRowsReportMasterDemand(datacols, listmovements, listobjects, liststatus, start, end, dict, done, mp, destroy, auto, vobo, photo,listusers,listlocs);
                 Task.WaitAll(task1, task2);
                string allslocations = task1.Result;
                 string allsobjects = task2.Result;
                 string allssemaphores = semaphoredb.GetRows();
             
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray allobjsjaa = JsonConvert.DeserializeObject<JArray>(allsobjects);
                JArray alllocjaa = JsonConvert.DeserializeObject<JArray>(allslocations);
                JArray alllsemaphorejaa = JsonConvert.DeserializeObject<JArray>(allssemaphores);
                var dictionaryobjs = allobjsjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                var dictionarylocs = alllocjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                Dictionary<int, string> statusdict = new Dictionary<int, string>();
                statusdict.Add(1, "Dictamen Pendiente");
                statusdict.Add(2, "Con Dictamen y En Espera de Datos Adicionales");
                statusdict.Add(3, "Con Dictamen, Datos Adicionales  y En Espera de Autorizaciones");
                statusdict.Add(4, "Con Dictamen, Datos Adicionales, Autorizaciones y En Espera de Comprobantes");
                statusdict.Add(5, "Con Dictamen, Datos Adicionales, Autorizaciones,Comprobantes y En Espera del VOBO");
                statusdict.Add(6, "Solicitud Autorizada y Aplicada");
                statusdict.Add(7, "Denegada");
                Dictionary<string, string> semaphorosvalues = new Dictionary<string, string>();
               Task<string>[] tasks=null;
              
                try
                {
                     var noapprovedList = (from mov in hwsja where mov["authorizations"].Children()["approved"].Contains("0") && (string)mov["status"] == "3" select new { id = (string)mov["_id"], iduser = (from xs in mov["authorizations"].Children() where (string)xs["approved"] == "0" select xs["id_user"].Value<string>()).ToList(), date = (string)mov["CreatedDate"], type = (string)mov["typemov"], namemov = (string)mov["nameMovement"], folio = (string)mov["folio"] }).ToList();
                     
                    ViewData["approva"] = noapprovedList.ToJson();
                    var semaphores1 = (from item in alllsemaphorejaa select new { color = (string)item["color"], days = (int)item["days"], type = (string)item["typeMovement"] }).ToList();
                    ViewData["semap"] = semaphores1.ToJson();
                      tasks= new Task<string>[noapprovedList.Count()];
                    foreach (var item in noapprovedList)
                    {
                        DateTime datenow = DateTime.Now;
                        DateTime dateCreated = DateTime.ParseExact(item.date.Substring(0, 10), "dd/MM/yyyy", null);
                        TimeSpan time = datenow - dateCreated;
                        int diferences = time.Days;
                        var values = 0;
                        bool mailsend = false;
                        foreach (var sem in semaphores1)
                        {
                            if (diferences >= sem.days && item.type == sem.type)
                            {
                                if (sem.days > values)
                                {
                                    try
                                    {
                                        semaphorosvalues.Add(item.id, sem.color);
                                    }
                                    catch (Exception ex)
                                    {
                                        semaphorosvalues[item.id] = sem.color;

                                    }
                                    values = sem.days;
                                    mailsend = true;
                                }
                            }
                        }
                      /*  if (mailsend)
                        {
                            try
                            {
                                JArray recipients = new JArray();
                                JArray attachments = new JArray();
                                string namemov = item.namemov;
                                foreach (string userid in item.iduser)
                                {
                                    recipients.Add(userid);
                                }
                                string to = JsonConvert.SerializeObject(recipients);
                                string attach = JsonConvert.SerializeObject(attachments);
                               
                                //  messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema");
                             tasks[noapprovedList.IndexOf(item)] = Task.Factory.StartNew(() => messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema"));
                                        }
                            catch (Exception ex) { }
                        }*/
                    }

                }
                catch (Exception ex) { }
                  ViewData["test"] = semaphorosvalues;
               // Parallel.ForEach(hwsja,itemtoken => 
                foreach(JObject item in hwsja )
                  {
                      //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                      try
                      {

                       // JObject  item = itemtoken.Value<JObject>();
                        
                          List<string> locationslist = new List<string>();
                          List<string> objlist = new List<string>();
                          string v = "";
                          //    if (datacols.TryGetValue("profileId",out v)){
                          string value = "";
                          //   var semaphores = (from mov in item["authorizations"] where (string)mov["approved"] == "1" select new { id = (string)mov["_id"], iduser = (string)mov["id_user"], date = (string)mov["CreatedDate"] }).ToList();
                          /* string getobjs = Objrefdb.GetRow(item["objects"]["objectReference"].ToString());
                           JObject objsjaa = JsonConvert.DeserializeObject<JObject>(getobjs);*/
                          StringBuilder objectsall = new StringBuilder();
                          StringBuilder locsall = new StringBuilder();

                         
                          foreach (var arrayobject in item["objects"])
                          {
                              string locsname = dictionarylocs[arrayobject["location"].ToString()];
                              string objsname = dictionaryobjs[arrayobject["objectReference"].ToString()];
                              if (!locationslist.Contains(locsname))
                              {
                                  locationslist.Add(locsname);

                              }
                              if (!objlist.Contains(objsname))
                              {
                                  objlist.Add(objsname);


                              }
                          }
                          objectsall.Append(String.Join(",", objlist));
                          locsall.Append(String.Join(",", locationslist));


                          try
                          {
                              if (item["deleteType"].ToString() == "planeada")
                              {
                                  item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                              }
                              else {
                                 /* switch (item["status"].ToString())
                                  {
                                      case "1":
                                          item["status"] = statusdict[1];
                                          break;
                                      case "2":
                                          item["status"] = statusdict[1];
                                          break;
                                      case "3":
                                          item["status"] = statusdict[2];
                                          break;
                                      case "4":
                                          item["status"] = statusdict[3];
                                          break;
                                      case "5":
                                          item["status"] = statusdict[4];
                                          break;

                                      case "6":
                                          item["status"] = statusdict[5];
                                          break;
                                      case "7":
                                          item["status"] = statusdict[6];
                                          break;
                                      default:
                                          item["status"] = "Pendiente";
                                          break;
                                  }*/
                                  item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                                  item["deleteType"] = "No planeada";
                              }
                          }
                          catch (Exception ex)
                          {
                              item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                          }

                          item["location"] = locsall.ToString();
                          item["movement"] = item["nameMovement"].ToString();
                          item["Creator"] = item["nameUser"].ToString();
                          item["object"] = objectsall.ToString();
                          item.Add("images", "sin Imagen");
                          item.Add("autouser", "sin Autorizador(es)");
                          item.Add("vobo", "sin Visto Bueno");
                          try
                          {
                              List<string> imageslist = item["objects"].Children()["images"].Values<string>().ToList();
                              if (imageslist.Count() > 0)
                              {
                                  if (photo == "2")
                                  {

                                     continue;
                                  }
                                  item["images"] = String.Join(",", imageslist);
                              }
                              else if (photo == "1")
                              {
                                  continue;
                              }

                          }
                          catch (Exception ex) { }

                          try
                          {
                              List<string> userautolist = item["authorizations"].Children()["user"].Values<string>().ToList();
                              if (userautolist.Count() > 0)
                                  item["autouser"] = String.Join(",", userautolist);
                              List<string> listdeny = new List<string>();
                              foreach (JObject autos in item["authorizations"])
                              {
                                  try
                                  {
                                      if (autos["approved"].ToString() == "2")
                                      {
                                          listdeny.Add(autos["user"].ToString());
                                      }
                                  }
                                  catch
                                  {

                                  }
                              }
                              if (listdeny.Count() > 0)
                                  item["autodeny"] = String.Join(",", listdeny);

                          }
                          catch (Exception ex)
                          {

                          }
                         /* try
                          {
                              List<string> receipt = new List<string>(); 
                              foreach (string rec in item["receiptFile"])
                              {
                                  try
                                  {

                                      receipt.Add("<a  href='/Uploads/Dictamenes/documentos/"+rec+"' target='_blank'>"+rec+"</a>");
                                      
                                  }
                                  catch
                                  {

                                  }
                              }
                              if (receipt.Count() > 0)
                                  item["receiptFile"] = String.Join("<br/>", receipt);

                          }
                          catch (Exception ex)
                          {

                          }*/
                          try
                          {
                              List<string> vobolist = item["approval"].Children()["user"].Values<string>().ToList();
                              if (vobolist.Count() > 0)
                                  item["vobo"] = String.Join(",", vobolist);
                              List<string> listvobos = new List<string>();
                              foreach (JObject vobos in item["approval"])
                              {
                                  try
                                  {
                                      if (vobos["approved"].ToString() == "2")
                                      {
                                          listvobos.Add(vobos["user"].ToString());
                                      }
                                  }
                                  catch
                                  {

                                  }
                              }
                              if (listvobos.Count() > 0)
                                  item["vobodeny"] = String.Join(",", listvobos);
                          }
                          catch (Exception ex)
                          {

                          }
                          int val = 0;
                          // times
                          int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                          int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                          if (month.ToString().Substring(0, 1) == "0")
                          {

                              month = Convert.ToInt16(month.ToString().Substring(1));

                          }
                          int[] arraylinex = aux.ToArray();
                          //

                          foreach (string locationame in locationslist)
                          {
                              if (graph.TryGetValue(locationame, out val))
                              {
                                  graph[locationame] = graph[locationame] + 1;
                                  arraylinex = auxgraph[locationame];
                                  arraylinex = getgraph(years, arraylinex, month, year, headm);
                                  auxgraph[locationame] = arraylinex;
                              }
                              else
                              {

                                  graph.Add(locationame, 1);
                                  arraylinex = getgraph(years, arraylinex, month, year, headm);
                                  auxgraph[locationame] = arraylinex;
                              }
                          }

                          try
                          {
                              JToken actas;
                              if (item.TryGetValue("dctFolio", out actas))
                              {
                                  item.Add("report", item["dctFolio"].ToString());
                                
                              }
                              if (item.TryGetValue("ActFolio", out actas))
                              {
                                  item.Add("report", item["ActFolio"].ToString());
                                 
                              }
                          }
                          catch { }
                          result.Add(item);
                          numhw++;
                          // }
                      }
                      catch (Exception ex)
                      {


                      }
                  }

              tasks = (from t in tasks where t != null select t).ToArray();
            Task.WaitAll(tasks);
                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;
                ViewData["semaphores"] = semaphorosvalues;
                Dictionary<string, string> semaforo = new Dictionary<string, string>();
             
               // semaphores.Add("54bc26126e57600c28db7ebd", "#ff0000");
                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }
                semaforo.Add("test", "TEST");
                foreach (var i in semaphorosvalues)
                {
                    semaforo.Add(i.Key, i.Value);
                }
                ViewData["semaforo"] = semaforo;
                
                 ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
        public ActionResult GenerateCustomReport(string startdate, string enddate, string movements = null, string objects = null, string locations = null, string users = null, string status = null)
        {
            if (startdate == "Indefinida" || startdate == null)
            {
                startdate = "01/01/1900";

            }
            if (enddate == "Indefinida" || enddate == null)
            {
                enddate = "01/01/3000";
            }
            //    string mapreduce = demanddb.Mapreduced();
            try
            {
                int numhw = 0;

                List<string> listmovements = new List<string>();
                List<string> listobjects = new List<string>();
                List<string> listlocations = new List<string>();
                List<string> listusers = new List<string>();
                List<int> liststatus = new List<int>();
                JArray movementja = JsonConvert.DeserializeObject<dynamic>(movements);
                JArray objectsja = JsonConvert.DeserializeObject<JArray>(objects);
                JArray locationsja = JsonConvert.DeserializeObject<JArray>(locations);
                JArray usersja = JsonConvert.DeserializeObject<JArray>(users);
                JArray statusja = JsonConvert.DeserializeObject<JArray>(status);
                //   var url = movementja.Select(m => new { id = (string)m["id"] });
                listmovements = (from mov in movementja select (string)mov["id"]).ToList();
                listobjects = (from obj in objectsja select (string)obj["id"]).ToList();
                listlocations = (from loc in locationsja select (string)loc["id"]).ToList();
                listusers = (from user in usersja select (string)user["id"]).ToList();
                // listmovements = url.Where(p => p.Name == "id").Select(p => p.Value).ToList<string>();*/
                // listmovements = url;
                /*   listobjects = objectsja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   listlocations = locationsja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   listusers = usersja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                   liststatus = statusja.Descendants().OfType<JProperty>().Where(p => p.Name == "id").ToList();
                */
                /*  foreach (JObject x in movementja)
                    {
                        listmovements.Add(x["id"].ToString());
                    }
                    foreach (JObject x in objectsja)
                    {
                        listobjects.Add(x["id"].ToString());
                    }
                    foreach (JObject x in locationsja)
                    {
                        listlocations.Add(x["id"].ToString());
                    } 
                    foreach (JObject x in usersja)
                    {
                        listusers.Add(x["id"].ToString());
                    }
                    */
                foreach (JObject x in statusja)
                {
                    if (x["id"].ToString() == "3")
                    {
                        for (int i = 1; i < 6; i++)
                        {

                            liststatus.Add(i);
                        }

                    }
                    else
                    {
                        liststatus.Add(Convert.ToInt16(x["id"].ToString()));
                    }
                }


                // JArray cols = JsonConvert.DeserializeObject<JArray>(movements);
                Dictionary<string, string> datacols = new Dictionary<string, string>();

                datacols.Add("folio", "id Solicitud");
                datacols.Add("object", "Activo");
                datacols.Add("location", "Ubicacion");


                datacols.Add("movement", "Descripcion");
                datacols.Add("status", "Tipo de Solicitud");
                datacols.Add("Creator", "Solicitada Por");
                datacols.Add("CreatedDate", "Fecha de Solicitud");



                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                //string mapreduce = demanddb.Mapreduced(datacols, listmovements, listobjects, listlocations, listusers, start, end);

                string gethw = demanddb.GetRowsReportDemand(datacols, listmovements, listobjects, listlocations, listusers, liststatus, start, end);
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                //timegraph

                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                //
                string allslocations = locationdb.GetRows();
                string allsobjects = Objrefdb.GetRows();
                JArray allobjsjaa = JsonConvert.DeserializeObject<JArray>(allsobjects);
                JArray alllocjaa = JsonConvert.DeserializeObject<JArray>(allslocations);
                var dictionaryobjs = allobjsjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                var dictionarylocs = alllocjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                Dictionary<int, string> statusdict = new Dictionary<int, string>();
                statusdict.Add(1, "Dictamen Pendiente");
                statusdict.Add(2, "Con Dictamen y En Espera de Datos Adicionales");
                statusdict.Add(3, "Con Dictamen, Datos Adicionales  y En Espera de Autorizaciones");
                statusdict.Add(4, "Con Dictamen, Datos Adicionales, Autorizaciones y En Espera de Comprobantes");
                statusdict.Add(5, "Con Dictamen, Datos Adicionales, Autorizaciones,Comprobantes y En Espera del VOBO");
                statusdict.Add(6, "Solicitud Autorizada y Aplicada");
                statusdict.Add(7, "Denegada");


                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        List<string> locationslist = new List<string>();
                        List<string> objlist = new List<string>();
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";

                        /* string getobjs = Objrefdb.GetRow(item["objects"]["objectReference"].ToString());
                         JObject objsjaa = JsonConvert.DeserializeObject<JObject>(getobjs);*/
                        StringBuilder objectsall = new StringBuilder();
                        StringBuilder locsall = new StringBuilder();
                        foreach (var arrayobject in item["objects"])
                        {
                            /*  string getobjs = Objrefdb.GetRow(arrayobject["objectReference"].ToString());
                              string getloc = locationdb.GetRow(arrayobject["location"].ToString());
                              JObject objsjaa = JsonConvert.DeserializeObject<JObject>(getobjs);
                              JObject locjaa = JsonConvert.DeserializeObject<JObject>(getloc);*/
                            string locsname = dictionarylocs[arrayobject["location"].ToString()];
                            string objsname = dictionaryobjs[arrayobject["objectReference"].ToString()];
                            // var dasda = from b in allobjsjaa where b["_id"].Equals(arrayobject["objectReference"].ToString()) select b;
                            // var dasd = alllocjaa.Where(x["_id"] == arrayobject["location"].ToString()).First();
                            // var disad =   dictionarylocs.Select(d => d.Value).Cast<Dictionary<string,string>>().Where(dd => dd.ContainsKey(arrayobject["location"]));
                            /* var url = (string)allobjsjaa.Descendants()
                                  .OfType<JProperty>()
                                  .Where(p => p.Name == "name" && (string)p.Value == objsname).First();
                                 // .Value;*/
                            if (!locationslist.Contains(locsname))
                            {
                                locationslist.Add(locsname);
                                //locsall.Append(locsname + ",");//+Environment.NewLine

                            }
                            if (!objlist.Contains(objsname))
                            {
                                // objectsall.Append(objsname + ",");
                                objlist.Add(objsname);


                            }
                        }
                        objectsall.Append(String.Join(",", objlist));
                        locsall.Append(String.Join(",", locationslist));


                        try
                        {
                            if (item["deleteType"].ToString() == "planeada")
                            {
                                item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                            }
                        }
                        catch (Exception ex)
                        {
                            switch (item["status"].ToString())
                            {
                                case "6":
                                    item["status"] = statusdict[6];
                                    break;
                                case "7":
                                    item["status"] = statusdict[7];
                                    break;
                                default:
                                    item["status"] = "Pendiente";
                                    break;
                            }
                        }

                        item["location"] = locsall.ToString();
                        item["movement"] = item["nameMovement"].ToString();
                        item["Creator"] = item["nameUser"].ToString();
                        //  item["object"] = objsjaa["name"].ToString();
                        item["object"] = objectsall.ToString();

                        int val = 0;
                        // times
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();
                        //

                        foreach (string locationame in locationslist)
                        {
                            if (graph.TryGetValue(locationame, out val))
                            {
                                graph[locationame] = graph[locationame] + 1;
                                arraylinex = auxgraph[locationame];
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[locationame] = arraylinex;
                            }
                            else
                            {

                                graph.Add(locationame, 1);
                                arraylinex = getgraph(years, arraylinex, month, year, headm);
                                auxgraph[locationame] = arraylinex;
                            }
                        }


                        result.Add(item);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }



                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
    
        public ActionResult GenerateMovementHistoryReport(string startdate, string enddate, string movements = null, string users = null)
        {
            if (startdate == "Indefinida" || startdate == null)
            {
                startdate = "01/01/1900";

            }
            if (enddate == "Indefinida" || enddate == null)
            {
                enddate = "01/01/3000";
            }
        //    string mapreduce = demanddb.Mapreduced();
            try
            {
                int numhw = 0;

              List<string> listmovements = new List<string>();
                 List<string> listusers = new List<string>();
                    JArray movementja = JsonConvert.DeserializeObject<dynamic>(movements);
                  JArray usersja = JsonConvert.DeserializeObject<JArray>(users);
                 //   var url = movementja.Select(m => new { id = (string)m["id"] });
                listmovements = (from mov in movementja select (string)mov["id"]).ToList();
                   listusers = (from user in usersja select (string)user["id"]).ToList();
          
              


                   Dictionary<string, string> datacols = new Dictionary<string, string>();
                
                    datacols.Add("folio", "id Solicitud");
                    datacols.Add("object", "Activo");
                    datacols.Add("location", "Conjunto");
                    
                   
                    datacols.Add("movement", "Descripcion");
                    datacols.Add("status", "Tipo de Solicitud");
                    datacols.Add("Creator", "Solicitada Por");
                    datacols.Add("CreatedDate", "Fecha de Solicitud");
                  
               

                string[] date = startdate.Split('/');
                string[] date2 = enddate.Split('/');
                string timestamp = String.Format("{0}{1}{2}000000", date[2], date[0], date[1]);
                string timestamp2 = String.Format("{0}{1}{2}000000", date2[2], date2[0], date2[1]);

                // graphlinetime
                int years = Convert.ToInt16(date2[2]) - Convert.ToInt16(date[2]);
                Dictionary<int, string> headm = new Dictionary<int, string>();
                List<int> aux = new List<int>();
                string firsh = "";
                headgraph data = Getdatagraph(years, date[2], date2[2]);
                headm = data.Head;
                aux = data.listA;
                firsh = data.range;

                //end graphlinetime


                var start = Convert.ToInt64(timestamp);
                var end = Convert.ToInt64(timestamp2);
                List<string> dates = new List<string>();
                if (date[2] == "1900")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(startdate);
                }
                if (date2[2] == "3000")
                {
                    dates.Add("Indefinida");
                }
                else
                {
                    dates.Add(enddate);

                }
                string[] datesarray = dates.ToArray();
                ViewData["dates"] = dates;
                //string mapreduce = demanddb.Mapreduced(datacols, listmovements, listobjects, listlocations, listusers, start, end);

                string gethw = demanddb.GetRowsReportHistoryDemand(datacols, listmovements, listusers, start, end);
                JArray hwsja = JsonConvert.DeserializeObject<JArray>(gethw);
                JArray result = new JArray();

                Dictionary<string, int> graph = new Dictionary<string, int>();
                //timegraph

                Dictionary<string, int[]> auxgraph = new Dictionary<string, int[]>();


                //
                string allslocations = locationdb.GetRows();
                string allsobjects = Objrefdb.GetRows();
                JArray allobjsjaa = JsonConvert.DeserializeObject<JArray>(allsobjects);
                JArray alllocjaa = JsonConvert.DeserializeObject<JArray>(allslocations);
                var dictionaryobjs = allobjsjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                var dictionarylocs = alllocjaa.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                Dictionary<int, string> statusdict = new Dictionary<int, string>();
                statusdict.Add(1, "Dictamen Pendiente");
                statusdict.Add(2, "Con Dictamen y En Espera de Datos Adicionales");
                statusdict.Add(3, "Con Dictamen, Datos Adicionales  y En Espera de Autorizaciones");
                statusdict.Add(4, "Con Dictamen, Datos Adicionales, Autorizaciones y En Espera de Comprobantes");
                statusdict.Add(5, "Con Dictamen, Datos Adicionales, Autorizaciones,Comprobantes y En Espera del VOBO");
                statusdict.Add(6, "Solicitud Autorizada y Aplicada");
                statusdict.Add(7, "Denegada");
               

                foreach (JObject item in hwsja)
                {
                    //    if (Convert.ToInt64(item["CreatedTimeStamp"].ToString()) >= start && Convert.ToInt64(item["CreatedTimeStamp"].ToString()) <= end) {
                    try
                    {
                        List<string> locationslist = new List<string>();
                        List<string> objlist = new List<string>();
                        string v = "";
                        //    if (datacols.TryGetValue("profileId",out v)){
                        string value = "";

                       /* string getobjs = Objrefdb.GetRow(item["objects"]["objectReference"].ToString());
                        JObject objsjaa = JsonConvert.DeserializeObject<JObject>(getobjs);*/
                        StringBuilder objectsall = new StringBuilder();
                        StringBuilder locsall = new StringBuilder();
                        foreach (var arrayobject in item["objects"])
                        {
                          /*  string getobjs = Objrefdb.GetRow(arrayobject["objectReference"].ToString());
                            string getloc = locationdb.GetRow(arrayobject["location"].ToString());
                            JObject objsjaa = JsonConvert.DeserializeObject<JObject>(getobjs);
                            JObject locjaa = JsonConvert.DeserializeObject<JObject>(getloc);*/
                            string locsname = dictionarylocs[arrayobject["location"].ToString()];
                            string objsname = dictionaryobjs[arrayobject["objectReference"].ToString()];
                           // var dasda = from b in allobjsjaa where b["_id"].Equals(arrayobject["objectReference"].ToString()) select b;
                           // var dasd = alllocjaa.Where(x["_id"] == arrayobject["location"].ToString()).First();
                           // var disad =   dictionarylocs.Select(d => d.Value).Cast<Dictionary<string,string>>().Where(dd => dd.ContainsKey(arrayobject["location"]));
                           /* var url = (string)allobjsjaa.Descendants()
                                 .OfType<JProperty>()
                                 .Where(p => p.Name == "name" && (string)p.Value == objsname).First();
                                // .Value;*/
                            if (!locationslist.Contains(locsname))
                            {
                                locationslist.Add(locsname);
                                //locsall.Append(locsname + ",");//+Environment.NewLine
                               
                            }
                            if (!objlist.Contains(objsname))
                            {
                               // objectsall.Append(objsname + ",");
                                objlist.Add(objsname);
                               
                                
                            }
                        }
                        objectsall.Append(String.Join(",", objlist));
                        locsall.Append(String.Join(",", locationslist));      
                       

                        try
                        {
                            if (item["deleteType"].ToString() == "planeada")
                            {
                                item["status"] = statusdict[Convert.ToInt16(item["status"].ToString())];
                            }
                        }
                        catch (Exception ex)
                        {
                            switch (item["status"].ToString())
                            {
                                case "6":
                                    item["status"] = statusdict[6];
                                    break;
                                case "7":
                                    item["status"] = statusdict[7];
                                    break;
                                default:
                                    item["status"] = "Pendiente";
                                    break;
                            }
                        }
                          
                        item["location"] = locsall.ToString();
                        item["movement"] = item["nameMovement"].ToString();
                        item["Creator"] = item["nameUser"].ToString();
                      //  item["object"] = objsjaa["name"].ToString();
                        item["object"] = objectsall.ToString();

                        int val = 0;
                        // times
                        int month = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(4, 2));
                        int year = Convert.ToInt16(item["CreatedTimeStamp"].ToString().Substring(0, 4));

                        if (month.ToString().Substring(0, 1) == "0")
                        {

                            month = Convert.ToInt16(month.ToString().Substring(1));

                        }
                        int[] arraylinex = aux.ToArray();
                        //

                        foreach (string locationame in locationslist)
                        {
                        if (graph.TryGetValue(locationame, out val))
                        {
                            graph[locationame] = graph[locationame] + 1;
                            arraylinex = auxgraph[locationame];
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[locationame] = arraylinex;
                        }
                        else
                        {

                            graph.Add(locationame, 1);
                            arraylinex = getgraph(years, arraylinex, month, year, headm);
                            auxgraph[locationame] = arraylinex;
                        }
                       }

                     
                        result.Add(item);
                        numhw++;
                        // }
                    }
                    catch (Exception ex)
                    {


                    }
                }



                ViewData["numproccess"] = numhw.ToString();
                ViewData["dates"] = datesarray;
                ViewData["cols"] = datacols;
                ViewData["graph"] = graph;
                ViewData["graphtime"] = auxgraph;
                ViewData["th"] = headm;
                ViewData["years"] = years;

                Dictionary<string, int[]> graphmult = new Dictionary<string, int[]>();
                Dictionary<string, int[]> graphend = new Dictionary<string, int[]>();

                List<string> headgraphmult = new List<string>();
                Dictionary<int, int[]> auxarray = new Dictionary<int, int[]>();

                headgraphmult.Add(firsh);
                foreach (var x in headm) { graphmult.Add(x.Value, null); }
                int w = 0;
                foreach (var x in auxgraph)
                {

                    headgraphmult.Add(x.Key);

                    auxarray.Add(w, x.Value);

                    w++;
                }

                w = 0;

                foreach (var x in graphmult)
                {


                    List<int> listrange = new List<int>();
                    foreach (var y in auxarray)
                    {

                        int[] a = y.Value;

                        listrange.Add(a[w]);

                    }

                    graphend.Add(x.Key, listrange.ToArray());
                    w++;
                }

                ViewData["headmult"] = headgraphmult;
                ViewData["bodygraph"] = graphend;

                return View(result);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }

        }
    
    }
}
