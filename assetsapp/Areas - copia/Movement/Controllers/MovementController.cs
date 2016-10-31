using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RivkaAreas.Movement.Models;

using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rivka.Form.Field;
using Rivka.Form;

namespace RivkaAreas.Movement.Controllers
{
    [Authorize]
    public class MovementController : Controller
    {
        protected ProfileTable _profileTable;
        protected MovementTable _movementTable;
        protected ProcessesTable _processTable;
        protected ProcessDiagram _processDiagram;
        protected AuthorizationTable _authorizationTable;
        protected UserTable _userTable;
        protected DemandTable _demandTable;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;

        /// <summary>
        ///     Initializes the models
        /// </summary>
        /// <author>
        ///     Juan Bautista
        /// </author>
        public MovementController()
        {
            this._profileTable = new ProfileTable("MovementProfiles");
            this._movementTable = new MovementTable();
            this._processTable = new ProcessesTable();
            this._processDiagram = new ProcessDiagram();
            this._authorizationTable = new AuthorizationTable();
            this._userTable = new UserTable();
            this._demandTable = new DemandTable();
        }


        public ActionResult Index()
        {       
            string rowArray = _processTable.GetRows();
            JArray profiles = JsonConvert.DeserializeObject<JArray>(rowArray);
            ViewBag.profiles = profiles;

            return View();
        }
      
        public JsonResult getProcessInfo(String idProcess)
        {
            try
            {
                String rowString = _processTable.GetRow(idProcess);
                JObject row = JsonConvert.DeserializeObject<JObject>(rowString);

                try
                {
                    JToken timeData = row["min_duration"];
                    row["min_duration_time"] = timeData["duration"];
                    if (timeData["type"].ToString() == "sec") { row["min_duration_type"] = "segundo(s)"; }
                    else if (timeData["type"].ToString() == "min") { row["min_duration_type"] = "minuto(s)"; }
                    else if (timeData["type"].ToString() == "hour") { row["min_duration_type"] = "hora(s)"; }
                    else if (timeData["type"].ToString() == "day") { row["min_duration_type"] = "día(s)"; }
                    else if (timeData["type"].ToString() == "mon") { row["min_duration_type"] = "mes(s)"; }
                    row.Remove("min_duration");

                    timeData = row["max_duration"];
                    row["max_duration_time"] = timeData["duration"];
                    if (timeData["type"].ToString() == "sec") { row["max_duration_type"] = "segundo(s)"; }
                    else if (timeData["type"].ToString() == "min") { row["max_duration_type"] = "minuto(s)"; }
                    else if (timeData["type"].ToString() == "hour") { row["max_duration_type"] = "hora(s)"; }
                    else if (timeData["type"].ToString() == "day") { row["max_duration_type"] = "día(s)"; }
                    else if (timeData["type"].ToString() == "mon") { row["max_duration_type"] = "mes(s)"; }
                    row.Remove("max_duration");
                }
                catch (Exception e) { }

                return Json(JsonConvert.SerializeObject(row));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public JsonResult getProcessCreator(String idProcess)
        {
            try
            {
                String rowString = _userTable.GetRow(idProcess);
                JObject row = JsonConvert.DeserializeObject<JObject>(rowString);
                return Json(JsonConvert.SerializeObject(row));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public ActionResult getMovementTable()
        {
            string rowArray = _processTable.GetRows();
            JArray processes = JsonConvert.DeserializeObject<JArray>(rowArray);
                
            return View(processes);
        }
       
        public ActionResult setMovementTable(String idProcess) {
            ViewBag.idProcess = idProcess;
            String movementArray = _movementTable.Get("processes", idProcess);
            JArray movments = JsonConvert.DeserializeObject<JArray>(movementArray);
            return View(movments);
        }

        public String deleteMovement(String idMovement) {
            if (idMovement != null && idMovement != "null" && idMovement != ""){
                //BsonDocument profile = _profileTable.getRow(idProfile);
                String rowString = _movementTable.GetRow(idMovement);
                JObject profile = JsonConvert.DeserializeObject<JObject>(rowString);

                if (profile != null)
                {
                    //Delete Authorizations
                    try
                    {
                        dynamic authorizationList = profile["authorization"];
                        foreach (string authorizationId in authorizationList)
                        {
                            _authorizationTable.DeleteRow(authorizationId);
                        }
                    }
                    catch (Exception e) { }

                    try {
                        String demandArray = _demandTable.Get("movement" , idMovement);
                        JArray demand = JsonConvert.DeserializeObject<JArray>(demandArray);

                        foreach (JObject docDemand in demand) {
                            docDemand["movement"] = null;
                            _demandTable.SaveRow(JsonConvert.SerializeObject(docDemand),docDemand["_id"].ToString());
                        }
 
                    } catch(Exception e){}

                   _movementTable.DeleteRow(idMovement);
                   try
                   {
                       _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Delete: Se ha eliminado movimiento.", "MovementProfiles", DateTime.Now.ToString());
                   }
                   catch { }
                   return "true";
                }
                return "false";
            }
            return "false";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idProcess"></param>
        /// <returns></returns>
        public JsonResult checkProcessDiagram(string idProcess)
        {
            bool isInDiagram = _processDiagram.IsInDiagram(idProcess);

            if(isInDiagram == true)
                return Json("{\"check\":true}");
            else
                return Json("{\"check\":false}");
        }

    }
}
