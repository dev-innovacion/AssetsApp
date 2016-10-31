using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RivkaAreas.Tags.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Rivka.Api;
using Rivka.Mail;
using Rivka.Security;
using RivkaAreas.Rule;
using System.Threading.Tasks;

namespace RivkaAreas.Tags.Controllers
{

    [Authorize]
    public class LabelingController : Controller
    {
        //
        // GET: /Tags/Labeling/
        protected Notifications classNotifications;
        protected LocationTable locationTable;
        protected UserProfileTable userprofileTable;
        protected UserTable userTable;
        protected ObjectTable objectTable;
        protected ObjectReal objectRealTable;
        protected Notifications notificationObject;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        public LabelingController()
        {
            locationTable = new LocationTable();
            userprofileTable = new UserProfileTable();
            userTable = new UserTable();
            objectTable = new ObjectTable();
            objectRealTable = new ObjectReal();
            notificationObject = new Notifications();
            classNotifications = new Notifications();
            _logTable = new LogBook.Controllers.LogBookController();
        }

        public ActionResult Index()
        {


            return View();
        }

        public ActionResult LocationModal()
        {
            return View();
        }

        /// <summary>
        ///     Save new objects in the system 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="location"></param>
        /// <param name="user"></param>
        /// <param name="objname"></param>
        /// <param name="cantidad"></param>
        /// <param name="EPC"></param>
        public void saveObjects(string obj, string location, string user, string objname, int cantidad, String EPC)
        {
            String cadena = "";
            String name = "";
            String userid = user;
            if (location == null || location == "")
            {
                location = "null";
            }

            //  JObject objref = JsonConvert.DeserializeObject<JObject>(objString);
            name = objname;
            int consecutivo = int.Parse(EPC.Substring(15), System.Globalization.NumberStyles.HexNumber);
            string hexValue;
            int counter = 0;
            for (int i = 0; i < cantidad; i++)
            {
                try
                {
                    hexValue = consecutivo.ToString("X10");
                    cadena = "{'objectReference':'" + obj + "','name':'" + name + "','location':'" + location + "','userid':'" + userid + "','process':null"
                                + ", 'EPC':'" + EPC.Substring(0, 14) + hexValue + "'}";
                    objectRealTable.SaveRow(cadena, "null");
                    consecutivo++;
                    counter++;
                }
                catch (Exception e) { }
            }
            if (counter > 0)
            {
                notificationObject.saveNotification("Objects", "Create", "Se han impreso " + counter + " etiquetas");
                _logTable.SaveLog(user, "Etiquetado", "Insert: Se han impreso " + counter + " etiquetas", "ObjectReal", DateTime.Now.ToString());
            }


        }

        /// <summary>
        ///     Gets the devices connected to Edgeware (Printers and Antennas)
        /// </summary>
        /// <returns>
        ///     A json string with the result { antenna: "Antena1", printer: "Printer 1"}
        ///     or {} if no devices were found
        /// </returns>
        public String GetDevices()
        {
            autobuildHTK_client.Rivka.Api.PrintServiceClient PrintService = new autobuildHTK_client.Rivka.Api.PrintServiceClient();

            autobuildHTK_client.Rivka.Api.AntennaClass[] antennasSource;
            string printer = "";
            JObject devices = new JObject();
            JArray antennas = new JArray();

            try
            {
                antennasSource = PrintService.GetAntenna();
                foreach (autobuildHTK_client.Rivka.Api.AntennaClass antenna in antennasSource)
                {
                    JObject newAntena = new JObject();
                    newAntena.Add("model", antenna.Model);
                    newAntena.Add("ip", antenna.IP);
                    antennas.Add(newAntena);
                }
            }
            catch (Exception e) { }

            try { printer = PrintService.GetPrinter(); }
            catch (Exception e) { }

            devices.Add("antenna", antennas);

            if (printer != "")
                devices.Add("printer", printer);

            return devices.ToString();
        }

        /// <summary>
        ///     Scan and read an Antenna
        /// </summary>
        /// <param name="ip">
        ///     The IP address of the Antenna
        /// </param>
        /// <returns></returns>
        public String ScanAntenna(string ip)
        {
            autobuildHTK_client.Rivka.Api.PrintServiceClient PrintService = new autobuildHTK_client.Rivka.Api.PrintServiceClient();

            string[] read = { };

            //Try to read an Antenna
            try { read = PrintService.Read(ip); }
            catch (Exception e) { }

            return JsonConvert.SerializeObject(read);
        }

