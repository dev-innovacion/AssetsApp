using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Text.RegularExpressions;

using RivkaAreas.List.Models;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rivka.Form;
using Rivka.Form.Field;
using Rivka.Api;
using Rivka.Security;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using System.Data;
namespace RivkaAreas.List.Controllers
{

    [Authorize]
    public class ListController : Controller
    {

        private ListTable listTable;
        private UserTable userTable;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        private Agent agent;
        protected validatePermissions validatepermissions;
        public ListController()
        {
            listTable = new ListTable();
            userTable = new UserTable();
            this._logTable = new LogBook.Controllers.LogBookController();
            agent = null;
            validatepermissions = new validatePermissions();
        }

        /// <summary>
        ///     This method initializes the agent
        /// </summary>
        /// <author>Quijada Romero Luis Gonzalo</author>
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
        ///     List's index view
        /// </summary>
        /// <author>Quijada Romero Luis Gonzalo</author>
        /// <returns>The list's view</returns>
        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("lists", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("lists", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
       
                 return View();
             }
             else
             {
                 return Redirect("~/Home");
             }
        }

        /// <summary>
        ///     This method allows to get the list's documents without the elements to make them shorter
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a JSON with the list's information
        /// </returns>
        public JsonResult getListTable()
        {
            if (this.Request.IsAjaxRequest())
            {
                try
                {
                    List<BsonDocument> resultList = listTable.getRows();
                    foreach (BsonDocument document in resultList)
                    {
                        try //trying to remove the elements field
                        {
                            document.Remove("elements");
                        }
                        catch (Exception e) { /*Ignored*/ }

                        try //trying to set the user creator's name
                        {
                            BsonDocument user = userTable.getRow(document.GetElement("creatorId").Value.ToString());
                            document.Set("creatorId", user.GetElement("user").Value);
                        }
                        catch (Exception e)
                        { //if it can't be created remove it, we can't show the id
                            document.Remove("creatorId");
                        }
                    }
                    return Json(resultList.ToJson());
                }
                catch (Exception e)
                {
                    return null; //if an error occours returns no document
                }
            }
            return null;
        }

        /// <summary>
        ///     This method allows to get the list documents with the specified id
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a JSON with the list information
        /// </returns>
        public JsonResult getFullList(String id) {
            try
            {
                BsonDocument document = listTable.getRow(id);
                return Json(document.ToJson());
            }
            catch (Exception e) { /*Ignored*/ }
            return null;
        }

