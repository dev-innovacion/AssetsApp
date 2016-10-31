using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RivkaAreas.Movement.Models;
using Rivka.Db;
using Rivka.Form;
using Rivka.Form.Field;
using Rivka.Security;
namespace RivkaAreas.Movement.Controllers
{

    [Authorize]
    public class MovementProfileController : Controller
    {
        protected MovementTable _movementTable;
        protected ProfileTable _profileTable;
        protected UserTable _userTeble;
        protected AuthorizationTable _authorizationTable;
        protected CategoryTable _categoryTable;
        protected ProcessesTable _processesTable;
        protected validatePermissions validatepermissions;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        public MovementProfileController()
        {
            this._profileTable = new ProfileTable("MovementProfiles");
            this._movementTable = new MovementTable();      
            this._userTeble = new UserTable();
            this._authorizationTable = new AuthorizationTable();
            this._categoryTable = new CategoryTable();
            validatepermissions = new validatePermissions();
            this._processesTable = new ProcessesTable();
            _logTable = new LogBook.Controllers.LogBookController();
        }
        public ActionResult Index()
        {
            //TODO: advanced search using Agregation.
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "r", dataPermissionsClient);
            if (access == true && accessClient == true)
            {
                String rowArray = _profileTable.GetRows();
                JArray profiles = JsonConvert.DeserializeObject<JArray>(rowArray);

                return View(profiles);
            }
            else
            {

                return Redirect("~/Home");
            } 
        }

        /// <summary>
        ///     newProfile
        /// </summary>
        /// <param name="idProfile"></param>
        /// <param name="idProcess"></param>

        public ActionResult newProfile(string idProfile = null, string idProcess = null)
        {
            bool upd = false;
            bool updclient = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
           
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "r", dataPermissionsClient);
            upd = validatepermissions.getpermissions("profiles", "u", dataPermissions);
            updclient = validatepermissions.getpermissions("custom_fields", "u", dataPermissionsClient);

        
             