        /// <summary>
        ///     Send to print labels to an available printer and save in the sytem
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public String PrintLabel(String data)//String objid, String quantity, String location)
        {

            JArray jsondata = new JArray();
            try
            {
                jsondata = JsonConvert.DeserializeObject<JArray>(data);
            }
            catch (Exception ex) { return null; }
            String cadena = "";
            String tipo = "";
            JObject objcad = new JObject();

            foreach (JObject row in jsondata)
            {
                int labels = 0;
                int quant = 0;
                String objid = "";
                String objname = "";
                String quantity = "";
                String location = "";
                String userid = "";
                try { objid = row["objid"].ToString(); }
                catch (Exception ex) { }
                try { objname = row["objname"].ToString(); }
                catch (Exception ex) { }
                try { location = row["location"].ToString(); }
                catch (Exception ex) { }
                try { quantity = row["quantity"].ToString(); }
                catch (Exception ex) { }
                try { userid = row["userid"].ToString(); }
                catch (Exception ex) { }
                try
                {
                    quant = Int16.Parse(quantity);
                }
                catch (Exception e) { }

                if (quant > 0 && objid != null && objid != "" && objid != "null")
                {

                    // Try to send the information to the WebService 
                    try
                    {
                        autobuildHTK_client.Rivka.Api.PrintServiceClient PrintService = new autobuildHTK_client.Rivka.Api.PrintServiceClient();

                        string availablePrinter = "";
                        //  Checks if there is an available printer
                        try
                        {
                            availablePrinter = PrintService.GetPrinter();
                            if (availablePrinter != null && availablePrinter != "")
                            {
                                String newEPC = "";
                                JObject obj = new JObject();
                                bool conected = PrintService.Connect(availablePrinter);
                                bool oLabel = PrintService.OpenLabel();

                                //Generate new EPCs
                                newEPC = objectTable.getEPC(objid, quantity);
                                obj = JsonConvert.DeserializeObject<JObject>(newEPC);
                                string nextEPC = obj["nextEPC"].ToString();

                                bool sEPC = PrintService.SetEPC(nextEPC);

                                int.TryParse(obj["quantity"].ToString(), out labels);
                                bool print = PrintService.Print(availablePrinter, labels);

                                PrintService.Disconnect(availablePrinter);
                                PrintService.Close();

                                if (print == true)
                                {
                                    saveObjects(objid, location, userid, objname, labels, obj["nextEPC"].ToString());
                                    if (labels != quant)
                                    {
                                        cadena = "Enviado. Solo se imprimieron " + labels.ToString() + " etiquetas.";
                                        tipo = "warning";
                                    }
                                    else
                                    {
                                        cadena = "Enviado Satisfactoriamente";
                                        tipo = "success";
                                    }
                                    bool ok = true;
                                    try
                                    {
                                        ok = RulesChecker.isValidToLocation(objid, location);
                                    }
                                    catch (Exception e) { }
                                    if (ok == false)
                                    {
                                        classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicacion no valida");
                                        // return "problem";
                                    }
                                }
                                else
                                {
                                    cadena = "Error en la Impresión";
                                    tipo = "error";
                                }

                            }
                            else
                            {
                                cadena = "No Impresora Encontrada";
                                tipo = "error";
                            }

                        }
                        catch (Exception e)
                        {
                            tipo = "error";
                            cadena = "No Impresora Encontrada";
                        }
                    }
                    catch (Exception e)
                    {
                        tipo = "error";
                        cadena = "Error de comunicación, Intente nuevamente";
                    }
                }
                else
                {
                    tipo = "error";
                    cadena = "Datos Incorrectos";
                }

            }

            objcad["cadena"] = cadena;
            objcad["tipo"] = tipo;

            return JsonConvert.SerializeObject(objcad);

        }

