using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RivkaAreas.Locations.Models;
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

namespace RivkaAreas.Locations.Controllers
{
    public class CircuitController : Controller
    {
        //
        // GET: /Locations/Circuit/
        private HardwareTable HardwareModel;
        private LocationTable locationTable;
        LocationProfileTable profileTable;
        private CircuitTable circuitTable;
        private CircuitLocationTable circuitLocationTable;

        public CircuitController()
        {
            profileTable = new LocationProfileTable();
            HardwareModel = new HardwareTable();
            locationTable = new LocationTable();
            circuitTable = new CircuitTable();
            circuitLocationTable = new CircuitLocationTable();
        }

        public ActionResult Index()
        {
            bool access = getpermissions("circuits", "r");
            if (access == true)
            {
              //  string processes = 
                string hardware = HardwareModel.GetRows();
                String locationsArray = locationTable.GetRows();
                JArray processesArray = new JArray();
                JArray hardwareArray = JsonConvert.DeserializeObject<JArray>(hardware);
                JArray Locations = JsonConvert.DeserializeObject<JArray>(locationsArray);

                ViewBag.Locations = Locations;
                ViewBag.Processes = processesArray;
                ViewBag.Hardware = hardwareArray;
                return View();
            }
            else
            {
                return Redirect("~/Home");
            }
        }

        public JsonResult getNodes(string parentCategory)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                if (parentCategory == null || parentCategory == "") parentCategory = "null";
                String docs = locationTable.Get("parent", parentCategory); //getting all the users
                JArray objs = JsonConvert.DeserializeObject<JArray>(docs);

