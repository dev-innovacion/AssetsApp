using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web.Security;
using System.Data;
using System.Data.OleDb;
using System.Security.Cryptography;
using System.Xml;
//using Microsoft.Office.Interop;
//using Microsoft.Office.Interop.Excel;
using System.Configuration;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;

using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Security;
using Rivka.Api;
using Rivka.Db.MongoDb;
using RivkaAreas.ObjectReference.Models;
using Rivka.Db;
using Rivka.Form;
using Rivka.Form.Field;
using Rivka.Images;
using Rivka.Files;
using RivkaAreas.Rule;
using Rivka.Mail;
namespace RivkaAreas.ObjectReference.Controllers
{

    [Authorize]
    public class ObjectController : Controller
    {
        // GET: /ObjectReference/Object/
        protected Notifications classNotifications;
        protected CategoryTable categoryTable; //category's model
        protected ObjectTable objectTable; //object's model
        protected UserTable userTable; //user's model
        protected CustomFieldsTable fieldTable; // field's model
        protected MongoModel objectsfields;
        protected MongoModel Refobjectsfields;
        protected LocationTable locationTable;
        protected Agent agent;
        protected validatePermissions validatepermissions;
        protected ObjectReal objectRealTable;
        protected Notifications notificationObject;
        protected ListTable _listTable;
        protected LocationProfileTable _locationProfile;

        /// <summary>
        ///     This constructor initializes the models
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        public ObjectController()
        {
            classNotifications = new Notifications();
            categoryTable = new CategoryTable();
            objectTable = new ObjectTable();
            userTable = new UserTable();
            fieldTable = new CustomFieldsTable("ObjectFields");
            objectsfields = new MongoModel("ObjectFields");
            Refobjectsfields = new MongoModel("ReferenceObjects");
            locationTable = new LocationTable();
            agent = null;
            validatepermissions = new validatePermissions();
            objectRealTable = new ObjectReal();
            notificationObject = new Notifications();
            _listTable = new ListTable();
            _locationProfile = new LocationProfileTable();
        }

        /// <summary>
        ///     This method initializes the agent
        /// </summary>
        private void initAgent()
        {
            if (agent == null)
            {
                String path = Server.MapPath("/App_Data/com-agent.conf");
                StreamReader objReader = new StreamReader(path);
                String sLine = objReader.ReadLine();

                JObject agentConf = JsonConvert.DeserializeObject<JObject>(sLine);
                agent = new Agent(agentConf["url"].ToString(), agentConf["user"].ToString(), agentConf["password"].ToString());
            }
        }

        /// <summary>
        ///     Sends a request to Edgeware to Print a Label
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public String PrintLabel(String objid, String quantity, String location)
        {
            int labels = 0;
            int quant = 0;
            String cadena = "";
            String tipo = "";
            JObject objcad = new JObject();
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

                            //string[] variables = PrintService.GetVariables();
                            //int nv = variables.Length;

                            //Geenerate new EPCs
                            newEPC = objectTable.getEPC(objid, quantity);
                            obj = JsonConvert.DeserializeObject<JObject>(newEPC);
                            string nextEPC = obj["nextEPC"].ToString();

                            bool sEPC = PrintService.SetEPC(nextEPC);

                            if (sEPC == true)
                                Console.Beep();

                            int.TryParse(obj["quantity"].ToString(), out labels);
                            bool print = PrintService.Print(availablePrinter, labels);

                            PrintService.Disconnect(availablePrinter);
                            PrintService.Close();

                            if (print == true)
                            {
                                saveObjects(objid, location, labels, obj["nextEPC"].ToString());
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
                                bool ok=true;
                                ok = RulesChecker.isValidToLocation(objid, location);
                                if (ok == false)
                                {
                                    classNotifications.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicación no válida");
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
            else {
                tipo = "error";
                cadena = "Datos Incorrectos";
            }
                

            objcad["cadena"] = cadena;
            objcad["tipo"] = tipo;

            return JsonConvert.SerializeObject(objcad);

        }

        /// <summary>
        ///     This method allows to delete several objects from the db
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
        public String deleteObjects(List<String> array)
        {

            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objectsreference", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("objectsreference", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest()) //only available with AJAX
                {
                    try //tryign to delete the users
                    {
                        if (array.Count == 0) return null; //if array is empty there are no users to delete
                        foreach (String id in array) //froeach id in the array we must delete the document with that id from the db
                        {
                            objectTable.deleteRow(id);
                        }

                        return "Borrado";
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public ActionResult ImpExcel(HttpPostedFileBase file, string idcategory)
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
                if (Request.Files["file"].ContentLength > 0 && (idcategory != "0" && idcategory != null))
                {
                    ViewData["idcategory"] = idcategory;
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
                        /*  string excelConnectionString = string.Empty;
                          excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=No;IMEX=1\"";
                          //connection String for xls file format.
                          if (fileExtension == ".xls")
                          {
                              excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=No;IMEX=1\"";
                          }
                          //connection String for xlsx file format.
                          else if (fileExtension == ".xlsx")
                          {

                              excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=No;IMEX=1\"";
                          }
                          //Create Connection to Excel work book and add oledb namespace
                          OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                          excelConnection.Open();
                          System.Data.DataTable dt = new System.Data.DataTable();

                          dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                          if (dt == null)
                          {
                              return null;
                          }

                          String[] excelSheets = new String[dt.Rows.Count];
                          int t = 0;
                          //excel data saves in temp file here.
                          foreach (DataRow row in dt.Rows)
                          {
                              excelSheets[t] = row["TABLE_NAME"].ToString();
                              t++;
                          }
                          OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                          string query = string.Format("Select * from [{0}]", excelSheets[0]);
                          using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                          {
                              dataAdapter.Fill(ds);
                          }


                          excelConnection1.Close();
                          excelConnection.Close();
                         */
                    }
                    /*  if (fileExtension.ToString().ToLower().Equals(".xml"))
                      {
                          string fileLocation = Server.MapPath("~/Content/") + Request.Files["FileUpload"].FileName;
                          if (System.IO.File.Exists(fileLocation))
                          {
                              System.IO.File.Delete(fileLocation);
                          }

                          Request.Files["FileUpload"].SaveAs(fileLocation);
                          XmlTextReader xmlreader = new XmlTextReader(fileLocation);
                          // DataSet ds = new DataSet();
                          ds.ReadXml(xmlreader);
                          xmlreader.Close();
                      } 
                      */


                    string fileLocation2 = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation2))
                    {

                        System.IO.File.Delete(fileLocation2);
                    }
                    Request.Files["file"].SaveAs(fileLocation2);

                    /*  Application xlApp = new Application();
                      Workbook xlWorkbook = xlApp.Workbooks.Open(fileLocation2, 0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                    _Worksheet xlWorksheet = (_Worksheet)xlWorkbook.Sheets[1];
                      Range xlRange = xlWorksheet.UsedRange;
                      int rowCount = xlRange.Rows.Count;
                      int colCount = xlRange.Columns.Count;

                      for (int i = 1; i <= rowCount; i++)
                      {
                          List<string> td = new List<string>();

                          for (int j = 1; j <= colCount; j++)
                          {
                              td.Add(xlWorksheet.Cells[i, j].Value.ToString());
                          }

                          tr.Add(td);
                      }*/

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



                    string rowsArray = categoryTable.getRow(idcategory);
                    //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                    JObject categoryJarray = JsonConvert.DeserializeObject<JObject>(rowsArray);
                    List<List<string>> data = new List<List<string>>();
                    string idfield = "";
                    foreach (JObject items in categoryJarray["customFields"])
                    {
                        foreach (var customf in items["fields"])
                        {
                            List<string> rows = new List<string>();
                            idfield = objectsfields.GetRow(customf["fieldID"].ToString());
                            if (idfield != null)
                            {
                                try
                                {
                                    JObject fieldata = JsonConvert.DeserializeObject<JObject>(idfield);

                                    rows.Add(fieldata["_id"].ToString());
                                    rows.Add(fieldata["name"].ToString());
                                    rows.Add(customf["required"].ToString());

                                    data.Add(rows);
                                }
                                catch (Exception e)
                                {

                                }
                            }
                        }
                        // data.Add(items["email"].ToString());
                    }
                    ViewData["categoriedata"] = data;

                    // xlWorkbook.Close();
                    // xlApp.Quit();

                    ViewData["Filelocation"] = fileLocation2;
                    ViewData["Filerequest"] = file;
                    return View(tr);
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                // System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
                //System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
                return null;
            }
        }

        public static SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, Int32 id)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
        }

