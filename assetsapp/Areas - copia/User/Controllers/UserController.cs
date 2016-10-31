using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using RivkaAreas.User.Models;

using System.Drawing;
using System.IO;
using System.Web.Security;

using Rivka.Form;
using Rivka.Form.Field;
using Rivka.Security;
using Rivka.Db.MongoDb;
using System.Runtime.Remoting;
using Rivka.Images;
using Rivka.Mail;
using Rivka.Files;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using System.Text;
using System.IO.Compression;
using System.Globalization;

namespace RivkaAreas.User.Controllers
{
    [Authorize]
    public class UserController : Controller
    {

        protected ProfileTable profileTable; //profile's model object
        protected UserTable userTable; //user's model object
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        protected CustomerTable customerTable;
        protected Notifications Notificate;
        protected ValidateLimits validatelim;
        protected validatePermissions validatepermissions;
        protected MongoModel witnessesModel;
        protected ListTable listTable;
        protected SystemSettingsTable systemSettingsTable;
        protected LocationTable locationTable;
        protected LocationProfileTable locationProfileTable;
        
        /// <summary>
        ///     The constructor initilizes the models
        /// </summary>
        public UserController()
        {
            profileTable = new ProfileTable();
            userTable = new UserTable();
            customerTable = new CustomerTable();
            validatelim = new ValidateLimits();
            validatepermissions = new validatePermissions();
            Notificate = new Notifications();
            witnessesModel = new MongoModel("witnesses");
            listTable = new ListTable();
            systemSettingsTable = new SystemSettingsTable();
            locationTable = new LocationTable();
            locationProfileTable = new LocationProfileTable();
            _logTable = new LogBook.Controllers.LogBookController();
        }

        public string getWitnesses() {
            JObject result = new JObject();

            string[] types = {"operative", "assets", "accounting"};

            foreach (string type in types)
            {
                string witnessString = witnessesModel.Get("witnessType", type);
                JObject witnessObject = JsonConvert.DeserializeObject<JArray>(witnessString).First as JObject;
                if (witnessObject != null)
                {
                    result.Add(type, witnessObject["user"].ToString());
                }
            }
            string resultString = JsonConvert.SerializeObject(result);
            return resultString;
        }

        public String saveWitnesses(string witnessJson) {
            JObject witnesses = JsonConvert.DeserializeObject<JObject>(witnessJson);
            foreach(KeyValuePair<string,JToken> token in witnesses){
                JObject newWitness = new JObject();
                newWitness["witnessType"] = token.Key;
                newWitness["user"] = token.Value.ToString();

                string existingWitnessString = witnessesModel.Get("witnessType", token.Key);
                JObject existingWitness = JsonConvert.DeserializeObject<JArray>(existingWitnessString).First as JObject;
                string id = null;
                if (existingWitness != null)
                {
                    id = existingWitness["_id"].ToString();
                }
                string witnessString = JsonConvert.SerializeObject(newWitness);
                witnessesModel.SaveRow(witnessString,id);
                _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Update: Testigo " + newWitness["user"], "witnesses", DateTime.Now.ToString());
            }
            return null;
        }

