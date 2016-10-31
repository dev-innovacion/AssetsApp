using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RivkaAreas.User.Models;

using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Security;

using Rivka.Form.Field;
using Rivka.Security;
namespace RivkaAreas.User.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        protected ProfileTable _profileTable;
        protected UserTable _userTable;
        //protected CommonFunctions _common = new CommonFunctions();
        protected validatePermissions validatepermissions;

        public UserProfileController()
        {
            this._profileTable = new ProfileTable();
            this._userTable = new UserTable();
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
            
            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                //TODO: advanced search using Agregation.
                var profiles = _profileTable.getRows();
                ViewBag.userTable = _userTable;

                return View(profiles);
            }
            else
            {

                return Redirect("~/Home");
            }
        }

        /// <summary>
        /// Gets all the profiles as a JsonResult
        /// </summary>
        /// <returns></returns>
        public JsonResult GetProfilesTable()
        {
            if (Request.IsAjaxRequest())
            {
                string raw_profiles = _profileTable.getJoinRows();
               JArray profiles = JsonConvert.DeserializeObject<JArray>(raw_profiles);

                foreach (JObject profile in profiles)
                {
                    profile.Remove("customFields");
                    profile.Remove("permissionsHTK");
                    //Count the number of users with the specified profileId
                    profile["numUsers"] = JsonConvert.DeserializeObject<JArray>(_userTable.Get("profileId", profile["_id"].ToString())).Count;
                    
                }

                return Json(JsonConvert.SerializeObject(profiles));
            }            

            return Json("[]");
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


           
            bool edit = false;
            bool editclient=false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
          //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            edit = validatepermissions.getpermissions("profiles", "u", dataPermissions);

            accessClient = validatepermissions.getpermissions("profiles", "r", dataPermissionsClient);
            editclient = validatepermissions.getpermissions("profiles", "u", dataPermissionsClient);
    
            
           
            if (Profile != null && (edit == false || editclient==false))
            {
                access = false;
                accessClient = false;
            }

            if (access == true && accessClient==true)
            {
                CustomFieldsTable cft = new CustomFieldsTable("CustomFields");
                String fieldsArray = cft.GetRows();
                JArray fields = JsonConvert.DeserializeObject<JArray>(fieldsArray);

                if (idProfile != null && idProfile != "null" && idProfile != "")
                {
                    BsonDocument profile = _profileTable.getRow(idProfile);
                    if (profile != null)
                    {
                        profile.Set("_id", profile.GetElement("_id").Value.ToString());
                        try
                        {
                            profile.Set("CreatedTimeStamp", profile.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex)
                        {

                        }
                        string profileJson = profile.ToJson();
                        ViewData["profile"] = new HtmlString(profileJson);
                    }
                }

                List<BsonDocument> profiles = _profileTable.getRows();
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
        public void deleteProfile(string idProfile, string users)
        {
            bool access = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
           
            bool accessClient = false;

            access = validatepermissions.getpermissions("profiles", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                var userList = JsonConvert.DeserializeObject<UserList>(users);//User Class already exists its used in Security Auth

                if (idProfile != "null" && idProfile != null && idProfile != "")
                {
                    BsonDocument profile = _profileTable.getRow(idProfile);

                    if (profile != null)
                    {
                        foreach (var user in userList.users)
                        {
                            _userTable.updateProfile(user);
                        }
                        _profileTable.deleteProfile(idProfile);
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
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (idProfile != null && idProfile != "null" && idProfile != "")
                {
                    BsonDocument profiles = _profileTable.getRow(idProfile);
                    if (profiles != null)
                    {
                        profiles.Set("_id", profiles.GetElement("_id").Value.ToString());
                        string profileJson = profiles.ToJson();
                        return profileJson;
                    }
                }
                return "";
            }
            else { return ""; }
        }

        [HttpPost]
        public ActionResult getUserByProfile(string idProfile)
        {
            List<BsonDocument> users = _userTable.get("profileId", idProfile);
            List<BsonDocument> profiles = _profileTable.getRows();
            ViewBag.canDelete = true;

            try
            {
                if (_profileTable.getRow(idProfile).GetElement("name").Value.ToString() == "Básico")
                    ViewBag.canDelete = false;
            }
            catch (Exception e)
            {
                ViewBag.canDelete = false;
            }


            ViewBag.profiles = profiles;
            ViewBag.idProfile = idProfile;

            return View(users);
        }

        /// <summary>
        ///     Saves a new profile, if idProfile is not null, then edits that one.
        /// </summary>
        /// <param name="newProfile"></param>
        /// <param name="idProfile"></param>
        /// <returns>
        ///     
        /// </returns>
        public string saveProfile(string newProfile, string idProfile = null)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("profiles", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                idProfile = (_profileTable.getRow(idProfile) == null) ? null : idProfile;
                idProfile = _profileTable.SaveRow(newProfile, idProfile);

                return idProfile;
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

    }
}
