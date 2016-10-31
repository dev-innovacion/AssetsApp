using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RivkaAreas.Processes.Models;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using Newtonsoft.Json;
using Rivka.Security;
using Rivka.Form;

namespace RivkaAreas.Processes.Controllers
{
    public class ProcessController : Controller
    {
        //
        // GET: /Processes/Process/
        private ProcessesTable processesTable;
        private MovementTable movementTable;
        private LocationTable locationTable;
        private AuthorizationTable authorizationTable;
        private HardwareTable hardwareTable;
        private RulesTable rulesTable;
        private FlowTable flowTable;
        private HardwareReferenceTable hardwareReferenceTable;
        protected validatePermissions validatepermissions;

        public ProcessController()
        {
            processesTable = new ProcessesTable();
            movementTable = new MovementTable();
            authorizationTable = new AuthorizationTable();
            locationTable = new LocationTable();
            hardwareTable = new HardwareTable();
            rulesTable = new RulesTable();
            hardwareReferenceTable = new HardwareReferenceTable();
            validatepermissions = new validatePermissions();
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
                //string processes = ProcessModel.GetRows();
                string hardware = hardwareTable.GetRows();

                //JArray processesArray = JsonConvert.DeserializeObject<JArray>(processes);
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
                //ViewBag.Processes = processesArray;
                ViewBag.Hardware = hardwareArray;
                return View();
            }
            else
            {
                return Redirect("~/Home");
            }
        }

        /// <summary>
        ///     This method allows to get the information of an especific process
        /// </summary>
        /// <param name="id_process"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns Json with the information
        /// </returns>
        
        public JsonResult getProcess(string id_process)
        {
            String processString = processesTable.GetRow(id_process);
            return Json(processString);
        }

        /// <summary>
        ///     This method allows to get the rules of an especific process
        /// </summary>
        /// <param name="id_process"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns Json with the information
        /// </returns>

        public JsonResult getRules(string id_process)
        {
            try
            {
                String processString = processesTable.GetRow(id_process);
                JObject process = JsonConvert.DeserializeObject<JObject>(processString);

                JToken rules = process["rules"];
                String rulesString = JsonConvert.SerializeObject(rules);

                JArray rulesArray = JsonConvert.DeserializeObject<JArray>(rulesString);

                foreach (JObject rule in rulesArray){

                    JToken locations = rule["Locations"];
                    String nameLocation = "";
                    foreach(String location in locations){
                        String locationString = locationTable.GetRow(location);
                        JObject locationRow = JsonConvert.DeserializeObject<JObject>(locationString);

                        nameLocation += locationRow["name"] + ",";

                    }
                    rule.Add("nameLocations", nameLocation);
                }

                return Json(JsonConvert.SerializeObject(rulesArray));

            }
            catch (Exception e) {
            return null;
        }
        }

