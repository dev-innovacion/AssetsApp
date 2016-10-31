using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RivkaAreas.Inventory.Models;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Inventory.Controllers
{
    public class InventoryController : Controller
    {
        //
        // GET: /Inventory/Inventory/
        protected InventoryTable _inventoryTable;
        protected UserTable _userTable;
        protected LocationTable _locationTable;
        protected ProfileTable _profileTable;
        protected HardwareTable _hardwareTable;
        protected MongoModel _locationProfiles = new MongoModel("LocationProfiles");
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;

        public InventoryController() {
            this._inventoryTable = new InventoryTable();
            this._userTable = new UserTable();
            this._locationTable = new LocationTable();
            this._profileTable = new ProfileTable();
            this._hardwareTable = new HardwareTable();
            this._logTable = new LogBook.Controllers.LogBookController();
        }

        public ActionResult Index()
        {
            string rowArray = _locationTable.GetRows();
            JArray locat = JsonConvert.DeserializeObject<JArray>(rowArray);
            Dictionary<string, string> data = new Dictionary<string, string>();

            foreach (JObject items in locat)
            {
                data.Add(items["_id"].ToString(), items["name"].ToString());
            }

            ViewData["locations"] = data;
            return View();
        }
        /// <summary>
        ///     Get the inventory table belonging to the current user
        /// </summary>
        /// <author> 
        ///     Abigail Rodriguez Alvarez
        /// </author>
        /// <returns>The inventories information</returns>
        /// 

        public JsonResult getRoute(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {

                String actualCategory = _locationTable.GetRow(parentCategory);
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
        public ActionResult getInventoryTable() {
            string userid = Session["_id"].ToString();
            String inventoryArray = _inventoryTable.Get("Creator",userid,"dateStart");
            JArray inventory = JsonConvert.DeserializeObject<JArray>(inventoryArray);

            foreach(JObject rowString in inventory){

                JToken userList = rowString["userList"];
                string cant = userList.Count().ToString();
                if (cant == "0") {
                    cant = "Todos"; //All Users
                } 
                rowString["cant"] = cant;

                try
                {
                    String locationString = _locationTable.GetRow(rowString["location"].ToString());
                    JObject location = JsonConvert.DeserializeObject<JObject>(locationString);
                    rowString["locationName"] = location["name"];
                }
                catch (Exception e) { }

                try
                {
                    String profileString = _profileTable.GetRow(rowString["profile"].ToString());
                    JObject profile = JsonConvert.DeserializeObject<JObject>(profileString);
                    rowString["profileName"] = profile["name"];
                }
                catch (Exception e) { }

                try
                {
                    String hardwareString = _hardwareTable.GetHardware(rowString["hardware"].ToString());
                    JArray hardware = JsonConvert.DeserializeObject<JArray>(hardwareString);
                    rowString["hardwareName"] = hardware[0]["name"];
                }
                catch (Exception e) { }

            }
            return View(inventory);
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
        public ActionResult getUserTable(string idProfile)
        {
            String userArray = _userTable.Get("profileId", idProfile);
            if (userArray == "[]" || userArray == null || userArray == "null" || userArray == "")
                return null;
            JArray users = JsonConvert.DeserializeObject<JArray>(userArray);
            return View(users);
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
            String categoriesString = _locationTable.Get("parent", parentCategory);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
          

            JArray categoryResult = new JArray();
            foreach (JObject category in categoriesObject)
            {
                try {
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
                catch (Exception ex) {
                    category["tipo"] = "1";
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

        public JsonResult getNodeContent2(String id, String userid = null)
        {
            JObject result = new JObject();
            if (id == "") id = "null";
            String categoriesString = "";
            if (id == "null" && userid != null)
            {
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);

                userstring = _profileTable.GetRow(userobj["profileId"].ToString());
                JObject profilobj = JsonConvert.DeserializeObject<JObject>(userstring);
                if (profilobj["name"].ToString() != "Administrador de sistema" && profilobj["name"].ToString() != "Otro")
                {
                    if (profilobj["name"].ToString() == "Gerente regional")
                    {
                        id = loadLocationsRegion(userid);

                    }
                    else if (profilobj["name"].ToString() == "Gerente de conjunto")
                    {
                        id = loadLocationsConjunto(userid);
                    }
                    else
                    {
                        id = loadLocationsConjunto(userid);
                    }

                    result["id"] = id;
                    id = (id == null || id == "null") ? "" : id;
                    if (id == "") result["name"] = "";
                    else
                    {
                        try
                        {
                            categoriesString = _locationTable.GetRow(id);
                            result["name"] = (JsonConvert.DeserializeObject<JObject>(categoriesString))["name"].ToString();
                        }
                        catch
                        {

                        }
                    }

                }
                else
                {
                    id = "null";
                    result["id"] = id;
                    result["name"] = "Home";

                }
            }

            //id = (id == "") ? "null" : id;
            //categoriesString = locationTable.Get("parent", id);
            ////  String objectsString = _objectTable.Get("parentCategory", id);

            //if (categoriesString == null) return null; //there are no subcategories

            //JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
            //// JArray objectObject = JsonConvert.DeserializeObject<JArray>(objectsString);


            //JArray newobjs = new JArray();

           

            //   return Json(JsonConvert.SerializeObject(newobjs), JsonRequestBehavior.AllowGet);
            return Json(JsonConvert.SerializeObject(result), JsonRequestBehavior.AllowGet);
        }

        public String loadLocationsRegion(String userid)
        {

            try
            {
                String region = "";
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getconjunt = _locationProfiles.Get("name", "Region");
                string idregion = "";
                JArray conjuntja = new JArray();
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idregion = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                JArray locatList = new JArray();
                JObject locat = new JObject();
                JArray ele = new JArray();
                String rowArray;
                List<String> list1 = new List<String>();
                List<String> list2 = new List<String>();


                //******************************************************************************
                foreach (JObject ob in locats)
                {
                    rowArray = _locationTable.GetRow(ob["id"].ToString());
                    locat = JsonConvert.DeserializeObject<JObject>(rowArray);
                    if (locat["profileId"].ToString() == idregion)
                    {
                        region = locat["_id"].ToString();
                        break;
                    }

                }

                return region;

            }
            catch (Exception ex)
            {
                return null;

            }
        }

        public String loadLocationsConjunto(String userid)
        {

            try
            {
                String region = "";
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getconjunt = _locationProfiles.Get("name", "Conjunto");
                string idregion = "";
                JArray conjuntja = new JArray();
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idregion = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                JArray locatList = new JArray();
                JObject locat = new JObject();
                JArray ele = new JArray();
                String rowArray;
                List<String> list1 = new List<String>();
                List<String> list2 = new List<String>();


                //******************************************************************************
                foreach (JObject ob in locats)
                {
                    rowArray = _locationTable.GetRow(ob["id"].ToString());
                    locat = JsonConvert.DeserializeObject<JObject>(rowArray);
                    if (locat["profileId"].ToString() == idregion)
                    {
                        region = locat["_id"].ToString();
                        break;
                    }

                }

                return region;

            }
            catch (Exception ex)
            {
                return null;

            }
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
      

        /// <summary>
        ///     This method allows to save the inventory
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="idInventory"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns result
        /// </returns>
        public String saveInventory(String jsonString, String idInventory = null) {
            try
            {
                _inventoryTable.SaveRow(jsonString,idInventory);
                _logTable.SaveLog(Session["_id"].ToString(), "Inventario", "Insert: guardar inventario", "Inventory", DateTime.Now.ToString());
                return "success";
            }
            catch (Exception e) 
            {
                return "error"; 
            }
        }

        /// <summary>
        ///     This method allows to delete a inventory
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns result
        /// </returns>
        public String deleteInventory(String inventoryId) {
            try
            {
                _inventoryTable.DeleteRow(inventoryId);
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }

        /// <summary>
        ///     This method allows to get tha information of an especific inventory
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns Json with the information
        /// </returns>
        public JsonResult getInventory(String inventoryId)
        {
            String inventoryString = _inventoryTable.GetRow(inventoryId);
            JObject inventory = JsonConvert.DeserializeObject<JObject>(inventoryString);

            return Json(JsonConvert.SerializeObject(inventory));
        }
    }
}