                return Json(JsonConvert.SerializeObject(objs));

            }
            else
            {
                return null;
            }
        }

        public JsonResult getRoute(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {

                String actualCategory = locationTable.GetRow(parentCategory);
                JObject actualCatObject = JsonConvert.DeserializeObject<JObject>(actualCategory);

                JObject categoryObject = new JObject();
                categoryObject.Add("id", actualCatObject["_id"].ToString());
                route.Add(categoryObject);
                parentCategory = actualCatObject["parent"].ToString();
            }

            JObject result = new JObject();
            result.Add("route", route);
            return Json(JsonConvert.SerializeObject(result));
        }

        public JsonResult getNodesDetails(string nodeid)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                String location = locationTable.GetRow(nodeid); //getting all the users
                
                if (location != null)
                {
                    JObject objs = JsonConvert.DeserializeObject<JObject>(location);
                    string nameprofile = "";
                    string mapa = "";
                    string plano = "";
                    if (objs["profileId"].ToString() != "null")
                    {
                        nameprofile = profileTable.getRow((String)objs["profileId"]).GetElement("name").Value.ToString();
                        mapa = profileTable.getRow((String)objs["profileId"]).GetElement("mapa").Value.ToString();
                        plano = profileTable.getRow((String)objs["profileId"]).GetElement("plano").Value.ToString();

                    }
                    objs.Add("nameprofile", nameprofile);
                    objs.Add("mapa", mapa);
                    objs.Add("plano", plano);

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


                    //JObject cad = JsonConvert.DeserializeObject<JObject>(objs["profileFields"].ToString());
                    //string prof = "{";
                    //foreach (KeyValuePair<string, JToken> token in cad)
                    //{
                    //    String fieldsArray = _fieldsTable.Get("name", token.Key.Replace("_HTKField", ""));
                    //    if (fieldsArray == "[]") continue;
                    //    JArray fieldList = JsonConvert.DeserializeObject<JArray>(fieldsArray);
                    //    string etiqueta = "";
                    //    foreach (JObject obj in fieldList)
                    //    {
                    //        etiqueta = obj["label"].ToString();
                    //    }
                    //    if (prof != "{") prof = prof + ",";
                    //    prof = prof + "\"" + etiqueta + "\":\"" + token.Value + "\"";
                    //}
                    //prof += "}";

                    //JObject ja = JsonConvert.DeserializeObject<JObject>(prof);
              

                    //objs["profileFields"] = ja;

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

        public String SaveDiagram(string Scenario, string id_diagram)
        {
             JObject scenario = JsonConvert.DeserializeObject<JObject>(Scenario);

             JArray obj1 = JsonConvert.DeserializeObject<JArray>(scenario["connections"].ToString());
            foreach(JObject o1 in obj1){
                JObject obj2 = JsonConvert.DeserializeObject<JObject>(o1["shape"].ToString());
                obj2.Remove("vertices");
                if (CircuitConectionExists(obj2["source"].ToString(), obj2["target"].ToString()) == "false")
                {
                     SaveCircuitLocation(JsonConvert.SerializeObject(obj2),"");
                }
               
            }
            id_diagram = (id_diagram != "") ? id_diagram : null;

            id_diagram = circuitTable.SaveRow(Scenario, id_diagram);
            return id_diagram;

        }

        //guardar circuitlocation
        public String SaveCircuitLocation(string datos, string idnodo)
        {
            JObject obj = JsonConvert.DeserializeObject<JObject>(datos);

            idnodo = (idnodo != "") ? idnodo : null;

            idnodo = circuitLocationTable.SaveRow(datos, idnodo);
            return idnodo;

        }

        public JsonResult getDiagram(string id_diagram)
        {
            string diagramResult = circuitTable.GetRow(id_diagram);

            if (diagramResult != null)
            {
                return Json(diagramResult);
            }

            return Json("");
        }

        public JsonResult getDiagrambyLocation(string id_location)
        {
            string diagramResult = circuitTable.Get("locationid", id_location);

            if (diagramResult != null)
            {
                return Json(diagramResult);
            }

            return Json("");
        }

        public string CircuitConectionExists(string source, string target){
            string result="false";
            string diagramResult = circuitLocationTable.Get("target", target);

            JArray obj1 = JsonConvert.DeserializeObject<JArray>(diagramResult);
            foreach(JObject o1 in obj1){
                if(source==o1["source"].ToString()){
                    result="true";
                }
                
            }

            return result;
        }

        public JsonResult getCircuits()
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                String rowArray = circuitTable.GetRows();
                JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);
                //JArray objects1 = new JArray();
                //JObject objs, pobjs;
                //string rowString = "", prowString = "";
                //foreach (JObject obj in objects)
                //{
                //    rowString = _profileTable.GetRow(obj["movement"].ToString());
                //    if (rowString == null || rowString == "") continue;
                //    objs = JsonConvert.DeserializeObject<JObject>(rowString);

                //    prowString = _processesTable.GetRow(objs["processes"].ToString());
                //    if (prowString == null || prowString == "") continue;
                //    pobjs = JsonConvert.DeserializeObject<JObject>(prowString);

                //    obj["movement"] = pobjs["name"].ToString() + " " + objs["name"].ToString();
                //    obj.Add("typeMovement", objs["typeMovement"].ToString());

                //    if (objs["typeMovement"].ToString() == "create")
                //    {
                //        rowString = _objectReferenceTable.getRow(obj["object"].ToString());
                //        if (rowString != null)
                //        {
                //            objs = JsonConvert.DeserializeObject<JObject>(rowString);
                //            obj["object"] = objs["name"].ToString();
                //        }
                //        else obj["object"] = "";


                //    }
                //    else
                //    {
                //        rowString = _objectTable.GetRow(obj["object"].ToString());
                //        if (rowString != null)
                //        {
                //            objs = JsonConvert.DeserializeObject<JObject>(rowString);
                //            obj["object"] = objs["name"].ToString();
                //        }
                //        else obj["object"] = "";
                //    }



                //    rowString = _userTable.GetRow(obj["Creator"].ToString());
                //    if (rowString != null)
                //    {
                //        objs = JsonConvert.DeserializeObject<JObject>(rowString);
                //        obj["Creator"] = objs["user"].ToString();
                //    }

                //    objects1.Add(obj);
                //}

                return Json(JsonConvert.SerializeObject(objects));

            }
            return null;
        }

    }
}
