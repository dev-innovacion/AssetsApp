using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RivkaAreas.Locations.Models;
using Newtonsoft.Json;
using Rivka.Images;
using Rivka.Security;
using Rivka.Form;
using Rivka.Form.Field;
using Rivka.Mail;
using MongoDB.Bson;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
namespace RivkaAreas.Locations.Controllers
{
    public class LocationTablesController : Controller
    {
        //
        // GET: /Locations/LocationTables/
        protected LocationProfileTable _profileTable; //profile's model object
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        protected LocationTable _locationTable;
        protected UserTable _userTable;
        protected validatePermissions validatepermissions;
        protected Notifications Notificate;
        protected ValidateLimits validatelim;
        protected AdjudicatingTable adjudicatingTable;
        protected ObjectReal objectReal;
        protected userProfileTable userProfile;

        public LocationTablesController()
        {
            _profileTable = new LocationProfileTable();
            _locationTable = new LocationTable();
            _userTable = new UserTable();
            validatepermissions = new validatePermissions();
            Notificate = new Notifications();
            validatelim = new ValidateLimits();
            adjudicatingTable = new AdjudicatingTable();
            objectReal = new ObjectReal();
            userProfile = new userProfileTable();
            _logTable = new LogBook.Controllers.LogBookController();
        }

        public String loadprofiles()
        {

            String profileOptions = "";
            String profileList = _profileTable.GetRows();

            profileOptions += "<option value='null' selected> Ninguno</option>";

            try
            {
                JArray users = JsonConvert.DeserializeObject<JArray>(profileList);

                foreach (JObject document in users) //for each profile we create an option element with id as value and the name as the text
                {
                    profileOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                    profileOptions += ">" + document["name"] + " " + document["lastname"] + "</option>"; //setting the text as the name
                }
            }
            catch { }
            return profileOptions;
        }

        public string loadsetups()
        {

            try
            {
                String setupOptions = "";
                String setupList = _locationTable.GetRows(); //getting all the profiles
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

        public ActionResult Index()
        {
            ViewData["profileList"] = loadprofiles();
            return View();
        }

        public String getFormView(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    BsonDocument document = _profileTable.getRow(profile);
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

        public String getFormTitlesView(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    BsonDocument document = _profileTable.getRow(profile);
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

        public JsonResult getLocationTable(string userid,string type)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string newJoin = "";
                string getconjunt = _profileTable.Get("name", "Conjunto");
                JArray conjuntja = new JArray();
                string idconjunto = "";
                string subloc = _profileTable.Get("name", "Sub-Ubicaciones");
                JArray sublocja = new JArray();
                string idsubloc = "";
                string idregion="";
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idconjunto = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }
                try
                {
                    sublocja = JsonConvert.DeserializeObject<JArray>(subloc);
                    idsubloc = (from mov in sublocja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }
                getconjunt=_profileTable.Get("name", "Region");
                 try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idregion = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                 JArray locatList = new JArray();
                 JObject locat=new JObject();
                 JArray ele = new JArray();
                 String rowArray;
                 List<String> list1 = new List<String>();
                 List<String> list2 = new List<String>();
                //******************************************************************************
                 foreach (JObject ob in locats)
                 {
                     rowArray = _locationTable.Get("parent", ob["id"].ToString());
                     locatList = JsonConvert.DeserializeObject<JArray>(rowArray);

                     foreach (JObject ob1 in locatList)
                     {
                         list1.Add(ob1["_id"].ToString());
                     }
                     rowArray = _locationTable.GetRow(ob["id"].ToString());
                     locat = JsonConvert.DeserializeObject<JObject>(rowArray);
                     if (locat["profileId"].ToString() != idregion)
                     {
                         string profileid = locat["profileId"].ToString();
                         string parent = ob["id"].ToString();
                         while (profileid != idregion && profileid != "" && parent!="null")
                         {
                             try
                             {
                                 string category = _locationTable.GetRow(parent);
                                 JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                                 list1.Add(actualCategory["_id"].ToString());
                                 profileid = actualCategory["profileId"].ToString();
                                 parent = actualCategory["parent"].ToString();
                             }
                             catch
                             {
                                 profileid = "";
                             }

                         }
                     }

                 }
                 

                if (type == "")
                {
                    //newJoin = _locationTable.GetRows();
                    newJoin = _locationTable.GetLocationsTable(userid, "", idregion, idconjunto,idsubloc,list1);
                }
                else {

                    newJoin = _locationTable.GetLocationsTable("profileId", type, idregion, idconjunto, idsubloc,list1);
                }
                 
                String rows = "";
                JArray objects = JsonConvert.DeserializeObject<JArray>(newJoin);
                JArray objects1 = new JArray();
                foreach (JObject obj in objects)
                {
                    userstring = _userTable.GetRow(obj["Creator"].ToString());
                    userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                    obj["Creator"] = userobj["name"].ToString() + " " + userobj["lastname"].ToString();

                    objects1.Add(obj);
                }


                rows = JsonConvert.SerializeObject(objects1);
                return Json(rows);

            }
            return null;
        }

        public JsonResult getLocationTableAdmin(string type)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                string newJoin = "";
                string getconjunt = _profileTable.Get("name", "Conjunto");
                string getsubloc = _profileTable.Get("name", "Sub-Ubicaciones");
                JArray conjuntja = new JArray();
                JArray sublocja = new JArray();
                string idconjunto = "";
                string idregion = "";
                 string idsubloc = "";

                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idconjunto = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                } 

                
                catch (Exception ex) { }
                  try
                {
                    sublocja = JsonConvert.DeserializeObject<JArray>(getsubloc);
                    idsubloc = (from mov in sublocja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }
                getconjunt = _profileTable.Get("name", "Region");
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idregion = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                //******************************************************************************
                if (type == "")
                {
                    //newJoin = _locationTable.GetRows();
                    newJoin = _locationTable.GetLocationsTable("", "", idregion, idconjunto,idsubloc);
                }
                else if (type == "SubLocations")
                {
                    newJoin = _locationTable.Get("profileId", idsubloc);
                }
                else
                {

                    newJoin = _locationTable.GetLocationsTable("profileId", type, idregion, idconjunto, idsubloc);
                }

                String rows = "";
                JArray objects = JsonConvert.DeserializeObject<JArray>(newJoin);
              
                JArray objects1 = new JArray();
               
                foreach (JObject obj in objects)
                {
                    try {
                        String userstring = _userTable.GetRow(obj["Creator"].ToString());
                        JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                        obj["Creator"] = userobj["name"].ToString() + " " + userobj["lastname"].ToString();
                    }
                    catch {
                        
                        obj["Creator"] =  " ";
                    }

                    string nameparent = "";
                    obj.Add("nameparent", "");
                    try
                    {
                        nameparent = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(obj["parent"].ToString()))["name"].ToString();
                        obj["nameparent"] = nameparent;
                    }
                    catch { }
                

                    if(type == "Conjunto"){
                        try
                        {
                            string regionString = _locationTable.GetRow(obj["parent"].ToString());
                            JObject region = JsonConvert.DeserializeObject<JObject>(regionString);
                            obj["region"] = region["name"];
                        }
                        catch (Exception ex) { }
                    }

                    if (type == "") {
                        if (obj["parent"].ToString() == "none") {
                            objects1.Add(obj);
                        }
                    }
                    else
                    {
                        objects1.Add(obj);
                    }
                }


                rows = JsonConvert.SerializeObject(objects1);
                return Json(rows);

            }
            return null;
        }

