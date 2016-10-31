using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Db.MongoDb;
using Rivka.Security;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using RivkaAreas.Employee.Models;
using Rivka.Form;
using System.Text.RegularExpressions;
using System.Globalization;
using Rivka.Images;
using Rivka.Mail;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using System.Data;
using Rivka.Files;
using System.IO.Compression;



namespace RivkaAreas.Employee.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {

        protected validatePermissions validatepermissions;
        protected MongoModel witnessesModel;
        protected EmployeeProfileTable employeeprofileTable; //profile's model object
        protected ListTable _listTable;
        protected EmployeeTable employeetable;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        protected Notifications Notificate;
        protected UserTable userTable;
        protected ProfileTable profileTable;
        protected ListTable listTable;
        public EmployeeController()
        {
            //  validatelim = new ValidateLimits();
            validatepermissions = new validatePermissions();
            employeeprofileTable = new EmployeeProfileTable();
            witnessesModel = new MongoModel("witnesses");
            _listTable = new ListTable();
            employeetable = new EmployeeTable();
            Notificate = new Notifications();
            _logTable = new LogBook.Controllers.LogBookController();
            userTable = new UserTable();
            profileTable = new ProfileTable();
            listTable = new ListTable();
        }
        public ActionResult Index()
        {
            //String dataPermissions = Session["Permissions"].ToString();
            //String dataPermissionsClient = Session["PermissionsClient"].ToString();
            //bool access = false;
            //bool accessClient = false;
            //  access = getpermissions("users", "r");
            //access = validatepermissions.getpermissions("employee", "r", dataPermissions);
            //accessClient = validatepermissions.getpermissions("employee", "r", dataPermissionsClient);
            //if (access == true) && accessClient == true)
            if(true)
            {
                /////bind empli
                try
                {
                    String profileOptions = "";
                    List<BsonDocument> profileList = employeeprofileTable.getRows(); //getting all the profiles
                    profileOptions += "<option value='null' selected> Elija una opción</option>";
                    foreach (BsonDocument document in profileList) //for each profile we create an option element with id as value and the name as the text
                    {
                        profileOptions += "<option value='" + document.GetElement("_id").Value + "'"; //setting the id as the value
                        profileOptions += ">" + document.GetElement("name").Value + "</option>"; //setting the text as the name
                    }
                    ViewData["profileList"] = new HtmlString(profileOptions);
                }
                catch (Exception ex)
                {
                    ViewData["profileList"] = null;
                }


                //Detects if the user is an Admin
                try
                {
                    JObject userdata = JsonConvert.DeserializeObject<JObject>(userTable.GetRow(this.Session["_id"].ToString()));
                    JObject userprofile = JsonConvert.DeserializeObject<JObject>(profileTable.GetRow(userdata["profileId"].ToString()));

                    if (userprofile["name"].ToString() == "Administrador de sistema")
                    {
                        ViewData["signalMasive"] = true;
                    }
                    else { ViewData["signalMasive"] = false; }
                }
                catch (Exception ex)
                {
                    ViewData["signalMasive"] = false;
                }
                loadAreas();
                return View();
            }
            else
            {
                return Redirect("~/Home");
            }



        }