        /// <summary>
        ///     Save the new Objects
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public String SaveLabels(String objects)
        {
            JObject result = new JObject();
            JArray conflictedObjects = new JArray();

            result.Add("status", "success");

            if (objects != null)
            {
                JArray Objects;
                try
                {
                    Objects = JsonConvert.DeserializeObject<JArray>(objects);
                    foreach(JObject newObject in Objects)
                    {
                        JObject saveObject = new JObject();
                        saveObject.Add("EPC", "");
                        saveObject.Add("objectReference", "");

                        try {
                            saveObject["EPC"] = newObject.GetValue("EPC").ToString();
                            saveObject["objectReference"] = newObject.GetValue("objectReference").ToString(); 
                        }
                        catch (Exception e) { }

                        // EPC and ObjectReference are required
                        if (saveObject["EPC"].ToString() != "" && saveObject["objectReference"].ToString() != "")
                        {
                            //Optional Fields
                            try { saveObject.Add("name", newObject.GetValue("name").ToString()); } catch(Exception e){}
                            try { saveObject.Add("userid", newObject.GetValue("userid").ToString()); } catch(Exception e){}
                            try { saveObject.Add("location", newObject.GetValue("location").ToString()); } catch(Exception e){}

                            // Checks for a valid EPC
                            if (saveObject["EPC"].ToString().Length == 24)
                            {
                                // Checks for unique EPC
                                string duplicated = objectRealTable.GetAll("EPC", saveObject["EPC"].ToString());

                                // Its unique
                                if (duplicated == "[]")
                                {
                                    objectRealTable.SaveRow(saveObject.ToString());
                                }
                                else
                                {
                                    result["status"] = "duplicated";
                                    conflictedObjects.Add(saveObject["EPC"].ToString());
                                }
                            }
                            else
                            {
                                result["status"] = "invalid";
                                conflictedObjects.Add(saveObject["EPC"].ToString());
                            }
                            try
                            {
                                _logTable.SaveLog(Session["_id"].ToString(), "Etiquetado", "Insert: Se han guardado nuevos activos", "ObjectReal", DateTime.Now.ToString());
                            }
                            catch { }
                        }
                        else
                            result["status"] = "error";
                    }
                }
                catch(Exception e){}

            }

            if (result["status"].ToString() == "duplicated" || result["status"].ToString() == "invalid")
                result.Add("duplicated", conflictedObjects);

            return result.ToString();
        }

        public JsonResult getNodeLocation(String parentCategory)
        {
            if (parentCategory == "") parentCategory = "null";
            String categoriesString = locationTable.Get("parent", parentCategory);

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

        public ActionResult getObjectModal()
        {
            //Get All Users
            {
                string rowArray = userTable.GetRows();
                JArray dataArray = JsonConvert.DeserializeObject<JArray>(rowArray);
                JObject data = new JObject();

                foreach (JObject items in dataArray)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString() + " " + items["lastname"].ToString());
                }

                ViewData["users"] = data;
            }

            //Get All Locations
            {
                string rowArray = locationTable.GetRows();
                JArray dataArray = JsonConvert.DeserializeObject<JArray>(rowArray);
                JObject data = new JObject();

                foreach (JObject items in dataArray)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString());
                }

                ViewData["locations"] = data;
            }

            //Get All Reference Objects
            {
                string rowArray = objectTable.GetRows();
                JArray dataArray = JsonConvert.DeserializeObject<JArray>(rowArray);
                JObject data = new JObject();

                foreach (JObject items in dataArray)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString());
                }

                ViewData["objects"] = data;
            }


            return View();
        }
        /// <summary>
        ///     Gets the user modal
        /// </summary>
        public ActionResult GetUsersModal()
        {
            string rowArray = userTable.GetRows();
            JArray usrs = JsonConvert.DeserializeObject<JArray>(rowArray);
            Dictionary<string, string> data = new Dictionary<string, string>();

            foreach (JObject items in usrs)
            {
                data.Add(items["_id"].ToString(), items["name"].ToString());
            }

            ViewData["users"] = data;
            return View();
        }

        public JsonResult getUserProfiles()
        {
            String categoriesString = userprofileTable.GetRows();
            if (categoriesString == null) return null; //there are no subcategories

            return Json(categoriesString);
        }

        public JsonResult getUsuarios(String idprofile)
        {
            String usersString = userTable.Get("profileId", idprofile);
            if (usersString == null) return null; //there are no subcategories

            return Json(usersString);
        }

        public JsonResult getUsuariosPorId(String iduser)
        {
            String usersString = userTable.GetRow(iduser);
            if (usersString == null) return null; //there are no subcategories

            return Json(usersString);
        }


        public JsonResult globalSearch(String data)
        {
            String usersString = userTable.Get("name", data);
            if (usersString == null) return null; //there are no subcategories

            return Json(usersString);

        }
    }
}