        public void deleteNode(string selectedID, bool fromRecursiveCall = false)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            //bool access = false;
            //bool accessClient = false;
            ////  access = getpermissions("users", "r");
            //access = validatepermissions.getpermissions("location", "d", dataPermissions);
            //accessClient = validatepermissions.getpermissions("location", "d", dataPermissionsClient);

            //if (access == true && accessClient == true)
            //{
                if (this.Request.IsAjaxRequest()) //only available with AJAX
                {
                    string locats = _locationTable.Get("parent", selectedID);
                    JArray conjuntja = new JArray();
                    JArray objects = JsonConvert.DeserializeObject<JArray>(locats);
                    foreach (JObject nod in objects)
                    {
                        deleteNode(nod["_id"].ToString(), true);
                    }

                    String node = _locationTable.GetRow(selectedID);
                    JObject objs = JsonConvert.DeserializeObject<JObject>(node);
                    _locationTable.DeleteRow(objs["_id"].ToString());
                    string locationName = objs["name"].ToString();
                    //if (!fromRecursiveCall)
                    //    Notificate.saveNotification("Locations", "Delete", "La ubicación '" + locationName + "' fue eliminada");
                }
        //    }
        }

        public JArray getRoute3(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {
                try
                {
                    string category = _locationTable.GetRow(parentCategory);
                    JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                    route.Add(actualCategory["_id"].ToString());
                    parentCategory = actualCategory["parent"].ToString();
                }
                catch
                {
                    parentCategory = "";
                }

            }

            return route;
        }

        public String getLocation(String locationID)
        {
            if (this.Request.IsAjaxRequest())
            {
                try 
                {
                    String doc = _locationTable.GetRow(locationID); //getting the user's data 
                    string result = "";
                    JObject object1 = JsonConvert.DeserializeObject<JObject>(doc);
                    //the next is the photo's information
                    string relativepath="";
                    string absolutepathdir="";
                    string filename="";
                    string fileabsolutepath="";
                    try{
                    if (object1["planeimgext"] != null){
                        if (object1["planeimgext"].ToString() != "null")
                        {
                            relativepath = "\\Uploads\\Images\\planos\\";
                            absolutepathdir = Server.MapPath(relativepath);
                            filename = object1["_id"].ToString() + "." + object1["planeimgext"].ToString();
                            fileabsolutepath = absolutepathdir + filename;
                            if (System.IO.File.Exists(fileabsolutepath))
                            {
                                string url = Url.Content(relativepath + filename);
                                object1.Add("ImgUrl2", url); //adding the image's url to the document
                            }

                        }

                        //the next is the photo's information
                        relativepath = "\\Uploads\\Images\\Locations\\";
                        absolutepathdir = Server.MapPath(relativepath);
                        filename = object1["_id"].ToString() + "." + object1["imgext"].ToString();
                        fileabsolutepath = absolutepathdir + filename;

                        if (doc == null)
                            return "null";
                        object1.Remove("_id");


                        if (System.IO.File.Exists(fileabsolutepath))
                        {
                            string url = Url.Content(relativepath + filename);
                            object1.Add("ImgUrl", url); //adding the image's url to the document
                        }
                    }
                }catch{ }
                    //Check Parent relation
                    JObject nameprofile = new JObject();
                    try
                    {
                        nameprofile = JsonConvert.DeserializeObject<JObject>(_profileTable.GetRow(object1["profileId"].ToString()));
                    }
                    catch { }
                    if (nameprofile["name"].ToString() == "Region") {
                        try{
                        object1.Add("case", "region");
                        //GET NAME GERENTE
                        JArray gerenteRegional = JsonConvert.DeserializeObject<JArray>(userProfile.Get("name", "Gerente regional"));
                        string regionID = (from mov in gerenteRegional select (string)mov["_id"]).First().ToString();
                        JArray users = JsonConvert.DeserializeObject<JArray>(_userTable.Get("profileId", regionID));
                        foreach (JObject user in users) {
                            try
                            {
                                JArray locations = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(user["userLocations"]));
                                foreach (JObject location in locations) {
                                    if (location["id"].ToString() == locationID) {
                                        object1.Add("Gerente", user["name"].ToString() + " " + user["lastname"].ToString());
                                        object1.Add("GerenteEmail", "Correo: " + user["email"].ToString());
                                    }
                                }
                            }catch(Exception ex){}
                        }

                        JArray dictamin = JsonConvert.DeserializeObject<JArray>(adjudicatingTable.GetRows());
                        foreach (JObject dic in dictamin) {
                            try
                            {
                                JObject type = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(dic["type"]));
                                JObject user = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(dic["user"]));
                                JObject location = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(dic["location"]));

                                if (type["value"].ToString() == "maintenance" && locationID == location["value"].ToString())
                                {
                                    JObject userinfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(user["value"].ToString()));
                                    object1.Add("GerenteResponsable", userinfo["name"] + " " + userinfo["lastname"]);
                                    object1.Add("GerenteResponsableEmail", "Correo: " + userinfo["email"]);
                                }
                                else if (type["value"].ToString() == "system" && location["value"].ToString() == "")
                                {
                                    JObject userinfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(user["value"].ToString()));
                                    object1.Add("GerenteSystem", userinfo["name"] + " " + userinfo["lastname"]);
                                    object1.Add("GerenteSystemEmail", "Correo: " + userinfo["email"]);
                                }
                                else if (type["value"].ToString() == "sound" && location["value"].ToString() == "")
                                {
                                    JObject userinfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(user["value"].ToString()));
                                    object1.Add("GerenteSound", userinfo["name"] + " " + userinfo["lastname"]);
                                    object1.Add("GerenteSoundEmail", "Correo: " + userinfo["email"]);
                                }
                            }
                            catch (Exception ex) { }
                        }
                    }catch{ }
                    }
                    else if (nameprofile["name"].ToString() == "Conjunto")
                    {
                        try{
                        object1.Add("case", "conjunto");
                        JObject thisInfo = new JObject();

                        try
                        {
                            thisInfo = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(locationID));
                            JObject path = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(thisInfo["parent"].ToString()));
                            object1.Add("ParentName", path["name"]);
                        }
                        catch (Exception ex) { }

                        //Get Geretnes && Directivos && Address info
                        JArray users = JsonConvert.DeserializeObject<JArray>(_userTable.GetRows());
                        JArray personal = new JArray();
                        foreach (JObject user in users)
                        {
                            JArray locations = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(user["userLocations"]));
                            foreach (JObject location in locations)
                            {
                                if (location["id"].ToString() == locationID || thisInfo["parent"].ToString() == location["id"].ToString())
                                {
                                    JObject perfileUser = JsonConvert.DeserializeObject<JObject>(userProfile.GetRow(user["profileId"].ToString()));
                                    JObject data = new JObject();

                                    switch (perfileUser["name"].ToString())
                                    {
                                        case "Gerente de conjunto":
                                            data.Add("label", "HTKFieldger_conj");
                                            break;
                                        case "Gerente regional":
                                            data.Add("label", "HTKFieldGer_reg");
                                            break;
                                        case "Gerente de operaciones":
                                            data.Add("label", "HTKFieldGer_oper");
                                            break;
                                        case "Gerente de auditoría":
                                            data.Add("label", "HTKFieldGer_audi");
                                            break;
                                        case "Gerente AM":
                                            data.Add("label", "HTKFieldGer_AM");
                                            break;
                                        case "Gerente GL":
                                            data.Add("label", "HTKFieldGer_GL");
                                            break;
                                        case "Gerente logística":
                                            data.Add("label", "HTKFieldGer_log");
                                            break;
                                        case "Gerente compras":
                                            data.Add("label", "HTKFieldGer_com");
                                            break;
                                        case "Gerente de TI":
                                            data.Add("label", "HTKFieldGer_tec");
                                            break;
                                        case "Gerente construcción":
                                            data.Add("label", "HTKFieldGer_cons");
                                            break;
                                        case "Gerente remodelación":
                                            data.Add("label", "HTKFieldGer_rem");
                                            break;
                                        case "Gerente de mantenimiento":
                                            data.Add("label", "HTKFieldGer_mantto");
                                            break;
                                        case "Supervisor de región":
                                            data.Add("label", "HTKFieldsup_reg");
                                            break;
                                        case "Auditor":
                                            data.Add("label", "HTKFieldauditores");
                                            break;
                                        case "CFO":
                                            data.Add("label", "HTKFieldc_f_o");
                                            break;
                                        case "COO":
                                            data.Add("label", "HTKFieldc_o_o");
                                            break;
                                        case "Analista contable":
                                            data.Add("label", "HTKFieldanal_cont");
                                            break;
                                        case "Integrador":
                                            data.Add("label", "HTKFieldint_");
                                            break;
                                        case "Staff administrativo construcción":
                                            data.Add("label", "HTKFieldstaff_admin");
                                            break;
                                        case "Coordinador de mantenimiento":
                                            data.Add("label", "HTKFieldcoord_mant");
                                            break;
                                        case "Administrador de logística":
                                            data.Add("label", "HTKFieldadminis");
                                            break;
                                        case "Gerente inventario":
                                            data.Add("label", "HTKFieldGer_inv");
                                            break;
                                        case "Director general":
                                            data.Add("label", "HTKFieldDir_genL");
                                            break;
                                        case "Director contraloría":
                                            data.Add("label", "HTKFieldDir_contrL");
                                            break;
                                        case "Director auditoría":
                                            data.Add("label", "HTKFieldDir_audiL");
                                            break;
                                        case "Director contabilidad":
                                            data.Add("label", "HTKFieldDir_contL");
                                            break;
                                        case "Director ICA":
                                            data.Add("label", "HTKFieldDir_icaL");
                                            break;
                                        case "Director compras":
                                            data.Add("label", "HTKFieldDir_compL");
                                            break;
                                        case "Director de sistemas":
                                            data.Add("label", "HTKFieldDir_sistL");
                                            break;
                                        case "Director construcción":
                                            data.Add("label", "HTKFieldDir_construL");
                                            break;
                                        case "Director operaciones":
                                            data.Add("label", "HTKFieldDir_opL");
                                            break;
                                    }
                                    data.Add("name", user["name"].ToString() + " " + user["lastname"].ToString());
                                    data.Add("email", user["email"].ToString());
                                    personal.Add(JsonConvert.SerializeObject(data));
                                }
                            }
                        }
                        object1.Add("personal", JsonConvert.SerializeObject(personal));

                        JObject address = new JObject();
                        JObject locationInfo = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(locationID));
                        try
                        {
                            address.Add("HTKFieldcalle", locationInfo["_HTKFieldcalle"]);
                            address.Add("HTKFieldcolonia", locationInfo["_HTKFieldcolonia"]);
                            address.Add("HTKFieldMunicipio", locationInfo["_HTKFieldMunicipio"]);
                            address.Add("HTKFieldciudad", locationInfo["_HTKFieldciudad"]);
                            address.Add("HTKFieldzipcode", locationInfo["_HTKFieldzipcode"]);
                            address.Add("HTKFieldestado", locationInfo["_HTKFieldestado"]);
                            address.Add("HTKFieldpais", locationInfo["_HTKFieldpais"]);

                            object1.Add("address", JsonConvert.SerializeObject(address));
                        }
                        catch {  }
                    }catch{ }
                    }

                    JArray LocationsADD = JsonConvert.DeserializeObject<JArray>(_locationTable.Get("parent",locationID));
                    JArray Loc = new JArray();
                    foreach (JObject location in LocationsADD) {
                        Loc.Add(location);
                    }
                    object1.Add("location", JsonConvert.SerializeObject(Loc));

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

        public String loadParents(string nodeid)
        {
            try
            {
                String objectOptions = "";
                String rowArray = _locationTable.GetRows();
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
        public String Assign(string idlocation, String sublocations,string name,string number,string idoffice=null)
        {
            try
            {
                JObject result = new JObject();
                JArray subsja = new JArray();
                try{
                    JObject office=JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(idoffice));
                    idoffice=office["_id"].ToString();
                   
                }catch{  idoffice="";}
                if (idoffice == null || idoffice == "")
                {
                    if (name == "" || number == "")
                    {
                        result.Add("status", "Ingrese el  nombre y unidad de explotacion para la oficina");
                        return JsonConvert.SerializeObject(result);
                    }
                    JObject newoffice=new JObject();
                    newoffice.Add("name",name);
                    newoffice.Add("number",number);
                   

                    newoffice.Add("tipo","1");
                    newoffice.Add("parent","none");
                    try
                    {
                     JObject prof=JsonConvert.DeserializeObject<JArray>(_profileTable.Get("name","Conjunto")).First() as JObject;
                     newoffice.Add("profileId",prof["_id"].ToString());
                    }
                    catch { }
                    idoffice = _locationTable.SaveRow(JsonConvert.SerializeObject(newoffice));
                }
                result.Add("idoffice", idoffice);
                try
                {
                    JObject location = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(idlocation));
                    idlocation = location["_id"].ToString();
                    if (location["parent"].ToString() == "none")
                    {
                        location["parent"] = idoffice;
                        location.Remove("_id");  
                        idlocation = _locationTable.SaveRow(JsonConvert.SerializeObject(location));
                    }
                }
                catch
                {

                }
                result.Add("idlocation", idlocation);
                try
                {

                    subsja = JsonConvert.DeserializeObject<JArray>(sublocations);
                    foreach (var sub in subsja)
                    {
                        try
                        {
                            JObject subjo = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(sub.ToString()));
                            subjo["parent"] = idlocation;
                            subjo.Remove("_id");
                            string idsub = _locationTable.SaveRow(JsonConvert.SerializeObject(subjo));
                        
                        }
                        catch { }
                    }
                }
                catch { }
                result.Add("status", "success");
                  return JsonConvert.SerializeObject(result); 
            }
            catch
            {
                return "Error";

            }
        }
        public String saveLocation(FormCollection formData, HttpPostedFileBase file, String subs=null, String address = null)
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

                if (this.Request.IsAjaxRequest())
                {
                    formData = CustomForm.unserialize(formData);  //use the method serialize to parse the string into an array
                    String locationID = (formData["locationID"] == "null") ? null : formData["locationID"]; //is this an insert or an update?, converting null in javascript to null in c#


                    String[] sublocations = subs.Split(',');
                    String[] addressArray = address.Split(',');

                    dynamic location = _locationTable.GetRow(locationID);
                    String doc = _locationTable.GetRow(locationID);
                    JObject docobj = new JObject();
                    if (locationID != null) docobj = JsonConvert.DeserializeObject<JObject>(doc);
                    /*the gived id does not exists*/
                    if (locationID != null && (location == null))
                    {
                        return "{\"status\":\"error\", \"msg\":\"El id especificado no existe\"}";
                    }
                    if (locationID == null && formData["tipo"] != "2")
                    {
                        limit = validateLimit();
                    }
                    if (limit == "true")
                    {

                        /*there is no profile with the gived id*/
                        if (formData["profileSelect"] != "null")
                        {
                            if (_profileTable.GetRow(formData["profileSelect"]) == null)
                            {
                                return "{\"status\":\"error\", \"msg\":\"El perfil especificado no existe\"}";
                            }
                        }


                        /*The selected location name is already in use and is not the user who has it*/
                        if (locationExists(formData["name"]) == "true" && (locationID == null || _locationTable.get("name", formData["name"])[0].GetElement("_id").Value.ToString() != locationID))
                        {
                            JArray locationData = JsonConvert.DeserializeObject<JArray>(_locationTable.Get("name", formData["name"]));
                            Boolean StatusLocationExist = false;
                            foreach (JObject loc in locationData) {
                                if (loc["profileId"].ToString().Trim() == formData["profileSelect"].ToString().Trim())
                                {
                                    StatusLocationExist = true;
                                    break;
                                }
                            }
                            if(StatusLocationExist) return "{\"status\":\"error\", \"msg\":\"La ubicación ya está siendo utilizado\"}";
                        }

                        if (numberExists(formData["number"], locationID) == "true") {

                            JArray locationData = JsonConvert.DeserializeObject<JArray>(_locationTable.Get("number", formData["number"]));
                            Boolean StatusLocationExist = false;
                            foreach (JObject loc in locationData)
                            {
                                if (loc["profileId"].ToString().Trim() == formData["profileSelect"].ToString().Trim())
                                {
                                    StatusLocationExist = true;
                                    break;
                                }
                            }
                            if (StatusLocationExist) return "{\"status\":\"error\", \"msg\":\"El número de indentificación ya existe\"}";
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

                        /*JObject profileSection = JsonConvert.DeserializeObject<JObject>(_profileTable.GetRow(formData["profileSelect"].ToString());
                        if (profileSection["name"].ToString() == "Conjunto" && (locationID != null || locationID != "")) {
                            JObject locationSection = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(locationID));
                            formData["region"] = locationSection["region"].ToString();
                        }*/

                        /* Format validations */
                        //if (!Regex.IsMatch(formData["name"], "([a-záéíóúñA-ZÁÉÍÓÚÑ0-9-_.]){4,}"))
                        //{
                        //    return "Formato incorrecto para: name";
                        //}

                        String nombre = formData["name"].Replace("+", " ");
                        //there are fields that we know that exists so we set them into the json
                        String jsonData = "{'name':'" + formData["name"].Replace("+", " ") + "'";//,'imgext':'" + ext
                            //+ "','planeimgext':'null'";

                        if (formData["number"] != "")
                        {
                            jsonData = jsonData + ",'number':'" + formData["number"] + "'";
                        }
                        /*if (formData["region"] != "null")
                        {
                            jsonData = jsonData + ",'region':'" + formData["region"] + "'";
                        }
                        if (formData["conjunto"] != "null")
                        {
                            jsonData = jsonData + ",'conjunto':'" + formData["conjunto"] + "'";
                        }*/

                        if (formData["description"] != "")
                        {
                            jsonData = jsonData + ",'description':'" + formData["description"] + "'";
                        }
                        else if (formData["description1"] != "")
                        {
                            jsonData = jsonData + ",'description':'" + formData["description1"] + "'";
                        }

                        //Check Parent relation
                        JObject nameprofile = JsonConvert.DeserializeObject<JObject>(_profileTable.GetRow(formData["profileSelect"].ToString()));
                        string parentValue = formData["parentSelect"];
                        if ((nameprofile["name"].ToString() == "Conjunto" || nameprofile["name"].ToString() == "Ubicacion" || nameprofile["name"].ToString() == "Sub-Ubicaciones") && locationID == null)
                        {
                            parentValue = "none";
                        }
                        parentValue = (parentValue == null || parentValue == "" ? "null" : parentValue);

                        jsonData = jsonData + ",'parent':'" + parentValue + "','tipo':'" + formData["tipo"] + "', 'setupname':'', 'setup' : '', 'profileId':'" + formData["profileSelect"]
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
                        formData.Remove("parentSelect");

                        formData.Remove("number");
                        formData.Remove("region");
                        formData.Remove("conjunto");
                        formData.Remove("description");
                        formData.Remove("description1");

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
                        jsonData += "}";

                        //GET Address
                        if (address != null && nameprofile["name"].ToString() == "Conjunto")
                        {
                            jsonData += ",";
                            int cant = 0;
                            string[] place = new string[] { "_HTKFieldcalle", "_HTKFieldcolonia", "_HTKFieldMunicipio", "_HTKFieldciudad", "_HTKFieldzipcode", "_HTKFieldestado", "_HTKFieldpais" };
                            foreach (String adr in addressArray)
                            {
                                jsonData += "'" + place[cant] + "':'" + adr + "'";

                                cant++;
                                if (cant < sublocations.Length)
                                {
                                    jsonData += ", ";
                                }
                            }
                        }
                        jsonData += "}}";

                        //now that we have the json and we know the data is ok, let's save it
                        string id = _locationTable.SaveRow(jsonData, locationID);

                        //Notificate the action
                        if (locationID == null)
                        {
                            Notificate.saveNotification("Locations", "Create", "La ubicación '" + locationName + "' ha sido creada");
                            _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Insert: " + nombre, "Locations", DateTime.Now.ToString());
                        }
                        else {
                            Notificate.saveNotification("Locations", "Update", "La ubicación '" + locationName + "' ha sido modificada");
                            _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Update: " + nombre, "Locations", DateTime.Now.ToString());
                        }

                        try
                        {
                            //Delete old relations
                            if (locationID != null)
                            {
                                JArray locationrelation = JsonConvert.DeserializeObject<JArray>(_locationTable.Get("parent", locationID));
                                foreach (JObject locationr in locationrelation)
                                {
                                    if (nameprofile["name"].ToString() == "Conjunto" )
                                    {
                                        string realObjString = objectReal.Get("location", locationr["_id"].ToString());
                                        if ((realObjString == "" || realObjString == "[]") && !sublocations.Contains(locationr["_id"].ToString()) && !sublocations.Contains("all"))
                                        {
                                            _locationTable.DeleteRow(locationr["_id"].ToString());
                                            _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Delete: " + locationr["name"].ToString(), "Locations", DateTime.Now.ToString());
                                        }
                                    }
                                    else
                                    {
                                        locationr["parent"] = "none";
                                        _locationTable.SaveRow(JsonConvert.SerializeObject(locationr), locationr["_id"].ToString());
                                        _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Update: " + locationr["name"].ToString(), "Locations", DateTime.Now.ToString());
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { }

                        //cambiar padre
                        if (subs != null)
                        {
                            foreach (String sub in sublocations)
                            {
                                if (sub != "all")
                                {
                                    try
                                    {
                                        String substrin = _locationTable.GetRow(sub);
                                        JObject subobj = JsonConvert.DeserializeObject<JObject>(substrin);

                                        if (nameprofile["name"].ToString() == "Conjunto")
                                        {
                                            if (subobj["parent"].ToString() == "none")
                                            {
                                                subobj["parent"] = id;
                                                subobj.Remove("_id");
                                                _locationTable.SaveRow(JsonConvert.SerializeObject(subobj));
                                                _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Insert: " + subobj["name"].ToString(), "Locations", DateTime.Now.ToString());
                                            }
                                        }
                                        else
                                        {
                                            subobj["parent"] = id;
                                            _locationTable.SaveRow(JsonConvert.SerializeObject(subobj), subobj["_id"].ToString());
                                            _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Update: " + subobj["name"].ToString(), "Locations", DateTime.Now.ToString());
                                        }
                                    }
                                    catch (Exception ex) { }
                                }
                                else if (sub == "all")
                                {
                                    string perfil = "";
                                    if (nameprofile["name"].ToString() == "Conjunto")
                                    {
                                        perfil = "Ubicacion";
                                    }
                                    else if (nameprofile["name"].ToString() == "Region")
                                    {
                                        perfil = "Conjunto";
                                    }
                                    else if (nameprofile["name"].ToString() == "Ubicacion")
                                    {
                                        perfil = "Sub-Ubicaciones";
                                    }
                                    try
                                    {
                                        string getconjunt = _profileTable.Get("name", perfil);
                                        JArray conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                                        string idconjunto = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                                        JArray locatList = JsonConvert.DeserializeObject<JArray>(_locationTable.Get("profileId", idconjunto));

                                        foreach (JObject locat in locatList) //for each profile we create an option element with id as value and the name as the text
                                        {
                                            if (locat["parent"].ToString() == "none")
                                            {
                                                locat["parent"] = id;
                                                if (perfil == "Conjunto")
                                                {
                                                    if (nameprofile["name"].ToString() == "Ubicacion")
                                                    {
                                                        string existloc = _locationTable.ExistLocation("name", locat["name"].ToString(), "parent", locationID);
                                                        if (existloc == "[]" || existloc == "" || existloc == null)
                                                        {
                                                            locat.Remove("_id");
                                                            _locationTable.SaveRow(JsonConvert.SerializeObject(locat));
                                                            _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Insert: " + locat["name"].ToString(), "Locations", DateTime.Now.ToString());
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _locationTable.SaveRow(JsonConvert.SerializeObject(locat), locat["_id"].ToString());
                                                        _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Update: " + locat["name"].ToString(), "Locations", DateTime.Now.ToString());
                                                    }
                                                }
                                                else{
                                                if (nameprofile["name"].ToString() == "Conjunto")
                                                {
                                                    string existloc = _locationTable.ExistLocation("name",locat["name"].ToString(), "parent", locationID);
                                                    if (existloc == "[]" || existloc == "" || existloc == null)
                                                    {
                                                        locat.Remove("_id");
                                                        _locationTable.SaveRow(JsonConvert.SerializeObject(locat));
                                                        _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Insert: " + locat["name"].ToString(), "Locations", DateTime.Now.ToString());
                                                    }
                                                }
                                                else
                                                {
                                                    _locationTable.SaveRow(JsonConvert.SerializeObject(locat), locat["_id"].ToString());
                                                    _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Update: " + locat["name"].ToString(), "Locations", DateTime.Now.ToString());
                                                }
                                               }
                                            }
                                        }
                                    }
                                    catch (Exception ex) { }
                                }
                            }
                        }

                        int h, w, flag = 0;
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

                            //Images resizeImage = new Images(absolutepath + "\\" + id + "." + ext, absolutepath, id + "." + ext);
                            //// If image bigger than 1MB, resize to 1024px max
                            //if (file.ContentLength > 1024 * 1024)
                            //    resizeImage.resizeImage(new Size(1024, 1024));

                            //// Create the thumbnail of the image
                            //resizeImage.createThumb();

                        }

                        //if (setup != "" && setup != "null" && setup != setuporiginal)
                        //    copysetup(id, setup, setuporiginal);

                        JObject result = new JObject();
                        result["id"] = id;
                        result["text"] = nombre;

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

        public void copysetup(string nodeid, string setup, string oldsetup)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                string nodo = "", oldnodo = "";

                List<BsonDocument> docs1 = _locationTable.get("setupname", setup);
                foreach (BsonDocument location in docs1)
                {
                    nodo = location.GetElement("_id").Value.ToString();
                }

                docs1 = _locationTable.get("setupname", oldsetup);
                foreach (BsonDocument location in docs1)
                {
                    oldnodo = location.GetElement("_id").Value.ToString();
                }

                List<BsonDocument> docs2 = _locationTable.get("parent", oldnodo);
                List<BsonDocument> docs3 = _locationTable.get("parent", nodeid);

                foreach (BsonDocument location in docs3)
                {

                    if (docs2.Find(x => x.GetElement("name").Value.ToString() == location.GetElement("name").Value.ToString()) != null)
                    {
                        deleteNode(location.GetElement("_id").Value.ToString());
                    }
                }

                copynodes(nodo, nodeid);
            }
        }

        public void copynodes(string oldparent, string newparent)
        {
            List<BsonDocument> docs = _locationTable.get("parent", oldparent); //getting all the users
            string id = "";
            foreach (BsonDocument location in docs)
            {
                String jsondata = "";
                jsondata += "{'name':'" + location["name"].ToString() + "','imgext':'','parent':'" + newparent + "','tipo':'" + location["tipo"].ToString() + "', 'setupname':'', 'setup' : '', 'profileId':'" + location["profileId"].ToString() + "', 'creatorId':'" + this.Session["_id"] + "', 'profileFields': " + location["profileFields"].ToString() + "}";

                id = _locationTable.SaveRow(jsondata);
                _logTable.SaveLog(Session["_id"].ToString(), "Ubicaciones", "Insert: " + location["name"].ToString(), "Locations", DateTime.Now.ToString());
                copynodes(location["_id"].ToString(), id);
            }

        }

        public String locationExists(String locationName)
        {
            try
            {
                List<BsonDocument> list = _locationTable.get("name", locationName);
                if (list.Count == 0)
                    return "false";
                return "true";
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string numberExists(string number, string locationID) {
            if (locationID == null)
            {
                string location = _locationTable.Get("number", number);
                if (location == "" || location == "[]" || location == null)
                {
                    return "false";
                }
                else
                {
                    return "true";
                }
            }
            else {
                string locationExist = _locationTable.GetRow(locationID);
                if (locationExist != "" || locationID != "[]" || locationID != null) {
                    JObject locationEObj = JsonConvert.DeserializeObject<JObject>(locationExist);
                    if (locationEObj["number"].ToString() == number) { return "false"; }
                    else
                    {
                        return numberExists(number, null);
                    }
                } else {
                    return numberExists(number, null);
                }
            }
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

        public String loadLocationsRegion(String userid)
        {

            try
            {
                String regions = "";
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getconjunt = _profileTable.Get("name", "Region");
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

                 regions += "<option value='null' selected> Seleccione Región</option>";
                //******************************************************************************
                foreach (JObject ob in locats)
                {
                    rowArray = _locationTable.GetRow(ob["id"].ToString());
                    locat = JsonConvert.DeserializeObject<JObject>(rowArray);
                    if (locat["profileId"].ToString() == idregion) {
                        regions += "<option value='" + locat["_id"] + "'>" + locat["name"] + "</option>";
                    }

                }
              
                return regions;

            }
            catch (Exception ex) {
                return null;

            }
        }

        public String loadLocationsConjunto(String userid)
        {

            try
            {
                String regions = "";
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getconjunt = _profileTable.Get("name", "Conjunto");
                JArray conjuntja = new JArray();
                string idconjunto = "";
                string idregion = "";
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idconjunto = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                getconjunt = _profileTable.Get("name", "Region");
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

                regions += "<option value='null' selected> Seleccione Región</option>";
                //******************************************************************************
                foreach (JObject ob in locats)
                {
                    rowArray = _locationTable.GetRow(ob["id"].ToString());
                    locat = JsonConvert.DeserializeObject<JObject>(rowArray);
                    if (locat["profileId"].ToString() == idconjunto)
                    {
                        regions += "<option value='" + locat["_id"] + "'>" + locat["name"] + "</option>";
                    }

                }

                return regions;

            }
            catch (Exception ex)
            {
                return null;

            }
        }

        public String loadLocationsConjuntoAlls()
        {

            try
            {
                String locationsOptions = "";
                string getconjunt = _profileTable.Get("name", "Conjunto"); 
                JArray conjuntja = new JArray();
                string idprof = "";
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }
                String rowArray = _locationTable.Get("profileId", idprof);
                JArray locatList = JsonConvert.DeserializeObject<JArray>(rowArray);

                locationsOptions += "<option value='all' selected> Todas las oficinas</option>";

                foreach (JObject document in locatList) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["name"].ToString() != "")
                    {
                        if (document["parent"].ToString() == "none")
                        {
                            locationsOptions += "<option value='" + document["_id"] + "' data-num='" + document["number"] + "'"; //setting the id as the value
                            locationsOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                        }
                    }

                }

                return locationsOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String loadSubLocationsAlls()
        {

            try
            {
                String locationsOptions = "";
                  JArray conjuntja = new JArray();
                  string idsubloc = "";
               

              string  getconjunt = _profileTable.Get("name", "Sub-Ubicaciones");
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idsubloc = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                String rowArray = _locationTable.Get("profileId",idsubloc);
                JArray locatList = new JArray();
                try
                {
                  locatList= JsonConvert.DeserializeObject<JArray>(rowArray);
                }
                catch { }
                locationsOptions += "<option value='all' selected> Todas las Sub ubicaciones</option>";

                foreach (JObject document in locatList) //for each profile we create an option element with id as value and the name as the text
                {
                    try
                    {
                        if (document["name"].ToString() != "")
                        {
                            if (document["parent"].ToString() == "none")
                            {
                                locationsOptions += "<option value='" + document["_id"] + "' data-num='" + document["number"] + "'"; ; //setting the id as the value
                                locationsOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                            }
                        }
                    }
                    catch
                    {

                    }

                }

                return locationsOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String loadLocationsAlls()
        {

            try
            {
                String locationsOptions = "";
                string getconjunt = _profileTable.Get("name", "Conjunto");
                JArray conjuntja = new JArray();
                string idconjunto = "";
                string subloc = _profileTable.Get("name", "Sub-Ubicaciones");
                JArray sublocja = new JArray();
                string idsubloc = "";
                string idregion = "";
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idconjunto = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }
                try
                {
                    sublocja = JsonConvert.DeserializeObject<JArray>(subloc);
                    idsubloc = (from mov in sublocja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }
                getconjunt = _profileTable.Get("name", "Region");
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idregion = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                String rowArray = _locationTable.GetLocationsTable("", "", idregion, idconjunto, idsubloc);
                JArray locatList = JsonConvert.DeserializeObject<JArray>(rowArray);

                locationsOptions += "<option value='all' selected> Todas las ubicaciones</option>";

                foreach (JObject document in locatList) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["name"].ToString() != "")
                    {
                        if (document["parent"].ToString() == "none")
                        {
                            locationsOptions += "<option value='" + document["_id"] + "' data-num='" + document["number"] + "'";; //setting the id as the value
                            locationsOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                        }
                    }

                }

                return locationsOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String GetGerenteConjunto(String location)
        {
            String gerente = "";
            JArray ruta = new JArray();
            String objarray;
            JArray users = new JArray();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            ruta = getRoute3(location);
            foreach (String ob in ruta)
            {
                list1.Add(ob);
            }


            objarray = _userTable.Get("position", "gerente");
            users = JsonConvert.DeserializeObject<JArray>(objarray);

            foreach (JObject obj in users)
            {
                JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                foreach (JObject l in locts)
                    list2.Add(l["id"].ToString());
                if (list1.Intersect<string>(list2).ToList<string>().Count > 0)
                {

                    gerente = JsonConvert.SerializeObject(obj);
                }
            }

            return gerente;
        }

        public String GetGerenteRegional(String location)
        {
            String gerente = "";
            JArray ruta = new JArray();
            String objarray;
            JArray users = new JArray();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            ruta = getRoute3(location);
            foreach (String ob in ruta)
            {
                list1.Add(ob);
            }


            objarray = _userTable.Get("position", "gerente_regional");
            users = JsonConvert.DeserializeObject<JArray>(objarray);

            foreach (JObject obj in users)
            {
                JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                foreach (JObject l in locts)
                    list2.Add(l["id"].ToString());
                if (list1.Intersect<string>(list2).ToList<string>().Count > 0)
                {

                    gerente = JsonConvert.SerializeObject(obj);
                }
            }

            return gerente;
        }

        public string checkRealObject(string locationID){
            string objectreal = objectReal.Get("location", locationID);
            if (objectreal != "" && objectreal != "[]" && objectreal != null)
            {
                return "true";
            }
            else {
                return "false";
            }
        }

        //Importacion secction -------------------------------------------------------
        /// <summary>
        /// This method get the content of the Excel file
        /// </summary>
        /// <param name="file">Excel File</param>
        /// <returns>Table of the content of the Excel file</returns>
        /// <author>Edwin (Origin) - Abigail Rodriguez(Edit)</author>
        public ActionResult ImpExcelUbicacion(HttpPostedFileBase file)
        {
            Dictionary<string, int> orderCell = new Dictionary<string, int>();
            string[] arrayalf = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            for (int i = 0; i < arrayalf.Length; i++)
            {
                orderCell.Add(arrayalf[i], i);
            }
            DataSet ds = new DataSet();

            List<List<string>> tr = new List<List<string>>();
            try
            {
                if (Request.Files["file"].ContentLength > 0)
                {
                    string fileExtension =
                                         System.IO.Path.GetExtension(Request.Files["file"].FileName);

                    if (fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                        if (System.IO.File.Exists(fileLocation))
                        {

                            System.IO.File.Delete(fileLocation);
                        }
                        Request.Files["file"].SaveAs(fileLocation);
                    }

                    string fileLocation2 = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation2))
                    {

                        System.IO.File.Delete(fileLocation2);
                    }
                    Request.Files["file"].SaveAs(fileLocation2);

                    using (DocumentFormat.OpenXml.Packaging.SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileLocation2, false))
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;


                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>(); ;
                        foreach (Row r in sheetData.Elements<Row>())
                        {
                            List<string> td = new List<string>();
                            int index = 0;
                            foreach (Cell c in r.Elements<Cell>())
                            {

                                string cellIndex = c.CellReference.ToString().Substring(0, 1);
                                bool validate = false;
                                int numcellx = 0;
                                foreach (var x in orderCell)
                                {
                                    if (x.Key == cellIndex)
                                    {
                                        numcellx = x.Value;
                                    }
                                    if (x.Key == cellIndex && x.Value == index)
                                    {
                                        validate = true;
                                        break;
                                    }
                                }

                                if (validate == false)
                                {
                                    numcellx = numcellx - index;
                                    for (int i = 0; i < numcellx; i++)
                                    {
                                        td.Add("");
                                    }
                                    index = index + numcellx;
                                }
                                Int32 id = -1;


                                if (c.DataType != null && c.DataType.Value == CellValues.SharedString)
                                {
                                    if (Int32.TryParse(c.InnerText, out id))
                                    {
                                        SharedStringItem item = GetSharedStringItemById(workbookPart, id);
                                        if (item.Text != null)
                                        {
                                            td.Add(item.Text.Text);
                                        }
                                        else if (item.InnerText != null)
                                        {
                                            td.Add(item.InnerText);
                                        }
                                        else if (item.InnerXml != null)
                                        {
                                            td.Add(item.InnerXml);
                                        }
                                    }
                                    else
                                    {
                                        td.Add(c.CellValue.Text);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        td.Add(c.CellValue.Text);
                                    }
                                    catch (Exception ex)
                                    {

                                        td.Add("");
                                    }
                                }
                                index++;
                            }
                            tr.Add(td);
                        }
                        spreadsheetDocument.Close();
                    }

                    List<List<string>> data = new List<List<string>>();
                    ViewData["categoriedata"] = data;
                    ViewData["Filelocation"] = fileLocation2;
                    ViewData["Filerequest"] = file;
                    return View(tr);
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string saveImportUbicacion(String data, String type, IEnumerable<HttpPostedFileBase> files) {
            JArray dataArry = JsonConvert.DeserializeObject<JArray>(data);
            JArray result = new JArray();

            JArray perfileLoc = JsonConvert.DeserializeObject<JArray>(_profileTable.Get("name", type));
            String idType = (from mov in perfileLoc select (string)mov["_id"]).First().ToString();
            
            foreach (JObject dat in dataArry)
            {
                if (dat["location"].ToString() == "" || dat["idLocation"].ToString() == "") continue;
                String formString = "";
                String address = "";
                String subs = "";
                FormCollection dataColl = new FormCollection();
                formString += "name=" + dat["location"].ToString() + "&profileSelect=" + idType + "&locationID=null&tipo=1&number=" + dat["idLocation"].ToString() + "&description=" + (dat["description"].ToString() == null ? "" : dat["description"].ToString());

                if (type == "Conjunto") {
                    try
                    {
                        address = (dat["address"].ToString() == null ? "" : dat["address"].ToString());
                    }
                    catch { }
                    string[] sublcations = dat["subslocation"].ToString().Trim().Split('|');
                    int contLoc = 0;
                    foreach (String subsloc in sublcations)
                    {
                        try
                        {
                            string idlocation = "";
                            JArray conjuntja = JsonConvert.DeserializeObject<JArray>(_locationTable.Get("number", subsloc));
                            foreach (JObject conj in conjuntja)
                            {
                                JObject profileConj = JsonConvert.DeserializeObject<JObject>(_profileTable.GetRow(conj["profileId"].ToString()));
                                if (profileConj["name"].ToString() == "Ubicacion")
                                {
                                   if(conj["parent"].ToString()=="none")
                                    idlocation = conj["_id"].ToString();
                                    break;
                                }
                            }

                            if (idlocation != "")
                            {
                                if (contLoc > 0) subs += ",";
                                subs += idlocation;
                                contLoc++;
                            }
                        }
                        catch { }
                    }
                }
                else if (type == "Region") {
                    string[] sublcations = dat["subslocation"].ToString().Trim().Split('|');
                    int contLoc = 0;
                    foreach (String subsloc in sublcations)
                    {
                        try
                        {
                            string idlocation = "";
                            JArray conjuntja = JsonConvert.DeserializeObject<JArray>(_locationTable.Get("number", subsloc));
                            foreach(JObject conj in conjuntja){
                                JObject profileConj = JsonConvert.DeserializeObject<JObject>(_profileTable.GetRow(conj["profileId"].ToString()));
                                if (profileConj["name"].ToString() == "Conjunto")
                                {
                                    idlocation = conj["_id"].ToString();
                                    break;
                                }
                            }

                            if (idlocation != "")
                            {
                                if (contLoc > 0) subs += ",";
                                subs += idlocation;
                                contLoc++;
                            }
                        }
                        catch { }
                    }
                }

                dataColl.Add("data", formString);

                string results = saveLocation(dataColl, null, (subs == null ? "" : subs), (address == null ? "" : address));
                result.Add(JsonConvert.DeserializeObject<JObject>(results));

            }
            return JsonConvert.SerializeObject(result); 
        }

        /// <summary>
        /// This method help get the content of the Excel file
        /// </summary>
        /// <param name="file">Excel File</param>
        /// <returns></returns>
        /// <author>Edwin</author>
        public static SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, Int32 id)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
        }

    }
}