        public string validateLimit()
        {
            try
            {
                string result = validatelim.validate("Users", null,null, "Users",1);


                return result;
            }
            catch (Exception ex)
            {
                return "error";
            }

        }
        /// <summary>
        ///     This is the index of the user section, it forms the profileList and the formView of users
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the view to be displayed
        /// </returns>
        public ActionResult Index()
        {
            //setUserKey("5498ab783d32b34928b6fc6d");
            //ACD.myObj anObj = new ACD.myObj();
            //var something = anObj.returnSomething();
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
          //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("users", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("users", "r", dataPermissionsClient);

            if (access == true && accessClient==true)
            {
                try
                {
                    String profileOptions = "";
                    List<BsonDocument> profileList = profileTable.getRows(); //getting all the profiles
                    List<BsonDocument> customerList = customerTable.getRows(); //getting all customers

                    List<BsonDocument> Customers = new List<BsonDocument>();
                    foreach (BsonDocument customer in customerList) //for each profile we create an option element with id as value and the name as the text
                    {
                        BsonDocument newCustomer = new BsonDocument();
                        newCustomer.Add("id", customer.GetElement("_id").Value.ToString());
                        newCustomer.Add("name", customer.GetElement("name").Value.ToString());

                        Customers.Add(newCustomer);
                    }
                    var customerJson = Customers.ToJson();
                    ViewData["customerList"] = new HtmlString(customerJson);

                    foreach (BsonDocument document in profileList) //for each profile we create an option element with id as value and the name as the text
                    {
                        profileOptions += "<option value='" + document.GetElement("_id").Value + "'"; //setting the id as the value
                        if (document.GetElement("name").Value == "Básico")
                            profileOptions += " selected"; //setting the Básico profile to the selected one
                        profileOptions += ">" + document.GetElement("name").Value + "</option>"; //setting the text as the name
                    }

                    ViewData["profileList"] = new HtmlString(profileOptions);

                    {   //setting positions select options
                        MongoModel lists = new MongoModel("Lists");
                        String listString = lists.Get("name", "_HTKUserPositions");
                        JObject list = JsonConvert.DeserializeObject<JArray>(listString).First as JObject;
                        String positionOptions = "";
                        foreach (JObject item in (JArray)list["elements"]["order"])
                        {
                            foreach (KeyValuePair<string, JToken> token in item)
                            {
                                positionOptions += "<option value='" + token.Key + "'>" + token.Value + "</option>";
                                break;
                            }
                        }

                        positionOptions += "<option disabled>---------------------------</option>";

                        foreach (JObject item in (JArray)list["elements"]["unorder"])
                        {
                            foreach (KeyValuePair<string, JToken> token in item)
                            {
                                positionOptions += "<option value='" + token.Key + "'>" + token.Value + "</option>";
                            }
                        }

                        ViewData["positionOptions"] = new HtmlString(positionOptions);
                    }

                    {
                        String usersString = userTable.GetRows();
                        JArray users = JsonConvert.DeserializeObject<JArray>(usersString);
                        String userOptions = "";
                        foreach (JObject user in users) {
                            userOptions += "<option value='" + user["_id"].ToString() + "'>" + user["name"].ToString() + " " + user["lastname"].ToString() + "</option>";
                        }
                        ViewData["users"] = new HtmlString(userOptions);
                    }
                }
                catch (Exception e)
                {
                    ViewData["profileList"] = null;
                }

                //Detects if the user is an Admin
                try {
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

                loadDepartments();

                return View();
            }
            else
            {

                return Redirect("~/Home");
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
                string relativepath = "\\Uploads\\Images\\User\\CustomImages\\";
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
        public String saveUser(FormCollection formData, HttpPostedFileBase file)
        {

            bool access = false;
            string limit = "true";
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool accessClient = false;
          //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("users", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("users", "u", dataPermissionsClient);

            if (access == true && accessClient==true)
            {
          
                if (this.Request.IsAjaxRequest())
                {
                    formData = CustomForm.unserialize(formData);  //use the method serialize to parse the string into an array

                    //Validate Locations
                    String userLocations = formData["userLocations"];
                    JArray locationsArray = JsonConvert.DeserializeObject<JArray>(userLocations);

                    foreach (JObject location in locationsArray)
                    {
                        JObject profileSelected = JsonConvert.DeserializeObject<JObject>(profileTable.GetRow(formData["profileSelect"].ToString()));
                        
                        if (profileSelected["name"].ToString() == "Gerente regional") {
                            JObject locationArray = JsonConvert.DeserializeObject<JObject>(locationTable.GetRow(location["id"].ToString()));
                            JObject locationProf = JsonConvert.DeserializeObject<JObject>(locationProfileTable.GetRow(locationArray["profileId"].ToString()));
                            if (locationProf["name"].ToString() == "Region")
                            {
                                string locationsValues = userTable.getUserLocation(formData["profileSelect"].ToString(), location["id"].ToString());
                                string idconjunto = "";
                                try
                                {
                                    JArray userInfoLoc = JsonConvert.DeserializeObject<JArray>(locationsValues);
                                    idconjunto = (from mov in userInfoLoc select (string)mov["_id"]).First().ToString();
                                }
                                catch { }
                                if (locationsValues != "[]" && idconjunto != formData["userID"])
                                {
                                    return "{\"msg\":\"La región ya tiene Gerente regional.\", \"status\":\"errorLocation\"}";
                                }
                                if (locationsArray.Count > 1) {
                                    return "{\"msg\":\"Solo puede le puede asignar una Región.\", \"status\":\"errorLocation\"}";
                                }
                            }
                            else {
                                return "{\"msg\":\"La ubicación seleccionada no es una región.\", \"status\":\"errorLocation\"}";
                            }
                        }
                        else if (profileSelected["name"].ToString() == "Gerente de conjunto") {
                            JObject locationArray = JsonConvert.DeserializeObject<JObject>(locationTable.GetRow(location["id"].ToString()));
                            JObject locationProf = JsonConvert.DeserializeObject<JObject>(locationProfileTable.GetRow(locationArray["profileId"].ToString()));
                            if (locationProf["name"].ToString() == "Conjunto")
                            {
                                string locationsValues = userTable.getUserLocation(formData["profileSelect"].ToString(), location["id"].ToString());
                                string idconjunto = "";
                                try
                                {
                                    JArray userInfoLoc = JsonConvert.DeserializeObject<JArray>(locationsValues);
                                    idconjunto = (from mov in userInfoLoc select (string)mov["_id"]).First().ToString();
                                }
                                catch { }
                                if (locationsValues != "[]" && idconjunto != formData["userID"])
                                {
                                    return "{\"msg\":\"El conjunto ya tiene Gerente de conjunto.\", \"status\":\"errorLocation\"}";
                                }
                                if (locationsArray.Count > 1)
                                {
                                    return "{\"msg\":\"Solo puede le puede asignar un Conjunto.\", \"status\":\"errorLocation\"}";
                                }
                            }
                            else
                            {
                                return "{\"msg\":\"La ubicación seleccionada no es un conjunto.\", \"status\":\"errorLocation\"}";
                            }
                        }
                        else if (profileSelected["name"].ToString() == "Contabilidad" || profileSelected["name"].ToString() == "Director de inmuebles" || profileSelected["name"].ToString() == "Autorizador de corporativo") { 
                            if(location["name"].ToString() != "Home"){
                                return "{\"msg\":\"Error en la ubicación, verifique los datos.\", \"status\":\"errorLocation\"}";
                            }
                        }
                    }

                    String userID = (formData["userID"] == "null") ? null : formData["userID"]; //is this an insert or an update?, converting null in javascript to null in c#
                    String userMail = formData["email"].Replace("%40", "@");
                    String userName = "";

                    dynamic user = userTable.getRow(userID);
                    /*the gived id does not exists*/
                    if (userID != null && (user == null))
                    {
                        return "{\"msg\":\"El id especificado no existe\", \"status\":\"error\"}";
                    }

                    if (userID == null)
                    {
                        limit = validateLimit();
                    }
                    if(limit=="true")
                    {
                    /*there is no profile with the gived id*/
                    if (profileTable.getRow(formData["profileSelect"]) == null)
                    {
                        return "{\"msg\":\"El perfil especificado no existe\", \"status\":\"error\"}";
                    }

                    /*The selected mail is already in use and is not the user who has it*/
                    if (userMail != "") {
                        if (emailExists(userMail) == "true" && (userID == null || userTable.get("email", userMail)[0].GetElement("_id").Value.ToString() != userID))
                        {
                            return "{\"msg\":\"El correo ya está siendo utilizado\", \"status\":\"error\"}";
                        }
                    }

                    /*The selected user name is already in use and is not the user who has it*/
                    if (userExists(formData["user"]) == "true" && (userID == null || userTable.get("user", formData["user"])[0].GetElement("_id").Value.ToString() != userID))
                    {
                        return "{\"msg\":\"El usuario ya está siendo utilizado\", \"status\":\"error\"}";
                    }

                    //due that the user's id is unique we use it has the image's name, so we store only the extension in the db
                    string ext = null;
                    if (file != null)
                    {
                        ext = file.FileName.Split('.').Last(); //getting the extension
                    }
                    else if (userID != null)
                    {
                        try
                        {
                            ext = user["imgext"].ToString();
                        }
                        catch (Exception e) { }
                    }

                    /* Format validations */
                    if (!Regex.IsMatch(formData["user"], "([a-zA-Z0-9-_.]){4,}"))
                    {
                        return "{\"msg\":\"Formato incorrecto para: user\", \"status\":\"error\"}";
                    }
                    else if (!Regex.IsMatch(formData["pwd"], ".{6,}"))
                    {
                        if (userID != null && formData["pwd"] == "") //if it is an update and there is not password specified, the password must be the same
                        {
                            formData["pwd"] = userTable.getRow(userID).GetElement("pwd").Value.ToString();
                        }
                        else
                            return "{\"msg\":\"Formato incorrecto para: password\", \"status\":\"error\"}";
                    }
                    else if (!Regex.IsMatch(userMail, "[A-Za-z0-9-_.]+@[A-Za-z0-9-.]+\\.[a-zA-Z]+") && userMail !="")
                    {
                        return "{\"msg\":\"Formato incorrecto para: email\", \"status\":\"error\"}"; 
                    }
                    else if (!Regex.IsMatch(formData["name"], "[A-ZÁÉÍÓÚÑa-záéíóúñ]+( [A-ZÁÉÍÓÚÑa-záéíóúñ]+){0,2}"))
                    {
                        return "{\"msg\":\"Formato incorrecto para: name\", \"status\":\"error\"}";
                    }
                    else if (!Regex.IsMatch(formData["lastname"], "[A-ZÁÉÍÓÚÑa-záéíóúñ]+( [A-ZÁÉÍÓÚÑa-záéíóúñ]+){0,1}"))
                    {
                        return "{\"msg\":\"Formato incorrecto para: lastname\", \"status\":\"error\"}";
                    }
                    else
                    {
                        formData["pwd"] = HashPassword.CreateHash(formData["pwd"].ToString());
                    }

                    //Change name representation
                    formData["name"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(formData["name"].ToString().ToLower().Trim());
                    formData["lastname"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(formData["lastname"].ToString().ToLower().Trim());

                    //there are fields that we know that exists so we set them into the json
                    String jsonData = "{'user':'" + formData["user"] + "','pwd':'" + formData["pwd"] + "','imgext':'" + ext
                        + "','email':'" + userMail + "','name':'" + formData["name"].Replace("+", " ")
                        + "','lastname':'" + formData["lastname"].Replace("+", " ") + "', 'profileId':'" + formData["profileSelect"]
                        + "', 'boss':'" + formData["boss"] + "','userKey':'" + formData["userKey"].ToString() + "', 'userLocations':" + formData["userLocations"]
                        + ",'areaSelect':'" + formData["areaSelect"] + "','departmentSelect':'" + formData["departmentSelect"] + "','descripcionpuesto':'" + formData["descripcionpuesto"] + "','alias':'" + formData["alias"] + "','permissionsHTK':" + formData["permissionsHTK"];

                    try //trying to set the creator's id
                    {
                        jsonData += ", 'creatorId':'";
                        if (userID == null)
                        {
                            jsonData += this.Session["_id"];
                        }
                        else
                        {
                            try
                            {
                                jsonData += user["creatorId"];
                            }
                            catch (Exception e) { /*Ignored*/}
                        }
                        jsonData += "'";
                    }
                    catch (Exception e) { /*Ignored*/ }

                    String profileId = formData["profileSelect"];

                    userName = formData["user"];
                    //remove the setted data in the json from the formData
                    formData.Remove("user");
                    formData.Remove("pwd");
                    formData.Remove("profileSelect");
                    formData.Remove("userID");
                    formData.Remove("name");
                    formData.Remove("lastname");
                    formData.Remove("email");
                    formData.Remove("data");
                    formData.Remove("file");
                    formData.Remove("position");
                    formData.Remove("boss");
                    formData.Remove("userKey");
                    formData.Remove("userLocations");
                    formData.Remove("areaSelect");
                    formData.Remove("departmentSelect");
                    formData.Remove("Descripcionpuesto");
                    formData.Remove("alias");
                    formData.Remove("permissionsHTK");
                    formData.Remove("checkTitle");
                    formData.Remove("c");
                    formData.Remove("u");
                    formData.Remove("d");
                    formData.Remove("r");
                    formData.Remove("a");


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
                    string id = userTable.saveRow(jsonData, userID);

                    //Notify this action
                    if (userID == null){
                        Notificate.saveNotification("Users", "Create", "El usuario '" + userName + "' ha sido creado");
                        _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Insert: " + userName, "Users", DateTime.Now.ToString());
                    }
                    else{
                        Notificate.saveNotification("Users", "Update", "El usuario '" + userName + "' ha sido modificado");
                        _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Update: " + userName, "Users", DateTime.Now.ToString());
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
                    //userTable.saveImage(file.InputStream, id);

                    return "{\"msg\":\"" + id + "\", \"status\":\"success\"}"; //returns the saved user's id
                }
                }
                return null;
            }
            else { return null; }
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
                    //ViewData["idcategory"] = idcategory;
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



                    /*string rowsArray = categoryTable.getRow(idcategory);
                    //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                    JObject categoryJarray = JsonConvert.DeserializeObject<JObject>(rowsArray);*/
                    List<List<string>> data = new List<List<string>>();
                   /* string idfield = "";
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
                    }*/
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

        public string searchiduserbyname(string fullname)
        {
            string id = "";
            try
            {
                fullname = fullname.TrimStart().TrimEnd();
                string name = fullname.Split(' ').First();
                string lastname = fullname.Split(' ').Last();
                Dictionary<string, string> fields = new Dictionary<string, string>();
                fields.Add("name", name);
                fields.Add("lastname", lastname);
                try
                {
                    JObject namejo = JsonConvert.DeserializeObject<JArray>(userTable.GetMultiple(fields)).First as JObject;
                    id = namejo["_id"].ToString();
                }
                catch { }
                return id;
            }
            catch
            {
                return id;
            }
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

                DataFileManager Filelimits;
                string filepath = "/App_Data/system.conf";
                string absolutedpath = Server.MapPath(filepath);
                Filelimits = new DataFileManager(absolutedpath, "juanin");
                String clientid = "";

                if (!Filelimits.empty())
                {
                    clientid = Filelimits["clientInfo"]["userId"];
                }

                String dataimport = data.ToString();
                JArray dataimportarray = JsonConvert.DeserializeObject<JArray>(dataimport);
                int count = 0; int totalAdd = 0; int totalFail = 0; int totalAddIMG = 0; int totalFailIMG = 0; int ubicationFail = 0;
                JArray result = new JArray();
                JObject bossNotFound = new JObject();
                foreach (JObject items in dataimportarray)
                {
                    count++; bool error = false; string getboss = "";
                    String userMail = items["email"].ToString().Replace("%40", "@");

                    string limit = "true";
                    limit = validateLimit();

                    if (limit == "true")
                    {
                        /*The selected mail is already in use and is not the user who has it*/
                        if (userMail != "")
                        {
                            if (emailExists(userMail) == "true")
                            {
                                result.Add("{\"error\":\"El correo ya está siendo utilizado\", \"registro\":\"" + count + "\"}");
                                error = true;
                            }
                        }

                        /*The selected user name is already in use and is not the user who has it*/
                        if (userExists(items["user"].ToString()) == "true")
                        {
                            result.Add("{\"error\":\"El ID del usuario ya está siendo utilizado\", \"registro\":\"" + count + "\"}");
                            error = true;
                        }
                    }

                    /* Format validations */
                    if (!Regex.IsMatch(items["user"].ToString(), "([a-zA-Z0-9-_.]){4,}") || items["user"].ToString() == "")
                    {
                        result.Add("{\"error\":\"Formato incorrecto para ID del Usuario\", \"registro\":\"" + count + "\"}");
                        error = true;
                    }
                    else if (!Regex.IsMatch(items["pwd"].ToString(), ".{6,}"))
                    {
                        result.Add("\"error\":\"Formato incorrecto para contraseña\", \"registro\":\"" + count + "\"}");
                        error = true;
                    }
                    else if (!Regex.IsMatch(userMail, "[A-Za-z0-9-_.]+@[A-Za-z0-9-.]+\\.[a-zA-Z]+") && userMail != "")
                    {
                        result.Add("\"error\":\"Formato incorrecto para el correo\",\"registro\":\"" + count + "\"}");
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
                    else
                    {
                        items["pwd"] = HashPassword.CreateHash(items["pwd"].ToString());
                    }

                    if (error) { totalFail++; continue; }


                    //Section to get profile ID
                    string profileID = "";

                    JArray profileOtro = JsonConvert.DeserializeObject<JArray>(profileTable.Get("name", "Null"));
                    JObject profileOt = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(profileOtro[0]));
                    profileID = profileOt["_id"].ToString();

                    if (items["perfil"] != null && items["perfil"].ToString() != "")
                    {
                        string profileResult = profileTable.Get("name", items["perfil"].ToString());
                        if (profileResult != "[]" && profileResult != "" && profileResult != null)
                        {
                            JArray conjuntja = JsonConvert.DeserializeObject<JArray>(profileResult);
                            profileID = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                        }
                    }

                    //Try get boss information
                    if (items["boss"] != null && items["boss"].ToString() != "")
                    {
                        string bossFound = userTable.Get("user", items["boss"].ToString());
                        if (bossFound != "[]")
                        {
                            JArray userBoss = JsonConvert.DeserializeObject<JArray>(bossFound);
                            string idBoss = (from mov in userBoss select (string)mov["_id"]).First().ToString();
                            items["boss"] = idBoss;
                        }
                        else
                        {
                            string iduser = searchiduserbyname(items["boss"].ToString());
                            if (iduser != "")
                            {
                                items["boss"] = iduser;
                            }
                            else
                            {
                                getboss = items["boss"].ToString();
                                items["boss"] = "";
                            }
                        }
                    }
                    //try get deparment id
                    try
                    {
                        if (items["departmentSelect"] != null && items["departmentSelect"].ToString() != "")
                        {
                            string iddep = searchDepartment(items["departmentSelect"].ToString());
                            if (iddep != "")
                            {
                                items["departmentSelect"] = iddep; 
                            }
                            else
                            {
                                items["departmentSelect"] ="";
                            }
                        }
                    }
                    catch {  }
                    //Get locations information
                    JArray locationArray = new JArray();
                    /*if (items["location"] != null && items["location"].ToString() != "")
                    {
                        string[] sections = items["location"].ToString().Split('%');
                        if (sections.Length < 2) //If it has more than one region
                        {
                            string[] regionId = items["location"].ToString().Split('*');

                            if (regionId[0].Trim() == "0")
                            {
                                JObject locationInfo = new JObject();
                                locationInfo.Add("id", "");
                                locationInfo.Add("name", "Home");
                                locationArray.Add(locationInfo);
                            }
                            else
                            {
                                if (items["perfil"].ToString() == "Gerente regional" && (int.Parse(regionId[0]) > 0 && int.Parse(regionId[0]) < 100))
                                {
                                    try
                                    {
                                        JArray conjunto = JsonConvert.DeserializeObject<JArray>(locationTable.Get("number", regionId[0].Trim()));
                                        
                                        string idLocation = (from mov in conjunto select (string)mov["_id"]).First().ToString();
                                        string nameLocation = (from mov in conjunto select (string)mov["name"]).First().ToString();
                                        string profileLocation = (from mov in conjunto select (string)mov["profileId"]).First().ToString();

                                        JObject locationProf = JsonConvert.DeserializeObject<JObject>(locationProfileTable.GetRow(profileLocation));
                                        if (locationProf["name"].ToString() != "Region") { ubicationFail++; continue; }

                                        string locationsValues = userTable.getUserLocation(profileLocation, idLocation);
                                        if (locationsValues != "[]") { ubicationFail++; continue; }
                                        if (locationArray.Count > 1) { ubicationFail++; continue; }

                                        JObject locationInfo = new JObject();
                                        locationInfo.Add("id", idLocation);
                                        locationInfo.Add("name", nameLocation);
                                        locationArray.Add(locationInfo);
                                    }
                                    catch { }
                                }
                                else
                                {
                                    string[] locations ={};
                                    try
                                    {
                                        locations = regionId[1].Split('|');
                                    }
                                    catch(Exception ex) {
                                        JObject locationInfo = new JObject();
                                        locationInfo.Add("id", "");
                                        locationInfo.Add("name", "Home");
                                        locationArray.Add(locationInfo);
                                    }
                                 if(locations!=null){
                                    foreach (string loc in locations)
                                    {
                                        try
                                        {
                                            JArray conjunto = JsonConvert.DeserializeObject<JArray>(locationTable.Get("number", loc.Trim()));

                                            string idLocation = (from mov in conjunto select (string)mov["_id"]).First().ToString();
                                            string nameLocation = (from mov in conjunto select (string)mov["name"]).First().ToString();
                                            string profileLocation = (from mov in conjunto select (string)mov["profileId"]).First().ToString();

                                            if (items["perfil"].ToString() == "Gerente de conjunto")
                                            {
                                                JObject locationProf = JsonConvert.DeserializeObject<JObject>(locationProfileTable.GetRow(profileLocation));
                                                if (locationProf["name"].ToString() != "Conjunto") { ubicationFail++; continue; }

                                                string locationsValues = userTable.getUserLocation(profileLocation, idLocation);
                                                if (locationsValues != "[]") { ubicationFail++; continue; }
                                                if (locationArray.Count > 1) { ubicationFail++; continue; }
                                            }

                                            JObject locationInfo = new JObject();
                                            locationInfo.Add("id", idLocation);
                                            locationInfo.Add("name", nameLocation);
                                            locationArray.Add(locationInfo);
                                        }
                                        catch { }
                                    }
                                }
                                }
                            }
                        }
                        else
                        {
                            foreach (string section in sections)
                            {
                                string[] sec = section.Split('*');
                                if (sec[0].Trim() == "0")
                                {
                                    JObject locationInfo = new JObject();
                                    locationInfo.Add("id", "");
                                    locationInfo.Add("name", "Home");
                                    locationArray.Add(locationInfo);
                                    break;
                                }
                            }
                        }
                    }*/
                    if (items["location"] != null && items["location"].ToString() != "")
                    {
                        List<string> locationslist = items["location"].ToString().Split('|').ToList();
                        foreach (string loc in locationslist)
                        {
                            try
                            {
                                JObject conjunto = JsonConvert.DeserializeObject<JArray>(locationTable.Get("name", loc.TrimStart().TrimEnd())).First as JObject;
                                JObject locationInfo = new JObject();
                                locationInfo.Add("id", conjunto["_id"].ToString());
                                locationInfo.Add("name", conjunto["name"].ToString());
                                locationArray.Add(locationInfo);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        JObject locationInfo = new JObject();
                        locationInfo.Add("id", "");
                        locationInfo.Add("name", "Home");
                        locationArray.Add(locationInfo);
                    }
                    //Change name representation
                    items["name"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(items["name"].ToString().ToLower().Trim());
                    items["lastname"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(items["lastname"].ToString().ToLower().Trim());

                    //there are fields that we know that exists so we set them into the json
                    String jsonData = "{'user':'" + items["user"].ToString() + "','pwd':'" + items["pwd"].ToString() + "','imgext':'"
                       + "','email':'" + userMail + "','name':'" + items["name"].ToString().Replace("+", " ")
                       + "','lastname':'" + items["lastname"].ToString().Replace("+", " ") + "', 'profileId':'" + profileID
                       + "', 'boss':'" + items["boss"].ToString() + "','userKey':'', 'userLocations':" + JsonConvert.SerializeObject(locationArray)
                       + ",'areaSelect':'" + items["areaSelect"].ToString() + "','departmentSelect':'" + items["departmentSelect"].ToString() + "','descripcionpuesto':'" + items["descripcionpuesto"].ToString() + "','alias':'" + items["alias"].ToString() + "'";

                    try //trying to set the creator's id
                    {
                        jsonData += ", 'creatorId':'";
                        jsonData += this.Session["_id"];
                        jsonData += "'}";
                    }
                    catch (Exception e) { /*Ignored*/ }

                    string id = userTable.saveRow(jsonData, null); //Save new user
                    _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Insert: " + items["user"].ToString(), "Users", DateTime.Now.ToString());
                    //Save boss not found
                    if (getboss != "")
                    {
                        if (bossNotFound[getboss] != null)
                        {
                            JArray ides = JsonConvert.DeserializeObject<JArray>(bossNotFound[getboss].ToString());
                            ides.Add(id);
                            bossNotFound[getboss] = JsonConvert.SerializeObject(ides);
                        }
                        else { bossNotFound.Add(getboss, "[\"" + id + "\"]"); }
                    }
                    //Load antiguo boss

                    if (bossNotFound[items["user"].ToString()] != null)
                    {
                        JArray idesAll = JsonConvert.DeserializeObject<JArray>(bossNotFound[items["user"].ToString()].ToString());

                        foreach (JValue ides in idesAll) {
                            if (id != ides.ToString()) //Check if it not the same user
                            {
                                JObject oldUser = JsonConvert.DeserializeObject<JObject>(userTable.GetRow(ides.ToString()));
                                oldUser["boss"] = id;
                                userTable.SaveRow(JsonConvert.SerializeObject(oldUser), oldUser["_id"].ToString());
                                _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Insert: " + oldUser["user"].ToString(), "Users", DateTime.Now.ToString());
                            }
                        }
                    }

                    result.Add("{\"success\":\"Usuario guardado en registo: " + count + "\"}");
                    totalAdd++;

                }

                //Save imagen files
                string ext = null; 
                string relativepath = @"\Uploads\Images\TempUser";
                string absolutepath = Server.MapPath(relativepath);
                //Delete all files
                if (System.IO.Directory.Exists(absolutepath + "\\"))
                {
                    System.IO.Directory.Delete(absolutepath + "\\", true);
                }
                //Create de upload directory
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

                            var hlp = 0;

                            string[] filesArray = System.IO.Directory.GetFiles(absolutepath + "\\" + fileName.Split('.').First() + "\\");

                            foreach (string s in filesArray)
                            {
                                string namefileS = Path.GetFileName(s);
                                JArray usersInfo = JsonConvert.DeserializeObject<JArray>(userTable.Get("user", namefileS.Split('.').First()));
                                JObject userInfo = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(usersInfo[0]));
                                userInfo["imgext"] = s.Split('.').Last();
                                userTable.SaveRow(JsonConvert.SerializeObject(userInfo), userInfo["_id"].ToString());
                                _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Insert: " + userInfo["user"].ToString(), "Users", DateTime.Now.ToString());
                                System.IO.File.Move(s,  Server.MapPath(@"\Uploads\Images\" + userInfo["_id"].ToString() + "." + s.Split('.').Last()));

                                totalAddIMG++;
                            }

                        }
                        catch (Exception e) {
                            totalFailIMG++;
                            result.Add("{\"error\":\"Error al gaurdar imagen\"}");
                        }
                    }
                }

                //Get data for new users
                String usersString = userTable.GetRows();
                JArray users = JsonConvert.DeserializeObject<JArray>(usersString);
                String userOptions = "";
                foreach (JObject user in users)
                {
                    userOptions += "<option value='" + user["_id"].ToString() + "'>" + user["name"].ToString() + " " + user["lastname"].ToString() + "</option>";
                }

                JObject finalResult = new JObject();
                finalResult.Add("userSuccess", totalAdd.ToString());
                finalResult.Add("userError", totalFail.ToString());
                finalResult.Add("imgSuccess", totalAddIMG.ToString());
                finalResult.Add("imgError", totalFailIMG.ToString());
                finalResult.Add("locationError", ubicationFail.ToString());
                finalResult.Add("details", JsonConvert.SerializeObject(result));
                finalResult.Add("newUsers", userOptions);

                return JsonConvert.SerializeObject(finalResult);
            }
            catch (Exception ex)
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
        public JsonResult getUserTable()
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                string userid = Session["_id"].ToString();
                List<BsonDocument> docs = userTable.getRows(); //getting all the users
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
                    {
                        document.Remove("profileFields");
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
                        BsonDocument perfil = profileTable.getRow(document.GetElement("profileId").Value.ToString());
                        //document.Set("Creator", "YO");//
                        document.Set("profilename", perfil.GetElement("name").Value);
                    }
                    catch (Exception e) { document["profilename"] = ""; }
                    /*try
                    {
                        document.Set("ubicacion", document.GetElement("userLocations").Value);
                    }
                    catch (Exception e) { document["ubicacion"] = ""; }*/
                    try
                    {
                        JArray rowString = JsonConvert.DeserializeObject<JArray>(listTable.Get("name", "departments"));

                        JArray listInfo = new JArray();
                        foreach (JObject obj in rowString)
                        {
                            listInfo = JsonConvert.DeserializeObject<JArray>(obj["elements"]["unorder"].ToString());
                        }

                        MongoDB.Bson.BsonValue valor = "";
                        foreach (JObject depa in listInfo)
                        {
                            foreach (KeyValuePair<string, JToken> token in depa)
                            {
                                if (token.Key == document.GetElement("departmentSelect").Value.ToString()) { 
                                    valor = token.Value.ToString();
                                }
                            }
                            if (valor.ToString() != "") break;
                        }
                        document.Set("departmentSelect", valor);
                    }
                    catch (Exception e) { document["departmentSelect"] = ""; }
                    try
                    {
                        BsonDocument boss = userTable.getRow(document.GetElement("boss").Value.ToString());
                        //document.Set("Creator", "YO");//
                        document.Set("boss", boss.GetElement("name").Value);
                    }
                    catch (Exception e) { document["boss"] = ""; }
                    try
                    {
                        document.Remove("creatorId");
                    }
                    catch (Exception e) { }
                    try
                    {
                        document.Remove("pwd");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    if (document["user"].ToString() == "admin")
                    {
                        if (document["_id"].ToString() == userid)
                        {
                            String newDocString = document.ToString();
                            JObject newDoc = JsonConvert.DeserializeObject<JObject>(newDocString);
                            docum.Add(newDoc);
                        }
                    }
                    else
                    {
                        String newDocString = document.ToString();
                        JObject newDoc = JsonConvert.DeserializeObject<JObject>(newDocString);
                        docum.Add(newDoc);
                        //docum.Add(document);
                    }
                }
                JObject result = new JObject();
                result.Add("users", docum);
                String resultString = JsonConvert.SerializeObject(result);
                return Json(resultString);
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
        public String deleteUser(String selectedID)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("users", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("users", "d", dataPermissionsClient);

            string usrstring = userTable.GetRow(selectedID);
            JObject usr = JsonConvert.DeserializeObject<JObject>(usrstring);

            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest()) //only available with AJAX
                {
                    try
                    {
                        userTable.deleteRow(selectedID); //deletes the selected document
                        //Notificate the action
                        Notificate.saveNotification("Users", "Delete", "Un usuario ha sido borrado");
                        _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Delete: " + usr["user"].ToString(), "Users", DateTime.Now.ToString());
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
        public String deleteUsers(List<String> array)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("users", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("users", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest()) //only available with AJAX
                {
                    try //tryign to delete the users
                    {
                        if (array.Count == 0) return null; //if array is empty there are no users to delete
                        foreach (String id in array) //froeach id in the array we must delete the document with that id from the db
                        {
                            string usrstring = userTable.GetRow(id);
                            JObject usr = JsonConvert.DeserializeObject<JObject>(usrstring);
                            userTable.deleteRow(id);
                            _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Delete: " + usr["user"].ToString(), "Users", DateTime.Now.ToString());
                        }
                        //Notificate the action
                        Notificate.saveNotification("Users", "Delete", array.Count + " usuarios han sido borrados");
                        
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
        public String userExists(String userName)
        {
            try
            {
                List<BsonDocument> list = userTable.get("user", userName);
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
        ///     This method checks if a gived email is already been in use for another user
        /// </summary>
        /// <param name="email">
        ///     It's a string that represents the email of the user
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a boolean, does this email already in use?
        /// </returns>
        public String emailExists(String email)
        {
            try
            {
                List<BsonDocument> list = userTable.get("email", email.Replace("%40", "@"));
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
        public String getUser(String userID)
        {
            if (this.Request.IsAjaxRequest())
            {
                try
                {
                    BsonDocument doc = userTable.getRow(userID); //getting the user's data

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

        /// <summary>
        ///      Gets a list of the Customers related to a User
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        public string GetCustomerRelation(string idUser)
        {
            var user = userTable.getRow(idUser);
            try
            {
                var CustomersArray = user.GetElement("customers").Value.ToJson();
                var customers = customerTable.GetCustomerRelated(CustomersArray);

                return customers;
            }
            catch (Exception e)
            {
                //Returns a Javascript void plain object
                return "{}";
            }


        }

        /// <summary>
        ///     Saves the relation between Users and Customers
        /// </summary>
        /// <param name="customerList">
        ///     An array of JSONs with the customers. e.g. [{id:"",name:""},{...}]
        /// </param>
        /// <param name="id_user">
        ///     The user's id that will contain the relation
        /// </param>
        public void SaveCustomerRelation(string customerList, string id_user)
        {
            if (this.Request.IsAjaxRequest())
            {
                // var customerArray = JsonConvert.DeserializeObject<JArray>(customerList);
                try
                {
                    userTable.saveRelation(customerList, id_user);
                }
                catch (Exception e) {/**/}

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id_user"></param>
        /// <param name="id_customer"></param>
        public void DeleteCustomerRelation(string id_user, string id_customer)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("users", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("users", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest())
                {
                    try
                    {
                        userTable.removeRelation(id_user, id_customer);
                    }
                    catch (Exception e) { this.Response.Status = "500"; }


                }
            }
        }

        public JsonResult globalSearch(String data)
        {

            List<BsonDocument> userResult = new List<BsonDocument>();
            List<BsonDocument> docs = userTable.getRows(); //getting all the users

            foreach (BsonDocument document in docs)
            {

                if (document["user"].ToString().ToLower().Contains(data.ToLower()) ||
                    document["name"].ToString().ToLower().Contains(data.ToLower()) ||
                    document["lastname"].ToString().ToLower().Contains(data.ToLower()))
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
                    try
                    {
                        document.Remove("pwd");
                    }
                    catch (Exception e) { /*Ignored*/ }

                    userResult.Add(document);
                }
            }


            return Json(userResult.ToJson());

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

        public String setUserKey(String userID) {
            String userString = userTable.GetRow(userID);
            JObject user = JsonConvert.DeserializeObject<JObject>(userString);
            String name = user["lastname"].ToString().Trim() + " " + user["name"].ToString().Trim();
            String key = generateUserKey(name, user["user"].ToString());

            foreach (char character in name) { 
                key += (int)character;
            }

            key += "|";
            foreach (char character in user["user"].ToString().Trim()) {
                key += (int)character;
            }

            user["e-signature"] = key;
            String id = user["_id"].ToString();
            user.Remove("_id");
            userString = JsonConvert.SerializeObject(user);
            userTable.SaveRow(userString, id);
            _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Update: " + user["user"].ToString(), "Users", DateTime.Now.ToString());
            return key;
        }

        public String generateUserKey(String userName, String userAccount) {
            String key = "";
            foreach (char character in userName.Trim())
            {
                key += (int)character;
            }

            key += "|";
            foreach (char character in userAccount.Trim())
            {
                key += (int)character;
            }
            return key;
        }

        public string FirmasStatus() {
            JArray userListNoFirm = JsonConvert.DeserializeObject<JArray>(userTable.Get("userKey", ""));
            JArray userList = JsonConvert.DeserializeObject<JArray>(userTable.GetRows());
            int total = userList.Count - userListNoFirm.Count;

            string result = "{\"user\":\"" + userList.Count.ToString() + "\",\"total\":\"" + total.ToString() + "\",\"miss\":\"" + userListNoFirm.Count.ToString() + "\"}";
            return result;
        }

        public void GenerateFirmasMasive() {
            JArray userListNoFirm = JsonConvert.DeserializeObject<JArray>(userTable.Get("userKey", ""));
            foreach (JObject user in userListNoFirm) {
                string key = generateUserKey(user["lastname"].ToString() + " " +user["name"].ToString(),user["user"].ToString());
                user["userKey"] = key;
                userTable.SaveRow(JsonConvert.SerializeObject(user), user["_id"].ToString());
                _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Update: " + user["user"].ToString(), "Users", DateTime.Now.ToString());
            }
        }

        public string ChgPasswordDays(string cantDays = null) {
            if (cantDays == null)
            {
                try
                {
                    JArray days = JsonConvert.DeserializeObject<JArray>(systemSettingsTable.Get("name", "daysChangePassword"));
                    JObject daysObj = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(days[0]));
                    return "{\"status\":\"success\", \"cant\":\"" + daysObj["days"] + "\"}";
                }
                catch (Exception ex)
                {
                    return "{\"status\":\"error\", \"cant\":\"\"}";
                }
            }
            else {
                try
                {
                    JArray days = JsonConvert.DeserializeObject<JArray>(systemSettingsTable.Get("name", "daysChangePassword"));
                    JObject daysObj = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(days[0]));
                    daysObj["days"] = cantDays;
                    systemSettingsTable.SaveRow(JsonConvert.SerializeObject(daysObj), daysObj["_id"].ToString());
                    return "{\"status\":\"save\"}";
                }
                catch (Exception ex) {
                    systemSettingsTable.SaveRow("{'name':'daysChangePassword','days':'" + cantDays + "'}");
                    return "{\"status\":\"save\"}";
                }
            }
        }

        public string getMyAccount()
        {
            JObject userInfo = JsonConvert.DeserializeObject<JObject>(userTable.GetRow(Session["_id"].ToString()));
            JObject result = new JObject();

            result.Add("name",userInfo["name"].ToString());
            result.Add("lastname", userInfo["lastname"].ToString());
            result.Add("email", userInfo["email"].ToString());
            result.Add("user", userInfo["user"].ToString());

            try
            {
                JObject boss = JsonConvert.DeserializeObject<JObject>(userTable.GetRow(userInfo["boss"].ToString()));
                result.Add("boss", boss["name"].ToString() + " "+ boss["lastname"].ToString());
            }
            catch (Exception ex)
            {
                result.Add("boss", "");
            }
            try {
                JObject puesto = JsonConvert.DeserializeObject<JObject>(profileTable.GetRow(userInfo["profileId"].ToString()));
                result.Add("puesto", puesto["name"].ToString());
            }
            catch (Exception ex) {
                result.Add("puesto", "");
            }

            if (userInfo["imgext"] != null && userInfo["imgext"].ToString() != "")
            {
                result.Add("imagen", "http://test.cafiweb.com/Uploads/Images/" + userInfo["_id"].ToString() + "." + userInfo["imgext"]);
            }
            else {
                result.Add("imagen", "null");
            }

            result.Add("key", userInfo["userKey"].ToString());

            return JsonConvert.SerializeObject(result);
        }

        public string updateMyAccount(string email, string password) {

            try
            {
                JObject userInfo = JsonConvert.DeserializeObject<JObject>(userTable.GetRow(Session["_id"].ToString()));

                if (password != null && password != "")
                {
                    userInfo["pwd"] = HashPassword.CreateHash(password);
                    userInfo["lastChgPassword"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                }

                userInfo["email"] = email;

                userTable.saveRow(JsonConvert.SerializeObject(userInfo), userInfo["_id"].ToString());
                _logTable.SaveLog(Session["_id"].ToString(), "Usuarios", "Update: " + userInfo["user"].ToString(), "Users", DateTime.Now.ToString());
                return "success";
            }
            catch (Exception ex) {
                return "error";
            }
        }
        public void loadDepartments()
        {
            try
            {
                String DepartmentsOptions = "";
                String rowArray = listTable.Get("name", "departments");
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray listas = new JArray();
                foreach (JObject obj in rowString)
                {
                    listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["unorder"].ToString());
                }
                DepartmentsOptions += "<option value='null' selected>Seleccione...</option>";
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
        public string searchDepartment(string namedepartment)
        {
            String iddep = "";
            try
            {
              
                String rowArray = listTable.Get("name", "departments");
                JObject rowString = JsonConvert.DeserializeObject<JArray>(listTable.Get("name", "departments")).First as JObject;
                JArray listas = new JArray();
                try
                {
                    listas = JsonConvert.DeserializeObject<JArray>(rowString["elements"]["unorder"].ToString());
                }
                catch { }
                 foreach (JObject puesto in listas)
                {
                    foreach (KeyValuePair<string, JToken> token in puesto)
                    {
                        if (namedepartment.ToLower().Trim() == token.Value.ToString().ToLower().Trim())
                        {
                            iddep = token.Key.ToString();
                            break;
                        }
                     }

                }

                 return iddep;
            }
            catch (Exception e)
            {
                return iddep;
            }
        }

        public string GetPermissionsUser(string profileID, string userID)
        {
            string result = "";
            if (userID == null || userID == "")
            {
                JObject profile = JsonConvert.DeserializeObject<JObject>(profileTable.GetRow(profileID));
                result = JsonConvert.SerializeObject(profile["permissionsHTK"]);

                return result;
            } else{
                JObject userInfo = JsonConvert.DeserializeObject<JObject>(userTable.GetRow(userID));
                if (userInfo["permissionsHTK"] != null) {
                    result = JsonConvert.SerializeObject(userInfo["permissionsHTK"]);
                } else {
                    JObject profile = JsonConvert.DeserializeObject<JObject>(profileTable.GetRow(profileID));
                    result = JsonConvert.SerializeObject(profile["permissionsHTK"]);
                }
                return result;
            }
        }
    }
}