        /// <summary>
        ///     This method saves a new list in the bd if not id is gived, or update an existing list if is gived
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="id"></param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the saved document's id 
        /// </returns>
        public String saveList(FormCollection formData)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("lists", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("lists", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest())
                {
                    try //trying to unserialize the formcollection gived
                    {
                        formData = CustomForm.unserialize(formData);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }

                    String listID = (formData["listID"] == "null") ? null : formData["listID"]; //is this an insert or an update?, converting null in javascript to null in c#
                    if (listID != null)
                    {
                        char[] delimiters = { ',', '[', ']' };
                        string[] changedElements = formData["changedElements"].Split(delimiters);
                        foreach (string element in changedElements)
                        {
                            removeElement(formData["listID"], element);
                        }
                    }

                    BsonDocument list = listTable.getRow(listID);

                    //validating listName
                    if (!Regex.IsMatch(formData["listName"], "[A-Za-z_]+[A-Za-z0-9.-_]*"))
                    {
                        return "El nombre de la lista tiene un formato incorrecto";
                    }
                    //validating listDisplayName
                    else if (!Regex.IsMatch(formData["listDisplayName"], ""))
                    {
                        return "El nombre para mostrar de la lista tiene un formato incorrecto";
                    }
                    else if (listNameUsed(formData["listName"]) && (list == null || formData["listName"] != list.GetElement("name").Value.ToString()))
                    {
                        return "El nombre de la lista ya está siendo utilizado";
                    }
                    
                    JObject jsonData = new JObject();
                    try
                    {
                        jsonData.Add("name",formData["listName"]);
                        jsonData.Add("displayName",formData["listDisplayName"]);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }

                    try //trying to set the creator's id
                    {
                        if (listID == null)
                        {
                            jsonData.Add("creatorId", this.Session["_id"].ToString());
                        }
                        else
                        {
                            try
                            {
                                jsonData.Add("creatorId", list["creatorId"].ToString());
                            }
                            catch (Exception e) { /*Ignored*/}
                        }
                    }
                    catch (Exception e) { /*Ignored*/ }

                    /*setting dependency*/
                    try
                    {
                        String dependency = formData["dependency"]; //checking dependency of this list
                        BsonDocument dependencyDocument = BsonDocument.Parse(dependency);
                        if (listHasElement(dependencyDocument.GetElement("listID").Value.ToString(), dependencyDocument.GetElement("elementName").Value.ToString()))
                        {
                            jsonData.Add("dependency",formData["dependency"]);
                        }
                    }
                    catch (Exception e) { /*Ignored*/ }

                    /***  Creating elements  ***/
                    JObject listElements = new JObject();
                    JArray unorderElements = new JArray();
                    try //trying to create the unorder elements
                    {
                        BsonDocument uoElements = BsonDocument.Parse(formData["unorderElements"]);
                        foreach (BsonElement element in uoElements)
                        {
                            if (Regex.IsMatch(element.Name.ToString(), "[A-Za-z0-9.-_]+") && Regex.IsMatch(element.Value.ToString(), "[A-Za-z0-9._-áéíóúÁÉÍÓÚñÑ]*"))
                            {
                                JObject newElement = new JObject();
                                newElement.Add(element.Name.ToString(), element.Value.ToString());
                                unorderElements.Add(newElement);
                            }
                        }

                        if (listID != null)
                        {
                            BsonDocument oldElements = list.GetElement("elements").Value.ToBsonDocument();
                            foreach (BsonDocument document in (BsonArray)oldElements.GetElement("unorder").Value)
                            {
                                BsonElement element = document.First();
                                JObject newElement = new JObject();
                                newElement.Add(element.Name.ToString(),element.Value.ToString());
                                unorderElements.Add(newElement);
                            }
                        }
                    }
                    catch (Exception e) { /*Ignored*/ }
                    listElements.Add("unorder", unorderElements);

                    JArray orderElements = new JArray();
                    try //trying to create the unorder elements
                    {
                        BsonDocument uoElements = BsonDocument.Parse(formData["orderElements"]);
                        int index = 0;
                        foreach (BsonElement element in uoElements)
                        {
                            if (Regex.IsMatch(element.Name.ToString(), "[A-Za-z0-9.-_]+") && Regex.IsMatch(element.Value.ToString(), "[A-Za-z0-9._-áéíóúÁÉÍÓÚñÑ]*"))
                            {
                                JObject orderElement = new JObject();
                                orderElement.Add(element.Name.ToString(), element.Value.ToString());
                                orderElement.Add("position", index);
                                orderElements.Add(orderElement);
                                ++index;
                            }
                        }
                    }
                    catch (Exception e) { /*Ignored*/ }
                    listElements.Add("order",orderElements);
                    jsonData.Add("elements", listElements);

                    String jsonString = JsonConvert.SerializeObject(jsonData);

                    String savedID = listTable.saveRow(jsonString, listID);
                    _logTable.SaveLog(Session["_id"].ToString(), "Listas", "Insert: guardar lista", "List", DateTime.Now.ToString());
                    jsonData.Add("client-name", "cinepolis");
                    jsonData.Add("innerCollection", "list");
                    jsonData.Add("auxId", listID);
                    jsonData.Add("idInClient", savedID);

                    jsonString = JsonConvert.SerializeObject(jsonData);
                    jsonString = jsonString.Replace("\"", "'");
                    String jsonAgent = "{'action':'spreadToFather','collection':'ObjectReference','document':" + jsonString + ",'Source':'cinepolis'}";

                //    initAgent();
               //     agent.sendMessage(jsonAgent);
                    return savedID;
                }
                return null;
            } return null;
        }

