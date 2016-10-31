using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RivkaAreas.ObjectAdmin.Models;
using Rivka.Db;
using Rivka.Form;
using Rivka.Form.Field;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MongoDB.Bson;
using Rivka.Security;
using Rivka.Error;
using System.Threading;
using System.Threading.Tasks;
using Rivka.Db.MongoDb;
namespace RivkaAreas.ObjectAdmin.Controllers
{
    [Authorize]
    public class ObjectAdminController : Controller
    {
        //
        // GET: /ObjectAdmin/ObjectAdmin/

        protected CategoryTable categoryTable;
        protected ObjectReal _objectTable;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        protected ObjectTable _objectReferenceTable;  
        protected ProfileTable _profileTable;
        protected MongoModel _profileUsers;
        protected LocationTable locationTable;
        protected UserProfileTable _userprofileTable;
        protected CustomFieldsTable fieldTable;
        protected MongoModel _referenceObjects;
        protected UserTable userTable;
        protected RivkaAreas.Reports.Models.ObjectsRealReport customTable;
        protected LocationProfileTable locationProfileTable;
        protected validatePermissions validatepermissions;
        protected ObjectFieldsTable _objFieldsTable;
        protected ListTable _listTable;
        protected Dictionary<string, string> departs = new Dictionary<string, string>();
        public int totalglobal;
        public int numActivos;
        public int numBaja;
        public int numMov; 
        public ObjectAdminController()
        {
            //this._common = new CommonFunctions();
            this._referenceObjects = new MongoModel("ReferenceObjects");
            this.customTable = new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
            this._profileUsers = new Rivka.Db.MongoDb.MongoModel("Profiles");
            this._objectTable = new ObjectReal("ObjectReal");
            this._profileTable = new ProfileTable("MovementProfiles");
            this.categoryTable = new CategoryTable();
            this.locationTable = new LocationTable();
            this.userTable = new UserTable();
            this._objectReferenceTable = new ObjectTable();
            this._userprofileTable = new UserProfileTable();
            this.fieldTable = new CustomFieldsTable("ObjectFields");
            this.locationProfileTable = new LocationProfileTable();
            validatepermissions = new validatePermissions();
            _objFieldsTable = new ObjectFieldsTable();
            _listTable = new ListTable();
            this._logTable = new LogBook.Controllers.LogBookController();
        }

        public String getLocationsByParent(string id,string type)
        {
            String options="<option value=''>Seleccione una "+type+"</option>";
            try
            {
                JArray locations=JsonConvert.DeserializeObject<JArray>(locationTable.Get("parent",id));

                foreach(JObject loc in locations){
                    try{
                      
                        options+="<option value='"+loc["_id"].ToString()+"'>"+loc["name"].ToString()+"</option>";
                    }catch{

                    }
                }


                return options;
            }
            catch
            {
                return options;
            }
        }
        public int deleteObjectsmov(String ids)
        {
            try
            {
                JArray idsja = JsonConvert.DeserializeObject<JArray>(ids);
                List<string> idsist = (from id in idsja select (string)id).ToList();
                JArray objsja = JsonConvert.DeserializeObject<JArray>(customTable.GetbyCustom("_id", idsist, "ObjectReal"));
                foreach (JObject obj in objsja)
                {
                    try
                    {
                        JToken tk;
                        string statuscurren = "true";
                        string statusname = "Esta en tu oficina";
                        if (obj.TryGetValue("system_status", out tk))
                        {
                            statuscurren = obj["system_status"].ToString().ToLower();
                            if (statuscurren=="true")
                            {
                                obj["system_status"] = false;
                                statusname = "Dado de baja";
                            }
                            else
                            {
                                obj["system_status"] = true;
                            }
                        }

                        if (obj.TryGetValue("status", out tk))
                        {
                           
                            obj["status"] = statusname;
                        }
                        else
                        {
                            obj.Add("status", statusname);
                        }
                        _objectTable.SaveRow(JsonConvert.SerializeObject(obj), obj["_id"].ToString());
                        if (statuscurren == "true")
                        customTable.DeleteRow(obj["_id"].ToString(),true);

                    }
                    catch { }
                }

                return Response.StatusCode=200;
            }
            catch
            {
              //  return new HttpStatusCodeResult(500);
             return  Response.StatusCode = 500;
            }
        }
                   
        public String goUpMov(string location,string objectid,string serie,string price,string date,string cant,string label)
        {
            try
            {
                JObject objrefdata = new JObject();
                int cantidad=0;
                try
                {
                    objrefdata = JsonConvert.DeserializeObject<JObject>(_objectReferenceTable.getRow(objectid));
                    
                }
                catch
                {
                    return "El objeto de referencia ya no exite en el sistema";
                }

                try
                {
                    cantidad = Convert.ToInt16(cant);
                }
                catch
                {
                    return "La cantidad debe de ser un valor numerico y mayor a cero";
                }
                JToken tk;
                for (int i = 0; i < cantidad; i++)
                {

                    try
                    {
                        JObject newobject = new JObject();
                        string idunico=_objectTable.GetIdUnico();
                        newobject.Add("objectReference", objectid);
                        newobject.Add("name", objrefdata["name"].ToString());
                        newobject.Add("location", location);
                        newobject.Add("EPC", "Porasignar"+idunico);
                        newobject.Add("serie", serie);
                        newobject.Add("object_id", idunico);
                        if (objrefdata.TryGetValue("assetType", out tk))
                        {
                            newobject.Add("assetType", objrefdata["assetType"].ToString());
                        }
                        else
                        {
                            newobject.Add("assetType", "");
                        
                        }
                        newobject.Add("label", label);
                       
                        newobject.Add("system_status", true);
                        newobject.Add("date", date);
                        newobject.Add("price", price);
                        string id = _objectTable.SaveRow(JsonConvert.SerializeObject(newobject));
                    }
                    catch
                    {

                    }
                }
                return "success";
            }
            catch
            {
                return "Ha ocurrido un error";
            }
        }
        public String goTransference(String ids, string location)
        {
            String result = "";
            try
            {
                JArray idsja = JsonConvert.DeserializeObject<JArray>(ids);
                List<string> idsist = (from id in idsja select (string)id).ToList();
                JArray objsja = JsonConvert.DeserializeObject<JArray>(customTable.GetbyCustom("_id", idsist, "ObjectReal"));
                foreach (JObject obj in objsja)
                {
                    try
                    {
                        obj["location"] = location;
                        customTable.SaveRow(JsonConvert.SerializeObject(obj), obj["_id"].ToString());

                    }
                    catch { }
                }

                return "success";
            }
            catch
            {
                return "ha ocurrido un error";
            }
        }
        public string validSerie(string serie)
        {
            try
            {
                JArray objdata = JsonConvert.DeserializeObject<JArray>(_objectTable.Get("serie", serie));

                foreach (JObject ob in objdata)
                {
                    if (serie == ob["serie"].ToString())
                    {
                        return "true";
                    }
                }

                return "false";
            }
            catch
            {
                return "false";
            }
        }
        public void getObjsRef()
        {
            JArray objja = new JArray();
            try
            {
                objja = JsonConvert.DeserializeObject<JArray>(_objectReferenceTable.GetRows());
            }
            catch { }
           ViewData["objrefja"]= objja;
        }
        public ActionResult Index()
        {
            //String dataPermissions = "";
            //try
            //{
            //    dataPermissions = Session["Permissions"].ToString();
            //}
            //catch (Exception e)
            //{
            //    return Redirect("~/Home");
            //}
            ////TODO: advanced search using Agregation.
            JArray objja = new JArray();
            try
            {
                objja = JsonConvert.DeserializeObject<JArray>(_objectReferenceTable.GetRows());
            }
            catch { }
            ViewData["objrefja"] = objja;
            String dataPermissions;
            String dataPermissionsClient;
            try
            {
                dataPermissions = Session["Permissions"].ToString();
                dataPermissionsClient = Session["PermissionsClient"].ToString();
            }
            catch (Exception e) {
                return Redirect("~/Home");  
            }
            bool access = false;
            bool accessClient = false;
          
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                Dictionary<string, string> conjdict = new Dictionary<string, string>();
                try
                {
                    JObject profja = JsonConvert.DeserializeObject<JArray>(locationProfileTable.Get("name", "Conjunto")).First as JObject;
                    JArray conjja = JsonConvert.DeserializeObject<JArray>(locationTable.Get("profileId", profja["_id"].ToString()));
                    foreach (JObject conj in conjja)
                    {
                        try
                        {
                            conjdict.Add(conj["_id"].ToString(), conj["name"].ToString());
                        }
                        catch { }
                    }
                
                }
                catch
                {

                }
                Dictionary<string, string> refdict = new Dictionary<string, string>();
                try
                {
                    JArray refja = JsonConvert.DeserializeObject<JArray>(_referenceObjects.GetRows());

                    foreach (JObject refobj in refja)
                    {
                        try
                        {
                            refdict.Add(refobj["_id"].ToString(), refobj["name"].ToString());
                        }
                        catch { }
                    }

                }
                catch
                {

                }
                bool isadmin = false;
                try
                {
                    JObject profuser = JsonConvert.DeserializeObject<JArray>(_profileUsers.Get("name", "Administrador de sistema")).First as JObject;
                    string iduserthis = "";
                    try
                    {
                        iduserthis = Session["_id"].ToString();
                    }
                    catch (Exception ex)
                    {
                        if (Request.Cookies["_id2"] != null)
                        {
                            Session["_id"] = Request.Cookies["_id2"].Value;
                            iduserthis = Session["_id"].ToString();
                        }
                    }

                    JObject userdat = JsonConvert.DeserializeObject<JObject>(userTable.GetRow(iduserthis));
                    if (userdat["profileId"].ToString() == profuser["_id"].ToString())
                    {
                        isadmin = true;
                    }
   
                }
                catch { }
                ViewData["isadmin"] = isadmin;
                ViewData["conjuntos"] = conjdict;
                ViewData["refobjects"] = refdict;
                //var profiles = _profileTable.getRows();
                //return View(profiles);

                String rowArray = _objectTable.GetRows();
                JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);

