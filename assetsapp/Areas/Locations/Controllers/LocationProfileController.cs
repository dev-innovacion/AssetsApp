using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RivkaAreas.Locations.Models;
using MongoDB.Bson;

using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rivka.Form;
using Rivka.Form.Field;
using Rivka.Security;
namespace RivkaAreas.Locations.Controllers
{
    [Authorize]
    public class LocationProfileController : Controller
    {
        //
        // GET: /Locations/LocationProfile/

        protected LocationProfileTable _locationprofileTable;
        protected LocationTable _locationTable;
        //protected CommonFunctions _common = new CommonFunctions();
        protected validatePermissions validatepermissions;

        public LocationProfileController()
        {
            this._locationprofileTable = new LocationProfileTable();
            this._locationTable = new LocationTable();
            //this._common = new CommonFunctions();
            validatepermissions = new validatePermissions();


        }

        //
        // GET: /UserProfile/

        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                 //TODO: advanced search using Agregation.
                 var locationsprofiles = _locationprofileTable.getRows();
                 ViewBag.locationTable = _locationTable;

                 return View(locationsprofiles);
             }
             else
             {
                 return Redirect("~/Home");
             }
        }
        /// <summary>
        ///     Insert a new profile, if is set the idProfile, make an update of that idProfile
        /// </summary>
        /// <param name="idProfile">
        ///     Id of the profile to update.
        /// </param>
        /// <returns>
        ///     Returns the view to create a new profile
        /// </returns>
        public ActionResult newProfile(string idProfile = null)
        {
               bool view = false;
               bool edit = false;
               bool editClient = false;
               bool viewClient = false;
               String dataPermissions = Session["Permissions"].ToString();
               String dataPermissionsClient = Session["PermissionsClient"].ToString();
               bool access = false;
               bool accessClient = false;
               //  access = getpermissions("users", "r");
               access = validatepermissions.getpermissions("profiles", "c", dataPermissions);
               accessClient = validatepermissions.getpermissions("profiles", "c", dataPermissionsClient);
               edit = validatepermissions.getpermissions("profiles", "u", dataPermissions);
               editClient = validatepermissions.getpermissions("profiles", "u", dataPermissionsClient);
               view = validatepermissions.getpermissions("profiles", "r", dataPermissions);
               viewClient = validatepermissions.getpermissions("profiles", "r", dataPermissionsClient);

               if (idProfile != null && (edit == false || editClient==false))
               {
                   access = false;
               }
               if (view == false || viewClient == false) { accessClient = false; access = false; }

               if (access == true && accessClient==true)
               {
                   CustomFieldsTable cft = new CustomFieldsTable("LocationCustomFields");

                   String fieldsArray = cft.GetRows();
                   JArray fields = JsonConvert.DeserializeObject<JArray>(fieldsArray);

                   if (idProfile != null && idProfile != "null" && idProfile != "" )
                   {
                           BsonDocument profile = _locationprofileTable.getRow(idProfile);
                           if (profile != null)
                           {
                               profile.Set("_id", profile.GetElement("_id").Value.ToString());
                               string profileJson = profile.ToJson();
                               ViewData["profile"] = new HtmlString(profileJson);
                           }
                   }

                   List<BsonDocument> profiles = _locationprofileTable.getRows();
                   ViewBag.profiles = profiles;

                   return View(fields);
               }
               else
               {
                   return Redirect("~/Home");
               }
        }

        /// <summary>
        ///     Deletes a profile and changes the user's idProfile to the specified
        ///     in the users param, by default all users will be changed to a "basic" profile
        /// </summary>
        /// <param name="idProfile">
        ///     A string with de idProfile to delete
        /// </param>
        /// <param name="users">
        ///     A json string with an array of users to change its idProfile
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public void deleteProfile(string idProfile , string locations)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                var locationList = JsonConvert.DeserializeObject<LocationList>(locations);//User Class already exists its used in Security Auth

                if (idProfile != null && idProfile != "null" &&  idProfile != "")
                {
                    BsonDocument profile = _locationprofileTable.getRow(idProfile);

                    if (profile != null)
                    {
                        foreach (var location in locationList.locations)
                        {
                            _locationTable.updateProfile(location);
                        }
                        _locationprofileTable.deleteProfile(idProfile);
                    }
                }
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
                BsonDocument profiles = _locationprofileTable.getRow(idProfile);
                if (profiles != null)
                {
                    profiles.Set("_id", profiles.GetElement("_id").Value.ToString());
                    string profileJson = profiles.ToJson();
                    return profileJson;
                }
            }
            return "";
        }

        [HttpPost]
        public ActionResult getLocationByProfile(string idProfile)
        {
            List<BsonDocument> locations = _locationTable.get("profileId", idProfile);
            List<BsonDocument> profiles = _locationprofileTable.getRows();
            ViewBag.canDelete = true;

            try
            {
                if (_locationprofileTable.getRow(idProfile).GetElement("name").Value.ToString() == "Básico")
                    ViewBag.canDelete = false;
            }
            catch (Exception e)
            {
                ViewBag.canDelete = false;
            }
            
            
            ViewBag.profiles = profiles;
            ViewBag.idProfile = idProfile;

            return View(locations);
        }

        public string saveProfile(string newProfile, string idProfile = null)
        {
            bool edit = false;
            bool editClient = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "c", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "c", dataPermissionsClient);
            edit = validatepermissions.getpermissions("profiles", "u", dataPermissions);
            editClient = validatepermissions.getpermissions("profiles", "u", dataPermissionsClient);
            if (idProfile != null && (edit == false || editClient==false))
             {
                accessClient=false;
                 access = false;
             }
             if (access == true && accessClient==true)
             {
                 string profile = newProfile;
                 idProfile = (idProfile != "" && idProfile != null && idProfile != "null") ? idProfile : null;

                 _locationprofileTable.saveRow(profile, idProfile);

                 return profile;
             }
             else { return null; }

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
        // <summary>
        //     Get a profile specified by the idProfile
        // </summary>
        // <param name="idProfile"></param>
        // <returns>
        //     Returns a Json text with the found profile
        // </returns>
        //[HttpPost]
        //public string getSubLocations(string idProfile, string idsubProfile)
        //{
        //    if (_common.isValidObjectId(idProfile))
        //    {
        //        BsonDocument profiles = _locationprofileTable.getRow(idProfile);
        //        if (profiles != null)
        //        {
        //            profiles.Set("_id", profiles.GetElement("_id").Value.ToString());
        //            string profileJson = profiles.ToJson();
        //            return profileJson;
        //        }
        //    }
        //    return "";
        //}
    }
}
