using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RivkaAreas.Monitor.Models;
using System.Drawing;
using System.IO;
using MongoDB.Bson;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Security;
using Rivka.Images;

using Rivka.Form;
using Rivka.Form.Field;
namespace RivkaAreas.Monitor.Controllers
{
    public class MonitorController : Controller
    {
        //
        // GET: /Monitor/Monitor/
        LocationTable locationTable;
        ProcessesTable _processesTable;
        DiagramTable _diagramTable;
        CircuitTable _circuitTable;
        UserTable _usertable;
        ObjectRealMonitorTable _monitorTable;

        public MonitorController()
        {
            locationTable = new LocationTable();
           // _fieldsTable = new CustomFieldsTable("LocationCustomFields");
            _processesTable = new ProcessesTable();
            _diagramTable = new DiagramTable();
            _circuitTable = new CircuitTable();
            _usertable = new UserTable();
            _monitorTable = new ObjectRealMonitorTable();
        }

        public ActionResult Index()
        {
            bool access = getpermissions("location", "r");
            if (access == true)
            {
                string rowArray = locationTable.Get("tipo","2");
              //  string rowArray = locationTable.GetRows();
                JArray locat = JsonConvert.DeserializeObject<JArray>(rowArray);
                Dictionary<string, string> data = new Dictionary<string, string>();

                foreach (JObject items in locat)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString());
                }

                ViewData["locations"] = data;

                rowArray = _processesTable.GetRows();
                locat = JsonConvert.DeserializeObject<JArray>(rowArray);
                data = new Dictionary<string, string>();

                foreach (JObject items in locat)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString());
                }

                ViewData["processes"] = data;


                rowArray = _diagramTable.GetRows(); 
                locat = JsonConvert.DeserializeObject<JArray>(rowArray);
                data = new Dictionary<string, string>();

                foreach (JObject items in locat)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString());
                }

                ViewData["diagrams"] = data;

                return View();
            }
            else
            {

                return Redirect("~/Home");
            }
        }

        public bool getpermissions(string permission, string type)
        {
            var datos = "";
            if (Session["Permissions"] != null)
            {
                datos = Session["Permissions"].ToString();
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
            else
            {

                this.Session["LoggedUser"] = null;
                this.Session["_id"] = null;

                //Unset auth cookie
                FormsAuthentication.SignOut();


                //Redirect to index
                Response.Redirect("~/User/Login");
                return false;
            }
        }

        public JsonResult getDiagrambyLocation(string id_location)
        {
            string diagramResult = _circuitTable.Get("locationid", id_location);

            if (diagramResult != null)
            {
                return Json(diagramResult);
            }

            return Json("");
        }

        public JsonResult getDiagrambyLocationControl(string id_location)
        {
            string diagramResult = _circuitTable.GetRows();
            JArray arraycontrol = JsonConvert.DeserializeObject<JArray>(diagramResult);
            string idcircuito = "";

            foreach (JObject obj in arraycontrol)
            {
                JArray processarray = JsonConvert.DeserializeObject<JArray>(obj["processes"].ToString());
                foreach (JObject obj1 in processarray)
                {
                    if (obj1["processId"].ToString() == id_location) {
                        idcircuito = obj["locationid"].ToString();
                    }
                
                }
            }

            if (idcircuito == "")
            {
                diagramResult = locationTable.GetRow(id_location);
                JObject control = JsonConvert.DeserializeObject<JObject>(diagramResult);
                control["locationid"] = control["parent"].ToString();
                arraycontrol = new JArray();

                arraycontrol.Add(control);

                diagramResult = JsonConvert.SerializeObject(arraycontrol);
            }
            else {
                diagramResult = _circuitTable.Get("locationid", idcircuito);
            }
            

            if (diagramResult != null)
            {
                return Json(diagramResult);
            }

            return Json("");
        }

        public JsonResult getNodesDetails(string nodeid)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                String location = locationTable.GetRow(nodeid); //getting all the users

                if (location != null)
                {
                    JObject objs = JsonConvert.DeserializeObject<JObject>(location);


                    //the next is the photo's information
                    try
                    {
                        string relativepath = "\\Uploads\\Images\\planos\\";
                        string imageExt = objs["imgext"].ToString();
                        string absolutepathdir = Server.MapPath(relativepath);
                        string filename = objs["_id"].ToString() + "." + imageExt;
                        string fileabsolutepath = absolutepathdir + filename;

                        if (imageExt != "" && System.IO.File.Exists(fileabsolutepath))
                        {
                            string url = Url.Content(relativepath + filename);
                            objs.Add("ImgUrl", url); //adding the image's url to the document
                        }
                    }
                    catch (Exception ex) { }

                    return Json(JsonConvert.SerializeObject(objs));
                }
                //
                return null;
                //return new HtmlString(locationsTrs);

            }
            else
            {
                return null;
            }
        }

        public ActionResult GetDiagrams()
        {
            if (this.Request.IsAjaxRequest())
            {
                string diagrams = _diagramTable.GetRows();
                JArray diagramList = null;
                try
                {
                    diagramList = JsonConvert.DeserializeObject<JArray>(diagrams);
                }
                catch (Exception e) { }

                ViewBag.diagramList = diagramList;

                return View();
            }
            else
                return this.Redirect("/");
        }

        public JsonResult getDiagram(string id_diagram)
        {
            string diagramResult = _diagramTable.GetRow(id_diagram);

            if (diagramResult != null)
            {
                return Json(diagramResult);
            }

            return Json("");
        }

        public JsonResult getUserTable()
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                string userid = Session["_id"].ToString();
                List<BsonDocument> docs = _usertable.getRows(); //getting all the users
                List<BsonDocument> docum = new List<BsonDocument>();
                foreach (BsonDocument document in docs)
                {
                    try
                    {
                        if (document.GetElement("imgext").Value != "")
                            document.Set("image", "/Uploads/Images/" + document.GetElement("_id").Value + "." + document.GetElement("imgext").Value);
                    }
                    catch (Exception e) { }
                    try
                    {
                        document.Remove("profileFields");
                    }
                    catch (Exception e) { }

                    try
                    { //trying to set the creator's name
                        BsonDocument creator = _usertable.getRow(document.GetElement("creatorId").Value.ToString());
                        //document.Set("Creator", "YO");//
                        document.Set("Creator", creator.GetElement("name").Value);
                    }
                    catch (Exception e)
                    {
                        document["Creator"] = "";
                    }
                    try
                    {
                        document.Remove("creatorId");
                    }
                    catch (Exception e) { }
                    try
                    {
                        document.Remove("pwd");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    if (document["user"].ToString() == "admin")
                    {
                        if (document["_id"].ToString() == userid) docum.Add(document);
                    }
                    else
                    {
                        docum.Add(document);
                    }
                }
                JObject result = new JObject();
                result.Add("users", docum.ToJson());
                return Json(JsonConvert.SerializeObject(result));
            }
            else
            {
                return null;
            }
        }

        public JsonResult getMonitorTable(string locationid)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                //String rowArray = _demandTable.GetRows();

                string newJoin = _monitorTable.Get("location_id",locationid);

                String rows = "";
                JArray objects = JsonConvert.DeserializeObject<JArray>(newJoin);
                
                rows = JsonConvert.SerializeObject(objects);
                return Json(rows);

            }
            return null;
        }
    }
}
