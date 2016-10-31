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
using Rivka.Security;
using Rivka.Form;
using Rivka.Form.Field;
using Rivka.Mail;

namespace RivkaAreas.Locations.Controllers
{
    [Authorize]
    public class LocationController : Controller
    {
        //
        // GET: /Locations/Location/
        LocationProfileTable  profileTable; //profile's model object
        LocationTable locationTable;
        CustomFieldsTable _fieldsTable;
        HardwareTable _hardwareTable;
        UserTable _userTable;
        protected ValidateLimits validatelim;
        ObjectReal _objectRealTable;
        Demand _demandTable;
        Notifications Notificate;
        protected validatePermissions validatepermissions;
        public LocationController()
        {
            profileTable = new LocationProfileTable();
            locationTable = new LocationTable();
            _fieldsTable = new CustomFieldsTable("LocationCustomFields");
            _hardwareTable = new HardwareTable();
            _userTable = new UserTable();
            validatelim = new ValidateLimits();
            _objectRealTable = new ObjectReal();
            _demandTable = new Demand();
            validatepermissions = new validatePermissions();
            Notificate = new Notifications();

        }
        public string validateLimit()
        {
            try
            {
                string result = validatelim.validate("Locations", null, null, "Locations", 1);


                return result;
            }
            catch (Exception ex)
            {
                return "error";
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("location", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("location", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
            try
            {
                String profileOptions = "";
                List<BsonDocument> profileList = profileTable.getRows(); //getting all the profiles

                profileOptions += "<option value='null' name='ninguno' selected> Ninguno</option>";
                foreach (BsonDocument document in profileList) //for each profile we create an option element with id as value and the name as the text
                {
                    profileOptions += "<option value='" + document.GetElement("_id").Value + "'"; //setting the id as the value
                    if (document.GetElement("name").Value == "basico")
                        profileOptions += " selected"; //setting the Básico profile to the selected one
                    profileOptions += ">" + document.GetElement("name").Value + "</option>"; //setting the text as the name
                }

                ViewData["profileList"] = new HtmlString(profileOptions);
            }
            catch (Exception e)
            {
                ViewData["profileList"] = null;
            }

            string rowArray = locationTable.GetRows(); 
            JArray locat = JsonConvert.DeserializeObject<JArray>(rowArray);
            Dictionary<string, string> data = new Dictionary<string, string>();

            foreach (JObject items in locat)
            {
                data.Add(items["_id"].ToString(), items["name"].ToString());
            }

            ViewData["locations"] = data;

            loadnodes();
          //  loadsetups();
            loadusers();
            GetAvailableHardware();
            return View();
            }
            else
            {

                return Redirect("~/Home");
            }
        }

        public String loadprofiles()
        {
            String profileOptions = "";
            List<BsonDocument> profileList = profileTable.getRows(); //getting all the profiles

            profileOptions += "<option value='null' name='ninguno' selected> Ninguno</option>";
            foreach (BsonDocument document in profileList) //for each profile we create an option element with id as value and the name as the text
            {
                profileOptions += "<option value='" + document.GetElement("_id").Value + "'"; //setting the id as the value
                if (document.GetElement("name").Value == "basico")
                    profileOptions += " selected"; //setting the Básico profile to the selected one
                profileOptions += ">" + document.GetElement("name").Value + "</option>"; //setting the text as the name
            }

            return profileOptions;
        }

        public string loadsetups()
        {

            try
            {
                String setupOptions = "";
                String setupList = locationTable.GetRows(); //getting all the profiles
                JArray setups = JsonConvert.DeserializeObject<JArray>(setupList);

                setupOptions += "<option value='null' selected> Ninguno</option>";

                foreach (JObject document in setups) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["setupname"].ToString() != "")
                    {
                        setupOptions += "<option value='" + document["setupname"] + "'"; //setting the id as the value
                        setupOptions += ">" + document["setupname"] + "</option>"; //setting the text as the name
                    }

                }


                return setupOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void loadusers()
        {

            try
            {
                String userOptions = "";
                String userList = _userTable.GetRows(); //getting all the profiles _

                JArray users = JsonConvert.DeserializeObject<JArray>(userList);

                userOptions += "<option value='null' selected> Ninguno</option>";

                foreach (JObject document in users) //for each profile we create an option element with id as value and the name as the text
                {

                    userOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                    userOptions += ">" + document["name"] + " " + document["lastname"] + "</option>"; //setting the text as the name
                }


                ViewData["userList"] = new HtmlString(userOptions);
            }
            catch (Exception e)
            {
                ViewData["userList"] = null;
            }
        }

        public void loadnodes() {
            try
            {
                String nodeOptions = "";
                String nodesList = locationTable.GetRows(); //getting all the profiles
                JArray nodes = JsonConvert.DeserializeObject<JArray>(nodesList);
                nodeOptions += "<option value='null' selected> Home</option>";

                foreach (JObject document in nodes) //for each profile we create an option element with id as value and the name as the text
                {
                        nodeOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                        nodeOptions += ">" + document["name"] + "</option>"; //setting the text as the name
                }


                ViewData["nodesList"] = new HtmlString(nodeOptions);
            }
            catch (Exception e)
            {
                ViewData["nodesList"] = null;
            }
        }

        /// <summary>
        ///     Get the available hardware not used yet
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAvailableHardware()
        {
            String result = "[]";
            try
            {
                string hardstring = _hardwareTable.GetFullHardware();
                JArray hardobj = JsonConvert.DeserializeObject<JArray>(hardstring);
                List<String> ids = new List<String>();
                JArray objs = new JArray();

                foreach (JObject obj in hardobj)
                {
                    if(!ids.Contains(obj["hardware_reference"].ToString())){
                        JObject newobj= new JObject();
                        newobj["hardware_reference"]=obj["hardware_reference"].ToString();
                        newobj["hardwareName"]=obj["hardwareName"].ToString();

                        newobj["image"] = obj["image"].ToString() != "" ? "/Content/Images/hardware/" + obj["image"].ToString() : "/Content/Images/hardware/antena.jpg";

                        JObject hw = new JObject();
                        hw["_id"] = obj["_id"].ToString();
                        hw["serie"] = obj["serie"].ToString();
                        newobj["list_hardware"] = new JArray();
                        if (!obj.Properties().Select(p => p.Name).ToList().Contains("location_id"))
                        {
                            newobj["list_hardware"] = new JArray(hw);
                        }
                        else if (obj["location_id"].ToString() == "" || obj["location_id"].ToString() == "null")
                        {
                            newobj["list_hardware"] = new JArray(hw);
                        }
                        objs.Add(newobj);
                    }else{
                        JObject hw = new JObject();
                        hw["_id"] = obj["_id"].ToString();
                        hw["serie"] = obj["serie"].ToString();

                        if (!obj.Properties().Select(p => p.Name).ToList().Contains("location_id"))
                        {
                            foreach (JObject item in objs)
                            {
                                if (item["hardware_reference"].ToString() == obj["hardware_reference"].ToString())
                                {
                                    JArray jobj = JsonConvert.DeserializeObject<JArray>(item["list_hardware"].ToString());
                                    jobj.Add(hw);
                                    item["list_hardware"] = jobj;
                                }
                            }
                        }
                        else if (obj["location_id"].ToString() == "" || obj["location_id"].ToString() == "null")
                        {
                            foreach (JObject item in objs)
                            {
                                if (item["hardware_reference"].ToString() == obj["hardware_reference"].ToString())
                                {
                                    JArray jobj = JsonConvert.DeserializeObject<JArray>(item["list_hardware"].ToString());
                                    jobj.Add(hw);
                                    item["list_hardware"] = jobj;
                                }
                            }
                        }

                        
                    }

                    ids.Add(obj["hardware_reference"].ToString());
                }


                return Json(JsonConvert.SerializeObject(objs));
            }
            catch (Exception e)
            {
                return Json(result);
            }
        }

        /// <summary>
        ///     This method creates the form's view for the specified profile in the form of tabs
        /// </summary>
        /// <param name="profile">
        ///     The profile's id which we want to print
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the form's html structure
        /// </returns>
        public String getFormView(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    BsonDocument document = profileTable.getRow(profile);
                    String formString = document.GetElement("customFields").Value.ToString();
                    String response = CustomForm.getFormView(formString, "LocationCustomFields"); //we use the CustomForm class to generate the form's fiew
                    return response;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        ///     This method creates the form's headers names for the specified profile, these headers are used by bootstrap to create the tabs
        /// </summary>
        /// <param name="profile">
        ///     The profile's id which we want to print
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the form's tabs headers html structure
        /// </returns>
        public String getFormTitlesView(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    BsonDocument document = profileTable.getRow(profile);
                    String formString = document.GetElement("customFields").Value.ToString();
                    String response = CustomForm.getFormTitlesView(formString); //it use the CustomForm class to create the headers
                    return response;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public String saveSetuplocation(String selectedID, String setup)
        {
            bool edit = false;
            bool editClient = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("location", "c", dataPermissions);
            accessClient = validatepermissions.getpermissions("location", "c", dataPermissionsClient);
            edit = validatepermissions.getpermissions("location", "u", dataPermissions);
            editClient = validatepermissions.getpermissions("location", "u", dataPermissionsClient);

            if ((access == true && accessClient == true) || (edit == true && editClient == true))
            {
               if (this.Request.IsAjaxRequest())
               {
                   String locationID = (selectedID == "null") ? null : selectedID; //is this an insert or an update?, converting null in javascript to null in c#
                   String jsonData = "";
                   dynamic location = locationTable.GetRow(locationID);
                   /*the gived id does not exists*/
                   if (locationID != null && (location == null))
                   {
                       return "El id especificado no existe";
                   }

                   if (setuplocationExists(setup) == "true" && (locationID == null || locationTable.get("setupname", setup)[0].GetElement("_id").Value.ToString() != locationID))
                   {
                       return "el nombre de configuración ya está siendo utilizado";
                   }

                   if (locationID != null)
                   {
                       String doc = locationTable.GetRow(locationID);
                       JObject docobj = JsonConvert.DeserializeObject<JObject>(doc);
                        docobj["setupname"]=setup;
                        jsonData = JsonConvert.SerializeObject(docobj);
                   }
                   else
                   {
                       jsonData = "{'name':'','imgext':'','parent':'','tipo':'', 'setupname':'" + setup + "', 'setup' : '', 'profileId':'','creatorId':'','profileFields':{}}";
                   }

                   locationTable.SaveRow(jsonData, locationID);
                   return setup;
               }
               return null;
           }
           else
           {
               return null;
           }
        }

        /// <summary>
        ///     This method allows to save a file.
        /// </summary>
        /// <param name="selectedID">
        ///     The document's id where the file is related.
        /// </param>
        /// <param name="name">
        ///     The file's name
        /// </param>
        /// <param name="file">
        ///     The file to store.
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the saved file's name 
        /// </returns>
        public String SaveCustomFile(String selectedID, String name, HttpPostedFileBase file)
        {
            String ext = null;
            String fileName = null;
            if (file != null)
            {
                ext = file.FileName.Split('.').Last(); //getting the extension
            }
            if (file != null)
            {
                string relativepath = "\\Uploads\\Images\\Locations\\CustomImages\\";
                string absolutepath = Server.MapPath(relativepath);
                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }
                fileName = name + DateTime.UtcNow.Ticks + "." + ext;
                file.SaveAs(absolutepath + "\\" + fileName);
            }
            return fileName;
        }

        /// <summary>
        ///     This method saves a user in the system
        /// </summary>
        /// <param name="formData">
        ///     The user's data.
        /// </param>
        /// <param name="file">
        ///     A user's file, if a file is sended.
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the saved user's id
        /// </returns>
        public String saveLocation(FormCollection formData, HttpPostedFileBase file)
        {
            bool edit = false;
           string limit = "true";
            bool editClient = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("location", "c", dataPermissions);
            accessClient = validatepermissions.getpermissions("location", "c", dataPermissionsClient);
            edit = validatepermissions.getpermissions("location", "u", dataPermissions);
            editClient = validatepermissions.getpermissions("location", "u", dataPermissionsClient);

            if ((access == true && accessClient == true) || (edit == true && editClient == true))
            {
         
               string setup = "", setuporiginal = "";
               if (this.Request.IsAjaxRequest())
               {
                   formData = CustomForm.unserialize(formData);  //use the method serialize to parse the string into an array
                   String locationID = (formData["locationID"] == "null") ? null : formData["locationID"]; //is this an insert or an update?, converting null in javascript to null in c#

                   dynamic location = locationTable.GetRow(locationID);
                   String doc = locationTable.GetRow(locationID);
                   JObject docobj= new JObject();
                   if(locationID!=null) docobj= JsonConvert.DeserializeObject<JObject>(doc);
                   /*the gived id does not exists*/
                   if (locationID != null && (location == null))
                   {
                       return "El id especificado no existe";
                   }
                   if (locationID == null && formData["tipo"]!="2")
                   {
                       limit = validateLimit();
                   }
                   if(limit=="true")
                   {

                   /*there is no profile with the gived id*/
                   if (formData["profileSelect"] != "null")
                   {
                       if (profileTable.getRow(formData["profileSelect"]) == null)
                       {
                           return "El perfil especificado no existe";
                       }
                   }

                   if (locationID != null) {
                      
                       
                       setuporiginal=docobj["setup"].ToString() ;
                   } 
                   setup = formData["setupSelect"];
                   /*The selected location name is already in use and is not the user who has it*/
                   if (locationExists(formData["name"]) == "true" && (locationID == null || locationTable.get("name", formData["name"])[0].GetElement("_id").Value.ToString() != locationID))
                   {
                       return "la ubicación ya está siendo utilizado";
                   }

                   //due that the user's id is unique we use it has the image's name, so we store only the extension in the db
                   string ext = null;
                   if (file != null)
                   {
                       ext = file.FileName.Split('.').Last(); //getting the extension
                   }
                   else if (locationID != null)
                   {
                       try
                       {
                           ext = docobj["imgext"].ToString();
                       }
                       catch (Exception e) { }
                   }

                   /* Format validations */
                   //if (!Regex.IsMatch(formData["name"], "([a-záéíóúñA-ZÁÉÍÓÚÑ0-9-_.]){4,}"))
                   //{
                   //    return "Formato incorrecto para: name";
                   //}

                   String nombre = formData["name"].Replace("+", " ");
                   //there are fields that we know that exists so we set them into the json
                   String jsonData = "{'name':'" + formData["name"].Replace("+", " ") + "','imgext':'" + ext
                       + "','responsable':'" + formData["responsable"]
                       + "','responsableNombre':'" + formData["responsableNombre"]
                       + "','planeimgext':'null"
                       + "','parent':'" + formData["parentSelect"] + "','tipo':'" + formData["tipo"] + "', 'setupname':'" + formData["setupname"] + "', 'setup' : '" + formData["setupSelect"] + "', 'profileId':'" + formData["profileSelect"]
                       + "'";

                   try //trying to set the creator's id
                   {
                       jsonData += ", 'creatorId':'";
                       if (locationID == null)
                       {
                           jsonData += this.Session["_id"];
                       }
                       else
                       {
                           try
                           {
                               jsonData += location["creatorId"];
                           }
                           catch (Exception e) { /*Ignored*/}
                       }
                       jsonData += "'";
                   }
                   catch (Exception e) { /*Ignored*/ }

                   String profileId = formData["profileSelect"];
                   String locationName = formData["name"];

                   //remove the setted data in the json from the formData

                   formData.Remove("profileSelect");
                   formData.Remove("locationID");
                   formData.Remove("name");
                   formData.Remove("tipo");
                   formData.Remove("setupname");
                   formData.Remove("setupSelect");
                   formData.Remove("parentSelect");

                   jsonData += ", 'profileFields':{";

                   //foreach element in the formData, let's append it to the jsonData in the profileFields
                   int cont = 0;
                   foreach (String key in formData.Keys)
                   {
                       jsonData += "'" + key + "':'" + formData[key] + "'";

                       cont++;
                       if (cont < formData.Keys.Count)
                       {
                           jsonData += ", ";
                       }
                   }
                   jsonData += "}}";

                   //now that we have the json and we know the data is ok, let's save it
                   string id = locationTable.SaveRow(jsonData, locationID);
                   
                    //Notificate the action
                   if (locationID == null)
                       Notificate.saveNotification("Locations", "Create", "La ubicación '" + locationName + "' ha sido creada");
                   else
                       Notificate.saveNotification("Locations", "Update", "La ubicación '" + locationName + "' ha sido modificada");


                   int h, w, flag=0;
                   //TODO:Aqui se guarda la imagen
                   if (file != null)
                   {
                       string relativepath = "\\Uploads\\Images\\Locations\\";
                       string absolutepath = Server.MapPath(relativepath);
                       if (!System.IO.Directory.Exists(absolutepath))
                       {
                           System.IO.Directory.CreateDirectory(absolutepath);
                       }
                       
                       file.SaveAs(absolutepath + id + "." + ext);

                       Images resizeImage = new Images(absolutepath + "\\" + id + "." + ext, absolutepath, id + "." + ext);
                       // If image bigger than 1MB, resize to 1024px max
                       if (file.ContentLength > 1024 * 1024)
                           resizeImage.resizeImage(new Size(1024, 1024));

                       // Create the thumbnail of the image
                       resizeImage.createThumb();
                       
                   }

                   if (setup != "" && setup != "null" && setup != setuporiginal)
                       copysetup(id, setup, setuporiginal);

                       JObject result=new JObject();
                       result["id"] = id;
                       result["text"] = nombre;
                       result["hasChildren"] = true;
                       result["spriteCssClass"] = "locationimg";

                       return JsonConvert.SerializeObject(result); //returns the saved user's id
               }
               }
               return null;
           }
           else
           {
               return null;
           }
        }

        /// <summary>
        ///     This method creates the user's table html structure
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the user's table html structure
        /// </returns>
        public ActionResult getLocationTable()
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                String docs = locationTable.GetRows(); //getting all the users
                JArray objects = JsonConvert.DeserializeObject<JArray>(docs);
                String locationsTrs = "";
                foreach (JObject location in docs) //for each user we create a new tr with the whole data, this is sended to the view where it's printed
                {
                    //setting id and data-idUser where id is the document's id preceded by the word user, and data-idUser is the document's id
                    locationsTrs += "<tr id='location" + location["_id"] + "' data-idLocation='" + location["_id"] + "'>";

                    //each user has a checkbox in the first position so it can form part of multi-register's oparations
                    locationsTrs += "<td><div><span><center><input type='checkbox' class='uniform checker' name='checker" + location["_id"] + "'></center></span></div></td>";

                    locationsTrs += "<td class='locationName'>" + location["name"] + "</td>";
                    locationsTrs += "<td class='profile' data-idProfile='" + location["profileId"].ToString() + "'>" + profileTable.getRow(location["profileId"].ToString()).GetElement("name").Value + "</td>";

                    locationsTrs += "<td>";
                    //try //trying to set the user's creator id
                    //{
                    //    locationsTrs += locationTable.getRow(location.GetElement("creatorId").Value.ToString()).GetElement("name").Value;
                    //}
                    //catch (Exception e) { /*ignored*/}
                    locationsTrs += "</td>";

                    locationsTrs += "<td>" + location["CreatedDate"].ToString() + "</td>";
                    locationsTrs += "<td>" + location["LastmodDate"].ToString() + "</td>";
                    locationsTrs += "<td>\n<div class='btn-group'>\n<a class='btn' href='#'><i class='icon-edit'></i></a>\n<a class='btn' href='#'><i class='icon-trash row-trash'></i></a></div></td>";
                    locationsTrs += "</tr>";
                }

                ViewData["locations"] = new HtmlString(locationsTrs);
                return View();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///     This method allows to delete a user from the db
        /// </summary>
        /// <param name="selectedID">
        ///     It's the id of the document to be deleted
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a message string.
        /// </returns>
        public String deleteLocation(String selectedID)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("location", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("location", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                 if (this.Request.IsAjaxRequest()) //only available with AJAX
                 {
                     try
                     {
                         locationTable.DeleteRow(selectedID); //deletes the selected document
                         //Notificate the action
                         Notificate.saveNotification("Locations", "Delete", "Una ubicación ha sido eliminada");

                         return "Registro Borrado";
                     }
                     catch (Exception e)
                     {
                         return "Ha ocurrido un error";
                     }
                 }
                 return null;
             }
             else { return null; }
        }

        /// <summary>
        ///     This method allows to delete several users from the db
        /// </summary>
        /// <param name="array">
        ///     It's an array of documents ids
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a message string
        /// </returns>
        public String deleteLocations(List<String> array)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("location", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("location", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                 if (this.Request.IsAjaxRequest()) //only available with AJAX
                 {
                     try //tryign to delete the users
                     {
                         if (array.Count == 0) return null; //if array is empty there are no users to delete
                         foreach (String id in array) //froeach id in the array we must delete the document with that id from the db
                         {
                             locationTable.DeleteRow(id);
                         }
                         //Notificate the action
                         Notificate.saveNotification("Locations", "Delete", array.Count + " ubicaciones han sido eliminadas");

                         return "Borrado";
                     }
                     catch (Exception e)
                     {
                         return null;
                     }
                 }
                 return null;
             } return null;
        }

        /// <summary>
        ///     This method checks if a gived userName is already been in use for another user
        /// </summary>
        /// <param name="userName">
        ///     It's a string that represents the userName which the user wants to use to login
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a boolean, Does this userName already in use?
        /// </returns>
        public String locationExists(String locationName)
        {
            try
            {
                List<BsonDocument> list = locationTable.get("name", locationName);
                if (list.Count == 0)
                    return "false";
                return "true";
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String setuplocationExists(String setupName)
        {
            try
            {
                List<BsonDocument> list = locationTable.get("setupname", setupName);
                if (list.Count == 0)
                    return "false";
                return "true";
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     This method gets a user's data and returns it to the view
        /// </summary>
        /// <param name="userID">
        ///     It's the user's identificator
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero 
        /// </author>
        /// <returns>
        ///     Returns the data's json
        /// </returns>
        public String getLocation(String locationID)
        {
            if (this.Request.IsAjaxRequest())
            {
               
                try
                {
                    String doc = locationTable.GetRow(locationID); //getting the user's data
                    string result="";
                    JObject object1 = JsonConvert.DeserializeObject<JObject>(doc);
                    //the next is the photo's information
                    string relativepath="";
                    string absolutepathdir="" ;
                    string filename ="";
                    string fileabsolutepath1="";

                    if (object1["planeimgext"].ToString() != "null")
                    {
                        try
                        {
                            relativepath = "\\Uploads\\Images\\planos\\";
                            absolutepathdir = Server.MapPath(relativepath);
                            filename = object1["_id"].ToString() + "." + object1["planeimgext"].ToString();
                            fileabsolutepath1 = absolutepathdir + filename;
                            if (System.IO.File.Exists(fileabsolutepath1))
                            {
                                string url = Url.Content(relativepath + filename);
                                object1.Add("ImgUrl2", url); //adding the image's url to the document
                            }
                        }
                        catch { }

                    }

                    //the next is the photo's information
                  
                    try
                    {
                        relativepath = "\\Uploads\\Images\\Locations\\";
                        absolutepathdir = Server.MapPath(relativepath);
                        filename = object1["_id"].ToString() + "." + object1["imgext"].ToString();
                        fileabsolutepath1 = absolutepathdir + filename;
                      
                    }
                    catch { }
                    if (doc == null)
                        return "null";
                    object1.Remove("_id");


                    if (System.IO.File.Exists(fileabsolutepath1))
                    {
                        string url = Url.Content(relativepath + filename);
                        object1.Add("ImgUrl", url); //adding the image's url to the document
                    }

                    

                    
                    return result = JsonConvert.SerializeObject(object1);
                 //   return doc.Replace("\"","'").ToJson(); //returns the json
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        ///     This method creates the user's table html structure
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the user's table html structure
        /// </returns>
        public JsonResult getLocationTree()
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                List<BsonDocument> docs = locationTable.get("parent", "null"); //getting all the users

                return Json(docs.ToJson());

            }
            else
            {
                return null;
            }
        }

        public void deleteNode(string selectedID, bool fromRecursiveCall = false)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("location", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("location", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest()) //only available with AJAX
                {
                    List<BsonDocument> docs = locationTable.get("parent", selectedID); //getting all the locations
                    foreach (BsonDocument nod in docs)
                    {
                        deleteNode(nod["_id"].ToString(), true);
                    }

                    String node = locationTable.GetRow(selectedID);
                    JObject objs = JsonConvert.DeserializeObject<JObject>(node);
                    locationTable.DeleteRow(objs["_id"].ToString());
                    string locationName = objs["name"].ToString();
                    if (!fromRecursiveCall)
                        Notificate.saveNotification("Locations", "Delete", "La ubicación '" + locationName + "' fue eliminada");
                }
            }
        }


        public JsonResult getNodes2(string id)
        {

            if (id == null || id == "") id = "null";
            String docs = locationTable.Get("parent", id); //getting all the users
            JArray objs = JsonConvert.DeserializeObject<JArray>(docs);
            JArray newobjs = new JArray();

            foreach (JObject obj in objs) {
                JObject obj1 = new JObject();
                obj1["id"] = obj["_id"];
                obj1["text"] = obj["name"];
                obj1["hasChildren"] = true;
               // obj1["items"] = new JArray();
              //  obj1["items"] = "[]";
                obj1["spriteCssClass"] = "locationimg";
                newobjs.Add(obj1);
            }

            return Json(JsonConvert.SerializeObject(newobjs), JsonRequestBehavior.AllowGet);
            //var dataContext = new SampleEntities();

            //var employees = from e in dataContext.Employees
            //                where (id.HasValue ? e.ReportsTo == id : e.ReportsTo == null)
            //                select new
            //                {
            //                    id = e.EmployeeID,
            //                    Name = e.FirstName + " " + e.LastName,
            //                    hasChildren = e.Employees1.Any()
            //                };

            //return Json(employees, JsonRequestBehavior.AllowGet);
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

        //list locations in panel
        public JsonResult getNodesList(string nodeid)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                String docs = locationTable.Get("parent", nodeid); //getting all the users
                JArray objs = JsonConvert.DeserializeObject<JArray>(docs);

                string imagenprofile = "";
                string mapa = "[]";
                String locationsTrs = "[";
                if (objs.Count > 0)
                {
                    foreach (JObject location in objs) //for each user we create a new tr with the whole data, this is sended to the view where it's printed
                {
                    try
                    {
                        if (location["profileId"].ToString() != "null")
                        {
                            mapa = location["map"].ToString();
                        }
                    }
                    catch (Exception ex) {
                       
                    }
                    try
                    {
                        if (location["imgext"].ToString() != "" && location["imgext"].ToString() != "null")
                        {
                            try
                            {
                                string relativepath = "\\Uploads\\Images\\Locations\\";
                                string imageExt = location["imgext"].ToString();
                                string absolutepathdir = Server.MapPath(relativepath);
                                string filename = location["_id"].ToString() + "." + imageExt;
                                string fileabsolutepath = absolutepathdir + filename;

                                if (System.IO.File.Exists(fileabsolutepath))
                                {
                                    string url = Url.Content(relativepath + filename);
                                    imagenprofile = "/Uploads/Images/Locations/" + filename; //adding the image's url to the document
                                }
                                else
                                {
                                    imagenprofile = "/Content/Images/planos/No-Imagen-Disponible.png";
                                }
                            }
                            catch (Exception ex) { }
                        }
                        else
                        {
                            imagenprofile = "/Content/Images/planos/No-Imagen-Disponible.png";
                        }
                   
                    //locationsTrs += "<li><i class='icon-home'></i>" + location.GetElement("name").Value + "<div id='div" + location.GetElement("_id").Value + "'></div></li>";
                    
                        locationsTrs += "{ \"name\": \"" + location["name"] + "\",\"tipo\": \"" + location["tipo"] + "\",\"imgext\": \"" + imagenprofile + "\", \"id\": \"" + location["_id"] + "\", \"mapa\": " + mapa + "},";
                       }
                    catch { }
                    }
                    locationsTrs = locationsTrs.Substring(0, locationsTrs.Length - 1);
                }
               
                locationsTrs += "]";
                //Clean the result
                return Json(locationsTrs);

            }
            else
            {
                return null;
            }
        }

        //list locations in panel

        public JsonResult getHomeDetails() {
            JObject objs = new JObject();
            int demands=0;
            String demandarray = _demandTable.Get("location", "null");
            if(demandarray!=null){
            
            JArray demandobjs = JsonConvert.DeserializeObject<JArray>(demandarray);
                demands=demandobjs.Count;
            }

            objs.Add("solicitudes", demands.ToString());
            objs.Add("objetos", locationTable.GetNumObjects("null").ToString());
            objs.Add("ImgUrl", "/Content/Images/planos/No-Imagen-Disponible.png");
            objs.Add("name", "Home");
            return Json(JsonConvert.SerializeObject(objs));
            //return new HtmlString(locationsTrs);
        
        }

        public JsonResult getNodesDetails(string nodeid)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                if (nodeid != "")
                {
                    String location = locationTable.GetRow(nodeid); //getting all the users
                    JObject objs = JsonConvert.DeserializeObject<JObject>(location);
                    if (location != null)
                    {
                        string nameprofile = "";
                        string mapa = "";
                        string plano = "";
                        if (objs["profileId"].ToString() != "null")
                        {
                            nameprofile = profileTable.getRow((String)objs["profileId"]).GetElement("name").Value.ToString();
                            mapa = profileTable.getRow((String)objs["profileId"]).GetElement("mapa").Value.ToString();
                            plano = profileTable.getRow((String)objs["profileId"]).GetElement("plano").Value.ToString();

                        }

                        //sacar solicitudes
                        String demandarray = _demandTable.Get("location", nodeid);
                        JArray demandobjs = new JArray();
                        try
                        {
                              demandobjs = JsonConvert.DeserializeObject<JArray>(demandarray);
                        }
                        catch { }
                        objs.Add("solicitudes", demandobjs.Count.ToString());

                         objs.Add("objetos", locationTable.GetNumObjects(nodeid).ToString());

                        if (objs["tipo"].ToString() == "2") { plano = "1"; }
                        objs.Add("nameprofile", nameprofile);
                        objs.Add("mapa", mapa);
                        objs.Add("plano", plano);

                        //the next is the photo's information
                        try
                        {
                            if (objs["imgext"].ToString() != "" && objs["imgext"].ToString() != "null")
                            {
                                try
                                {
                                    string relativepath = "\\Uploads\\Images\\Locations\\";
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
                            }
                            else
                            {
                                objs.Add("ImgUrl", "/Content/Images/planos/No-Imagen-Disponible.png");
                            }
                        }
                        catch {  }
                        try
                        {
                            if (objs["planeimgext"].ToString() != "" && objs["planeimgext"].ToString() != "null")
                            {
                                try
                                {
                                    string relativepath = "\\Uploads\\Images\\planos\\";
                                    string imageExt = objs["planeimgext"].ToString();
                                    string absolutepathdir = Server.MapPath(relativepath);
                                    string filename = objs["_id"].ToString() + "." + imageExt;
                                    string fileabsolutepath = absolutepathdir + filename;

                                    if (imageExt != "" && System.IO.File.Exists(fileabsolutepath))
                                    {
                                        string url = Url.Content(relativepath + filename);
                                        objs.Add("ImgUrl2", url); //adding the image's url to the document
                                    }
                                }
                                catch (Exception ex) { }
                            }
                            else
                            {
                                objs.Add("ImgUrl2", "/Content/Images/planos/No-Imagen-Disponible-Ubicacion.png");
                            }
                        }
                        catch { }

                        JObject cad = new JObject();
                        try
                        {
                            cad = JsonConvert.DeserializeObject<JObject>(objs["profileFields"].ToString());
                        }
                        catch { }
                            string prof = "{";
                        foreach (KeyValuePair<string, JToken> token in cad)
                        {
                            try
                            {
                                String fieldsArray = _fieldsTable.Get("name", token.Key.Replace("_HTKField", ""));
                                if (fieldsArray == "[]") continue;
                                JArray fieldList = JsonConvert.DeserializeObject<JArray>(fieldsArray);
                                string etiqueta = "";
                                foreach (JObject obj in fieldList)
                                {
                                    etiqueta = obj["label"].ToString();
                                }
                                if (prof != "{") prof = prof + ",";
                                prof = prof + "\"" + etiqueta + "\":\"" + token.Value + "\"";
                            }
                            catch { }
                        }
                        prof += "}";

                        JObject ja = new JObject();
                        try
                        {
                            ja = JsonConvert.DeserializeObject<JObject>(prof);
                        }
                        catch { }
                        //BsonDocument b1 = new BsonDocument();
                        //foreach (var obj in ja)
                        //{
                        //    b1.Add(obj.Key, obj.Value.ToString());
                        //}
                        try
                        {
                            objs["profileFields"] = ja;
                        }
                        catch { }

                    }
                    //
                    return Json(JsonConvert.SerializeObject(objs));
                    //return new HtmlString(locationsTrs);

                }
                else {
                    return null;
                }
               
            }
            else
            {
                return null;
            }
        }

        public String getHardware(string locationid)
        {
            System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader("/Content/Config/Hardware/Motorola_MC3190/hardware_config.xml");

            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                string mihardware = "";
                if (locationid != null && locationid != "")
                {

                    string hardstring = _hardwareTable.GetLocatedHardware(locationid);
                    if (hardstring == "" || hardstring == null) return "";
                    JArray hardobj = JsonConvert.DeserializeObject<JArray>(hardstring);
                    string cadena = "";
                    string dirimg="";
                    foreach (JObject h1 in hardobj)
                    {
                        dirimg = "/Content/Images/planos/No-Imagen-Disponible.png";

                        if (h1["image"].ToString() != "")
                            dirimg = "/Content/Images/hardware/" + h1["hardware_reference"].ToString() + "." + h1["image"].ToString();

                        cadena = "<tr id='" + h1["_id"].ToString() + "'><td class='info'><div class='span12'>" +

                      "<div class='span5' ><img style='max-height: 50px;'  src='" + dirimg + "'></div>"
                       + "<div class='span7'><i class='hardwarename'>" + h1["hardwareName"].ToString() + "</i></div>"
                           + "</div></td>" +
                           "<td class='tools'><div>" +
                           "<button title='Configurar' class='confighardware btn blue btn-mini pull-right'  ><i class='icon-cog icon-white'></i></button></div><div>" +
                            "<button title='Borrar' class='removehardware btn red btn-mini pull-right' ><i class='icon-trash icon-white'></i></button>"
                           + "</div></td></tr>";


                        mihardware = mihardware + cadena;

                    }
                }

                return mihardware;

            }
            else
            {
                return null;
            }
        }


        public String getHardwareSelect()
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                string hardstring = _hardwareTable.GetRows();
                if (hardstring == "" || hardstring == null) return "";
                JArray hardobj = JsonConvert.DeserializeObject<JArray>(hardstring);
                string cadena = "";
                cadena += "<option value='null' selected> Ninguno</option>";
                foreach (JObject h1 in hardobj)
                {
                    try
                    {
                        if (h1["location_id"].ToString() == "") continue;
                    }
                    catch (Exception ex) {
                        cadena += "<option value='" + h1["_id"].ToString() + "' data-smart='" + h1["smart"].ToString() + "'>" + h1["name"].ToString() + "</option>"; 
                    }
                    
                }

                return cadena;

            }
            else
            {
                return null;
            }
        }

        public String saveHardware(String jsonString, String idHardware = null)
        {
            try
            {
                if (idHardware != null)
                {
                    String hardware = _hardwareTable.GetRow(idHardware);
                    JObject newhw = JsonConvert.DeserializeObject<JObject>(hardware);
                    JObject cadena = JsonConvert.DeserializeObject<JObject>(jsonString);
                    foreach ( KeyValuePair<string, JToken> cad in cadena) {
                        newhw[cad.Key] = cadena[cad.Key];
                    }

                    _hardwareTable.SaveRow(JsonConvert.SerializeObject(newhw), idHardware);

                    return "success";
                }
                else {
                    _hardwareTable.SaveRow(jsonString, idHardware);
                }
                
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }

        public String saveHardwareLocation(String id_hardware, String id_location)
        {
            try
            {
                if (id_hardware != "")
                {
                    String hardware = _hardwareTable.GetRow(id_hardware);
                    JObject newHardware = JsonConvert.DeserializeObject<JObject>(hardware);

                    var location = newHardware.GetValue("location_id");

                    if ( location == null || location.ToString() == "") 
                    {
                        newHardware["location_id"] = id_location;
                        _hardwareTable.SaveRow(newHardware.ToString(), id_hardware);

                        return "success";
                    }
                    else
                        return "error";
                    
                }

                return "error";
            }
            catch (Exception e)
            {
                return "error";
            }
        }

        /// <summary>
        /// Remove hardware from a control location
        /// </summary>
        /// <param name="id_hardware"></param>
        /// <param name="id_location"></param>
        /// <returns></returns>
        public String removeHardwareLocation(String id_hardware, String id_location)
        {
            try
            {
                if (id_hardware != "")
                {
                    String hardware = _hardwareTable.GetRow(id_hardware);
                    JObject newHardware = JsonConvert.DeserializeObject<JObject>(hardware);

                    var location = newHardware.GetValue("location_id");

                    if (location != null && location.ToString() == id_location)
                    {
                        newHardware["location_id"] = "";
                        _hardwareTable.SaveRow(newHardware.ToString(), id_hardware);
                    }

                    return "success";
                }

                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }

        public ActionResult saveLocationScenario(string id_location, string scenario)
        {
            bool edit = false;
            bool editClient = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("location", "c", dataPermissions);
            accessClient = validatepermissions.getpermissions("location", "c", dataPermissionsClient);
            edit = validatepermissions.getpermissions("location", "u", dataPermissions);
            editClient = validatepermissions.getpermissions("location", "u", dataPermissionsClient);

            if ((access == true && accessClient == true) || (edit == true && editClient == true))
            {
               if (this.Request.IsAjaxRequest())
               {
                   if (id_location == "")
                       return null;

                   /*Convert everything*/
                   var scenarioArray = JsonConvert.DeserializeObject<JArray>(scenario);
                   String location = locationTable.GetRow(id_location);
                   var newLocation = JsonConvert.DeserializeObject<JObject>(location);

                   newLocation["scenario"] = scenarioArray;
                   string relativepath = "\\Uploads\\Images\\planos\\";
                   string absolutepathdir = Server.MapPath(relativepath);
                   string filename= newLocation["_id"].ToString() + "." + newLocation["planeimgext"].ToString();
                   string fileabsolutepath = absolutepathdir + filename;
                   Images imagen = new Images(fileabsolutepath);

                   foreach (JObject obj in scenarioArray) {
                       try {
                           var croprect = (JObject)obj.GetValue("crop");
                           imagen.SetCoords(croprect["xcrop"].ToString(), croprect["ycrop"].ToString(), croprect["widthcrop"].ToString(), croprect["heightcrop"].ToString());
                           imagen.cropOK();

                           imagen.SaveImage(absolutepathdir + obj.GetValue("id_location") + ".jpg");
                           updateImageLocation(obj.GetValue("id_location").ToString(), "jpg");
                       }
                       catch (Exception ex) {
                           continue;
                       }
                        
                   }

                   locationTable.SaveRow(JsonConvert.SerializeObject(newLocation), id_location);
               }
               else
                   return this.Redirect("/Locations/Location");

               return null;
           }
           else { return null; }
        }

        public void copysetup(string nodeid, string setup, string oldsetup) {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                string nodo="", oldnodo="";

                List<BsonDocument> docs1 = locationTable.get("setupname", setup);
                foreach (BsonDocument location in docs1)
                {
                    nodo = location.GetElement("_id").Value.ToString();
                }

                docs1 = locationTable.get("setupname", oldsetup);
                foreach (BsonDocument location in docs1)
                {
                    oldnodo = location.GetElement("_id").Value.ToString();
                }

                List<BsonDocument> docs2 = locationTable.get("parent", oldnodo);
                List<BsonDocument> docs3 = locationTable.get("parent", nodeid);

                foreach (BsonDocument location in docs3)
                {
                    
                    if (docs2.Find(x => x.GetElement("name").Value.ToString() == location.GetElement("name").Value.ToString()) !=null)
                    {
                        deleteNode(location.GetElement("_id").Value.ToString());
                    }
                }

                copynodes(nodo, nodeid);
            }
        }

        public void copynodes(string oldparent, string newparent){
            List<BsonDocument> docs = locationTable.get("parent", oldparent); //getting all the users
            string id = "";
            foreach (BsonDocument location in docs)
            {
                String jsondata = "";
                jsondata += "{'name':'" + location["name"].ToString() + "','imgext':'','parent':'" + newparent + "','tipo':'" + location["tipo"].ToString() + "', 'setupname':'', 'setup' : '" +
                    location["setup"].ToString() + "', 'profileId':'" + location["profileId"].ToString() + "', 'creatorId':'" + this.Session["_id"] + "', 'profileFields': " + location["profileFields"].ToString() + "}";

                id = locationTable.SaveRow(jsondata);
                copynodes(location["_id"].ToString(),id);
            }
        
        }


        public ActionResult saveLocationMap(string id_location, string overlay)
        {
            if (this.Request.IsAjaxRequest())
            {
                if (id_location == "")
                    return null;

                /*Convert everything*/
                var mapArray = JsonConvert.DeserializeObject<JArray>(overlay);
                String location = locationTable.GetRow(id_location);
                var newLocation = JsonConvert.DeserializeObject<JObject>(location);

                newLocation["map"] = mapArray;

                locationTable.SaveRow(JsonConvert.SerializeObject(newLocation), id_location);
            }
            else
                return this.Redirect("/Locations/Location");

            return null;
        }

        //update image
        public void updateImageLocation(string id_location, string imgtext)
        {
            if (this.Request.IsAjaxRequest())
            {
                if (id_location != "") {
                    String location = locationTable.GetRow(id_location);
                    var newLocation = JsonConvert.DeserializeObject<JObject>(location);

                    newLocation["planeimgext"] = imgtext;

                    locationTable.SaveRow(JsonConvert.SerializeObject(newLocation), id_location);
                }

                
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

        /// <summary>
        /// Allows to get the route
        /// </summary>
        /// <param name="parentCategory"></param>
        /// <author>
        ///     Abigail Rodriguez Alvarez
        /// </author>
        /// <returns>Route of the tree</returns>
        public JsonResult getRoute(String parentCategory = "null")
        {
                //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {

                String actualCategory = locationTable.GetRow(parentCategory);
                 JObject actualCatObject = JsonConvert.DeserializeObject<JObject>(actualCategory);

                JObject categoryObject= new JObject();
                categoryObject.Add("id", actualCatObject["_id"].ToString());
                route.Add(categoryObject);
                parentCategory = actualCatObject["parent"].ToString();
            }

            JObject result = new JObject();
            result.Add("route", route);
            return Json( JsonConvert.SerializeObject(result)); 
        }

        //update parent
        public String updateParentLocation(string id, string newparent)
        {
            if (newparent == "") newparent = "null";
            if (this.Request.IsAjaxRequest())
            {
                if (id != "")
                {
                    String location = locationTable.GetRow(id);
                    var newLocation = JsonConvert.DeserializeObject<JObject>(location);
                    if(newparent==id){
                        return "No puede ser padre de sí mismo.";
                    }
                    newLocation["parent"] = newparent;

                    locationTable.SaveRow(JsonConvert.SerializeObject(newLocation), id);
                    return "success"; 
                }

            }

            return null;
        }

        public String loadParents(string nodeid)
        {
            try
            {
                String objectOptions = "";
                String rowArray = locationTable.GetRows();
                JArray parentList = JsonConvert.DeserializeObject<JArray>(rowArray);

                objectOptions += "<option value='null' selected> Ninguno</option>";

                foreach (JObject document in parentList) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["name"].ToString() != "")
                    {
                        objectOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                        objectOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                    }

                }


                return objectOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String SavePlanoFile(String selectedID, HttpPostedFileBase file)
        {
            String ext = null;
            String fileName = null;
            if (file != null)
            {
                ext = file.FileName.Split('.').Last(); //getting the extension
            }
            if (file != null)
            {
                string relativepath = "\\Uploads\\Images\\planos\\";
                string absolutepath = Server.MapPath(relativepath);
                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }
                fileName = selectedID + "." + ext;
                file.SaveAs(absolutepath + "\\" + fileName);

                if (selectedID != "")
                {
                    String location = locationTable.GetRow(selectedID);
                    var newLocation = JsonConvert.DeserializeObject<JObject>(location);

                    newLocation["planeimgext"] = ext;

                    locationTable.SaveRow(JsonConvert.SerializeObject(newLocation), selectedID);
                }
            }
            return fileName;
        }
    }

        
}

