using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using RivkaAreas.ObjectAdmin.Models;
using Rivka.Db;
using Rivka.Form;
using Rivka.Form.Field;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MongoDB.Bson;
using Rivka.Db.MongoDb;
using System.Globalization;
using Rivka.Security;
using RivkaAreas.Message.Controllers;
using Rivka.Mail;
using RivkaAreas.Rule;
using Rivka.Error;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.xml;
using iTextSharp.text.html.simpleparser;
using RivkaAreas.User.Controllers;
using System.Text;
namespace RivkaAreas.ObjectAdmin.Controllers
{
    [Authorize]
    public class DemandController : Controller
    {
        //
        // GET: /ObjectAdmin/Demand/
        protected Notifications classNotifications;
        protected ObjectReal _objectTable;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        protected Demand _demandTable;
        protected ProfileTable _profileTable;
        protected AuthorizationTable _autoTable;
        protected UserTable _userTable;
        protected DemandAuthorization _demandautoTable;
        protected ObjectTable _objectReferenceTable;
        protected CategoryTable _categoryTable;
        protected UserProfileTable _userProfileTable;
        protected MongoModel _movementProfileTable;
        protected ProcessesTable _processesTable;
        protected LocationTable _locationTable;
        protected LocationProfileTable locationsProfilesdb;
        protected ListTable _listTable;
        protected ValidateLimits validatelim;
        protected MessageController message;
        protected Messenger messenger;
        protected validatePermissions validatepermissions;
        protected RulesChecker validateRule;
        protected AdjudicatingTable _dictaminadorTable;
        protected WitnessTable _witnessTable;
        protected LoginController loginC = new LoginController();
        public Dictionary<string, Dictionary<int, string>> movementStatus = new Dictionary<string, Dictionary<int, string>>();

        protected MessageController messagesC;
        public DemandController()
        {
            //this._common = new CommonFunctions();
            classNotifications = new Notifications();
            this._objectTable = new ObjectReal("ObjectReal");
            this._demandTable = new Demand("Demand");
            this._profileTable = new ProfileTable("MovementProfiles");
            this._autoTable = new AuthorizationTable();
            this._userTable = new UserTable();
            this._demandautoTable = new DemandAuthorization("DemandAuthorization");
            this._objectReferenceTable = new ObjectTable();
            this._categoryTable = new CategoryTable();
            this._userProfileTable = new UserProfileTable();
            this._processesTable = new ProcessesTable();
            this._locationTable = new LocationTable();
            this._movementProfileTable = new MongoModel("MovementProfiles");
            this.locationsProfilesdb = new LocationProfileTable();
            this._listTable = new ListTable();
            this._witnessTable = new WitnessTable();
            this._logTable = new LogBook.Controllers.LogBookController();

            validatelim = new ValidateLimits();
            message = new MessageController();
            messenger = new Messenger();
            messagesC = new MessageController();
            validatepermissions = new validatePermissions();
            this._dictaminadorTable = new AdjudicatingTable();
            this.movementStatus.Add("delete", new Dictionary<int, string>(){
                {1,"DICTAMEN PENDIENTE"},
                {2,"CON DICTAMEN Y EN ESPERA DE DATOS ADICIONALES"},
                {3,"CON DICTAMEN, DATOS ADICIONALES Y EN ESPERA DE AUTORIZACIONES"},
                {4,"CON DICTAMEN, DATOS ADICIONALES, AUTORIZACIONES Y EN ESPERA DE COMPROBANTES"},
                {5,"CON DICTAMEN, DATOS ADICIONALES, AUTORIZACIONES, COMPROBANTES Y EN ESPERA DEL VOBO"},
                {6,"SOLICITUD AUTORIZADA Y APLICADA"},
                {7,"DENEGADA"},
                {9,"CON DICTAMEN, DATOS ADICIONALES, AUTORIZACIONES Y EN ESPERA DE CONTABILIDAD"}
            });

            this.movementStatus.Add("create", new Dictionary<int, string>(){
                {1,"SOLICITUD EN PROCESO"},
                {2,"SOLICITUD EN PROCESO"},
                {3,"SOLICITUD EN PROCESO"},
                {4,"SOLICITUD EN PROCESO"},
                {5,"ESPERA DE VISTO BUENO"},
                {6,"SOLICITUD AUTORIZADA Y APLICADA"},
                {7,"DENEGADA"},
            });

            this.movementStatus.Add("movement", new Dictionary<int, string>(){
                {1,"SOLICITUD EN PROCESO"},
                {2,"SOLICITUD EN PROCESO"},
                {3,"SOLICITUD EN PROCESO"},
                {4,"SOLICITUD EN PROCESO"},
                {5,"ESPERA DE VISTO BUENO"},
                {6,"SOLICITUD AUTORIZADA Y APLICADA"},
                {7,"DENEGADA"},
                {8,"regreso"}
            });

            this.movementStatus.Add("temporal", new Dictionary<int, string>(){
                {3,"SOLICITUD EN ESPERA DE APROBACIÓN"},
                {8,"LOS ACTIVOS YA HAN SALIDO"},
                {6,"SOLICITUD AUTORIZADA Y APLICADA"},
                {7,"DENEGADA"},
                {5,"LOS ACTIVOS YA HAN REGRESADO"}
            });
        }