        /// <summary>
        ///     This method allows to delete the list with the specified id
        /// </summary>
        /// <param name="id">
        ///     The list's id we want to delete
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns></returns>
        public void deleteList( String id ) {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("lists", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("lists", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                 if (this.Request.IsAjaxRequest())
                 {
                     listTable.deleteRow(id);
                     return;
                 }
                 return;
             } return;
        }

        /// <summary>
        ///     This method allows to delete the selected list
        /// </summary>
        /// <param name="array">
        ///     Array of lists we want to delete
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns></returns>
        public String deleteLists(List<String> array)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("lists", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("lists", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                 if (this.Request.IsAjaxRequest()) //only available with AJAX
                 {
                     try //tryign to delete the lists
                     {
                         if (array.Count == 0) return null; //if array is empty there are no users to delete
                         foreach (String id in array) //froeach id in the array we must delete the document with that id from the db
                         {
                             listTable.deleteRow(id);
                         }

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
        ///     This method allows to remove a list's element
        /// </summary>
        /// <param name="ListID">
        ///     The container list of the element
        /// </param>
        /// <param name="elementName">
        ///     The element's name to delete
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns></returns>
        public String removeElement(String ListID, String elementName)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("lists", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("lists", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest())
                {
                    BsonDocument document = listTable.getRow(ListID);
                    BsonDocument elements = document.GetElement("elements").Value.ToBsonDocument();
                    BsonArray unorderArray = (BsonArray)elements.GetElement("unorder").Value;
                    foreach (BsonDocument doc in unorderArray)
                    {
                        BsonElement element = doc.First();
                        if (element.Name == elementName)
                        {
                            unorderArray.Remove(doc); //we found it!!!!

                            String currentDocumentID = document.GetElement("_id").Value.ToString();
                            document.Remove("_id");
                            listTable.saveRow(document.ToJson(), currentDocumentID);
                            _logTable.SaveLog(Session["_id"].ToString(), "Listas", "Delete: eliminar elemento de lista", "List", DateTime.Now.ToString());
                            return null;
                        }
                    }
                }
                return null;
            } return null;
        }


        /// <summary>
        ///     This methood allows to change a list's elements
        /// </summary>
        /// <param name="ListID">
        ///     the list's id
        /// </param>
        /// <param name="elementName">
        ///     The element's name which we want to change
        /// </param>
        /// <param name="newValue">
        ///     The value to set to the element
        /// </param>
        /// <returns></returns>
        public String changeElement( String ListID, String elementName, String newValue ){

            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("lists", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("lists", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (this.Request.IsAjaxRequest())
                {
                    BsonDocument document = listTable.getRow(ListID);
                    BsonDocument elements = document.GetElement("elements").Value.ToBsonDocument();
                    BsonArray unorderArray = (BsonArray)elements.GetElement("unorder").Value;
                    foreach (BsonDocument doc in unorderArray)
                    {
                        BsonElement element = doc.First();
                        if (element.Name == elementName)
                        {
                            element.Value = newValue;

                            String currentDocumentID = document.GetElement("_id").Value.ToString();
                            document.Remove("_id");
                            listTable.saveRow(document.ToJson(), currentDocumentID);
                            _logTable.SaveLog(Session["_id"].ToString(), "Listas", "Update: modificar elemento de lista", "List", DateTime.Now.ToString());
                            return null;
                        }
                    }
                }
                return null;
            }
            else { return null; }
        }


        /// <summary>
        ///     This method allows to know if there is an element in a list
        /// </summary>
        /// <param name="listID">
        ///     The container list where the element is
        /// </param>
        /// <param name="elementName">
        ///     The element's name in the list
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a booleand. is this element in the gived list?
        /// </returns>
        public Boolean listHasElement(String listID, String elementName) {
            BsonDocument list = listTable.getRow(listID);
            if (list != null) {
                BsonDocument elements = list.GetElement("elements").Value.ToBsonDocument();

                //checking in order elements
                BsonArray order = (BsonArray)elements.GetElement("order").Value;
                foreach (BsonDocument document in order) {
                    foreach (BsonElement value in document) {
                        if (value.Name == elementName) return true;
                        break;
                    }
                }

                //checking in onorder elements
                BsonArray unorder = (BsonArray)elements.GetElement("unorder").Value;
                foreach (BsonDocument document in unorder)
                {
                    foreach (BsonElement value in document)
                    {
                        if (value.Name == elementName) return true;
                        break;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     This methis allows to know if a name is already in use by other list
        /// </summary>
        /// <param name="listName">
        ///     The  listName to check
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a boolean, does this name is already in use?
        /// </returns>
        public Boolean listNameUsed(String listName) {
            if (listTable.get("name", listName).Count() > 0) {
                return true;
            }
            return false;
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
    }
}