                rowArray = _userprofileTable.GetRows();
                JArray users = JsonConvert.DeserializeObject<JArray>(rowArray);
                Dictionary<string, string> data = new Dictionary<string, string>();

                rowArray = userTable.GetRow(Session["_id"].ToString()); 
                JObject userlog = JsonConvert.DeserializeObject<JObject>(rowArray);
                rowArray = _userprofileTable.GetRow(userlog["profileId"].ToString());
                JObject userprofile = new JObject();
                try
                {
                    userprofile = JsonConvert.DeserializeObject<JObject>(rowArray);
                }
                catch { }
                foreach (JObject items in users)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString());
                }

                String fieldsString = fieldTable.GetRows();
                JArray fieldsArray = JsonConvert.DeserializeObject<JArray>(fieldsString);

                rowArray = _objectTable.GetObjectsReferences();
                JArray objsref = JsonConvert.DeserializeObject<JArray>(rowArray);
                Dictionary<string, string> data2 = new Dictionary<string, string>();

                foreach (JObject items in objsref)
                {
                    if (!data2.ContainsKey(items["_id"].ToString()))
                        data2.Add(items["_id"].ToString(), items["name"].ToString());
                }

               rowArray = locationTable.GetRows();
                JArray locat = JsonConvert.DeserializeObject<JArray>(rowArray);
                Dictionary<string, string> data3 = new Dictionary<string, string>();
                JArray profileloc = new JArray();
                List<string> listlocprof = new List<string>();
                try
                {
                    profileloc = JsonConvert.DeserializeObject<JArray>(locationProfileTable.GetRows());
                    foreach (JObject j in profileloc)
                    {
                        try
                        {
                            if (j["name"].ToString() == "Region" || j["name"].ToString() == "Conjunto")
                                listlocprof.Add(j["_id"].ToString());
                        }
                        catch { }
                    }
                }
                catch { }
                foreach (JObject items in locat)
                {
                    if (listlocprof.Contains(items["profileId"].ToString()))
                    {
                        if (!data3.ContainsKey(items["_id"].ToString()))
                            data3.Add(items["_id"].ToString(), items["name"].ToString());
                    }
                }

                loadDepartments();
                loadproveedores();
                ViewData["locations"] = data3;

                ViewData["users"] = data;
                ViewData["fields"] = fieldsArray;
                ViewData["objsReference"] = data2;
                string profilename="";
                try{
                    profilename=userprofile["name"].ToString();
                }catch{ }
                ViewData["userProfile"] = profilename;

                return View(objects);
            }
            else
            {

                return Redirect("~/Home");
            } 
        }
        public String deleteObject(String id)
        {
            try
            {
                _objectTable.deleteRow(id);

                return "Eliminado correctamente";
            }
            catch
            {
                return "No se pudo eliminar el activo";
            }
        }

        public bool getpermissions(string permission, string type)
        {
            var datos = "";
            try
            {
                datos = Session["Permissions"].ToString();
            }
            catch (Exception ex)
            {
                if (Request.Cookies["permissions"] != null)
                {
                    Session["Permissions"] = Request.Cookies["permissions"].Value;
                    datos = Session["Permissions"].ToString();
                }

            }
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
        /// Sends the instructions to Edgeware for printing
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        public JsonResult PrintLabels(string labels) 
        {
            string result = "";



            return Json(result);
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
            String categoriesString = locationTable.Get("parent", parentCategory);
            String objectsString = _objectTable.Get("parentCategory", parentCategory);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
            JArray objectObject = JsonConvert.DeserializeObject<JArray>(objectsString);

            //categoriesObject.Add(objectObject);

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
        public void bindsessions()
        {
            try
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                if (Request.Cookies["_loggeduser"] != null)
                {
                    Session["LoggedUser"] = Request.Cookies["_loggeduser"].Value;
                }
                if (Request.Cookies["permissions"] != null)
                {
                    Session["Permissions"] = Request.Cookies["permissions"].Value;

                }
                if (Request.Cookies["permissionsclient"] != null)
                {
                    Session["PermissionsClient"] = Request.Cookies["permissionsclient"].Value;

                }
            }
            catch
            {

            }
        }
        public string saveFiles(HttpPostedFileBase attachment)
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);

            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);

                if (attachment != null)
                {
                    filename = attachment.FileName.ToString();

                }

                string ext = null;
                string patch = "";
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;
                string relativepath = "\\Uploads\\Images\\ObjectsFiles";
                string absolutepath = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absolutepath))
                {
                   // System.IO.Directory.Delete(absolutepath, true);
                }

                if (attachment != null)
                {
                    ext = attachment.FileName.Split('.').Last(); //getting the extension
                }
                if (attachment != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }

                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;

                    }
                }

               
                return patch;


            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public JsonResult getObjectInfo(String idobject)
        {
            try
            {
                String rowString = _objectTable.GetRow(idobject); 
                JObject row = JsonConvert.DeserializeObject<JObject>(rowString);

                rowString = _objectReferenceTable.GetRow(row["objectReference"].ToString());
                if (rowString != null) {
                    JObject row1 = JsonConvert.DeserializeObject<JObject>(rowString);
                    //reference info
                    JToken jk;
                    if (!row.TryGetValue("name", out jk))
                        row.Add("name", "");
                    if (!row.TryGetValue("marca", out jk))
                        row.Add("marca", "");
                    if (!row.TryGetValue("modelo", out jk))
                        row.Add("modelo", "");
                    if (!row.TryGetValue("proveedor", out jk))
                        row.Add("proveedor", "");
                    if (!row.TryGetValue("department", out jk))
                        row.Add("department", "");
                    if (!row.TryGetValue("perfil", out jk))
                        row.Add("perfil", "");
                    if (!row.TryGetValue("object_idact", out jk))
                        row.Add("object_idact", "");
                    try { row["name"] = row1["name"].ToString(); }catch { }
                    try { row["marca"] = row1["marca"].ToString(); }catch { }
                    try { row["modelo"] = row1["modelo"].ToString(); }catch { }
                    try { row["proveedor"] = row1["proveedor"].ToString(); }catch { }
                    try { row["department"] = row1["department"].ToString(); }catch { }
                    try { row["perfil"] = row1["perfil"].ToString(); }catch { }
                    try { row["object_idact"] = row["object_id"].ToString(); }catch { }
                    try { row["object_id"] = row1["object_id"].ToString(); }
                    catch { }
                    row["profileFields"] = row1["profileFields"];
                    row["category"] = row1["parentCategory"].ToString();
                    row["ext"] = row1["ext"].ToString();

                    row["currentmove"] = _objectTable.GetdemandFolio(idobject);
                    row["allmoves"] = _objectTable.GetAlldemandsFolio(idobject);

                    try{
                        if (row["currentmove"].ToString() != " " && row["currentmove"].ToString() != "")
                        {
                            row["status"] = "En movimiento";

                        }
                        else {
                            if (row["system_status"].ToString() == "false" || row["system_status"].ToString() == "False")
                            {
                                row["status"] = "Dado de baja";
                            }
                            else {
                                row["status"] = "Está en tu oficina";
                            }
                            
                        }

                    }
                    catch {
                        row["status"] = "Está en tu oficina";
                    }
                    
                    try {
                        if (row["serie"].ToString() == "")
                        {
                            row["serie"] = row1["serie"].ToString();

                        } 
                    }
                    catch {
                        row["serie"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "serie") { 
                            if(row1["serie"].ToString()!="")
                               row["serie"] = row1["serie"].ToString();
                            }
                        }
                    
                    }
                    try {
                        if (row["price"].ToString() == "")
                        {
                            row["price"] = row1["precio"].ToString();
                        } 
                    }
                    catch {
                        row["price"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "precio")
                            {
                                if (row1["precio"].ToString() != "")
                                    row["price"] = row1["precio"].ToString();
                            }
                        }
                    }
                    try {
                        if (row["department"].ToString() == "")
                        {
                          //  row["department"] = row1["department"].ToString();
                        }
                    }
                    catch {
                        row["department"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "department")
                            {
                                //if (row1["department"].ToString() != "")
                                   // row["department"] = row1["department"].ToString();
                            }
                        }
                    }
                    //try
                    //{
                    //    row["marca"] = row1["marca"].ToString();
                    //}
                    //catch { 
                    
                    //}


                    try
                    {
                        if (row["marca"].ToString() == "")
                        {
                          //  row["marca"] = row1["marca"].ToString();
                        }
                    }
                    catch
                    {
                       // row["marca"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "marca")
                            {
                                //if (row1["marca"].ToString() != "")
                                 //   row["marca"] = row1["marca"].ToString();
                            }
                        }
                    }
                    try
                    {
                        if (row["modelo"].ToString() == "")
                        {
                            row["modelo"] = row1["modelo"].ToString();
                        } 
                    }
                    catch {
                       // row["modelo"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "modelo")
                            {
                               // if (row1["modelo"].ToString() != "")
                                 //   row["modelo"] = row1["modelo"].ToString();
                            }
                        }
                    }
                    try
                    {
                        if (row["perfil"].ToString() == "")
                        {
                            row["perfil"] = row1["perfil"].ToString();
                        }
                    }
                    catch {
                       // row["perfil"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "perfil")
                            {
                                //if (row1["perfil"].ToString() != "")
                                   // row["perfil"] = row1["perfil"].ToString();
                            }
                        }
                    }
                   
                    try
                    {
                        if (row["object_id"].ToString() == "")
                        {
                          //  row["object_id"] = row1["object_id"].ToString();
                        } 
                    }
                    catch {
                        //row["object_id"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "object_id")
                            {
                             //   if (row1["object_id"].ToString() != "")
                               //     row["object_id"] = row1["object_id"].ToString();
                            }
                        }
                    }
                    
                    try
                    {
                        if (row["folio"].ToString() == "")
                        {
                            row["folio"] = row1["folio"].ToString();
                        }
                    }
                    catch
                    {
                       // row["folio"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "folio")
                            {
                               // if (row1["folio"].ToString() != "")
                                 //   row["folio"] = row1["folio"].ToString();
                            }
                        }
                    }

                    //try
                    //{
                    //    if (row["folio"].ToString() == "")
                    //    {
                    //        row["folio"] = row1["folio"].ToString();
                    //    }
                    //}
                    //catch
                    //{
                    //    row["folio"] = "";
                    //    foreach (KeyValuePair<string, JToken> token in row1)
                    //    {
                    //        if (token.Key == "folio")
                    //        {
                    //            if (row1["folio"].ToString() != "")
                    //                row["folio"] = row1["folio"].ToString();
                    //        }
                    //    }
                    //}
                    row.Add("sublocation", "");
                    try
                    {
                        JObject loc = JsonConvert.DeserializeObject<JObject>(locationTable.GetRow(row["location"].ToString()));
                        JObject prof = JsonConvert.DeserializeObject<JObject>(locationProfileTable.GetRow(loc["profileId"].ToString()));
                        string nameprof = prof["name"].ToString();
                        if (nameprof == "Sub-Ubicaciones")
                        {
                            row["sublocation"]= loc["name"].ToString();
                        }
                    }
                    catch { }
                    row["idreference"] = row1["_id"].ToString();

                    row["locationRoute"] = getRoute2(row["location"].ToString());
                }


                rowString = locationTable.GetRow(row["location"].ToString());
                if (rowString != null)
                {
                    JObject row1 = JsonConvert.DeserializeObject<JObject>(rowString);

                    row["locationname"] = row1["name"].ToString();
                }
                try
                {
                    rowString = userTable.GetRow(row["Creator"].ToString());
                
                    if (rowString != null)
                    {
                        JObject row1 = JsonConvert.DeserializeObject<JObject>(rowString);
                        JToken j;
                        if (row.TryGetValue("username",out j))
                            row.Add("username", "");

                        row["username"] = row1["name"].ToString()+" "+row1["lastname"].ToString();
                    }
                }
                catch (Exception ex) { row["username"] = ""; }


                try
                {
                    rowString = categoryTable.GetRow(row["category"].ToString()); 

                    if (rowString != null)
                    {
                        JObject row1 = JsonConvert.DeserializeObject<JObject>(rowString);

                        row["nameCategory"] = row1["name"].ToString();
                    }
                }
                catch (Exception ex) { row["nameCategory"] = ""; }
                return Json(JsonConvert.SerializeObject(row));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String loadLocationsRegion(String userid)
        {

            try
            {
                String region = "";
                String userstring = userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getconjunt = locationProfileTable.Get("name", "Region"); 
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
                    rowArray = locationTable.GetRow(ob["id"].ToString());
                    locat = JsonConvert.DeserializeObject<JObject>(rowArray);
                    if (locat["profileId"].ToString() == idregion)
                    {
                        region =  locat["_id"].ToString();
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
                String userstring = userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getconjunt = locationProfileTable.Get("name", "Conjunto");
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
                    rowArray = locationTable.GetRow(ob["id"].ToString());
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

        public JsonResult getNodeContent2(String id , String userid=null)
        {
            JObject result = new JObject();
            if (id == "") id = "null";
             String categoriesString="";
            if (id=="null" && userid != null) {
                 String userstring = userTable.GetRow(userid); 
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JObject profilobj = new JObject();
                try
                {
                    userstring = _userprofileTable.GetRow(userobj["profileId"].ToString());
                    JsonConvert.DeserializeObject<JObject>(userstring);
                }
                catch {
                    profilobj.Add("name", "Administrador de sistema");
                }

                JToken tk;
                if (!profilobj.TryGetValue("name", out tk))
                {
                    profilobj.Add("name", "Administrador de sistema");
                }
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
                    else {
                        id = loadLocationsConjunto(userid);
                    }

                    result["id"] = id;
                    id=(id==null || id=="null")?"":id;
                    if (id == "") result["name"] = "";
                    else {
                        try
                        {
                            categoriesString = locationTable.GetRow(id);
                            result["name"] = (JsonConvert.DeserializeObject<JObject>(categoriesString))["name"].ToString();
                        }
                        catch {
                        
                        }
                        }
                    
                }
                else { id = "null";
                result["id"] = id;
                result["name"] = "Home";
                
                }
            }

            id = (id == "") ? "null" : id;
            categoriesString = locationTable.Get("parent", id);
          //  String objectsString = _objectTable.Get("parentCategory", id);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
           // JArray objectObject = JsonConvert.DeserializeObject<JArray>(objectsString);


            JArray newobjs = new JArray();

            foreach (JObject obj in categoriesObject)
            {
                try
                {
                    JObject obj1 = new JObject();
                    obj1["id"] = obj["_id"];
                    obj1["text"] = obj["name"];
                    obj1["hasChildren"] = true;

                    //  obj1["items"] = "[]";
                    obj1["spriteCssClass"] = "objectimg";
                    newobjs.Add(obj1);
                }
                catch { }
            }
            result["hijos"] = newobjs;
         
            //   return Json(JsonConvert.SerializeObject(newobjs), JsonRequestBehavior.AllowGet);
            return Json(JsonConvert.SerializeObject(result), JsonRequestBehavior.AllowGet);
        }
         public String generateIndex(int total,int take)
        {
          
        try{
                     

              System.Text.StringBuilder pagination = new System.Text.StringBuilder();

                    try
                    {
                    Dictionary<string,string> pairs=new Dictionary<string,string>();
                  
                        int skip=0;
                        int index=0;
                        if (total < take)
                        {
                            pairs.Add("0", total.ToString());
                        }
                        else
                        {
                            for (int i = 0; i < total; i++)
                            {

                                if (index == take)
                                {
                                    pairs.Add(((i + 1) - take).ToString(), i.ToString());


                                    if (total - i < take)
                                    {
                                        pairs.Add((i + 1).ToString(), total.ToString());
                                    }

                                    index = 0;
                                }

                                index++;

                            }
                        }

                        foreach (var dict in pairs)
                        {
                            try
                            {
                                pagination.Append(" <option value='" + dict.Key + "' data-skip='" + dict.Key + "' >" + dict.Key + "-" + dict.Value + "</option>");
                            }
                            catch { }
                        }

                      
                    }
                    catch
                    {
                       
                    }
                  
                    return pagination.ToString();
                
           }
          catch{
              return "<option value='0'>Todos</option>";
           }
         
        }
        /// <summary>
        ///     Allows to get the table elements to show in the object references section
        /// </summary>
        /// <param name="parentCategory">
        ///     Specifies which element to show
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a json with the elements to show
        /// </returns>
        //[HttpPost]
         public ActionResult getData(String parentCategory = "null", String vertodo = "0", bool filter = false, String skip = "0")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            int skip1 = 0;
            try
            {
                skip1 = Convert.ToInt16(skip);
            }
            catch { }
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);
            loadDepartments();
            bool isconjunt = false;
            try
            {
                JObject location1 = JsonConvert.DeserializeObject<JObject>(locationTable.GetRow(parentCategory));

                JObject categories = JsonConvert.DeserializeObject<JObject>(locationProfileTable.GetRow(location1["profileId"].ToString()));

                if (categories["name"].ToString().ToLower().Contains("conjunto") || categories["name"].ToString().ToLower().Contains("ubicacion"))
                {
                    isconjunt = true;
                }
               


            }
            catch { }
            JObject result = new JObject();
            if (access == true && accessClient == true)
            {
                if (parentCategory == "") parentCategory = "null";
                String objectsString="";
                String categoryString = "";
                JArray objectsObject = new JArray();
                JArray categories = new JArray();
                totalglobal = 0;
                numActivos = 0;
                numBaja = 0;
                numMov = 0; 
                JArray paginationja = new JArray();
                if (isconjunt==false)
                {

                    result.Add("objects", "[]");
                    result.Add("total", totalglobal);
                    result.Add("activos", numActivos);
                    result.Add("dadosbaja", numBaja);
                    result.Add("enMov", numMov);
                    result.Add("options", "null");
                    return Json(JsonConvert.SerializeObject(result));

                }else
                if (vertodo == "0")
                {
                    objectsString = _objectTable.GetObjects(parentCategory,null,filter);
                    
                    categoryString = categoryTable.GetRows();
                    //doing changes to objects array
                    objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);

                    categories = JsonConvert.DeserializeObject<JArray>(categoryString);
                    Dictionary<string, JObject> listrefobj = new Dictionary<string, JObject>();
                  
                    try
                    {
                        List<string> listref = (from obj in objectsObject select (string)obj["objectReference"]).ToList();
                        JArray jaref = JsonConvert.DeserializeObject<JArray>(_objectTable.getRefObj(listref));

                        foreach (JObject item in jaref)
                        {
                            try
                            {
                                listrefobj.Add(item["_id"].ToString().ToLower(), item);
                            }
                            catch { }
                        }
                    }
                    catch { }
                    Dictionary<string, string> listCategories = new Dictionary<string, string>();
                    foreach (JObject items in categories)
                    {
                        listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                    }
                    int take = 0;
                    totalglobal = objectsObject.Count();
                   
                     List<string> idsact1 = new List<string>();
                       JArray getdemandin = new JArray();
                     JArray getdemandout = new JArray();
                     try
                     {
                         idsact1 = (from id in objectsObject select (string)id).ToList();
                         try
                         {
                             getdemandin = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 0));
                         }
                         catch { }
                         try
                         {
                             getdemandout = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 1));

                         }
                         catch { }
                     }
                     catch
                     {

                     }
                     List<string> idsact = new List<string>();
                     foreach (JObject document in objectsObject)
                     {

                         /*   if (indexfor > skip1)
                            {
                                //continue;
                                return;
                            }
                            else
                            {
                                /*if (take == 5000)

                                    break;
                                  take++;*/
                         /*  if (indexfor >= 5000)
                               pls.Stop();

                       }*/
                         try
                         {
                             JObject objo = new JObject();
                             JToken tk;
                             if (listrefobj.TryGetValue(document["objectReference"].ToString().ToLower(), out objo))
                             {
                                 if (!document.TryGetValue("name", out tk))
                                     document.Add("name", "");
                                 if (!document.TryGetValue("modelo", out tk))
                                     document.Add("modelo", "");
                                 if (!document.TryGetValue("marca", out tk))
                                     document.Add("marca", "");
                                 if (!document.TryGetValue("nameassetType", out tk))
                                     document.Add("nameassetType", "");
                                 if (!document.TryGetValue("object_id_ref", out tk))
                                     document.Add("object_id_ref", "");
                                if (!document.TryGetValue("object_id", out tk))
                                     document.Add("object_id", "");
                                 if (!document.TryGetValue("proveedor", out tk))
                                     document.Add("proveedor", "");
                                 if (!document.TryGetValue("department", out tk))
                                     document.Add("department", "");
                                 try
                                 {

                                     document["name"] = objo["name"].ToString();
                                 }
                                 catch { }
                                 try
                                 {

                                     document["department"] = objo["department"].ToString();
                                 }
                                 catch { }
                                 try
                                 {

                                     document["object_id_ref"] = objo["object_id"].ToString();
                                 }
                                 catch { }
                                 try
                                 {

                                     document["marca"] = objo["marca"].ToString();
                                 }
                                 catch { }
                                 try
                                 {
                                     document["modelo"] = objo["modelo"].ToString();
                                 }
                                 catch { }
                                 try
                                 {
                                     document["nameassetType"] = objo["nameassetType"].ToString();
                                 }
                                 catch { }
                                 try
                                 {
                                     document["proveedor"] = objo["proveedor"].ToString();
                                 }
                                 catch { }
                             }
                         }
                         catch { }
                         try
                         {
                             idsact.Add(document["_id"].ToString());
                         }
                         catch
                         {

                         }
                         if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                             document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);
                         document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();
                         try
                         {
                             //    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                             document["currentmove"] = "";
                             foreach (JObject item in getdemandin)
                             {
                                 try
                                 {
                                     foreach (JObject obj in item["objects"])
                                     {
                                         try
                                         {
                                             if (obj["id"].ToString() == document["_id"].ToString())
                                             {
                                                 document["currentmove"] = item["folio"].ToString() + " " + item["namemov"].ToString();
                                                 break;
                                             }
                                         }
                                         catch
                                         {

                                         }
                                     }
                                 }
                                 catch
                                 {

                                 }
                             }
                         }
                         catch (Exception ex)
                         {
                             document["currentmove"] = "";
                         }

                         try
                         {
                             //  document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                             document["allmoves"] = "";
                             List<string> folioslist = new List<string>();
                             foreach (JObject item in getdemandout)
                             {
                                 try
                                 {
                                     foreach (JObject obj in item["objects"])
                                     {
                                         try
                                         {
                                             if (obj["id"].ToString() == document["_id"].ToString())
                                             {
                                                 folioslist.Add(item["folio"].ToString() + " " + item["namemov"].ToString());

                                             }
                                         }
                                         catch
                                         {

                                         }
                                     }
                                 }
                                 catch
                                 {

                                 }
                             }
                             document["allmoves"] = String.Join(",\n ", folioslist);
                         }
                         catch (Exception ex)
                         {
                             document["allmoves"] = "";
                         }
                         try
                         {
                             if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                             {
                                 document["status"] = "En movimiento";
                                 numMov++;
                             }
                             else
                             {
                                 if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                                 {
                                     document["status"] = "Dado de baja";
                                     numBaja++;
                                 }
                                 else
                                 {
                                     document["status"] = "Está en tu Oficina";
                                     numActivos++;
                                 }

                             }

                         }
                         catch
                         {
                             document["status"] = "Está en tu Oficina";
                             numActivos++;
                         }


                         try
                         {
                             if (document["ext"].ToString() != "")
                             {
                                 document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                             }
                         }
                         catch (Exception e) { /*Ignored*/ }
                         JToken aux;
                         if (!document.TryGetValue("etiquetado", out aux))
                         {
                             document.Add("etiquetado", "Normal");
                         }
                         if (document.TryGetValue("label", out aux))
                         {

                             if (document["label"].ToString() == "normal")
                             {
                                 document["etiquetado"] = "Normal";
                             }
                             else
                             {
                                 document["etiquetado"] = "No Etiquetable";
                             }

                         }
                         if (!document.TryGetValue("nameassetType", out aux))
                         {
                             document.Add("nameassetType", "");
                         }
                         if (document.TryGetValue("assetType", out aux))
                         {
                             if (document["assetType"].ToString().ToLower().Contains("system"))
                                 document["nameassetType"] = "Sistemas";
                             else if (document["assetType"].ToString().ToLower().Contains("maintenance"))
                                 document["nameassetType"] = "Mantenimiento";
                             else if (document["assetType"].ToString().ToLower().Contains("vinos"))
                                 document["nameassetType"] = "Vinos";
                             else
                             {
                                 document["nameassetType"] = "";
                             }
                         }


                         if (departs != null)
                         {
                             if (!document.TryGetValue("departmentName", out aux))
                             {
                                 document.Add("departmentName", "");
                             }
                             try
                             {
                                 if (departs.ContainsKey(document["department"].ToString()))
                                     document["departmentName"] = departs[document["department"].ToString()];
                             }
                             catch
                             {
                                 document["departmentName"] = "";
                             }

                         }
                         else
                         {
                             document["departmentName"] = "";
                         }
                         try
                         {
                             document["name"] = document["object"].ToString();
                         }
                         catch { }
                         paginationja.Add(document);

                     }
                    objectsObject = paginationja;
                    try
                    {
                        RivkaAreas.Reports.Models.ObjectsRealReport objdc = new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
                        JArray actjo = JsonConvert.DeserializeObject<JArray>(objdc.GetbyCustom("_id", idsact, "ObjectReal"));
                        Dictionary<string, string> namesdict = actjo.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                        Dictionary<string, string> namesdict4 = actjo.ToDictionary(x => (string)x["_id"], x => (string)x["assetType"]);

                        JArray newja = new JArray();
                        foreach (JObject obj in objectsObject)
                        {
                            try
                            {

                                JToken tk;
                                if (namesdict.ContainsKey(obj["_id"].ToString()))
                                {
                                    string name = namesdict[obj["_id"].ToString()];
                                    if (name.Length > 0)
                                        obj["name"] = name;
                                }
                            }
                            catch { }
                            try
                            {


                                if (namesdict4.ContainsKey(obj["_id"].ToString()))
                                {
                                    string nameasset = namesdict4[obj["_id"].ToString()];
                                    //if (nameasset.Length > 0)
                                       // obj["nameassetType"] = nameasset;
                                }
                            }
                            catch { }

                            newja.Add(obj);
                        }
                        objectsObject = newja;
                    }
                    catch
                    {

                    }
               
                    objectsString = JsonConvert.SerializeObject(objectsObject);
                }
                else { //objects recursively

                   objectsString = GetAllSubObjects( parentCategory,skip1,filter);
                }

                String options="";
                try
                {
                    options = generateIndex(totalglobal, 5000);
                }
                catch { }
               
                result.Add("objects", objectsString);
                result.Add("total", totalglobal);
                result.Add("activos", numActivos);
                result.Add("dadosbaja", numBaja);
                result.Add("enMov", numMov);
                result.Add("options", "");
                ViewData["resultjson"] = JsonConvert.SerializeObject(result);
               
                return View("search");
               // return Json(JsonConvert.SerializeObject(result));
            }
            else
            {
                return null;
            }
        }

        public String getFormView2(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    String document = categoryTable.GetRow(profile);
                    JObject row = JsonConvert.DeserializeObject<JObject>(document);
                    String formString = row["customFields"].ToString();
                    String response = CustomForm.getFormView(formString, "ObjectFields"); //we use the CustomForm class to generate the form's fiew
                    //return response.Replace("HTKField", "HTKFieldDetalles");
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
                    String document = categoryTable.GetRow(profile);
                    JObject row = JsonConvert.DeserializeObject<JObject>(document);
                    String formString = row["customFields"].ToString();
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

        public JsonResult getusers(String profileid)
        {
            String userstring;
            JArray users = new JArray();
            if (profileid == "null" || profileid == null)
            {
                userstring = userTable.GetRows();
            }
            else {
                userstring = userTable.Get("profileId", profileid);
            }
             
            users = JsonConvert.DeserializeObject<JArray>(userstring);
            return Json(JsonConvert.SerializeObject(users));

        }

        public JsonResult getObjectFields()
        {
            String objstring;
            objstring = _objFieldsTable.GetRows();
            return Json(objstring);

        }

        public void AsignarUsuario(String selectids, String iduser) {
            String objstring="";
            JObject objs = new JObject();
            JArray arreglo = JsonConvert.DeserializeObject<JArray>(selectids);
            String jsonData;
            foreach (String cad in arreglo)
            {
                objstring = _objectTable.GetRow(cad);
                objs = JsonConvert.DeserializeObject<JObject>(objstring);
                objs["userid"] = iduser;

                jsonData = JsonConvert.SerializeObject(objs);

                _objectTable.SaveRow(jsonData,cad);
                _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + cad, "ObjectReal", DateTime.Now.ToString());
                //guardar en usuario
                userTable.UpdateObjects(iduser, cad);

            }
        
        }
        public void AsignarSerie(String selectid, String serie)
        {
            String objstring = "";
            JObject objs = new JObject();
            String jsonData;

            objstring = _objectTable.GetRow(selectid);
            objs = JsonConvert.DeserializeObject<JObject>(objstring);
            JToken val = "";
            if (objs.TryGetValue("serie", out val))
            {
                objs["serie"] = serie;
            }
            else
            {
                objs.Add("serie", serie);
            }
            jsonData = JsonConvert.SerializeObject(objs);

            _objectTable.SaveRow(jsonData, selectid);
            _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + selectid, "ObjectReal", DateTime.Now.ToString());

        }
        public void AsignarUsuarioSelect(String selectid, String iduser)
        {
            String objstring = "";
            JObject objs = new JObject();
            String jsonData;

            objstring = _objectTable.GetRow(selectid);
            objs = JsonConvert.DeserializeObject<JObject>(objstring);
            objs["userid"] = iduser;

            jsonData = JsonConvert.SerializeObject(objs);

            _objectTable.SaveRow(jsonData, selectid);
            _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + selectid, "ObjectReal", DateTime.Now.ToString());
            //guardar en usuario
            userTable.UpdateObjects(iduser, selectid);
            
        }

        
        /// <summary>
        /// Returns all objects from a location
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public String GetAllSubObjects(string locationId,int skip=0,bool filter=false)
        {
            string results = "";
            String categoryString = "";
            JArray categories = new JArray();
            try
            {
                results = _objectTable.GetSubObjects(locationId,filter);
                JArray objectList = JsonConvert.DeserializeObject<JArray>(results);
                Dictionary<string, string> namesdict = new Dictionary<string, string>();
                try
                {
                 List<string> objreflist=(from obj in objectList select(string)obj["objectReference"]).ToList();
                 RivkaAreas.Reports.Models.ObjectsRealReport custom=new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
                 JArray objref = JsonConvert.DeserializeObject<JArray>(custom.GetbyCustom("_id", objreflist, "ReferenceObjects"));
                 foreach (JObject o in objreflist)
                 {
                     try
                     {
                         namesdict.Add(o["_id"].ToString(), o["name"].ToString());
                     }
                     catch { }
                 }
                
                }
                catch { }
                Dictionary<string, JObject> listrefobj = new Dictionary<string, JObject>();

                try
                {
                    List<string> listref = (from obj2 in objectList select (string)obj2["objectReference"]).ToList();
                    JArray jaref = JsonConvert.DeserializeObject<JArray>(_objectTable.getRefObj(listref));

                    foreach (JObject item in jaref)
                    {
                        try
                        {
                            listrefobj.Add(item["_id"].ToString().ToLower(), item);
                        }
                        catch { }
                    }
                }
                catch { }
                categoryString = categoryTable.GetRows();
                //doing changes to objects array
                categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                Dictionary<string, string> listCategories = new Dictionary<string, string>();
                foreach (JObject items in categories)
                {
                    listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                }
                int take = 0;
                totalglobal = objectList.Count();
                JArray paginationja = new JArray();
                List<string> idsact1 = new List<string>();
                Dictionary<string, string> actin = new Dictionary<string, string>();
                Dictionary<string, string> actout = new Dictionary<string, string>();
                JArray getdemandin = new JArray();
                JArray getdemandout = new JArray();
                try
                {
                    idsact1 = (from id in objectList select (string)id["_id"]).ToList();
                    try
                    {
                         getdemandin = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 0));
                      }
                    catch { }
                    try
                    {
                        getdemandout = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 1));
                        
                    }
                    catch { }
                }
                catch
                {

                }
                List<string> idsact=new List<string>();
                foreach (JObject document in objectList) //for each profile we create an option element with id as value and the name as the text
                {
                    // if (objectList.IndexOf(document) < skip)
                    /* if(indexfor>skip)
                     {
                         //continue;
                         return;
                     }
                     else
                     {
                         /*if (take == 5000)

                             break;
                           take++;*/
                    /*   if (indexfor >= 5000)
                           pls.Stop();

                   }*/
                    try
                    {
                        idsact.Add(document["_id"].ToString());
                    }
                    catch
                    {

                    }
                    try
                    {
                        JObject objo = new JObject();
                        JToken tk;
                        if (listrefobj.TryGetValue(document["objectReference"].ToString().ToLower(), out objo))
                        {
                            if (!document.TryGetValue("name", out tk))
                                document.Add("name", "");
                            if (!document.TryGetValue("modelo", out tk))
                                document.Add("modelo", "");
                            if (!document.TryGetValue("marca", out tk))
                                document.Add("marca", "");
                            if (!document.TryGetValue("nameassetType", out tk))
                                document.Add("nameassetType", "");
                            if (!document.TryGetValue("object_id", out tk))
                                document.Add("object_id", "");
                            if (!document.TryGetValue("proveedor", out tk))
                                document.Add("proveedor", "");
                            if (!document.TryGetValue("object_id_ref", out tk))
                                document.Add("object_id_ref", "");
                            try
                            {

                                document["object_id_ref"] = objo["object_id"].ToString();
                            }
                            catch { }
                            try
                            {
                                document["nameassetType"] = objo["nameassetType"].ToString();
                            }
                            catch { }
                            try
                            {

                                document["name"] = objo["name"].ToString();
                            }
                            catch { }
                            try
                            {

                                document["marca"] = objo["marca"].ToString();
                            }
                            catch { }
                            try
                            {
                                document["modelo"] = objo["modelo"].ToString();
                            }
                            catch { } 
                            try
                            {
                                document["proveedor"] = objo["proveedor"].ToString();
                            }
                            catch { }
                            string mm = "";
                            if (namesdict.TryGetValue(document["objectReference"].ToString(), out mm))
                            {
                                document["name"] = mm;
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                            document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);
                    }
                    catch { }
                    document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();

                    try
                    {
                        //    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                        document["currentmove"] = "";
                        foreach (JObject item in getdemandin)
                        {
                            try
                            {
                                foreach (JObject obj in item["objects"])
                                {
                                    try
                                    {
                                        if (obj["id"].ToString() == document["_id"].ToString())
                                        {
                                            document["currentmove"] = item["folio"].ToString() + " " + item["namemov"].ToString();
                                            break;
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        document["currentmove"] = "";
                    }

                    try
                    {
                        //  document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                        document["allmoves"] = "";
                        List<string> folioslist = new List<string>();
                        foreach (JObject item in getdemandout)
                        {
                            try
                            {
                                foreach (JObject obj in item["objects"])
                                {
                                    try
                                    {
                                        if (obj["id"].ToString() == document["_id"].ToString())
                                        {
                                            folioslist.Add(item["folio"].ToString() + " " + item["namemov"].ToString());
                                            break;
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                        try
                        {
                            document["allmoves"] = String.Join(",\n ", folioslist);
                        }
                        catch
                        {
                            document["allmoves"] = ".";
                        }
                    }
                    catch (Exception ex)
                    {
                        document["allmoves"] = "";
                    }

                    try
                    {
                        if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                        {
                            document["status"] = "En movimiento";
                            numMov++;
                        }
                        else
                        {
                            if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                            {
                                document["status"] = "Dado de baja";
                                numBaja++;
                            }
                            else
                            {
                                document["status"] = "Está en tu oficina";
                                numActivos++;
                            }

                        }

                    }
                    catch
                    {
                        document["status"] = "Está en tu oficina";
                        numActivos++;
                    }
                    try
                    {
                        if (document["label"].ToString() == "normal")
                            document["etiquetado"] = "Normal";
                        else
                            document["etiquetado"] = "No Etiquetable";
                    }
                    catch { }
                    try
                    {
                      /*  if (document["assetType"].ToString().ToLower().Contains("system"))
                            document["nameassetType"] = "Sistemas";
                        else if (document["assetType"].ToString().ToLower().Contains("maintenance"))
                            document["nameassetType"] = "Mantenimiento";
                       else
                        {
                            document["nameassetType"] = "Vinos";
                        }*/
                    }
                    catch { }
                    if (departs != null)
                    {
                        try
                        {
                            if (departs.ContainsKey(document["department"].ToString()))
                                document["departmentName"] = departs[document["department"].ToString()];
                        }
                        catch
                        {
                            document["departmentName"] = "";
                        }
                    }
                    else
                    {
                        document["departmentName"] = "";
                    }
                    if (document["ext"].ToString() != "")
                    {
                        document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                        document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]);
                    }

                    paginationja.Add(document);
                }
                objectList = paginationja;
                try
                {
                    RivkaAreas.Reports.Models.ObjectsRealReport objdc=new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
                    JArray actjo = JsonConvert.DeserializeObject<JArray>(objdc.GetbyCustom("_id", idsact, "ObjectReal"));
                    Dictionary<string, string> namesdict2 = actjo.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                    Dictionary<string, string> namesdict4 = actjo.ToDictionary(x => (string)x["_id"], x => (string)x["assetType"]);
                   
                    JArray newja = new JArray();
                    foreach (JObject obj in objectList)
                    {
                        try {

                            JToken tk;
                            if (namesdict2.ContainsKey(obj["_id"].ToString()))
                            {
                               string name= namesdict2[obj["_id"].ToString()];
                               if (name.Length > 0)
                               {
                                   //obj["name"] = name;
                               }
                            }
                        }
                        catch { }
                        try
                        {

                          
                            if (namesdict4.ContainsKey(obj["_id"].ToString()))
                            {
                                string nameasset = namesdict4[obj["_id"].ToString()];
                                //if (nameasset.Length > 0)
                                 //   obj["nameassetType"] = nameasset;
                            }
                        }
                        catch { }

                        newja.Add(obj);
                    }
                    objectList = newja;
                }
                catch
                {

                }
               
                return objectList.ToString();
            }
            catch (Exception e)
            {
                Error.Log(e, "Trying to get all Objects");
            }


            return results;
        }

        /// <summary>
        /// Get all objects from a location
        /// </summary>
        /// <param name="locationid"></param>
        /// <returns></returns>
        public String GetAllObjects(string locationid)
        {
            String objectsString = "";
            String categoryString = "";
            JArray objectsObject = new JArray();

            String locationsString = "";
            JArray locationsObject = new JArray();
            JArray categories = new JArray();
            objectsString = _objectTable.GetObjects(locationid);
            categoryString = categoryTable.GetRows();
            //doing changes to objects array
            objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
            categories = JsonConvert.DeserializeObject<JArray>(categoryString);

            Dictionary<string, string> listCategories = new Dictionary<string, string>();
            foreach (JObject items in categories)
            {
                listCategories.Add(items["_id"].ToString(), items["name"].ToString());
            }

            foreach (JObject document in objectsObject)
            {
                if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                    document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);

                document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();
                document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                try
                {
                    document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                }
                catch (Exception ex)
                {
                    document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                }
                try
                {
                    if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                    {
                        document["status"] = "En movimiento";

                    }
                    else
                    {
                        if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                        {
                            document["status"] = "Dado de baja";
                        }
                        else
                        {
                            document["status"] = "Está en tu conjunto";
                        }

                    }

                }
                catch
                {
                    document["status"] = "Está en tu conjunto";
                }
                if (document["label"].ToString() == "normal")
                    document["etiquetado"] = "Normal";
                else
                    document["etiquetado"] = "No Etiquetable";

                /*if (document["assetType"].ToString() == "system")
                    document["nameassetType"] = "Sistemas";
                else if (document["assetType"].ToString() == "maintenance")
                    document["nameassetType"] = "Mantenimiento";
                else
                {
                    document["nameassetType"] = "Proyección y Sonido";
                }
                */
                if (departs != null)
                {
                    try
                    {
                        if (departs.ContainsKey(document["department"].ToString()))
                            document["departmentName"] = departs[document["department"].ToString()];
                    }
                    catch
                    {
                        document["departmentName"] = "";
                    }

                }
                else
                {
                    document["departmentName"] = "";
                }
                try
                {
                    if (document["ext"].ToString() != "")
                    {
                        document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                    }

                }
                catch (Exception e) { /*Ignored*/ }


            }

            objectsString = JsonConvert.SerializeObject(objectsObject);
            string objs = "";
            locationsString = locationTable.Get("parent", locationid);
            locationsObject = JsonConvert.DeserializeObject<JArray>(locationsString);
            foreach(JObject obj in locationsObject){

                objs=GetAllObjects(obj["_id"].ToString());

                if (objs == "" || objs == "[]") continue;
                if (objectsString == "[]") objectsString = objectsString.Replace("]", "");
                else
                   objectsString= objectsString.Replace(']', ',');
                   objs= objs.Replace("[","");
                   objectsString +=objs;
                
                
            }


            return objectsString;
        }

        public String GetHowToShowObjects(String idlocation) {
            String result = "";
            idlocation = (idlocation == "") ? "null" : idlocation;
            String rowString="";
            JObject rowArray = new JObject();
            if (idlocation == "null" || idlocation == null)
            {
                rowString = locationTable.Get("_id","null");
                rowArray = JsonConvert.DeserializeObject<JArray>(rowString).First() as JObject;
            }
            else
            {
                rowString = locationTable.GetRow(idlocation);
                 rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
            }
           
            
            JObject obj= new JObject();
            rowString=locationProfileTable.GetRow(rowArray["profileId"].ToString());
            
            try {
                obj = JsonConvert.DeserializeObject<JObject>(rowString);
                result = obj["vertodo"].ToString();
            }
            catch (Exception) {
                result = "0";
            }

            return result;
        }

        public JsonResult getDataByObjectReference(String objectReference = "null")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (objectReference == "") objectReference = "null";
                String objectsString = "";
                String categoryString = "";
                JArray objectsObject = new JArray();
                JArray categories = new JArray();

                objectsString = _objectTable.GetObjects(null, objectReference);
                categoryString = categoryTable.GetRows();

                //doing changes to objects array
                objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                Dictionary<string, string> listCategories = new Dictionary<string, string>();

                foreach (JObject items in categories)
                {
                    listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                }

                foreach (JObject document in objectsObject)
                {
                    if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                        document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);

                    document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();
                    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                    try
                    {
                        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    }
                    catch (Exception ex)
                    {
                        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    }
                    try
                    {
                        if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                        {
                            document["status"] = "En movimiento";

                        }
                        else
                        {
                            if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                            {
                                document["status"] = "Dado de baja";
                            }
                            else
                            {
                                document["status"] = "Está en tu conjunto";
                            }

                        }

                    }
                    catch
                    {
                        document["status"] = "Está en tu conjunto";
                    }
                    if (document["label"].ToString() == "normal")
                        document["etiquetado"] = "Normal";
                    else
                        document["etiquetado"] = "No Etiquetable";

                   /* if (document["assetType"].ToString() == "system")
                        document["nameassetType"] = "Sistemas";
                    else if (document["assetType"].ToString() == "maintenance")
                        document["nameassetType"] = "Mantenimiento";
                    else
                    {
                        document["nameassetType"] = "Proyección y Sonido";
                    }*/

                    if (departs != null)
                    {
                        try
                        {
                            if (departs.ContainsKey(document["department"].ToString()))
                                document["departmentName"] = departs[document["department"].ToString()];
                        }
                        catch
                        {
                            document["departmentName"] = "";
                        }

                    }
                    else
                    {
                        document["departmentName"] = "";
                    }
                    try
                    {
                        if (document["ext"].ToString() != "")
                        {
                            document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                        }
                    }
                    catch (Exception e) { /*Ignored*/ }
                }
                objectsString = JsonConvert.SerializeObject(objectsObject);
                

                JObject result = new JObject();
                result.Add("objects", objectsString);
                return Json(JsonConvert.SerializeObject(result));
            }
            else
            {
                return null;
            }
        }

        public List<BsonValue> GetObjectsReference(String texto) {
            List<BsonValue> valores = new List<BsonValue>();
            String objstring;
            JArray objs = new JArray();
            objstring = _objectReferenceTable.GetRows();
            objs = JsonConvert.DeserializeObject<JArray>(objstring);
            foreach (JObject ob in objs) {
                JObject obj1 = JsonConvert.DeserializeObject<JObject>(ob["profileFields"].ToString());
                foreach(KeyValuePair<String, JToken> token in obj1){
                    if (token.Value.ToString().Contains(texto)) {
                        valores.Add(ob["_id"].ToString());
                        break;
                    }
                }
            }
            return valores;
        
        }

        public JsonResult getDataByTexto2(String texto = "")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (texto == "") texto = "null";
                String objectsString = "";
                String categoryString = "";
                JArray objectsObject = new JArray();
                List<BsonValue> valores = GetObjectsReference(texto);
                JArray categories = new JArray();

              //  objectsString = _objectTable.GetObjectsByText(texto, valores);
                categoryString = categoryTable.GetRows();
                //doing changes to objects array
                objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                Dictionary<string, string> listCategories = new Dictionary<string, string>();

                foreach (JObject items in categories)
                {
                    listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                }

                foreach (JObject document in objectsObject)
                {
                    if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                        document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);
                    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                    try
                    {
                        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    }
                    catch (Exception ex)
                    {
                        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    }
                    try
                    {
                        if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                        {
                            document["status"] = "En movimiento";

                        }
                        else
                        {
                            if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                            {
                                document["status"] = "Dado de baja";
                            }
                            else
                            {
                                document["status"] = "Está en tu conjunto";
                            }

                        }

                    }
                    catch
                    {
                        document["status"] = "Está en tu conjunto";
                    }
                    if (document["label"].ToString() == "normal")
                        document["etiquetado"] = "Normal";
                    else
                        document["etiquetado"] = "No Etiquetable";

                   /* if (document["assetType"].ToString() == "system")
                        document["nameassetType"] = "Sistemas";
                    else if (document["assetType"].ToString() == "maintenance")
                        document["nameassetType"] = "Mantenimiento";
                    else
                    {
                        document["nameassetType"] = "Proyección y Sonido";
                    }*/

                    if (departs != null)
                    {
                        try
                        {
                            if (departs.ContainsKey(document["department"].ToString()))
                                document["departmentName"] = departs[document["department"].ToString()];
                        }
                        catch
                        {
                            document["departmentName"] = "";
                        }

                    }
                    else
                    {
                        document["departmentName"] = "";
                    }
                    try
                    {
                        if (document["ext"].ToString() != "")
                        {
                            document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                        }
                    }
                    catch (Exception e) { /*Ignored*/ }
                }
                objectsString = JsonConvert.SerializeObject(objectsObject);


                JObject result = new JObject();
                result.Add("objects", objectsString);
                return Json(JsonConvert.SerializeObject(result));
            }
            else
            {
                return null;
            }
        }

        public ActionResult getDataByTexto(String texto = "")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (texto == "") texto = "null";
                String objectsString = "";
                String categoryString = "";
                JArray objectsObject = new JArray();
                List<BsonValue> valores = new List<BsonValue>();//GetObjectsReference(texto);
                JArray categories = new JArray();
                List<string> locationsids = new List<string>();
                List<string> usersids = new List<string>();
                List<string> assetTypenames = new List<string>();
                List<string> refobjsids = new List<string>();
                try
                {
                    JArray locs = JsonConvert.DeserializeObject<JArray>(locationTable.GetLocationsByText(texto));
                    locationsids = (from loc in locs select (string)loc["_id"]).ToList();
                    RivkaAreas.Reports.Models.ObjectsRealReport objdb= new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
                    JArray sublocations=JsonConvert.DeserializeObject<JArray>(objdb.GetbyCustom("parent",locationsids,"Locations"));
                    List<String> sublocationsids = (from loc in sublocations select (string)loc["_id"]).ToList();
                 locationsids.AddRange(sublocationsids);
                }
                catch
                { }
                try
                {
                    JArray refobj = JsonConvert.DeserializeObject<JArray>(_objectTable.GetRefObjByText(texto));
                    refobjsids = (from refe in refobj select (string)refe["_id"]).ToList();
                }
                catch { }
                try
                {
                    JArray users = JsonConvert.DeserializeObject<JArray>(userTable.GetUserByText(texto));
                    usersids = (from userx in users select (string)userx["_id"]).ToList();
                }
                catch { }
                try
                {
                    JArray categor = JsonConvert.DeserializeObject<JArray>(categoryTable.GetCatByText(texto));
                    assetTypenames = (from cate in categor select (string)cate["name"]).ToList();
                    foreach (JObject category in categor)
                    {
                        switch (category["name"].ToString())
                        {
                            case "Sistemas":
                                assetTypenames.Add("system");
                                break;
                            case "Mantenimiento":
                                assetTypenames.Add("maintenance");
                                break;
                            case "Proyección y sonido":
                                assetTypenames.Add("sound");
                                break;

                        }
                    }
                }
                catch { }
                objectsString = _objectTable.GetObjectsByText(texto, valores, locationsids, usersids, assetTypenames, refobjsids);
                categoryString = categoryTable.GetRows();
                //doing changes to objects array
                objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                try
                {
                    foreach (JObject item in objectsObject)
                    {
                        try
                        {
                            item.Add("nameCreator", item["Creator"].ToString() + " " + item["lastname"].ToString());
                        }
                        catch { }
                    }
                }
                catch { }
                categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                Dictionary<string, string> listCategories = new Dictionary<string, string>();

                foreach (JObject items in categories)
                {
                    listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                }

                foreach (JObject document in objectsObject)
                {
                    JToken tk;
                    try
                    {
                        if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                            document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);
                        document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());

                        if (!document.TryGetValue("status", out tk))
                        {
                            document.Add("status", "");
                        }
                        if (!document.TryGetValue("label", out tk))
                        {
                            document.Add("label", "normal");
                        }
                        if (!document.TryGetValue("etiquetado", out tk))
                        {
                            document.Add("etiquetado", "");
                        }
                        if (!document.TryGetValue("assetType", out tk))
                        {
                            document.Add("assetType", "");
                        }
                        if (!document.TryGetValue("nameassetType", out tk))
                        {
                            document.Add("nameassetType", "");
                        }
                        if (!document.TryGetValue("departmentName", out tk))
                        {
                            document.Add("departmentName", "");
                        }
                        if (!document.TryGetValue("department", out tk))
                        {
                            document.Add("department", "");
                        }

                        if (document["status"].ToString() == "") document["status"] = "Está en tu conjunto";

                        if (document["label"].ToString() == "normal")
                            document["etiquetado"] = "Normal";
                        else
                            document["etiquetado"] = "No Etiquetable";

                        if (document["assetType"].ToString() == "system")
                            document["nameassetType"] = "Sistemas";
                        else if (document["assetType"].ToString() == "maintenance")
                            document["nameassetType"] = "Mantenimiento";
                        else
                        {
                            document["nameassetType"] = "Proyección y Sonido";
                        }

                        if (departs != null)
                        {
                            try
                            {
                                if (departs.ContainsKey(document["department"].ToString()))
                                    document["departmentName"] = departs[document["department"].ToString()];
                            }
                            catch
                            {
                                document["departmentName"] = "";
                            }

                        }
                        else
                        {
                            document["departmentName"] = "";
                        }
                        try
                        {
                            if (document["ext"].ToString() != "")
                            {
                                document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                            }
                        }
                        catch (Exception e) { /*Ignored*/ }
                    }
                    catch
                    {

                    }
                }
               
                objectsString = JsonConvert.SerializeObject(objectsObject);
                totalglobal = objectsObject.Count();

                JObject result = new JObject();
                result.Add("objects", objectsString);
                result.Add("total", totalglobal);
               
                ViewData["resultjson"] = JsonConvert.SerializeObject(result);
              //  return Json(JsonConvert.SerializeObject(result));
             return   View("search");
            }
            else
            {
                return null;
            }
            return null;
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


        public String updateLocation(string id, string newlocation)
        {
            if (newlocation == "") newlocation = "null";
            if (this.Request.IsAjaxRequest())
            {
                if (id != "")
                {
                    String obj = _objectTable.GetRow(id);
                    var newobj = JsonConvert.DeserializeObject<JObject>(obj);

                    newobj["location"] = newlocation;
                    _objectTable.SaveRow(JsonConvert.SerializeObject(newobj), id);
                    _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + id, "ObjectReal", DateTime.Now.ToString());
                    return "success";
                }

            }

            return null;
        }

        public string loadLocations(string conjunto)
        {

            try
            {
                String locationsOptions = "";
                String rowArray = locationTable.Get("parent", conjunto);
                JArray locatList = JsonConvert.DeserializeObject<JArray>(rowArray);

                locationsOptions += "<option value='null' selected> Seleccione Ubicacion</option>";

                foreach (JObject document in locatList) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["name"].ToString() != "")
                    {
                        locationsOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                        locationsOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                    }

                }


                return locationsOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String getRoute2(String parentCategory = "null")
        {
            //Creating the route data
            String route = "";
            List<string> rutas = new List<string>();
            while (parentCategory != "null" && parentCategory != "")
            {

                string category = locationTable.GetRow(parentCategory);
                JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                rutas.Add(actualCategory["name"].ToString());
                parentCategory = actualCategory["parent"].ToString();
            }

            for (int i = rutas.Count; i > 0; i--) {
                route = route + rutas[i-1] + "/";
            }

                return route;
        }

        public JArray getRoute3(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {

                string category = locationTable.GetRow(parentCategory);
                JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                route.Add(actualCategory["_id"].ToString());
                parentCategory = actualCategory["parent"].ToString();
            }

            return route;
        }


        public String ValidateLocationUser(String locationid, String userid) {
            String cadena = "";
            String userarray = userTable.GetRow(userid);
            JObject userobj = JsonConvert.DeserializeObject<JObject>(userarray);
            JArray ele = new JArray();
            List<String> list3 = new List<String>();
            List<String> list2 = new List<String>();

            ele = getRoute3(locationid);
            foreach (String ob in ele)
            {
                list3.Add(ob);
            }

            JObject positionUSer = new JObject();
            try
            {
                positionUSer = JsonConvert.DeserializeObject<JObject>(_userprofileTable.GetRow(userobj["profileId"].ToString()));
            }
            catch
            {
                positionUSer.Add("name", "");
            }
            if (positionUSer["name"].ToString() == "Gerente de conjunto" || positionUSer["name"].ToString() == "Gerente regional")

            {
              
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());
                foreach (JObject l in locats)
                    list2.Add(l["id"].ToString());
                      /* END: Patch*/
                if (list2.Intersect<string>(list3).ToList<string>().Count > 0)
                {
                    cadena = "1";                
                }
            }
            return cadena;
        }

        public void loadDepartments()
        {
            try
            {
                String DepartmentsOptions = "";
                String rowArray = _listTable.Get("name", "departments");
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray listas = new JArray();
                foreach (JObject obj in rowString)
                {
                    listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["unorder"].ToString());
                }
                DepartmentsOptions += "<option value='null' selected> Seleccione Departamento</option>";
                foreach (JObject puesto in listas)
                {
                    foreach (KeyValuePair<string, JToken> token in puesto)
                    {
                        DepartmentsOptions += "<option value='" + token.Key + "'"; //setting the id as the value
                        DepartmentsOptions += ">" + token.Value + "</option>"; //setting the text as the name
                        departs.Add(token.Key, token.Value.ToString());
                    }

                }
                ViewData["departList"] = new HtmlString(DepartmentsOptions);
            }
            catch (Exception e)
            {
                ViewData["departList"] = null;
            }
        }

        public void loadproveedores()
        {
            try
            {
                String DepartmentsOptions = "";
                String rowArray = _listTable.Get("name", "proveedores");
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray listas = new JArray();
                foreach (JObject obj in rowString)
                {
                    listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["order"].ToString());
                }
                DepartmentsOptions += "<option value='null' selected> Seleccione Proveedor</option>";
                foreach (JObject puesto in listas)
                {
                    foreach (KeyValuePair<string, JToken> token in puesto)
                    {
                        if (token.Key == "position") continue;
                        DepartmentsOptions += "<option value='" + token.Key + "'"; //setting the id as the value
                        DepartmentsOptions += ">" + token.Value + "</option>"; //setting the text as the name
                    }

                }

                ViewData["proveedorList"] = new HtmlString(DepartmentsOptions);
            }
            catch (Exception e)
            {
                ViewData["proveedorList"] = null;
            }
        }

        public String saveObject(string  objeto)
        {
            //if (id == "null" || id == "") //differents ways to receive null from javascript
            //{
            //    id = null;
            //}

            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objects", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                JObject datos = JsonConvert.DeserializeObject<JObject>(objeto);
                String objectID = (datos["objectID"].ToString() == "null" || datos["objectID"].ToString() == "") ? null : datos["objectID"].ToString(); //is this an insert or an update?, converting null in javascript to null in c#
                JObject newObject = new JObject();
                String obj = _objectTable.GetRow(objectID); 
                /*the gived id does not exists*/
                if (objectID != null && (objectID == null))
                {
                    return "El id especificado no existe";
                }
                if (obj != null) {
                    newObject = JsonConvert.DeserializeObject<JObject>(obj);
                }
                JToken jk;
                if (!newObject.TryGetValue("fechafactura", out jk))
                    newObject.Add("fechafactura", "");
                if (!newObject.TryGetValue("observation", out jk))
                    newObject.Add("observation", "");
                if (!newObject.TryGetValue("userlabel", out jk))
                    newObject.Add("userlabel", "");
                if (!newObject.TryGetValue("vale", out jk))
                    newObject.Add("vale", "");
                if (!newObject.TryGetValue("filefactura", out jk))
                    newObject.Add("filefactura", "");
                if (!newObject.TryGetValue("objectfile", out jk))
                    newObject.Add("objectfile", "");
                if (!newObject.TryGetValue("garantia", out jk))
                    newObject.Add("garantia", "");
                if (!newObject.TryGetValue("lastmaintenance", out jk))
                    newObject.Add("lastmaintenance", "");
               if (!newObject.TryGetValue("nextmaintenance", out jk))
                    newObject.Add("nextmaintenance", "");
                if (!newObject.TryGetValue("comments", out jk))
                    newObject.Add("comments", "");
                if (!newObject.TryGetValue("price", out jk))
                    newObject.Add("price", "");
                if (!newObject.TryGetValue("date", out jk))
                    newObject.Add("date", "");
                 
                newObject["name"] = datos["name"].ToString();
              //  newObject["location"] = datos["idlocation"].ToString();
                newObject["price"] = datos["precio"].ToString();
                newObject["date"] = datos["fecha"].ToString();
                newObject["department"] = datos["departamento"].ToString();
               // newObject["marca"] = datos["marca"].ToString();
               // newObject["modelo"] = datos["modelo"].ToString();
               // newObject["perfil"] = datos["perfil"].ToString();
                //newObject["object_id"] = (datos["object_id"].ToString().Length > 0) ? datos["object_id"].ToString() : newObject["object_id"].ToString();
                newObject["folio"] = datos["folio"].ToString();
               // newObject["proveedor"] = datos["proveedor"].ToString();
                newObject["num_pedido"] = datos["pedido"].ToString();
                newObject["num_solicitud"] = datos["solicitud"].ToString();
                newObject["num_reception"] = datos["recepcion"].ToString();
                newObject["num_ERP"] = datos["num_ERP"].ToString();
                newObject["serie"] = datos["serie"].ToString();
                newObject["factura"] = datos["factura"].ToString();
                newObject["RH"] = datos["RH"].ToString();
                
                newObject["fechafactura"] = datos["fechafactura"].ToString();
                newObject["observation"] = datos["observation"].ToString();
                newObject["userlabel"] = datos["userlabel"].ToString();
                newObject["vale"] = datos["vale"].ToString();
                newObject["filefactura"] = datos["filefactura"].ToString();
                newObject["objectfile"] = datos["objectfile"].ToString();
                newObject["garantia"] = datos["garantia"].ToString();
                newObject["lastmaintenance"] = datos["lastmaintenance"].ToString();
                newObject["nextmaintenance"] = datos["nextmaintenance"].ToString();
                newObject["comments"] = datos["comments"].ToString();
               
                
                
               
                try
                {

                }
                catch { }

                //JObject profileFields = new JObject();

                ////foreach element in the formData, let's append it to the jsonData in the profileFields
                //foreach (String key in data.Keys)
                //{
                //    profileFields.Add(key, data[key]);
                //}
                //objectArray.Add("profileFields", profileFields);
                try
                {
                    newObject["assetType"] =datos["assetType"].ToString();
                }
                catch (Exception e) { }

                string id = _objectTable.SaveRow(JsonConvert.SerializeObject(newObject), objectID);
                _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + id, "ObjectReal", DateTime.Now.ToString());
                return id;
            }
            else { return null; }
        }

    }
}