             if (idProfile != null && ( upd == false || updclient==false ))
             {
                 access = false;
                 accessClient = false;
             }
             if (access == true && accessClient==true)
             {
            CustomFieldsTable cft = new CustomFieldsTable("MovementFields");
            String fieldsArray = cft.GetRows();
            JArray fields = JsonConvert.DeserializeObject<JArray>(fieldsArray);

            if (idProfile != null && idProfile != "null" && idProfile != "")
            {
                    String rowString = _profileTable.GetRow(idProfile);
                    JObject profile = JsonConvert.DeserializeObject<JObject>(rowString);
                    if (profile != null)
                    {
                        String profileJson = JsonConvert.SerializeObject(profile);
                        ViewData["profile"] = new HtmlString(profileJson);
                    } 
            }

            if (idProcess != null) {
                String processString = _processesTable.GetRow(idProcess);
                JObject process = JsonConvert.DeserializeObject<JObject>(processString);
                if (process != null)
                {
                    String processJson = JsonConvert.SerializeObject(process);
                    ViewData["process"] = new HtmlString(processJson);
                }
            }

            String rowArray = _profileTable.GetRows();
            JArray profiles = JsonConvert.DeserializeObject<JArray>(rowArray);
            ViewBag.profiles = profiles;

            return View(fields);
        }
             else
             {

                 return Redirect("~/Home");
             } 
        }

        /// <summary>
        ///     Get a profile specified by the idProfile
        /// </summary>
        /// <param name="idProfile"></param>
        /// <returns>
        ///     Returns a Json text with the found profile
        /// </returns>
        [HttpPost]
        public string getProfile(string idProfile)
        {
            if (idProfile != null && idProfile != "null" && idProfile != "")
            {
                //BsonDocument profiles = _profileTable.getRow(idProfile);
                String rowString = _profileTable.GetRow(idProfile);
                JObject profiles = JsonConvert.DeserializeObject<JObject>(rowString);
                if (profiles != null)
                {
                    //profiles.Set("_id", profiles["_id"].ToString());
                    //string profileJson = profiles.ToJson();
                    String profileJson = JsonConvert.SerializeObject(profiles);
                    return profileJson;
                }
            }
            return "";
        }


        /// <summary>
        ///     Get movment by profile
        /// </summary>
        /// <param name="idProfile"></param>
       
        [HttpPost]
        public ActionResult getMovementByProfile(string idProfile)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "d", dataPermissionsClient);
            if (access == true && accessClient == true)
            {
            string movements = _movementTable.Get("profileId", idProfile);
            List<RivkaAreas.Movement.Models.Movement> MovementList = JsonConvert.DeserializeObject<List<RivkaAreas.Movement.Models.Movement>>(movements);
            //List<BsonDocument> profiles = _profileTable.getRows();
            String rowArray = _profileTable.GetRows();
            JArray profiles = JsonConvert.DeserializeObject<JArray>(rowArray);
            ViewBag.canDelete = true;

            try
            {
                string rowString = _profileTable.GetRow(idProfile);
                JObject document = JsonConvert.DeserializeObject<JObject>(rowString);
                //_profileTable.getRow(idProfile).GetElement("name").Value.ToString()
                if (document["name"].ToString() == "Básico")
                    ViewBag.canDelete = false;
            }
            catch (Exception e)
            {
                ViewBag.canDelete = false;
            }


            ViewBag.profiles = profiles;
            ViewBag.idProfile = idProfile;

            return View(MovementList);
        }
            else { return null; }
        }


        /// <summary>
        ///     Save the profile
        /// </summary>
        /// <param name="newProfile"></param>
        /// <param name="idProcess"></param>
        public string saveProfile(string newProfile, string idProfile = null)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "u", dataPermissionsClient);
            if (access == true && accessClient == true)
            {
            string profile = newProfile;
            idProfile = (idProfile == "") ? null : idProfile;

            _profileTable.SaveRow(profile, idProfile);
            try
            {
                _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Insert: Se ha guardado movimiento.", "MovementProfiles", DateTime.Now.ToString());
            }
            catch { }
            return profile;
            } return null;
        }

        /// <summary>
        ///     Deletes a profile and changes the movement's idProfile to the specified
        ///     in the movement param, by default all movements will be changed to a "basic" profile
        /// </summary>
        /// <param name="idProfile">
        ///     A string with de idProfile to delete
        /// </param>
        /// <param name="users">
        ///     A json string with an array of movements to change its idProfile
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public void deleteProfile(string idProfile, string movements)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("custom_fields", "d", dataPermissionsClient);
            if (access == true && accessClient == true)
            {

                if (idProfile != null && idProfile != "null" && idProfile != "")
                {
                    //BsonDocument profile = _profileTable.getRow(idProfile);
                    String rowString = _profileTable.GetRow(idProfile);
                    JObject profile = JsonConvert.DeserializeObject<JObject>(rowString);

                    if (profile != null)
                    {
                        //Delete Authorizations
                        try
                        {
                            dynamic authorizationList = profile["authorization"];
                            foreach (string authorizationId in authorizationList) {
                                _authorizationTable.DeleteRow(authorizationId);
                            }
                        }
                        catch (Exception e) { }

                        //Change Process Movement
                        if (movements != null)
                        {
                            var movementList = JsonConvert.DeserializeObject<MovementList>(movements);
                            foreach (var movement in movementList.movements)
                            {
                                _movementTable.updateProfile(movement);
                            }
                        }
                        _profileTable.DeleteRow(idProfile);
                    }
                   
                }
            }
        }

        /// <summary>
        ///     Get permissions
        /// </summary>
        public bool getpermissions(string permission, string type)
        {
            var datos = Session["Permissions"].ToString();

            JObject allp = JsonConvert.DeserializeObject<JObject>(datos);

            if (allp[permission]["grant"].Count() > 0)
            {
                foreach (string x in allp[permission]["grant"])
                {
                    if (x.Contains(type))
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        /// <summary>
        ///     Select the users with the specific profile
        /// </summary>
        /// <param name="idProfile">
        ///     A string with de idProfile to search
        /// </param>
        /// <author> 
        ///     Abigail Rodriguez Alvarez
        /// </author>
        /// <returns></returns>
        public ActionResult getUserTable(string idProfile) { 
            String userArray = _userTeble.Get("profileId",idProfile);
            if (userArray == "[]" || userArray == null || userArray == "null" || userArray == "")
                return null;
            JArray users = JsonConvert.DeserializeObject<JArray>(userArray);
            return View(users);
        }


        /// <summary>
        ///     This method allows to save the authorization
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="idInventory"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns result
        /// </returns>
        public String saveAuthorization(String jsonString, String id)
        {
            string idAuthorization = _authorizationTable.SaveRow(jsonString,id);
            try
            {
                _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Insert: Se ha guardado autorizacion.", "AuthorizationMovement", DateTime.Now.ToString());
            }
            catch { }
            return idAuthorization;
        }

        /// <summary>
        ///     This method allows to get tha information of an especific authorization
        /// </summary>
        /// <param name="id"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns Json with the information
        /// </returns>

        public JsonResult getAuthorization(string id) { 
            return Json(_authorizationTable.GetRow(id));
        }

        /// <summary>
        ///     This method allows to delete a authorization from actual movement
        /// </summary>
        /// <param name="idMovement"></param>
        /// <param name="id"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns result
        /// </returns>
        public String deleteAuthorization(String idMovement , String id)
        {
            String movementString = _movementTable.GetRow(idMovement);
            JObject movement = JsonConvert.DeserializeObject<JObject>(movementString);

            JToken authorization = movement["authorization"];
            for (int x = 0; x < authorization.Count(); x++) {
                if (authorization[x].ToString() == id)
                {
                    authorization[x].Remove();
                    break;
                }
            }

            movement["authorization"] = authorization;

            movementString = JsonConvert.SerializeObject(movement);
            _movementTable.SaveRow(movementString,idMovement);
            try
            {
                _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Delete: Se ha eliminado autorizacion.", "MovementProfiles", DateTime.Now.ToString());
            }
            catch { }
            string result = _authorizationTable.DeleteRow(id);
            return result;
        }

        /// <summary>
        ///     This method allows to get the document's childs by id
        /// </summary>
        /// <param name="parentCategory">
        ///     The category's id that we want to find its children
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns an array with the information needed to represent a tree
        /// </returns>
        public JsonResult getNodeContent(String parentCategory)
        {
            if (parentCategory == "") parentCategory = "null";
            String categoriesString = _categoryTable.Get("parentCategory", parentCategory);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
            foreach (JObject category in categoriesObject)
            {
                try
                { //try to remove customFields, if can't be removed it doesn't care
                    category.Remove("customFields");
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
            }
            categoriesString = JsonConvert.SerializeObject(categoriesObject);

            return Json(categoriesString);
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

                string category = _categoryTable.GetRow(parentCategory);
                JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                JObject categoryObject = new JObject();
                categoryObject.Add("id", actualCategory["_id"].ToString());
                route.Add(categoryObject);
                parentCategory = actualCategory["parentCategory"].ToString();
            }

            JObject result = new JObject();
            result.Add("route", route);
            return Json(JsonConvert.SerializeObject(result));
        }
         /// <summary>
        ///     This method check if the name of the movemente already exists in the process
        /// </summary>
        /// <param name="idProcess">
        ///     The process id that we want check
        /// </param>
        /// <param name="nameMovement">
        ///     The the name to cheack
        /// </param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns the result
        /// </returns>

        public String validateName(String idProcess, String nameMovement, String idProfile)
        {
            String result = "false"; //Name not repet

            String movmentArray = _movementTable.Get("processes", idProcess);
            JArray movments = JsonConvert.DeserializeObject<JArray>(movmentArray);

            //Check idProfile
            if (idProfile == "")
            {
                  foreach (JObject movement in movments)
                    {
                        if (movement["name"].ToString() == nameMovement)
                        {
                            result = "true"; //Name Repet
                            break;
                        }
                    }
            }

            else {
                String movementString = _movementTable.GetRow(idProfile);
                JObject movementLast = JsonConvert.DeserializeObject<JObject>(movementString);

                if (nameMovement != movementLast["name"].ToString())
                {
                    //Check if name changed already exist
                    int cant = 0;
                    foreach (JObject movement in movments)
                        if (movement["name"].ToString() == nameMovement) cant++ ;

                    if (cant > 0) result = "true";   //Name Repet                        
                }
            }

            return result;
        }
    }
}