        public void loadAreas()
        {
            try
            {
                String AreasOptions = "";
                String rowArray = listTable.Get("name", "Area");
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                
                JArray listas = new JArray();
                foreach (JObject obj in rowString)
                {
                    listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["unorder"].ToString());
                }
                //listas = listas.OrderByDescending
                AreasOptions += "<option value='null' selected>Seleccione...</option>";
                foreach (JObject puesto in listas)
                {
                    foreach (KeyValuePair<string, JToken> token in puesto)
                    {
                        AreasOptions += "<option value='" + token.Key + "'"; //setting the id as the value
                        AreasOptions += ">" + token.Value + "</option>"; //setting the text as the name
                    }

                }

                ViewData["areaList"] = new HtmlString(AreasOptions);
            }
            catch (Exception e)
            {
                ViewData["areaList"] = null;
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
                string relativepath = "\\Uploads\\Images\\Employee\\CustomImages\\";
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



        public String saveEmployee(FormCollection formData, HttpPostedFileBase file)
        {

            bool access = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool accessClient = false;
            access = validatepermissions.getpermissions("employee", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("employee", "u", dataPermissionsClient);
            //if (access == true && accessClient == true)
            if(true)
            {
                if (this.Request.IsAjaxRequest())
                {
                    formData = CustomForm.unserialize(formData);  //use the method serialize to parse the string into an array

                    String employeeID = (formData["employeeID"] == "null") ? null : formData["employeeID"]; //check insert new or update existing
                    String EmployeeName = "";
                    JObject employee = new JObject();
                    //get employee if update
                    if (employeeID != null)
                    {
                        String employeestring = employeetable.GetRow(employeeID);
                        employee = JsonConvert.DeserializeObject<JObject>(employeestring);
                    }

                    /*check when update , employee exist or not*/
                    if (employeeID != null && (employee == null))
                    {
                        return "{\"msg\":\"El id especificado no existe\", \"status\":\"error\"}";
                    }


                    /*The selected emoloyee Id is already in use and is not the employee who has it*/
                    if (employeeExists(formData["employee"]) == "true" && (employeeID == null || employeetable.get("employee", formData["employee"])[0].GetElement("_id").Value.ToString() != employeeID))
                    {
                        return "{\"msg\":\"El empleado ya está siendo utilizado\", \"status\":\"error\"}";
                    }

                    //due that the Employee's id is unique we use it has the image's name, so we store only the extension in the db
                    string ext = null;
                    if (file != null)
                    {
                        ext = file.FileName.Split('.').Last(); //getting the extension
                    }
                    else if (employeeID != null)
                    {
                        try
                        {
                            ext = employee["imgext"].ToString();
                        }
                        catch (Exception e) { }
                    }



                    //JArray listp = new JArray();
                    /* Format validations */



                    if (!Regex.IsMatch(formData["name"], "[A-ZÁÉÍÓÚÑa-záéíóúñ]+( [A-ZÁÉÍÓÚÑa-záéíóúñ]+){0,2}"))
                    {
                        return "{\"msg\":\"Formato incorrecto para: name\", \"status\":\"error\"}";
                    }
                    else if (!Regex.IsMatch(formData["lastname"], "[A-ZÁÉÍÓÚÑa-záéíóúñ]+( [A-ZÁÉÍÓÚÑa-záéíóúñ]+){0,1}"))
                    {
                        return "{\"msg\":\"Formato incorrecto para: Apellido Paterno\", \"status\":\"error\"}";
                    }
                    else if (!Regex.IsMatch(formData["motherlastname"], "[A-ZÁÉÍÓÚÑa-záéíóúñ]+( [A-ZÁÉÍÓÚÑa-záéíóúñ]+){0,1}"))
                    {
                        return "{\"msg\":\"Formato incorrecto para: Apellido Materno\", \"status\":\"error\"}";
                    }
                    else if (!Regex.IsMatch(formData["employee"], "([a-zA-Z0-9-_.]){4,}"))
                    {
                        return "{\"msg\":\"Formato incorrecto para: ID Empleado\", \"status\":\"error\"}";
                    }
                    ///check selected profile id exist or not
                    ///
                    else if (formData["profileId"] == "null")
                    {
                        return "{\"msg\":\"Elija El perfil\", \"status\":\"error\"}";
                    }
                    else if (employeeprofileTable.getRow(formData["profileId"]) == null)
                    {
                        return "{\"msg\":\"El perfil especificado no existe\", \"status\":\"error\"}";
                    }
                    else if (formData["type"] == "null")
                    {
                        return "{\"msg\":\"Elija El Tipo de Empleado\", \"status\":\"error\"}";
                    }
                    else if (formData["area"] == "null")
                    {
                        return "{\"msg\":\"Elija El Área\", \"status\":\"error\"}";
                    }
                    else
                    {
                        EmployeeName = formData["employee"];

                    }

                    /* Format validations */
                    //Change name representation
                    formData["name"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(formData["name"].ToString().ToLower().Trim());
                    formData["lastname"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(formData["lastname"].ToString().ToLower().Trim());

                    //there are fields that we know that exists so we set them into the json
                    String jsonData = "{'employee':'" + formData["employee"] + "','name':'" + formData["name"].Replace("+", " ") + "','imgext':'" + ext
                        + "','lastname':'" + formData["lastname"].Replace("+", " ")
                        + "','motherlastname':'" + formData["motherlastname"].Replace("+", " ") + "','type':'" + formData["type"]
                        + "','profileId':'" + formData["profileId"] + "','area':'" + formData["area"];

                    try //trying to set the creator's id
                    {
                        jsonData += "','creatorId':'";
                        jsonData += this.Session["_id"];
                        jsonData += "'";
                    }
                    catch (Exception e) { /*Ignored*/ }

                    //remove the setted data in the json from the formData
                    formData.Remove("employeeID");
                    formData.Remove("employee");
                    formData.Remove("name");
                    formData.Remove("lastname");
                    formData.Remove("motherlastname");
                    formData.Remove("type");
                    formData.Remove("profileId");
                    formData.Remove("area");


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
                    string id = employeetable.saveRow(jsonData, employeeID);
                    //Notify this action
                    if (employeeID == null)
                    {
                        Notificate.saveNotification("Employees", "Create", "El empleado '" + EmployeeName + "' ha sido creado");
                        _logTable.SaveLog(Session["_id"].ToString(), "empleados", "Insert: " + EmployeeName, "Employee", DateTime.Now.ToString());
                    }
                    else
                    {
                        Notificate.saveNotification("Employees", "Update", "El empleado '" + EmployeeName + "' ha sido modificado");
                        _logTable.SaveLog(Session["_id"].ToString(), "empleados", "Update: " + EmployeeName, "Employee", DateTime.Now.ToString());
                    }

                    //TODO:Aqui se guarda la imagen
                    if (file != null)
                    {
                        string relativepath = "\\Uploads\\Images\\";
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


                    return "{\"msg\":\"" + id + "\", \"status\":\"success\"}"; //returns the saved user's id

                }
                return null;
            }

            else { return null; }

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
        public JsonResult getEmployeeTable()
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                string userid = Session["_id"].ToString();
                List<BsonDocument> docs = employeetable.getRows(); //getting all the employee
                JArray docum = new JArray();
                foreach (BsonDocument document in docs)
                {
                    try
                    {
                        if (document.GetElement("imgext").Value != "")
                            document.Set("image", "/Uploads/Images/" + document.GetElement("_id").Value + "." + document.GetElement("imgext").Value);
                    }
                    catch (Exception e) { }


                    try
                    { //trying to set the creator's name
                        BsonDocument creator = userTable.getRow(document.GetElement("creatorId").Value.ToString());
                        //document.Set("Creator", "YO");//
                        document.Set("Creator", creator.GetElement("name").Value);
                    }
                    catch (Exception e)
                    {
                        document["Creator"] = "";
                    }
                    try
                    {
                        BsonDocument perfil = employeeprofileTable.getRow(document.GetElement("profileId").Value.ToString());
                        //document.Set("Creator", "YO");//
                        document.Set("profilename", perfil.GetElement("name").Value);
                    }
                    catch (Exception e) { document["profilename"] = ""; }



                    try
                    {
                        document.Remove("creatorId");
                    }
                    catch (Exception e) { }


                    String newDocString = document.ToString();
                    JObject newDoc = JsonConvert.DeserializeObject<JObject>(newDocString);
                    docum.Add(newDoc);
                }
                JObject result = new JObject();
                result.Add("employees", docum);
                String resultString = JsonConvert.SerializeObject(result);
                return Json(resultString);
            }
            else
            {
                return null;
            }
        }

        public JsonResult globalSearch(String data)
        {

            List<BsonDocument> employeeResult = new List<BsonDocument>();
            List<BsonDocument> docs = employeetable.getRows(); //getting all the users

            foreach (BsonDocument document in docs)
            {

                if (document["employee"].ToString().ToLower().Contains(data.ToLower()) ||
                    document["name"].ToString().ToLower().Contains(data.ToLower()) ||
                    document["lastname"].ToString().ToLower().Contains(data.ToLower()) ||
                    document["motherlastname"].ToString().ToLower().Contains(data.ToLower()))
                {
                    try
                    {
                        if (document.GetElement("imgext").Value != "")
                            document.Set("image", "/Uploads/Images/" + document.GetElement("_id").Value + "." + document.GetElement("imgext").Value);
                    }
                    catch (Exception e) { }
                    try
                    {
                        document.Remove("profileFields");
                    }
                    catch (Exception e) { }

                    try
                    { //trying to set the creator's name
                        BsonDocument creator = userTable.getRow(document.GetElement("creatorId").Value.ToString());
                        document.Set("Creator", creator.GetElement("name").Value);
                    }
                    catch (Exception e)
                    {
                        document["Creator"] = "";
                    }
                    try
                    {
                        document.Remove("creatorId");
                    }
                    catch (Exception e) { }


                    employeeResult.Add(document);
                }
            }


            return Json(employeeResult.ToJson());

        }
        public String deleteEmployee(String selectedID)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("employee", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("employee", "d", dataPermissionsClient);

            string employeestring = employeetable.GetRow(selectedID);
            JObject employee = JsonConvert.DeserializeObject<JObject>(employeestring);

            //if (access == true && accessClient == true)
            if(true)
            {
                if (this.Request.IsAjaxRequest()) //only available with AJAX
                {
                    try
                    {
                        employeetable.deleteRow(selectedID); //deletes the selected document
                        //Notificate the action
                        Notificate.saveNotification("Employess", "Delete", "Un Empleado ha sido borrado");
                        _logTable.SaveLog(Session["_id"].ToString(), "Empleados", "Delete: " + employee["employee"].ToString(), "Employess", DateTime.Now.ToString());
                        return "Registro Borrado";
                    }
                    catch (Exception e)
                    {
                        return "Ha ocurrido un error";
                    }
                }
                return null;
            }
            else { return null; };
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
        public String deleteEmployees(List<String> array)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("employee", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("employee", "d", dataPermissionsClient);


            //if (access == true && accessClient == true)
            if(true)
            {
                if (this.Request.IsAjaxRequest()) //only available with AJAX
                {
                    try //tryign to delete the users
                    {
                        if (array.Count == 0) return null; //if array is empty there are no users to delete
                        foreach (String id in array) //froeach id in the array we must delete the document with that id from the db
                        {
                            string empstring = employeetable.GetRow(id);
                            JObject emp = JsonConvert.DeserializeObject<JObject>(empstring);
                            employeetable.deleteRow(id);
                            _logTable.SaveLog(Session["_id"].ToString(), "Empleados", "Delete: " + emp["employee"].ToString(), "Employees", DateTime.Now.ToString());
                        }
                        //Notificate the action
                        Notificate.saveNotification("Employees", "Delete", array.Count + " empleados han sido borrados");

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
        public String getEmployee(String employeeID)
        {
            if (this.Request.IsAjaxRequest())
            {
                try
                {
                    BsonDocument doc = employeetable.getRow(employeeID); //getting the user's data

                    //the next is the photo's information
                    string relativepath = "/Uploads/Images/";
                    string absolutepathdir = Server.MapPath(relativepath);
                    string filename = doc["_id"].ToString() + "." + doc["imgext"].ToString();
                    string fileabsolutepath = absolutepathdir + filename;

                    if (doc == null)
                        return "null";
                    doc.Remove("_id");

                    if (System.IO.File.Exists(fileabsolutepath))
                    {
                        string url = Url.Content(relativepath + filename);
                        doc.Add(new BsonElement("ImgUrl", url)); //adding the image's url to the document
                    }
                    return doc.ToJson(); //returns the json
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public String employeeExists(String employee)
        {
            try
            {
                List<BsonDocument> list = employeetable.get("employee", employee);
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
                    BsonDocument document = employeeprofileTable.getRow(profile);
                    String formString = document.GetElement("customFields").Value.ToString();
                    String response = CustomForm.getFormView(formString, "CustomFields"); //we use the CustomForm class to generate the form's fiew
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
                    BsonDocument document = employeeprofileTable.getRow(profile);
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

        /// <summary>
        /// This method get the content of the Excel file
        /// </summary>
        /// <param name="file">Excel File</param>
        /// <returns>Table of the content of the Excel file</returns>
        /// <author>Edwin (Origin) - Abigail Rodriguez(Edit)</author>
        public ActionResult ImpExcel(HttpPostedFileBase file)
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
                if (Request.Files["file"].ContentLength > 0/* && (idcategory != "0" && idcategory != null)*/)
                {
                    string fileExtension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
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
                //throw new Exception(ex.Message);
                // System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
                //System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
                return null;
            }
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

        /// <summary>
        /// This method save all the users imported data 
        /// </summary>
        /// <param name="file">Excel File</param>
        /// <returns></returns>
        /// <author>Edwin (Origin) - Abigail Rodriguez(Edit)</author>
        public String saveImport(String data, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {

                String dataimport = data.ToString();
                JArray dataimportarray = JsonConvert.DeserializeObject<JArray>(dataimport);
                int count = 0; int totalAdd = 0; int totalFail = 0; int totalAddIMG = 0; int totalFailIMG = 0;
                JArray result = new JArray();
                JObject bossNotFound = new JObject();
                foreach (JObject items in dataimportarray)
                {
                    count++; bool error = false;

                    /*The selected employee name is already in use and is not the employee who has it*/
                    if (employeeExists(items["employee"].ToString()) == "true")
                    {
                        result.Add("{\"error\":\"El ID del Empleado ya está siendo utilizado\", \"registro\":\"" + count + "\"}");
                        error = true;
                    }
                    /* Format validations */
                    else if (!Regex.IsMatch(items["employee"].ToString(), "([a-zA-Z0-9-_.]){4,}") || items["employee"].ToString() == "")
                    {
                        result.Add("{\"error\":\"Formato incorrecto para ID del Empleado\", \"registro\":\"" + count + "\"}");
                        error = true;
                    }

                    else if (!Regex.IsMatch(items["name"].ToString(), "[A-ZÁÉÍÓÚÑa-záéíóúñ]+( [A-ZÁÉÍÓÚÑa-záéíóúñ]+){0,2}"))
                    {
                        result.Add("\"error\":\"Formato incorrecto para: name\", \"registro\":\"" + count + "\"}");
                        error = true;
                    }
                    else if (!Regex.IsMatch(items["lastname"].ToString(), "[A-ZÁÉÍÓÚÑa-záéíóúñ]+( [A-ZÁÉÍÓÚÑa-záéíóúñ]+){0,1}"))
                    {
                        result.Add("{\"error\":\"Formato incorrecto para apellido\",\"registro\":\"" + count + "\"}");
                        error = true;
                    }
                    else if (!Regex.IsMatch(items["motherlastname"].ToString(), "[A-ZÁÉÍÓÚÑa-záéíóúñ]+( [A-ZÁÉÍÓÚÑa-záéíóúñ]+){0,1}"))
                    {
                        result.Add("{\"error\":\"Formato incorrecto para apellido\",\"registro\":\"" + count + "\"}");
                        error = true;
                    }

                    if (error) { totalFail++; continue; }


                    //Section to get profile ID
                    string profileID = "";

                    //JArray profileOtro = JsonConvert.DeserializeObject<JArray>(employeeprofileTable.Get("name", "Null"));
                    //JObject profileOt = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(profileOtro[0]));
                    //profileID = profileOt["_id"].ToString();

                    if (items["profileId"] != null && items["profileId"].ToString() != "")
                    {
                        string profileResult = employeeprofileTable.Get("name", items["profileId"].ToString());
                        if (profileResult != "[]" && profileResult != "" && profileResult != null)
                        {
                            JArray conjuntja = JsonConvert.DeserializeObject<JArray>(profileResult);
                            profileID = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                        }
                    }
                    //Change name representation
                    items["name"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(items["name"].ToString().ToLower().Trim());
                    items["lastname"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(items["lastname"].ToString().ToLower().Trim());

                    //there are fields that we know that exists so we set them into the json
                    String jsonData = "{'employee':'" + items["employee"] + "','name':'" + items["name"].ToString().Replace("+", " ") + "','imgext':'"
                        + "','lastname':'" + items["lastname"].ToString().Replace("+", " ")
                        + "','motherlastname':'" + items["motherlastname"].ToString().Replace("+", " ") + "','type':'" + items["type"]
                        + "','profileId':'" + profileID + "','area':'" + items["area"];

                    try //trying to set the creator's id
                    {
                        jsonData += "','creatorId':'";
                        jsonData += this.Session["_id"];
                        jsonData += "'}";
                    }
                    catch (Exception e) { /*Ignored*/ }

                    string id = employeetable.saveRow(jsonData, null); //Save new user
                    _logTable.SaveLog(Session["_id"].ToString(), "Empleados", "Insert: " + items["employee"].ToString(), "Employees", DateTime.Now.ToString());

                    result.Add("{\"success\":\"Empleado guardado en registo: " + count + "\"}");
                    totalAdd++;

                }

                //Save imagen files
                string ext = null;
                string relativepath = @"\Uploads\Images\TempEmployee";
                string absolutepath = Server.MapPath(relativepath);
                ////Delete all files
                if (System.IO.Directory.Exists(absolutepath + "\\"))
                {
                    System.IO.Directory.Delete(absolutepath + "\\", true);
                }
                ////Create de upload directory
                if (!System.IO.Directory.Exists(absolutepath + "\\"))
                    System.IO.Directory.CreateDirectory(absolutepath + "\\");

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i]; //Uploaded file
                    string fileName = file.FileName;
                    ext = file.FileName.Split('.').Last(); //getting the extension   

                    if (ext == "rar" || ext == "zip")
                    {
                        try
                        {
                            //Saves de compress file
                            file.SaveAs(absolutepath + "\\" + fileName);

                            //Extract file
                            ZipFile.ExtractToDirectory(absolutepath + "\\" + fileName, absolutepath + "\\");
                            string[] filesArray = System.IO.Directory.GetFiles(absolutepath + "\\" + fileName.Split('.').First() + "\\");

                            foreach (string s in filesArray)
                            {
                                string namefileS = Path.GetFileName(s);
                                JArray employeesInfo = JsonConvert.DeserializeObject<JArray>(employeetable.Get("employee", namefileS.Split('.').First()));
                                if (employeesInfo.Count() > 0 && employeesInfo != null)
                                {
                                    JObject employeeInfo = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(employeesInfo[0]));
                                    employeeInfo["imgext"] = s.Split('.').Last();
                                    employeetable.SaveRow(JsonConvert.SerializeObject(employeeInfo), employeeInfo["_id"].ToString());
                                    _logTable.SaveLog(Session["_id"].ToString(), "Empleados", "Insert: " + employeeInfo["employee"].ToString(), "Employees", DateTime.Now.ToString());
                                    System.IO.File.Move(s, Server.MapPath(@"\Uploads\Images\" + employeeInfo["_id"].ToString() + "." + s.Split('.').Last()));
                                    totalAddIMG++;
                                }



                            }

                        }
                        catch (Exception e)
                        {
                            totalFailIMG++;
                            result.Add("{\"error\":\"Error al gaurdar imagen\"}");
                        }
                    }
                }


                JObject finalResult = new JObject();
                finalResult.Add("employeeSuccess", totalAdd.ToString());
                finalResult.Add("employeeError", totalFail.ToString());
                finalResult.Add("imgSuccess", totalAddIMG.ToString());
                finalResult.Add("imgError", totalFailIMG.ToString());
                finalResult.Add("details", JsonConvert.SerializeObject(result));

                return JsonConvert.SerializeObject(finalResult);
            }
            catch (Exception ex)
            {

                return null;
            }
        }
    }
}