        /// <summary>
        ///     Object Reference's index section
        /// </summary>
        /// <param name="parentCategory">
        ///     The category from where the categories must be showed
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada romero
        /// </author>
        /// <returns>
        ///     Returns the Object Reference's index view
        /// </returns>
        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objectsreference", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objectsreference", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {

                loadusers();
                loadDepartments();
                loadCategories();
                loadProveedor();
                return View();
            }
            else
            {
                return Redirect("~/Home");
            }
        }

        /// <summary>
        ///     Allows to get 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult getObject(String id)
        {
            try {
                JObject data = JsonConvert.DeserializeObject<JObject>(objectTable.GetRow(id));
                JToken ja;
                data.Add("depreciacion", "");
                try {
                    string category = data["parentCategory"].ToString();
                    JObject categoryjo = JsonConvert.DeserializeObject<JObject>(categoryTable.GetRow(category));
                    data["depreciacion"] = categoryjo["depreciacion"].ToString();
                }
                catch { }
                
                return Json(JsonConvert.SerializeObject(data),JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return null;
            }
        }

        public String saveImport(String data, HttpPostedFileBase folderimages = null)
        {
            try
            {
                /*   ASCIIEncoding ascii = new ASCIIEncoding();
                   byte[] byteArray = Encoding.UTF8.GetBytes(data);
                   byte[] asciiArray = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, byteArray);
                   string descompress = ascii.GetString(asciiArray);*/
                //  string descompress = decode.decompress1(data);
                //   String  descompress = decode.decompressFromBase64(data);

                DataFileManager Filelimits;
                string filepath = "/App_Data/system.conf";
                string absolutedpath = Server.MapPath(filepath);
                Filelimits = new DataFileManager(absolutedpath, "juanin");
                String clientid="";

                if (!Filelimits.empty())
                {
                    clientid = Filelimits["clientInfo"]["userId"];
                }

                String cadena = "";
                String dataimport = data.ToString();
                JArray dataimportarray = JsonConvert.DeserializeObject<JArray>(dataimport);
                int cont = 0;
                string relativepath = "\\Uploads\\Images\\ObjectReferences\\";
                string absolutepath = Server.MapPath(relativepath);
                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }

                string []cads; 
                string namefolder="";
                if (folderimages != null) {
                    folderimages.SaveAs(absolutepath + folderimages.FileName);
                    cads=folderimages.FileName.Split('.');
                    namefolder=cads[0];
                }
                
                ZipFile.ExtractToDirectory(absolutepath + folderimages.FileName,absolutepath);

                foreach (JObject items in dataimportarray)
                {
                    if (RevisarIDArticulo(items["object_id"].ToString(),"null") == "si")
                    {
                        cont++;
                        continue; }
                    items["ext"] = items["imagen"].ToString().Split('.').Last();
                    string tipo="";
                    if (items["assetType"].ToString() == "Sistemas") tipo = "system";
                    if (items["assetType"].ToString() == "Mantenimiento") tipo = "maintenance";
                    if (items["assetType"].ToString() == "Proyección y Sonido") tipo = "sound";

                    //Set price format - pendiente


                    String jsonimportsave = "{'name':'" + items["name"] + "','location_id':'" + items["location_id"] + "','department':'" + items["department"] + "','marca':'" + items["marca"] + "',"
                    + "'modelo':'" + items["modelo"] + "','perfil':'" + items["perfil"] + "','precio':'" + items["precio"] + "','object_id':'" + items["object_id"] + "',"
                    + "'assetType':'" + tipo + "',"
                    +"'ext':'" + items["ext"] + "','parentCategory':'" + items["parentCategory"] + "','profileFields':" + items["profileFields"] + "}";

                    string Idcateg = Refobjectsfields.SaveRow(jsonimportsave);

                    if( clientid!="") objectTable.NewObjectEPC(Idcateg, clientid);
                  
                    try{

                        if (System.IO.File.Exists(absolutepath +namefolder+"\\"+ items["imagen"].ToString()))
                        {
                            System.IO.File.Copy(absolutepath + namefolder + "\\" + items["imagen"].ToString(), absolutepath + Idcateg + "." + items["ext"]);

                            //  archivo.SaveAs(absolutepath + "\\" + id + "." + ext);
                        }
                    }catch(Exception ex){
                    
                    }
                    
                }
                if (folderimages != null)
                {
                    if (System.IO.File.Exists(absolutepath + folderimages.FileName))
                    {
                        System.IO.File.Delete(absolutepath + folderimages.FileName);
                    }

                    if (System.IO.Directory.Exists(absolutepath + namefolder))
                    {
                        System.IO.Directory.Delete(absolutepath + namefolder, true);
                    }
                }
                if (cont > 0)
                {
                    cadena = cont.ToString() + " objetos no fueron agregados.";
                }
                else {
                    cadena = "saved";
                }

                return cadena;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        /// <summary>
        ///     This section allows to create a new category
        /// </summary>
        /// <param name="idProfile">
        ///     The modifying document's id
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the createCategory view (fields is the array with the Object reference's customFields )
        /// </returns>
        public ActionResult createCategory(string parentCategory, string idProfile = null)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objectsreference", "c", dataPermissions);
            accessClient = validatepermissions.getpermissions("objectsreference", "c", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                CustomFieldsTable cft = new CustomFieldsTable("ObjectFields");
                String fields = cft.GetRows();
                JArray fieldsArray = JsonConvert.DeserializeObject<JArray>(fields);

                if (idProfile != null) //modigying an existing document
                {
                    String category = categoryTable.getRow(idProfile);
                    if (category != null) //the profile does exist
                    {
                        ViewData["profile"] = new HtmlString(category);
                    }
                }

                ViewData["parentCategory"] = new HtmlString(parentCategory);

                return View(fieldsArray);
            }
            else
            {
                return Redirect("~/Home");
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
        [HttpPost]
        public JsonResult getData(String parentCategory = "null")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objectsreference", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objectsreference", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                string idcatthis = parentCategory;

                if (parentCategory == "") parentCategory = "null";
                String categories = categoryTable.Get("parentCategory", parentCategory);
                String objectsString = "";
                if (parentCategory == "null") { objectsString = objectTable.GetRows(); }
                else { objectsString = objectTable.Get("parentCategory", parentCategory); }

                //removing categories customFields
                JArray categoriesObject = new JArray();
                try
                {
                    categoriesObject = JsonConvert.DeserializeObject<JArray>(categories);
                }
                catch(Exception e)
                {
                }
                foreach (JObject document in categoriesObject)
                {
                    document.Remove("customFields");
                    try
                    { //trying to set the creator's name
                        String rowString = userTable.GetRow(document["Creator"].ToString());
                        JObject rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
                        document["Creator"] = rowArray["name"] + " " + rowArray["lastname"];
                    }
                    catch (Exception e)
                    {
                        document["Creator"] = "";
                    }

                    try
                    {
                        if (document["ext"].ToString() != "")
                        {
                            document.Add("image", "/Uploads/Images/Categories/" + document["_id"].ToString() + "." + document["ext"].ToString());
                        }
                        document.Remove("ext");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    try //trying to set category's objects number
                    {
                        String objects = objectTable.Get("parentCategory", document["_id"].ToString());
                        JArray childObject = JsonConvert.DeserializeObject<JArray>(objects);
                        document["objectCount"] = childObject.Count;
                    }
                    catch (Exception e)
                    {

                    }
                }
                categories = JsonConvert.SerializeObject(categoriesObject);

                //creating the objets table prototype
                List<string> prototype = new List<string>();
                List<string> requiredValues = new List<string>();
                String prototypeString = categoryTable.GetRow(parentCategory);
                if (prototypeString != null)
                {
                    JObject prototypeObject = JsonConvert.DeserializeObject<JObject>(prototypeString);
                    foreach (JObject tab in prototypeObject["customFields"])
                    {
                        foreach (JObject field in tab["fields"])
                        {
                            String fieldString = fieldTable.GetRow(field["fieldID"].ToString());
                            if (fieldString != null)
                            {
                                String fieldLabel = JsonConvert.DeserializeObject<JObject>(fieldString)["label"].ToString();
                                String fieldName = JsonConvert.DeserializeObject<JObject>(fieldString)["name"].ToString();
                                prototype.Add(fieldLabel);
                                requiredValues.Add(fieldName);
                            }
                        }
                    }
                }

                //doing changes to objects array
                JArray objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                foreach (JObject document in objectsObject)
                {
                    try
                    { //trying to set the creator's name
                        String rowString = userTable.GetRow(document["Creator"].ToString());
                        JObject rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
                        document["Creator"] = rowArray["name"] + " " + rowArray["lastname"];
                    }
                    catch (Exception e)
                    {
                        document["Creator"] = "";
                    }

                    try
                    { //trying to set the creator's name
                        String rowString = locationTable.GetRow(document["location_id"].ToString());
                        JObject rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
                        document["location"] = rowArray["name"];
                    }
                    catch (Exception e)
                    {
                        document["location"] = "";
                    }

                    try
                    { //trying to set the creator's name
                        String rowString = _listTable.Get("name", "departments");
                        JArray rowArray = JsonConvert.DeserializeObject<JArray>(rowString);
                        JArray listas = new JArray();
                        foreach (JObject obj in rowArray)
                        {
                            listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["unorder"].ToString());
                        }

                        //document["department"] = listas[document["department"].ToString()];
                        bool findDep = false;
                        foreach (JObject depa in listas)
                        {
                            foreach (KeyValuePair<string, JToken> token in depa)
                            {
                                if (token.Key == document["department"].ToString())
                                {
                                    document["department"] = token.Value.ToString();
                                    findDep = true;
                                }
                            }
                            if (findDep) break;
                        }
                    }
                    catch (Exception e)
                    {
                        document["department"] = "";
                    }
                   
                    try
                    {
                        document.Remove("parentCategory");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    try
                    {
                        if (document["ext"].ToString() != "")
                            document.Add("image", "/Uploads/Images/ObjectReferences/" + document["_id"].ToString() + "." + document["ext"].ToString());
                        document.Remove("ext");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    JObject profileFields = (JObject)document["profileFields"];
                    JObject profileCopy = new JObject();
                    foreach (string key in requiredValues)
                    {

                        if (profileFields[key] == null)
                        {
                            profileCopy.Add(key, "");
                        }
                        else
                        {
                            profileCopy.Add(key, profileFields[key]);
                        }
                    }
                    document["profileFields"] = profileCopy;
                }
                objectsString = JsonConvert.SerializeObject(objectsObject);

                //Creating the route data
                JArray route = new JArray();
                while (parentCategory != "null")
                {

                    String actualCategory = categoryTable.GetRow(parentCategory);
                    if (actualCategory == null)
                    {
                        this.Response.StatusCode = 500;
                        return null;
                    }
                    JObject actualCatObject = JsonConvert.DeserializeObject<JObject>(actualCategory);
                    JObject categoryObject = new JObject();
                    categoryObject.Add("id", actualCatObject["_id"]);
                    categoryObject.Add("name", actualCatObject["name"]);
                    route.Add(categoryObject);
                    parentCategory = actualCatObject["parentCategory"].ToString();
                }

                JObject result = new JObject();
                result.Add("categories", categories);
                result.Add("objects", objectsString);
                result.Add("route", route);
                result.Add("prototype", prototype.ToJson());
                result.Add("idcat", idcatthis);
                return Json(JsonConvert.SerializeObject(result));
            }
            else
            {

                return null;
            }
        }

        [HttpPost]
        public void deleteObject(String id)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objectsreference", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("objectsreference", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                try
                {
                    objectTable.deleteRow(id);
                }
                catch (Exception)
                {
                    this.Response.StatusCode = 500;
                }
            }
        }

        /// <summary>
        ///     This method allows to get the objects in a category  
        /// </summary>
        /// <param name="idCategory">
        ///     The category's id
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getObjectByCategory(string idCategory)
        {
            //dynamic objects = objectTable.get("categoryId", idCategory);
            //List<BsonDocument> categories = categoryTable.getRows();
            ViewBag.canDelete = true;
            //try
            //{
            //    if (_profileTable.getRow(idProfile).GetElement("name").Value.ToString() == "Básico")
            //        ViewBag.canDelete = false;
            //}
            //catch (Exception e)
            //{
            //    ViewBag.canDelete = false;
            //}
            //ViewBag.profiles = categories;
            ViewBag.idProfile = idCategory;

            return View(/*objects*/);
        }

        /// <summary>
        ///     Allows to create a new category or modify an existing one
        /// </summary>
        /// <param name="categoryData">
        ///     It's the category's information to save
        /// </param>
        /// <param name="idProfile">
        ///     The category's id to modify
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the category's saved id
        /// </returns>
        [HttpPost]
        public string saveCategory(string categoryData, HttpPostedFileBase file, string parentCategory = "null", string idProfile = null)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objectsreference", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("objectsreference", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                JObject categoryObject;
                try //does the categoryData have a corret json syntax?
                {
                    categoryObject = JsonConvert.DeserializeObject<JObject>(categoryData);
                }
                catch (Exception e)
                {
                    return null;
                }

                //due that the user's id is unique we use it has the image's name, so we store only the extension in the db
                string ext = null;
                if (file != null)
                {
                    ext = file.FileName.Split('.').Last(); //getting the extension
                }
                else if (idProfile != null && idProfile != "")
                {
                    String categoryString = categoryTable.getRow(idProfile);
                    JObject category = JsonConvert.DeserializeObject<JObject>(categoryString);
                    try
                    {
                        ext = category["ext"].ToString();
                    }
                    catch (Exception e) { }
                }

                categoryObject["ext"] = ext;

                //does the parentCategory's id exist?
                parentCategory = (parentCategory == null) ? "null" : parentCategory;
                if (parentCategory != "null" && categoryTable.getRow(parentCategory) == null)
                {
                    parentCategory = "null";
                }
                categoryObject["parentCategory"] = parentCategory;

                //everything is fine, lets serialize it again!
                try
                {
                    categoryData = JsonConvert.SerializeObject(categoryObject);
                }
                catch (Exception e)
                {
                    return null;
                }
                idProfile = (categoryTable.getRow(idProfile) == null) ? null : idProfile; //the gived id does not exist save it as new
                String auxid = idProfile;
                idProfile = categoryTable.saveRow(categoryData, idProfile);
                try
                {
                    initAgent();
                    categoryObject.Add("client-name", "cinepolis");
                    categoryObject.Add("innerCollection", "category");
                    categoryObject.Add("auxId", auxid);
                    categoryObject.Add("idInClient", idProfile);
                    String jsonData = JsonConvert.SerializeObject(categoryObject);
                    jsonData = jsonData.Replace("\"", "'");
                    String jsonAgent = "{'action':'spreadToFather','collection':'ObjectReference','document':" + jsonData + ",'Source':'cinepolis'}";
                    String response = agent.sendMessage(jsonAgent);
                }
                catch { }
                if (file != null)
                {
                    string relativepath = "\\Uploads\\Images\\Categories\\";
                    string absolutepath = Server.MapPath(relativepath);
                    if (!System.IO.Directory.Exists(absolutepath))
                    {
                        System.IO.Directory.CreateDirectory(absolutepath);
                    }
                    file.SaveAs(absolutepath + "\\" + idProfile + "." + ext);

                    Images resizeImage = new Images(absolutepath + "\\" + idProfile + "." + ext, absolutepath, idProfile + "." + ext);
                    // If image bigger than 1MB, resize to 1024px max
                    if (file.ContentLength > 1024 * 1024)
                        resizeImage.resizeImage(new System.Drawing.Size(1024, 1024));

                    // Create the thumbnail of the image
                    resizeImage.createThumb();
                }

                return idProfile;
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
                string relativepath = "\\Uploads\\Images\\ObjectReferences\\CustomImages\\";
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
        ///     This method allows to save an object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="file"></param>
        /// <param name="files"></param>
        /// <param name="parentCategory"></param>
        /// <param name="id"></param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns></returns>
        public String saveObject(FormCollection data, HttpPostedFileBase file, IEnumerable<HttpPostedFileBase> files, String parentCategory = "null", String id = null)
        {
            if (id == "null" || id == "") //differents ways to receive null from javascript
            {
                id = null;
            }

            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objectsreference", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("objectsreference", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                data = CustomForm.unserialize(data);
                JObject objectArray = new JObject();
                try //name must be gived
                {
                    if (data["name"] == null || data["name"] == "")
                    {
                        return null; //name is wrong
                    }
                    objectArray.Add("name", data["name"].ToString());
                    data.Remove("name");
                }
                catch (Exception e)
                { //what to do if name is wrong
                    return null;
                }

                //try //name must be gived
                //{
                //    if (data["userSelect"] == null || data["userSelect"] == "")
                //    {
                //        return null; //name is wrong
                //    }
                //    objectArray.Add("userSelect", data["userSelect"].ToString());
                //    data.Remove("userSelect");
                //}
                //catch (Exception e)
                //{ //what to do if name is wrong
                //    return null;
                //}


                string ext = null;
                if (file != null)
                {
                    ext = file.FileName.Split('.').Last(); //getting the extension
                }
                else if (id != null && id != "")
                {
                    String objectString = objectTable.GetRow(id);
                    JObject objObject = JsonConvert.DeserializeObject<JObject>(objectString);
                    try
                    {
                        ext = objObject["ext"].ToString();
                    }
                    catch (Exception e) { }
                }

                objectArray.Add("ext", ext);
                objectArray.Add("location_id", data["idlocation"].ToString());
                objectArray.Add("department", data["departamento"].ToString());
                objectArray.Add("proveedor", data["proveedor"].ToString());
                objectArray.Add("marca", data["marca"].ToString());
                objectArray.Add("modelo", data["modelo"].ToString());
                objectArray.Add("perfil", data["perfil"].ToString());
                objectArray.Add("precio", data["precio"].ToString());
                objectArray.Add("object_id", data["object_id"].ToString());
                if (data["categoryid"].ToString()!="null")
                    objectArray.Add("parentCategory", data["categoryid"].ToString());
              //  objectArray.Add("assetType", data["assetType"].ToString());

                data.Remove("object_id");
                data.Remove("marca");
                data.Remove("modelo");
                data.Remove("perfil");
                data.Remove("departamentoSelect");
                data.Remove("proveedorSelect");
                data.Remove("category");
                data.Remove("precio");
                data.Remove("idlocation");
                data.Remove("departamento");
                data.Remove("categoryid");

                JObject profileFields = new JObject();

                //foreach element in the formData, let's append it to the jsonData in the profileFields
                foreach (String key in data.Keys)
                {
                    profileFields.Add(key, data[key]);
                }
                objectArray.Add("profileFields", profileFields);
                try
                {
                    objectArray.Add("assetType", data["assetType"].ToString());
                }
                catch(Exception e){}


                String jsonData = JsonConvert.SerializeObject(objectArray);

                String auxid = id;
                id = objectTable.saveRow(jsonData, id);

                //sacar IdCliente
                DataFileManager Filelimits;
                string filepath = "/App_Data/system.conf";
                string absolutedpath = Server.MapPath(filepath);
                Filelimits = new DataFileManager(absolutedpath, "juanin");

                if (!Filelimits.empty()) {
                    String clientid;
                    clientid = Filelimits["clientInfo"]["userId"];
                    objectTable.NewObjectEPC(id, clientid);
                }
                try {
                    initAgent();
                    objectArray.Add("client-name", "cinepolis");
                    objectArray.Add("innerCollection", "objectReference");
                    objectArray.Add("auxId", auxid);
                    objectArray.Add("idInClient", id);
                    jsonData = JsonConvert.SerializeObject(objectArray);
                    jsonData = jsonData.Replace("\"", "'");
                    String jsonAgent = "{'action':'spreadToFather','collection':'ObjectReference','document':" + jsonData + ",'Source':'cinepolis'}";
                    agent.sendMessage(jsonAgent);
                }
                catch { }
                if (file != null)
                {
                    string relativepath = "\\Uploads\\Images\\ObjectReferences\\";
                    string absolutepath = Server.MapPath(relativepath);
                    if (!System.IO.Directory.Exists(absolutepath))
                    {
                        System.IO.Directory.CreateDirectory(absolutepath);
                    }
                    file.SaveAs(absolutepath + "\\" + id + "." + ext);

                    Images resizeImage = new Images(absolutepath + "\\" + id + "." + ext, absolutepath, id + "." + ext);
                    // If image bigger than 1MB, resize to 1024px max
                    if (file.ContentLength > 1024 * 1024)
                        resizeImage.resizeImage(new System.Drawing.Size(1024, 1024));

                    // Create the thumbnail of the image
                    resizeImage.createThumb();

                }

                return null;
            }
            else { return null; }
        }

        /// <summary>
        ///     Allows to get the objects form view
        /// </summary>
        /// <param name="parentCategory">
        ///     The category where the objec is found
        /// </param>
        /// <returns>
        ///     The object's view form
        /// </returns>
        [HttpPost]
        public String getObjectForm(String parentCategory)
        {
            String category = categoryTable.GetRow(parentCategory);
            JObject categoryObject = JsonConvert.DeserializeObject<JObject>(category);
            String body = CustomForm.getFormView(categoryObject["customFields"].ToString(), "ObjectFields");
            String headers = CustomForm.getFormTitlesView(categoryObject["customFields"].ToString());
            JObject Result = new JObject();
            Result.Add("body", body);
            Result.Add("headers", headers);
            return JsonConvert.SerializeObject(Result);
        }

        /// <summary>
        ///     This method allows to get the object's form from it's parents fields
        /// </summary>
        /// <param name="parentCategory">
        ///     The object's parent
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a html string with the headers and tab's bodys of  the object
        /// </returns>
        [HttpPost]
        public String getObjectMixedForm(String parentCategory)
        {
            List<JObject> categoryStack = new List<JObject>();
            JObject row;
            while (parentCategory != "null")
            { //loop to the root to get all the parents
                row = JsonConvert.DeserializeObject<JObject>(categoryTable.getRow(parentCategory));
                categoryStack.Add(row);
                parentCategory = row["parentCategory"].ToString();
            }
            if (categoryStack.Count == 0) //there are not super categories
                return null;
            JObject lastCategory = categoryStack.Last();
            String lastCategoryFields = lastCategory["customFields"].ToString();
            JObject actual;
            String actualCategoryFields;
            for (int i = categoryStack.Count - 2; i >= 0; i--)
            {
                actual = categoryStack[i];
                actualCategoryFields = actual["customFields"].ToString();
                lastCategoryFields = mixCustomFields(lastCategoryFields, actualCategoryFields);
            }
            String body = CustomForm.getFormView(lastCategoryFields, "ObjectFields");
            String headers = CustomForm.getFormTitlesView(lastCategoryFields);

            JObject Result = new JObject();
            Result.Add("body", body);
            Result.Add("headers", headers);
            return JsonConvert.SerializeObject(Result);
        }

        /// <summary>
        ///     This method allows to mix two categoriesPrototypes
        /// </summary>
        /// <param name="upperCustomFields">
        ///     The parentDocument's custom fields section where the data must be added.
        ///     Example: [{\"tabName\":\"tab1\",\"fields\":[{\"fieldID\":\"52d72f5b1a5775e857443381\",\"position\":1,\"size\":1,\"required\":0}]}]
        /// </param>
        /// <param name="lowerCustomFields">
        ///     The category to add, in the lower level, it has more weight than the leftCustomFields.
        ///     Example: [{\"tabName\":\"tab1\",\"fields\":[{\"fieldID\":\"52d72f5b1a5775e857443381\",\"position\":1,\"size\":1,\"required\":0}]}]
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the mixed customFields
        /// </returns>
        public String mixCustomFields(String upperCustomFields, String lowerCustomFields)
        {
            JArray upperObject = JsonConvert.DeserializeObject<dynamic>(upperCustomFields);
            JArray lowerObject = JsonConvert.DeserializeObject<dynamic>(lowerCustomFields);

            //getting all parent's tabs
            List<JObject> upperTabs = new List<JObject>(); //Tab's list
            List<JObject> upperFields = new List<JObject>(); //Field's list
            foreach (JObject tab in upperObject)
            {
                upperTabs.Add(tab);
                JArray fields = (JArray)tab.GetValue("fields");

                //getting all parent's fields
                foreach (JObject field in fields)
                {
                    try
                    {
                        field.Add("containerTab", tab.GetValue("tabName").ToString());
                    }
                    catch (Exception e) { }
                    upperFields.Add(field);
                }
            }

            foreach (JObject tab in lowerObject)
            {
                List<JObject> fieldList = new List<JObject>();

                //remove existing fields from parent
                foreach (JObject field in (JArray)tab.GetValue("fields"))
                {
                    JObject existingField = upperFields.Find(x => x.GetValue("fieldID").ToString() == field.GetValue("fieldID").ToString());
                    if (existingField != null)
                    {
                        JObject tabContainer = upperTabs.Find(x => x.GetValue("tabName").ToString() == existingField.GetValue("containerTab").ToString());
                        JArray fields = (JArray)tabContainer.GetValue("fields");
                        List<JObject> removeList = new List<JObject>();

                        //getting elements to remove
                        foreach (JObject tabField in fields)
                        {
                            if (tabField.GetValue("fieldID").ToString() == existingField.GetValue("fieldID").ToString())
                            {
                                removeList.Add(tabField);
                            }
                        }

                        //removing elements
                        foreach (JObject fieldToRemove in removeList)
                        {
                            fields.Remove(fieldToRemove);
                        }
                    }
                }

                JObject modTab = upperTabs.Find(x => x.GetValue("tabName").ToString() == tab.GetValue("tabName").ToString());
                if (modTab == null) //the tab does not exist
                {
                    upperTabs.Add(tab); //add it to the tabs with all the fields
                }
                else
                { //the tab does exist, set all the fields in the tab
                    JArray fieldsArray = (JArray)modTab.GetValue("fields");
                    foreach (JObject field in (JArray)tab.GetValue("fields"))
                    {
                        fieldsArray.Add(field);
                    }
                }
            }

            String a = JsonConvert.SerializeObject(upperTabs);
            return a;
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
        public void deleteCategory(String idProfile, String objects)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objectsreference", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("objectsreference", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (idProfile == null || idProfile == "")
                {
                    return;
                }
                List<String> categoryStack = new List<String>();
                categoryStack.Add(idProfile);
                while (categoryStack.Count > 0)
                {
                    String actualCategory = categoryStack.First();
                    String childrenString = categoryTable.get("parentCategory", actualCategory);
                    JArray childrenArray = JsonConvert.DeserializeObject<JArray>(childrenString);
                    foreach (JObject child in childrenArray)
                    {
                        categoryStack.Add(child["_id"].ToString());
                    }
                    String objectString = objectTable.Get("parentCategory", actualCategory);
                    JArray objectArray = JsonConvert.DeserializeObject<JArray>(objectString);
                    foreach (JObject child in objectArray)
                    {
                        try
                        {
                            objectTable.deleteRow(child["_id"].ToString());
                        }
                        catch (Exception e) { /*Ignored*/ }
                    }
                    try
                    {
                        categoryTable.deleteRow(actualCategory);
                        categoryStack.RemoveAt(0);
                    }
                    catch (Exception e) {/*Ignored*/}
                }
            }
        }

        public void loadusers()
        {

            try
            {
                String userOptions = "";
                String userList = userTable.GetRows();//getting all the profiles _

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
                    }

                }

                ViewData["departList"] = new HtmlString(DepartmentsOptions);
            }
            catch (Exception e)
            {
                ViewData["departList"] = null;
            }
        }
        public void loadProveedor()
        {
            try
            {
                String provedoresOptions = "";
                String rowArray = _listTable.Get("name", "proveedores");
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray listas = new JArray();
                Dictionary<string, string> dictprov = new Dictionary<string, string>();
                foreach (JObject obj in rowString)
                {
                    listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["order"].ToString());
                }
              
                provedoresOptions += "<option value='null' selected> Seleccione Proveedor</option>";
                foreach (JObject puesto in listas)
                {
                    foreach (KeyValuePair<string, JToken> token in puesto)
                    {
                        if (token.Key == "position")
                            continue;
                        provedoresOptions += "<option value='" + token.Key + "'"; //setting the id as the value
                        provedoresOptions += ">" + token.Value + "</option>"; //setting the text as the name
                    }

                }

                ViewData["proveedorList"] = new HtmlString(provedoresOptions);
            }
            catch (Exception e)
            {
                ViewData["proveedorList"] = null;
            }
        }
        //*****************
        public void loadCategories()
        {
            try
            {
                String CategoriesOptions = "";
                String rowArray = categoryTable.GetRows(); 
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                
                CategoriesOptions += "<option value='null' selected> Seleccione Tipo de Activo</option>";
                foreach (JObject puesto in rowString)
                {
                    CategoriesOptions += "<option value='" + puesto["_id"].ToString() + "'"; //setting the id as the value
                    CategoriesOptions += ">" + puesto["name"].ToString() + "</option>"; //setting the text as the name
                }

                ViewData["categoryList"] = new HtmlString(CategoriesOptions);
            }
            catch (Exception e)
            {
                ViewData["categoryList"] = null;
            }
        }

        public JsonResult globalSearch(String parentCategory, String data)
        {

            if (parentCategory == "") parentCategory = "null";
            List<String> categoriesStack = new List<String>();
            categoriesStack.Add(parentCategory);

            JArray categoriesResult = new JArray();
            JArray objectsResult = new JArray();
            String currentCategory;

            while (categoriesStack.Count > 0)
            {
                currentCategory = categoriesStack[0];
                categoriesStack.RemoveAt(0);

                String categories = categoryTable.get("parentCategory", currentCategory);
                JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categories);
                foreach (JObject document in categoriesObject)
                {
                    if (document["name"].ToString().ToLower().Contains(data.ToLower()))
                    {

                        document.Remove("customFields");
                        try
                        { //trying to set the creator's name
                            String rowString = userTable.GetRow(document["Creator"].ToString());
                            JObject rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
                            document["Creator"] = rowArray["name"] + " " + rowArray["lastname"];
                        }
                        catch (Exception e)
                        {
                            document["Creator"] = "";
                        }

                        try
                        {
                            if (document["ext"].ToString() != "")
                                document.Add("image", "/Uploads/Images/Categories/" + document["_id"].ToString() + "." + document["ext"].ToString());
                            document.Remove("ext");
                        }
                        catch (Exception e) { /*Ignored*/ }

                        try //trying to set category's objects number
                        {
                            String objects = objectTable.Get("parentCategory", document["_id"].ToString());
                            JArray childObject = JsonConvert.DeserializeObject<JArray>(objects);
                            document["objectCount"] = childObject.Count;
                        }
                        catch (Exception e)
                        {

                        }

                        categoriesResult.Add(document);
                    }
                    categoriesStack.Add(document["_id"].ToString());
                }

                 String objectsString = objectTable.Get("parentCategory", currentCategory); 
                JArray objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                foreach (JObject objectDocument in objectsObject)
                {
                    if (objectDocument["object_id"].ToString().ToLower().Contains(data.ToLower()) ||
                        objectDocument["name"].ToString().ToLower().Contains(data.ToLower()) ||
                        objectDocument["marca"].ToString().ToLower().Contains(data.ToLower()) ||
                        objectDocument["modelo"].ToString().ToLower().Contains(data.ToLower()))
                    {
                        try
                        { //trying to set the creator's name
                            String rowString = userTable.GetRow(objectDocument["Creator"].ToString());
                            JObject rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
                            objectDocument["Creator"] = rowArray["name"] + " " + rowArray["lastname"];
                        }
                        catch (Exception e)
                        {
                            objectDocument["Creator"] = "";
                        }
                        try
                        {
                            objectDocument.Remove("parentCategory");
                        }
                        catch (Exception e) { /*Ignored*/ }

                        try
                        {
                            if (objectDocument["ext"].ToString() != "")
                                objectDocument.Add("image", "/Uploads/Images/ObjectReferences/" + objectDocument["_id"].ToString() + "." + objectDocument["ext"].ToString());
                            objectDocument.Remove("ext");
                        }
                        catch (Exception e) { /*Ignored*/ }
                        objectsResult.Add(objectDocument);
                    }
                }
            }
            JObject result = new JObject();
            result.Add("categories", categoriesResult);
            result.Add("objects", objectsResult);
            return Json(JsonConvert.SerializeObject(result));
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

        public JsonResult getRoute2(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {

                string category = locationTable.GetRow(parentCategory);
                JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                JObject categoryObject = new JObject();
                categoryObject.Add("id", actualCategory["_id"].ToString());
                route.Add(categoryObject);
                parentCategory = actualCategory["parent"].ToString();
            }

            JObject result = new JObject();
            result.Add("route", route);
            return Json(JsonConvert.SerializeObject(result));
        }

        public void AsignarUbicacion(List<String> selectids, String idlocation)
        {
            String objstring = "";
            JObject objs = new JObject();
            String jsonData;
            foreach (String cad in selectids)
            {
                objstring = objectTable.GetRow(cad);
                objs = JsonConvert.DeserializeObject<JObject>(objstring);
                objs["location_id"] = idlocation;

                jsonData = JsonConvert.SerializeObject(objs);

                objectTable.SaveRow(jsonData, cad);
            }

        }

        public void saveObjects(string obj, string location, int cantidad, String EPC)
        {
            String cadena = "";
            String name = "";

            if (location == null || location == "")
            {
                location = "null";
            }

            String objString = objectTable.GetRow(obj);
            if (objString != null)
            {
                JObject objref = JsonConvert.DeserializeObject<JObject>(objString);
                name = objref["name"].ToString();
                int consecutivo = int.Parse(EPC.Substring(15), System.Globalization.NumberStyles.HexNumber);
                string hexValue;
                int counter = 0;
                for (int i = 0; i < cantidad; i++)
                {
                    try
                    {
                    hexValue = consecutivo.ToString("X10");
                    cadena = "{'objectReference':'" + obj + "','name':'" + name + "','location':'" + location + "','process':null"
                                + ", 'EPC':'" + EPC.Substring(0, 14) + hexValue + "'}";
                    objectRealTable.SaveRow(cadena, "null");
                    consecutivo++;
                        counter = 1;
                    }
                    catch (Exception e) { }
                }

                notificationObject.saveNotification("Objects", "Create", "Se han impreso " + counter + " etiquetas");

            }
        }

        public String RevisarReglaUbicacion(String idObj, String idLocation) {
            bool ok = false;
            ok = RulesChecker.isValidToLocation(idObj, idLocation);
            return ok.ToString();
        }

        public String loadLocationsAlls()
        {

            try
            {
                String locationsOptions = "";
                string getconjunt = _locationProfile.Get("name", "Conjunto");
                JArray conjuntja = new JArray();
                string idconjunto = "";
                string idregion = "";
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idconjunto = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                getconjunt = _locationProfile.Get("name", "Region");
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idregion = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                String rowArray = locationTable.GetLocationsTable("", "", idregion, idconjunto);
                JArray locatList = JsonConvert.DeserializeObject<JArray>(rowArray);

                locationsOptions += "<option value='null' selected> Seleccione un Ubicación</option>";

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

        public String RevisarIDArticulo(String codigo, string id) {
            String result = "";
            String objList = objectTable.GetRows();//getting all the profiles _
            JArray objs = JsonConvert.DeserializeObject<JArray>(objList);

            foreach(JObject ob in objs){
                try
                {
                    if (ob["object_id"].ToString() == codigo && ob["_id"].ToString()!=id)
                    {
                        result = "si";
                        break;
                    }
                }
                catch {
                    continue;
                }
                
            }
            
            return result;
        }
    }
}