        /// <summary>
        ///     Allows to delete an uploaded file
        /// </summary>
        /// <param name="name"> The file's name to delete</param>
        /// <returns>
        ///     Returns success or error
        /// </returns>
        /// <author>Luis Gonzalo Quijada Romero</author>
        public string deleteObjectFile(String name)
        {
            if (this.Request.IsAjaxRequest())
            {
                string relativepath = "\\Uploads\\Dictamenes\\imagenes";
                string absolutepath = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absolutepath))
                {
                    try
                    {
                        System.IO.File.Delete(absolutepath + "\\" + name);
                        return "success";
                    }
                    catch (Exception e)
                    {
                        this.Response.StatusCode = 400;
                        return "error";
                    }
                }
            }
            this.Response.StatusCode = 403;
            return "error";
        }

        public string updateTemporal(String id, String data,String extra=null, HttpPostedFileBase file = null)
        {
            String demandString = _demandTable.GetRow(id);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandString);
            JArray dataalls = JsonConvert.DeserializeObject<JArray>(data);
            JObject extraobj = new JObject();
            if(extra!=null)
                extraobj = JsonConvert.DeserializeObject<JObject>(extra);

            JArray objetos = new JArray();

          //  demand["updateInfo"] = dataalls;
            if (demand["status"].ToString() == "8") {
                foreach (JObject ob in dataalls)
                {
                    foreach (JObject ob1 in demand["objects"])
                    {
                        if (ob1["id"].ToString() == ob["id"].ToString() && ob1["orderNumber"].ToString() == ob["orderNumber"].ToString())
                        {
                            ob1["repaired"] = ob["repaired"];
                            ob1["entry"] = ob["entry"];
                            ob1["repaired_motive"] = ob["repaired_motive"];
                            if (ob1["entry"].ToString() == "0") {
                                ob1["marca"] = ob["marca"];
                                ob1["modelo"] = ob["modelo"];
                                ob1["serie_old"] = ob1["serie"];
                                ob1["name_old"] = ob1["name"];
                                ob1["serie"] = ob["serie"];
                                ob1["name"] = ob["name"];
                                ob1["img1"] = ob["img1"];
                                ob1["img2"] = ob["img2"];
                                ob1["objectReference"] = ob["objReference"];
                            }
                            objetos.Add(ob1);
                        }
                    }
                }
                JObject new_extras = new JObject();

                foreach (KeyValuePair<string, JToken> token in extraobj)
                {
                    new_extras.Add(token.Key,token.Value);
                }
                foreach (KeyValuePair<string, JToken> token1 in JsonConvert.DeserializeObject<JObject>(demand["extras"].ToString()))
                {
                    try { new_extras.Add(token1.Key, token1.Value); }
                    catch { continue; }
                    
                }
                demand["extras"] = new_extras;
                demand["objects"] = objetos;
                demand["status"] = 5;
                demand["return_date"] = DateTime.Now.ToString();
            }
            
            demand.Remove("_id");
            demandString = JsonConvert.SerializeObject(demand);
            _demandTable.SaveRow(demandString, id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #"+demand["folio"], "Demand", DateTime.Now.ToString());

            string namemov = "";
            try
            {
                namemov = getNameMovement(demand["movement"].ToString());
            }
            catch (Exception ex) { }
            JArray recipients = new JArray();
            JArray attachments = new JArray();
            foreach (var dd in demand["approval"])
            {
                try
                {
                    recipients.Add(dd["id_user"].ToString());
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
            
            JArray objReferencias = JsonConvert.DeserializeObject<JArray>(demand["objects"].ToString());
            Dictionary<string, string> namerefdict = new Dictionary<string, string>();
            Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

            try
            {
                List<string> listconjunt = (from refe in objReferencias select (string)refe["location"]).ToList();
                String resultref = _objectTable.getParents(listconjunt);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

            }
            catch { }
            try
            {
                List<string> listactnames = (from refe in objReferencias select (string)refe["objectReference"]).ToList();
                String resultref = _objectTable.getCategory(listactnames);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

            }
            catch { }

            string usstring = _userTable.GetRow(demand["Creator"].ToString());
            JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

            string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

            String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
            StringBuilder act = new StringBuilder();
            string modelo = "";
            string marca = "";
            string idart = "";
            string serie = "";
            string ubicacion = "";
            string nombre = "";
            string cantq = "1";
            string category = "";
            string deniednote = "";
            string epc = "";
            bool denied = false;
            foreach (JObject nr in objReferencias)
            {
                JObject n;
                JToken nt2;
                if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                {
                    denied = false;
                    serie = nr["serie"].ToString();
                    cantq = nr["quantity"].ToString();
                    try
                    {
                        if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                            epc = nr["epc"].ToString();
                        else
                            epc = "No aplica";

                    }
                    catch (Exception ex)
                    {
                        epc = "No aplica";
                    }
                    if (n.TryGetValue("modelo", out nt2))
                        modelo = n["modelo"].ToString();
                    if (n.TryGetValue("marca", out nt2))
                        marca = n["marca"].ToString();
                    if (n.TryGetValue("object_id", out nt2))
                        idart = n["object_id"].ToString();
                    if (n.TryGetValue("name", out nt2))
                        nombre = n["name"].ToString();
                    if (n.TryGetValue("namecategory", out nt2))
                        category = n["namecategory"].ToString();

                    if (nr.TryGetValue("location", out nt2))
                    {
                        ubicacion = getNameLocation(nr["location"].ToString());
                    }
                    else { ubicacion = ""; }
                    if (nr.TryGetValue("denied_note", out nt2))
                    {
                        deniednote = nr["denied_note"].ToString();
                        denied = true;
                    }

                    if (denied == true)
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;color:red;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5><h5>Denegado por: " + deniednote + "</h5></div><br>");
                    }
                    else
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5></div><br>");
                    }

                }
            }
            string bodygeneric = "<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
            string customtitle = "<h4>Los activos ya han regresado y el movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " está en espera del visto bueno.<br>";
            HtmlString body = new HtmlString(customtitle + creadorstring+bodygeneric);

            string to = JsonConvert.SerializeObject(recipients);
            string attach = JsonConvert.SerializeObject(attachments);
            messagesC.SendMail(to, "Movimiento  de " + namemov + " con Folio #" + demand["folio"].ToString() + ",esta Pendiente del Visto Bueno. Los activos ya han regresado.", customtitle + bodygeneric, attach, "Sistema");


            return "success";
        }

        public string approveTemporal(String id, String data, HttpPostedFileBase file = null)
        {
            String demandString = _demandTable.GetRow(id);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandString);
            JArray dataalls = JsonConvert.DeserializeObject<JArray>(data);
            //string name = "";
            //string relativepath = "\\Uploads\\Dictamenes\\imagenes";
            //string absolutepath = Server.MapPath(relativepath);
            //if (!System.IO.Directory.Exists(absolutepath))
            //{
            //    System.IO.Directory.CreateDirectory(absolutepath);
            //}

            //try
            //{
            //    System.Drawing.Image bmpReturn = null;
            //    string ext=".jpeg";
            //    foreach (JObject objnew in dataalls["newObjects"])
            //    {
            //        try
            //        {
            //            bmpReturn = null;

            //            if (objnew["image"].ToString() != "" && objnew["image"].ToString().IndexOf("data:image/")!=-1)
            //            {
            //                var base64Data = Regex.Match(objnew["image"].ToString(), @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            //                var binData = Convert.FromBase64String(base64Data);
            //                var stream = new MemoryStream(binData);
            //                string fileName = objnew["id"].ToString() + DateTime.UtcNow.Ticks+ext;
            //                bmpReturn = System.Drawing.Image.FromStream(stream);
            //                bmpReturn.Save(absolutepath + "\\" + fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            //                objnew["image"] = fileName;
            //            }
            //            else
            //            {
            //                if (objnew["image"].ToString() != "")
            //                {
            //                    string name1 = objnew["image"].ToString().Split('/').Last();
            //                    objnew["image"] = name1;
            //                }
            //            }
            //        }
            //        catch (Exception ex) { }
            //        try
            //        {
            //            bmpReturn = null;

            //            if (objnew["image1"].ToString() != "" && objnew["image1"].ToString().IndexOf("data:image/") != -1)
            //            {
            //                var base64Data = Regex.Match(objnew["image1"].ToString(), @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            //                var binData = Convert.FromBase64String(base64Data);
            //                var stream = new MemoryStream(binData);
            //                string fileName = objnew["id"].ToString() + DateTime.UtcNow.Ticks + ext;
            //                bmpReturn = System.Drawing.Image.FromStream(stream);
            //                bmpReturn.Save(absolutepath + "\\" + fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            //                objnew["image1"] = fileName;
            //            }
            //            else
            //            {
            //                if (objnew["image1"].ToString() != "")
            //                {
            //                    string name1 = objnew["image1"].ToString().Split('/').Last();
            //                    objnew["image1"] = name1;
            //                }
            //            }
            //        }
            //        catch (Exception ex) { }

            //    }
            //}
            //catch (Exception ex)
            //{

            //}

            JArray objetos = new JArray();

            //  demand["updateInfo"] = dataalls;
            if (demand["status"].ToString() == "8")
            {
                foreach (JObject ob in dataalls)
                {
                    foreach (JObject ob1 in demand["objects"])
                    {
                        if (ob1["id"].ToString() == ob["id"].ToString() && ob1["orderNumber"].ToString() == ob["orderNumber"].ToString())
                        {
                            ob1["repaired"] = ob["repaired"];
                            ob1["entry"] = ob["entry"];
                            ob1["repaired_motive"] = ob["repaired_motive"];
                            objetos.Add(ob1);
                        }
                    }
                }

                demand["objects"] = objetos;
                demand["status"] = 5;
            }

            //try
            //{
            //    if (demand["updateInfo"]["newObjects"].Count()>0)
            //    {
            //        foreach (var item in demand["approval"])
            //        {
            //            item["approved"] = "0";
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            demand.Remove("_id");
            demandString = JsonConvert.SerializeObject(demand);
            _demandTable.SaveRow(demandString, id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "Demand", DateTime.Now.ToString());
            return "success";
        }

        /// <summary>
        ///     Allows to upload a file
        /// </summary>
        /// <param name="id">The id or name which is related to the file.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>
        ///     The name wich the file was stored
        /// </returns>
        /// <author>Luis Gonzalo Quijada Romero</author>
        public string saveObjectFile(String id, HttpPostedFileBase file)
        {
            if (this.Request.IsAjaxRequest())
            {
                String fileName = null;
                if (file != null)
                {
                    string ext = file.FileName.Split('.').Last(); //getting the extension
                    string name = id;
                    string relativepath = "\\Uploads\\Dictamenes\\imagenes";
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
            this.Response.StatusCode = 403;
            return null;
        }

        public string validateLimit(int cant, int count = 0, string msg = null, string folio = null)
        {
            try
            {
                string result = validatelim.validate("Demand", "total", null, "Objs_Reals", cant, false, count);
                string message1 = "";

                if (msg == null)
                {
                    message1 = "Ha llegado a su limite de Objetos Reales";
                }
                else
                {
                    message1 = msg;
                }
                if (result == "true")
                {

                }
                else if (result == "false")
                {

                }
                else
                {
                    string iduserthis = "";
                    string username = "";
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
                    try
                    {
                        username = Session["LoggedUser"].ToString();
                    }
                    catch (Exception ex)
                    {
                        if (Request.Cookies["_loggeduser"] != null)
                        {
                            Session["LoggedUser"] = Request.Cookies["_loggeduser"].Value;
                            username = Session["LoggedUser"].ToString();
                        }
                    }
                    string userrow = _userTable.GetRow(Session["_id"].ToString());
                    JObject userJO = JsonConvert.DeserializeObject<JObject>(userrow);
                    string to = "[\"" + userJO["email"].ToString() + "\"]";
                    string attach = "[]";
                    string bodymsj = "";
                    if (count == 0)
                    {
                        bodymsj = result + "Objetos Reales";
                    }
                    else
                    {
                        int ac = Convert.ToInt32(result);

                        bodymsj = "Se ha Autorizado su solicitud con Folio:" + folio + ",Cantidad de Objetos Solicitada:" + cant + ",Cantidad de Objetos Creados: " + ac;
                    }
                    string Idm = message.SendMail(to, "Advertencia Limite de Objetos Reales", bodymsj, attach, iduserthis, username);
                    string jsondata = "{\"idmail\":\"" + Idm + "\",\"user\":\"admin\",\"to\":\"" + Session["_id"].ToString() + "\",\"subject\":\"Advertencia\",\"body\":\"" + message1 + "\",\"image\":\"/Content/Images/not.gif\"}";

                    messenger.pushMessage(jsondata, "notificationobj");
                }
                return result;
            }
            catch (Exception ex)
            {
                return "error";
            }

        }
        public ActionResult Index(string folio=null,string idmail=null)
        {
            ////TODO: advanced search using Agregation.

            if (idmail != null)
            {
                try
                {
                    JArray id = new JArray();
                    id.Add(idmail);
                    try
                    {
                        
                           string iduserthis = Session["_id"].ToString();
                        
                    }
                    catch (Exception ex)
                    {
                        if (Request.Cookies["_id2"] != null)
                        {
                            Session["_id"] = Request.Cookies["_id2"].Value;
                         
                        }
                    }
                    string result = messagesC.readAlls(JsonConvert.SerializeObject(id), "true",Session["_id"].ToString());
                }
                catch (Exception ex)
                {

                }
            }
            JObject demandjo = new JObject();

            if (folio != null)
            {
                try
                {
                    string demandx = _demandTable.GetDemandTableByFolio(folio);
                    demandjo = JsonConvert.DeserializeObject<JArray>(demandx).First() as JObject;
                   
                }
                catch (Exception ex)
                {

                }
            }
            ViewData["demandact"] = demandjo;
            if (Request.Cookies["_id2"] != null)
            {
                Session["_id"] = Request.Cookies["_id2"].Value;
            }
            try
            {
                string permission = Session["Permissions"].ToString();
            }
            catch (Exception ex)
            {
                if (Request.Cookies["permissions"] != null)
                {
                    Session["Permissions"] = Request.Cookies["permissions"].Value;
                }
            }
            try
            {
                string permissionclient = Session["PermissionsClient"].ToString();
            }
            catch (Exception ex)
            {
                if (Request.Cookies["permissionsclient"] != null)
                {
                    Session["PermissionsClient"] = Request.Cookies["permissionsclient"].Value;
                }

            }

            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("demand", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("demand", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {

                //var profiles = _profileTable.getRows();
                //return View(profiles);

                String rowArray = _demandTable.GetRows();
                JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);
                loadMovements();
             //   loadLocationsConjunto();
                loadDepartments();
                if (getProfileUser(Session["_id"].ToString()) == "Administrador de sistema" || getHomeLocation(Session["_id"].ToString()) == "si")
                    loadLocationsConjuntoAlls();
                else
                {
                    loadLocationsConjuntoUser(Session["_id"].ToString());
                }
                ViewData["id"] = Session["_id"].ToString();
                getCategories();
                return View(objects);
            }
            else
            {

                return Redirect("~/Home");
            }
        }

        public bool getpermissions(string permission, string type)
        {
            if (Request.Cookies["permissions"] != null)
            {
                Session["Permissions"] = Request.Cookies["permissions"].Value;

            }
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

        public void loadMovements()
        {

            try
            {
                String movementOptions = "";
                String rowArray = _profileTable.GetRows();
                JArray moveList = JsonConvert.DeserializeObject<JArray>(rowArray);

                movementOptions += "<option value='null' selected> Seleccione Movimiento</option>";

                foreach (JObject document in moveList) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["name"].ToString() != "")
                    {
                        movementOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                        movementOptions += " data-type='" + document["typeMovement"] + "'";
                        movementOptions += " data-temp='" + document["temporal"] + "'";
                        movementOptions += " data-auto='" + document["autotransfer"] + "'";
                        movementOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                    }

                }


                ViewData["movementList"] = new HtmlString(movementOptions);
            }
            catch (Exception e)
            {
                ViewData["movementList"] = null;
            }
        }

        public void loadLocationsConjunto()
        {

            try
            {
                String locationsOptions = "";
                string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
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

                locationsOptions += "<option value='null' selected> Seleccione Conjunto</option>";

                foreach (JObject document in locatList) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["name"].ToString() != "")
                    {
                        locationsOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                        locationsOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                    }

                }

                ViewData["conjuntoList"] = new HtmlString(locationsOptions);
            }
            catch (Exception e)
            {
                ViewData["conjuntoList"] = null;
            }
        }

        public void loadDepartments() {
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
                    }

                }

                ViewData["departList"] = new HtmlString(DepartmentsOptions);
            }
            catch (Exception e)
            {
                ViewData["departList"] = null;
            }
        }

        public string loadObjectReferenceInfo()
        {
            try
            {
                String NameOptions = "", ModelOptions = "", MarcaOptions = "";
                JArray rowString = JsonConvert.DeserializeObject<JArray>(_objectReferenceTable.GetRows());
                JArray listas = new JArray();

                NameOptions += "<option value='null' selected> Seleccione Descripción del activo</option>";
                ModelOptions += "<option value='null' selected> Seleccione Modelo</option>";
                MarcaOptions += "<option value='null' selected> Seleccione Marca</option>";

                foreach (JObject obj in rowString)
                {
                    NameOptions += "<option value='" + obj["_id"].ToString() + "'"; //setting the id as the value
                    NameOptions += ">" + obj["name"].ToString() + "</option>"; //setting the text as the name

                    ModelOptions += "<option value='" + obj["_id"].ToString() + "'"; //setting the id as the value
                    ModelOptions += ">" + obj["modelo"].ToString() + "</option>"; //setting the text as the name

                    MarcaOptions += "<option value='" + obj["_id"].ToString() + "'"; //setting the id as the value
                    MarcaOptions += ">" + obj["marca"].ToString() + "</option>"; //setting the text as the name
                }
                return NameOptions + "|" + ModelOptions + "|" + MarcaOptions; //"{\"name\":\"" + NameOptions + "\", \"modelo\":\"" + ModelOptions + "\", \"marca\":\"" + MarcaOptions + "\"}";
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public string loadLocations(string conjunto,string type="0")
        {

            try
            {
                String locationsOptions = "";
                String rowArray="";
                JArray locatList = new JArray();
                if (conjunto == "")
                {
                    rowArray = _locationTable.GetRows();
                }
                else {
                    if (type == "0")
                    {
                        rowArray = _locationTable.Get("parent", conjunto);
                    }
                    else
                    {
                        try
                        {
                            rowArray = _locationTable.GetRow(conjunto);
                            JObject result = JsonConvert.DeserializeObject<JObject>(rowArray);
                            rowArray = _locationTable.Get("parent", result["parent"].ToString());
                        }
                        catch
                        {

                        }
                    }
                }
               
                locatList = JsonConvert.DeserializeObject<JArray>(rowArray);

                locationsOptions += "<option value='null' selected> Seleccione Ubicación</option>";

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

        public String loadObjects(string location_id, string typemove, string demandid = null)
        {

            try
            {
                String objectOptions = "";
                String rowArray = _objectTable.Get("location", location_id);
                JArray moveList = JsonConvert.DeserializeObject<JArray>(rowArray);
                string nameobj = "";

                String rowArray1 = "";
                JObject moveList1 = new JObject();
                string objid = "", namelocation = "", idubi = "";
                string[] cad = { };
                string checkedval = "";
                if (demandid != null && demandid != "")
                {
                    rowArray1 = _demandTable.GetRow(demandid);
                    moveList1 = JsonConvert.DeserializeObject<JObject>(rowArray1);
                    cad = moveList1["object"].ToString().Split(',');
                }

                if (typemove == "movement")
                {
                    objectOptions += "<tr><th></th><th>Objeto</th><th> EPC</th><th colspan='2' >Nueva Ubicación</th></tr>";
                }
                else
                {
                    objectOptions += "<tr><th></th><th>Objeto</th><th> EPC</th></tr>";
                }

                foreach (JObject document in moveList) //for each profile we create an option element with id as value and the name as the text
                {
                    try
                    {
                        nameobj = document["name"].ToString();
                    }
                    catch (Exception ex)
                    {
                        nameobj = "";
                    }


                    if (typemove == "movement")
                    {
                        objid = "";
                        namelocation = "";
                        checkedval = "";
                        idubi = "null";
                        foreach (string elem in cad)
                        {
                            string[] cad2 = elem.Split('|');
                            objid = cad2[0];

                            if (objid == document["_id"].ToString())
                            {
                                idubi = cad2[1];
                                String locationArray = _locationTable.GetRow(idubi);
                                JObject objlocation = JsonConvert.DeserializeObject<JObject>(locationArray);
                                namelocation = objlocation["name"].ToString();
                                checkedval = "checked";
                            }
                        }


                        objectOptions += "<tr id='" + document["_id"] + "'><td><div><span><center><input type='checkbox' class='uniform checker' name='checker" + document["_id"] + "' " + checkedval + "></center></span></div></td><td>" + nameobj + "</td><td id='td" + document["_id"] + "' data-location='" + document["location"].ToString() + "'>" + document["EPC"].ToString() + "</td>";
                        objectOptions += "<td><label id='label" + document["_id"] + "' value='' data-locationid='" + idubi + "'>" + namelocation + "</label></td><td><div class='btn blue fileinput-button' ><i class='icon-plus icon-white' onclick=newLocation('" + document["_id"] + "')></i></div></td></tr>"; //setting the id as the value
                    }
                    else
                    {

                        objid = "";
                        checkedval = "";
                        foreach (string elem in cad)
                        {
                            if (elem == document["_id"].ToString())
                            {
                                checkedval = "checked";
                            }
                        }

                        objectOptions += "<tr id='" + document["_id"] + "'><td><div><span><center><input type='checkbox' class='uniform checker' name='checker" + document["_id"] + "' " + checkedval + "></center></span></div></td><td>" + nameobj + "</td><td>" + document["EPC"].ToString() + "</td>";
                        objectOptions += "</tr>"; //setting the id as the value
                    }

                }


                return objectOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String loadObjects2(string demand_id, string typemove)
        {

            try
            {
                String objectOptions = "";
                String rowArray = _demandTable.GetRow(demand_id);
                JObject moveList = JsonConvert.DeserializeObject<JObject>(rowArray);
                string nameobj = "";
                string objid = "", namelocation = "", oldlocation = "";
                string[] cad = moveList["object"].ToString().Split(',');


                if (typemove == "movement")
                {
                    objectOptions += "<tr><th></th><th>Objeto</th><th> Ubicación anterior</th><th>Nueva Ubicación</th></tr>";
                }
                else
                {
                    objectOptions += "<tr><th></th><th>Objeto</th><th> EPC</th></tr>";
                }

                foreach (string document in cad) //for each profile we create an option element with id as value and the name as the text
                {
                    String objArray = "";
                    if (typemove == "movement")
                    {
                        string[] cad2 = document.Split('|');
                        objid = cad2[0];
                        namelocation = cad2[1];
                        oldlocation = cad2[2];

                        objArray = _objectTable.GetRow(objid);
                    }
                    else
                    {
                        objid = document;

                        objArray = _objectTable.GetDeleted("_id", objid);
                    }

                    JObject obj = JsonConvert.DeserializeObject<JObject>(objArray);

                    String locationArray = "";
                    JObject objlocation = new JObject();
                    JObject objlocation2 = new JObject();

                    if (namelocation != "")
                    {
                        locationArray = _locationTable.GetRow(namelocation);
                        objlocation = JsonConvert.DeserializeObject<JObject>(locationArray);

                        locationArray = _locationTable.GetRow(oldlocation);
                        objlocation2 = JsonConvert.DeserializeObject<JObject>(locationArray);
                    }

                    if (typemove == "delete")
                    {
                        locationArray = _locationTable.GetRow(obj["location"].ToString());
                        objlocation2 = JsonConvert.DeserializeObject<JObject>(locationArray);
                    }
                    try
                    {
                        nameobj = obj["name"].ToString();
                    }
                    catch (Exception ex)
                    {
                        nameobj = "";
                    }


                    if (typemove == "movement")
                    {
                        objectOptions += "<tr id='" + obj["_id"] + "'><td></td><td>" + nameobj + "</td><td>" + objlocation2["name"].ToString() + "</td>";
                        objectOptions += "<td><label id='label" + obj["_id"] + "' value='' data-locationid='null'>" + objlocation["name"] + "</label></td></tr>"; //setting the id as the value
                    }
                    else
                    {
                        objectOptions += "<tr id='" + obj["_id"] + "'><td></td><td>" + nameobj + "</td><td>" + obj["EPC"].ToString() + "</td>";
                        objectOptions += "</tr>"; //setting the id as the value
                    }

                }


                return objectOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String getFormView(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    BsonDocument document = _profileTable.getRow(profile);
                    String formString = document.GetElement("movementFields").Value.ToString();
                    String response = CustomForm.getFormView(formString, "MovementFields"); //we use the CustomForm class to generate the form's fiew
                    return response;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public String getFormView2(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    BsonDocument document = _profileTable.getRow(profile);
                    String formString = document.GetElement("movementFields").Value.ToString();
                    String response = CustomForm.getFormView(formString, "MovementFields"); //we use the CustomForm class to generate the form's fiew
                    return response.Replace("HTKField", "HTKFieldDetalles");
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
                    String formString = document.GetElement("movementFields").Value.ToString();
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

        /// <summary>
        /// New method to save demands
        /// </summary>
        /// <param name="demand"></param>
        public string newSaveDemand(string demand)
        {
            JObject objectDemand = null;
            try
            {
                objectDemand = JsonConvert.DeserializeObject<JObject>(demand);
            }
            catch (Exception e)
            {
                Error.Log(e, "Trying to deserialize demand");
                return null;
            }

            return "";
        }
        public void bindingSessions()
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

        private string Getcontador()
        {
            try
            {
                //Get contador information - Abiss
                JArray contadorArry = JsonConvert.DeserializeObject<JArray>(_userProfileTable.Get("name", "Contabilidad"));
                string contadorID = (from mov in contadorArry select (string)mov["_id"]).First().ToString();

                JArray users = JsonConvert.DeserializeObject<JArray>(_userTable.Get("profileId", contadorID));
                JObject userInfo = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(users[0]));

                JObject info = new JObject();
                info.Add("id_auto", "null");
                info.Add("id_user", userInfo["_id"].ToString());
                info.Add("user", userInfo["user"].ToString());
                info.Add("name", userInfo["name"].ToString());
                info.Add("lastname", userInfo["lastname"].ToString());
                info.Add("approved", "0");
                info.Add("date", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                return "[" + JsonConvert.SerializeObject(info) + "]";
            }
            catch
            {
                return "";
            }
        }

        private string GetWitness()
        {
            try
            {
                //Get witness information - Abiss
                JArray witnessArry = JsonConvert.DeserializeObject<JArray>(_witnessTable.GetRows());

                JArray result = new JArray();
                foreach (JObject witness in witnessArry)
                {
                    JObject userInfo;
                    try
                    {
                        userInfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(witness["user"].ToString()));
                    }
                    catch { continue; }

                    JObject info = new JObject();
                    info.Add("id_auto", "null");
                    info.Add("id_user", userInfo["_id"].ToString());
                    info.Add("witnessType", witness["witnessType"].ToString());
                    info.Add("user", userInfo["user"].ToString());
                    info.Add("name", userInfo["name"].ToString());
                    info.Add("lastname", userInfo["lastname"].ToString());
                    info.Add("approved", "0");
                    info.Add("date", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                    result.Add(info);
                }
                return JsonConvert.SerializeObject(result);
            }
            catch {
                return "";
            }
        }

        private string GetLogistica()
        {
            //Get contador information - Abiss
            try
            {
                JArray contadorArry = JsonConvert.DeserializeObject<JArray>(_userProfileTable.Get("name", "Logística"));
                string contadorID = (from mov in contadorArry select (string)mov["_id"]).First().ToString();

                JArray users = JsonConvert.DeserializeObject<JArray>(_userTable.Get("profileId", contadorID));
                JObject userInfo = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(users[0]));

                JObject info = new JObject();
                info.Add("id_auto", "null");
                info.Add("id_user", userInfo["_id"].ToString());

                return userInfo["_id"].ToString();
            }
            catch
            {
                return "";
            }
        }

        public String GetCorreoUser(String userid) {
            String result = "";
           String objarray = _userTable.GetRow(userid);
           JObject userobj = JsonConvert.DeserializeObject<JObject>(objarray);
           if (userobj["email"] != null) {
               result = userobj["email"].ToString();
           }
           return result;
        }

        public String saveDemand(String demand)
        {
            String dataPermissions = "";
            String dataPermissionsClient = "";
            try
            {
                 dataPermissions = Session["Permissions"].ToString();
                 dataPermissionsClient = Session["PermissionsClient"].ToString();
            }
            catch (Exception ex)
            {
                 bindingSessions();
                dataPermissions = Session["Permissions"].ToString();
                dataPermissionsClient = Session["PermissionsClient"].ToString();
            }
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("demand", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("demand", "u", dataPermissionsClient);
            JObject datos = new JObject();
            JObject newdemand = new JObject();
            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest())
                {
                    datos = JsonConvert.DeserializeObject<JObject>(demand);
                    String demandID = (datos["demandID"].ToString() == "null" || datos["demandID"].ToString() == "") ? null : datos["demandID"].ToString(); //is this an insert or an update?, converting null in javascript to null in c#
                    //idobject = formData["object"];
                    // idlocation = formData["location"];
                    string creatorDemand = "";
                    String demandobj = _demandTable.GetRow(demandID);
                    /*the gived id does not exists*/
                    if (demandID != null && (demandobj == null))
                    {
                        return "El id especificado no existe";
                    }


                    /*there is no profile with the gived id*/
                    if (_profileTable.GetRow(datos["movement"].ToString()) == null)
                    {
                        return "El movimiento especificado no existe";
                    }

                    //string canttotal = "0";
                    //canttotal = datos["total"].ToString();

                    //int cants = 0;
                    //int demandcant = 0;

                    //int.TryParse(canttotal, out cants);
                    //int.TryParse(formData["total"], out demandcant);
                    //if (demandcant != 0)
                    //{
                    //    int canttotalx = demandcant - cants;
                    //    validateLimit(canttotalx);
                    //}

                    // end validation

                    int estatus = 3;
                    JArray listlocats = new JArray();

                    string typemove = "";
                    String movearray = _profileTable.GetRow(datos["movement"].ToString());
                    JObject moveobj = JsonConvert.DeserializeObject<JObject>(movearray);
                    String autoTransfer = "false";
                    try {
                        autoTransfer = moveobj["autotransfer"].ToString();
                    }
                    catch (Exception ex) { autoTransfer = "false"; }

                    typemove = moveobj["typeMovement"].ToString();
                    List<string> reflist = new List<string>();
                    foreach (JObject obj3 in datos["objects"])
                    {
                        JToken t;
                        JObject elem = new JObject();
                        elem.Add("location", obj3["location"].ToString());
                        if (typemove == "movement" && (datos["temp"].ToString() == "false" || datos["temp"].ToString() == "False"))
                        {
                            elem.Add("conjuntoDestiny", obj3["conjuntoDestiny"].ToString()); 
                            elem.Add("locationDestiny", obj3["locationDestiny"].ToString()); 
                        }
                        //if (typemove == "delete") {
                        //    elem.Add("assetType",obj3["assetType"].ToString());
                        //} 
                        if (obj3.TryGetValue("objectReference", out t))
                        {
                            reflist.Add(obj3["objectReference"].ToString());
                        }
                        listlocats.Add(elem);
                    }
                    string authos, notifs, vistobueno;

                    if (autoTransfer == "True") {
                        authos = notifs = vistobueno = "[]";
                    }
                    else {
                        authos = Getauthorizations(datos["movement"].ToString(), listlocats, Session["_id"].ToString());
                        notifs = Getnotifications(datos["movement"].ToString(), listlocats, Session["_id"].ToString());
                        vistobueno = Getapproval(datos["movement"].ToString(), listlocats, Session["_id"].ToString());
                    }
                     
                    string contador = "";
                    string witness = "";
                    if (authos == "[]" && vistobueno == "[]") { estatus = 6; newdemand["AuthorizedDate"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); }
                    string dictaminador = "";
                    if (moveobj["typeMovement"].ToString() == "delete")
                    {
                        if (datos["deleteType"].ToString() != "no_planeada")
                        {
                            estatus = 1;
                            dictaminador = getDictaminador(listlocats, datos["assetType"].ToString());
                            JObject dict = new JObject();
                            dict = JsonConvert.DeserializeObject<JObject>(dictaminador);

                            newdemand["adjudicating"] = dict["userid"].ToString();
                            if (datos["destinyOptions"].ToString() == "venta") newdemand["invoiceRequired"] = datos["invoiceRequired"];
                        }

                        //Get testigos info
                        if (datos["destinyOptions"].ToString() == "destruccion") { witness = GetWitness(); }

                        //Add extra authorizations
                        authos = addAuthorizations(datos["destinyOptions"].ToString(), authos, listlocats);

                        newdemand["destinyOptions"] = datos["destinyOptions"];
                        newdemand["deleteType"] = datos["deleteType"];
                    }
                    contador = Getcontador();
                    string assetType = "";
                    //try
                    //{
                    //    JArray refobject9 = JsonConvert.DeserializeObject<JArray>(_objectTable.getMMO(reflist,0));
                    //    JToken t;
                    //    foreach (JObject row in refobject9)
                    //    {
                    //        foreach (JObject objs in datos["objects"])
                    //        {
                    //            try
                    //            {
                    //                if (objs["objectReference"].ToString() == row["_id"].ToString())
                    //                {
                    //                    if (row.TryGetValue("assetType", out t))
                    //                    {
                    //                        if (objs.TryGetValue("assetType", out t))
                    //                        {
                    //                            objs["assetType"] = row["assetType"].ToString();
                    //                        }
                    //                        else
                    //                        {
                    //                            objs.Add("assetType", row["assetType"].ToString());
                    //                        }

                    //                    }

                    //                }
                    //            }
                    //            catch { }
                    //        }
                    //    }
                    //}
                    //catch { }

                    //Verfi all validators need in the demand exists
                    List<string> missingResons = new List<string>();
                    try
                    {
                        if (authos == "" || authos == "[]") { missingResons.Add("authorizations"); }
                    }
                    catch { }
                    try
                    {
                        if (vistobueno == "" || vistobueno == "[]" && autoTransfer != "True") { missingResons.Add("approval"); }
                    }
                    catch { }
                    try
                    {
                        if ((contador == "" || contador == "[]") && moveobj["typeMovement"].ToString() == "delete") { missingResons.Add("contador"); }
                    }
                    catch { }
                    try
                    {
                        if ((witness == "" || witness == "[]") && datos["destinyOptions"].ToString() == "destruccion") { missingResons.Add("witness"); }
                    }
                    catch { }
                    try
                    {
                        if (moveobj["typeMovement"].ToString() == "delete") {
                            if ((newdemand["adjudicating"].ToString() == "" || newdemand["adjudicating"].ToString() == "[]") && datos["deleteType"].ToString() == "planeada") //datos["destinyOptions"].ToString() == "robo"
                            {
                                missingResons.Add("dictaminador");
                            }

                            JArray autorizationsArray = JsonConvert.DeserializeObject<JArray>(authos);
                           // if (autorizationsArray.Count < 2)
                            if (autorizationsArray.Count ==0)
                            {
                                try
                                {
                                    foreach (JObject autori in autorizationsArray)
                                    {
                                        JObject userInfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(autori["id_user"].ToString()));
                                        JObject userProfInfo = JsonConvert.DeserializeObject<JObject>(_userProfileTable.GetRow(userInfo["profileId"].ToString()));

                                        if (datos["destinyOptions"].ToString() == "venta" && userProfInfo["name"].ToString() == "Gerente regional")
                                        {
                                            missingResons.Add("corporativo");
                                        }
                                        else if (datos["destinyOptions"].ToString() != "venta" && userProfInfo["name"].ToString() == "Gerente regional")
                                        {
                                            missingResons.Add("inmuebles");
                                        }
                                        else
                                        {
                                            missingResons.Add("authorizations");
                                        }
                                    }
                                }
                                catch
                                {
                                    missingResons.Add("authorizations");
                                }
                            }
                           

                            if (autorizationsArray.Count == 0)
                            {
                                if (datos["destinyOptions"].ToString() == "venta")
                                {
                                    missingResons.Add("corporativo");
                                }
                                else
                                {
                                    missingResons.Add("inmuebles");
                                }
                            }
                        }
                    }
                    catch { }

                    if (missingResons.Count > 0 && autoTransfer != "True")
                    {
                        JObject missResons = new JObject();
                        missResons.Add("error", "true");
                        missResons.Add("info", JsonConvert.SerializeObject(missingResons));
                        return JsonConvert.SerializeObject(missResons);
                    }

                    newdemand["folio"] = generateFolio(demandID);
                    newdemand["objects"] = datos["objects"];
                    newdemand["authorizations"] = JsonConvert.DeserializeObject<JArray>(authos);
                    newdemand["notifications"] = JsonConvert.DeserializeObject<JArray>(notifs);
                    newdemand["approval"] = JsonConvert.DeserializeObject<JArray>(vistobueno);
                    if (contador != "") newdemand["contador"] = JsonConvert.DeserializeObject<JArray>(contador);
                    if (witness != "") { newdemand["witness"] = JsonConvert.DeserializeObject<JArray>(witness); newdemand["setRecepit"] = "false"; }
                    newdemand["status"] = estatus;
                    newdemand["movement"] = datos["movement"];
                    newdemand["movementFields"] = new JObject();
                    newdemand["extras"] = datos["extras"];
                    newdemand["profileFields"] = datos["profileFields"];
                    //jsonData += ", 'movementFields':{";

                    ////foreach element in the formData, let's append it to the jsonData in the profileFields
                    //int cont = 0;
                    //foreach (String key in formData.Keys)
                    //{
                    //    jsonData += "'" + key + "':'" + formData[key] + "'";

                    //    cont++;
                    //    if (cont < formData.Keys.Count)
                    //    {
                    //        jsonData += ", ";
                    //    }
                    //}
                    //jsonData += "}}";

                    bool ok = false;

                    //now that we have the json and we know the data is ok, let's save it
                    string id = _demandTable.SaveRow(JsonConvert.SerializeObject(newdemand), demandID);
                    _logTable.SaveLog(Session["_id"].ToString(),"Movimientos","Insert: Folio #"+newdemand["folio"].ToString(),"Demand",DateTime.Now.ToString());

                    Dictionary<string, string> namerefdict = new Dictionary<string, string>();
                    Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

                    try
                    {
                        List<string> listconjunt = (from refe in newdemand["objects"] select (string)refe["location"]).ToList();
                        String resultref = _objectTable.getParents(listconjunt);
                        JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                        namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

                    }
                    catch { }

                    try
                    {
                        List<string> listactnames = (from refe in newdemand["objects"] select (string)refe["objectReference"]).ToList();
                        String resultref = _objectTable.getCategory(listactnames);
                        JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                        activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

                    }
                    catch { }
                    string usstring = _userTable.GetRow(Session["_id"].ToString());
                    JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

                    string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

                    String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
                    StringBuilder act = new StringBuilder();
                    string modelo = "";
                    string marca = "";
                    string idart = "";
                    string serie = "";
                    string nombre = "";
                    string cantq = "1";
                    string category = "";
                    string cantq2 = "";
                    string conjd = "";
                    string epc = "";
                    string locn="";
                    string infotrans = "";
                    string ubicacion = "";
                    foreach (JObject nr in newdemand["objects"])
                    {
                        JObject n;
                        JToken nt2;
                        serie = nr["serie"].ToString();
                        cantq = nr["quantity"].ToString();
                        try{
                            if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                                epc = nr["epc"].ToString();
                            else
                                epc = "No aplica";

                        }catch(Exception ex){
                            epc = "No aplica";
                        }
                        if (typemove == "movement" && (datos["temp"].ToString() == "false" || datos["temp"].ToString() == "False"))
                        {
                            if (nr.TryGetValue("quantity_new", out nt2))
                                cantq2 = nr["quantity_new"].ToString();
                            if (nr.TryGetValue("conjuntoDestiny", out nt2))
                            {
                                conjd = getNameLocation(nr["conjuntoDestiny"].ToString());
                            }
                            if (autoTransfer == "True") {
                                if (nr.TryGetValue("locationDestiny", out nt2))
                                {
                                    locn = getNameLocation(nr["locationDestiny"].ToString());
                                }
                            }
                            infotrans = "<h5>Cantidad a mover: " + cantq2 + "</h5><h5>Conjunto destino: " + conjd + "</h5><h5>Ubicación destino: " + locn + "</h5>";
                        }
                        if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                        {
                            if (n.TryGetValue("modelo", out nt2))
                                modelo = n["modelo"].ToString();
                            if (n.TryGetValue("marca", out nt2))
                                marca = n["marca"].ToString();
                            if (n.TryGetValue("object_id", out nt2))
                                idart = n["object_id"].ToString();
                            if (n.TryGetValue("name", out nt2))
                                nombre = n["name"].ToString();
                            if (n.TryGetValue("namecategory", out nt2))
                                category = n["namecategory"].ToString();
                            if (nr.TryGetValue("location", out nt2))
                            {
                                ubicacion = getNameLocation(nr["location"].ToString());
                            }
                            else { ubicacion = ""; }
                            act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5>" + infotrans + "</div><br>");
                        }
                    }
                    string bodygeneric = "<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;

                    string namemov = "";
                    try
                    {
                        namemov = getNameMovement(newdemand["movement"].ToString());
                    }
                    catch (Exception ex) { }
                    classNotifications.saveNotification("Movements", "Create", "Se ha Generado una nueva Solicitud de "+namemov+", #folio: " + newdemand["folio"]);
                    try
                    {
                        String demandobj1 = _demandTable.GetRow(id);
                        JObject demandobj1ja = JsonConvert.DeserializeObject<JObject>(demandobj1);
                        creatorDemand = demandobj1ja["Creator"].ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                    JArray recipients = new JArray();
                    JArray attachments = new JArray();
                    
                    if (estatus == 1)
                    {
                        recipients.Add(newdemand["adjudicating"].ToString());
                        string to = JsonConvert.SerializeObject(recipients);
                        string attach = JsonConvert.SerializeObject(attachments);
                        messagesC.SendMail(to, "Movimiento de " + namemov + " con Folio #" + newdemand["folio"].ToString() + ",esta Pendiente de Dictaminar", "La Solicitud de " + namemov + " con Folio #" + newdemand["folio"].ToString() + ",esta Pendiente de Dictaminar." + bodygeneric, attach, "Sistema");

                        //List<String> listemails = new List<String>();
                        //foreach (string recip in recipients)
                        //{
                        //    String em = GetCorreoUser(recip);
                        //    listemails.Add(em);
                        //}

                        //messagesC.SendExternalMail("", "Sistema", listemails, "La Solicitud de " + namemov + " con Folio #" + newdemand["folio"].ToString() + ",esta Pendiente de Dictaminar", attachments, "Movimiento de " + namemov + " con Folio #" + newdemand["folio"].ToString() + ",esta Pendiente de Dictaminar");
                    }
                    else if (estatus == 3)
                    {

                        foreach (var autor in newdemand["authorizations"])
                        {
                            try
                            {
                                recipients.Add(autor["id_user"].ToString());
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }

                        }
                        string to = JsonConvert.SerializeObject(recipients);
                        string attach = JsonConvert.SerializeObject(attachments);
                        //*****************************************************************************************

                        messagesC.SendMail(to, "Nuevo movimiento de " + namemov + " generado, Folio: " + newdemand["folio"].ToString(), "Ha sido generado un nuevo movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + newdemand["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su autorización." + creadorstring+bodygeneric, attach, "Sistema");
                        //List<String> listemails = new List<String>();
                        //foreach (string recip in recipients)
                        //{
                        //    String em = GetCorreoUser(recip);
                        //    listemails.Add(em);
                        //}

                        //messagesC.SendExternalMail("", "Sistema", listemails, "Ha sido generado un nuevo movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + newdemand["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su autorización." + bodygeneric, attachments, "Nuevo movimiento de " + namemov + " generado, Folio: " + newdemand["folio"].ToString());

                    }
                    if (estatus == 6)
                    {
                        if (typemove == "create")
                        {
                            string recepcion = "";
                            string pedido = "";
                            string solicitud = "";
                            string observaciones = "";
                            try {
                                recepcion = JsonConvert.DeserializeObject<JObject>(newdemand["profileFields"].ToString())["_HTKFieldno_recepcion"].ToString();
                            }
                            catch {
                                recepcion = "";
                            }
                            try
                            {
                                observaciones = JsonConvert.DeserializeObject<JObject>(newdemand["profileFields"].ToString())["_HTKFieldDescripcion"].ToString();
                            }
                            catch
                            {
                                observaciones = "";
                            }
                            try
                            {
                                pedido = JsonConvert.DeserializeObject<JObject>(newdemand["profileFields"].ToString())["_HTKFieldno_pedido"].ToString();
                            }
                            catch {
                                pedido = "";
                            }
                            try
                            {
                                solicitud = JsonConvert.DeserializeObject<JObject>(newdemand["profileFields"].ToString())["_HTKFieldno_solicitud"].ToString(); 
                            }
                            catch {
                                solicitud = "";
                            }
                            

                            foreach (JObject obj in newdemand["objects"])
                            {
                                String objarray = _objectReferenceTable.GetRow(obj["id"].ToString());
                                JObject objref = JsonConvert.DeserializeObject<JObject>(objarray);
                                int cant;
                                int.TryParse(obj["quantity"].ToString(), out cant);
                                saveObjects(creatorDemand,obj["id"].ToString(), objref["name"].ToString(), obj["location"].ToString(), moveobj["processes"].ToString(), obj["marca"].ToString(), obj["modelo"].ToString(), obj["object_id"].ToString(), obj["perfil"].ToString(), obj["serie"].ToString(), obj["price"].ToString(), obj["label"].ToString(), obj["date"].ToString(), obj["quantity"].ToString(), newdemand["folio"].ToString(), "", obj["department"].ToString(), recepcion, pedido, solicitud,observaciones);
                                ok = RulesChecker.isValidToLocation(obj["id"].ToString(), obj["location"].ToString());
                                if (ok == false)
                                {
                                    classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicacion no valida en Solicitud de "+namemov+", #folio: " + newdemand["folio"]);
                                    // return "problem";
                                }

                            }

                        }

                        if (typemove == "movement")
                        {
                            string var1 = "1";
                            bool ok2 = false;
                            string pro1, pro2;
                            foreach (JObject obj in newdemand["objects"])
                            {
                                ok = RulesChecker.isValidToLocation(obj["id"].ToString(), obj["locationDestiny"].ToString());
                                if (ok == false)
                                {
                                    var1 = "0";
                                }
                                string larray = _locationTable.GetRow(obj["location"].ToString());
                                JObject lobj = JsonConvert.DeserializeObject<JObject>(larray);
                                try { pro1 = lobj["processId"].ToString(); }
                                catch { pro1 = ""; }

                                larray = _locationTable.GetRow(obj["locationDestiny"].ToString());
                                lobj = JsonConvert.DeserializeObject<JObject>(larray);
                                try { pro2 = lobj["processId"].ToString(); }
                                catch { pro2 = ""; }

                                if (pro1 != "" && pro2 != "")
                                {
                                    ok2 = RulesChecker.isValidProcessFlow(pro1, pro2);

                                    if (ok2 == false)
                                        classNotifications.saveNotification("Rules", "Invalid", "Flujo de procesos es invalido en Solicitud de "+namemov+", #folio: " + newdemand["folio"]);

                                }
                            }

                            if (var1 == "0") ok = false;
                            if (ok == false)
                            {
                                classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicación no valida en Solicitud de "+namemov+", #folio: " + newdemand["folio"]);
                                // return "problem";
                            }
                            SaveMovement(JsonConvert.DeserializeObject<JArray>(newdemand["objects"].ToString()), moveobj["processes"].ToString());

                            recipients.Add(GetLogistica()); 
                        }
                        if (typemove == "delete")
                        {
                            DeleteObjects(JsonConvert.DeserializeObject<JArray>(newdemand["objects"].ToString()), moveobj["processes"].ToString());
                        }
                        if (newdemand["contador"].ToString() != "") {
                            foreach (var dd in newdemand["contador"])
                            {
                                try
                                {
                                    recipients.Add(dd["id_user"].ToString());
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }

                            }
                        }
                        recipients.Add(creatorDemand);
                        string to = JsonConvert.SerializeObject(recipients);
                        string attach = JsonConvert.SerializeObject(attachments);
                        messagesC.SendMail(to, "Movimiento de " + namemov + " completado", "Ha sido completado el movimiento de " + namemov + " con Folio #" + newdemand["folio"].ToString() + ". Completado por : " + getNameUser(Session["_id"].ToString()) + bodygeneric, attach, "Sistema");

                        //List<String> listemails = new List<String>();
                        //foreach (string recip in recipients)
                        //{
                        //    String em = GetCorreoUser(recip);
                        //    listemails.Add(em);
                        //}

                      //  messagesC.SendExternalMail("", "Sistema", listemails, "Ha sido completado el movimiento de " + namemov + " con Folio #" + newdemand["folio"].ToString() + ". Completado por : " + getNameUser(Session["_id"].ToString()), attachments, "Movimiento de " + namemov + " completado");

                    }

                    return "saved"; //returns the saved user's id

                }
                return null;
            }
            else { return null; }
        }

        public string addAuthorizations(String destinyOptions, String authos, JArray location)
        {
            JArray authosArray = JsonConvert.DeserializeObject<JArray>(authos);
            String objarray = "", result = "";
            JArray ele = new JArray();

            if (destinyOptions == "venta") objarray = _userProfileTable.Get("name", "Autorizador de corporativo");
            else objarray = _userProfileTable.Get("name", "Director de inmuebles");

            JArray dits = JsonConvert.DeserializeObject<JArray>(objarray);
            string idprofile = (from mov in dits select (string)mov["_id"]).First().ToString();

            JArray dict = JsonConvert.DeserializeObject<JArray>(_userTable.Get("profileId", idprofile));

            try
            {
                foreach (JObject cad1 in location)
                {
                    ele = getRoute3(cad1["location"].ToString());
                    foreach (JObject cad2 in dict)
                    {
                        foreach (String cad3 in ele)
                        {
                            foreach (JObject cad4 in cad2["userLocations"])
                            {
                                if (cad4["id"].ToString() == "null" && cad4["name"].ToString() == "Home")
                                {
                                    result = result + "{\"id_auto\":\"null\",\"id_user\":\"" + cad2["_id"].ToString() + "\"," + "\"user\":\"" + cad2["user"].ToString() + "\",\"name\":\"" + cad2["name"].ToString() + "\",\"lastname\":\"" + cad2["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                    goto Finish;
                                }
                                else if (cad3 == cad4["id"].ToString())
                                {
                                    result = result + "{\"id_auto\":\"null\",\"id_user\":\"" + cad2["_id"].ToString() + "\"," + "\"user\":\"" + cad2["user"].ToString() + "\",\"name\":\"" + cad2["name"].ToString() + "\",\"lastname\":\"" + cad2["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                    goto Finish;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }

            Finish:
            if (result != "") authosArray.Add(JsonConvert.DeserializeObject<JObject>(result));

            return JsonConvert.SerializeObject(authosArray);
        }

        public string Getauthorizations(String movement, JArray location, String userid = null)
        {
            string result = "[";
            string autoarray = "";
            JObject autoobj = new JObject();
            JArray users = new JArray();
            JArray users2 = new JArray();
            JObject userobj = new JObject();
            JArray ele = new JArray();
            string objarray = "";
            int cont = 0;
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();

            String movearray = _profileTable.GetRow(movement);
            JObject moveobj = JsonConvert.DeserializeObject<JObject>(movearray);

            JArray authoslist = JsonConvert.DeserializeObject<JArray>(moveobj["authorization"].ToString());

            String profilesarray = _userProfileTable.GetRows();
            JArray profiles = JsonConvert.DeserializeObject<JArray>(profilesarray);

            Dictionary<string, string> perfiles = new Dictionary<string, string>();
            Dictionary<string, string> perfiles2 = new Dictionary<string, string>();
            perfiles = profiles.ToDictionary(x => (string)x["_id"].ToString(), x => (string)x["name"].ToString());
            perfiles2 = profiles.ToDictionary(x => (string)x["name"].ToString(), x => (string)x["_id"].ToString());

            foreach (JObject cad1 in location)
            {
                ele = getRoute3(cad1["location"].ToString());
                foreach (String ob in ele)
                {
                    list1.Add(ob);
                }
            }

            bool temp = false;
            try
            {
                bool.TryParse(moveobj["temporal"].ToString(),out temp);
   
            }
            catch { temp = false; }

            if (moveobj["typeMovement"].ToString() == "movement"  && temp==false)
            {
                foreach (JObject cad1 in location)
                {
                    ele = getRoute3(cad1["locationDestiny"].ToString());
                    foreach (String ob in ele)
                    {
                        list3.Add(ob);
                    }
                }
            }

            objarray = _userTable.GetRow(userid);
            userobj = JsonConvert.DeserializeObject<JObject>(objarray);

            try
            {
                objarray = _userTable.GetRow(userobj["boss"].ToString());
                JObject bossobj = JsonConvert.DeserializeObject<JObject>(objarray);
                JObject positionBoss = JsonConvert.DeserializeObject<JObject>(_userProfileTable.GetRow(bossobj["profileId"].ToString()));

                if (!result.Contains(bossobj["_id"].ToString()))
                {
                    if (cont != 0) result = result + ",";
                    result = result + "{\"id_auto\":\"null\",\"id_user\":\"" + bossobj["_id"].ToString() + "\"," +
                                "\"user\":\"" + bossobj["user"].ToString() + "\",\"name\":\"" + bossobj["name"].ToString() + "\",\"lastname\":\"" + bossobj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                    cont++;
                }
            }
            catch { }

            foreach (string auto in authoslist)
            {
                try{
                autoarray = _autoTable.GetRow(auto);
                autoobj = JsonConvert.DeserializeObject<JObject>(autoarray);
                if (autoobj["type"].ToString() == "authorization")
                {
                    if (autoobj["position"].ToString() == perfiles2["Gerente destino"])
                    {
                        objarray = _userTable.Get("profileId", perfiles2["Gerente de conjunto"]);
                        users = JsonConvert.DeserializeObject<JArray>(objarray);

                        objarray = _userTable.Get("profileId", perfiles2["Gerente regional"]);
                        users2 = JsonConvert.DeserializeObject<JArray>(objarray);

                        foreach (JObject o2 in users2)
                        {
                            users.Add(o2);
                        }


                        foreach (JObject obj in users)
                        {
                            JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                            /* Patch */
                            list2.Clear();
                            foreach (JObject l in locts)
                                list2.Add(l["id"].ToString());
                            /* END: Patch*/
                            if (list2.Intersect<string>(list3).ToList<string>().Count > 0)
                            {

                                if (!result.Contains(obj["_id"].ToString()))
                                {
                                    if (cont != 0) result = result + ",";
                                    result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                    cont++;
                                }

                            }
                        }
                    }
                    else if (autoobj["position"].ToString() == perfiles2["Solicitante"])
                    {
                        objarray = _userTable.GetRow(userid);
                        userobj = JsonConvert.DeserializeObject<JObject>(objarray);


                        if (!result.Contains(userobj["_id"].ToString()))
                        {
                            if (cont != 0) result = result + ",";
                            result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + userobj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + userobj["user"].ToString() + "\",\"name\":\"" + userobj["name"].ToString() + "\",\"lastname\":\"" + userobj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                        }
                    }
                    else if (autoobj["position"].ToString() == perfiles2["Administrador de sistema"])
                    {
                        objarray = _userTable.Get("profileId", perfiles2["Administrador de sistema"]);
                        users = JsonConvert.DeserializeObject<JArray>(objarray);
                        foreach (JObject obj in users)
                        {
                            if (!result.Contains(obj["_id"].ToString()))
                            {
                            if (cont != 0) result = result + ",";
                                result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                    "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                cont++;
                                break;
                        }

                    }

                    }
                    else
                    {
                        objarray = _userTable.Get("profileId", autoobj["position"].ToString());
                        users = JsonConvert.DeserializeObject<JArray>(objarray);

                        foreach (JObject obj in users)
                        {
                            JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                            /* Patch */
                            list2.Clear();
                            foreach (JObject l in locts)
                                list2.Add(l["id"].ToString());
                            /* END: Patch*/

                            if (list1.Intersect<string>(list2).ToList<string>().Count > 0)
                            {

                                if (!result.Contains(obj["_id"].ToString()))
                                {
                                    if (cont != 0) result = result + ",";
                                    result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                    cont++;
                                }
                            }

                        }
                    }
                }
            }catch{ }

            }

            result = result + "]";
            return result;

        }

        public String GetGerenteConjunto(String location) {
            String gerente = "";
            JArray ruta = new JArray();
            String objarray;
            JArray users = new JArray();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            ruta = getRoute3(location);

            String profilesarray = _userProfileTable.GetRows();
            JArray profiles = JsonConvert.DeserializeObject<JArray>(profilesarray);

            Dictionary<string, string> perfiles = new Dictionary<string, string>();
            perfiles = profiles.ToDictionary(x => (string)x["name"].ToString(), x => (string)x["_id"].ToString());

            foreach (String ob in ruta)
                {
                    list1.Add(ob);
                }


            objarray = _userTable.Get("profileId", perfiles["Gerente de conjunto"]);
            users = JsonConvert.DeserializeObject<JArray>(objarray);

            foreach (JObject obj in users)
            {
                JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                foreach (JObject l in locts)
                    list2.Add(l["id"].ToString());
                if (list1.Intersect<string>(list2).ToList<string>().Count > 0)
                {
                    gerente = JsonConvert.SerializeObject(obj);
                    break;
                }
            }

            return gerente;
        }

        //notificacions
        public string Getnotifications(String movement, JArray location, String userid = null)
        {
            try
            {
            string result = "[";
            string autoarray = "";
            JObject autoobj = new JObject();
            JArray users = new JArray();
            JArray users2 = new JArray();
            JObject userobj = new JObject();
            JArray ele = new JArray();
            string objarray = "";
            int cont = 0;
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();

            String movearray = _profileTable.GetRow(movement);
            JObject moveobj = JsonConvert.DeserializeObject<JObject>(movearray);

            JArray authoslist = JsonConvert.DeserializeObject<JArray>(moveobj["authorization"].ToString());

            String profilesarray = _userProfileTable.GetRows();
            JArray profiles = JsonConvert.DeserializeObject<JArray>(profilesarray);

            Dictionary<string, string> perfiles = new Dictionary<string, string>();
            Dictionary<string, string> perfiles2 = new Dictionary<string, string>();
            perfiles = profiles.ToDictionary(x => (string)x["_id"].ToString(), x => (string)x["name"].ToString());
            perfiles2 = profiles.ToDictionary(x => (string)x["name"].ToString(), x => (string)x["_id"].ToString());

            foreach (JObject cad1 in location)
            {
                ele = getRoute3(cad1["location"].ToString());
                foreach (String ob in ele)
                {
                    list1.Add(ob);
                }
            }

            bool temp = false;
            try
            {
                bool.TryParse(moveobj["temporal"].ToString(), out temp);

            }
            catch { temp = false; }

            if (moveobj["typeMovement"].ToString() == "movement" && temp == false)
            {
                foreach (JObject cad1 in location)
                {
                    ele = getRoute3(cad1["locationDestiny"].ToString());
                    foreach (String ob in ele)
                    {
                        list3.Add(ob);
                    }
                }
            }

            foreach (string auto in authoslist)
            {
                autoarray = _autoTable.GetRow(auto);
                autoobj = JsonConvert.DeserializeObject<JObject>(autoarray);
                if (autoobj["type"].ToString() == "notification")
                {
                    if (autoobj["position"].ToString() == perfiles2["Gerente destino"])
                    {
                        objarray = _userTable.Get("profileId", perfiles2["Gerente de conjunto"]);
                        users = JsonConvert.DeserializeObject<JArray>(objarray);

                        objarray = _userTable.Get("profileId", perfiles2["Gerente regional"]);
                        users2 = JsonConvert.DeserializeObject<JArray>(objarray);

                        foreach (JObject o2 in users2)
                        {
                            users.Add(o2);
                        }


                        foreach (JObject obj in users)
                        {
                            JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                            /* Patch */
                            list2.Clear();
                            foreach (JObject l in locts)
                                list2.Add(l["id"].ToString());
                            /* END: Patch*/
                            if (list2.Intersect<string>(list3).ToList<string>().Count > 0)
                            {

                                if (!result.Contains(obj["_id"].ToString()))
                                {
                                    if (cont != 0) result = result + ",";
                                    result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                    cont++;
                                }

                            }
                        }
                    }
                    //else if (autoobj["position"].ToString() == "jefe_inmediato")
                    //{
                    //    objarray = _userTable.GetRow(userid);
                    //    userobj = JsonConvert.DeserializeObject<JObject>(objarray);

                    //    objarray = _userTable.GetRow(userobj["boss"].ToString());
                    //    JObject bossobj = JsonConvert.DeserializeObject<JObject>(objarray);

                    //    if (!result.Contains(bossobj["_id"].ToString()))
                    //    {
                    //        if (cont != 0) result = result + ",";
                    //        result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + bossobj["_id"].ToString() + "\"," +
                    //                    "\"user\":\"" + bossobj["user"].ToString() + "\",\"name\":\"" + bossobj["name"].ToString() + "\",\"lastname\":\"" + bossobj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                    //    }
                    //}
                    else if (autoobj["position"].ToString() == perfiles2["Solicitante"])
                    {
                        objarray = _userTable.GetRow(userid);
                        userobj = JsonConvert.DeserializeObject<JObject>(objarray);


                        if (!result.Contains(userobj["_id"].ToString()))
                        {
                            if (cont != 0) result = result + ",";
                            result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + userobj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + userobj["user"].ToString() + "\",\"name\":\"" + userobj["name"].ToString() + "\",\"lastname\":\"" + userobj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                        }
                    }
                    else if (autoobj["position"].ToString() == perfiles2["Administrador de sistema"])
                    {
                        objarray = _userTable.Get("profileId", perfiles2["Administrador de sistema"]);
                        users = JsonConvert.DeserializeObject<JArray>(objarray);
                        foreach (JObject obj in users)
                    {
                            if (!result.Contains(obj["_id"].ToString()))
                        {
                            if (cont != 0) result = result + ",";
                                result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                    "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                cont++;
                                break;
                            }

                        }

                    }
                    else
                    {
                        objarray = _userTable.Get("profileId", autoobj["position"].ToString());
                        users = JsonConvert.DeserializeObject<JArray>(objarray);

                        foreach (JObject obj in users)
                        {
                            JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                            /* Patch */
                            list2.Clear();
                            foreach (JObject l in locts)
                                list2.Add(l["id"].ToString());
                            /* END: Patch*/

                            if (list1.Intersect<string>(list2).ToList<string>().Count > 0)
                            {

                                if (!result.Contains(obj["_id"].ToString()))
                                {
                                    if (cont != 0) result = result + ",";
                                    result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                    cont++;
                                }
                            }

                        }
                    }
                }
            }

            result = result + "]";
            return result;
            }
            catch
            {
                return "[]";
            }
        }

        //visto bueno
        public string Getapproval(String movement, JArray location, String userid = null)
        {
            string result = "[";
            string autoarray = "";
            JObject autoobj = new JObject();
            JArray users = new JArray();
            JArray users2 = new JArray();
            JObject userobj = new JObject();
            JArray ele = new JArray();
            string objarray = "";
            int cont = 0;
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();

            String movearray = _profileTable.GetRow(movement);
            JObject moveobj = JsonConvert.DeserializeObject<JObject>(movearray);

            JArray authoslist = JsonConvert.DeserializeObject<JArray>(moveobj["authorization"].ToString());

            String profilesarray = _userProfileTable.GetRows();
            JArray profiles = JsonConvert.DeserializeObject<JArray>(profilesarray);

            Dictionary<string, string> perfiles= new Dictionary<string, string>();
            Dictionary<string, string> perfiles2 = new Dictionary<string, string>();
            perfiles = profiles.ToDictionary(x => (string)x["_id"].ToString(), x => (string)x["name"].ToString());
            perfiles2 = profiles.ToDictionary(x => (string)x["name"].ToString(), x => (string)x["_id"].ToString());
            foreach (JObject cad1 in location)
            {
                ele = getRoute3(cad1["location"].ToString());
                foreach (String ob in ele)
                {
                    list1.Add(ob);
                }
            }

            bool temp = false;
            try
            {
                bool.TryParse(moveobj["temporal"].ToString(), out temp);

            }
            catch { temp = false; }

            if (moveobj["typeMovement"].ToString() == "movement" && temp == false)
            {
                foreach (JObject cad1 in location)
                {
                    if (cad1["locationDestiny"].ToString()!="")
                        ele = getRoute3(cad1["locationDestiny"].ToString());
                    else{
                        ele = getRoute3(cad1["conjuntoDestiny"].ToString());
                    }
                    
                    foreach (String ob in ele)
                    {
                        list3.Add(ob);
                    }
                }
            }

            foreach (string auto in authoslist)
            {
                try{
                autoarray = _autoTable.GetRow(auto);
                autoobj = JsonConvert.DeserializeObject<JObject>(autoarray);
                if (autoobj["type"].ToString() == "approval")
                {
                    if (autoobj["position"].ToString() == perfiles2["Gerente destino"])
                    {
                        objarray = _userTable.Get("profileId", perfiles2["Gerente de conjunto"]);
                        users = JsonConvert.DeserializeObject<JArray>(objarray);

                        //objarray = _userTable.Get("profileId", perfiles2["Gerente regional"]);
                        //users2 = JsonConvert.DeserializeObject<JArray>(objarray);

                        //foreach (JObject o2 in users2) {
                        //    users.Add(o2);
                        //}
                        
                        foreach (JObject obj in users)
                        {
                            JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                            /* Patch */
                            list2.Clear();
                            foreach (JObject l in locts)
                                list2.Add(l["id"].ToString());
                            /* END: Patch*/
                            if (list2.Intersect<string>(list3).ToList<string>().Count > 0)
                            {

                                if (!result.Contains(obj["_id"].ToString()))
                                {
                                    if (cont != 0) result = result + ",";
                                    result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                    cont++;
                                }

                            }
                        }
                    }
                    //else if (autoobj["position"].ToString() == "jefe_inmediato")
                    //{
                    //    objarray = _userTable.GetRow(userid);
                    //    userobj = JsonConvert.DeserializeObject<JObject>(objarray);

                    //    objarray = _userTable.GetRow(userobj["boss"].ToString());
                    //    JObject bossobj = JsonConvert.DeserializeObject<JObject>(objarray);

                    //    if (!result.Contains(bossobj["_id"].ToString()))
                    //    {
                    //        if (cont != 0) result = result + ",";
                    //        result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + bossobj["_id"].ToString() + "\"," +
                    //                    "\"user\":\"" + bossobj["user"].ToString() + "\",\"name\":\"" + bossobj["name"].ToString() + "\",\"lastname\":\"" + bossobj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                    //    }
                    //}
                    else if (autoobj["position"].ToString() == perfiles2["Solicitante"])
                    {
                        objarray = _userTable.GetRow(userid);
                        userobj = JsonConvert.DeserializeObject<JObject>(objarray);


                        if (!result.Contains(userobj["_id"].ToString()))
                        {
                            if (cont != 0) result = result + ",";
                            result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + userobj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + userobj["user"].ToString() + "\",\"name\":\"" + userobj["name"].ToString() + "\",\"lastname\":\"" + userobj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                        }
                    }
                    else if (autoobj["position"].ToString() == perfiles2["Administrador de sistema"])
                    {
                        objarray = _userTable.Get("profileId", perfiles2["Administrador de sistema"]);
                        users = JsonConvert.DeserializeObject<JArray>(objarray);
                        foreach (JObject obj in users)
                        {
                            if (!result.Contains(obj["_id"].ToString()))
                            {
                                if (cont != 0) result = result + ",";
                                result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                    "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                cont++;
                                break;
                            }
                            
                        }
                        
                    }
                    else
                    {
                        objarray = _userTable.Get("profileId",autoobj["position"].ToString());
                        users = JsonConvert.DeserializeObject<JArray>(objarray);

                        foreach (JObject obj in users)
                        {
                            JArray locts = JsonConvert.DeserializeObject<JArray>(obj["userLocations"].ToString());
                            /* Patch */
                            list2.Clear();
                            foreach (JObject l in locts)
                                list2.Add(l["id"].ToString());
                            /* END: Patch*/

                            if (list1.Intersect<string>(list2).ToList<string>().Count > 0)
                            {

                                if (!result.Contains(obj["_id"].ToString()))
                                {
                                    if (cont != 0) result = result + ",";
                                    result = result + "{\"id_auto\":\"" + autoobj["_id"].ToString() + "\",\"id_user\":\"" + obj["_id"].ToString() + "\"," +
                                        "\"user\":\"" + obj["user"].ToString() + "\",\"name\":\"" + obj["name"].ToString() + "\",\"lastname\":\"" + obj["lastname"].ToString() + "\",\"approved\":\"0\",\"date\":\"" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\"}";
                                    cont++;
                                }
                            }

                        }
                    }
                }
            }catch{  }
            }

            result = result + "]";
            return result;

        }

        public String generateFolio(string demandid)
        {
            string folio = "0000000001";
            if (this.Request.IsAjaxRequest())
            {
                if (demandid != null && demandid != "")
                {
                    String rowArray = _demandTable.GetRow(demandid);
                    JObject objects = JsonConvert.DeserializeObject<JObject>(rowArray);

                    folio = objects["folio"].ToString();
                }
                else
                {
                    string demandString = _demandTable.GetRowsAll();
                    if (demandString != null && demandString != "" && demandString != "[]") {
                        JArray demandArray = JsonConvert.DeserializeObject<JArray>(demandString);
                        int cantFolio = demandArray.Count + 1;
                        folio = cantFolio.ToString("0000000000");
                    }
                }
            }
            return folio;
        }

        /*public String generateFolio(string demandid)
        {

            string folio = "0000000001";
            if (this.Request.IsAjaxRequest())
            {
                if (demandid != null && demandid != "")
                {
                    String rowArray = _demandTable.GetRow(demandid);
                    JObject objects = JsonConvert.DeserializeObject<JObject>(rowArray);

                    folio = objects["folio"].ToString();
                }
                else
                {
                    int ultimofolio = 1;
                    string num = "";
                    DateTime fecha = new DateTime();
                    string format = "dd/MM/yyyy HH:mm:ss";
                    DateTime ff = new DateTime();
                    String rowArray = _demandTable.GetRows();
                    JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);
                    if (objects.Count > 0)
                    {
                        DateTime.TryParseExact(objects[0]["CreatedDate"].ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
                        int.TryParse(objects[0]["folio"].ToString(), out ultimofolio);
                        foreach (JObject obj in objects)
                        {
                            DateTime.TryParseExact(obj["CreatedDate"].ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out ff);
                            if (ff > fecha) { fecha = ff; int.TryParse(obj["folio"].ToString(), out ultimofolio); }
                        }
                        folio = "";
                        ultimofolio = ultimofolio + 1;
                        num = ultimofolio.ToString();

                        for (int i = 0; i < 10 - num.Length; i++)
                        {
                            folio = folio + "0";
                        }
                        folio = folio + num;
                    }

                }


            }

            return folio;
        }*/

        public String generateDCTFolio(string demandid)
        {

            string folio = "000001";
            if (this.Request.IsAjaxRequest())
            {
                int ultimofolio = 1;
                string num = "";
                String rowArray = _demandTable.GetAutorizations("deleteType", "planeada", 0);
                JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);
                List<int> listfolios = new List<int>();
                if (objects.Count > 0)
                {
                    foreach(JObject obj in objects)
                    {
                        JToken n2;
                        if (obj.TryGetValue("dctFolio", out n2)) {
                            int.TryParse(obj["dctFolio"].ToString().Replace("DCT-", ""), out ultimofolio);
                            listfolios.Add(ultimofolio);
                        }
                    }
                    folio = "";
                    if (listfolios.Count() > 0)
                    {
                        ultimofolio = listfolios.Max();
                        ultimofolio = ultimofolio + 1;
                    }
                    else ultimofolio = 1;
                    
                    num = ultimofolio.ToString();

                    for (int i = 0; i < 6 - num.Length; i++)
                    {
                        folio = folio + "0";
                    }
                    folio = folio + num;
                }

            }

            return "DCT-" + folio;
        }


        public String generateSTAFolio(string demandid)
        {

            string folio = "000001";
            
            int ultimofolio = 1;
            string num = "";
            String pstring = _demandTable.GetProfileTemporalDemand();
            JArray parray = JsonConvert.DeserializeObject<JArray>(pstring);
            List<string> listmovs = new List<string>();
            foreach(JObject o1 in parray){
                listmovs.Add(o1["_id"].ToString());
            }
            String rowArray = _demandTable.GetdemandsInProfiles(listmovs);
            JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);
            List<int> listfolios = new List<int>();
            if (objects.Count > 0)
            {
                foreach (JObject obj in objects)
                {
                    JToken n2;
                    if (obj.TryGetValue("staFolio", out n2))
                    {
                        int.TryParse(obj["staFolio"].ToString().Replace("STA-", ""), out ultimofolio);
                        listfolios.Add(ultimofolio);
                    }
                }
                folio = "";
                if (listfolios.Count() > 0)
                {
                    ultimofolio = listfolios.Max();
                    ultimofolio = ultimofolio + 1;
                }
                else ultimofolio = 1;
                num = ultimofolio.ToString();

                for (int i = 0; i < 6 - num.Length; i++)
                {
                    folio = folio + "0";
                }
                folio = folio + num;
                }

            return "STA-" + folio;
        }

        private String createAuthoRow(JObject demand, int pend=0)
        {
            string cssClass = "";
            string cssClass2 = "";
            if ((int)demand["status"] >= 1 && (int)demand["status"] <= 5)
            {
                cssClass = "pending";
            }
            else if ((int)demand["status"] == 6)
            {
                if (demand["movement"].ToString() == "Salida Temporal")
                {
                    cssClass = "pending";
                }
                else
                {
                    cssClass = "approved";
                }
            }
            else if ((int)demand["status"] == 8)
            {
                cssClass = "approved";
            }
            else if ((int)demand["status"] == 7)
            {
                cssClass = "denied";
            }
            else if ((int)demand["status"] == 9)
            {
                cssClass = "pending";
            }
            if (pend == 1) cssClass2 = "style='background-color: #1f1f1f;'";
            String movementString = "";

            if (demand["temporal"].ToString() == "true" || demand["temporal"].ToString() == "True")
            {
                try
                {
                    movementString = this.movementStatus["temporal"][(int)demand["status"]];
                }
                catch (Exception EX)
                {

                }

                if ((int)demand["status"] == 8)
                {
                    cssClass = "pending";
                }
            }
            else
            {
                try
                {
                    movementString = this.movementStatus[demand["typeStatus"].ToString()][(int)demand["status"]];
                }
                catch (Exception EX) { }
                }

            JObject obj = GetLastAuthorization(demand["_id"].ToString());

            String fechaDictamen = "";
            try {
                fechaDictamen = demand["dctDate"].ToString();
            }
            catch (Exception ex) {
                fechaDictamen = "";
            }

            String nameDictaminador="";
            try{
                nameDictaminador = getNameUser(demand["adjudicating"].ToString());
            }
            catch{
                nameDictaminador = "";
            }

            String autorizadores = "";
            if (demand["authorizations"].ToString() != "[]")
            {
                JArray a1 = JsonConvert.DeserializeObject<JArray>(demand["authorizations"].ToString());
                String cadena = "";
                int cantNot = 0;
                foreach (JObject o1 in a1)
                {
                    if (o1["approved"].ToString() == "1")
                    {
                        if (cantNot > 0) { cadena += " | "; }
                        try { cadena += o1["name"].ToString() + " " + o1["lastname"].ToString() + " : " + o1["date"].ToString(); }
                        catch { cadena += o1["name"].ToString() + " " + o1["lastname"].ToString() + " : "; }
                    }
                    cantNot++;
                }
                autorizadores = cadena;
            }
            else
            {
                autorizadores = "";
            }

            String row = "<tr ><td><label >" + demand["folio"].ToString() + "</label></td>" +
                "<td>" + demand["namemovement"] + "</td>" +
                "<td>" + demand["CreatorLastName"] + " " + demand["CreatorName"] + "</td>" +
                "<td>" + obj["fecha"] + "</td>" +
                "<td>" + demand["DenyDate"] + "</td>" +
                "<td>" + demand["Deniers"] + "</td>" +
                "<td>" + fechaDictamen + "</td>" +
                "<td>" + nameDictaminador + "</td>" +
                 "<td>" + autorizadores + "</td>" +
                "<td> <div class=' " + cssClass + "'>" + movementString + "</div>" +
                "<td><input value='Ver Detalles' class='btn light_blue verdet' type='button'' data-iddemand='" +
                demand["_id"].ToString() + "' onclick=Verdetalles('" + demand["_id"].ToString() + "')></div></td>" +
                "</tr>";
            return row;
        }

        public JObject GetLastAuthorization(String demandid) {
            string demandarray = _demandTable.GetRow(demandid);
            JObject demandobj = JsonConvert.DeserializeObject<JObject>(demandarray);
            JArray autos = JsonConvert.DeserializeObject<JArray>(demandobj["authorizations"].ToString());

            JObject result = new JObject();
            result["fecha"] = "";
            result["deniers"] = "";
            DateTime fecha = new DateTime();
            string format = "dd/MM/yyyy HH:mm:ss";
            DateTime ff = new DateTime();
            if (autos.Count > 0)
            {
                try
                {

                    DateTime.TryParseExact(demandobj["CreatedDate"].ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
                    foreach (JObject obj in autos)
                    {
                        if (obj["approved"].ToString() != "2")
                        {
                            DateTime.TryParseExact(obj["date"].ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out ff);
                            if (ff > fecha) { fecha = ff; }
                        }
                        else {
                            result["deniers"] = getProfileUser(obj["id_user"].ToString()) + ": " + obj["name"].ToString() + " " + obj["lastname"].ToString();
                        }
                        
                    }

                    if (ff.ToString("dd/MM/yyyy HH:mm:ss") == "01/01/0001 00:00:00")
                        result["fecha"]= "";
                    else
                        result["fecha"]= ff.ToString("dd/MM/yyyy HH:mm:ss");


                }
                catch (Exception ex)
                {

                    
                }
            }
            return result;
        }

        public JsonResult getAutorizaciones(string userid)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                JObject obj2 = new JObject();
                int cont = 0;
                string misautorizaciones = "";
                List<String> l1 = new List<String>();
                if (userid != null && userid != "")
                {

                    //Check if it's an admin
                    JObject userInfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(userid));
                    JObject userProfInfo = JsonConvert.DeserializeObject<JObject>(_userProfileTable.GetRow(userInfo["profileId"].ToString()));

                    if (userProfInfo["name"].ToString() == "Administrador de sistema")
                    {
                        JArray autosAdmin = new JArray();
                        string cadenaAdmin = "";
                        JArray demandobjs = JsonConvert.DeserializeObject<JArray>(_demandTable.GetAutorizations(null));
                        foreach (JObject demand in demandobjs)
                        {
                            try
                            {
                                if (demand["temporal"].ToString() == "")
                                {
                                    demand["temporal"] = false;
                                }
                            }
                            catch
                            {
                                demand["temporal"] = false;
                            }
                            autosAdmin = JsonConvert.DeserializeObject<JArray>(demand["approval"].ToString());
                            bool isAdiminApprol = false;
                            if (autosAdmin == null)
                            { continue; }
                            foreach (JObject obj in autosAdmin)
                            {
                                if (obj["id_user"].ToString() == userid)
                                {
                                    isAdiminApprol = true;
                                    l1.Add(demand["folio"].ToString());

                                    if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7")
                                    {
                                        cadenaAdmin = this.createAuthoRow(demand, 1);
                                        cont++;
                                    }
                                    else
                                    {
                                        cadenaAdmin = this.createAuthoRow(demand);
                                    }
                                }
                            }
                            if (!isAdiminApprol) cadenaAdmin = this.createAuthoRow(demand);
                            misautorizaciones = misautorizaciones + cadenaAdmin;
                        }
                        goto Finish;
                    }
                    //END -- If it's Admin


                    string userstring = _userTable.GetRow(userid);
                    if (userstring == "" || userstring == null) return null;
                    JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);

                    string demandarray = _demandTable.GetAutorizations("authorizations.id_user", userid);
                    JArray demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);

                    JArray autos = new JArray();
                    string cadena = "";
                    cont = 0;
                    foreach (JObject demand in demandobj)
                    {
                        try
                        {
                            if (demand["temporal"].ToString() == "")
                            {
                                demand["temporal"] = false;
                            }
                        }
                        catch
                        {
                            demand["temporal"] = false;
                        }
                        if (demand["authorizations"].ToString() == "[]" || demand["status"].ToString() == "Cancelada" || demand["status"].ToString() == "1" || demand["status"].ToString() == "2"
                         //   || demand["status"].ToString() == "4" || demand["status"].ToString() == "5"
                            )
                            continue;

                        autos = JsonConvert.DeserializeObject<JArray>(demand["authorizations"].ToString());
                        foreach (JObject obj in autos)
                        {
                            if (obj["id_user"].ToString() == userid)
                            {
                                l1.Add(demand["folio"].ToString());
                                
                                if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7")
                                {
                                    cadena = this.createAuthoRow(demand,1);
                                    cont++;
                                }
                                else {
                                    cadena = this.createAuthoRow(demand);
                                }
                            }
                        }
                        misautorizaciones = misautorizaciones + cadena;
                    }


                    demandarray = _demandTable.GetAutorizations("adjudicating", userid);
                    demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);

                    foreach (JObject demand in demandobj)
                    {
                        try
                        {
                            if (demand["temporal"].ToString() == "")
                            {
                                demand["temporal"] = false;
                            }
                        }
                        catch
                        {
                            demand["temporal"] = false;
                        }
                        try
                        {
                            if (demand["adjudicating"].ToString() == userid)
                            {
                                l1.Add(demand["folio"].ToString());
                              
                                if (demand["status"].ToString() == "1")
                                {
                                    cadena = this.createAuthoRow(demand,1);
                                    cont++;
                                }
                                else {
                                    cadena = this.createAuthoRow(demand);
                                }
                                misautorizaciones = misautorizaciones + cadena;
                            }


                        }
                        catch
                        {

                        }
                    }

                    //Get Contable for deleteDemand - Abiss
                    demandarray = _demandTable.GetAutorizations("contador.id_user", userid);
                    if (demandarray != null && demandarray != "" && demandarray != "[]")
                    {
                        demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);
                        foreach (JObject demand in demandobj)
                        {
                            try
                            {
                                if (demand["temporal"].ToString() == "")
                                {
                                    demand["temporal"] = false;
                                }
                            }
                            catch
                            {
                                demand["temporal"] = false;
                            }
                            if (Convert.ToInt16(demand["status"].ToString()) >= 4)
                            {
                                try
                                {
                                    autos = JsonConvert.DeserializeObject<JArray>(demand["contador"].ToString());
                                    foreach (JObject obj in autos)
                                    {
                                        if (obj["id_user"].ToString() == userid && !l1.Contains(demand["folio"].ToString()))
                                        {
                                            l1.Add(demand["folio"].ToString());

                                            if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7")
                                            {
                                                cadena = this.createAuthoRow(demand, 1);
                                                cont++;
                                            }
                                            else
                                            {
                                                cadena = this.createAuthoRow(demand);
                                            }
                                            misautorizaciones = misautorizaciones + cadena;
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    //END

                    //Get Contable for deleteDemand - Abiss
                    demandarray = _demandTable.GetAutorizations("witness.id_user", userid);
                    if (demandarray != null && demandarray != "" && demandarray != "[]")
                    {
                        demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);
                        foreach (JObject demand in demandobj)
                        {
                            try
                            {
                                if (demand["temporal"].ToString() == "")
                                {
                                    demand["temporal"] = false;
                                }
                            }
                            catch
                            {
                                demand["temporal"] = false;
                            }
                            if (Convert.ToInt16(demand["status"].ToString()) >= 4 && demand["status"].ToString() != "9")
                            {
                                try
                                {
                                    autos = JsonConvert.DeserializeObject<JArray>(demand["witness"].ToString());
                                    foreach (JObject obj in autos)
                                    {
                                        if (obj["id_user"].ToString() == userid && !l1.Contains(demand["folio"].ToString()))
                                        {
                                            l1.Add(demand["folio"].ToString());

                                            if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7"
                                                && demand["setRecepit"] != null && demand["setRecepit"].ToString() == "true")
                                            {
                                                cadena = this.createAuthoRow(demand, 1);
                                                cont++;
                                            }
                                            else
                                            {
                                                cadena = this.createAuthoRow(demand);
                                            }
                                            misautorizaciones = misautorizaciones + cadena;
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    //END

                    demandarray = _demandTable.GetAutorizations("approval.id_user", userid);
                    demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);
                    foreach (JObject demand in demandobj)
                    {
                        try
                        {
                            if (demand["temporal"].ToString() == "")
                            {
                                demand["temporal"] = false;
                            }
                        }
                        catch
                        {
                            demand["temporal"] = false;
                        }
                        try
                        {
                            string movarray = _movementProfileTable.GetRow(demand["movement"].ToString());
                            JObject movobj = JsonConvert.DeserializeObject<JObject>(movarray);

                            if (movobj["typeMovement"].ToString() == "delete")
                            {
                                int dmd = 0;
                                int.TryParse(demand["status"].ToString(), out dmd);
                                //if (dmd < 5) continue; Abiss
                            }
                        }
                        catch (Exception ex) { }
                        try
                        {
                            autos = JsonConvert.DeserializeObject<JArray>(demand["approval"].ToString());
                            foreach (JObject obj in autos)
                            {
                                if (obj["id_user"].ToString() == userid && !l1.Contains(demand["folio"].ToString()))
                                {
                                    l1.Add(demand["folio"].ToString());
                                    
                                    if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7")
                                    {
                                        cadena = this.createAuthoRow(demand,1);
                                        cont++;
                                    }
                                    else {
                                        cadena = this.createAuthoRow(demand);
                                    }
                                    misautorizaciones = misautorizaciones + cadena;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                    JObject profstring = JsonConvert.DeserializeObject<JObject>(_userProfileTable.GetRow(userobj["profileId"].ToString()));

                    //es el usuario gerente de conjunto?
                    if (profstring["name"].ToString() == "Gerente de conjunto")
                    {
                        String usersarray = _userTable.Get("boss", userid);
                        JArray subusers=JsonConvert.DeserializeObject<JArray>(usersarray);

                        List<String> listsubuesrs = new List<string>();
                        foreach (JObject subu in subusers) {
                            listsubuesrs.Add(subu["_id"].ToString());
                        }

                        String movsarray = _movementProfileTable.Get("typeMovement", "movement"); 
                        JArray submovs=JsonConvert.DeserializeObject<JArray>(movsarray);

                        List<String> listmovs = new List<string>();
                        foreach (JObject subu in submovs) {
                            try
                            {
                                if(subu["temporal"].ToString()=="True")
                                    listmovs.Add(subu["_id"].ToString());
                            }
                            catch { }
                            
                        }

                        demandarray = _demandTable.GetAutorizations3(listsubuesrs, listmovs);
                        demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);

                        foreach (JObject demand in demandobj)
                        {
                            try
                            {
                                if (demand["temporal"].ToString() == "")
                                {
                                    demand["temporal"] = false;
                                }
                            }
                            catch
                            {
                                demand["temporal"] = false;
                            }
                            try
                            {
                                if (!l1.Contains(demand["folio"].ToString()))
                                {
                                    l1.Add(demand["folio"].ToString());

                                    if (demand["status"].ToString() == "6")
                                    {
                                        cadena = this.createAuthoRow(demand, 1);
                                        misautorizaciones = misautorizaciones + cadena;
                                        cont++;
                                    }
                                    else if (demand["status"].ToString() == "7" || demand["status"].ToString() == "8")
                                    {
                                        cadena = this.createAuthoRow(demand);
                                        misautorizaciones = misautorizaciones + cadena;
                                    }

                                }
                            }
                            catch { }
                        }
                    }
                }

            Finish:
                obj2["cadena"] = misautorizaciones;
                obj2["cont"] = cont.ToString();

                return Json(JsonConvert.SerializeObject(obj2));

            }
            else
            {
                return null;
            }
        }

        public JsonResult getAutorizaciones2(string movement, string stats, string userid)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                JObject obj2 = new JObject();
                int cont = 0;
                string misautorizaciones = "";
                List<String> l1 = new List<String>();
                if (userid != null && userid != "")
                {

                    //Check if it's an admin
                    JObject userInfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(userid));
                    JObject userProfInfo = JsonConvert.DeserializeObject<JObject>(_userProfileTable.GetRow(userInfo["profileId"].ToString()));

                    if (userProfInfo["name"].ToString() == "Administrador de sistema")
                    {
                        JArray autosAdmin = new JArray();
                        string cadenaAdmin = "";
                        JArray demandobjs = JsonConvert.DeserializeObject<JArray>(_demandTable.GetAutorizations2(movement, stats, null));
                        //string demandarray = _demandTable.GetAutorizations2(movement, stats, "authorizations.id_user", userid);
                        foreach (JObject demand in demandobjs)
                        {
                            try
                            {
                                if (demand["temporal"].ToString() == "")
                                {
                                    demand["temporal"] = false;
                                }
                            }
                            catch
                            {
                                demand["temporal"] = false;
                            }
                            try {
                                autosAdmin = JsonConvert.DeserializeObject<JArray>(demand["approval"].ToString());
                                bool isAdiminApprol = false;
                                foreach (JObject obj in autosAdmin)
                                {
                                    if (obj["id_user"].ToString() == userid)
                                    {
                                        isAdiminApprol = true;
                                        l1.Add(demand["folio"].ToString());

                                        if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7")
                                        {
                                            cadenaAdmin = this.createAuthoRow(demand, 1);
                                            cont++;
                                        }
                                        else
                                        {
                                            cadenaAdmin = this.createAuthoRow(demand);
                                        }
                                    }
                                }
                                if (!isAdiminApprol) cadenaAdmin = this.createAuthoRow(demand);
                                misautorizaciones = misautorizaciones + cadenaAdmin;
                            }
                            catch (Exception ex) {
                                continue;
                            }
                            
                            
                        }
                        goto Finish;
                    }
                    //END -- If it's Admin

                    string userstring = _userTable.GetRow(userid);
                    if (userstring == "" || userstring == null) return null;
                    JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);

                    string demandarray = _demandTable.GetAutorizations2(movement, stats, "authorizations.id_user", userid);
                    JArray demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);
                    JArray autos = new JArray();
                    string cadena = "";
                    cont = 0;
                    foreach (JObject demand in demandobj)
                    {
                        try
                        {
                            if (demand["temporal"].ToString() == "")
                            {
                                demand["temporal"] = false;
                            }
                        }
                        catch
                        {
                            demand["temporal"] = false;
                        }
                        if (demand["authorizations"].ToString() == "[]" || demand["status"].ToString() == "Cancelada" || demand["status"].ToString() == "1" || demand["status"].ToString() == "2"
                            
                            )
                            continue;

                        autos = JsonConvert.DeserializeObject<JArray>(demand["authorizations"].ToString());
                        foreach (JObject obj in autos)
                        {
                            try
                            {
                                if (obj["id_user"].ToString() == userid)
                                {
                                    l1.Add(demand["folio"].ToString());

                                    if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7")
                                    {
                                        cadena = this.createAuthoRow(demand, 1);
                                        cont++;
                                    }
                                    else
                                    {
                                        cadena = this.createAuthoRow(demand);
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                        misautorizaciones = misautorizaciones + cadena;
                    }


                    demandarray = _demandTable.GetAutorizations2(movement, stats, "adjudicating", userid);
                    demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);

                    foreach (JObject demand in demandobj)
                    {
                        try
                        {
                            if (demand["temporal"].ToString() == "")
                            {
                                demand["temporal"] = false;
                            }
                        }
                        catch { demand["temporal"] = false; }
                        try
                        {
                            if (demand["adjudicating"].ToString() == userid)
                            {
                                l1.Add(demand["folio"].ToString());

                                if (demand["status"].ToString() != "1")
                                {
                                    cadena = this.createAuthoRow(demand, 1);
                                    cont++;
                                }
                                else
                                {
                                    cadena = this.createAuthoRow(demand);
                                }
                                misautorizaciones = misautorizaciones + cadena;
                            }
                        }
                        catch { }
                    }

                    demandarray = _demandTable.GetAutorizations2(movement, stats, "approval.id_user", userid);
                    demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);
                    foreach (JObject demand in demandobj)
                    {
                        try
                        {
                            if (demand["temporal"].ToString() == "")
                            {
                                demand["temporal"] = false;
                            }
                        }
                        catch { demand["temporal"] = false; }
                        try
                        {
                            string movarray = _movementProfileTable.GetRow(demand["movement"].ToString());
                            JObject movobj = JsonConvert.DeserializeObject<JObject>(movarray);

                            if (movobj["typeMovement"].ToString() == "delete")
                            {
                                int dmd = 0;
                                int.TryParse(demand["status"].ToString(), out dmd);
                                if (dmd < 5) continue;
                            }
                            try
                            {
                                autos = JsonConvert.DeserializeObject<JArray>(demand["approval"].ToString());
                                foreach (JObject obj in autos)
                                {
                                    try
                                    {
                                        if (obj["id_user"].ToString() == userid && !l1.Contains(demand["folio"].ToString()))
                                        {
                                            l1.Add(demand["folio"].ToString());

                                            if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7")
                                            {
                                                cadena = this.createAuthoRow(demand, 1);
                                                cont++;
                                            }
                                            else
                                            {
                                                cadena = this.createAuthoRow(demand);
                                            }
                                            misautorizaciones = misautorizaciones + cadena;
                                        }
                                    }
                                    catch { }
                                }
                            }
                            catch { }

                        }
                        catch { }
                    }


                    //For Contador show autoriztions
                    demandarray = _demandTable.GetAutorizations2(movement, stats, "contador.id_user", userid);
                    demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);
                    foreach (JObject demand in demandobj)
                    {
                        try
                        {
                            if (demand["temporal"].ToString() == "")
                            {
                                demand["temporal"] = false;
                            }
                        }
                        catch { demand["temporal"] = false; }
                        try
                        {
                            string movarray = _movementProfileTable.GetRow(demand["movement"].ToString());
                            JObject movobj = JsonConvert.DeserializeObject<JObject>(movarray);

                            if (movobj["typeMovement"].ToString() == "delete")
                            {
                                int dmd = 0;
                                int.TryParse(demand["status"].ToString(), out dmd);
                                if (dmd < 4) continue;

                                try
                                {
                                    if (Convert.ToInt16(demand["status"].ToString()) >= 4)
                                    {
                                        autos = JsonConvert.DeserializeObject<JArray>(demand["contador"].ToString());
                                        foreach (JObject obj in autos)
                                        {
                                            try
                                            {
                                                if (obj["id_user"].ToString() == userid && !l1.Contains(demand["folio"].ToString()))
                                                {
                                                    l1.Add(demand["folio"].ToString());

                                                    if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7")
                                                    {
                                                        cadena = this.createAuthoRow(demand, 1);
                                                        cont++;
                                                    }
                                                    else
                                                    {
                                                        cadena = this.createAuthoRow(demand);
                                                    }
                                                    misautorizaciones = misautorizaciones + cadena;
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }

                    //For Testigo show autorizations
                    demandarray = _demandTable.GetAutorizations2(movement, stats, "witness.id_user", userid);
                    demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);
                    foreach (JObject demand in demandobj)
                    {
                        try
                        {
                            if (demand["temporal"].ToString() == "")
                            {
                                demand["temporal"] = false;
                            }
                        }
                        catch { demand["temporal"] = false; }
                        try
                        {
                            string movarray = _movementProfileTable.GetRow(demand["movement"].ToString());
                            JObject movobj = JsonConvert.DeserializeObject<JObject>(movarray);

                            if (movobj["typeMovement"].ToString() == "delete")
                            {
                                int dmd = 0;
                                int.TryParse(demand["status"].ToString(), out dmd);
                                if (dmd < 4) continue;
                                try
                                {
                                    if (Convert.ToInt16(demand["status"].ToString()) >= 4 && demand["status"].ToString() != "9")
                                    {
                                        autos = JsonConvert.DeserializeObject<JArray>(demand["witness"].ToString());
                                        foreach (JObject obj in autos)
                                        {
                                            try
                                            {
                                                if (obj["id_user"].ToString() == userid && !l1.Contains(demand["folio"].ToString()))
                                                {
                                                    l1.Add(demand["folio"].ToString());

                                                    if (obj["approved"].ToString() == "0" && demand["status"].ToString() != "7"
                                                    && demand["setRecepit"] != null && demand["setRecepit"].ToString() == "true")
                                                    {
                                                        cadena = this.createAuthoRow(demand, 1);
                                                        cont++;
                                                    }
                                                    else
                                                    {
                                                        cadena = this.createAuthoRow(demand);
                                                    }
                                                    misautorizaciones = misautorizaciones + cadena;
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }

                    JObject profstring = JsonConvert.DeserializeObject<JObject>(_userProfileTable.GetRow(userobj["profileId"].ToString()));
                    //es el usuario gerente de conjunto?
                    if (profstring["name"].ToString() == "Gerente de conjunto")
                    {
                        String usersarray = _userTable.Get("boss", userid);
                        JArray subusers = JsonConvert.DeserializeObject<JArray>(usersarray);

                        List<String> listsubuesrs = new List<string>();
                        foreach (JObject subu in subusers)
                        {
                            listsubuesrs.Add(subu["_id"].ToString());
                        }

                        String movsarray = _movementProfileTable.Get("typeMovement", "movement");
                        JArray submovs = JsonConvert.DeserializeObject<JArray>(movsarray);

                        List<String> listmovs = new List<string>();
                        foreach (JObject subu in submovs)
                        {
                            try
                            {
                                if (subu["temporal"].ToString() == "True")
                                    listmovs.Add(subu["_id"].ToString());
                            }
                            catch { }

                        }

                        demandarray = _demandTable.GetAutorizations3(listsubuesrs, listmovs);
                        demandobj = JsonConvert.DeserializeObject<JArray>(demandarray);

                        foreach (JObject demand in demandobj)
                        {
                            try
                            {
                                if (demand["temporal"].ToString() == "")
                                {
                                    demand["temporal"] = false;
                                }
                            }
                            catch
                            {
                                demand["temporal"] = false;
                            }
                            try
                            {
                                if (!l1.Contains(demand["folio"].ToString()))
                                {
                                    l1.Add(demand["folio"].ToString());

                                    if (demand["status"].ToString() == "6")
                                    {
                                        cadena = this.createAuthoRow(demand, 1);
                                        misautorizaciones = misautorizaciones + cadena;
                                        cont++;
                                    }
                                    else if (demand["status"].ToString() == "7" || demand["status"].ToString() == "8")
                                    {
                                        cadena = this.createAuthoRow(demand);
                                        misautorizaciones = misautorizaciones + cadena;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }

            Finish:
                obj2["cadena"] = misautorizaciones;
                obj2["cont"] = cont.ToString();

                return Json(JsonConvert.SerializeObject(obj2));

            }
            else
            {
                return null;
            }
        }

        public JsonResult getDemamdTable(string movement, string stats)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                //String rowArray = _demandTable.GetRows();
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }

                string myId = Session["_id"].ToString();

                bool admin = false;
                if (Session["Username"].ToString() == "admin")
                    admin = true;
               
                string newJoin = _demandTable.GetDemandTable(movement, stats, myId,admin);

                String rows = "";
                JArray objects = JsonConvert.DeserializeObject<JArray>(newJoin);
                JArray objects1 = new JArray();
                foreach (JObject obj in objects) {
                    try
                    {
                        if (obj["temporal"].ToString() == "") {
                            obj["temporal"] = false;
                        }
                    }
                    catch {
                        obj["temporal"] = false;
                    }

                    if (obj["authorizations"].ToString() != "[]")
                    {
                        JArray a1 = JsonConvert.DeserializeObject<JArray>(obj["authorizations"].ToString());
                        String cadena="";
                        //String cadena2 = "";
                        String dateauto="";
                        String datedeny="";
                        int cantNot = 0;
                        foreach (JObject o1 in a1)
                        {
                            if (o1["approved"].ToString() == "1")
                            {
                                if (cantNot > 0) { cadena += " | "; }
                                try { cadena += o1["name"].ToString() + " " + o1["lastname"].ToString(); }
                                catch { cadena += o1["name"].ToString() + " " + o1["lastname"].ToString(); }
                            }
                            cantNot++;
                        }

                        obj["autorizers"] = cadena;
                        try
                        {   JToken x1;
                        if (obj.TryGetValue("ApprovedDate", out x1))
                        {
                            obj["ApprovedDate"] = dateauto;
                        }
                        else
                        {
                            obj.Add("ApprovedDate", dateauto);
                        }
                        }
                        catch
                        {

                        }
                    }
                    else {
                        obj["autorizers"] = "";
                        obj.Add("ApprovedDate", "");
                    }
                    try{
                        obj["dctName"] = getNameUser(obj["adjudicating"].ToString());
                    }
                    catch{
                        obj["dctName"] = "";
                    }
                   
                    objects1.Add(obj);
                }

                
                rows = JsonConvert.SerializeObject(objects1);
                return Json(rows);

            }
            return null;
        }

        public String UpdateStatusDemand(String selectedID, int estatus)
        {
            bool edit = false;
            bool editclient = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("demand", "c", dataPermissions);
            edit = validatepermissions.getpermissions("demand", "u", dataPermissions);

            accessClient = validatepermissions.getpermissions("demand", "c", dataPermissionsClient);
            editclient = validatepermissions.getpermissions("demand", "u", dataPermissionsClient);


            if ((access == true && accessClient == true) || (edit == true && editclient == true))
            {
                if (this.Request.IsAjaxRequest())
                {
                    String demandID = (selectedID == "null") ? null : selectedID; //is this an insert or an update?, converting null in javascript to null in c#

                    dynamic demand = _demandTable.GetRow(demandID);
                    /*the gived id does not exists*/
                    if (demandID != null && (demand == null))
                    {
                        return "El id especificado no existe";
                    }

                    if (demandID != null)
                    {
                        _demandTable.UpdateRowStatus("status", estatus, demandID);
                        return "success";
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        public string getCategory(string id)
        {
            try
            {
                JObject refobj = JsonConvert.DeserializeObject<JObject>(_objectReferenceTable.GetRow(id));
                return refobj["parentCategory"].ToString();
            }
            catch
            {
                return "null";
            }
        }
        public JsonResult getDemandDetails(String selectedID, String userid=null)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                JObject objs=new JObject();
                string rowString = "";
                string demand = _demandTable.GetRow(selectedID);
                if (demand == null)
                {
                    this.Response.StatusCode = 400;
                    return null;
                }
                JObject obj = JsonConvert.DeserializeObject<JObject>(demand);
             
                List<string> objsid = (from ob in obj["objects"].Children() select ob["id"].Value<string>()).ToList();
                List<string> referencesid = new List<string>();
                List<string> locationsid = new List<string>();
                JArray parents = new JArray();
                Dictionary<string, string> parentsdict = new Dictionary<string, string>();
                Dictionary<string, string> referencesdict = new Dictionary<string, string>();
                rowString = _profileTable.GetRow(obj["movement"].ToString());
                obj.Add("movementId", obj["movement"].ToString());
                try
                {
                    objs = JsonConvert.DeserializeObject<JObject>(rowString);
                    obj["movement"] = objs["name"].ToString();
                    try { 
                        obj["autotransfer"] = objs["autotransfer"].ToString();
                    }catch{
                        obj["autotransfer"] = "false";
                    }
                    try
                    {
                        obj["temporal"] = objs["temporal"].ToString();
                    }
                    catch
                    {
                        obj["temporal"] = "false";
                    }
                }
                catch
                {
                }

                string typeMovement = objs["typeMovement"].ToString();
                obj.Add("typeMovement", typeMovement);
                try
                {
                    locationsid = (from ob in obj["objects"].Children() select ob["location"].Value<string>()).ToList();
                    String parentsloc = _objectTable.getParents(locationsid);
                    parents = JsonConvert.DeserializeObject<JArray>(parentsloc);
                    parentsdict = parents.ToDictionary(x => (string)x["_id"].ToString(), x => (string)x["nameparent"].ToString());
                }
                catch { }
                JArray referencesja = new JArray();
                try
                {
                    referencesid = (from ob in obj["objects"].Children() select ob["objectReference"].Value<string>()).ToList();
                    String referencesobj = _objectTable.getCategory(referencesid);
                    referencesja = JsonConvert.DeserializeObject<JArray>(referencesobj);
                      referencesdict = referencesja.ToDictionary(x => (string)x["_id"].ToString(), x => (string)x["assetType"].ToString());
               }
                catch { }
                  String joinobjs="[]";
                if (typeMovement == "create")
                {
                    joinobjs = _objectTable.getMMO(objsid,0);
                }else
                {
                    joinobjs = _objectTable.getMMO(objsid, 1);
                }
               
                JArray joinarrayobj = new JArray();
                try
                {
                    joinarrayobj = JsonConvert.DeserializeObject<JArray>(joinobjs);
                    foreach (JObject item in obj["objects"])
                    {
                        try
                        {
                            foreach (JObject values in joinarrayobj)
                            {
                                try
                                {
                                    if (item["id"].ToString() == values["_id"].ToString())
                                    {
                                        JToken tk;

                                        if (item.TryGetValue("marca", out tk))
                                        {
                                            try
                                            {
                                                if (item["marca"].ToString()=="")
                                                item["marca"] = values["marca"].ToString();
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                item.Add("marca", values["marca"].ToString());
                                            }
                                            catch { }
                                        }

                                        if (item.TryGetValue("serie", out tk))
                                        {
                                            try
                                            {
                                                if (item["serie"].ToString() == "")
                                                item["serie"] = values["serie"].ToString();
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                item.Add("serie", values["serie"].ToString());
                                            }
                                            catch { }
                                        }

                                        if (item.TryGetValue("modelo", out tk))
                                        {
                                            try
                                            {
                                                if (item["modelo"].ToString() == "")
                                                item["modelo"] = values["modelo"].ToString();
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                item.Add("modelo", values["modelo"].ToString());
                                            }
                                            catch { }
                                        }
                                        if (item.TryGetValue("object_id", out tk))
                                        {
                                            try
                                            {
                                                if (item["object_id"].ToString() == "")
                                                item["object_id"] = values["object_id"].ToString();
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                item.Add("object_id", values["object_id"].ToString());
                                            }
                                            catch { }
                                        }

                                    }
                                }
                                catch
                                {

                                }
                            }

                        }
                        catch { }  

                    }
                }
                catch { }

                if (typeMovement == "create")
                {
                    foreach (JObject ob in obj["objects"])
                    {
                        try
                        {
                            rowString = _objectReferenceTable.GetRow(ob["id"].ToString());
                            objs = JsonConvert.DeserializeObject<JObject>(rowString);
                            ob["name"] = objs["name"].ToString();

                            rowString = _categoryTable.GetRow(objs["parentCategory"].ToString());
                            JObject objs1 = JsonConvert.DeserializeObject<JObject>(rowString);

                            ob["nameCategory"] = objs1["name"].ToString();

                            if (objs["ext"].ToString() != "")
                                ob["image"] = "/Uploads/Images/ObjectReferences/" + ob["id"].ToString() + "." + objs["ext"].ToString();
                            else
                                ob["image"] = "";

                            rowString = _locationTable.GetRow(ob["location"].ToString());
                            objs = JsonConvert.DeserializeObject<JObject>(rowString);
                            ob["locationName"] = objs["name"].ToString();

                            try
                            {
                                string resultdict;
                                if (parentsdict.TryGetValue(ob["location"].ToString(), out resultdict))
                                {
                                    JToken sd;
                                    if(ob.TryGetValue("conjuntoname",out sd)){
                                        ob["conjuntoname"] = resultdict;
                                    }
                                    else
                                    {
                                        ob.Add("conjuntoname", resultdict);
                                    }
                                }
                            }
                            catch { }
                            //    try
                            //{
                            //    string resultdict;
                            //    if (referencesdict.TryGetValue(ob["objectReference"].ToString(), out resultdict))
                            //    {
                            //        JToken jd;
                            //        if (ob.TryGetValue("assetType", out jd))
                            //        {
                            //            ob["assetType"] = resultdict;
                            //        }
                            //        else
                            //        {
                            //            ob.Add("assetType", resultdict);
                            //        }
                            //    }
                            //}
                            //catch { }
                        }
                        catch (Exception e) { }
                    }
                }
                else
                {
                    if (typeMovement == "delete" && obj["deleteType"].ToString() == "planeada")
                    {

                        //START - Get details for delete with distruction
                        //Location details
                        if (obj["destinyOptions"].ToString() == "destruccion" &&
                            obj["objects"] != null && obj["objects"].ToString() != "" && obj["objects"].ToString() != "[]")
                        {
                            JObject locat = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(obj["objects"][0]["location"].ToString()));
                            JObject conju = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(locat["parent"].ToString()));
                            obj["ACTDESconjunto"] = conju["name"].ToString();

                            String address = "";

                            try
                            {
                                address = conju["_HTKFieldcalle"].ToString();
                            }
                            catch (Exception ex) { }
                            try
                            {
                                address=address+ " " + conju["_HTKFieldcolonia"].ToString();
                            }
                            catch (Exception ex) { }
                            try
                            {
                                address = address + " " + conju["_HTKFieldMunicipio"].ToString();
                            }
                            catch (Exception ex) { }
                            try
                            {
                                address = address + ". " + conju["_HTKFieldciudad"].ToString();
                            }
                            catch (Exception ex) { }
                            try
                            {
                                address = address + " C.P." + conju["_HTKFieldzipcode"].ToString();
                            }
                            catch (Exception ex) { }
                            try
                            {
                                address = address + " " + conju["_HTKFieldestado"].ToString();
                            }
                            catch (Exception ex) { }
                            try
                            {
                                address = address + ", " + conju["_HTKFieldpais"].ToString();
                            }
                            catch (Exception ex) { }
                            
                            obj["ACTDESaddress"] = address;
                        }
                      //END

                        string adjudicating = obj["adjudicating"].ToString();
                        JObject adjName = new JObject();
                        if (adjudicating != "")
                        {
                            try
                            {
                                //Type
                                JArray adjudica = JsonConvert.DeserializeObject<JArray>(_dictaminadorTable.Get("user.value", adjudicating));
                                foreach (JObject adjud in adjudica)
                                    obj["adjudicatingType"] = adjud["type"]["text"].ToString();
                                //Name
                                adjName = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(adjudicating));
                                obj["adjudicating"] = adjName["lastname"].ToString() + " " + adjName["name"].ToString();
                                obj["adjudicatingID"] = adjudicating;
                            }
                            catch (Exception e) { }
                        }

                        adjName = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(userid));
                        try
                        {
                            obj["userPosition"] = adjName["position"].ToString();
                        }
                        catch (Exception e) { }
                    }
                    bool temp = false;
                    try
                    {
                        bool.TryParse(objs["temporal"].ToString(), out temp);

                    }
                    catch { temp = false; }
                    string idconjunto="";
                    foreach (JObject ob in obj["objects"])
                    {
                        try
                        {
                            string objectRow = "";
                            if (typeMovement == "movement" && temp == false)
                            {
                                objectRow = _objectTable.GetRow(ob["id"].ToString());
                                objs = JsonConvert.DeserializeObject<JObject>(objectRow);
                                ob["EPC"] = objs["EPC"].ToString();
                                ob["modelo"] = objs["modelo"].ToString();
                                ob["marca"] = objs["marca"].ToString();
                                ob["serie"] = objs["serie"].ToString();
                                ob["object_id"] = objs["object_id"].ToString();
                                ob["name"] = objs["name"].ToString();
                            }
                            

                            rowString = _objectReferenceTable.GetRow(ob["objectReference"].ToString());
                            objs = JsonConvert.DeserializeObject<JObject>(rowString);
                            ob["name"] = objs["name"].ToString();

                            rowString = _categoryTable.GetRow(objs["parentCategory"].ToString());
                            JObject objs1 = JsonConvert.DeserializeObject<JObject>(rowString);

                            ob["nameCategory"] = objs1["name"].ToString();

                            if (objs["ext"].ToString() != "")
                                ob["image"] = "/Uploads/Images/ObjectReferences/" + ob["objectReference"].ToString() + "." + objs["ext"].ToString();
                            else
                                ob["image"] = "";

                            rowString = _locationTable.GetRow(ob["location"].ToString());
                            objs1 = JsonConvert.DeserializeObject<JObject>(rowString);
                            ob["locationName"] = objs1["name"].ToString();

                            rowString = _locationTable.GetRow(objs1["parent"].ToString());
                            idconjunto = objs1["parent"].ToString();
                            JObject objs2 = JsonConvert.DeserializeObject<JObject>(rowString);
                            ob["conjuntoName"] = objs2["name"].ToString();

                            

                            if (typeMovement == "movement" && temp==false )
                            {
                                if (ob["locationDestiny"].ToString() != "null" && ob["locationDestiny"] != null)
                                {
                                    try {
                                        rowString = _locationTable.GetRow(ob["locationDestiny"].ToString());
                                        objs = JsonConvert.DeserializeObject<JObject>(rowString);
                                        ob["locationDestinyName"] = objs["name"].ToString();
                                    }
                                    catch {
                                        ob["locationDestinyName"] = "";
                                    }
                                   

                                }
                                else { ob["locationDestinyName"] = ""; }

                                try {
                                    rowString = _locationTable.GetRow(ob["conjuntoDestiny"].ToString());
                                    objs = JsonConvert.DeserializeObject<JObject>(rowString);
                                    ob["conjuntoDestinyName"] = objs["name"].ToString();
                                }
                                catch {
                                    ob["conjuntoDestinyName"] = "";
                                }
                                
                            }
                            
                        }
                        catch (Exception e) { }

                        try
                        {
                            JArray rowAString = JsonConvert.DeserializeObject<JArray>(_listTable.Get("name", "departments"));

                            JArray listInfo = new JArray();
                            foreach (JObject departObj in rowAString)
                            {
                                listInfo = JsonConvert.DeserializeObject<JArray>(departObj["elements"]["unorder"].ToString());
                            }

                            JObject relObj = JsonConvert.DeserializeObject<JObject>(_objectTable.GetRow(ob["id"].ToString()));
                            bool findDep = false;
                            foreach (JObject depa in listInfo)
                            {
                                foreach (KeyValuePair<string, JToken> token in depa)
                                {
                                    if (token.Key == relObj["department"].ToString())
                                    {
                                        ob["department"] = token.Value.ToString();
                                        findDep = true;
                                    }
                                }
                                if (findDep) break;
                            }
                        }
                        catch { ob["department"] = ""; }
                    }

                    if (typeMovement == "movement" && temp == true)
                    {
                        string gerent=GetGerenteConjunto(idconjunto);
                        JObject gerenteobj=JsonConvert.DeserializeObject<JObject>(gerent);
                        obj["gerenteConjunto"] = gerenteobj["name"].ToString() + " " + gerenteobj["lastname"].ToString();
                    }
                }
                
                rowString = _userTable.GetRow(obj["Creator"].ToString());
                if (rowString != null)
                {
                    obj["Creator_id"] = obj["Creator"].ToString();
                    objs = JsonConvert.DeserializeObject<JObject>(rowString);
                    obj["Creator"] = objs["user"].ToString();
                    
                }

                if (obj["contador"] != null)
                {
                    JArray cont = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(obj["contador"]));
                    JObject contador = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(cont[0]));

                    if (contador["id_user"].ToString() == Session["_id"].ToString())
                    {
                        obj["iscontador"] = "true";
                    }
                    else { obj["iscontador"] = "false"; }
                }
                else { obj["iscontador"] = "false"; }

                demand = JsonConvert.SerializeObject(obj);
                //demand = demand.Replace("HTKField", "HTKFieldDetalles");
                return Json(demand);
                //return new HtmlString(locationsTrs);

            }
            else
            {
                return null;
            }
        }

        public JsonResult setAuthorizationTable(String iddemand)
        {
            JObject result = new JObject();
            String demandArray = _demandTable.GetRow(iddemand);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandArray);

            JArray autos = JsonConvert.DeserializeObject<JArray>(demand["authorizations"].ToString());

            string cadena = "";
            string objects1 = "";
            String cadarray = "";
            JObject obj1 = new JObject();
            JObject obj2 = new JObject();
            String motivo = "";
            foreach (JObject obj in autos)
            {
                if (obj["approved"].ToString() == "2")
                {
                    cadena = "No";
                    motivo = obj["note"].ToString();
                }
                if (obj["approved"].ToString() == "1") cadena = "Sí";

                cadarray = _userTable.GetRow(obj["id_user"].ToString());
                obj1 = JsonConvert.DeserializeObject<JObject>(cadarray);
                cadarray = _userProfileTable.GetRow(obj1["profileId"].ToString());
                obj2 = JsonConvert.DeserializeObject<JObject>(cadarray);
                objects1 = objects1 + "<tr><td><label id='profiledetail' >" + obj2["name"].ToString() + "</label></td><td><label id='userdetail' >" + obj1["user"].ToString() + "</label></td><td><label id='autodetail' >" + cadena + "</label></td></tr>";
                cadena = "";

            }
            result["tabla"] = objects1;
            result["motivo"] = motivo;
            return Json(JsonConvert.SerializeObject(result));
        }

        public string getNameMovement(string movementid)
        {
            String movementRow = _movementProfileTable.GetRow(movementid);
            JObject movementRowja = new JObject();
            string namemov = "";
            try
            {
                movementRowja = JsonConvert.DeserializeObject<JObject>(movementRow);
                namemov = movementRowja["name"].ToString();
            }
            catch (Exception ex)
            {

            }

            return namemov;

        }

        public string getNameLocation(string idlocation)
        {
            String movementRow = _locationTable.GetRow(idlocation);
            JObject movementRowja = new JObject();
            string namemov = "";
            try
            {
                movementRowja = JsonConvert.DeserializeObject<JObject>(movementRow);
                namemov = movementRowja["name"].ToString();
            }
            catch (Exception ex)
            {

            }

            return namemov;

        }

        public String getDemandApproved(String iddemand, string userid, string deniedObjects = "[]")
        {
            String demandArray = _demandTable.GetRow(iddemand);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandArray);
            JArray autos = JsonConvert.DeserializeObject<JArray>(demand["authorizations"].ToString());
            String UserRow = _userTable.GetRow(userid);
            JObject userRowja = JsonConvert.DeserializeObject<JObject>(UserRow);
            JArray deniedObjectsArray = JsonConvert.DeserializeObject<JArray>(deniedObjects);

            //Keep status as integer
            demand["status"] = Convert.ToInt16(demand["status"].ToString());

            string nameuser = "";
            try
            {
                nameuser = userRowja["lastname"].ToString() + " " + userRowja["name"].ToString();
            }
            catch (Exception ex) { }

            string namemov = "";
            try
            {
                namemov = getNameMovement(demand["movement"].ToString());
            }
            catch (Exception ex) { }
            JArray autos2 = new JArray();

            JArray objReferencias = JsonConvert.DeserializeObject<JArray>(demand["objects"].ToString());
            String foliodemand = demand["folio"].ToString();
            Dictionary<string, string> namerefdict = new Dictionary<string, string>();
            Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();
           
             try
            {
                List<string> listconjunt = (from refe in objReferencias select (string)refe["location"]).ToList();
                String resultref = _objectTable.getParents(listconjunt);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);
                
            }
            catch { }
            try
            {
                List<string> listactnames = (from refe in objReferencias select (string)refe["objectReference"]).ToList();
                String resultref = _objectTable.getCategory(listactnames);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

            }
            catch { }
            String moveArray = _profileTable.GetRow(demand["movement"].ToString());
            JObject move = JsonConvert.DeserializeObject<JObject>(moveArray);

            string typemove = move["typeMovement"].ToString();
            List<string> denyobjslist = new List<string>();
            List<dynamic> objsnote = new List<dynamic>();
            //Set the denied objects

            foreach (JObject dObject in deniedObjectsArray)
            {
                foreach (JObject obj in demand["objects"])
                {
                    //if (obj["denied_note"] == null)
                    //{
                        try
                        {
                            if (dObject["id"].ToString() == obj["id"].ToString() &&
                                dObject["orderNumber"].ToString() == obj["orderNumber"].ToString())
                            {
                                JObject dUser = new JObject();
                                dUser.Add("id", userid);
                                dUser.Add("name", nameuser);

                                obj["denied_note"] = dObject["deniedNote"];
                                obj["denied_user"] = dUser;
                                denyobjslist.Add(dObject["id"].ToString());
                                break;
                            }
                        }
                        catch (Exception e) { continue; }
                   // }
                }
            }

            int autori = 0;
            foreach (JObject obj in autos)
            {
                if (obj["approved"].ToString() == "1")
                {
                    autori++;
                }
                else if (obj["id_user"].ToString() == userid)
                {
                    obj["approved"] = "1";
                    obj["date"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    autori++;
                }

                autos2.Add(obj);
            }
            string usstring = _userTable.GetRow(demand["Creator"].ToString());
            JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

            string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

            String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
            StringBuilder act = new StringBuilder();
            string modelo = "";
            string marca = "";
            string idart = "";
            string serie = "";
            string nombre = "";
            string cantq = "1";
            string category = "";
            string deniednote="";
            string cantq2 = "";
            string conjd = "";
            string epc = "";
            string infotrans = "";
            bool denied=false;
            string ubicacion = "";
            //objReferencias = demand["objects"];
            foreach (JObject nr in demand["objects"])
            {
                JObject n;
                JToken nt2;
                if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                {
                    denied = false;
                    serie = nr["serie"].ToString();
                    cantq = nr["quantity"].ToString();
                    try
                    {
                        if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                            epc = nr["epc"].ToString();
                        else
                            epc = "No aplica";

                    }
                    catch (Exception ex)
                    {
                        epc = "No aplica";
                    }
                    if (typemove == "movement" && (move["temporal"].ToString() == "false" || move["temporal"].ToString() == "False"))
                    {
                        if (nr.TryGetValue("quantity_new", out nt2))
                            cantq2 = nr["quantity_new"].ToString();
                        if (nr.TryGetValue("conjuntoDestiny", out nt2))
                        {
                            conjd = getNameLocation(nr["conjuntoDestiny"].ToString());
                        }
                        infotrans = "<h5>Cantidad a mover: " + cantq2 + "</h5><h5>Conjunto destino: " + conjd + "</h5>";
                    }
                    if (n.TryGetValue("modelo", out nt2))
                        modelo = n["modelo"].ToString();
                    if (n.TryGetValue("marca", out nt2))
                        marca = n["marca"].ToString();
                    if (n.TryGetValue("object_id", out nt2))
                        idart = n["object_id"].ToString();
                    if (n.TryGetValue("name", out nt2))
                        nombre = n["name"].ToString();
                    if (n.TryGetValue("namecategory", out nt2))
                        category = n["namecategory"].ToString();

                    if (nr.TryGetValue("location", out nt2))
                    {
                        ubicacion = getNameLocation(nr["location"].ToString());
                    }
                    else { ubicacion = ""; }

                    if (nr.TryGetValue("denied_note", out nt2)){
                        deniednote = nr["denied_note"].ToString();
                        denied=true;
                    }

                    if (denied == true)
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;color:red;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5>" + infotrans + "<h5>Denegado por: " + deniednote + "</h5></div><br>");
                    }
                    else {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5>" + infotrans + "</div><br>");
                    }
                    
                }
            }
            string bodygeneric = creadorstring+"<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
            string customtitle = "";
            demand["authorizations"] = autos2;
            if (autori == autos.Count) demand["AuthorizedDate"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            String id = demand["_id"].ToString();
            demand.Remove("_id");
            _demandTable.SaveRow(JsonConvert.SerializeObject(demand), id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "Demand", DateTime.Now.ToString());
            classNotifications.saveNotification("Movements", "Authorize", "Se ha Aprobado la Solicitud de "+namemov+" con #folio: " + foliodemand);
            JArray recipients = new JArray();
            JArray attachments = new JArray();
            recipients.Add(demand["Creator"].ToString());

            if (denyobjslist.Count() > 0)
            {
                String gerenteconjunto = GetGerenteConjunto(namerefdict.First().Key);
                JObject gerent = JsonConvert.DeserializeObject<JObject>(gerenteconjunto);
                if (!recipients.ToString().Contains(gerent["_id"].ToString())) {
                    recipients.Add(gerent["_id"].ToString());
                }
            }
           
            string to = JsonConvert.SerializeObject(recipients);
            string attach = JsonConvert.SerializeObject(attachments);
            
            try
            {
                if (denyobjslist.Count() > 0)
                {
                    var listd = (from obj1 in deniedObjectsArray.Children() where denyobjslist.Contains(obj1["id"].ToString()) select new { id = (string)obj1["id"], name = (string)"", note = (string)obj1["deniedNote"] }).ToList();
                        
                    JArray listnameobj = new JArray();
                    foreach(var obj in denyobjslist)
                    {
                        String objectrow = "";
                        JObject objjo = new JObject();
                        if (typemove == "create")
                        {
                            objectrow = _objectReferenceTable.GetRow(obj);
                           objjo = JsonConvert.DeserializeObject<JObject>(objectrow);
                        }
                        else {
                            objectrow = _objectTable.GetRow(obj);
                            objjo = JsonConvert.DeserializeObject<JObject>(objectrow);
                        }
                        
                        try
                        {
                            foreach (var listob in listd) {

                                if (listob.id == obj)
                                {
                                    JObject nroj = new JObject();
                                    nroj.Add("id", listob.id);
                                    nroj.Add("note", listob.note);
                                    nroj.Add("name", objjo["name"].ToString());

                                    listnameobj.Add(nroj);

                                }
                            
                            }
                          


                        }
                        catch (Exception ex)
                        {
                            continue;
                        }      
                    }
                    string motivobj = "";
                    try
                    {
                        foreach (JObject motivs in listnameobj)
                        {
                            string namx = motivs["name"].ToString();
                            string notx = motivs["note"].ToString();
                            motivobj += "<h4>El Activo " + namx + ",Fue Rechazado por el siguiente Motivo:</h4><h5>" + notx+"</h5>";
                          
                        }
                    }
                    catch (Exception ex) { }
                     string nameslist = String.Join(",", listnameobj.Children()["name"].Values<string>().ToList());
                   
                    customtitle="<h4>Los Activos:" + nameslist + ", Fueron Rechazados Por: " + nameuser + ", En tu Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + "<h4>Motivos por Activo:</h4><div  style='border:#858691 dotted 2px;padding: 10px;'>" + motivobj+"</div><br>";
                    HtmlString body = new HtmlString(customtitle);

                    messagesC.SendMail(to, "Activos fueron Rechazados Por: " + nameuser,body.ToString() , attach, "Sistema");

                    //List<String> listemails = new List<String>();
                    //foreach (string recip in recipients)
                    //{
                    //    String em = GetCorreoUser(recip);
                    //    listemails.Add(em);
                    //}

                    //messagesC.SendExternalMail("", "Sistema", listemails, body.ToString(), attachments, "Activos fueron Rechazados Por: " + nameuser);

                }
            }
            catch (Exception ex)
            {

            }
            customtitle ="<h4>"+ nameuser + " autorizó la Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString()+"</h4>";
            HtmlString body1 = new HtmlString(customtitle+bodygeneric);

            messagesC.SendMail(to, "Movimiento " + namemov + " #" + demand["folio"].ToString() + " fue autorizada por " + nameuser, body1.ToString(), attach, "Sistema");
            List<String> listemails2 = new List<String>();
            //foreach (string recip in recipients)
            //{
            //    String em = GetCorreoUser(recip);
            //    listemails2.Add(em);
            //}

            //messagesC.SendExternalMail("", "Sistema", listemails2, body1.ToString(), attachments, "Movimiento " + namemov + " #" + demand["folio"].ToString() + " fue autorizada por " + nameuser);

            if (autori == autos.Count )
            {
                recipients.Clear();
                attachments.Clear();
             
                try
                {
                    recipients.Add(demand["Creator"].ToString());
                }
                catch (Exception ex)
                {
                    
                }
                int cant = 0;
                bool ok = false;
                int estatus = 0;
               
                if (demand["status"].ToString() == "3" && typemove != "delete" )
                {
                    int temp = 0;
                    if (typemove == "movement") {
                        try {
                            if (move["temporal"].ToString() == "true" || move["temporal"].ToString() == "True")
                            {
                                temp = 1;
                                UpdateStatusDemand(iddemand, 8);
                                recipients.Add(GetLogistica());
                                to = JsonConvert.SerializeObject(recipients);
                                attach = JsonConvert.SerializeObject(attachments);
                                customtitle = "<h4>Las autorizaciones de la Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + " han sido completadas.<br/></h4><br>";
                                HtmlString body = new HtmlString(customtitle + bodygeneric);

                                string autor = String.Join(",", from rec in recipients select (string)rec);
                                messagesC.SendMail(to, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ", body.ToString(), attach, "Sistema");

                                //List<String> listemails3 = new List<String>();
                                //foreach (string recip in recipients)
                                //{
                                //    String em = GetCorreoUser(recip);
                                //    listemails3.Add(em);
                                //}

                                //messagesC.SendExternalMail("", "Sistema", listemails3, body.ToString(), attachments, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ");

                            }
                        }
                        catch {
                            temp = 0;
                        }
                    }
                   
                    if (demand["approval"].ToString() == "[]" && temp==0)
                    {
                        UpdateStatusDemand(iddemand, 6);
                        estatus = 6;
                        if (demand["contador"].ToString() != "") {
                            foreach (var dd in demand["contador"])
                            {
                                try
                                {
                                    recipients.Add(dd["id_user"].ToString());
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }

                            }
                        }
                        if (typemove == "movement") { recipients.Add(GetLogistica()); }
                        to = JsonConvert.SerializeObject(recipients);
                        attach = JsonConvert.SerializeObject(attachments);
                       // List<string> conjuntosname = new List<string>();
                        customtitle = "<h4>Ha sido completada la solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + " </h4><br><h5>Completada por:" + Session["LoggedUser"].ToString() + "</h5>";
                        HtmlString body = new HtmlString(customtitle+bodygeneric);


                        messagesC.SendMail(to, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ", body.ToString(), attach, "Sistema");
                        //List<String> listemails3 = new List<String>();
                        //foreach (string recip in recipients)
                        //{
                        //    String em = GetCorreoUser(recip);
                        //    listemails3.Add(em);
                        //}

                        //messagesC.SendExternalMail("", "Sistema", listemails3, body.ToString(), attachments, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ");

                    }
                    else if(temp==0){
                        UpdateStatusDemand(iddemand, 5);
                        to = JsonConvert.SerializeObject(recipients);
                        attach = JsonConvert.SerializeObject(attachments);
                        customtitle = "<h4>Las autorizaciones de la Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + " han sido completadas.<br/></h4><br>";
                        HtmlString body = new HtmlString(customtitle+bodygeneric);

                        string autor = String.Join(",", from rec in recipients select (string)rec);
                        messagesC.SendMail(to, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ", body.ToString(), attach, "Sistema");

                        //List<String> listemails3 = new List<String>();
                        //foreach (string recip in recipients)
                        //{
                        //    String em = GetCorreoUser(recip);
                        //    listemails3.Add(em);
                        //}

                        //messagesC.SendExternalMail("", "Sistema", listemails3, body.ToString(), attachments, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ");


                        recipients.Clear();
                        attachments.Clear();
                        foreach (var dd in demand["approval"])
                        {
                            try
                            {
                                recipients.Add(dd["id_user"].ToString());
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }

                        }
                        //if (typemove == "movement") {
                        //    try {
                        //        recipients.Add(GetLogistica());
                                    
                        //    }
                        //    catch { 
                            
                        //    }
                            
                        //}

                       
                        
                        body1 = new HtmlString(customtitle + bodygeneric);
                        to = JsonConvert.SerializeObject(recipients);
                        attach = JsonConvert.SerializeObject(attachments);
                        messagesC.SendMail(to, "Movimiento  de " + namemov + " con Folio #" + demand["folio"].ToString() + ",esta Pendiente del Visto Bueno", body.ToString(), attach, "Sistema");

                        //List<String> listemails4 = new List<String>();
                        //foreach (string recip in recipients)
                        //{
                        //    String em = GetCorreoUser(recip);
                        //    listemails4.Add(em);
                        //}

                        //messagesC.SendExternalMail("", "Sistema", listemails4, body.ToString(), attachments, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + ",esta Pendiente del Visto Bueno");

                    }
                    
                }
                if (typemove == "delete")
                {
                    /*if (demand["destinyOptions"].ToString() != "robo" 
                        && demand["destinyOptions"].ToString() != "siniestro"
                        && demand["destinyOptions"].ToString() != "donacion"
                        && demand["destinyOptions"].ToString() != "venta")
                    {
                        UpdateStatusDemand(iddemand, 4);
                        to = JsonConvert.SerializeObject(recipients);
                        attach = JsonConvert.SerializeObject(attachments);
                        customtitle = "<h4>Todos los Autorizadores Aprobaron tu Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + ",En Espera de Comprobante</h4><br>";
                        HtmlString body = new HtmlString(customtitle + bodygeneric);

                        messagesC.SendMail(to, "Todos los Autorizadores Aprobaron tu Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString(), body.ToString(), attach, "Sistema");

                        return autori.ToString();
                    }
                    else
                    {*/
                    if (demand["approval"].ToString() == "[]")
                    {
                        UpdateStatusDemand(iddemand, 6); estatus = 6;
                        if (demand["contador"].ToString() != "")
                        {
                            foreach (var dd in demand["contador"])
                            {
                                try
                                {
                                    recipients.Add(dd["id_user"].ToString());
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }

                            }
                        }
                        if (typemove == "movement") { recipients.Add(GetLogistica()); }
                        to = JsonConvert.SerializeObject(recipients);
                        attach = JsonConvert.SerializeObject(attachments);
                        customtitle = "<h4>Las autorizaciones del movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " han sido completadas.<br/></h4><br>";
                        HtmlString body = new HtmlString(customtitle + bodygeneric);

                        messagesC.SendMail(to, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ", body.ToString(), attach, "Sistema");

                        //List<String> listemails4 = new List<String>();
                        //foreach (string recip in recipients)
                        //{
                        //    String em = GetCorreoUser(recip);
                        //    listemails4.Add(em);
                        //}

                        //messagesC.SendExternalMail("", "Sistema", listemails4, body.ToString(), attachments, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ");

                    }
                    else
                    {
                        if (demand["status"].ToString() == "3")
                        {
                            UpdateStatusDemand(iddemand, 9);
                            to = JsonConvert.SerializeObject(recipients); //USer to send the email information
                            attach = JsonConvert.SerializeObject(attachments); //File atachments
                            customtitle = "<h4>Las autorizaciones del movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " han sido completadas.<br/></h4><br>";
                            HtmlString body = new HtmlString(customtitle + bodygeneric);
                            messagesC.SendMail(to, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ", body.ToString(), attach, "Sistema");

                            //List<String> listemails = new List<String>();
                            //foreach (string recip in recipients)
                            //{
                            //    String em = GetCorreoUser(recip);
                            //    listemails.Add(em);
                            //}

                            //messagesC.SendExternalMail("", "Sistema", listemails, body.ToString(), attachments, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizada. ");


                            recipients.Clear();
                            attachments.Clear();
                            foreach (var dd in demand["contador"])
                            {
                                try
                                {
                                    recipients.Add(dd["id_user"].ToString());
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }

                            }
                            to = JsonConvert.SerializeObject(recipients);
                            attach = JsonConvert.SerializeObject(attachments);
                            customtitle = "<h4>Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + ",esta Pendiente  de Información Contable</h4><br>";
                            body = new HtmlString(customtitle + bodygeneric);
                            messagesC.SendMail(to, "Movimiento de " + namemov + " " + demand["destinyOptions"].ToString() + " con Folio #" + demand["folio"].ToString() + ",esta Pendiente de Información Contable", body.ToString(), attach, "Sistema");

                            //List<String> listemails4 = new List<String>();
                            //foreach (string recip in recipients)
                            //{
                            //    String em = GetCorreoUser(recip);
                            //    listemails4.Add(em);
                            //}

                            //messagesC.SendExternalMail("", "Sistema", listemails4, body.ToString(), attachments, "Movimiento de " + namemov + " " + demand["destinyOptions"].ToString() + " con Folio #" + demand["folio"].ToString() + ",esta Pendiente de Información Contable");

                        }
                        else if (demand["status"].ToString() == "9")
                        {
                            UpdateStatusDemand(iddemand, 4);
                            to = JsonConvert.SerializeObject(recipients); //USer to send the email information
                            attach = JsonConvert.SerializeObject(attachments); //File atachments
                            customtitle = "<h4>Contabilidad " + namemov + " con Folio #" + demand["folio"].ToString() + "</h4><br>";
                            HtmlString body = new HtmlString(customtitle + bodygeneric);
                            messagesC.SendMail(to, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizado. ", body.ToString(), attach, "Sistema");

                            //List<String> listemails4 = new List<String>();
                            //foreach (string recip in recipients)
                            //{
                            //    String em = GetCorreoUser(recip);
                            //    listemails4.Add(em);
                            //}

                            //messagesC.SendExternalMail("", "Sistema", listemails4, body.ToString(), attachments, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido autorizado. ");

                        }
                    }
                     //}
                }
                if (estatus == 6) {
                    if (typemove == "create")
                    {
                        string recepcion="";
                        string pedido="";
                        string solicitud = "";
                        string observaciones = "";
                        try {
                            recepcion = JsonConvert.DeserializeObject<JObject>(demand["profileFields"].ToString())["_HTKFieldno_recepcion"].ToString();
                        }
                        catch {
                            recepcion = "";
                        }
                        try
                        {
                            observaciones = JsonConvert.DeserializeObject<JObject>(demand["profileFields"].ToString())["_HTKFieldDescripcion"].ToString();
                        }
                        catch
                        {
                            observaciones = "";
                        }
                        try
                        {
                            pedido = JsonConvert.DeserializeObject<JObject>(demand["profileFields"].ToString())["_HTKFieldno_pedido"].ToString();
                        }
                        catch {
                            pedido = "";
                        }
                        try
                        {
                            solicitud = JsonConvert.DeserializeObject<JObject>(demand["profileFields"].ToString())["_HTKFieldno_solicitud"].ToString(); 
                        }
                        catch {
                            solicitud = "";
                        }
                        
                        foreach (JObject obj in objReferencias)
                        {
                            try {
                                if (obj["denied_note"].ToString() != "")
                                {
                                    continue;
                                }
                                
                            }
                            catch (Exception ex){
                                String objarray = _objectReferenceTable.GetRow(obj["id"].ToString());
                                JObject objref = JsonConvert.DeserializeObject<JObject>(objarray);
                                int.TryParse(obj["quantity"].ToString(), out cant);
                                string nameObj = objref["name"].ToString();
                                saveObjects(demand["Creator"].ToString(),obj["id"].ToString(), nameObj, obj["location"].ToString(), move["processes"].ToString(), obj["marca"].ToString(), obj["modelo"].ToString(), obj["object_id"].ToString(),obj["perfil"].ToString(), obj["serie"].ToString(), obj["price"].ToString(), obj["label"].ToString(), obj["date"].ToString(), obj["quantity"].ToString(), foliodemand, "", obj["department"].ToString(), recepcion, pedido, solicitud,observaciones);
                                ok = RulesChecker.isValidToLocation(obj["id"].ToString(), obj["location"].ToString());
                                if (ok == false)
                                {
                                    classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicacion no valida en la solicitud de "+namemov+" , #folio: " + foliodemand);
                                    // return "problem";
                                }
                            }
                           
                        }


                    }

                    bool temp = false;
                    try
                    {
                        bool.TryParse(move["temporal"].ToString(), out temp);

                    }
                    catch { temp = false; }

                    if (typemove == "movement" && temp==false)
                    {
                        string var1 = "1";
                        bool ok2 = false;
                        string pro1, pro2;
                        foreach (JObject obj in objReferencias)
                        {
                            try
                            {
                                if (obj["denied_note"].ToString() != "")
                                {
                                    continue;
                                }

                            }
                            catch (Exception ex) {
                                ok = RulesChecker.isValidToLocation(obj["id"].ToString(), obj["locationDestiny"].ToString());
                                if (ok == false)
                                {
                                    var1 = "0";
                                }
                                string larray = _locationTable.GetRow(obj["location"].ToString());
                                JObject lobj = JsonConvert.DeserializeObject<JObject>(larray);
                                try { pro1 = lobj["processId"].ToString(); }
                                catch { pro1 = ""; }

                                larray = _locationTable.GetRow(obj["locationDestiny"].ToString());
                                lobj = JsonConvert.DeserializeObject<JObject>(larray);
                                try { pro2 = lobj["processId"].ToString(); }
                                catch { pro2 = ""; }

                                if (pro1 != "" && pro2 != "")
                                {
                                    ok2 = RulesChecker.isValidProcessFlow(pro1, pro2);

                                    if (ok2 == false)
                                        classNotifications.saveNotification("Rules", "Invalid", "Flujo de procesos es invalido en la Solicitud de "+namemov+", #folio: " + foliodemand);

                                }
                            
                            }
                           
                        }

                        if (var1 == "0") ok = false;
                        if (ok == false)
                        {
                            classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicación no valida en la Solicitud de "+namemov+", #folio: " + foliodemand);
                            // return "problem";
                        }
                        SaveMovement(objReferencias, move["processes"].ToString());
                    }
                    if (typemove == "delete")
                    {
                        DeleteObjects(objReferencias, move["processes"].ToString());
                    }
                
                }
                

            }
            return autori.ToString();
        }

        public String getDemandApprovedGood(String iddemand, string userid, string assetTypeObjects = "[]", string valuebookObjects = "[]", string locationsObjects = "[]", string deniedObjects = "[]")
        {
            String demandArray = _demandTable.GetRow(iddemand);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandArray);
            JArray autos = JsonConvert.DeserializeObject<JArray>(demand["approval"].ToString());
            String UserRow = _userTable.GetRow(userid);
            JObject userRowja = JsonConvert.DeserializeObject<JObject>(UserRow);
            string nameuser = "";
            JArray assetTypes = JsonConvert.DeserializeObject<JArray>(assetTypeObjects);
            JArray valuebooks = JsonConvert.DeserializeObject<JArray>(valuebookObjects);
            JArray locations = JsonConvert.DeserializeObject<JArray>(locationsObjects);
            JArray deniedObjectsArray = JsonConvert.DeserializeObject<JArray>(deniedObjects);
            List<string> typeuserids = new List<string>();

            //Gets user name
            try
            {
                nameuser = userRowja["lastname"].ToString() + " " + userRowja["name"].ToString();
            }
            catch (Exception ex) { }

            //Set the denied objects
            List<string> denyobjslist = new List<string>();
            foreach (JObject dObject in deniedObjectsArray)
            {
                foreach (JObject obj in demand["objects"])
                {
                    if (obj["denied_note"] == null)
                    {
                        try
                        {
                            if (dObject["id"].ToString() == obj["id"].ToString() &&
                                dObject["orderNumber"].ToString() == obj["orderNumber"].ToString())
                            {
                                JObject dUser = new JObject();
                                dUser.Add("id", userid);
                                dUser.Add("name", nameuser);

                                obj["denied_note"] = dObject["deniedNote"];
                                obj["denied_user"] = dUser;
                                denyobjslist.Add(dObject["id"].ToString());

                                break;
                            }
                        }
                        catch (Exception e) { continue; }
                    }
                }
            }
            //-----

            //Keep status as integer
            demand["status"] = Convert.ToInt16(demand["status"].ToString());
            String moveArray = _profileTable.GetRow(demand["movement"].ToString());
            JObject move = JsonConvert.DeserializeObject<JObject>(moveArray);

            string typemove = move["typeMovement"].ToString();
            //Set the asset type to the objects
           List<string> nameprofile = new List<string>();
            if (typemove == "create") {
            foreach (JObject dObject in assetTypes)
            {
                foreach (JObject obj in demand["objects"])
                {
                    try
                    {
                        if (dObject["id"].ToString() == obj["id"].ToString() &&
                            dObject["orderNumber"].ToString() == obj["orderNumber"].ToString())
                        {
                            obj["assetType"] = dObject["assetType"];
                            break;
                        }
                    }
                    catch (Exception e) { }
                  
                }
            }
            nameprofile.Add("Contabilidad");
            }

            if (typemove == "delete") {
                foreach (JObject dObject in valuebooks)
                {
                    foreach (JObject obj in demand["objects"])
                    {
            try
            {
                            if (dObject["id"].ToString() == obj["id"].ToString() &&
                                dObject["orderNumber"].ToString() == obj["orderNumber"].ToString())
                            {
                                //obj["value_book"] = dObject["value"];
                                break;
                            }
                        }
                        catch (Exception e) { }
                    }
                }

                nameprofile.Add("Contabilidad");
            }

            bool temp = false;
            try
            {
                bool.TryParse(move["temporal"].ToString(), out temp);

            }
            catch { temp = false; }

            if (typemove == "movement" && temp==false)
            {
                foreach (JObject dObject in locations)
                {
                    foreach (JObject obj in demand["objects"])
                    {
                        try
                        {
                            if (dObject["id"].ToString() == obj["id"].ToString() &&
                                dObject["orderNumber"].ToString() == obj["orderNumber"].ToString())
                            {
                                obj["quantity_new"] = dObject["quantity_new"];
                                obj["locationDestiny"] = dObject["location_destiny"];
                                break;
                            }
                        }
                        catch (Exception e) { }
                    }
                }
                nameprofile.Add("Contabilidad");
                nameprofile.Add("Logística");
            }

            string namemov = "";
            try
            {
                namemov = getNameMovement(demand["movement"].ToString());
            }
            catch (Exception ex) { }
            try
            {

                JArray profile = JsonConvert.DeserializeObject<JArray>(_userProfileTable.getProfiles(nameprofile));
                List<string> profilesids = new List<string>();
                profilesids = (from profileid in profile select (string)profileid["_id"]).ToList();
                JArray users = JsonConvert.DeserializeObject<JArray>(_userTable.getUsersByProfile(profilesids));
                typeuserids = (from usersr in users select (string)usersr["_id"]).ToList();
            }
            catch
            {

            }
            JArray autos2 = new JArray();

            JArray objReferencias = JsonConvert.DeserializeObject<JArray>(demand["objects"].ToString());
            String foliodemand = demand["folio"].ToString();
            Dictionary<string, string> namerefdict = new Dictionary<string, string>();
            Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

            try
            {
                List<string> listconjunt = (from refe in objReferencias select (string)refe["location"]).ToList();
                String resultref = _objectTable.getParents(listconjunt);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

            }
            catch { }
            try
            {
                List<string> listactnames = (from refe in objReferencias select (string)refe["objectReference"]).ToList();
                String resultref = _objectTable.getCategory(listactnames);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
               
                activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

            }
            catch { }

            int autori = 0;
            foreach (JObject obj in autos)
            {
                if (obj["approved"].ToString() == "1")
                {
                    autori++;
                }
                else if (obj["id_user"].ToString() == userid)
                {
                    obj["approved"] = "1";
                    obj["date"]=DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") ;
                    autori++;
                }

                autos2.Add(obj);
            }
            string usstring = _userTable.GetRow(demand["Creator"].ToString());
            JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

            string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

            String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
            StringBuilder act = new StringBuilder();
            string modelo = "";
            string marca = "";
            string idart = "";
            string serie = "";
            string nombre = "";
            string cantq = "1";
            string category = "";
            string deniednote = "";
            string cantq2 = "";
            string conjd = "";
            string locn = "";
            string infotrans = "";
            string epc = "";
            bool denied = false;
            string ubicacion = "";
            foreach (JObject nr in objReferencias)
            {
                JObject n;
                JToken nt2;
                denied = false;
                if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                {
                    serie = nr["serie"].ToString();
                    cantq = nr["quantity"].ToString();
                    try
                    {
                        if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                            epc = nr["epc"].ToString();
                        else
                            epc = "No aplica";

                    }
                    catch (Exception ex)
                    {
                        epc = "No aplica";
                    }
                    if (typemove == "movement" && (move["temporal"].ToString() == "false" || move["temporal"].ToString() == "False"))
                    {
                        if (nr.TryGetValue("quantity_new", out nt2))
                            cantq2 = nr["quantity_new"].ToString();
                        if (nr.TryGetValue("conjuntoDestiny", out nt2))
                        {
                            conjd = getNameLocation(nr["conjuntoDestiny"].ToString());
                        }
                        if (nr.TryGetValue("locationDestiny", out nt2))
                        {
                            locn = getNameLocation(nr["locationDestiny"].ToString());
                        }

                        infotrans = "<h5>Cantidad a mover: " + cantq2 + "</h5><h5>Conjunto destino: " + conjd + "</h5><h5>Ubicacion destino: " + locn + "</h5>";
                    }
                    if (n.TryGetValue("modelo", out nt2))
                        modelo = n["modelo"].ToString();
                    if (n.TryGetValue("marca", out nt2))
                        marca = n["marca"].ToString();
                    if (n.TryGetValue("object_id", out nt2))
                        idart = n["object_id"].ToString();
                    if (n.TryGetValue("name", out nt2))
                        nombre = n["name"].ToString();
                    if (n.TryGetValue("namecategory", out nt2))
                        category = n["namecategory"].ToString();
                    if (nr.TryGetValue("location", out nt2))
                    {
                        ubicacion = getNameLocation(nr["location"].ToString());
                    }
                    else { ubicacion = ""; }
                    if (nr.TryGetValue("denied_note", out nt2))
                    {
                        deniednote = nr["denied_note"].ToString();
                        denied = true;
                    }

                    if (denied == true)
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;color:red;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5>" + infotrans + "<h5>Denegado por: " + deniednote + "</h5></div><br>");
                    }
                    else
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5>" + infotrans + "</div><br>");
                    }
                  //  act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Id Activo: " + idart + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5></div><br>");
                }
            }
            string bodygeneric = creadorstring+"<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
            string customtitle = "";
            demand["approval"] = autos2;
            if (autori == autos.Count) demand["ApprovedDate"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            String id = demand["_id"].ToString();
            demand.Remove("_id");
            _demandTable.SaveRow(JsonConvert.SerializeObject(demand), id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "Demand", DateTime.Now.ToString());
            classNotifications.saveNotification("Movements", "Authorize", "Se ha dado visto bueno al movimiento de "+namemov+"con #folio: " + foliodemand);
            JArray recipients = new JArray();
            JArray attachments = new JArray();
            recipients.Add(demand["Creator"].ToString());

            try
            {
                String gerenteconjunto = GetGerenteConjunto(namerefdict.First().Key);
                JObject gerent = JsonConvert.DeserializeObject<JObject>(gerenteconjunto);
                if (!recipients.ToString().Contains(gerent["_id"].ToString()))
                {
                    recipients.Add(gerent["_id"].ToString());
                }
            }
            catch { 
            
            }
            

            string to = JsonConvert.SerializeObject(recipients);
            string attach = JsonConvert.SerializeObject(attachments);
            
            //enviar mensaje de rechazados
            try
            {
                if (denyobjslist.Count() > 0)
                {
                    var listd = (from obj1 in deniedObjectsArray.Children() where denyobjslist.Contains(obj1["id"].ToString()) select new { id = (string)obj1["id"], name = (string)"", note = (string)obj1["deniedNote"] }).ToList();

                    JArray listnameobj = new JArray();
                    foreach (var obj in denyobjslist)
                    {
                        String objectrow = "";
                        JObject objjo = new JObject();
                        if (typemove == "create")
                        {
                            objectrow = _objectReferenceTable.GetRow(obj);
                            objjo = JsonConvert.DeserializeObject<JObject>(objectrow);
                        }
                        else
                        {
                            objectrow = _objectTable.GetRow(obj);
                            objjo = JsonConvert.DeserializeObject<JObject>(objectrow);
                        }
                        try
                        {
                            foreach (var listob in listd)
                            {

                                if (listob.id == obj)
                                {
                                    JObject nroj = new JObject();
                                    nroj.Add("id", listob.id);
                                    nroj.Add("note", listob.note);
                                    nroj.Add("name", objjo["name"].ToString());

                                    listnameobj.Add(nroj);

                                }

                            }



                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    string motivobj = "";
                    try
                    {
                        foreach (JObject motivs in listnameobj)
                        {
                            string namx = motivs["name"].ToString();
                            string notx = motivs["note"].ToString();
                            motivobj += "<h4>El Activo " + namx + ",Fue Rechazado por el siguiente Motivo:</h4><h5>" + notx + "</h5>";

                        }
                    }
                    catch (Exception ex) { }
                    string nameslist = String.Join(",", listnameobj.Children()["name"].Values<string>().ToList());

                    customtitle = "<h4>Los Activos:" + nameslist + ", Fueron Rechazados Por: " + nameuser + ", En tu Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + "<h4>Motivos por Activo:</h4><div  style='border:#858691 dotted 2px;padding: 10px;'>" + motivobj + "</div><br>";
                    HtmlString body2 = new HtmlString(customtitle);

                    messagesC.SendMail(to, "Los Activos Fueron Rechazados Por: " + nameuser, body2.ToString(), attach, "Sistema");

                    //List<String> listemails4 = new List<String>();
                    //foreach (string recip in recipients)
                    //{
                    //    String em = GetCorreoUser(recip);
                    //    listemails4.Add(em);
                    //}

                    //messagesC.SendExternalMail("", "Sistema", listemails4, body2.ToString(), attachments, "Los Activos Fueron Rechazados Por: " + nameuser);

                }
            }
            catch (Exception ex)
            {

            }



            customtitle ="<h4>"+ nameuser + " dio Visto Bueno al movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + "</h4><br>";
            HtmlString body = new HtmlString(customtitle + bodygeneric);
            messagesC.SendMail(to, nameuser + " dio Visto Bueno al movimiento de " + namemov + " con Folio #" + demand["folio"].ToString(), body.ToString(), attach, "Sistema");

            //List<String> listemails5 = new List<String>();
            //foreach (string recip in recipients)
            //{
            //    String em = GetCorreoUser(recip);
            //    listemails5.Add(em);
            //}

            //messagesC.SendExternalMail("", "Sistema", listemails5, body.ToString(), attachments, nameuser + " dio Visto Bueno al movimiento de " + namemov + " con Folio #" + demand["folio"].ToString());


            bool temp1 = false;
            try
            {
                bool.TryParse(move["temporal"].ToString(), out temp1);

            }
            catch { temp1 = false; }
            if (autori == autos.Count)
            {
                recipients.Clear();
                attachments.Clear();
                foreach (var autor in demand["approval"])
                {
                    try
                    {
                        recipients.Add(autor["id_user"].ToString());
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
                int cant = 0;
                bool ok = false;
                int estatus = 0;
                
                if (demand["status"].ToString() == "8")
                {
                    estatus = 8;
                }
                else
                {
                    UpdateStatusDemand(iddemand, 6);
                    estatus = 6;
                }
                    foreach (string useridx in typeuserids)
                    {
                        recipients.Add(useridx);
                    }
                    if (demand["contador"].ToString() != "")
                    {
                        foreach (var dd in demand["contador"])
                        {
                            try
                            {
                                recipients.Add(dd["id_user"].ToString());
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }

                        }
                    }
                    if (typemove == "movement") { recipients.Add(GetLogistica()); }
                    to = JsonConvert.SerializeObject(recipients);
                    attach = JsonConvert.SerializeObject(attachments);
                    customtitle = "<h4>Se ha dado el Visto Bueno al movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + "</h4><br>";
                    body = new HtmlString(customtitle + bodygeneric);

                    messagesC.SendMail(to, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString()+" ha sido aprobado.", body.ToString(), attach, "Sistema");

                    //List<String> listemails6 = new List<String>();
                    //foreach (string recip in recipients)
                    //{
                    //    String em = GetCorreoUser(recip);
                    //    listemails6.Add(em);
                    //}

                    //messagesC.SendExternalMail("", "Sistema", listemails6, body.ToString(), attachments, "Movimiento de " + namemov + " con Folio #" + demand["folio"].ToString() + " ha sido aprobado.");

              
                if (estatus == 6)
                {
                    if (typemove == "create")
                    {
                        string recepcion = "";
                        string pedido = "";
                        string solicitud = "";
                        string observaciones = "";
                        try
                        {
                            observaciones = JsonConvert.DeserializeObject<JObject>(demand["profileFields"].ToString())["_HTKFieldDescripcion"].ToString();
                        }
                        catch
                        {
                            observaciones = "";
                        }
                        try
                        {
                            recepcion = JsonConvert.DeserializeObject<JObject>(demand["profileFields"].ToString())["_HTKFieldno_recepcion"].ToString();
                        }
                        catch
                        {
                            recepcion = "";
                        }
                        try
                        {
                            pedido = JsonConvert.DeserializeObject<JObject>(demand["profileFields"].ToString())["_HTKFieldno_pedido"].ToString();
                        }
                        catch
                        {
                            pedido = "";
                        }
                        try
                        {
                            solicitud = JsonConvert.DeserializeObject<JObject>(demand["profileFields"].ToString())["_HTKFieldno_solicitud"].ToString();
                        }
                        catch
                        {
                            solicitud = "";
                        }

                        foreach (JObject obj in objReferencias)
                        {
                             try {
                                if (obj["denied_note"].ToString() != "")
                                {
                                    continue;
                                }
                                
                            }catch(Exception ex){
                                String objarray = _objectReferenceTable.GetRow(obj["id"].ToString());
                                JObject objref = JsonConvert.DeserializeObject<JObject>(objarray);
                                int.TryParse(obj["quantity"].ToString(), out cant);
                                string nameObj = objref["name"].ToString();
                                saveObjects(demand["Creator"].ToString(),obj["id"].ToString(), nameObj, obj["location"].ToString(),  move["processes"].ToString(),obj["marca"].ToString(),obj["modelo"].ToString(),obj["object_id"].ToString(),obj["perfil"].ToString(), obj["serie"].ToString(), obj["price"].ToString(), obj["label"].ToString(), obj["date"].ToString(),obj["quantity"].ToString(), foliodemand, obj["assetType"].ToString(),obj["department"].ToString(),recepcion,pedido,solicitud,observaciones);
                                ok = RulesChecker.isValidToLocation(obj["id"].ToString(), obj["location"].ToString());
                                if (ok == false)
                                {
                                    classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicacion no valida en el movimiento de "+namemov+", #folio: " + foliodemand);
                                    // return "problem";
                                }
                            }
                            
                        }


                    }

                    

                    if (typemove == "movement" && temp==false)
                    {
                        string var1 = "1";
                        bool ok2 = false;
                        string pro1, pro2;
                        JArray objReferencias2 = new JArray();
                        foreach (JObject obj in objReferencias)
                        {
                            try
                            {
                                if (obj["denied_note"].ToString() != "")
                                {
                                    continue;
                                }

                            }
                            catch (Exception ex)
                            {
                                objReferencias2.Add(obj);
                                ok = RulesChecker.isValidToLocation(obj["id"].ToString(), obj["locationDestiny"].ToString());
                                if (ok == false)
                                {
                                    var1 = "0";
                                }
                                string larray = _locationTable.GetRow(obj["location"].ToString());
                                JObject lobj = JsonConvert.DeserializeObject<JObject>(larray);
                                try { pro1 = lobj["processId"].ToString(); }
                                catch { pro1 = ""; }

                                larray = _locationTable.GetRow(obj["locationDestiny"].ToString());
                                lobj = JsonConvert.DeserializeObject<JObject>(larray);
                                try { pro2 = lobj["processId"].ToString(); }
                                catch { pro2 = ""; }

                                if (pro1 != "" && pro2 != "")
                                {
                                    ok2 = RulesChecker.isValidProcessFlow(pro1, pro2);

                                    if (ok2 == false)
                                        classNotifications.saveNotification("Rules", "Invalid", "Flujo de procesos es invalido en el movimiento de "+namemov+", #folio: " + foliodemand);

                                }
                            }
                        }

                        if (var1 == "0") ok = false;
                        if (ok == false)
                        {
                            classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicación no valida en el movimiento de "+namemov+", #folio: " + foliodemand);
                            // return "problem";
                        }
                        SaveMovement(objReferencias2, move["processes"].ToString());
                    }

                    if (typemove == "movement" && temp == true)
                    {
                        UpdateTempMovement(id);
                    }

                    if (typemove == "delete")
                    {
                        DeleteObjects(objReferencias, move["processes"].ToString());
                    }

                }
                if (estatus == 8)
                {

                    if (typemove == "movement" && temp == false)
                        {
                            string var1 = "1";
                            bool ok2 = false;
                            string pro1, pro2;
                            JArray newObjects = new JArray();
                            List<string> newact = new List<string>();
                            try
                            {
                                newObjects = JsonConvert.DeserializeObject<JArray>(demand["updateInfo"]["newObjects"].ToString());

                }
                            catch (Exception exx)
                            {


                            }

                            JArray objReferencias2 = new JArray();
                            foreach (JObject obj in objReferencias)
                            {
                                try
                                {
                                    if (obj["denied_note"].ToString() != "")
                                    {
                                        continue;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    objReferencias2.Add(obj);
                                    ok = RulesChecker.isValidToLocation(obj["id"].ToString(), obj["locationDestiny"].ToString());
                                    if (ok == false)
                                    {
                                        var1 = "0";
                                    }
                                    string larray = _locationTable.GetRow(obj["location"].ToString());
                                    JObject lobj = JsonConvert.DeserializeObject<JObject>(larray);
                                    try { pro1 = lobj["processId"].ToString(); }
                                    catch { pro1 = ""; }

                                    larray = _locationTable.GetRow(obj["locationDestiny"].ToString());
                                    lobj = JsonConvert.DeserializeObject<JObject>(larray);
                                    try { pro2 = lobj["processId"].ToString(); }
                                    catch { pro2 = ""; }

                                    if (pro1 != "" && pro2 != "")
                                    {
                                        ok2 = RulesChecker.isValidProcessFlow(pro1, pro2);

                                        if (ok2 == false)
                                            classNotifications.saveNotification("Rules", "Invalid", "Flujo de procesos es invalido en el movimiento de " + namemov + ", #folio: " + foliodemand);

                                    }

                                    foreach (var news in newObjects)
                                    {
                                        try
                                        {
                                            if (obj["id"].ToString() == news["id"].ToString())
                                            {
                                                JToken valtok = "";
                                                if (obj.TryGetValue("marca", out valtok))
                                                    obj["marca"] = news["marca"].ToString();
                                                else
                                                    obj.Add("marca", news["marca"]);

                                                if (obj.TryGetValue("modelo", out valtok))
                                                    obj["modelo"] = news["modelo"].ToString();
                                                else
                                                    obj.Add("modelo", news["modelo"]);

                                                if (obj.TryGetValue("epc", out valtok))
                                                    obj["epc"] = news["epc"].ToString();
                                                else
                                                    obj.Add("epc", news["epc"]);
                                                if (obj.TryGetValue("serie", out valtok))
                                                    obj["serie"] = news["serie"].ToString();
                                                else
                                                    obj.Add("serie", news["serie"]);

                                                if (obj.TryGetValue("descripcion", out valtok))
                                                    obj["descripcion"] = news["descripcion"].ToString();
                                                else
                                                    obj.Add("descripcion", news["descripcion"]);

                                                newact.Add(obj["id"].ToString());
                                            }
                                        }
                                        catch (Exception exc) { continue; }
                                    }
                                }
                            }

                            if (var1 == "0") ok = false;
                            if (ok == false)
                            {
                                classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicación no valida en la Solicitud de " + namemov + ", #folio: " + foliodemand);
                                // return "problem";
                            }
                            SaveMovement(objReferencias2, move["processes"].ToString(),newact);
                        }
                    
                }


            }
            return autori.ToString();
        
        }

        public string updateContableDemand(string demandid, string jsonData) {

            try
            {
                JObject demand = JsonConvert.DeserializeObject<JObject>(_demandTable.GetRow(demandid));
                JArray datas = JsonConvert.DeserializeObject<JArray>(jsonData);
                foreach (JObject ob in demand["objects"])
                {
                    foreach (JObject data in datas)
                    {
                        if (data["id"].ToString() == ob["id"].ToString())
                        {
                            ob["propietario"] = data["propietario"].ToString();
                            ob["value_book"] = data["libros"].ToString();
                            if (data["depositAccount"] != null) {
                                ob["deposit_account"] = data["depositAccount"].ToString();
                            }
                            break;
                        }
                    }
                }

                foreach (JObject cont in demand["contador"]) { cont["approved"] = "1"; }

                _demandTable.SaveRow(JsonConvert.SerializeObject(demand), demandid);
                _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "Demand", DateTime.Now.ToString());
                JArray recipients = new JArray();
                JArray attachments = new JArray();
                try
                {
                    recipients.Add(demand["Creator"].ToString());
                }
                catch { }

                if (demand["status"].ToString() == "9")
                {
                    UpdateStatusDemand(demandid, 4);
                    string to = JsonConvert.SerializeObject(recipients); //USer to send the email information
                    string attach = JsonConvert.SerializeObject(attachments); //File atachments

                    JArray objReferencias = JsonConvert.DeserializeObject<JArray>(demand["objects"].ToString());
                    Dictionary<string, string> namerefdict = new Dictionary<string, string>();
                    Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

                    try
                    {
                        List<string> listconjunt = (from refe in objReferencias select (string)refe["location"]).ToList();
                        String resultref = _objectTable.getParents(listconjunt);
                        JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                        namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

                    }
                    catch { }
                    try
                    {
                        List<string> listactnames = (from refe in objReferencias select (string)refe["objectReference"]).ToList();
                        String resultref = _objectTable.getCategory(listactnames);
                        JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                        activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

                    }
                    catch { }
                    string usstring = _userTable.GetRow(demand["Creator"].ToString());
                    JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

                    string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

                    String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
                    StringBuilder act = new StringBuilder();
                    string modelo = "";
                    string marca = "";
                    string idart = "";
                    string serie = "";
                    string nombre = "";
                    string cantq = "1";
                    string category = "";
                    string deniednote = "";
                    string sold_price="";
                    string buyer="";
                    string propietario = "";
                    string value_book= "";
                    string deposit_account ="";
                    bool denied = false;
                    string epc = "";
                    string ubicacion = "";
                    foreach (JObject nr in objReferencias)
                    {
                        JObject n;
                        JToken nt2;
                        denied = false;
                        if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                        {
                            serie = nr["serie"].ToString();
                            cantq = nr["quantity"].ToString();
                            try
                            {
                                if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                                    epc = nr["epc"].ToString();
                                else
                                    epc = "No aplica";

                            }
                            catch (Exception ex)
                            {
                                epc = "No aplica";
                            }
                            if (n.TryGetValue("modelo", out nt2))
                                modelo = n["modelo"].ToString();
                            if (n.TryGetValue("marca", out nt2))
                                marca = n["marca"].ToString();
                            if (n.TryGetValue("object_id", out nt2))
                                idart = n["object_id"].ToString();
                            if (n.TryGetValue("name", out nt2))
                                nombre = n["name"].ToString();
                            if (n.TryGetValue("namecategory", out nt2))
                                category = n["namecategory"].ToString();

                            if (nr.TryGetValue("location", out nt2))
                            {
                                ubicacion = getNameLocation(nr["location"].ToString());
                            }
                            else { ubicacion = ""; }

                            if (nr.TryGetValue("denied_note", out nt2))
                            {
                                deniednote = nr["denied_note"].ToString();
                                denied = true;
                            }

                            if (nr.TryGetValue("sold_price", out nt2))
                            {
                                sold_price = nr["sold_price"].ToString();
                            }
                            if (nr.TryGetValue("buyer", out nt2))
                            {
                                buyer = nr["buyer"].ToString();
                            }
                            if (nr.TryGetValue("propietario", out nt2))
                            {
                                propietario = nr["propietario"].ToString();
                            }
                            if (nr.TryGetValue("value_book", out nt2))
                            {
                                value_book = nr["value_book"].ToString();
                            }
                            if (nr.TryGetValue("deposit_account", out nt2))
                            {
                                deposit_account = nr["deposit_account"].ToString();
                            }

                            if (denied == true)
                            {
                                act.Append("<div style='border:#858691 dotted 2px;padding: 10px;color:red;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>modelo: " + modelo + "</h5><h5>EPC: " + epc + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5><h5>Denegado por: " + deniednote + "</h5></div><br>");
                            }
                            else
                            {
                                act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5><h5>Precio en venta: " + sold_price + "</h5><h5>Comprador: " + buyer + "</h5><h5>Propietario: " + propietario + "</h5><h5>Valor en libros: " + value_book + "</h5><h5>Cuenta de deposito: " + deposit_account + "</h5></div><br>");
                            }

                        }
                    }
                    string bodygeneric = creadorstring+"<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
                    string customtitle = "<h4>Datos asignados por contabilidad a la solicitud con Folio #" + demand["folio"].ToString() + "</h4><br>";
                    HtmlString body = new HtmlString(customtitle + bodygeneric);

                    messagesC.SendMail(to, "Datos asignados por contabilidad a la solicitud con Folio #" + demand["folio"].ToString(), customtitle + bodygeneric, attach, "Sistema");

                    //List<String> listemails6 = new List<String>();
                    //foreach (string recip in recipients)
                    //{
                    //    String em = GetCorreoUser(recip);
                    //    listemails6.Add(em);
                    //}

                    //messagesC.SendExternalMail("", "Sistema", listemails6, customtitle + bodygeneric, attachments, "Datos asignados por contabilidad a la solicitud con Folio #" + demand["folio"].ToString());


                }
                return "success";
            }
            catch {
                return "error";
            }

        }

        public String getDemandAdjudicated(String iddemand, string userid, string deniedObjects = "[]")
        {
            String demandArray = _demandTable.GetRow(iddemand);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandArray);
            JArray deniedObjectsArray = JsonConvert.DeserializeObject<JArray>(deniedObjects);

            String foliodemand = demand["folio"].ToString();
            String id = demand["_id"].ToString();
            demand["dctFolio"] = generateDCTFolio(id);
            demand["dctDate"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            demand.Remove("_id");

            //Set the denied objects
            JObject userInfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(userid));
            string nameuser = userInfo["lastname"].ToString() + " "+ userInfo["name"].ToString();
            List<string> denyobjslist = new List<string>();
            String idlocation = "";

            foreach (JObject dObject in deniedObjectsArray)
            {
                foreach (JObject obj in demand["objects"])
                {
                    try
                    {
                        if (dObject["id"].ToString() == obj["id"].ToString() &&
                            dObject["orderNumber"].ToString() == obj["orderNumber"].ToString())
                        {
                            JObject dUser = new JObject();
                            dUser.Add("id", userid);
                            dUser.Add("name", nameuser);

                            obj["denied_note"] = dObject["deniedNote"];
                            obj["denied_user"] = dUser;
                            denyobjslist.Add(dObject["id"].ToString());
                            idlocation = obj["location"].ToString();
                            break;
                        }
                    }
                    catch (Exception e) { continue; }
                }
            }

            //Keep status as integer
            demand["status"] = Convert.ToInt16(demand["status"].ToString());

            _demandTable.SaveRow(JsonConvert.SerializeObject(demand), id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "Demand", DateTime.Now.ToString());
            //Get Deny objects details
            string motivobj = "";
            try
            {
                if (denyobjslist.Count() > 0)
                {
                    var listd = (from obj1 in deniedObjectsArray.Children() where denyobjslist.Contains(obj1["id"].ToString()) select new { id = (string)obj1["id"], name = (string)"", note = (string)obj1["deniedNote"] }).ToList();

                    JArray listnameobj = new JArray();
                    foreach (var obj in denyobjslist)
                    {
                        String objectrow = _objectTable.GetRow(obj);
                        JObject objjo = JsonConvert.DeserializeObject<JObject>(objectrow);
                        try
                        {
                            foreach (var listob in listd)
                            {

                                if (listob.id == obj)
                                {
                                    JObject nroj = new JObject();
                                    nroj.Add("id", listob.id);
                                    nroj.Add("note", listob.note);
                                    nroj.Add("name", objjo["name"].ToString());
                                    listnameobj.Add(nroj);
                                }
                            }
                        }
                        catch (Exception ex) { }
                    }
                    try
                    {
                        foreach (JObject motivs in listnameobj)
                        {
                            string namx = motivs["name"].ToString();
                            string notx = motivs["note"].ToString();
                            motivobj += "<br><h4>El Activo " + namx + ", Fue Rechazado por el siguiente Motivo:</h4><h5>" + notx + "</h5>";
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            catch { }

            string namemov = "";
            try
            {
                namemov = getNameMovement(demand["movement"].ToString());
            }
            catch (Exception ex) { }
            JArray recipients = new JArray();
            JArray attachments = new JArray();
            recipients.Add(demand["Creator"].ToString());
            try {
                String gerenteconjunto = GetGerenteConjunto(idlocation);
                JObject gerent = JsonConvert.DeserializeObject<JObject>(gerenteconjunto);
                if (!recipients.ToString().Contains(gerent["_id"].ToString()))
                {
                    recipients.Add(gerent["_id"].ToString());
                }
            }
            catch {
            }
            JArray objReferencias = JsonConvert.DeserializeObject<JArray>(demand["objects"].ToString());
            Dictionary<string, string> namerefdict = new Dictionary<string, string>();
            Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

            try
            {
                List<string> listconjunt = (from refe in objReferencias select (string)refe["location"]).ToList();
                String resultref = _objectTable.getParents(listconjunt);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

            }
            catch { }
            try
            {
                List<string> listactnames = (from refe in objReferencias select (string)refe["objectReference"]).ToList();
                String resultref = _objectTable.getCategory(listactnames);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

            }
            catch { }
            string usstring = _userTable.GetRow(demand["Creator"].ToString());
            JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

            string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

            String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
            StringBuilder act = new StringBuilder();
            string modelo = "";
            string marca = "";
            string idart = "";
            string serie = "";
            string nombre = "";
            string cantq = "1";
            string category = "";
            string deniednote = "";
            string epc = "";
            bool denied = false;
            string ubicacion = "";
            foreach (JObject nr in objReferencias)
            {
                JObject n;
                JToken nt2;
                denied = false;
                if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                {
                    serie = nr["serie"].ToString();
                    cantq = nr["quantity"].ToString();
                    try
                    {
                        if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                            epc = nr["epc"].ToString();
                        else
                            epc = "No aplica";

                    }
                    catch (Exception ex)
                    {
                        epc = "No aplica";
                    }
                    if (n.TryGetValue("modelo", out nt2))
                        modelo = n["modelo"].ToString();
                    if (n.TryGetValue("marca", out nt2))
                        marca = n["marca"].ToString();
                    if (n.TryGetValue("object_id", out nt2))
                        idart = n["object_id"].ToString();
                    if (n.TryGetValue("name", out nt2))
                        nombre = n["name"].ToString();
                    if (n.TryGetValue("namecategory", out nt2))
                        category = n["namecategory"].ToString();

                    if (nr.TryGetValue("location", out nt2))
                    {
                        ubicacion = getNameLocation(nr["location"].ToString());
                    }
                    else { ubicacion = ""; }

                    if (nr.TryGetValue("denied_note", out nt2))
                    {
                        deniednote = nr["denied_note"].ToString();
                        denied = true;
                    }

                    if (denied == true)
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;color:red;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5><h5>Denegado por: " + deniednote + "</h5></div><br>");
                    }
                    else
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5></div><br>");
                    }

                }
            }
            string bodygeneric = creadorstring+"<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
            string customtitle = "<h4>El Dictamen en la Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + ",Ha sido Generado por " + nameuser + "</h4><h4>Motivo: " + motivobj + "</h4><br>";
            HtmlString body = new HtmlString(customtitle + bodygeneric);
            
            string to = JsonConvert.SerializeObject(recipients);
            string attach = JsonConvert.SerializeObject(attachments);
            messagesC.SendMail(to, "El Dictamen en la Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString(), customtitle + bodygeneric, attach, "Sistema");

            //List<String> listemails6 = new List<String>();
            //foreach (string recip in recipients)
            //{
            //    String em = GetCorreoUser(recip);
            //    listemails6.Add(em);
            //}

            //messagesC.SendExternalMail("", "Sistema", listemails6, customtitle + bodygeneric, attachments, "El Dictamen en la Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString());


            if (demand["deleteType"].ToString() == "no_planeada") {
                UpdateStatusDemand(iddemand, 3);
                return generatePDF2(id);
            } 
            UpdateStatusDemand(iddemand, 2);
            return generatePDF(id);
        }

        public String getDemandAdjudicated2(String iddemand, List<HttpPostedFileBase> files)
        {
            String demandArray = _demandTable.GetRow(iddemand);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandArray);
           
            String id = demand["_id"].ToString();

            //Keep status as integer
            demand["status"] = Convert.ToInt16(demand["status"].ToString());
            JArray archivos = new JArray();
            int num = 0;
            foreach(HttpPostedFileBase f1 in files){
                num++;
                string fileExt = f1.FileName.Split('.').Last();
                archivos.Add("Comprobante" + num + "_" + demand["folio"].ToString().TrimStart('0') + "." + fileExt);
            }
          
            if (files != null) demand["receiptFile"] = archivos;
            demand.Remove("_id");
            _demandTable.SaveRow(JsonConvert.SerializeObject(demand), id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "Demand", DateTime.Now.ToString());
            if (demand["destinyOptions"].ToString() != "destruccion") UpdateStatusDemand(iddemand, 5);
            if (files != null)
            {
                string relativepath = "\\Uploads\\Dictamenes\\documentos\\";
                string absolutepath = Server.MapPath(relativepath);
                num = 0;
                foreach (HttpPostedFileBase f1 in files)
                {
                    num++;
                    if (!System.IO.Directory.Exists(absolutepath))
                    {
                        System.IO.Directory.CreateDirectory(absolutepath);
                    }
                    string fileExt = f1.FileName.Split('.').Last();
                    f1.SaveAs(absolutepath + "\\Comprobante" + num + "_" + demand["folio"].ToString().TrimStart('0') + "." + fileExt); ;
                }
            }
            return id;
        }

        public void DenyDemand(String iddemand, string userid, string nota)
        {
            String demandArray = _demandTable.GetRow(iddemand);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandArray);
            JArray autos = JsonConvert.DeserializeObject<JArray>(demand["authorizations"].ToString());

            String moveArray = _profileTable.GetRow(demand["movement"].ToString());
            JObject move = JsonConvert.DeserializeObject<JObject>(moveArray);
            bool temp1 = false;
            try
            {
                bool.TryParse(move["temporal"].ToString(), out temp1);

            }
            catch { temp1 = false; }

            //Keep status as integer
            demand["status"] = Convert.ToInt16(demand["status"].ToString());

            JArray autos2 = new JArray();
            JArray objReferencias = JsonConvert.DeserializeObject<JArray>(demand["objects"].ToString());
            String foliodemand = demand["folio"].ToString();
            Dictionary<string, string> namerefdict = new Dictionary<string, string>();
            Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

            try
            {
                List<string> listconjunt = (from refe in objReferencias select (string)refe["location"]).ToList();
                String resultref = _objectTable.getParents(listconjunt);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

            }
            catch { }
            try
            {
                List<string> listactnames = (from refe in objReferencias select (string)refe["objectReference"]).ToList();
                String resultref = _objectTable.getCategory(listactnames);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

            }
            catch { }

           
            String UserRow = _userTable.GetRow(userid);
            JObject userRowja = JsonConvert.DeserializeObject<JObject>(UserRow);
            string nameuser = "";
            try
            {
                nameuser = userRowja["lastname"].ToString() + " " + userRowja["name"].ToString();
            }
            catch (Exception ex) { }
            bool valid = false;
            try
            {
                if (demand["status"].ToString() == "8" && demand["updateInfo"]["newObjects"].Count() >0)
                {
                    valid = true;
                }
            }
            catch (Exception ex) { }

           
            if (valid)
            {
                UpdateStatusDemand(iddemand, 6);
            }
            else
            {
                String id = demand["_id"].ToString();
                demand.Remove("_id");
                _demandTable.UpdateRow("DenyDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), id);
                _demandTable.UpdateRow("DeniersID", userid, id);
                _demandTable.UpdateRow("Deniers", nameuser, id);
                _demandTable.UpdateRow("DenyNote", nota, id);

                if (temp1 == true && demand["status"].ToString() == "5")
                {
                    UpdateStatusDemand(iddemand, 8);
                }
                else {
                    UpdateStatusDemand(iddemand, 7);
                }
                
            }
            string namemov = "";
            try
            {
                namemov = getNameMovement(demand["movement"].ToString());
            }
            catch (Exception ex) { }
            string usstring = _userTable.GetRow(demand["Creator"].ToString());
            JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

            string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

            String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
            StringBuilder act = new StringBuilder();
            string modelo = "";
            string marca = "";
            string idart = "";
            string serie = "";
            string nombre = "";
            string cantq = "1";
            string category = "";
            string deniednote = "";
            bool denied = false;
            string epc = "";
            string ubicacion = "";
            foreach (JObject nr in objReferencias)
            {
                JObject n;
                JToken nt2;
                denied = false;
                if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                {
                    serie = nr["serie"].ToString();
                    cantq = nr["quantity"].ToString();
                    try
                    {
                        if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                            epc = nr["epc"].ToString();
                        else
                            epc = "No aplica";

                    }
                    catch (Exception ex)
                    {
                        epc = "No aplica";
                    }
                    if (n.TryGetValue("modelo", out nt2))
                        modelo = n["modelo"].ToString();
                    if (n.TryGetValue("marca", out nt2))
                        marca = n["marca"].ToString();
                    if (n.TryGetValue("object_id", out nt2))
                        idart = n["object_id"].ToString();
                    if (n.TryGetValue("name", out nt2))
                        nombre = n["name"].ToString();
                    if (n.TryGetValue("namecategory", out nt2))
                        category = n["namecategory"].ToString();

                    if (nr.TryGetValue("location", out nt2))
                    {
                        ubicacion = getNameLocation(nr["location"].ToString());
                    }
                    else { ubicacion = ""; }

                    if (nr.TryGetValue("denied_note", out nt2))
                    {
                        deniednote = nr["denied_note"].ToString();
                        denied = true;
                    }

                    if (denied == true)
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;color:red;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5><h5>Denegado por: " + deniednote + "</h5></div><br>");
                    }
                    else
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5></div><br>");
                    }
                   // act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Id Activo: " + idart + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5></div><br>");
              
                 }
            }
            string bodygeneric = creadorstring+"<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
            string customtitle = "<h4>La Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + ",Ha sido Rechazada por " + nameuser + "</h4><h4>Motivo: " + nota + "</h4><br>";

            if (temp1 == true && demand["status"].ToString() == "5")
            {
                
                classNotifications.saveNotification("Movements", "Deny", "Solicitud devuelta, verificar datos. Se ha rechazado la solicitud de " + namemov + " con #folio: " + foliodemand);
                JArray recipients = new JArray();
                JArray attachments = new JArray();
                recipients.Add(demand["Creator"].ToString());
                string to = JsonConvert.SerializeObject(recipients);
                string attach = JsonConvert.SerializeObject(attachments);
                HtmlString body = new HtmlString(customtitle + bodygeneric);
                messagesC.SendMail(to, "La Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + ",Ha sido regresada  por " + nameuser, body.ToString()+" para verificar los datos.", attach, "Sistema");

                //List<String> listemails6 = new List<String>();
                //foreach (string recip in recipients)
                //{
                //    String em = GetCorreoUser(recip);
                //    listemails6.Add(em);
                //}

                //messagesC.SendExternalMail("", "Sistema", listemails6, body.ToString() + " para verificar los datos.", attachments, "La Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + ",Ha sido regresada  por " + nameuser);

            }
            else {
                
                classNotifications.saveNotification("Movements", "Deny", "Se ha rechazado la solicitud de " + namemov + " con #folio: " + foliodemand);
                JArray recipients = new JArray();
                JArray attachments = new JArray();
                recipients.Add(demand["Creator"].ToString());
                string to = JsonConvert.SerializeObject(recipients);
                string attach = JsonConvert.SerializeObject(attachments);
                HtmlString body = new HtmlString(customtitle + bodygeneric);
                messagesC.SendMail(to, "La Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + ",Ha sido Rechazada por " + nameuser, body.ToString(), attach, "Sistema");

                //List<String> listemails6 = new List<String>();
                //foreach (string recip in recipients)
                //{
                //    String em = GetCorreoUser(recip);
                //    listemails6.Add(em);
                //}

                //messagesC.SendExternalMail("", "Sistema", listemails6, body.ToString(), attachments, "La Solicitud de " + namemov + " con Folio #" + demand["folio"].ToString() + ",Ha sido Rechazada por " + nameuser);

            }
        }

        /// <summary>
        /// Returns all objects from a location
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public String GetAllSubObjects(string locationId, string assetType = null)
        {
            string results = "";
            //JArray objres = JsonConvert.DeserializeObject<JArray>(_objectTable.GetRows());
            //foreach (JObject ob in objres)
            //{
            //    try
            //    {
            //        JToken tk;
            //        if (ob.TryGetValue("date", out tk)) { 
            //        ob["date"] = "2015-01-10";
            //        }
            //        else
            //        {
            //            ob.Add("date", "2015-01-10");
            //        }
            //        if (ob.TryGetValue("data", out tk))
            //        {
                       
            //            ob.Remove("data");
            //        }
            //        else
            //        {
                        
            //        }
            //        if (ob.TryGetValue("price", out tk))
            //        {
            //            ob["price"] = "10.00";
            //        }
            //        else
            //        {
            //            ob.Add("price", "10.00");
            //        }
            //        if (ob.TryGetValue("objectReference", out tk))
            //        {
            //            ob["objectReference"] = "5593627201cc0c91622468c4";
            //        }
            //        else
            //        {
            //            ob.Add("objectReference", "5593627201cc0c91622468c4");
            //        }
            //        _objectTable.SaveRow(JsonConvert.SerializeObject(ob), ob["_id"].ToString());
            //    }
            //    catch { }
            //}
            try
            {
                JArray objectList = new JArray();
                if (locationId!="" && locationId!=null && locationId!="null")
                {
                    results = _objectTable.GetSubObjectsDemand(locationId);
                    objectList = JsonConvert.DeserializeObject<JArray>(results);

                    JArray tempList = new JArray();
                    List<string> objsid = (from objs in objectList.Children() select (string)objs["_id"]).ToList();
                    String valids = _demandTable.GetValidObjects(objsid);
                    JArray objsvalid = new JArray();
                    List<string> exists = new List<string>();
                    try
                    {
                        objsvalid = JsonConvert.DeserializeObject<JArray>(valids);
                        exists = objsvalid.Children()["objects"].Children()["id"].Values<string>().ToList();
                    }
                    catch (Exception ex)
                    {

                    }
                    //Choose just the matching assetTypes
                    if (assetType != null)
                    {
                        foreach (JObject document in objectList) //for each profile we create an option element with id as value and the name as the text
                        {
                            if (!exists.Contains(document["_id"].ToString()))
                            {
                                if (document["assetType"].ToString() == assetType)
                                {
                                    if (document["ext"].ToString() != "")
                                    {
                                        document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                                        try {
                                            if (System.IO.File.Exists(Server.MapPath("~") + "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]))
                                            {
                                                document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]);
                                            }
                                            else
                                            {
                                                document.Add("image_thumb", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                                            }
                                        }
                                        catch(Exception ex) {
                                            document.Add("image_thumb", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                                        }
                                        
                                       
                                    }
                                    tempList.Add(document);
                                }
                            }
                        }

                        objectList = tempList;
                    }
                    else
                    {
                        foreach (JObject document in objectList) //for each profile we create an option element with id as value and the name as the text
                        {
                            if (document["ext"].ToString() != "")
                            {
                                document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                               // document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]);
                                try
                                {
                                    if (System.IO.File.Exists(Server.MapPath("~") + "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]))
                                    {
                                        document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]);
                                    }
                                    else
                                    {
                                        document.Add("image_thumb", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    document.Add("image_thumb", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                                }
                            }

                        }
                    }
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
        /// Loads the objects reference by category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public String loadObjectsReference(string categoryId)
        {
            if (categoryId == "") categoryId = "null";

            try
            {
                //String objectOptions = "";
                String rowArray = "";
                if (categoryId == "Todas" || categoryId == null)
                {
                    rowArray = _objectReferenceTable.GetRows();
                }else{
                    rowArray = _objectReferenceTable.get("parentCategory", categoryId);
                }
                 
                JArray moveList = JsonConvert.DeserializeObject<JArray>(rowArray);

                foreach (JObject document in moveList) //for each profile we create an option element with id as value and the name as the text
                {
                    try { 
                    if (document["ext"].ToString() != "")
                    {
                        JToken token;
                        if (document.TryGetValue("image", out token))
                        {
                            document["image"] = "/Uploads/Images/ObjectReferences/" + document["_id"].ToString() + "." + document["ext"].ToString();
                        }
                        else
                        {
                            document.Add("image", "/Uploads/Images/ObjectReferences/" + document["_id"] + "." + document["ext"]);
                               }
                        if (document.TryGetValue("image_thumb", out token))
                        {
                            document["image_thumb"] = "/Uploads/Images/ObjectReferences/thumb_" + document["_id"].ToString() + "." + document["ext"].ToString();
                        }
                        else
                        {
                            try
                            {
                                if (System.IO.File.Exists(Server.MapPath("~") + "/Uploads/Images/ObjectReferences/thumb_" + document["_id"] + "." + document["ext"]))
                                {
                                    document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["_id"] + "." + document["ext"]);
                                }
                                else
                                {
                                    document.Add("image_thumb", "/Uploads/Images/ObjectReferences/" + document["_id"] + "." + document["ext"]);
                                }
                            }
                            catch (Exception ex)
                            {
                                document.Add("image_thumb", "/Uploads/Images/ObjectReferences/" + document["_id"] + "." + document["ext"]);
                            }
                          //  document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["_id"] + "." + document["ext"]);
                    
                        }
                    }
                    }catch(Exception ex){

                    }
                    try {
                        string rowString1 = _categoryTable.GetRow(document["parentCategory"].ToString());
                        JObject objs1 = JsonConvert.DeserializeObject<JObject>(rowString1);

                        document["categoryName"] = objs1["name"].ToString();
                    }
                    catch (Exception ex){ }
                    try {
                        string rowString = _userTable.GetRow(document["Creator"].ToString());
                        JObject objs = JsonConvert.DeserializeObject<JObject>(rowString);
                        document["nameCreator"] = objs["name"].ToString() + " " + objs["lastname"].ToString();
                    }
                    catch(Exception ex) {
                        document["nameCreator"] = "";
                    }
                }
                return moveList.ToString();

                //return objectOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public string getTypeMovement(string moveid)
        {
            if (this.Request.IsAjaxRequest())
            {
                if (moveid != null && moveid != "null")
                {
                    String rowArray = _profileTable.GetRow(moveid);
                    JObject moveobj = JsonConvert.DeserializeObject<JObject>(rowArray);

                    return moveobj["typeMovement"].ToString();

                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        public String getDemand(String demandID)
        {
            if (this.Request.IsAjaxRequest())
            {
                try
                {
                    String demandArray = _demandTable.GetRow(demandID);
                    JObject demand = JsonConvert.DeserializeObject<JObject>(demandArray);

                    String rowString = _profileTable.GetRow(demand["movement"].ToString());
                    JObject objs = JsonConvert.DeserializeObject<JObject>(rowString);

                    demand.Add("typeMovement", objs["typeMovement"].ToString());
                    if (objs["typeMovement"].ToString() == "create")
                    {
                        String cateArray = _objectReferenceTable.GetRow(demand["object"].ToString());
                        JObject category = JsonConvert.DeserializeObject<JObject>(cateArray);

                        demand.Add("category", category["parentCategory"].ToString());

                    }
                    //else
                    //{
                    //    rowString = _objectTable.GetRow(obj["object"].ToString());
                    //    objs = JsonConvert.DeserializeObject<JObject>(rowString);
                    //    obj["object"] = objs["name"].ToString();
                    //}


                    String demandobj = "";
                    demandobj = JsonConvert.SerializeObject(demand);

                    return demandobj;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public JsonResult getRoute(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {
                try {
                    string category = _categoryTable.GetRow(parentCategory);
                    JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                    JObject categoryObject = new JObject();
                    categoryObject.Add("id", actualCategory["_id"].ToString());
                    route.Add(categoryObject);
                    parentCategory = actualCategory["parentCategory"].ToString();
                }
                catch {
                    parentCategory = "";
                }
                
            }

            JObject result = new JObject();
            result.Add("route", route);
            return Json(JsonConvert.SerializeObject(result));
        }

        public JsonResult getRoute2(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {
                try {
                    string category = _locationTable.GetRow(parentCategory);
                    JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                    JObject categoryObject = new JObject();
                    categoryObject.Add("id", actualCategory["_id"].ToString());
                    route.Add(categoryObject);
                    parentCategory = actualCategory["parent"].ToString();
                }
                catch {
                    parentCategory = "";
                }
                
            }

            JObject result = new JObject();
            result.Add("route", route);
            return Json(JsonConvert.SerializeObject(result));
        }

        public String getRouteNames(String parentCategory = "null")
        {
            //Creating the route data
            String route = "";
            List<string> rutas = new List<string>();
            while (parentCategory != "null" && parentCategory != "")
            {

                string category = _locationTable.GetRow(parentCategory);
                JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                rutas.Add(actualCategory["name"].ToString());
                parentCategory = actualCategory["parent"].ToString();
            }

            for (int i = rutas.Count; i > 0; i--)
            {
                route = route + rutas[i - 1] + "/";
            }

            return route;
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
                catch {
                    parentCategory = "";
                }
               
            }

            return route;
        }

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
        public bool ValidSerie(String series)
        {
            try
            {
                JArray numbers = JsonConvert.DeserializeObject<JArray>(series);
                JArray resultarray = new JArray();
                List<string> listseries = (from serie in numbers where (string)serie!="" select (string)serie).ToList();
                String result = _objectTable.Validserie(listseries);
                try
                {
                    resultarray = JsonConvert.DeserializeObject<JArray>(result);
                }
                catch { }
                if (resultarray.Count() > 0)
                {
                    return true;
                }
                else
                {
                    result = _objectTable.ValidserieDemands(listseries);
                    resultarray.Clear();
                    try
                    {
                        resultarray = JsonConvert.DeserializeObject<JArray>(result);

                    }
                    catch { }

                    if (resultarray.Count() > 0) {
                        foreach (JObject obj in resultarray)
                        {
                            JToken t;
                            JArray tarray = JsonConvert.DeserializeObject<JArray>(obj["objects"].ToString());
                            foreach (JObject o1 in tarray) {
                                if (o1.TryGetValue("denied_note", out t))
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            
                        }
                    }
                        
                  return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public JsonResult getNodeContent2(String parentCategory)
        {
            if (parentCategory == "") parentCategory = "null";
            String categoriesString = _locationTable.Get("parent", parentCategory);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
            foreach (JObject category in categoriesObject)
            {
                try
                { //try to remove customFields, if can't be removed it doesn't care
                    category.Remove("profileFields");
                }
                catch (Exception e) { /*Ignored*/ }

                try
                {
                    category.Remove("parent");
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

        public void saveObjects(string Creator, string obj, string name, string location, string process, string marca, string modelo, string object_id, string perfil, string serie, string price, string label, string fecha, string quantity, string folio = null, string assetType = "", string department = "", string recepcion = "", string pedido = "", string solicitud = "",string observaciones="")
        {

            String cadena = "";
            int objsrealsave = 0;
            if (folio == null)
            {
                //for (int i = 0; i < cantidad; i++)
                //{
                string idunico = _objectTable.GetIdUnico();
                    cadena += "{'objectReference':'" + obj + "','name':'" + name + "','location':'" + location + "','status':'','process':'" + process
                                + "','serie':'" + serie
                                + "','price':'" + price
                                + "','perfil':'" + perfil
                                + "','date':'" + fecha
                                + "','label':'" + label
                                + "','assetType':'" + assetType
                                + "','department':'" + department
                                + "','folio':'" + folio
                                + "','marca':'" + marca
                                + "','modelo':'" + modelo
                                + "','quantity':'" + quantity
                                + "','object_id':'" + idunico
                                + "','num_solicitud':'" + solicitud
                                + "','num_pedido':'" + pedido
                                + "','num_reception':'" + recepcion
                                + "','observation':'"+observaciones
                                + "','Creator':'" + Creator
                                + "', 'EPC':'Porasignar"+idunico+"'}";
                    _objectTable.SaveRow(cadena, "null");
                    _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Insert: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                    objsrealsave++;
              //  }
                if (objsrealsave > 0)
                {
                    classNotifications.saveNotification("Objects", "Create", "Se han generado " + objsrealsave + " Activos Solicitud con #folio: " + folio);
                }
            }
            else
            {
                objsrealsave = 0;
                //ajustar validate
                //string result = validateLimit(cantidad, 1, "Su Solicitud con Folio:" + folio + ",ha sido aprobada,pero ha sido limitada", folio);

                //if (result == "true")
                //{

                //for (int i = 0; i < cantidad; i++)
                //{
                string idunico = _objectTable.GetIdUnico();
                                  
                    cadena += "{'objectReference':'" + obj + "','name':'" + name + "','location':'" + location + "','status':''"
                                  + ",'serie':'" + serie
                                  + "','price':'" + price
                                   + "','perfil':'" + perfil
                                  + "','date':'" + fecha
                                  + "','label':'" + label
                                  + "','assetType':'" + assetType
                                   + "','department':'" + department
                                  + "','folio':'" + folio
                                  + "','marca':'" + marca
                                  + "','modelo':'" + modelo
                                   + "','quantity':'" + quantity
                                  + "','object_id':'" + idunico
                                  + "','num_solicitud':'" + solicitud
                                  + "','num_pedido':'" + pedido
                                  + "','num_reception':'" + recepcion
                                   + "','observation':'" + observaciones
                                  + "','Creator':'" + Creator
                                  + "', 'EPC':'Porasignar" + idunico + "'}";
                    _objectTable.SaveRow(cadena, "null");
                    _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Insert: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                    objsrealsave++;
               // }
                if (objsrealsave > 0)
                {
                    classNotifications.saveNotification("Objects", "Create", "Se han generado " + objsrealsave + " Activos Solicitud con #folio: " + folio);
                }
                //}
                //else if (result == "false") { 

                //}
                //else
                //{
                //    objsrealsave = 0;
                //    int notavailables = 0;
                //    if (int.TryParse(result, out notavailables))
                //    {
                //        int availables = cantidad - notavailables;
                //        for (int i = 0; i < cantidad; i++)
                //        {
                //            if (i < availables)
                //            {
                //                cadena += "{'objectReference':'" + obj + "','name':'" + name + "','location':'" + location
                //                           + "', 'EPC':''}";
                //                _objectTable.SaveRowFalse(cadena, "null");


                //            }
                //            else
                //            {

                //                cadena += "{'objectReference':'" + obj + "','name':'" + name + "','location':'" + location
                //                           + "', 'EPC':''}";
                //                _objectTable.SaveRow(cadena, "null");
                //                objsrealsave++;

                //            }
                //        }
                //        if (objsrealsave > 0)
                //        {
                //            classNotifications.saveNotification("Objects", "Create", "Se han generado " + objsrealsave + " Activos Solicitud con #folio: " + folio);
                //        }
                //    }


                //}
            }
        }

        public void getCategories()
        {
            MongoModel Categories = new MongoModel("Categories");
            String categoriesselect = "<option value='null'></option>";
            try
            {
            JArray catja = JsonConvert.DeserializeObject<JArray>(Categories.GetRows());
            foreach (JObject cat in catja)
            {
                try
                {
                    categoriesselect += "<option value='" + cat["_id"].ToString() + "' >" + cat["name"].ToString() + "</option>";
                }
                catch { }
            }
            }
            catch { }
            ViewData["categories"] = categoriesselect;
        }
        /// <summary>
        /// Checks the users password for the authorization
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public String Login(string username, string password)
        {

            //Check the user on the database
            UserTable usertable = new UserTable();
            String user = _userTable.Get("name", username);
            JObject userObject = JsonConvert.DeserializeObject<JArray>(user).First as JObject;
            if (!HashPassword.ValidatePassword(password, userObject["pwd"].ToString()))
            {
                return "Introduzca la contraseña correcta";
            }
            return "";
        }

        public String SaveMovement(JArray array, String processid,List<string> listnews=null)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try //tryign to delete the users
                {
                    if (array.Count == 0) return null; //if array is empty there are no users to delete

                    String estado = "";
                    String pobj = _processesTable.GetRow(processid);
                    JObject newobj = JsonConvert.DeserializeObject<JObject>(pobj);

                    estado = newobj["status"].ToString();
                    foreach (JObject element in array) //froeach id in the array we must delete the document with that id from the db
                    {
                        if (listnews != null && listnews.Contains(element["id"].ToString()))
                            updateObject(element, estado);
                        else {
                            int cant, cant2;
                            int.TryParse(element["quantity_new"].ToString(), out cant);
                            int.TryParse(element["quantity"].ToString(), out cant2);
                            updateLocation(element["id"].ToString(), element["locationDestiny"].ToString(), estado, cant2,cant);
                        }
                    }

                    return "Success";
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public String DeleteObjects(JArray array, String processid)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try //tryign to delete the users
                {
                    if (array.Count == 0) return null; //if array is empty there are no users to delete

                    String estado = "";
                    String pobj = _processesTable.GetRow(processid);
                    JObject newobj = JsonConvert.DeserializeObject<JObject>(pobj);

                    estado = newobj["status"].ToString();

                    foreach (JObject element in array) //froeach id in the array we must delete the document with that id from the db
                    {
                        try
                        {
                            if (element["denied_note"].ToString() != "")
                            {
                                continue;
                            }

                        }
                        catch (Exception ex) {
                            String obj = _objectTable.GetRow(element["id"].ToString());
                            JObject obj2 = JsonConvert.DeserializeObject<JObject>(obj);

                            obj2["status"] = estado;

                            _objectTable.SaveRow(JsonConvert.SerializeObject(obj2), element["id"].ToString());
                            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + obj2["folio"], "ObjectReal", DateTime.Now.ToString());
                            _objectTable.DeleteRow(element["id"].ToString(), true);
                            //  updateStatus(element, "false");
                        }
                       
                    }

                    return "Success";
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public String updateLocation(string id, string newlocation, String estado, int cant, int cant2=0)
        {
            if (newlocation == "") newlocation = "null";
            string folio = "";
            if (this.Request.IsAjaxRequest())
            {
                if (id != "")
                {
                    String obj = _objectTable.GetRow(id);
                    JObject nuevo = new JObject();
                    var newobj = JsonConvert.DeserializeObject<JObject>(obj);
                    try { folio = newobj["folio"].ToString(); }
                    catch (Exception ex) { folio = ""; }
                    if (cant == cant2)
                    {
                        newobj["location"] = newlocation;
                        newobj["status"] = estado;
                        _objectTable.SaveRow(JsonConvert.SerializeObject(newobj), id);
                        _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                    }
                    else {
                        if (newobj["name"].ToString().Contains("Butaca"))
                        {
                            newobj["quantity"] = (cant - cant2).ToString();
                            _objectTable.SaveRow(JsonConvert.SerializeObject(newobj), id);
                            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                            String oldobj = _objectTable.GetButacas(newlocation);
                            JArray butacas = JsonConvert.DeserializeObject<JArray>(oldobj);
                            if (butacas.Count() > 0)
                            {
                                foreach (JObject butaca in butacas)
                                {
                                    int cant3 = 0;
                                    int.TryParse(butaca["quantity"].ToString(), out cant3);
                                    butaca["quantity"] = (cant3 + cant2).ToString();
                                    _objectTable.SaveRow(JsonConvert.SerializeObject(butaca), butaca["_id"].ToString());
                                    _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                                    break;
                                }
                            }
                            else {
                                
                                nuevo = newobj;
                                nuevo["location"] = newlocation;
                                nuevo["status"] = estado;
                                nuevo["quantity"] = cant2.ToString();
                                nuevo.Remove("_id");
                                _objectTable.SaveRow(JsonConvert.SerializeObject(nuevo));
                                _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                            }
                            
                        }
                        else {
                            newobj["quantity"] = (cant - cant2).ToString();
                            _objectTable.SaveRow(JsonConvert.SerializeObject(newobj), id);
                            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                            nuevo = newobj;
                            nuevo["location"] = newlocation;
                            nuevo["status"] = estado;
                            nuevo["quantity"] = cant2.ToString();
                            nuevo.Remove("_id");
                            _objectTable.SaveRow(JsonConvert.SerializeObject(nuevo));
                            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                        }
                        
                    }
                    
                   
                    return "success";
                }

            }

            return null;
        }

        public String updateObject(JObject activ, String estado)
        {
            try
            {
                string id = activ["id"].ToString();
                string newlocation = activ["locationDestiny"].ToString();
                if (newlocation == "") newlocation = "null";
                if (this.Request.IsAjaxRequest())
                {
                    if (id != "")
                    {
                        String obj = _objectTable.GetRow(id);
                        var newobj = JsonConvert.DeserializeObject<JObject>(obj);
                        string folio;
                        try { folio = newobj["folio"].ToString(); }
                        catch (Exception ex) { folio = ""; }
                        newobj["location"] = newlocation;
                        newobj["status"] = "Activo";
                        JToken valtok = "";
                        if (newobj.TryGetValue("marca", out valtok))
                            newobj["marca"] = activ["marca"].ToString();
                        else
                            newobj.Add("marca", activ["marca"]);

                        if (newobj.TryGetValue("modelo", out valtok))
                            newobj["modelo"] = activ["modelo"].ToString();
                        else
                            newobj.Add("modelo", activ["modelo"]);

                        if (newobj.TryGetValue("EPC", out valtok))
                            newobj["EPC"] = activ["epc"].ToString();
                        else
                            newobj.Add("EPC", activ["epc"]);
                        if (newobj.TryGetValue("serie", out valtok))
                            newobj["serie"] = activ["serie"].ToString();
                        else
                            newobj.Add("serie", activ["serie"]);
                        if (newobj.TryGetValue("name", out valtok))
                            newobj["name"] = activ["descripcion"].ToString();
                        else
                            newobj.Add("name", activ["descripcion"]);

                        _objectTable.SaveRow(JsonConvert.SerializeObject(newobj), id);
                        _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + folio, "ObjectReal", DateTime.Now.ToString());
                        return "success";
                    }

                }
                return null;
            }
            catch (Exception ex) {
                return null;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public String UpdateMovement(string id, string aditional = null, HttpPostedFileBase file = null, HttpPostedFileBase file1 = null, HttpPostedFileBase file2 = null)
        {
            String movementString = _demandTable.GetRow(id);
            JObject movement = JsonConvert.DeserializeObject<JObject>(movementString);
            String folio;
            try { folio = movement["folio"].ToString(); }
            catch (Exception ex) { folio = ""; }
            string namemov = "";
            try
            {
                namemov = getNameMovement(movement["movement"].ToString());
            }
            catch (Exception ex) { }
            JArray recipients = new JArray();
            JArray attachments = new JArray();

            Dictionary<string, string> namerefdict = new Dictionary<string, string>();
            Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

            try
            {
                List<string> listconjunt = (from refe in movement["objects"] select (string)refe["location"]).ToList();
                String resultref = _objectTable.getParents(listconjunt);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

            }
            catch { }

            try
            {
                List<string> listactnames = (from refe in movement["objects"] select (string)refe["objectReference"]).ToList();
                String resultref = _objectTable.getCategory(listactnames);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

            }
            catch { }

            string usstring = _userTable.GetRow(movement["Creator"].ToString());
            JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

            string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

            String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
            StringBuilder act = new StringBuilder();
            string modelo = "";
            string marca = "";
            string idart = "";
            string serie = "";
            string nombre = "";
            string cantq = "1";
            string category = "";
            string epc = "";
            string deniednote = "";
            bool denied = false;
            string ubicacion = "";
            foreach (JObject nr in movement["objects"])
            {
                JObject n;
                JToken nt2;
                denied = false;
                serie = nr["serie"].ToString();
                cantq = nr["quantity"].ToString();
                try
                {
                    if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                        epc = nr["epc"].ToString();
                    else
                        epc = "No aplica";

                }
                catch (Exception ex)
                {
                    epc = "No aplica";
                }
                if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                {
                    if (n.TryGetValue("modelo", out nt2))
                        modelo = n["modelo"].ToString();
                    if (n.TryGetValue("marca", out nt2))
                        marca = n["marca"].ToString();
                    if (n.TryGetValue("object_id", out nt2))
                        idart = n["object_id"].ToString();
                    if (n.TryGetValue("name", out nt2))
                        nombre = n["name"].ToString();
                    if (n.TryGetValue("namecategory", out nt2))
                        category = n["namecategory"].ToString();

                    if (nr.TryGetValue("location", out nt2))
                    {
                        ubicacion = getNameLocation(nr["location"].ToString());
                    }
                    else { ubicacion = ""; }

                    if (nr.TryGetValue("denied_note", out nt2))
                    {
                        deniednote = nr["denied_note"].ToString();
                        denied = true;
                    }

                    if (denied == true)
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;color:red;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5><h5>Denegado por: " + deniednote + "</h5></div><br>");
                    }
                    else
                    {
                        act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5></div><br>");
                    }

                }
            }

            if (aditional != null)
            {
                JArray aditionalObject = JsonConvert.DeserializeObject<JArray>(aditional);
                //Get acta info
                if (movement["destinyOptions"] != null && movement["destinyOptions"].ToString() == "destruccion")
                {
                    if (movement["setRecepit"].ToString() == "false" && Convert.ToInt16(movement["status"].ToString()) > 3)
                    {
                        movement["actInfo"] = aditionalObject[0];
                        aditionalObject[0].Remove();
                    }
                    //Witness signature Acction
                    else if (movement["setRecepit"].ToString() == "true")
                    {
                        var cantApproved = 0;
                        foreach (JObject witness in movement["witness"])
                        {
                            //If it the same user it approved the signature
                            if (witness["id_user"].ToString() == Session["_id"].ToString())
                            {
                                witness["approved"] = "1";
                                witness["date"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            }
                            //Cheack if all the witness approved
                            if (witness["approved"].ToString() == "0") cantApproved++;
                        }

                        movement["status"] = Convert.ToInt16(movement["status"].ToString());
                        _demandTable.SaveRow(JsonConvert.SerializeObject(movement), id);
                        _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + folio, "Demand", DateTime.Now.ToString());
                        if (cantApproved == 0)
                        { //AbissNow
                            generateActaPDF(id); //Creates Acta
                            UpdateStatusDemand(id, 5);
                            try
                            {
                                foreach (var autor in movement["approval"])
                                {
                                    try
                                    {
                                        recipients.Add(autor["id_user"].ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }

                                }
                                string to = JsonConvert.SerializeObject(recipients);
                                string attach = JsonConvert.SerializeObject(attachments);


                                string bodygeneric = creadorstring+"<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
                                messagesC.SendMail(to, "En espera de visto bueno Movimiento de " + namemov + ", Folio: " + movement["folio"].ToString(), "Ha sido actualizado movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + movement["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su visto bueno." + bodygeneric, attach, "Sistema");

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        return "Saved";
                    }
                }

                JArray objarray = JsonConvert.DeserializeObject<JArray>(movement["objects"].ToString());
                JArray objs = new JArray();
                foreach (JObject element in objarray)
                {
                    foreach (JObject adittion in aditionalObject)
                    {
                        if(element["id"].ToString()==adittion["id"].ToString()){
                            foreach (KeyValuePair<string, JToken> token in adittion)
                            {
                                if (token.Key != "id") 
                                    element[token.Key] = token.Value;
                            }

                        }
                    }
                    objs.Add(element);
                }

                movement["objects"] = objs;
            }

            if (movement["status"].ToString() == "4")
            {
                if (movement["destinyOptions"] != null && movement["destinyOptions"].ToString() == "destruccion")
                {
                    if (movement["setRecepit"].ToString() == "false")
                    {
                        movement["setRecepit"] = "true";
                        movementString = JsonConvert.SerializeObject(movement);
                        _demandTable.SaveRow(movementString, id);
                        _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + movement["folio"], "Demand", DateTime.Now.ToString());
                        try
                        {
                            foreach (var autor in movement["witness"])
                            {
                                try
                                {
                                    recipients.Add(autor["id_user"].ToString());
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }

                            }
                            string to = JsonConvert.SerializeObject(recipients);
                            string attach = JsonConvert.SerializeObject(attachments);

                            string bodygeneric = "<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
                            messagesC.SendMail(to, "Esta Pendiente de Firma para aprobar la destrucción Movimiento de " + namemov + ", Folio: " + movement["folio"].ToString(), "Ha sido actualizado movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + movement["folio"].ToString() + ".<br/> Dicho movimiento está Pendiente de Firma para aprobar la destrucción." + bodygeneric, attach, "Sistema");
                         

                        }
                        catch (Exception ex) { }
                    }
                    else { //Update firmas and create acta change status
                        //UpdateStatusDemand(id, 5);
                    }
                }
                else
                {
                    try
                    {
                        foreach (var autor in movement["approval"])
                        {
                            try
                            {
                                recipients.Add(autor["id_user"].ToString());
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }

                        }
                        string to = JsonConvert.SerializeObject(recipients);
                        string attach = JsonConvert.SerializeObject(attachments);


                        string bodygeneric = "<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
                        messagesC.SendMail(to, "En espera de visto bueno Movimiento de " + namemov + ", Folio: " + movement["folio"].ToString(), "Ha sido actualizado movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + movement["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su visto bueno." + bodygeneric, attach, "Sistema");
                   
                    }
                    catch (Exception ex)
                    {

                    }
                }

                //Save files
                List<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
                if (file != null)
                {
                    files.Add(file);
                }
                if (file1 != null)
                {
                    files.Add(file1);
                }
                if (file2 != null)
                {
                    files.Add(file2);
                }

                string archivo = getDemandAdjudicated2(id, files);

                return archivo;
            }
            else
            {
                movement["status"] = 3;
                movement.Remove("_id");
                movementString = JsonConvert.SerializeObject(movement);
                _demandTable.SaveRow(movementString, id);
                _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + movement["folio"], "Demand", DateTime.Now.ToString());
                foreach (var autor in movement["authorizations"])
                {
                    try
                    {
                        recipients.Add(autor["id_user"].ToString());
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
               
                string to = JsonConvert.SerializeObject(recipients);
                string attach = JsonConvert.SerializeObject(attachments);
               
                string bodygeneric = "<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
                messagesC.SendMail(to, "Ha sido actualizado Movimiento de " + namemov + ", Folio: " + movement["folio"].ToString(), "Ha sido actualizado movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + movement["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su autorización." + bodygeneric, attach, "Sistema");
              
            }

            return movementString;

        }

        public String UpdateMovementAccounting(string id, string accounting = null)
        {
            String movementString = _demandTable.GetRow(id);
            JObject movement = JsonConvert.DeserializeObject<JObject>(movementString);

            if (accounting != null)
            {
                JArray aditionalObject = JsonConvert.DeserializeObject<JArray>(accounting);
                JArray objarray = JsonConvert.DeserializeObject<JArray>(movement["objects"].ToString());
                JArray objs = new JArray();
                foreach (JObject element in objarray)
                {
                    foreach (JObject adittion in aditionalObject)
                    {
                        if (element["id"].ToString() == adittion["id"].ToString())
                        {
                            foreach (KeyValuePair<string, JToken> token in adittion)
                            {
                                if (token.Key != "id")
                                    element[token.Key] = token.Value;
                            }

                        }
                    }
                    objs.Add(element);
                }

                movement["objects"] = objs;
            }
            string namemov = "";
            try
            {
                namemov = getNameMovement(movement["movement"].ToString());
            }
            catch (Exception ex) { }
            JArray recipients = new JArray();
            JArray attachments = new JArray();

            movement.Remove("_id");
            movementString = JsonConvert.SerializeObject(movement);
            _demandTable.SaveRow(movementString, id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + movement["folio"], "Demand", DateTime.Now.ToString());
            foreach (var autor in movement["authorizations"])
            {
                try
                {
                    recipients.Add(autor["id_user"].ToString());
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
            string to = JsonConvert.SerializeObject(recipients);
            string attach = JsonConvert.SerializeObject(attachments);
            Dictionary<string, string> namerefdict = new Dictionary<string, string>();
            Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

            try
            {
                List<string> listconjunt = (from refe in movement["objects"] select (string)refe["location"]).ToList();
                String resultref = _objectTable.getParents(listconjunt);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

            }
            catch { }

            try
            {
                List<string> listactnames = (from refe in movement["objects"] select (string)refe["objectReference"]).ToList();
                String resultref = _objectTable.getCategory(listactnames);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

            }
            catch { }

            string usstring = _userTable.GetRow(movement["Creator"].ToString());
            JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

            string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

            String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
            StringBuilder act = new StringBuilder();
            string modelo = "";
            string marca = "";
            string idart = "";
            string serie = "";
            string nombre = "";
            string cantq = "1";
            string category = "";
            string epc = "";
            bool denied = false;
            string ubicacion = "";
            foreach (JObject nr in movement["objects"])
            {
                JObject n;
                JToken nt2;
                denied = false;
                serie = nr["serie"].ToString();
                cantq = nr["quantity"].ToString();
                try
                {
                    if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                        epc = nr["epc"].ToString();
                    else
                        epc = "No aplica";

                }
                catch (Exception ex)
                {
                    epc = "No aplica";
                }
                if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                {
                    if (n.TryGetValue("modelo", out nt2))
                        modelo = n["modelo"].ToString();
                    if (n.TryGetValue("marca", out nt2))
                        marca = n["marca"].ToString();
                    if (n.TryGetValue("object_id", out nt2))
                        idart = n["object_id"].ToString();
                    if (n.TryGetValue("name", out nt2))
                        nombre = n["name"].ToString();
                    if (n.TryGetValue("namecategory", out nt2))
                        category = n["namecategory"].ToString();

                    if (nr.TryGetValue("location", out nt2))
                    {
                        ubicacion = getNameLocation(nr["location"].ToString());
                    }
                    else { ubicacion = ""; }

                    act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5></div><br>");
                }
            }
            string bodygeneric = creadorstring+"<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
            messagesC.SendMail(to, "Ha sido actualizado Movimiento de " + namemov + ", Folio: " + movement["folio"].ToString(), "Ha sido actualizado movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + movement["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su autorización." + bodygeneric, attach, "Sistema");
            //List<String> listemails = new List<String>();
            //foreach (string recip in recipients)
            //{
            //    String em = GetCorreoUser(recip);
            //    listemails.Add(em);
            //}

            //messagesC.SendExternalMail("", "Sistema", listemails, "Ha sido actualizado movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + movement["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su autorización." + bodygeneric, attachments, "Ha sido actualizado movimiento de " + namemov + ", Folio: " + movement["folio"].ToString());

            return movementString;

        }

        public String UpdateTempMovement(string id) {
            String movementString = _demandTable.GetRow(id);
            JObject movement = JsonConvert.DeserializeObject<JObject>(movementString);
            JArray objarray = JsonConvert.DeserializeObject<JArray>(movement["objects"].ToString());
            JArray objs = new JArray();

            foreach (JObject element in objarray)
            {
                if (element["entry"].ToString() == "0") {
                    String objstring = _objectTable.GetRow(element["id"].ToString());
                    JObject obj1 = JsonConvert.DeserializeObject<JObject>(objstring);

                    String objstring2 = _objectReferenceTable.GetRow(element["objectReference"].ToString());
                    JObject obj2 = JsonConvert.DeserializeObject<JObject>(objstring2);

                    obj1["objectReference"] = element["objectReference"];
                    obj1["name"] = element["name"];
                    obj1["marca"] = element["marca"];
                    obj1["modelo"] = element["modelo"];
                    obj1["serie"] = element["serie"];
                    obj1["object_id"] = obj2["object_id"];
                 //   obj1["status"] = movement[""];
                    obj1.Remove("_id");
                    _objectTable.SaveRow(JsonConvert.SerializeObject(obj1), element["id"].ToString());
                    _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + obj1["folio"], "ObjectReal", DateTime.Now.ToString());
                }
            }
           
            string namemov = "";
            try
            {
                namemov = getNameMovement(movement["movement"].ToString());
            }
            catch (Exception ex) { }
            JArray recipients = new JArray();
            JArray attachments = new JArray();

            movement.Remove("_id");
            movementString = JsonConvert.SerializeObject(movement);
            _demandTable.SaveRow(movementString, id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + movement["folio"], "Demand", DateTime.Now.ToString());
            foreach (var autor in movement["authorizations"])
            {
                try
                {
                    recipients.Add(autor["id_user"].ToString());
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
            string to = JsonConvert.SerializeObject(recipients);
            string attach = JsonConvert.SerializeObject(attachments);

            Dictionary<string, string> namerefdict = new Dictionary<string, string>();
            Dictionary<string, JObject> activosinfo = new Dictionary<string, JObject>();

            try
            {
                List<string> listconjunt = (from refe in movement["objects"] select (string)refe["location"]).ToList();
                String resultref = _objectTable.getParents(listconjunt);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                namerefdict = resultja.ToDictionary(x => (string)x["_id"], x => (string)x["nameparent"]);

            }
            catch { }

            try
            {
                List<string> listactnames = (from refe in movement["objects"] select (string)refe["objectReference"]).ToList();
                String resultref = _objectTable.getCategory(listactnames);
                JArray resultja = JsonConvert.DeserializeObject<JArray>(resultref);
                activosinfo = resultja.ToDictionary(x => (string)x["_id"], x => (JObject)x);

            }
            catch { }

            String conjuntosname = String.Join(",", namerefdict.Values.Distinct().ToList());
            StringBuilder act = new StringBuilder();
            string modelo = "";
            string marca = "";
            string idart = "";
            string serie = "";
            string nombre = "";
            string cantq = "1";
            string category = "";
            string epc = "";
            bool denied = false;
            string ubicacion = "";
            string usstring = _userTable.GetRow(movement["Creator"].ToString());
            JObject usrobj = JsonConvert.DeserializeObject<JObject>(usstring);

            string creadorstring = "<h5>Creador:<label style='font-weight: bold;'>" + usrobj["name"].ToString() + " " + usrobj["lastname"].ToString() + "</label></h5><h5 style='font-weight: bold;'>Número de usuario:" + usrobj["user"].ToString() + "</h5><br>";

            foreach (JObject nr in movement["objects"])
            {
                JObject n;
                JToken nt2;
                denied = false;
                serie = nr["serie"].ToString();
                cantq = nr["quantity"].ToString();
                try
                {
                    if (nr["epc"].ToString() != "" && nr["epc"].ToString() != "null")
                        epc = nr["epc"].ToString();
                    else
                        epc = "No aplica";

                }
                catch (Exception ex)
                {
                    epc = "No aplica";
                }
                if (activosinfo.TryGetValue(nr["objectReference"].ToString(), out n))
                {
                    if (n.TryGetValue("modelo", out nt2))
                        modelo = n["modelo"].ToString();
                    if (n.TryGetValue("marca", out nt2))
                        marca = n["marca"].ToString();
                    if (n.TryGetValue("object_id", out nt2))
                        idart = n["object_id"].ToString();
                    if (n.TryGetValue("name", out nt2))
                        nombre = n["name"].ToString();
                    if (n.TryGetValue("namecategory", out nt2))
                        category = n["namecategory"].ToString();

                    if (nr.TryGetValue("location", out nt2))
                    {
                        ubicacion = getNameLocation(nr["location"].ToString());
                    }
                    else { ubicacion = ""; }
                    act.Append("<div style='border:#858691 dotted 2px;padding: 10px;'><h5>Nombre: " + nombre + "</h5><h5>Cantidad: " + cantq + "</h5><h5>Ubicación: " + ubicacion + "</h5><h5>Id Activo: " + idart + "</h5><h5>EPC: " + epc + "</h5><h5>modelo: " + modelo + "</h5><h5>marca: " + marca + "</h5><h5>Categoria: " + category + "</h5><h5>Serie: " + serie + "</h5></div><br>");
                }
            }
            string bodygeneric = creadorstring+"<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
            messagesC.SendMail(to, "Ha sido actualizado Movimiento de " + namemov + ", Folio: " + movement["folio"].ToString(), "Ha sido actualizado movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + movement["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su autorización." + bodygeneric, attach, "Sistema");
            //List<String> listemails = new List<String>();
            //foreach (string recip in recipients)
            //{
            //    String em = GetCorreoUser(recip);
            //    listemails.Add(em);
            //}

            //messagesC.SendExternalMail("", "Sistema", listemails, "Ha sido actualizado movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + movement["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su autorización." + bodygeneric, attachments, "Ha sido actualizado movimiento de " + namemov + ", Folio: " + movement["folio"].ToString());

            //string bodygeneric = "<h5>Conjunto(s):<label style='font-weight: bold;'>" + conjuntosname + "</label></h5><h5 style='font-weight: bold;'>Los activos Involucrados son:</h5><br>" + act;
           // messagesC.SendMail(to, "Nuevo movimiento de " + namemov + " generado, Folio: " + newdemand["folio"].ToString(), "Ha sido generado un nuevo movimiento de " + namemov + ".<br/> Solicitante: " + getNameUser(Session["_id"].ToString()) + "<br/> Folio: #" + newdemand["folio"].ToString() + ".<br/> Dicho movimiento está en espera de su autorización." + bodygeneric, attach, "Sistema");
            return movementString;
        }

        //public String UpdateMovementAccounting(string id, string accounting = null)
        //{
        //    String movementString = _demandTable.GetRow(id);
        //    JObject movement = JsonConvert.DeserializeObject<JObject>(movementString);


        //    if (accounting != null)
        //    {
        //        JArray aditionalObject = JsonConvert.DeserializeObject<JArray>(accounting);
        //        JArray objarray = JsonConvert.DeserializeObject<JArray>(movement["objects"].ToString());
        //        JArray objs = new JArray();
        //        foreach (JObject element in objarray)
        //        {
        //            foreach (JObject adittion in aditionalObject)
        //            {
        //                if (element["id"].ToString() == adittion["id"].ToString())
        //                {
        //                    foreach (KeyValuePair<string, JToken> token in adittion)
        //                    {
        //                        if (token.Key != "id")
        //                            element[token.Key] = token.Value;
        //                    }

        //                }
        //            }
        //            objs.Add(element);
        //        }

        //        movement["objects"] = objs;
        //    }
        //    string namemov = "";
        //    try
        //    {
        //        namemov = getNameMovement(movement["movement"].ToString());
        //}
        //    catch (Exception ex) { }
        //    JArray recipients = new JArray();
        //    JArray attachments = new JArray();

        //    movement.Remove("_id");
        //    movementString = JsonConvert.SerializeObject(movement);
        //    _demandTable.SaveRow(movementString, id);

        //    foreach (var autor in movement["authorizations"])
        //    {
        //        try
        //        {
        //            recipients.Add(autor["id_user"].ToString());
        //        }
        //        catch (Exception ex)
        //        {
        //            continue;
        //        }

        //    }
        //    string to = JsonConvert.SerializeObject(recipients);
        //    string attach = JsonConvert.SerializeObject(attachments);
        //    messagesC.SendMail(to, "La Solicitud  de " + namemov + " con Folio #" + movement["folio"].ToString() + ",esta Pendiente de Autorizar", "La Solicitud  de " + namemov + " con Folio #" + movement["folio"].ToString() + ",esta Pendiente de Autorizar", attach, "Sistema");

        //    return movementString;

        //}
      
        public String UpdateInvoice(String id, HttpPostedFileBase file = null) {
            String demandString = _demandTable.GetRow(id);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandString);

            if (file != null)
            {
                string fileExt = file.FileName.Split('.').Last();
                demand["invoiceFile"] = "Factura_" + demand["folio"].ToString().TrimStart('0') + "." + fileExt;
            }
            demand.Remove("_id");
            _demandTable.SaveRow(JsonConvert.SerializeObject(demand), id);
            _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "Demand", DateTime.Now.ToString());
            if (file != null)
            {
                string fileExt = file.FileName.Split('.').Last();
                string relativepath = "\\Uploads\\Dictamenes\\documentos\\";
                string absolutepath = Server.MapPath(relativepath);
                
                    if (!System.IO.Directory.Exists(absolutepath))
                    {
                        System.IO.Directory.CreateDirectory(absolutepath);
                    }
                    file.SaveAs(absolutepath + "\\Factura_" + demand["folio"].ToString().TrimStart('0') + "." + fileExt);

            }
            return id;
        
        }

        public String updateStatus(string id, string newstatus)
        {
            if (newstatus == "") newstatus = "null";
            if (this.Request.IsAjaxRequest())
            {
                if (id != "")
                {
                    String obj = _objectTable.GetRow(id);
                    var newobj = JsonConvert.DeserializeObject<JObject>(obj);

                    newobj["system_status"] = newstatus;

                    _objectTable.SaveRow(JsonConvert.SerializeObject(newobj), id);
                    _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + newobj["folio"], "ObjectReal", DateTime.Now.ToString());
                    return "success";
                }

            }

            return null;
        }

        public String updateObjectsAdjudicated(string iddemand, string idObject, string tipoObs)
        {
            if (tipoObs == "") return null;
            if (this.Request.IsAjaxRequest())
            {
                if (idObject != "")
                {
                    String demandstring = _demandTable.GetRow(iddemand);
                    JObject demandobj = JsonConvert.DeserializeObject<JObject>(demandstring);

                    JArray objetos = JsonConvert.DeserializeObject<JArray>(demandobj["objects"].ToString());
                    JArray newobjetos = new JArray();

                    foreach (JObject ob1 in objetos)
                    {
                        if (ob1["id"].ToString() == idObject)
                        {
                            ob1["typeObs"] = JsonConvert.DeserializeObject<JArray>("["+tipoObs+"]");
                        }
                        newobjetos.Add(ob1);
                    }

                    demandobj["objects"] = newobjetos;

                    _demandTable.SaveRow(JsonConvert.SerializeObject(demandobj), iddemand);
                    _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demandobj["folio"], "Demand", DateTime.Now.ToString());
                    return "success";
                }

            }

            return null;
        }

        public String getDictaminador(JArray location, string assettype)
        {
            List<string> list1 = new List<string>();
            JArray ele = new JArray();
            JObject dict = new JObject();
            dict["userid"] = "";
            dict["username"] = "";

            try
            {
                String objarray = _dictaminadorTable.Get("type.value", assettype);
                JArray dits = JsonConvert.DeserializeObject<JArray>(objarray);

                foreach (JObject cad1 in location)
                {
                    ele = getRoute3(cad1["location"].ToString());
                    foreach (JObject cad2 in dits)
                    {
                        foreach (String cad3 in ele)
                        {
                            if (cad2["location"]["value"].ToString() == "" && cad2["location"]["text"].ToString() == "Home") {
                                dict["userid"] = cad2["user"]["value"].ToString();
                                dict["username"] = cad2["user"]["text"].ToString();
                                break;
                            }
                            else if (cad3 == cad2["location"]["value"].ToString())
                            {
                                dict["userid"] = cad2["user"]["value"].ToString();
                                dict["username"] = cad2["user"]["text"].ToString();
                                break;
                            }
                        }

                    }
                }
            }
            catch
            {
            }


            return JsonConvert.SerializeObject(dict);
        }

        public string generatePDF(String demandid)
        {

            try
            {
                String objarray = _demandTable.GetRow(demandid);
                JObject demandobj = JsonConvert.DeserializeObject<JObject>(objarray);
                JArray objs = new JArray();
                String ubicacion = "";
                //Create PDF document
                string file = demandobj["dctFolio"].ToString() + ".pdf";
                string pdfurl = Server.MapPath("~") + "\\Uploads\\Dictamenes\\" + file;
                string relativepath = "\\Uploads\\Dictamenes\\";
                string absoluteurl = Server.MapPath(relativepath);
                //if (System.IO.Directory.Exists(absoluteurl))
                //{
                //    System.IO.Directory.Delete(absoluteurl, true);
                //    // System.IO.Directory.CreateDirectory(absoluteurl);
                //}

                if (!System.IO.Directory.Exists(absoluteurl))
                {
                    System.IO.Directory.CreateDirectory(absoluteurl);
                }
                Document doc = new Document(PageSize.A4);
                PdfWriter writer;
                try
                {

                    writer = PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Dictamenes/" + file,

                    FileMode.Create));
                }
                catch (Exception ex)
                {
                    if (!System.IO.Directory.Exists(absoluteurl))
                    {

                        System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    writer = PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Dictamenes/" + file,

                   FileMode.Create));

                }
                objs = JsonConvert.DeserializeObject<JArray>(demandobj["objects"].ToString());
                String documento = "";
                if (objs.Count > 0)
                {
                    JObject locat = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(objs[0]["location"].ToString()));
                    JObject conju = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(locat["parent"].ToString()));
                    ubicacion = conju["name"].ToString();
                }
                documento = documento + "<table>"
                + "<tr><td><table border='1'><tr><td><span style='text-align:center;'>DCT OBS</span></td></tr></table></td><td><span style='text-align:center;'>SISTEMA CAFI</span> <br/> <span style='text-align:center;'>Dictámen de obsolescencia</span> </td><td><img src='" + Server.MapPath("~") + "/Content/Images/clientes/logoCinepolis.png' alt='logo'></td></tr>"
                + "<tr><td></td><td></td><td><span style='font-weight:bold;'>Folio :</span> " + demandobj["folio"].ToString() + "<br/><span style='font-weight:bold;'>Fecha :</span> " + demandobj["CreatedDate"].ToString() + "<br/><span style='font-weight:bold;'>Conjunto :</span> " + ubicacion + "<br/></td></tr>"
                + "<tr><td colspan=3>A QUIEN CORRESPONDA <br/> Por este medio ratifico que el(los) artículo(s) relacionado(s) en este documento: </td></tr>"
                + "<tr><td colspan=3>"
                + "<table border='1'><tr><th ><span style='font-size:10px;text-align:center;'>Cant</span></th><th ><span style='font-size:10px;text-align:center;'>Descripción</span></th><th ><span style='font-size:10px;text-align:center;'>Marca</span></th><th ><span style='font-size:10px;text-align:center;'>Modelo</span></th><th ><span style='font-size:10px;text-align:center;'>Serie</span></th><th ><span style='font-size:10px;text-align:center;'>Crit. Sel</span></th><th><span style='font-size:10px;text-align:center;'>Imagen 1</span></th><th><span style='font-size:10px;text-align:center;'>Imagen 2</span></th><th ><span style='font-size:10px;text-align:center;'>Imagen 3</span></th></tr>";

                foreach (JObject ob1 in objs)
                {
                    if (ob1["denied_note"] != null) continue;
                    String refarray = _objectTable.GetRow(ob1["id"].ToString());
                    JObject refobj = JsonConvert.DeserializeObject<JObject>(refarray);
                    string nombre = "";
                    JToken tk;
                    if (!refobj.TryGetValue("name", out tk))
                    {
                        refobj.Add("name", "");
                    }
                    try
                    {
                        if (refobj["name"].ToString() == "")
                        {
                            String refarray1 = _objectReferenceTable.GetRow(refobj["objectReference"].ToString());
                            JObject refobj1 = JsonConvert.DeserializeObject<JObject>(refarray1);
                            nombre = refobj1["name"].ToString();
                        }
                        else
                        {
                            nombre = refobj["name"].ToString();
                        }
                    }
                    catch { }
                    documento = documento + "<tr><td><span style='font-size:10px;text-align:center;'>" + refobj["quantity"] + "</span></td><td><span style='font-size:10px;text-align:center;'>" + nombre + "</span></td>";
                    try
                    {
                        documento = documento + "<td><span style='font-size:10px;text-align:center;'>" + refobj["marca"] + "</span></td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td><span style='font-size:10px;text-align:center;'>" + refobj["modelo"] + "</span></td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td><span style='font-size:10px;text-align:center;'>" + refobj["serie"] + "</span></td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td><span style='font-size:10px;text-align:center;'>" + ob1["typeObs"].ToString().Replace("[", "").Replace("]", "") + "</span></td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }


                    JArray images = JsonConvert.DeserializeObject<JArray>(ob1["images"].ToString());
                    for (int i = 0; i < 3;i++ )
                    {
                        if (i > images.Count-1)
                        {
                            documento = documento + "<td></td>";
                        }
                        else if (images.Count > 0)
                        {
                            documento = documento + "<td><img height=50 width=50 src='" + Server.MapPath("~") + "/Uploads/Dictamenes/imagenes/" + images[i] + "' alt='logo'></td>";
                        }
                        
                    }
                    
                    documento = documento + "</tr>";
                }

                String userarray = _userTable.GetRow(demandobj["adjudicating"].ToString());
                JObject user = new JObject();
                try
                {
                    user = JsonConvert.DeserializeObject<JObject>(userarray);
                }
                catch (Exception ex)
                {
                    user.Add("userKey", "");
                }
                documento = documento + "</table>"
                + "</td></tr>"
                + "<tr><td colspan=3><br/> <span style='font-size:10px;'>Segun criterios de obsolescencia:</span></td></tr>"
                + "<tr><td colspan=3 > "
                      + "<ol>"
                          + "<li><span style='font-size:10px;'>De acuerdo al diagnóstico de reparación su costo de mantenimiento es mayor al 51% del costo del equipo "
                          + " según consta en el histórico de consumo de refacciones en el sistema MAXIMO y la lista de precios del proveedor.</span></li>"
                       + "<li><span style='font-size:10px;'>De acuerdo al diagnóstico de reparación no encuentran refacciones para su puesta en marcha nuevamente al ser"
                        + " un modelo descontinuado según informes del proveedor y/o fabricante del equipo.</span></li>   "
                        + " <li><span style='font-size:10px;'>Se quedó sin uso para el negocio debido a que se modificó el proceso de elaboración, según consta"
                        + " en el Manual de Operación del Negocio</span>.</li>"
                        + " <li><span style='font-size:10px;'>Se quedó sin uso para el negocio debido a que fue sustituido por uno de mayor capacidad, según consta"
                        + " en el Manual de Operación del Negocio.</span></li>"
                        + " <li><span style='font-size:10px;'>Se quedó sin uso para el conjunto debido a que no va con la nueva imagen y decoración, según consta"
                        + " en el proyecto.</span></li>"
                    + "</ol>"
                 + "</td></tr>"
                + "<tr><td colspan=3>" +
                    "<span style='font-size:10px;'>En cualquier caso, se procederá a su Venta, Donación o Destrucción y baja mediante sistema CAFI.</span> "
                    + "</td></tr>"
                + "<tr><td colspan=3><br/><span style='font-size:10px;'>Dictaminador: " + user["name"] + " " + user["lastname"] + " <br/> Firma del Dictaminador: " + user["userKey"].ToString() + "</span></td></tr>"
                +"</table>";
                doc.Open();
                HTMLWorker htmlWorker = new HTMLWorker(doc);
                StringReader sr = new StringReader(documento);
                //XMLWorkerHelper.GetInstance().ParseXHtml(
                //    writer, document, html
                //  );
                htmlWorker.Parse(sr);
                doc.Close();

                //  System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Dictamenes/" + file);


                //  Downloadpdf(file);
                return "/Uploads/Dictamenes/" + file;
                //  Response.End();
                //SPFile file = listItem.File;


                //  Response.TransmitFile(Server.MapPath("~") + "/App_Data/" + file);
                //  Response.Redirect(Server.MapPath("~") + "/App_Data/" + file);
                // Write the file to the Response

                //   Response.Flush();
                //    Response.End();
                /*     WebClient myClient = new WebClient();
                     string basefile = Path.GetFileName(file);
                     myClient.DownloadFile(url, file);*/

                //  File.Delete(myfile.FullName);
                //  System.IO.File.Delete(Server.MapPath("~") + "/App_Data/" + file);


            }
            catch (Exception ex)
            {
                return null;
            }

        }

        //***************************
        public string generatePDF2(String demandid)
        {

            try
            {
                String objarray = _demandTable.GetRow(demandid);
                JObject demandobj = JsonConvert.DeserializeObject<JObject>(objarray);
                JArray objs = new JArray();
                String ubicacion = "";
                //Create PDF document
                string file = demandobj["dctFolio"].ToString() + ".pdf";
                string pdfurl = Server.MapPath("~") + "\\Uploads\\Dictamenes\\" + file;
                string relativepath = "\\Uploads\\Dictamenes\\";
                string absoluteurl = Server.MapPath(relativepath);
                //if (System.IO.Directory.Exists(absoluteurl))
                //{
                //    System.IO.Directory.Delete(absoluteurl, true);
                //    // System.IO.Directory.CreateDirectory(absoluteurl);
                //}

                if (!System.IO.Directory.Exists(absoluteurl))
                {
                    System.IO.Directory.CreateDirectory(absoluteurl);
                }
                Document doc = new Document(PageSize.A4);
                PdfWriter writer;
                try
                {

                    writer = PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Dictamenes/" + file,

                    FileMode.Create));
                }
                catch (Exception ex)
                {
                    if (!System.IO.Directory.Exists(absoluteurl))
                    {

                        System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    writer = PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Dictamenes/" + file,

                   FileMode.Create));

                }
                objs = JsonConvert.DeserializeObject<JArray>(demandobj["objects"].ToString());
                String documento = "";
                if (objs.Count > 0)
                {
                    String locatarray = _locationTable.GetRow(objs[0]["location"].ToString());
                    JObject locat = JsonConvert.DeserializeObject<JObject>(locatarray);
                    ubicacion = locat["name"].ToString();
                }
                documento = documento + "<table>"
                + "<tr><td><table border='1'><tr><td><span style='text-align:center;'>DCT OBS</span></td></tr></table></td><td><span style='text-align:center;'>SISTEMA CAFI</span> <br/> <span style='text-align:center;'>Acta de hechos</span> </td><td><img src='" + Server.MapPath("~") + "/Content/Images/clientes/logoCinepolis.png' alt='logo'></td></tr>"
                + "<tr><td></td><td></td><td><span style='font-weight:bold;'>Folio :</span> " + demandobj["folio"].ToString() + "<br/><span style='font-weight:bold;'>Fecha :</span> " + demandobj["CreatedDate"].ToString() + "<br/><span style='font-weight:bold;'>Conjunto :</span> " + ubicacion + "<br/></td></tr>"
                + "<tr><td colspan=3>A QUIEN CORRESPONDA <br/> Por este medio ratifico que el(los) artículo(s) relacionado(s) en este documento: </td></tr>"
                + "<tr><td colspan=3>"
                + "<table border='1'><tr><th>Cant</th><th>Descripcion</th><th>Marca</th><th>Modelo</th><th>Serie</th><th>Crit. Sel</th><th>Imagen 1</th><th>Imagen 2</th><th>Imagen 3</th></tr>";

                foreach (JObject ob1 in objs)
                {
                    String refarray = _objectTable.GetRow(ob1["id"].ToString());
                    JObject refobj = JsonConvert.DeserializeObject<JObject>(refarray);

                    documento = documento + "<tr><td>1</td><td>" + refobj["name"] + "</td>";
                    try
                    {
                        documento = documento + "<td>" + ob1["marca"] + "</td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td>" + ob1["modelo"] + "</td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td>" + refobj["serie"] + "</td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td>" + ob1["typeObs"].ToString().Replace("[", "").Replace("]", "") + "</td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }


                    JArray images = JsonConvert.DeserializeObject<JArray>(ob1["images"].ToString());
                    for (int i = 0; i < 3; i++)
                    {
                        if (i > images.Count - 1)
                        {
                            documento = documento + "<td></td>";
                        }
                        else if (images.Count > 0)
                        {
                            documento = documento + "<td><img height=50 width=50 src='" + Server.MapPath("~") + "/Uploads/Dictamenes/imagenes/" + images[i] + "' alt='logo'></td>";
                        }

                    }

                    documento = documento + "</tr>";
                }

                String userarray = _userTable.GetRow(demandobj["adjudicating"].ToString());
                JObject user = new JObject();
                try
                {
                    user = JsonConvert.DeserializeObject<JObject>(userarray);
                }
                catch (Exception ex)
                {
                    user.Add("userKey", "");
        }
                documento = documento + "</table>"
                + "</td></tr>"
                + "<tr><td colspan=3><br/>Firma del Dictaminador: " + user["userKey"].ToString() + "</td></tr>"
                + "<tr><td colspan=3>Segun criterios de obsolescencia:</td></tr>"
                + "<tr><td colspan=3> "
                      + "<ol>"
                          + "<li>De acuerdo al diagnóstico de reparación su costo de mantenimiento es mayor al 51% del costo del equipo "
                          + " según consta en el histórico de consumo de refacciones en el sistema MAXIMO y la lista de precios del proveedor.</li>"
                       + "<li>De acuerdo al diagnóstico de reparación no encuentran refacciones para su puesta en marcha nuevamente al ser"
                        + " un modelo descontinuado según informes del proveedor y/o fabricante del equipo.</li>   "
                        + " <li>Se quedó sin uso para el negocio debido a que se modificó el proceso de elaboración, según consta"
                        + " en el Manual de Operación del Negocio.</li>"
                        + " <li>Se quedó sin uso para el negocio debido a que fue sustituido por uno de mayor capacidad, según consta"
                        + " en el Manual de Operación del Negocio.</li>"
                        + " <li>Se quedó sin uso para el conjunto debido a que no va con la nueva imagen y decoración, según consta"
                        + " en el proyecto.</li>"
                    + "</ol>"
                 + "</td></tr>"
                + "<tr><td colspan=3>" +
                    "En cualquier caso, se procederá a su Venta, Donación o Destrucción y baja mediante sistema CAFI. "
                    + "</td></tr></table>";
                doc.Open();
                HTMLWorker htmlWorker = new HTMLWorker(doc);
                StringReader sr = new StringReader(documento);
                //XMLWorkerHelper.GetInstance().ParseXHtml(
                //    writer, document, html
                //  );
                htmlWorker.Parse(sr);
                doc.Close();

                //  System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Dictamenes/" + file);


                //  Downloadpdf(file);
                return "/Uploads/Dictamenes/" + file;
                //  Response.End();
                //SPFile file = listItem.File;


                //  Response.TransmitFile(Server.MapPath("~") + "/App_Data/" + file);
                //  Response.Redirect(Server.MapPath("~") + "/App_Data/" + file);
                // Write the file to the Response

                //   Response.Flush();
                //    Response.End();
                /*     WebClient myClient = new WebClient();
                     string basefile = Path.GetFileName(file);
                     myClient.DownloadFile(url, file);*/

                //  File.Delete(myfile.FullName);
                //  System.IO.File.Delete(Server.MapPath("~") + "/App_Data/" + file);


            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public string generateActaPDF(String demandid)
        {

            try
            {
                String objarray = _demandTable.GetRow(demandid);
                JObject demandobj = JsonConvert.DeserializeObject<JObject>(objarray);
                JArray objs = new JArray();
                String id = demandobj["_id"].ToString();
                String folio = "";

                try
                {
                    folio = demandobj["ActFolio"].ToString();
                    return folio;
                }
                catch
                {
                    folio = generateActaFolio(id);
                    demandobj["ActFolio"] = folio;
                    demandobj.Remove("_id");
                    _demandTable.SaveRow(JsonConvert.SerializeObject(demandobj), id);
                    _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demandobj["folio"], "Demand", DateTime.Now.ToString());
                }


                //Create PDF document
                string file = folio + ".pdf";
                string pdfurl = Server.MapPath("~") + "\\Uploads\\Dictamenes\\documentos\\" + file;
                string relativepath = "\\Uploads\\Dictamenes\\documentos\\";
                string absoluteurl = Server.MapPath(relativepath);

                if (!System.IO.Directory.Exists(absoluteurl))
                {
                    System.IO.Directory.CreateDirectory(absoluteurl);
                }
                Document doc = new Document(PageSize.A4);
                PdfWriter writer;
                try
                {

                    writer = PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Dictamenes/documentos/" + file,

                    FileMode.Create));
                }
                catch (Exception ex)
                {
                    if (!System.IO.Directory.Exists(absoluteurl))
                    {

                        System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    writer = PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Dictamenes/documentos/" + file,

                   FileMode.Create));

                }
                String empresa = "";
                String cine = "";
                String domicilio = "";
                String destructor = "";
                String tipoactivo = "";
                String dictaminador = "";
                String fechadictamen = "";
                String vendido = "";
                String idlocation = "";
                List<String> fotos=new List<String>();

                fotos.Add("FOTO ANTES");
                fotos.Add("FOTO DURANTE");
                fotos.Add("FOTO DESPUÉS");
                objs = JsonConvert.DeserializeObject<JArray>(demandobj["objects"].ToString());
                String userarray = _userTable.GetRow(demandobj["adjudicating"].ToString());
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userarray);
                JArray dictarray = JsonConvert.DeserializeObject<JArray>(_dictaminadorTable.Get("user.value", demandobj["adjudicating"].ToString()));
                dictaminador = userobj["name"] + " " + userobj["lastname"];
                fechadictamen = demandobj["dctDate"].ToString();

                tipoactivo = (from mov in dictarray select (string)mov["type"]["text"]).First().ToString();

                if (objs.Count > 0) {
                    JObject locat = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(objs[0]["location"].ToString()));
                    JObject conju = JsonConvert.DeserializeObject<JObject>(_locationTable.GetRow(locat["parent"].ToString()));
                    cine = conju["name"].ToString();

                    try
                    {
                        domicilio = conju["_HTKFieldcalle"].ToString();
                    }
                    catch (Exception ex) { }
                    try
                    {
                        domicilio = domicilio + " " + conju["_HTKFieldcolonia"].ToString();
                    }
                    catch (Exception ex) { }
                    try
                    {
                        domicilio = domicilio + " " + conju["_HTKFieldMunicipio"].ToString();
                    }
                    catch (Exception ex) { }
                    try
                    {
                        domicilio = domicilio + ". " + conju["_HTKFieldciudad"].ToString();
                    }
                    catch (Exception ex) { }
                    try
                    {
                        domicilio = domicilio + " C.P." + conju["_HTKFieldzipcode"].ToString();
                    }
                    catch (Exception ex) { }
                    try
                    {
                        domicilio = domicilio + " " + conju["_HTKFieldestado"].ToString();
                    }
                    catch (Exception ex) { }
                    try
                    {
                        domicilio = domicilio + ", " + conju["_HTKFieldpais"].ToString();
                    }
                    catch (Exception ex) { }

                    //domicilio = conju["_HTKFieldcalle"].ToString() + " " + conju["_HTKFieldcolonia"].ToString() + " " +
                    //    conju["_HTKFieldMunicipio"].ToString() + ". " + conju["_HTKFieldciudad"].ToString() + " C.P." +
                    //    conju["_HTKFieldzipcode"].ToString() + " " +
                    //    conju["_HTKFieldestado"].ToString() + ", " + conju["_HTKFieldpais"].ToString();
                    idlocation = conju["_id"].ToString();
                }
                
                String documento = "<table><tr bgcolor='#336699'><td></td></tr><tr><td><span style='text-align:center;'>ACTA ADMINISTRATIVA DE DESTRUCCIÓN DE ACTIVOS FIJOS</span><br/><span style='text-align:center;'>F-DICA-20 v. 4.0.1</span><br/><br/></td></tr>"
                + "<tr><td>Siendo las <b>" + demandobj["actInfo"]["hour"] + "</b> horas del día <b>" + demandobj["actInfo"]["date"] + "</b> se procedió a destruir los Activos Fijos propiedad de <b>" + demandobj["actInfo"]["prop"]
                + "</b> en donde se encontraban ubicados físicamente, en el Conjunto Cinematográfico " + cine + " con domicilio en <b>" + domicilio + "</b>. <br/>"
                + "Los activos fijos se destruyen según el dictámen técnico establecido en el formato F-DICA-11, según la evaluación de los técnicos especialistas de <b>" + tipoactivo + "</b><br/>"
                + " y expedida por <b>" + dictaminador + "</b> el día <b>" + fechadictamen + "</b>. <br/><br/>"
                + "<span style='font-weight:bold;'>Detalle de los activos fijos (Anexo 2), sistemas CAFI</span> <br/><br/>"
                + "<table border='1' width:100% ><tr bgcolor='#336699' align='center'><th><font color='white'>Cantidad</font></th><th><font color='white'>Descripcion</font></th><th><font color='white'>Tipo de Obsolescencia</font></th></tr>";
                foreach (JObject ob1 in objs)
                {
                    if (ob1["denied_user"] != null) continue;
                    String refarray = _objectTable.GetRow(ob1["id"].ToString());
                    JObject refobj = JsonConvert.DeserializeObject<JObject>(refarray);
                    documento = documento + "<tr><td>1</td><td>" + refobj["name"] + "</td>";
                    documento = documento + "<td>" + ob1["typeObs"].ToString().Replace("[", "").Replace("]", "") + "</td>"; 
                    documento = documento + "</tr>";
                }
                documento = documento + "</table><br/><br/>"
                + "Para efectos de cumplimiento a las disposiciones Fiscales que aplican al rubro de Activos, se deja constancia de la "
                + "solicitud y autorización de la destrucción en el Anexo 2, que cumple con el proceso de Control Interno para la baja "
                + "de Activos de la Contabilidad de la Compañía; mismo que indica Cantidad, Descripción y el conjunto en que fue utilizada "
                + "durante su vida útil. <br/><br/>"
                + "<span style='font-weight:bold;'>Detalles de los activos fijos destruidos (Anexo 3), ANTES, DURANTE Y DESPUÉS </span> <br/>"
                + "<span style='font-weight:bold;'>Notas y observaciones derivadas de la destrucción :</span> <br/>"
                + "<ol type='A'>"
                + "<li> La \"chatarra\" que generó la destrucción de este equipo fue vendida como \"fierro viejo\" en <b>" + vendido + "</b> por"
                + " lo que fue retirada de los compradores, los cuales se depositan en la cuenta de la empresa <span style='font-weight:bold;'>(ANEXO 4)</span>.</li>"
                + "<li> Para efectos de los requisitos contables de la empresa, se le dará el tratamiento a lo dispuesto en el art. 37 párrafo 6° "
                + "de la LISR el cual menciona que \"...en caso de los bienes dejen de ser útiles para obtener los ingresos, el contribuyente "
                + "deberá mantener sin deducción un pesos en sus registros...\"</li></ol><br/>"
                + "La diligencia de destrucción se realiza de conformidad con las disposiciones fiscales en presencia de las áreas participantes, firmando"
                + " de conformidad al calce de la hoja."
                + "</td></tr>"
                + "<tr><td>"
                + "<span style='font-weight:bold;text-align:center;'>ANEXO 2 - Reporte en CAFI de autorización de baja.</span><br/>"
                + "<span style='text-align:center;'>Registro de baja</span><br/>"
                + "<span style='text-align:center;'>Detalle del Movimiento</span><br/><br/>"
                + "<table border='1'><tr bgcolor='#336699'><th align='center'><font color='white'>No</font></th><th><font color='white'>Descripcion</font></th><th><font color='white'>Marca</font></th><th><font color='white'>Modelo</font></th><th><font color='white'>Serie</font></th><th><font color='white'>EPC</font></th></tr>";
                foreach (JObject ob1 in objs)
                {
                    if (ob1["denied_user"] != null) continue;

                    String refarray = _objectTable.GetRow(ob1["id"].ToString());
                    JObject refobj = JsonConvert.DeserializeObject<JObject>(refarray);

                    documento = documento + "<tr><td>1</td><td>" + refobj["name"] + "</td>";
                    try
                    {
                        documento = documento + "<td>" + refobj["marca"] + "</td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td>" + refobj["modelo"] + "</td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td>" + refobj["serie"] + "</td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }
                    try
                    {
                        documento = documento + "<td>" + refobj["EPC"] + "</td>";
                    }
                    catch
                    {
                        documento = documento + "<td></td>";
                    }

                    documento = documento + "</tr>";
                }
                documento = documento + "</table>"
                + "</td></tr>"
                + "<tr><td>"
                + "<span style='font-weight:bold;text-align:center;'>ANEXO 3 - Archivo fotográfico</span><br/><br/>"
                + "<table border='1'><tr align='center' bgcolor='#336699'><th><font color='white'>Activo</font></th><th><font color='white'>Descripcion</font></th></tr>";
                foreach (JObject ob1 in objs)
                {
                    String refarray = _objectTable.GetRow(ob1["id"].ToString());
                    JObject refobj = JsonConvert.DeserializeObject<JObject>(refarray);

                    documento = documento + "<tr><td>" + refobj["name"] + "</td><td></td></tr>";
                    JArray images = JsonConvert.DeserializeObject<JArray>(ob1["imgDestrucion"].ToString());
                    for (int i = 0; i < images.Count; i++)
                    {
                        if (images[i].ToString() != "") {
                            if (System.IO.File.Exists(Server.MapPath("~") + "/Uploads/Dictamenes/imagenes/" + images[i]))
                            {
                                documento = documento + "<tr><td>" + fotos[i] + "</td><td align='center'><img height=50 width=50 src='" + Server.MapPath("~") + "/Uploads/Dictamenes/imagenes/" + images[i] + "' alt='logo'></td></tr>";
                    }
                            else {
                                documento = documento + "<tr><td>" + fotos[i] + "</td><td></td></tr>";
                            }
                            
                        }
                        
                    }

                }

                List<String> str1 = new List<String>();
                foreach (string data in demandobj["receiptFile"]) {
                    str1.Add(data);
                }

                JObject testigo1 = new JObject(), testigo2 = new JObject(), testigo3 = new JObject();
                String gerenteconjunto = "";
                String testigos = _demandTable.GetWitnesses();
                JArray testigosobjs = JsonConvert.DeserializeObject<JArray>(testigos);


                gerenteconjunto = GetGerenteConjunto(idlocation);
                JObject gerente = JsonConvert.DeserializeObject<JObject>(gerenteconjunto);
                foreach (JObject ob2 in testigosobjs)
                {
                    JObject userInfo = JsonConvert.DeserializeObject<JObject>(_userTable.GetRow(ob2["user"].ToString()));
                    if (ob2["witnessType"].ToString() == "operative")
                    {
                        testigo1["name"] = ob2["CreatorName"].ToString() + " " + ob2["CreatorLastName"].ToString();
                        testigo1["userKey"] = userInfo["userKey"];
                    }
                    if (ob2["witnessType"].ToString() == "assets")
                    {
                        testigo2["name"] = ob2["CreatorName"].ToString() + " " + ob2["CreatorLastName"].ToString();
                        testigo1["userKey"] = userInfo["userKey"];
                    }
                    if (ob2["witnessType"].ToString() == "accounting")
                    {
                        testigo3["name"] = ob2["CreatorName"].ToString() + " " + ob2["CreatorLastName"].ToString();
                        testigo1["userKey"] = userInfo["userKey"];
                    }
                }

                documento = documento + "</table>"
                + "</td></tr>"
                + "<tr><td>"
                + "<span style='font-weight:bold;text-align:center;'>ANEXO 4 - Ingresos Obtenidos por venta de chatarra</span><br/><br/>"
                + "<table border='1'><tr><th>COMPROBANTE DE ENTREGA DEL COMPRADOR</th></tr>";

                if (str1 != null && str1[0].ToString() != "" ) { 
                     documento = documento   + "<tr><td align='center' valign='middle'><img width=300 src='" + Server.MapPath("~") + "/Uploads/Dictamenes/documentos/" + str1[0] + "' alt='logo'></td></tr>";
                }
                else{
                    documento = documento + "<tr><td></td></tr>";
                }
                documento = documento + "</table><br/>"
                + "<table border='1'><tr><th>FICHA DE DEPÓSITO</th></tr>";

                if (str1 != null && str1[1].ToString() != "")
                {
                    documento = documento + "<tr><td align='center' valign='middle'><img width=300 src='" + Server.MapPath("~") + "/Uploads/Dictamenes/documentos/" + str1[1] + "' alt='logo'></td></tr>";
                }
                else
                {
                    documento = documento + "<tr><td></td></tr>";
                }
                documento = documento + "<br></table>"
                + "</td></tr>"

                 + "<tr><td height=30><br/>"
                + "<span style='font-weight:bold;'>TESTIGOS</span><br/><br/><br/><br/>"
                + "<table>"
                    + "<tr>"
                        + "<td width='40%'>"
                        + "<span style='font-weight:bold;text-align:center;'>Por parte de la Gerencia de Auditoría Operativa y Financiera</span>"
                        + "<br/><br/><font size=2><u>" + testigo1["userKey"] + "</u></font><br/>"
                            + "<span style='text-align:center;'>" + testigo1["name"] + "</span><br/><span style='text-align:center;'>Gerente de Auditoría</span> "
                        + "</td>"
                        + "<td width='10%'></td>"
                         + "<td width='40%'>"
                         + "<span style='font-weight:bold;text-align:center;'>Por parte del Conjunto</span>"
                        + "<br/><br/><font size=2><u>" + gerente["userKey"] + "</u></font><br/><span style='text-align:center;'>" + gerente["name"] + " " + gerente["lastname"] + "</span><br/><span style='text-align:center;'>Gerente de Conjunto</span> "
                         + "</td>"
                    + "</tr>"
                    + "<tr><td><br/><br/><br/></td><td></td><td></td></tr>"
                    + "<tr>"
                         + "<td width='40%'>"
                            + "<span style='font-weight:bold;text-align:center;'>Por parte de la Dirección de Inmuebles y Control de Activos</span>"
                        + "<br/><br/><font size=2><u>" + testigo1["userKey"] + "</u></font><br/><span style='text-align:center;'>" + testigo2["name"] + "</span><br/><span style='text-align:center;'>Director de Inmuebles y Control de Activos</span> "
                        + "</td>"
                       + "<td width='10%'></td>"
                       + "<td width='40%'>"
                           + "<span style='font-weight:bold;text-align:center;'>Por parte de la Dirección de Contabilidad</span>"
                          + "<br/><br/><font size=2><u>" + testigo1["userKey"] + "</u></font><br/><span style='text-align:center;'>" + testigo3["name"] + "</span><br/><span style='text-align:center;'>Gerente Fiscal</span> "

                        + "</td>"
                    + "</tr>"
                + "</table>"
                + "</td></tr>"
                + "</table>";

                doc.Open();
                HTMLWorker htmlWorker = new HTMLWorker(doc);
                StringReader sr = new StringReader(documento);
                //XMLWorkerHelper.GetInstance().ParseXHtml(
                //    writer, document, html
                //  );
                htmlWorker.Parse(sr);
                doc.Close();
                return folio;

            }
            catch (Exception ex)
            {
                return null;
            }

        }


        public String generateActaFolio(string demandid)
        {

            string folio = "000001";
            if (this.Request.IsAjaxRequest())
            {
                int ultimofolio = 1;
                string num = "";
                String rowArray = _demandTable.GetAutorizations("destinyOptions", "destruccion", 0);
                JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);
                List<int> listfolios = new List<int>();
                if (objects.Count > 0)
                {
                    
                   
                    foreach (JObject obj in objects)
                    {
                        JToken n2;
                        if (obj.TryGetValue("ActFolio", out n2))
                        {
                            int.TryParse(obj["ActFolio"].ToString().Replace("ACT-", ""), out ultimofolio);
                            listfolios.Add(ultimofolio);
                        }
                    }

                    folio = "";
                    if (listfolios.Count() > 0)
                    {
                        ultimofolio = listfolios.Max();
                        ultimofolio = ultimofolio + 1;
                    }
                    else ultimofolio = 1;
                    num = ultimofolio.ToString();

                    for (int i = 0; i < 6 - num.Length; i++)
                    {
                        folio = folio + "0";
                    }
                    folio = folio + num;
                }

            }

            return "ACT-" + folio;
        }

        /// <summary>
        ///     Allows to create the temporal movement pdf
        /// </summary>
        /// <param name="demandid">The demand's id to generate</param>
        /// <returns>The generated file name</returns>
        /// <author>Quijada Romero Luis Gonzalo</author>
        public ActionResult generateTempPDF(String demandid)
        {
            if (demandid == null) {
                return null;
            }
            String demandString = _demandTable.GetRow(demandid);
            JObject demand = JsonConvert.DeserializeObject<JObject>(demandString);

            String folio = "";

            try
            {
                folio = demand["staFolio"].ToString();
                //return folio;
            }
            catch
            {
                folio = generateSTAFolio(demandid);
                demand["staFolio"] = folio;
                demand.Remove("_id");
                _demandTable.SaveRow(JsonConvert.SerializeObject(demand), demandid);
                _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "Demand", DateTime.Now.ToString());
            }

            string file = folio + ".pdf";
            string pdfurl = Server.MapPath("~") + "\\Uploads\\Dictamenes\\documentos\\" + file;
            string relativepath = "\\Uploads\\Dictamenes\\documentos\\";
            string absoluteurl = Server.MapPath(relativepath);

            if (!System.IO.Directory.Exists(absoluteurl))
            {
                System.IO.Directory.CreateDirectory(absoluteurl);
            }

            JArray newObjects = new JArray();
            JArray newObjects2 = new JArray();
            String descripciones = "";
            int cont = 1;
            JArray returnedObjectsNew = new JArray();
            try
            {
                foreach (JObject obj in demand["objects"])
                {
                    try
                    {
                        if (obj["denied_note"].ToString() != "")
                            continue;
                    }
                    catch
                    {

                    }
                    if (obj["entry"].ToString() != "1")
                    {
                        try { descripciones = descripciones + cont.ToString() + ". " + obj["repaired_motive"].ToString() + " ";
                        cont++;
                        }
                        catch { descripciones = descripciones+""; }

                        returnedObjectsNew.Add(obj);
                    }
                }
            }
            catch (Exception e) { }
            try {
                foreach (JObject obj in demand["objects"] as JArray)
                {
                    try
                    {
                        if (obj["denied_note"].ToString() != "")
                            continue;
                    }
                    catch
                    {

                    }

                    if (obj["entry"].ToString() == "1")
                    {
                        String objestring = _objectTable.GetRow(obj["id"].ToString());
                        JObject obje = JsonConvert.DeserializeObject<JObject>(objestring);
                        obj["object_id"] = obje["object_id"];
                        obj["EPC"] = obje["EPC"].ToString();
                        try { if (obj["name"].ToString() == "") obj["name"] = obje["name"]; }
                        catch (Exception ex) { obj["name"] = obje["name"]; }

                        try { if (obj["serie"].ToString() == "") obj["serie"] = obje["serie"]; }
                        catch (Exception ex) { obj["serie"] = obje["serie"]; }

                        try
                        {
                            descripciones = descripciones + cont.ToString() + ". " + obj["repaired_motive"].ToString() + " ";
                            cont++;
                        }
                        catch { descripciones = descripciones + ""; }


                        newObjects.Add(obj);
                    }

                }
            }
            catch (Exception e) { }

            try
            {
                foreach (JObject obj in demand["objects"] as JArray)
                {
                    try
                    {
                        if (obj["denied_note"].ToString() != "")
                            continue;
                    }
                    catch
                    {

                    }

                        String objestring = _objectTable.GetRow(obj["id"].ToString());
                        JObject obje = JsonConvert.DeserializeObject<JObject>(objestring);
                        obj["object_id"] = obje["object_id"];
                        obj["EPC"] = obje["EPC"].ToString();
                        try { if (obj["name"].ToString() == "") obj["name"] = obje["name"]; }
                        catch(Exception ex){obj["name"] = obje["name"];}
                        obj["name_old"] = obje["name"];

                         try { if (obj["serie"].ToString() == "") obj["serie"] = obje["serie"]; }
                         catch (Exception ex) { obj["serie"] = obje["serie"]; }
                        obj["serie_old"] = obje["serie"];

                        newObjects2.Add(obj);
                    

                }
            }
            catch (Exception e) { }
            demand["objects"] = newObjects2;
            demand["newobjects"] = newObjects;
            JObject data = new JObject();
            String idLocation = demand["objects"].First["location"].ToString();
            String locationString = _locationTable.GetRow(idLocation);
            JObject location = JsonConvert.DeserializeObject<JObject>(locationString);

            locationString = _locationTable.GetRow(location["parent"].ToString());
            JObject conjunto = JsonConvert.DeserializeObject<JObject>(locationString);

            location["IDconjunto"] = conjunto["number"].ToString();
            location["locationRoute"] = getRouteNames(idLocation);
            data["location"] = location;
            demand["description"] = descripciones;
            data["demand"] = demand;

            
            data["returnedObjects"] = returnedObjectsNew;

           

            String managerString = GetGerenteConjunto(idLocation);
            JObject manager = JsonConvert.DeserializeObject<JObject>(managerString);
            data["manager"] = manager;

            String creatorId = demand["Creator"].ToString();
            String creatorString = _userTable.GetRow(creatorId);
            JObject creator = JsonConvert.DeserializeObject<JObject>(creatorString);
            data["creator"] = creator;
            try
            {
            String voboId = (demand["approval"] as JArray).First["id_user"].ToString();
            String voboUserString = _userTable.GetRow(voboId);
            JObject voboUser = JsonConvert.DeserializeObject<JObject>(voboUserString);
            data["voboUser"] = voboUser;
            }
            catch (Exception ex) { }

            //     var pdffile = new RazorPDF.PdfResult(data, "generateTempPDF",);

            return new RazorPDF.PdfResult(data, "generateTempPDF");
        }

        public ActionResult getExample() {
            return new RazorPDF.PdfResult(null,"getExample");
        }

        public String getRelatedDemandsData(String filter) {
            JArray demandsList = new JArray();
            if (filter != "ALTA") {
                String deleteString = _movementProfileTable.Get("typeMovement", "delete");
                JArray delete = JsonConvert.DeserializeObject<JArray>(deleteString);
                foreach (JObject deleteObject in delete)
                {
                    demandsList.Add(deleteObject);
                }
            }
            if (filter != "BAJA") {
                String createString = _movementProfileTable.Get("typeMovement", "create");
                JArray create = JsonConvert.DeserializeObject<JArray>(createString);
                foreach (JObject createObjet in create) {
                    demandsList.Add(createObjet);
                }
            }

            JArray result = new JArray();
            foreach (JObject movement in demandsList) {
                String movementsString = _demandTable.Get("movement",movement["_id"].ToString());
                JArray movements = JsonConvert.DeserializeObject<JArray>(movementsString);
                foreach (JObject demand in movements) {
                    JObject newObject = new JObject();
                    newObject["_id"] = demand["_id"].ToString();
                    newObject["folio"] = demand["folio"].ToString();
                    newObject["CreatedDate"] = demand["CreatedDate"].ToString();
                    newObject["type"] = movement["typeMovement"].ToString() == "create" ? "ALTA" : "BAJA";

                    String objectsString = "";
                    foreach (JObject obj in demand["objects"] as JArray) {
                        objectsString += obj["id"] + ",";
                    }
                    objectsString = objectsString.Remove(objectsString.Length-1);
                    newObject["objects"] = objectsString;
                    result.Add(newObject);
                }
            }

            result = new JArray(result.OrderBy(obj => obj["folio"]));
            String resultString = JsonConvert.SerializeObject(result); ;
            return resultString;
        }

        public String getlastMovements(String userid) {
            String userString = _userTable.GetRow(userid);
            JObject userobj = JsonConvert.DeserializeObject<JObject>(userString);
            try
            {
                JArray movs = JsonConvert.DeserializeObject<JArray>(userobj["movementsConsulted"].ToString());
                String row = "";

                foreach (JObject mov in movs)
                {
                    row = row + "<tr ><td></td><td><label >" + mov["folio"].ToString() + "</label></td>" +
                    "<td>" + mov["movement"] + "</td>" +
                    "<td>" + mov["status"] + "</td>" +
                    "<td>" + mov["CreatorLastName"] + " " + mov["CreatorName"] + "</td>" +
                    "<td>" + mov["DateCreated"] + "</td>" +
                    "<td><input value='Ver' class='btn light_blue verdet' type='button'' data-iddemand='" +
                    mov["id"].ToString() + "' onclick=Verdetalles2('" + mov["id"].ToString() + "')></td>" +
                    "</tr>";
                }

                return row;
            }
            catch (Exception ex) {
                return "";
            }
        }


        //******************************
        public String getProfileUser(String userid)
        {
            String perfil = "";
            try
            {
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);

                String profilestring = _userProfileTable.GetRow(userobj["profileId"].ToString());
                JObject profileobj = JsonConvert.DeserializeObject<JObject>(profilestring);
                perfil=profileobj["name"].ToString();
            }
            catch
            {
            }
            return perfil;
        }

        public String getNameUser(String userid)
        {
            String name = "";
            try
            {
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);

                name = userobj["name"].ToString() +" "+ userobj["lastname"].ToString();
            }
            catch
            {
            }
            return name;
        }

        public String getUserKey(String userid)
        {
            String name = "";
            try
            {
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);

                name = userobj["userKey"].ToString();
            }
            catch
            {
            }
            return name;
        }

        public String getNameCategory(String parentCategory)
        {
            String name = "";
            try
            {
                String objstring = _categoryTable.GetRow(parentCategory);
                JObject obj = JsonConvert.DeserializeObject<JObject>(objstring);

                name = obj["name"].ToString();
            }
            catch
            {
            }
            return name;
        }


        public String saveLastMovement(String userid, String demand) {
            String userString = _userTable.GetRow(userid);
            JObject userobj = JsonConvert.DeserializeObject<JObject>(userString);
            JObject demandobj = JsonConvert.DeserializeObject<JObject>(demand);
            JArray demandarray = new JArray();
            try{
            demandarray=JsonConvert.DeserializeObject<JArray>(userobj["movementsConsulted"].ToString());
                if(demandarray.Count<5){
                    demandarray.AddFirst(demandobj);
                }else{
                    demandarray.RemoveAt(4);
                    demandarray.AddFirst(demandobj);
                }
            }
            catch(Exception ex){
             demandarray.Add(demandobj);
            }
            
            userobj["movementsConsulted"] = demandarray;
            try
            {
                _userTable.SaveRow(JsonConvert.SerializeObject(userobj), userid);
            //    _logTable.SaveLog(Session["_id"].ToString(), "Movimientos", "Update: Folio #" + demand["folio"], "User", DateTime.Now.ToString());
                return "success";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public void loadLocationsConjuntoAlls()
        {

            try
            {
                String locationsOptions = "";
                string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
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

                locationsOptions += "<option value='null' selected> Seleccione un Conjunto</option>";

                foreach (JObject document in locatList) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["name"].ToString() != "")
                    {
                        locationsOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                        locationsOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                    }

                }

                ViewData["locationsList"] = new HtmlString(locationsOptions);
            }
            catch (Exception e)
            {
                ViewData["locationsList"] = null;
            }
        }

        public string loadLocationsConjuntoAlls2()
        {

            try
            {
                String locationsOptions = "";
                string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
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

                locationsOptions += "<option value='null' selected> Seleccione un Conjunto</option>";

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

        public void loadLocationsConjuntoUser(String userid)
        {

            try
            {
                String locationsOptions = "";
                JArray locats = new JArray();
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray conjuntos = new JArray();

                locationsOptions += "<option value='null' selected> Seleccione Conjunto</option>";
                locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getperfil = _userProfileTable.Get("name", "Gerente regional");
                JArray perfiles = new JArray();
                string idperfil = "";
                try
                {
                    perfiles = JsonConvert.DeserializeObject<JArray>(getperfil);
                    idperfil = (from mov in perfiles select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                if (userobj["profileId"].ToString() == idperfil)
                {
                    string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
                    JArray conjuntja = new JArray();
                    string idprof = "";
                    try
                    {
                        conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                        idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                    }
                    catch (Exception ex) { }

                    getconjunt = locationsProfilesdb.Get("name", "Region");
                    conjuntja = new JArray();
                    String idprof2 = "";
                    try
                    {
                        conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                        idprof2 = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                    }
                    catch (Exception ex) { }


                    JArray locatList = new JArray();
                    String rowArray;
                    List<String> list1 = new List<String>();
                    List<String> list2 = new List<String>();
                    rowArray = _locationTable.Get("profileId", idprof);
                    locatList = JsonConvert.DeserializeObject<JArray>(rowArray);
                    JArray ele = new JArray();
                    foreach (JObject ob in locatList)
                    {
                        list1.Add(ob["_id"].ToString());
                    }

                    rowArray = _locationTable.Get("profileId", idprof2);
                    locatList = JsonConvert.DeserializeObject<JArray>(rowArray);
                    foreach (JObject ob in locatList)
                    {
                        list2.Add(ob["_id"].ToString());
                    }

                    foreach (JObject ob in locats)
                    {
                        if (list2.Contains(ob["id"].ToString()))
                        {
                            rowArray = _locationTable.GetLocations(idprof, ob["id"].ToString());
                            locatList = JsonConvert.DeserializeObject<JArray>(rowArray);
                            foreach (JObject ob1 in locatList)
                            {
                                locationsOptions += "<option value='" + ob1["_id"] + "'>" + ob1["name"] + "</option>";

                            }
                        }
                        else {
                            if (list1.Contains(ob["id"].ToString()))
                                locationsOptions += "<option value='" + ob["id"] + "'>" + ob["name"] + "</option>";
                            else
                            {
                                ele = getRoute3(ob["id"].ToString());
                                foreach (String ob2 in ele)
                                {
                                    if (list1.Contains(ob2))
                                    {
                                        locationsOptions += "<option value='" + ob["id"] + "'>" + ob["name"] + "</option>";
                                        break;
                                    }
                                }
                            }
                        
                        }
                          
                    }

                }
                else {
                    string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
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
                    List<String> list1 = new List<String>();
                    List<String> list2 = new List<String>();
                    foreach (JObject ob in locatList)
                    {
                        list1.Add(ob["_id"].ToString());
                    }

                    JArray ele = new JArray();
                    foreach (JObject cad1 in locats)
                    {
                        if (list1.Contains(cad1["id"].ToString()))
                            locationsOptions += "<option value='" + cad1["id"] + "'>" + cad1["name"] + "</option>";
                        else
                        {
                            ele = getRoute3(cad1["id"].ToString());
                            foreach (String ob in ele)
                            {
                                if (list1.Contains(ob))
                                {
                                    locationsOptions += "<option value='" + cad1["id"] + "'>" + cad1["name"] + "</option>";
                                    break;
                                }
                            }
                        }
                    }
                }




                ViewData["locationsList"] = new HtmlString(locationsOptions);
            }
            catch (Exception e)
            {
                ViewData["locationsList"] = null;
            }
        }

        public String locationsUser(String userid) {
            try
            {
                String locationsOptions = "";
                JArray locats = new JArray();
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray conjuntos = new JArray();

                String profstring= _userProfileTable.GetRow(userobj["profileId"].ToString());
                JObject profobj = JsonConvert.DeserializeObject<JObject>(profstring);

                locationsOptions += "<option value='null' selected> Seleccione Conjunto</option>";
                locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());
                if (profobj["name"].ToString() == "Gerente regional")
                {
                    string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
                    JArray conjuntja = new JArray();
                    string idprof = "";
                    try
                    {
                        conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                        idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                    }
                    catch (Exception ex) { }

                    getconjunt = locationsProfilesdb.Get("name", "Region");
                    conjuntja = new JArray();
                    String idprof2 = "";
                    try
                    {
                        conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                        idprof2 = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                    }
                    catch (Exception ex) { }


                    JArray locatList = new JArray();
                    String rowArray;
                    List<String> list1 = new List<String>();
                    List<String> list2 = new List<String>();
                    rowArray = _locationTable.Get("profileId", idprof);
                    locatList = JsonConvert.DeserializeObject<JArray>(rowArray);
                    JArray ele = new JArray();
                    foreach (JObject ob in locatList)
                    {
                        list1.Add(ob["_id"].ToString());
                    }

                    rowArray = _locationTable.Get("profileId", idprof2);
                    locatList = JsonConvert.DeserializeObject<JArray>(rowArray);
                    foreach (JObject ob in locatList)
                    {
                        list2.Add(ob["_id"].ToString());
                    }

                    foreach (JObject ob in locats)
                    {
                        if (list2.Contains(ob["id"].ToString()))
                        {
                            rowArray = _locationTable.GetLocations(idprof, ob["id"].ToString());
                            locatList = JsonConvert.DeserializeObject<JArray>(rowArray);
                            foreach (JObject ob1 in locatList)
                            {
                                locationsOptions += "<option value='" + ob1["_id"] + "'>" + ob1["name"] + "</option>";

                            }
                        }
                        else
                        {
                            if (list1.Contains(ob["id"].ToString()))
                                locationsOptions += "<option value='" + ob["id"] + "'>" + ob["name"] + "</option>";
                            else
                            {
                                ele = getRoute3(ob["id"].ToString());
                                foreach (String ob2 in ele)
                                {
                                    if (list1.Contains(ob2))
                                    {
                                        locationsOptions += "<option value='" + ob["id"] + "'>" + ob["name"] + "</option>";
                                        break;
                                    }
                                }
                            }

                        }

                    }

                }
                else if (profobj["name"].ToString() == "Administrador de sistema")
                {
                    string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
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

                    locationsOptions += "<option value='null' selected> Seleccione un Conjunto</option>";

                    foreach (JObject document in locatList) //for each profile we create an option element with id as value and the name as the text
                    {
                        if (document["name"].ToString() != "")
                        {
                            locationsOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                            locationsOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                        }

                    }
                }
                else
                {
                    string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
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
                    List<String> list1 = new List<String>();
                    List<String> list2 = new List<String>();
                    foreach (JObject ob in locatList)
                    {
                        list1.Add(ob["_id"].ToString());
                    }

                    JArray ele = new JArray();
                    foreach (JObject cad1 in locats)
                    {
                        if (list1.Contains(cad1["id"].ToString()))
                            locationsOptions += "<option value='" + cad1["id"] + "'>" + cad1["name"] + "</option>";
                        else
                        {
                            ele = getRoute3(cad1["id"].ToString());
                            foreach (String ob in ele)
                            {
                                if (list1.Contains(ob))
                                {
                                    locationsOptions += "<option value='" + cad1["id"] + "'>" + cad1["name"] + "</option>";
                                    break;
                                }
                            }
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

        public String getHomeLocation(String userid)
        {
            String home = "";
            try
            {
                String userstring = _userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);

                JArray cads = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());
                foreach(JObject ob in cads){
                    if (ob["name"].ToString() == "Home") {
                        home = "si"; break;
                    }
                }
            }
            catch
            {
                home = "";
            }
            return home;
        }

        public string getPropietarioAndCuentaInfo(string type) {
            JObject result = new JObject();
            String PropietarioOptions = "";
            String CuentaOptions = "";
            try
            {
                String rowArray = _listTable.Get("name", "propietario");
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray listas = new JArray();
                foreach (JObject obj in rowString)
                {
                    listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["unorder"].ToString());
                }
                PropietarioOptions += "<option value='null' selected> Seleccione Propietario</option>";
                foreach (JObject info in listas)
                {
                    foreach (KeyValuePair<string, JToken> token in info)
                    {
                        PropietarioOptions += "<option value='" + token.Key + "'"; //setting the id as the value
                        PropietarioOptions += ">" + token.Value + "</option>"; //setting the text as the name
                    }
                }
            }
            catch { }
            result.Add("propietario", PropietarioOptions);

            if (type == "venta") {
                try
                {
                    String rowArray = _listTable.Get("name", "cuenta_deposito");
                    JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                    JArray listas = new JArray();
                    foreach (JObject obj in rowString)
                    {
                        listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["unorder"].ToString());
                    }
                    CuentaOptions += "<option value='null' selected> Seleccione Cuenta de Deposito</option>";
                    foreach (JObject info in listas)
                    {
                        foreach (KeyValuePair<string, JToken> token in info)
                        {
                            CuentaOptions += "<option value='" + token.Key + "'"; //setting the id as the value
                            CuentaOptions += ">" + token.Value + "</option>"; //setting the text as the name
                        }
                    }
                }
                catch { }
            }

            result.Add("cuenta_deposito", CuentaOptions);
            return JsonConvert.SerializeObject(result);
        }
    }
}
