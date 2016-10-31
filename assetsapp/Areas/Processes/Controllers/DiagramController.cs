using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RivkaAreas.Processes.Models;
using Rivka.Db;
using Rivka.Security;
namespace RivkaAreas.Processes.Controllers
{
    public class DiagramController : Controller
    {
        private DiagramTable DiagramModel;
        private ProcessesTable ProcessModel;
        private HardwareTable HardwareModel;
        private FlowTable FlowModel;
        private HardwareReferenceTable hardwareReferenceTable;
        protected validatePermissions validatepermissions;

        /// <summary>
        /// Initializes the model required
        /// </summary>
        public DiagramController()
        {
            validatepermissions = new validatePermissions();
            DiagramModel = new DiagramTable();
            ProcessModel = new ProcessesTable();
            HardwareModel = new HardwareTable();
            FlowModel = new FlowTable();
            hardwareReferenceTable = new HardwareReferenceTable();
        }

        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("processes", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("processes", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                string processes = ProcessModel.GetRows();
                string hardware = HardwareModel.GetRows();

                JArray processesArray = JsonConvert.DeserializeObject<JArray>(processes);
                JArray hardwareArray = JsonConvert.DeserializeObject<JArray>(hardware);
                foreach (JObject document in hardwareArray)
                {
                    try
                    { //trying to set the creator's name
                        String rowString = hardwareReferenceTable.GetRow(document["hardware_reference"].ToString());
                        JObject rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
                        document["name"] = rowArray["name"];
                    }
                    catch (Exception e)
                    {
                        document["name"] = "";
                    }
                }
                ViewBag.Processes = processesArray;
                ViewBag.Hardware = hardwareArray;

                return View();
            }
            else
            {
                return Redirect("~/Home");
            }
        }

        /// <summary>
        ///     Saves a complete processes scenario
        /// </summary>
        /// <param name="Scenario">
        ///     A json String with the entire scenario
        /// </param>
        /// <author>
        ///     Juan Bautista
        /// </author>
        public JsonResult SaveDiagram(string Scenario, string id_diagram)
        {
            JObject scenario = JsonConvert.DeserializeObject<JObject>(Scenario);
            string result = "{";
            id_diagram = ( id_diagram != "" ) ? id_diagram : null;

            //Diagram name is required
            if (scenario["name"].ToString() != "")
            {
                id_diagram = DiagramModel.SaveRow(Scenario, id_diagram);
                result += "\"status\": \"success\", \"id\": \"" + id_diagram + "\"";
                SaveDiagramRules(scenario);
            }
            else
            {
                result += "\"status\": \"error\", \"message\": \"Nombre del Diagrama es requerido\" ";
            }

            result += "}";


            return Json(result);

        }

        /// <summary>
        ///     Saves the Flow rules, from the processes diagram
        /// </summary>
        /// <param name="Diagram"></param>
        private void SaveDiagramRules(JObject Diagram)
        {
            JArray Processes = (JArray)Diagram["processes"];
            JArray Connections = (JArray)Diagram["connections"];
            JArray Rules = new JArray();
            String jsonData = "";
            String flowstring = "";
            JObject flowobj = new JObject();
            String id = "null";
            String origen = "";
            JArray objarray = new JArray();
            Dictionary<string, string> decisions = new Dictionary<string, string>();

            foreach(JObject obj in Connections){
                if (obj["shape"]["source"].ToString().Length < 24) {
                    if (!decisions.ContainsKey(obj["shape"]["source"].ToString()))
                    {
                        decisions.Add(obj["shape"]["source"].ToString(), "");
                    }
                    
                }
                if (obj["shape"]["target"].ToString().Length < 24) {
                    if (decisions.ContainsKey(obj["shape"]["target"].ToString()))
                    {
                        decisions[obj["shape"]["target"].ToString()] = obj["shape"]["source"].ToString();
                    }
                    else {
                        decisions.Add(obj["shape"]["target"].ToString(), obj["shape"]["source"].ToString());
                    }
                }
            }

            foreach(JObject Process in Processes)
            {
                JArray NewRule = new JArray();
                string processId = Process["processId"].ToString();
                foreach (JObject Rule in Connections)
                {
                    if (Rule["shape"]["target"].ToString() == Process["processId"].ToString() && Process["processId"].ToString().Length==24)
                    {
                        if (Rule["shape"]["source"].ToString().Length < 24)
                        {
                            origen = decisions[Rule["shape"]["source"].ToString()];
                        }
                        else
                        {
                            origen = Rule["shape"]["source"].ToString();
                        }

                        flowstring = FlowModel.Get("source", origen);
                        if (flowstring != null && flowstring!="[]")
                        {
                            objarray = JsonConvert.DeserializeObject<JArray>(flowstring);
                            foreach(JObject flow in objarray){
                                id = flow["_id"].ToString();
                                FlowModel.UpdateTargets(id, Rule["shape"]["target"].ToString());
                            }
                            
                        }
                        else {
                            jsonData = "{'source':'" + origen + "', targets:['" + Rule["shape"]["target"].ToString() + "']}";

                            FlowModel.SaveRow(jsonData);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the diagram list
        /// </summary>
        /// <returns>
        ///     A view with the diagram list with  CRUD options
        /// </returns>
        public ActionResult GetDiagrams()
        {
            if (this.Request.IsAjaxRequest())
            {
                string diagrams = DiagramModel.GetRows();
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

        public void DeleteDiagram(string id_diagram)
        {
            if (this.Request.IsAjaxRequest())
            {
                try
                {
                    DiagramModel.DeleteRow(id_diagram);
                }
                catch(Exception e){}
            }
        }

        /// <summary>
        ///     Gets a process diagram by its id
        /// </summary>
        /// <param name="id_diagram"></param>
        /// <returns>
        ///     A json result with the process diagram
        /// </returns>
        /// <author>
        ///     Juan Bautista Celaya
        /// </author>
        public JsonResult getDiagram(string id_diagram)
        {
            string diagramResult = DiagramModel.GetRow(id_diagram);

            if (diagramResult != null)
            {
                return Json(diagramResult);
            }

            return Json("");
        }

        public String diagramExists(string name) {
            if (name != null && name != "") {
                String result = DiagramModel.Get("name", name);
                if ( result != null && result != "[]") {
                    return "true";
                } 
            }

            return "false";
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

    }
}
