using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Db.MongoDb;
using Rivka.Error;
using RivkaAreas.Reports.Models;
using System.Drawing;
using MongoDB.Driver;
using System.IO;
using System.util;
using System.Net;
using System.Text.RegularExpressions;
using RivkaAreas.Design.Controllers;
using RivkaAreas.Reports.Controllers;
using RivkaAreas.Message.Controllers;
using System.Threading.Tasks;

namespace adminMain.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        protected getReports Reportsdb = new getReports("Reports");
        protected MongoModel Dashboard = new MongoModel("Dashboard");
        protected MongoModel NewDashboard = new MongoModel("Dashboards");
        protected MongoModel Widgets = new MongoModel("DashboardWidgets");
        protected MongoModel Notificationsdb = new MongoModel("Notifications");
        protected ReportsController semaphores = new ReportsController();
        protected DesignController design = new DesignController();
        protected MongoModel alertsdb = new MongoModel("Alerts");
        protected MongoModel semaphoredb = new MongoModel("Semaphore");
        protected DemandReport demanddb = new DemandReport("Demand");
        protected UsersReport Userdb = new UsersReport("Users");
        protected MessageController messagesC = new MessageController();

        /// <summary>
        ///     Get the main dashboard
        /// </summary>
        /// <returns></returns>
        /*
        public ActionResult Index()
        {
           // RivkaAreas.Rule.RulesChecker.isValidToLocation("asd","asd");
            try
            {
                string widgets = "";
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }

                string iduser = Session["_id"].ToString();
                try
                {
                    widgets = Dashboard.Get("userId", iduser);

                }
                catch (Exception ex)
                {
                    widgets = null;
                }

                JArray widgetja = JsonConvert.DeserializeObject<JArray>(widgets);
                List<string[]> widgetlist = new List<string[]>();
                foreach (JObject item1 in widgetja)
                {
                    string profilex = item1["profile"].ToString();
                    profilex = profilex.Replace("\r\n", "");
                    profilex = profilex.Replace("\\", "");

                    string movements = "[]";
                    try
                    {
                        movements = item1["movements"].ToString();
                        movements = movements.Replace("\r\n", "");
                        movements = movements.Replace("\\", "");

                    }
                    catch (Exception ex) { }
                    string objects = "[]";
                    try
                    {
                        objects = item1["objects"].ToString();
                        objects =objects.Replace("\r\n", "");
                        objects = objects.Replace("\\", "");
                    }
                    catch (Exception ex) { }
                    string locations = "[]";
                    try
                    {
                        locations = item1["locations"].ToString();
                        locations = locations.Replace("\r\n", "");
                        locations = locations.Replace("\\", "");
                    }
                    catch (Exception ex) { }
                    string users = "[]";
                    try
                    {
                        users = item1["users"].ToString();
                        users= users.Replace("\r\n", "");
                        users = users.Replace("\\", "");
                    }
                    catch (Exception ex) { }
                  
                    string fields = item1["fields"].ToString();
                    fields = fields.Replace("\r\n", "");
                    fields = fields.Replace("\\", "");

                    string[] aux = { item1["_id"].ToString(), profilex, item1["start_date"].ToString(), item1["end_date"].ToString(), fields, item1["graph"].ToString(), item1["urlaction"].ToString(), item1["category"].ToString() ,movements,objects,locations,users};
                    widgetlist.Add(aux);
                }

                string getreports = Reportsdb.GetRowsReports(iduser, null);
                JArray reportsja = JsonConvert.DeserializeObject<JArray>(getreports);

                Dictionary<string, string> reports = new Dictionary<string, string>();
                List<int> undef = new List<int>();
                foreach (JObject item1 in reportsja)
                {
                    reports.Add(item1["_id"].ToString(), item1["name"].ToString());

                    if (item1["start_date"].ToString() == "Indefinida" || item1["end_date"].ToString() == "Indefinida")
                    {
                        undef.Add(0);
                    }
                    else
                    {
                        undef.Add(1);
                    }
                }

                String notifications = Notificationsdb.GetRows("CreatedTimeStamp");
                JArray notificationsja = new JArray();
                try
                {
                    notificationsja = JsonConvert.DeserializeObject<JArray>(notifications);
                    JArray partialnot = new JArray();
                    int count = 0;
                    foreach (JObject item in notificationsja.Reverse())
                    {
                        if (count < 8)
                        {
                            partialnot.Add(item);
                            count++;
                        }
                        else
                        {
                            break;
                        }

                    }
                    notificationsja = partialnot;
                }
                catch (Exception ex) { }
                JObject colors = design.getColors();
                ViewData["colors"] = colors;
                ViewData["reports"] = reports;
                ViewData["undef"] = undef;
                ViewData["Widgets"] = widgetlist;
                ViewData["Notifications"] = notificationsja;
                return View();
            }
            catch (Exception ex)
            {
                Dictionary<string, string> reports = new Dictionary<string, string>();
                List<int> undef = new List<int>();
                List<string[]> widgetlist = new List<string[]>();
                ViewData["reports"] = reports;
                ViewData["undef"] = undef;
                ViewData["Widgets"] = widgetlist;
                return View();
            }

        }
        */
        /// <summary>
        ///     New Dashboard Style
        /// </summary>
        /// <returns></returns>
        /// 
    
        public ActionResult Index()
        {
            JArray dashboards = new JArray();
            JArray widgets = new JArray();
            JArray dashboardList = new JArray();
            JObject myDashboard = new JObject();
            try
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }

                string iduser = Session["_id"].ToString();

                try
                {
                    dashboards = JsonConvert.DeserializeObject<JArray>(NewDashboard.GetAll("userId", iduser));
                }
                catch (Exception e) { Error.Log(e, "NewDashboard"); }

                // Create first dashboard
                if (dashboards.Count == 0)
                {
                    JObject newDash = new JObject();
                    newDash.Add("name", "Main Dashboard");
                    newDash.Add("selected", true);
                    newDash.Add("userId", iduser);
                    newDash.Add("_id", NewDashboard.SaveRow(newDash.ToString()));

                    dashboards.Add(newDash);
                }

                foreach(JObject dashboard in dashboards)
                {
                    JObject newElement = new JObject();
                    newElement.Add("id", dashboard.GetValue("_id").ToString());
                    newElement.Add("name", dashboard.GetValue("name").ToString());
                    dashboardList.Add(newElement);
                    
                    if (dashboard.GetValue("selected").ToString() == "True")
                    {
                        myDashboard = dashboard;
                    }
                }

                //Get the Dashboard Widgets
                try
                {
                    widgets = JsonConvert.DeserializeObject<JArray>(Widgets.GetAll("dashboard", myDashboard.GetValue("_id").ToString()));
                    myDashboard.Add("widgets", widgets);
                }
                catch (Exception e)
                {
                    Error.Log(e, "Getting Widgets");
                }

                ViewBag.dashboardList = dashboardList.ToString();
                ViewBag.myDashboard = myDashboard.ToString();

                string logomenu = design.getLogoMenu();
                ViewBag.logomenu = logomenu;
            }
            catch(Exception e)
            {
                Error.Log(e,"Getting Dashboard");
            }

            try
            {
                bool ok=false;
                if (Request.Cookies["semaphores"] != null)
                {
                   
                    Session["Semaphores"] = Request.Cookies["semaphores"].Value;

                }
                else
                {
                    this.Session["Semaphores"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    HttpCookie aCookiesem = new HttpCookie("semaphores");
                    aCookiesem.Value = Session["Semaphores"].ToString();
                    aCookiesem.Expires = DateTime.Now.AddDays(10);
                    Response.Cookies.Add(aCookiesem);
                    ok=true;
                }

                DateTime datenow = DateTime.Now;
                DateTime dateCreated = DateTime.ParseExact(Session["Semaphores"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                TimeSpan time = datenow - dateCreated;
                int diferences = time.Days;
               if (diferences >= 1 || ok==true)
                {
              //  String semaphores = semaphoredb.GetRows();

               // JArray semaphoresja = new JArray();
                string ressult=null;
              /*  try
                {
                   // DateTime fecha ;
                  //  DateTime.TryParse("01/01/1900 00:20:20", out fecha);
                   // semaphoresja = JsonConvert.DeserializeObject<JArray>(semaphores);
                   // try
                   // {
                     //  foreach (JObject sema in semaphoresja)
                       // {
                         //   DateTime.TryParse(sema["date"].ToString(),out fecha);
                            
                           // break;
                        ///}
                    //}
                    //catch { DateTime.TryParse("01/01/1900 00:20:20", out fecha); }
                  //  string ressult = semaphores.sendalerts(Session["_id"].ToString());

                  /*  if (fecha.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy"))
                    {
                        ressult="";
                    }
                    else
                    {
                        foreach (JObject sema in semaphoresja)
                        {
                            sema["date"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            semaphoredb.SaveRow(JsonConvert.SerializeObject(sema), sema["_id"].ToString());
                        }
                        
                    }*/
                
              

                   ressult = sendalerts();
                  if (ressult != null)
                  {
                      Session["Semaphores"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                      HttpCookie aCookie;
                      string cookieName;
                      cookieName = Request.Cookies["semaphores"].Name;
                      aCookie = new HttpCookie(cookieName);
                      aCookie.Value = Session["Semaphores"].ToString();
                    //  aCookie.Expires = DateTime.Now.AddDays(-1);
                      Response.Cookies.Add(aCookie);

                    /*  HttpCookie aCookiesem = new HttpCookie("semaphores");
                      aCookiesem.Value = Session["Semaphores"].ToString();
                      aCookiesem.Expires = DateTime.Now.AddDays(10);
                      Response.Cookies.Add(aCookiesem);*/
                  }

                }     

            }
            catch (Exception ex)
            {

            }
            return View();
        }

        /// <summary>
        ///     Save the dashboard
        /// </summary>
        /// <returns></returns>
        public string SaveDashboard(string dashboard)
        {
            return "";
        }

        /// <summary>
        ///     Get the dashboard selected
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetDashboard(string id)
        {
            return "";
        }

        /// <summary>
        ///     Save the widget
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        public string SaveWidget(string widget)
        {
            JObject widgetObject = new JObject();
            try
            {
                widgetObject = JsonConvert.DeserializeObject<JObject>(widget);
            }
            catch(Exception e)
            {
                Error.Log(e, "Converting a Widget");
            }
            string idSaved = "";
            try
            {
                string widgetId = widgetObject["_id"].ToString(); 
                widgetObject.Remove("_id");
                idSaved = Widgets.SaveRow(widgetObject.ToString(), widgetId);
            }
            catch (Exception e) 
            {
                Error.Log(e, "Saving a Widget");            
            }

            return idSaved;
        
        }

        /// <summary>
        ///     Saves position and size of the widget
        /// </summary>
        /// <param name="id"></param>
        /// <param name="attributes"></param>
        public void SaveWidgetAttributes(string id, string attributes)
        {
            if (id != "" && attributes != "")
            {
                try
                {
                    Widgets.UpdateRow("attrs", attributes, id);
                }
                catch (Exception e)
                {
                    Error.Log(e, "Trying to save Widget Attributes");
                }
            }
        }

        public void DeleteWidget(string id)
        {
            if (id != "")
            {
                try
                {
                    Widgets.DeleteRow(id);
                }
                catch (Exception e)
                {
                    Error.Log(e, "Trying to delete a Widget");
                }
            }
        }

        /// <summary>
        ///     Return all the sources required for the Dashboard Widgets
        /// </summary>
        /// <returns></returns>
        public string GetSources(string customSources = null)
        {
            //Sources
            MongoModel Users = new MongoModel("Users");
            MongoModel UserProfiles = new MongoModel("Profiles");
            MongoModel Locations = new MongoModel("Locations");
            MongoModel LocationProfiles = new MongoModel("LocationProfiles");
            MongoModel ReferenceObjects = new MongoModel("ReferenceObjects");
            MongoModel Movements = new MongoModel("MovementProfiles");
            MongoModel Categories = new MongoModel("Categories");
            MongoModel Lists = new MongoModel("Lists");

            JObject Sources = new JObject();
            string[] sourceList = { 
                    "Users", 
                    "LocationProfiles", 
                    "Locations",
                    "ReferenceObjects",
                    "Profiles",
                    "MovementProfiles"
                };

            if (customSources != null)
            {
                try
                {
                    sourceList = JsonConvert.DeserializeObject<string[]>(customSources);
                }
                catch(Exception e)
                {
                    Error.Log(e, "Trying deserialize an object");
                }
            }

            foreach (string source in sourceList)
            {
                try
                {
                    JArray rawSource = new JArray();
                    switch (source)
                    {
                        case "Users":
                            rawSource = JsonConvert.DeserializeObject<JArray>(Users.GetRowsAll());
                            break;
                        case "LocationProfiles":
                            rawSource = JsonConvert.DeserializeObject<JArray>(LocationProfiles.GetRowsAll());
                            break;
                        case "Locations":
                            rawSource = JsonConvert.DeserializeObject<JArray>(Locations.GetRowsAll());
                            break;
                        case "ReferenceObjects":
                            rawSource = JsonConvert.DeserializeObject<JArray>(ReferenceObjects.GetRowsAll());
                            break;
                        case "Profiles":
                            rawSource = JsonConvert.DeserializeObject<JArray>(UserProfiles.GetRowsAll());
                            break;
                        case "MovementProfiles":
                            rawSource = JsonConvert.DeserializeObject<JArray>(Movements.GetRowsAll());
                            break;
                        case "Categories":
                            rawSource = JsonConvert.DeserializeObject<JArray>(Categories.GetRowsAll());
                            break;
                        case "AssetType":
                            {
                                JArray tmpRawSource = JsonConvert.DeserializeObject<JArray>( Lists.Get("name", "_HTKassetsType"));
                                foreach (JObject tmpRow in tmpRawSource)
                                {
                                    foreach (JObject element in tmpRow["elements"]["unorder"])
                                    {
                                        foreach (KeyValuePair<string, JToken> token in element)
                                        {
                                            JObject value = new JObject();
                                            value.Add("_id", token.Key);
                                            value.Add("name", token.Value);
                                            rawSource.Add(value);
                                        }
                                    }
                                }
                                break;
                            }
                    }

                    JArray dataList = new JArray();

                    foreach (JObject rawObject in rawSource)
                    {
                        JObject newObject = new JObject();
                        newObject.Add("id", rawObject["_id"]);

                        if(source == "Users")
                            newObject.Add("value", rawObject["name"] + " " + rawObject["lastname"]);
                        else
                            newObject.Add("value", rawObject["name"]);
                        
                        dataList.Add(newObject);
                    }

                    Sources.Add(source, dataList);

                }
                catch (Exception e)
                {
                    Error.Log(e, "Trying to get "+ source +" Table");
                }
            }


            return Sources.ToString();
        }

        public string sendalerts()
        {

            try
            {
                String demands = demanddb.GetByStatus("status", 3);
                String alerts = alertsdb.GetRows();
                JArray alertsja = new JArray();
                String semaphores = semaphoredb.GetRows();

                JArray demandsja = new JArray();
                JArray semaphoresja = new JArray();
              
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
                    String array0 = demanddb.GetDemandsAuthorizefor();
                    var noapprovedList = JsonConvert.DeserializeObject<JArray>(array0);//(from mov in demandsja where mov["authorizations"].Children()["approved"].Contains("0") && (string)mov["status"] == "3" select new { id = (string)mov["_id"], iduser = (from xs in mov["authorizations"].Children() where (string)xs["approved"] == "0" select xs["id_user"].Value<string>()).ToList(), date = (string)mov["CreatedDate"], type = (string)mov["typemov"], namemov = (string)mov["nameMovement"], folio = (string)mov["folio"] }).ToList();
                    String array1 = demanddb.GetDemandsApprovefor();
                    var noapprovedList2 = JsonConvert.DeserializeObject<JArray>(array1); //(from mov in demandsja where mov["approval"].Children()["approved"].Contains("0") && (string)mov["status"] == "5" select new { id = (string)mov["_id"], iduser = (from xs in mov["approval"].Children() where (string)xs["approved"] == "0" select xs["id_user"].Value<string>()).ToList(), date = (string)mov["AuthorizedDate"], type = (string)mov["typemov"], namemov = (string)mov["nameMovement"], folio = (string)mov["folio"] }).ToList();
                    String array2 = demanddb.GetDemandsAdjudicatefor();
                    var noapprovedList3 = JsonConvert.DeserializeObject<JArray>(array2);// (from mov in demandsja where (string)mov["status"] == "1" select new { id = (string)mov["_id"], iduser = mov["adjudicating"], date = (string)mov["CreatedDate"], type = (string)mov["typemov"], namemov = (string)mov["nameMovement"], folio = (string)mov["folio"] }).ToList();
                    String array3 = demanddb.GetDemandsUpdatefor();
                    var noapprovedList4 = JsonConvert.DeserializeObject<JArray>(array3);
                    String array4 = demanddb.GetDemandsUpdateforContabilidad();
                    var noapprovedList5 = JsonConvert.DeserializeObject<JArray>(array4);


                    var semaphores1 = (from item in semaphoresja select new { color = (string)item["color"], days = (int)item["days"], type = (string)item["typeMovement"] }).ToList();
                   
                    var alertslist = (from alr in alertsja.Children() select new { user = (string)alr["to"], demand = (string)alr["demand"], date = (string)alr["LastmodDate"] }).ToList();
                     //  tasks = new Task<string>[noapprovedList.Count()];
                    //   List<string> listn = (from ap in noapprovedList join al in alertslist on ap.id equals al.demand select (string)al.demand).ToList();
                    // Dictionary<string, string> alertsusers = alertslist.ToDictionary(x => (string)x.user, x => (string)x.demand);

                    foreach (JObject item in noapprovedList)
                    {
                        DateTime datenow = DateTime.Now;
                        DateTime dateCreated;
                        if (item["type"].ToString() == "delete")
                            dateCreated = DateTime.ParseExact(item["LastmodDate"].ToString().Substring(0, 10), "dd/MM/yyyy", null);
                        else
                        {
                            dateCreated = DateTime.ParseExact(item["CreatedDate"].ToString().Substring(0, 10), "dd/MM/yyyy", null);
                        }
                        if (item["type"].ToString() == "movement" && (item["temporal"].ToString() == "True" || item["temporal"].ToString() == "true"))
                        {
                            item["type"] = "temporal";
                        }
                        TimeSpan time = datenow - dateCreated;
                        int diferences = time.Days;
                        var values = 0;
                        bool mailsend = false;
                        List<string> listusers = new List<string>();
                        foreach (var sem in semaphores1)
                        {
                            if (diferences >= sem.days && item["type"].ToString() == sem.type)
                            {
                                foreach (JObject userx in item["authorizations"])
                                {
                                  var results = alertslist.Where(x => x.user == userx["id_user"].ToString() && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                  bool upd = false;
                                  if (results.Count() > 0)
                                  {
                                      foreach (var obj in results)
                                      {
                                          DateTime datelast = DateTime.ParseExact(obj.date, "dd/MM/yyyy HH:mm:ss", null);

                                          TimeSpan time1 = datenow - datelast;
                                          int diferences2 = time1.Days;
                                          if (diferences2 >= 1)
                                          {
                                              upd = true;
                                          }
                                      }
                                  }
                                  if (results.Count() == 0 || upd)
                                  {
                                    if (userx["approved"].ToString() == "0") //results.Count()==0
                                    {
                                        try
                                        {
                                            JObject boss = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(userx["id_user"].ToString()));
                                            listusers.Add(boss["boss"].ToString());
                                        }
                                        catch { }
                                        listusers.Add(userx["id_user"].ToString());
                                        mailsend = true;
                                    }
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
                                string namemov = item["namemove"].ToString();
                                bool send = false;
                                foreach (string uid in listusers.Distinct())
                                {
                                    var results = alertslist.Where(x => x.user == uid && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                    bool upd = false;
                                  if (results.Count() > 0)
                                  {
                                      foreach (var obj in results)
                                      {
                                          DateTime datelast = DateTime.ParseExact(obj.date, "dd/MM/yyyy HH:mm:ss", null);

                                          TimeSpan time1 = datenow - datelast;
                                          int diferences2 = time1.Days;
                                          if (diferences2 >= 1)
                                          {
                                              upd = true;
                                          }
                                      }
                                  }
                                    
                                    if (results.Count() == 0 || upd)
                                    {
                                        recipients.Add(uid);
                                        send = true;
                                    }
                                }
                                if (send)
                                {
                                    string to = JsonConvert.SerializeObject(recipients);
                                    string attach = JsonConvert.SerializeObject(attachments);

                                    //  messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema");
                                    messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema");
                                    //  Task<string>[] tasksave = null;
                                    //  tasks[noapprovedList.IndexOf(item)] = Task.Factory.StartNew(() => messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema"));
                                    //   tasksave = new Task<string>[listusers.Distinct().Count()];
                                    int ind = 0;
                                    foreach (string destiny in listusers.Distinct())
                                    {
                                        JObject alertdata = new JObject();
                                        alertdata.Add("to", destiny);
                                        alertdata.Add("demand", item["_id"].ToString());
                                        alertdata.Add("from", "Sisteme");
                                        alertdata.Add("subject", "Urgente:Solicitud con folio #" + item["folio"].ToString() + ",esta pendiente de autorizar");
                                        alertdata.Add("msg", "La Solicitud de " + namemov + " con folio #" + item["folio"].ToString() + ",esta pendiente de autorizar,favor de Autorizarla lo antes posible!!");
                                        String alertjson = JsonConvert.SerializeObject(alertdata);
                                        alertsdb.SaveRow(alertjson);
                                        //   tasksave[ind] = Task.Factory.StartNew(() => alertsdb.SaveRow(alertjson));

                                    }
                                }
                               // Task.WaitAll(tasksave);
                            }
                            catch (Exception ex) { }
                        }



                    }
                    try
                    {
                        alertsja = JsonConvert.DeserializeObject<JArray>(alertsdb.GetRows());
                        alertslist = (from alr in alertsja.Children() select new { user = (string)alr["to"], demand = (string)alr["demand"], date = (string)alr["LastmodDate"] }).ToList();

                    }
                    catch (Exception ex)
                    {

                    }
                    //para visto bueno
                    foreach (JObject item in noapprovedList2)
                    {
                        DateTime datenow = DateTime.Now;
                        DateTime dateCreated;
                        if (item["type"].ToString() == "delete" || item["temporal"].ToString() == "True")
                            dateCreated = DateTime.ParseExact(item["LastmodDate"].ToString().Substring(0, 10), "dd/MM/yyyy", null);
                        else
                        {
                            dateCreated = DateTime.ParseExact(item["AuthorizedDate"].ToString().Substring(0, 10), "dd/MM/yyyy", null);
                        }
                        if (item["type"].ToString() == "movement" && (item["temporal"].ToString() == "True" || item["temporal"].ToString() == "true"))
                        {
                            item["type"] = "temporal";
                        }
                        TimeSpan time = datenow - dateCreated;
                        int diferences = time.Days;
                        var values = 0;
                        bool mailsend = false;
                        List<string> listusers = new List<string>();
                        foreach (var sem in semaphores1)
                        {
                            if (diferences >= sem.days && item["type"].ToString() == sem.type)
                            {

                                foreach (JObject userx in item["approval"])
                                {
                                    var results = alertslist.Where(x => x.user == userx["id_user"].ToString() && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                    bool upd = false;
                                    if (results.Count() > 0)
                                    {
                                        foreach (var obj in results)
                                        {
                                            DateTime datelast = DateTime.ParseExact(obj.date, "dd/MM/yyyy HH:mm:ss", null);

                                            TimeSpan time1 = datenow - datelast;
                                            int diferences2 = time1.Days;
                                            if (diferences2 >= 1)
                                            {
                                                upd = true;
                                            }
                                        }
                                    }
                                    if (results.Count()==0 || upd)
                                    {

                                        if (userx["approved"].ToString() == "0") //results.Count()==0
                                        {
                                            try
                                            {
                                                JObject boss = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(userx["id_user"].ToString()));
                                                listusers.Add(boss["boss"].ToString());
                                            }
                                            catch { }
                                            listusers.Add(userx["id_user"].ToString());
                                            mailsend = true;
                                        }

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
                                string namemov = item["namemove"].ToString();
                                bool send = false;
                                foreach (string uid in listusers.Distinct())
                                {
                                    var results = alertslist.Where(x => x.user == uid && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                    bool upd = false;
                                    if (results.Count() > 0)
                                    {
                                        foreach (var obj in results)
                                        {
                                            DateTime datelast = DateTime.ParseExact(obj.date, "dd/MM/yyyy HH:mm:ss", null);

                                            TimeSpan time1 = datenow - datelast;
                                            int diferences2 = time1.Days;
                                            if (diferences2 >= 1)
                                            {
                                                upd = true;
                                            }
                                        }
                                    }

                                    if (results.Count() == 0 || upd)
                                    {
                                        recipients.Add(uid);
                                        send = true;
                                    }
                                }

                                if (send)
                                {
                                    string to = JsonConvert.SerializeObject(recipients);
                                    string attach = JsonConvert.SerializeObject(attachments);
                                    messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de dar visto bueno", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de Visto Bueno,favor de dar Visto Bueno lo antes posible!!", attach, "Sistema");
                                    //  messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema");

                                    //     Task<string>[] tasksave = null;
                                    //  tasks[noapprovedList2.IndexOf(item)] = Task.Factory.StartNew(() => messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de dar visto bueno", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de Visto Bueno,favor de dar Visto Bueno lo antes posible!!", attach, "Sistema"));
                                    //  tasksave = new Task<string>[listusers.Distinct().Count()];
                                    int ind = 0;
                                    foreach (string destiny in listusers.Distinct())
                                    {
                                        JObject alertdata = new JObject();
                                        alertdata.Add("to", destiny);
                                        alertdata.Add("demand", item["_id"].ToString());
                                        alertdata.Add("from", "Sisteme");
                                        alertdata.Add("subject", "Urgente:Solicitud con folio #" + item["folio"].ToString() + ",esta pendiente de Visto Bueno");
                                        alertdata.Add("msg", "La Solicitud de " + namemov + " con folio #" + item["folio"].ToString() + ",esta pendiente de Visto Bueno,favor de dar Visto Bueno lo antes posible!!");
                                        String alertjson = JsonConvert.SerializeObject(alertdata);
                                        alertsdb.SaveRow(alertjson);

                                        //    tasksave[ind] = Task.Factory.StartNew(() => alertsdb.SaveRow(alertjson));

                                    }
                                    // Task.WaitAll(tasksave);
                                }
                            }
                            catch (Exception ex) { }
                        }

                    }
                    try
                    {
                        alertsja = JsonConvert.DeserializeObject<JArray>(alertsdb.GetRows());
                        alertslist = (from alr in alertsja.Children() select new { user = (string)alr["to"], demand = (string)alr["demand"], date = (string)alr["LastmodDate"] }).ToList();

                    }
                    catch (Exception ex)
                    {

                    }
                    //dictaminar
                    foreach (JObject item in noapprovedList3)
                    {
                        DateTime datenow = DateTime.Now;
                        DateTime dateCreated = DateTime.ParseExact(item["CreatedDate"].ToString().Substring(0, 10), "dd/MM/yyyy", null);
                        TimeSpan time = datenow - dateCreated;
                        int diferences = time.Days;
                        if (item["type"].ToString() == "movement" && (item["temporal"].ToString() == "True" || item["temporal"].ToString() == "true"))
                        {
                            item["type"] = "temporal";
                        }
                        var values = 0;
                        bool mailsend = false;
                        List<string> listusers = new List<string>();
                        foreach (var sem in semaphores1)
                        {
                            if (diferences >= sem.days && item["type"].ToString() == sem.type)
                            {
                              /*  var results = alertslist.Where(x => x.user == userx["id_user"].ToString() && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                if (results.Count() == 0)
                                {*/
                                try
                                {
                                    JObject boss = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(item["adjudicating"].ToString()));
                                    listusers.Add(boss["boss"].ToString());
                                }
                                catch { }
                                listusers.Add(item["adjudicating"].ToString());
                                mailsend = true;
                                 //  }

                            }


                        }

                        if (mailsend)
                        {
                            try
                            {
                                JArray recipients = new JArray();
                                JArray attachments = new JArray();
                                string namemov = item["namemove"].ToString();
                                bool send = false;
                                foreach (string uid in listusers.Distinct())
                                {
                                    var results = alertslist.Where(x => x.user == uid && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                    bool upd = false;
                                    if (results.Count() > 0)
                                    {
                                        foreach (var obj in results)
                                        {
                                            DateTime datelast = DateTime.ParseExact(obj.date, "dd/MM/yyyy HH:mm:ss", null);

                                            TimeSpan time1 = datenow - datelast;
                                            int diferences2 = time1.Days;
                                            if (diferences2 >= 1)
                                            {
                                                upd = true;
                                            }
                                        }
                                    }

                                    if (results.Count() == 0 || upd)
                                    {
                                        recipients.Add(uid);
                                        send = true;
                                    }
                                }
                                
                                
                                if (send)
                                {
                                    string to = JsonConvert.SerializeObject(recipients);
                                    string attach = JsonConvert.SerializeObject(attachments);
                                    messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de dictaminar", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de dictaminar,favor de dictaminar lo antes posible!!", attach, "Sistema");
                                    //  messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema");

                                    //   Task<string>[] tasksave = null;
                                    //    tasks[noapprovedList3.IndexOf(item)] = Task.Factory.StartNew(() => messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de dictaminar", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de dictaminar,favor de dictaminar lo antes posible!!", attach, "Sistema"));
                                    //   tasksave = new Task<string>[listusers.Distinct().Count()];
                                    int ind = 0;

                                    foreach (string destiny in listusers.Distinct())
                                    {
                                        var results = alertslist.Where(x => x.user == destiny && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                        if (results.Count() == 0)
                                        {
                                            JObject alertdata = new JObject();
                                            alertdata.Add("to", destiny);
                                            alertdata.Add("demand", item["_id"].ToString());
                                            alertdata.Add("from", "Sisteme");
                                            alertdata.Add("subject", "Urgente:Solicitud con folio #" + item["folio"].ToString() + ",esta pendiente de dictaminar");
                                            alertdata.Add("msg", "La Solicitud de " + namemov + " con folio #" + item["folio"].ToString() + ",esta pendiente de dictaminar,favor de dar dictaminar lo antes posible!!");
                                            String alertjson = JsonConvert.SerializeObject(alertdata);
                                            alertsdb.SaveRow(alertjson);
                                        }
                                        //  tasksave[ind] = Task.Factory.StartNew(() => alertsdb.SaveRow(alertjson));

                                    }
                                }
                              //  Task.WaitAll(tasksave);
                            }
                            catch (Exception ex) { }
                        }

                    }
                    try
                    {
                        alertsja = JsonConvert.DeserializeObject<JArray>(alertsdb.GetRows());
                        alertslist = (from alr in alertsja.Children() select new { user = (string)alr["to"], demand = (string)alr["demand"], date = (string)alr["LastmodDate"] }).ToList();

                    }
                    catch (Exception ex)
                    {

                    }
                    //actualizar
                    foreach (JObject item in noapprovedList4)
                    {
                        DateTime datenow = DateTime.Now;
                        DateTime dateCreated;
                        if (item["status"].ToString() == "2")
                        {
                            dateCreated = DateTime.ParseExact(item["dctDate"].ToString().Substring(0, 10), "dd/MM/yyyy", null);
                        }
                        else
                            dateCreated = DateTime.ParseExact(item["AuthorizedDate"].ToString().Substring(0, 10), "dd/MM/yyyy", null);
                        TimeSpan time = datenow - dateCreated;
                        if (item["type"].ToString() == "movement" && (item["temporal"].ToString() == "True" || item["temporal"].ToString() == "true"))
                        {
                            item["type"] = "temporal";
                        }
                        int diferences = time.Days;
                        var values = 0;
                        bool mailsend = false;
                        List<string> listusers = new List<string>();
                        foreach (var sem in semaphores1)
                        {
                            if (diferences >= sem.days && item["type"].ToString() == sem.type)
                            {

                                try
                                {
                                    JObject boss = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(item["Creator"].ToString()));
                                    listusers.Add(boss["boss"].ToString());
                                }
                                catch { }
                                listusers.Add(item["Creator"].ToString());
                                mailsend = true;

                            }


                        }

                        if (mailsend)
                        {
                            try
                            {
                                JArray recipients = new JArray();
                                JArray attachments = new JArray();
                                string namemov = item["namemove"].ToString();
                                bool send = false;
                                foreach (string uid in listusers.Distinct())
                                {
                                    var results = alertslist.Where(x => x.user == uid && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                    bool upd = false;
                                    if (results.Count() > 0)
                                    {
                                        foreach (var obj in results)
                                        {
                                            DateTime datelast = DateTime.ParseExact(obj.date, "dd/MM/yyyy HH:mm:ss", null);

                                            TimeSpan time1 = datenow - datelast;
                                            int diferences2 = time1.Days;
                                            if (diferences2 >= 1)
                                            {
                                                upd = true;
                                            }
                                        }
                                    }

                                    if (results.Count() == 0 || upd)
                                    {
                                        recipients.Add(uid);
                                        send = true;
                                    }
                                }
                                if (send)
                                {
                                    string to = JsonConvert.SerializeObject(recipients);
                                    string attach = JsonConvert.SerializeObject(attachments);
                                    messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de actualizar", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de actualizar,favor de actualizar lo antes posible!!", attach, "Sistema");
                                    //  messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema");

                                    //   Task<string>[] tasksave = null;
                                    // tasks[noapprovedList4.IndexOf(item)] = Task.Factory.StartNew(() => messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de actualizar", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de actualizar,favor de actualizar lo antes posible!!", attach, "Sistema"));
                                    // tasksave = new Task<string>[listusers.Distinct().Count()];
                                    int ind = 0;
                                    foreach (string destiny in listusers.Distinct())
                                    {
                                        JObject alertdata = new JObject();
                                        alertdata.Add("to", destiny);
                                        alertdata.Add("demand", item["_id"].ToString());
                                        alertdata.Add("from", "Sisteme");
                                        alertdata.Add("subject", "Urgente:Solicitud con folio #" + item["folio"].ToString() + ",esta pendiente de actualizar");
                                        alertdata.Add("msg", "La Solicitud de " + namemov + " con folio #" + item["folio"].ToString() + ",esta pendiente de actualizar,favor de dar actualizar lo antes posible!!");
                                        String alertjson = JsonConvert.SerializeObject(alertdata);
                                        alertsdb.SaveRow(alertjson);
                                        //  tasksave[ind] = Task.Factory.StartNew(() => alertsdb.SaveRow(alertjson));

                                    }
                                }
                                // Task.WaitAll(tasksave);
                            }
                            catch (Exception ex) { }
                        }

                    }
                    try
                    {
                        alertsja = JsonConvert.DeserializeObject<JArray>(alertsdb.GetRows());
                        alertslist = (from alr in alertsja.Children() select new { user = (string)alr["to"], demand = (string)alr["demand"], date = (string)alr["LastmodDate"] }).ToList();

                    }
                    catch (Exception ex)
                    {

                    }
                    //actualizar contabilidad
                    foreach (JObject item in noapprovedList5)
                    {
                        DateTime datenow = DateTime.Now;
                        DateTime dateCreated;
                        dateCreated = DateTime.ParseExact(item["LastmodDate"].ToString().Substring(0, 10), "dd/MM/yyyy", null);
                        TimeSpan time = datenow - dateCreated;
                        int diferences = time.Days;
                        if (item["type"].ToString() == "movement" && (item["temporal"].ToString() == "True" || item["temporal"].ToString() == "true"))
                        {
                            item["type"] = "temporal";
                        }
                        var values = 0;
                        bool mailsend = false;
                        List<string> listusers = new List<string>();
                        foreach (var sem in semaphores1)
                        {
                            if (diferences >= sem.days && item["type"].ToString() == sem.type)
                            {
                                foreach (JObject userx in item["contador"])
                                {
                                    //var results = alertslist.Where(x => x.user == userx["id_user"].ToString() && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                     var results = alertslist.Where(x => x.user == userx["id_user"].ToString() && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                     bool upd = false;
                                     if (results.Count() > 0)
                                     {
                                         foreach (var obj in results)
                                         {
                                             DateTime datelast = DateTime.ParseExact(obj.date, "dd/MM/yyyy HH:mm:ss", null);

                                             TimeSpan time1 = datenow - datelast;
                                             int diferences2 = time1.Days;
                                             if (diferences2 >= 1)
                                             {
                                                 upd = true;
                                             }
                                         }
                                     }
                                     if (results.Count() == 0 || upd)
                                     {

                                         if (userx["approved"].ToString() == "0") //results.Count()==0
                                         {
                                             try
                                             {
                                                 JObject boss = JsonConvert.DeserializeObject<JObject>(Userdb.GetRow(userx["id_user"].ToString()));
                                                 listusers.Add(boss["boss"].ToString());
                                             }
                                             catch { }
                                             listusers.Add(userx["id_user"].ToString());
                                             mailsend = true;
                                         }
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
                                string namemov = item["namemove"].ToString();
                                bool send = false;
                                foreach (string uid in listusers.Distinct())
                                {
                                    var results = alertslist.Where(x => x.user == uid && x.demand == item["_id"].ToString()).Select(x => x).ToList();
                                    bool upd = false;
                                    if (results.Count() > 0)
                                    {
                                        foreach (var obj in results)
                                        {
                                            DateTime datelast = DateTime.ParseExact(obj.date, "dd/MM/yyyy HH:mm:ss", null);

                                            TimeSpan time1 = datenow - datelast;
                                            int diferences2 = time1.Days;
                                            if (diferences2 >= 1)
                                            {
                                                upd = true;
                                            }
                                        }
                                    }

                                    if (results.Count() == 0 || upd)
                                    {
                                        recipients.Add(uid);
                                        send = true;
                                    }
                                }
                                if (send)
                                {
                                    string to = JsonConvert.SerializeObject(recipients);
                                    string attach = JsonConvert.SerializeObject(attachments);
                                    messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de actualizar", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de actualizar,favor de actualizar lo antes posible!!", attach, "Sistema");
                                    //  messagesC.SendMail(to, "Urgente :Solicitud Pendiente de Autorizar", "La Solicitud de " + namemov + " con Folio #" + item.folio + ",esta Pendiente de Autorizar,favor de Autorizarla lo antes posible!!", attach, "Sistema");

                                    //  Task<string>[] tasksave = null;
                                    //  tasks[noapprovedList5.IndexOf(item)] = Task.Factory.StartNew(() => messagesC.SendMail(to, "Urgente :Solicitud con folio #" + item["folio"].ToString() + ", Pendiente de actualizar", "La Solicitud de " + namemov + " con Folio #" + item["folio"].ToString() + ",esta Pendiente de actualizar,favor de actualizar lo antes posible!!", attach, "Sistema"));
                                    //  tasksave = new Task<string>[listusers.Distinct().Count()];
                                    int ind = 0;
                                    foreach (string destiny in listusers.Distinct())
                                    {
                                        JObject alertdata = new JObject();
                                        alertdata.Add("to", destiny);
                                        alertdata.Add("demand", item["_id"].ToString());
                                        alertdata.Add("from", "Sisteme");
                                        alertdata.Add("subject", "Urgente:Solicitud con folio #" + item["folio"].ToString() + ",esta pendiente de actualizar");
                                        alertdata.Add("msg", "La Solicitud de " + namemov + " con folio #" + item["folio"].ToString() + ",esta pendiente de actualizar,favor de dar actualizar lo antes posible!!");
                                        String alertjson = JsonConvert.SerializeObject(alertdata);
                                        alertsdb.SaveRow(alertjson);
                                        //  tasksave[ind] = Task.Factory.StartNew(() => alertsdb.SaveRow(alertjson));

                                    }
                                }
                                //Task.WaitAll(tasksave);
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
    }
}