        public JsonResult getLocations(string id_process)
        {
            try
            {
                String processString = processesTable.GetRow(id_process);
                JObject process = JsonConvert.DeserializeObject<JObject>(processString);

                String rulesString = JsonConvert.SerializeObject(process["locations"]);

                JArray rulesArray = JsonConvert.DeserializeObject<JArray>(rulesString);
                JArray locarray = new JArray();
                foreach (string rule in rulesArray)
                {
                    String nameLocation = "";

                    String locationString = locationTable.GetRow(rule);
                    JObject locationRow = JsonConvert.DeserializeObject<JObject>(locationString);

                    nameLocation = locationRow["name"].ToString();

                    JObject obj = new JObject(); 
                    obj.Add("id",rule);
                    obj.Add("nameLocation", nameLocation);

                    locarray.Add(obj);
                }

                return Json(JsonConvert.SerializeObject(locarray));

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String saveProcessFull(String name, String status, String min_length, String min_length_type, String max_length, String max_length_type, String type, String processId)
        {
            if (processId != null)
            {
                String processString = processesTable.GetRow(processId);
                if (processString != null)
                {
                    JObject newRow = new JObject();
                    newRow["name"] = name;
                    newRow["status"] = status;
                    newRow["type"] = type;

                    JObject minduration = new JObject();
                    minduration["duration"] = min_length;
                    minduration["type"] = min_length_type;
                    newRow["min_duration"] = minduration;

                    JObject maxduration = new JObject();
                    maxduration["duration"] = max_length;
                    maxduration["type"] = max_length_type;
                    newRow["max_duration"] = maxduration;

                    processString = JsonConvert.SerializeObject(newRow);
                    processesTable.SaveRow(processString, processId);
                    return processId;
                }
            }
            return null;
        }

        /// <summary>
        ///     Saves the rules of a process
        /// </summary>
        /// <param name="rulesString">
        ///     A string containing the rules data
        /// </param>
        /// <param name="id_process">
        ///     The id of the process
        /// </param>
        /// <returns>
        ///     A string with the status result
        /// </returns>
        /// <author>
        ///     Abigail Rodriguez 
        /// </author>
        public String SaveRules(string rulesString, string locationsString, string id_process = null)
        {
            JArray rulesArray = JsonConvert.DeserializeObject<JArray>("["+rulesString+"]");
            JArray locatarray = new JArray(); 
            foreach (JObject rule in rulesArray) {
                String ruleRow = "{'process': '" + id_process + "'," +
                "'Hardware' : '" + rule["Hardware"] + "'," +
                "'Locations' :" + rule["Locations"] +
                "}";

                locatarray = JsonConvert.DeserializeObject<JArray>(rule["Locations"].ToString());
                foreach (String cad in locatarray) {
                    AddProcessToLocation(cad, id_process);
                }

                String idRule = rulesTable.SaveRow(ruleRow, rule["ruleId"].ToString());
                rule.Add("rule", idRule);
                rule.Remove("ruleId");
            }

            String processString = processesTable.GetRow(id_process);
            JObject process = JsonConvert.DeserializeObject<JObject>(processString);


            try
            {
                //Check if a rule was delete
                JToken oldRules = process["rules"];
                String oldRulesString = JsonConvert.SerializeObject(oldRules);
                JArray oldRule = JsonConvert.DeserializeObject<JArray>(oldRulesString);
                //Get equals (rules)
                foreach (JObject newrule in rulesArray)
                {
                    foreach (JObject oldrule in oldRule)
                    {
                        if (oldrule["rule"] != null)
                        {
                            if (newrule["rule"].ToString() == oldrule["rule"].ToString())
                            {
                                oldrule.Remove("rule");
                            }
                        }
                    }
                }
                //Delete rules left
                foreach (JObject oldrule in oldRule)
                {
                    if (oldrule["rule"] != null)
                    {
                        rulesTable.DeleteRowPhysical(oldrule["rule"].ToString());
                    }
                }
            }
            catch (Exception e) { }


            process["rules"] = rulesArray;

            if (locationsString != "") {
                JArray locs = JsonConvert.DeserializeObject<JArray>(locationsString);
                process["locations"] = locs;

                foreach (String cad in locs)
                {
                    AddProcessToLocation(cad, id_process);
                }
            }

            processesTable.SaveRow(process.ToString(),id_process);
            
            return null;
        }

        /// <summary>
        ///     Saves a new process
        /// </summary>
        /// <param name="process">
        ///     A json string containing the process data
        /// </param>
        /// <param name="id_process">
        ///     The id of the process
        /// </param>
        /// <returns>
        ///     A json string with the status result and the saved id
        /// </returns>
        /// <author>
        ///     Abigail Rodriguez 
        /// </author>
        public JsonResult SaveProcess(string process, string id_process = null)
        {
            JObject processObject = JsonConvert.DeserializeObject<JObject>(process);

            //Dictionary<string, string> processArray = CustomForm.unserialize(process);
            //BsonDocument processObject = new BsonDocument();
            //processObject = BsonDocument.Parse(processArray.ToJson());

            string response = "{";

            if(id_process == null || id_process == ""){
                //Check if the process name already exists
                String check = processesTable.Get("name", processObject["name"].ToString());
                if (check != "[]") {
                    response += " \"status\" : \"error\" , \"message\" : \"Proceso ya existe\" }";
                    return Json(response); 
                }
            }
            
            String jsonString = "{";

            try{
                jsonString += "'name':'" + processObject["name"].ToString() + "',";
            }catch(Exception e){}

            try
            {
                jsonString += "'status':'" + processObject["status"].ToString() + "',";

            }
            catch (Exception e) { }

            try
            {
                jsonString += "'type':'" + processObject["type"].ToString() + "',";

            }
            catch (Exception e) { }
            try
            {
                jsonString += "'min_duration': {" +
                                "'duration' : '" + processObject["min_length"].ToString() + "'," +
                                "'type' : '" + processObject["min_length_type"].ToString() + "'}," +
                        "'max_duration': {" +
                            "'duration' : '" + processObject["max_length"].ToString() + "'," +
                                "'type' : '" + processObject["max_length_type"].ToString() + "'}";
            }
            catch (Exception e) { }

            string processId = "";
            jsonString += "}";
            
            //Check if the id exists
            if (id_process != null && id_process != ""){
                 
                String processString = processesTable.GetRow(id_process);
                JObject processLast = JsonConvert.DeserializeObject<JObject>(processString);

                if (processObject["name"].ToString() != processLast["name"].ToString())
            {
                    //Check if name changed already exist
                    processString = processesTable.Get("name", processObject["name"].ToString());
                    JArray processList = JsonConvert.DeserializeObject<JArray>(processString);
                    if (processList.Count > 0) {
                        response += " \"status\" : \"error\" , \"message\" : \"Proceso ya existe\" }";
                        return Json(response); 
                    }
            }
                
                processId = processesTable.SaveRow(jsonString, id_process);
                
                response += " \"status\" : \"success\" , \"data\" : { \"process_id\" : \""+ processId +"\"} }";

                return Json(response);
            }                

            processId = processesTable.SaveRow(jsonString);
            response += " \"status\" : \"success\" , \"data\" : { \"process_id\" : \"" + processId + "\"} }";

            return Json(response);
               
        }

        /// <summary>
        ///     Gets a html table with all the stored processes
        /// </summary>
        /// <returns></returns>
        public ActionResult getProcessesTable()
        {
            if (this.Request.IsAjaxRequest())
            {
                string processes = processesTable.GetRows();
                JArray processesArray = JsonConvert.DeserializeObject<JArray>(processes);
               
                foreach (JObject process in processesArray)
                {
                    try
                    {
                        JToken timeData = process["min_duration"];
                        process["min_duration_time"] = timeData["duration"];
                        if (timeData["type"].ToString() == "sec") { process["min_duration_type"] = "segundo(s)"; }
                        else if (timeData["type"].ToString() == "min") { process["min_duration_type"] = "minuto(s)"; }
                        else if (timeData["type"].ToString() == "hour") { process["min_duration_type"] = "hora(s)"; }
                        else if (timeData["type"].ToString() == "day") { process["min_duration_type"] = "día(s)"; }
                        else if (timeData["type"].ToString() == "mon") { process["min_duration_type"] = "mes(s)"; }
                        process.Remove("min_duration");

                        timeData = process["max_duration"];
                        process["max_duration_time"] = timeData["duration"];
                        if (timeData["type"].ToString() == "sec") { process["max_duration_type"] = "segundo(s)"; }
                        else if (timeData["type"].ToString() == "min") { process["max_duration_type"] = "minuto(s)"; }
                        else if (timeData["type"].ToString() == "hour") { process["max_duration_type"] = "hora(s)"; }
                        else if (timeData["type"].ToString() == "day") { process["max_duration_type"] = "día(s)"; }
                        else if (timeData["type"].ToString() == "mon") { process["max_duration_type"] = "mes(s)"; }
                        process.Remove("max_duration");

                    }
                    catch (Exception e) { }
                }
               
                return View(processesArray);
            }
            return this.Redirect("/Processes/Process");
        }

        /// <summary>
        ///     Delete a process
        /// </summary>
        /// <param name="id">
        ///     The id of the process
        /// </param>
        /// <returns>
        ///     String with de result
        /// </returns>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        public String deleteProcess(String id) {

            String result = processesTable.DeleteRow(id);

            try
            {
                //Delete movements of processes
                String movementArray = movementTable.Get("processes", id);
                JArray movements = JsonConvert.DeserializeObject<JArray>(movementArray);
                foreach (JObject movement in movements)
                {
                    movementTable.DeleteRow(movement["_id"].ToString());

                    //Delete authorization of movements
                    JToken authorizations = movement["authorization"];
                    foreach (String authorization in authorizations)
                    {
                        authorizationTable.DeleteRow(authorization);
                    }
                }
            }
            catch (Exception e) { }

            return result;
        }
        /// <summary>
        ///     This method allows to get the document's childs by id
        /// </summary>
        /// <param name="parentCategory">
        ///     The category's id that we want to find its children
        /// </param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns an array with the information needed to represent a tree
        ///     only tipo = 1, location type
        /// </returns>
        public JsonResult getNodeContent(String parentCategory)
        {
            if (parentCategory == "") parentCategory = "null";
            String categoriesString = locationTable.Get("parent", parentCategory);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
            JArray categoryResult = new JArray();
            foreach (JObject category in categoriesObject)
            {
                if (category["tipo"].ToString() == "1")
                {
                    try
                    { //try to remove customFields, if can't be removed it doesn't care
                        category.Remove("profileFields");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    try
                    {
                        category.Remove("parentCategory");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    try
                    {
                        category.Remove("CreatedDate");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    try
                    {
                        category.Remove("LastmodDate");
                    }
                    catch (Exception e) { /*Ignored*/ }
                    try
                    {
                        category.Remove("map");
                    }
                    catch (Exception e) { /*Ignored*/ }
                    try
                    {
                        category.Remove("imgext");
                    }
                    catch (Exception e) { /*Ignored*/ }
                
                    categoryResult.Add(category);
                }

            }
            String categoryResultString = JsonConvert.SerializeObject(categoryResult);

            return Json(categoryResultString);
        }

        /// <summary>
        ///     This method allows to get the route of the childs by id parent
        /// </summary>
        /// <param name="parentCategory">
        ///     The category's id that we want to find the route
        /// </param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns an array with the information needed to represent the route
        /// </returns>

        public JsonResult getRoute(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {
                string category = locationTable.GetRow(parentCategory);
                JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                JObject categoryObject = new JObject();
                categoryObject.Add("id", actualCategory["_id"].ToString());
                route.Add(categoryObject);
                parentCategory = actualCategory["parent"].ToString();
            }

            JObject result = new JObject();
            result.Add("route", route);
            return Json(JsonConvert.SerializeObject(result));
        }



        /// <summary>
        ///     Get permissions
        /// </summary>

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

        public void AddProcessToLocation(String Location, String Process) {
            String locatstring = locationTable.GetRow(Location);
            JObject obj = JsonConvert.DeserializeObject<JObject>(locatstring);

            obj["processId"] = Process;

            locationTable.SaveRow(JsonConvert.SerializeObject(obj), Location);
        }
    
    }
}
