using iTextSharp.text;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Db.MongoDb;
using Rivka.Security;
using RivkaAreas.Assignments.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.IO;
//using RivkaAreas.Employee.Models;
//using Rivka.Form;
//using System.Text.RegularExpressions;
//using System.Globalization;
//using Rivka.Images;
//using Rivka.Mail;
//using DocumentFormat.OpenXml.Spreadsheet;
//using DocumentFormat.OpenXml.Packaging;
//using System.Data;
//using Rivka.Files;
//using System.IO.Compression;



namespace RivkaAreas.Assignments.Controllers
{
    [Authorize]
    public class AssignmentsController : Controller
    {
        protected validatePermissions validatepermissions;
        protected MongoModel _witnessesModel;
        protected AssignmentsTable _assignmentsTable;
        protected EmployeeTable _employeeTable;
        protected ObjectReal _objectReal;
        protected EmployeeProfileTable _employeeProfileTable;
        protected LocationProfileTable locationProfileTable;
        protected LocationTable locationTable;
        protected UserTable userTable;
        protected UserProfileTable _userprofileTable;
        protected VoucherFolioNumberTable _VoucherFolioNumberTable;
        protected ListTable listTable;
        public AssignmentsController()
        {
            _assignmentsTable = new AssignmentsTable();
            _employeeTable = new EmployeeTable();
            _objectReal = new ObjectReal();
            validatepermissions = new validatePermissions();
            _employeeProfileTable = new EmployeeProfileTable();
            locationTable = new LocationTable();
            locationProfileTable = new LocationProfileTable();
            locationTable = new LocationTable();
            userTable = new UserTable();
            _userprofileTable = new UserProfileTable();
            _VoucherFolioNumberTable = new VoucherFolioNumberTable();
            listTable = new ListTable();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult newAssignment(string idAssignment)
        {

            if (!string.IsNullOrEmpty(idAssignment))
            {
                BsonDocument assignmentlist = _assignmentsTable.getRow(idAssignment);
                try
                {
                    String EmployeeString = _employeeTable.GetRow(assignmentlist.GetElement("employee").Value.ToString());
                    JObject Employee = JsonConvert.DeserializeObject<JObject>(EmployeeString);
                    ViewBag.EmployeeId = assignmentlist.GetElement("employee").Value.ToString();
                    ViewBag.EditAssignMentId = idAssignment;
                    //ViewBag.Employeedetail = Employee["name"] + " " + Employee["lastname"] + " " + Employee["motherlastname"] + " " + Employee["employee"];
                    var name = Employee["name"].ToString();
                    string lname = Employee["lastname"].ToString();
                    string mname = Employee["motherlastname"].ToString();
                    ViewBag.Employeedetail = name + " " + lname + " " + mname;

                }
                catch (Exception ex)
                {
                    ViewBag.EditAssignMentId = "";
                }
            }
            else
            {
                ViewBag.EditAssignMentId = "";
            }


            
            MongoCursor rowArray = locationTable.GetCursors();
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (BsonDocument item in rowArray)
            {
                item.GetElement("_id").Value.ToString();
                if (!data.ContainsKey(item.GetElement("_id").Value.ToString()))
                    data.Add(item.GetElement("_id").Value.ToString(), item.GetElement("name").Value.ToString());
            }
            ViewData["locations"] = data;
            return View();
        }
        public ActionResult getAssignmentTable()
        {
            String assignmentArray = _assignmentsTable.getRows();
            JArray assignment = JsonConvert.DeserializeObject<JArray>(assignmentArray);
            JArray result = new JArray();
            foreach (JObject resu in assignment)
            {
                try
                {
                    String EmployeeString = _employeeTable.GetRow(resu["employee"].ToString());
                    JObject Employee = JsonConvert.DeserializeObject<JObject>(EmployeeString);
                    resu["employeeId"] = Employee["employee"];
                    resu["Name"] = Employee["name"] + " " + Employee["lastname"];
                    resu["Image"] = "/Uploads/Images/" + Employee["_id"] + "." + Employee["imgext"];
                }
                catch (Exception e)
                {
                    resu["employeeId"] = "";
                    resu["Name"] = "";
                    resu["Image"] = "";
                }
                try
                {
                    string asset = "";
                    Int64 count = 0;
                    try
                    {
                        foreach (var item in resu["objectIds"])
                        {
                            String ObjectString = _objectReal.GetRow(item["id"].ToString());
                            JObject Object = JsonConvert.DeserializeObject<JObject>(ObjectString);
                            if (count == 0)
                            {
                                asset = Object["object_id"].ToString();
                            }
                            else
                            {
                                asset = asset + ";" + Object["object_id"].ToString();
                            }
                            count++;
                        }
                    }
                    catch (Exception ex)
                    { }

                    resu["AssetIds"] = asset;
                    if (asset != "")
                    {
                        result.Add(resu);
                    }
                }
                catch (Exception e)
                {
                    resu["AssetIds"] = "";
                }
            }
            return View(result);
        }

        public ActionResult Voucher()
        {
            return View();
        }

        //public ActionResult VoucherSubmodule(string stringval)
        //{
        //    string empName = "";
        //    string emplId = "";
        //    String em_ID = "";
        //    String empArray = _employeeTable.GetRowsAll();
        //    JArray empJArray = JsonConvert.DeserializeObject<JArray>(empArray);
        //    Dictionary<string, string> empList = new Dictionary<string, string>();

        //    if (!string.IsNullOrWhiteSpace(stringval))
        //    {
        //        foreach (JObject empResult in empJArray)
        //        {
        //            try
        //            {
        //                String _emID = empResult["_id"].ToString();
        //                String eId = empResult["employee"].ToString();
        //                String ename = empResult["name"].ToString();
        //                empList.Add(_emID, ename);
        //                if (eId == stringval || ename == stringval)
        //                {
        //                    em_ID = _emID;
        //                    emplId = ename;
        //                    empName = eId;
        //                }


        //            }
        //            catch (Exception ex)
        //            {

        //            }
        //        }
        //    }


        //    String assignmentArray = "";
        //    if (!string.IsNullOrWhiteSpace(stringval))
        //    {
        //        assignmentArray = _assignmentsTable.get("employee", em_ID).ToJson();
        //    }
        //    else
        //    {
        //        assignmentArray = _assignmentsTable.GetRows();
        //    }

        //    JArray assignment = JsonConvert.DeserializeObject<JArray>(assignmentArray);
        //    JArray result = new JArray();
        //    List<string> objectIds = new List<string>();
        //    string empId = "";


        //    foreach (JObject resu in assignment)
        //    {
        //        try
        //        {
        //            if (stringval != null && stringval != "")
        //            {
        //                String EmployeeString = _employeeTable.GetRow(em_ID);
        //                JObject Employee = JsonConvert.DeserializeObject<JObject>(EmployeeString);
        //                empName = Employee["employee"].ToString();
        //                emplId = Employee["name"].ToString();
        //                string employeeId = Employee["_id"].ToString();

        //                resu["Name"] = Employee["name"] + " " + Employee["lastname"];
        //                resu["Image"] = "/Uploads/Images/" + Employee["_id"] + "." + Employee["imgext"];
        //            }
        //            else
        //            {
        //                String EmployeeString = _employeeTable.GetRow(resu["employee"].ToString());
        //                JObject Employee = JsonConvert.DeserializeObject<JObject>(EmployeeString);
        //                resu["employeeId"] = Employee["employee"];
        //                empName = Employee["employee"].ToString();
        //                emplId = Employee["name"].ToString();
        //                string employeeId = Employee["_id"].ToString();

        //                resu["Name"] = Employee["name"] + " " + Employee["lastname"];
        //                resu["Image"] = "/Uploads/Images/" + Employee["_id"] + "." + Employee["imgext"];
        //            }

        //        }
        //        catch (Exception e)
        //        {
        //            resu["employeeId"] = "";
        //            resu["Name"] = "";
        //            resu["Image"] = "";
        //        }
        //        try
        //        {

        //            string asset = "";
        //            Int64 count = 0;
        //            try
        //            {
        //                foreach (var item in resu["objectIds"])
        //                {

        //                    String ObjectString = _objectReal.GetRow(item["id"].ToString());
        //                    JObject Object = JsonConvert.DeserializeObject<JObject>(ObjectString);
        //                    empId = Object["_id"].ToString();
        //                    objectIds.Add(empId);
        //                    if (count == 0)
        //                    {
        //                        asset = Object["object_id"].ToString();
        //                    }
        //                    else
        //                    {
        //                        asset = asset + ";" + Object["object_id"].ToString();
        //                    }
        //                    count++;
        //                }
        //            }
        //            catch (Exception ex)
        //            { }



        //            resu["AssetIds"] = asset;
        //            if (asset != "")
        //            {
        //                result.Add(resu);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            resu["AssetIds"] = "";
        //        }
        //    }
        //    BsonDocument locList = new BsonDocument();
        //    JArray resultReal = new JArray();
        //    JObject resReal = new JObject();
        //    ViewBag.EmployeeName = empName;
        //    ViewBag.EmployeeId = emplId;


        //    try
        //    {
        //        if (objectIds.Count != 0)
        //        {
        //            String ObjectRealFolio = _objectReal.GetRow(objectIds[0]);
        //            JObject ObjectFolio = JsonConvert.DeserializeObject<JObject>(ObjectRealFolio);
        //            string folio = ObjectFolio["folio"].ToString() == null ? "" : ObjectFolio["folio"].ToString();
        //            Session["Folio"] = folio;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Folio = ex.Message;
        //    }



        //    foreach (var _objectis in objectIds)
        //    {

        //        String ObjectReal = _objectReal.GetRow(_objectis);
        //        JObject Object = JsonConvert.DeserializeObject<JObject>(ObjectReal);
        //        int quantity = Convert.ToInt16(Object["quantity"]);
        //        string name = Object["name"].ToString();
        //        string location = Object["location"].ToString();

        //        locList = locationTable.getRow(location);
        //        String loc = locList.GetElement("name").Value.ToString();
        //        string assetType = Object["assetType"].ToString();
        //        resReal["name"] = name;
        //        resReal["location"] = loc;
        //        resReal["quantity"] = quantity;
        //        resReal["assetType"] = assetType;

        //        resultReal.Add(resReal);
        //    }

        //    return View(resultReal);
        //}


        public ActionResult VoucherSubmodule(String stringval)
        {
            JArray resultReturn = new JArray();
            Session["Folio"] = "";
            if (!String.IsNullOrEmpty(stringval))
            {
                String empArray = _employeeTable.GetRowsAll();
                JArray empJArray = JsonConvert.DeserializeObject<JArray>(empArray);
                Dictionary<string, string> empList = new Dictionary<string, string>();
                string employeeName = "";
                string employeelId = "";
                String em_ID = "";
                string em_Qty = "";
                foreach (JObject empResult in empJArray)
                {
                    try
                    {
                        String _emID = empResult["_id"].ToString();
                        String eId = empResult["employee"].ToString();
                        String ename = empResult["name"].ToString();
                        String lastname = empResult["lastname"].ToString();
                        String fullname = ename + " " + lastname;
                        empList.Add(_emID, ename);
                        if (eId == stringval || ename.ToLower() == stringval.ToLower() || fullname.ToLower() == stringval.ToLower() || lastname.ToLower() == stringval.ToLower())
                        {
                            em_ID = _emID;
                            employeelId = ename;
                            employeeName = eId;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                // Check folio number exists or not
                if (!string.IsNullOrEmpty(em_ID))
                {
                    BsonDocument foliolist = _VoucherFolioNumberTable.get("employee", em_ID);


                    if (foliolist.Elements.Count() != 0)
                    {
                        String folio_number = foliolist.GetElement("folionumber").Value.ToString();
                        Session["Folio"] = "00000" + folio_number;
                    }
                    else
                    {
                        BsonDocument getLastfolionumber = _VoucherFolioNumberTable.getLastRow();
                        if (getLastfolionumber.Count() != 0)
                        {
                            String lastEmp = getLastfolionumber.GetElement("employee").Value.ToString();
                            Int64 last_folio_number = Convert.ToInt64(getLastfolionumber.GetElement("folionumber").Value);

                            // Add Folio Number
                            last_folio_number = last_folio_number + 1;
                            String jsonData = "{'employee':'" + em_ID + "','folionumber':'" + last_folio_number + "'}";
                            string Voucherid = _VoucherFolioNumberTable.saveRow(jsonData);
                            Session["Folio"] = "00000" + last_folio_number;
                        }
                        else
                        {
                            Int64 new_folio_number = 1;
                            String jsonData = "{'employee':'" + em_ID + "','folionumber':'" + new_folio_number + "'}";
                            string Voucherid = _VoucherFolioNumberTable.saveRow(jsonData);
                            Session["Folio"] = "00000" + new_folio_number;
                        }
                    }
                }
                List<BsonDocument> assignmentlist = _assignmentsTable.get("employee", em_ID);

                if (assignmentlist.Count() == 0)
                {
                    return View(resultReturn);
                }
                else
                {
                    string id = this.Session["_id"].ToString();

                    String EmployeeString = _employeeTable.GetRow(em_ID);
                    JObject Employee = JsonConvert.DeserializeObject<JObject>(EmployeeString);

                    string empName = Employee["employee"].ToString();
                    ViewBag.EmployeeName = empName;
                    string emplId = Employee["name"].ToString();
                    string lname = Employee["lastname"].ToString();
                    ViewBag.EmployeeId = emplId + " " + lname;

                    ViewBag.Area = Employee["area"].ToString();
                
                    //BsonDocument locList = new BsonDocument();
                    JObject resReal = new JObject();
                    foreach (var item in assignmentlist)
                    {
                        String assignObject = item.GetElement("objectIds").Value.ToJson();

                        JArray assignment = JsonConvert.DeserializeObject<JArray>(assignObject);

                        em_Qty = assignment.Count().ToString();
                        ViewBag.AssetQuantity = em_Qty;

                        foreach (var oitem in assignment)
                        {
                            String _asset = _objectReal.getRow(oitem["id"].ToString(), item.GetElement("_id").Value.ToString());
                            if (_asset != "" && _asset != null)
                            {
                                JObject _assetobject = JsonConvert.DeserializeObject<JObject>(_asset);
                                int quantity = Convert.ToInt16(_assetobject["quantity"]);
                                string name = "";// _assetobject["name"].ToString();
                                string marca = "";
                                string modelo = "";

                                if (_assetobject["name"] != null)
                                    name = _assetobject["name"].ToString();
                                if (_assetobject["marca"] != null)
                                    marca = _assetobject["marca"].ToString();
                                if (_assetobject["modelo"] != null)
                                    modelo = _assetobject["modelo"].ToString();

                                string location = _assetobject["location"].ToString();
                                string EPC = _assetobject["EPC"].ToString();
                                string serie = _assetobject["serie"].ToString();
                                resReal["marca"] = marca;
                                resReal["EPC"] = EPC;
                                resReal["modelo"] = modelo;
                                resReal["serie"] = serie;

                                //Get Location hierarchy
                                BsonDocument assignment_list = _objectReal.getActivo("EPC", EPC);
                                String assign_Object = assignment_list.GetElement("_id").Value.ToJson();
                                string _pattern = assign_Object.Replace("ObjectId(\"", "");
                                string _replacement = _pattern.Replace("\")", "");
                                BsonDocument __asset = _objectReal.getbasonRow(_replacement);
                                string _location = __asset.GetElement("location").Value.ToString();
                                BsonDocument _locList = new BsonDocument();
                                string _Location = "";
                                bool flag = false;
                                if (_location != "" || _location != null)
                                {
                                    _locList = locationTable.getRow(_location);
                                    string _parentId = _locList.GetElement("parent").Value.ToString();
                                    _Location = _Location + _locList.GetElement("name").Value.ToString();
                                    if (_parentId != "")
                                    {
                                        for (int i = 0; i < __asset.Count(); i++)
                                        {
                                            _locList = locationTable.getRow(_parentId);
                                            _Location = _Location + "/ " + _locList.GetElement("name").Value.ToString();
                                            _parentId = _locList.GetElement("parent").Value.ToString();
                                            if (_parentId == "null")
                                            {
                                                flag = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                //locList = locationTable.getRow(location);
                                //String loc = locList.GetElement("name").Value.ToString();
                                string assetType = _assetobject["object_id"].ToString();
                                resReal["name"] = name;
                                resReal["location"] = _Location;
                                resReal["quantity"] = quantity;
                                resReal["assetType"] = assetType;
                                resultReturn.Add(resReal);
                            }
                        }
                    }

                }

                return View(resultReturn);
            }
            return View(resultReturn);
        }
        public String deleteAssignment(String assignmentID)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "d", dataPermissionsClient);

            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                if (access == true && accessClient == true)
                {
                    try
                    {
                        _assignmentsTable.deleteRow(assignmentID);
                        //Notificate the action

                        return "Registro Borrado";
                    }
                    catch (Exception e)
                    {
                        return "Ha ocurrido un error";
                    }
                }
                else
                { return null; }

            }
            else { return null; };
        }

        public ActionResult getEmployee(String data)
        {
            JArray resultReturn = new JArray();
            if (data == "" || data == null)
            {
                return View(resultReturn);
            }
            else
            {
                String employeeArray = _employeeTable.getRows();
                JArray employee = JsonConvert.DeserializeObject<JArray>(employeeArray);
                foreach (JObject employeeString in employee)
                {
                    if (!employeeString["type"].ToString().ToLower().Contains("empleado"))
                    {
                        BsonDocument perfil = _employeeProfileTable.getRow(employeeString["profileId"].ToString());
                        String EmployeeProfile = perfil.GetElement("name").Value.ToString();
                        if (employeeString["employee"].ToString().ToLower().Contains(data.ToLower()) || employeeString["name"].ToString().ToLower().Contains(data.ToLower()) || employeeString["lastname"].ToString().ToLower().Contains(data.ToLower()) || employeeString["motherlastname"].ToString().ToLower().Contains(data.ToLower()) || employeeString["type"].ToString().ToLower().Contains(data.ToLower()) || employeeString["area"].ToString().ToLower().Contains(data.ToLower()) || EmployeeProfile.ToLower().Contains(data.ToLower()))
                        {
                            employeeString["profileId"] = EmployeeProfile;
                            resultReturn.Add(employeeString);
                        }
                    }

                }
                return View(resultReturn);
            }
        }

        public ActionResult getAssetAssignedtoEmployee(String idEmployee)
        {
            List<BsonDocument> assignmentlist = _assignmentsTable.get("employee", idEmployee);
            JArray resultReturn = new JArray();
            foreach (var item in assignmentlist)
            {
                String assignObject = item.GetElement("objectIds").Value.ToJson();
                // JObject objectList = JsonConvert.DeserializeObject<JObject>(assignObject);
                JArray assignment = JsonConvert.DeserializeObject<JArray>(assignObject);

                foreach (var oitem in assignment)
                {
                    String _asset = _objectReal.getRow(oitem["id"].ToString(), item.GetElement("_id").Value.ToString());
                    if (_asset != "" && _asset != null)
                    {
                        JObject _assetobject = JsonConvert.DeserializeObject<JObject>(_asset);

                        resultReturn.Add(_assetobject);
                    }
                }
            }
            return View(resultReturn);
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


        public JsonResult getNodeContent2(String id, String userid = null)
        {
            JObject result = new JObject();
            if (id == "") id = "null";
            String categoriesString = "";
            if (id == "null" && userid != null)
            {
                String userstring = userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                userstring = _userprofileTable.GetRow(userobj["profileId"].ToString());
                JObject profilobj = JsonConvert.DeserializeObject<JObject>(userstring);
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
                    else
                    {
                        id = loadLocationsConjunto(userid);
                    }

                    result["id"] = id;
                    id = (id == null || id == "null") ? "" : id;
                    if (id == "") result["name"] = "";
                    else
                    {
                        try
                        {
                            categoriesString = locationTable.GetRow(id);
                            result["name"] = (JsonConvert.DeserializeObject<JObject>(categoriesString))["name"].ToString();
                        }
                        catch
                        {

                        }
                    }

                }
                else
                {
                    id = "null";
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
                JObject obj1 = new JObject();
                obj1["id"] = obj["_id"];
                obj1["text"] = obj["name"];
                obj1["hasChildren"] = true;

                //  obj1["items"] = "[]";
                obj1["spriteCssClass"] = "objectimg";
                newobjs.Add(obj1);
            }
            result["hijos"] = newobjs;

            //   return Json(JsonConvert.SerializeObject(newobjs), JsonRequestBehavior.AllowGet);
            return Json(JsonConvert.SerializeObject(result), JsonRequestBehavior.AllowGet);
        }
        //public JsonResult getNodeContent2(String id, String userid = null)
        //{
        //    JObject result = new JObject();
        //    if (id == "") id = "null";
        //    String categoriesString = "";
        //    if (id == "null" && userid != null)
        //    {
        //        String userstring = userTable.GetRow(userid);
        //        JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
        //        JObject profilobj = new JObject();
        //        try
        //        {
        //            userstring = _userprofileTable.GetRow(userobj["profileId"].ToString());
        //            JsonConvert.DeserializeObject<JObject>(userstring);
        //        }
        //        catch
        //        {
        //            profilobj.Add("name", "Administrador de sistema");
        //        }

        //        JToken tk;
        //        if (!profilobj.TryGetValue("name", out tk))
        //        {
        //            profilobj.Add("name", "Administrador de sistema");
        //        }
        //        if (profilobj["name"].ToString() != "Administrador de sistema" && profilobj["name"].ToString() != "Otro")
        //        {
        //            if (profilobj["name"].ToString() == "Gerente regional")
        //            {
        //                id = loadLocationsRegion(userid);

        //            }
        //            else if (profilobj["name"].ToString() == "Gerente de conjunto")
        //            {
        //                id = loadLocationsConjunto(userid);
        //            }
        //            else
        //            {
        //                id = loadLocationsConjunto(userid);
        //            }

        //            result["id"] = id;
        //            id = (id == null || id == "null") ? "" : id;
        //            if (id == "") result["name"] = "";
        //            else
        //            {
        //                try
        //                {
        //                    categoriesString = locationTable.GetRow(id);
        //                    result["name"] = (JsonConvert.DeserializeObject<JObject>(categoriesString))["name"].ToString();
        //                }
        //                catch
        //                {

        //                }
        //            }

        //        }
        //        else
        //        {
        //            id = "null";
        //            result["id"] = id;
        //            result["name"] = "Home";

        //        }
        //    }

        //    id = (id == "") ? "null" : id;
        //    categoriesString = locationTable.Get("parent", id);
        //    //  String objectsString = _objectTable.Get("parentCategory", id);

        //    if (categoriesString == null) return null; //there are no subcategories

        //    JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
        //    // JArray objectObject = JsonConvert.DeserializeObject<JArray>(objectsString);


        //    JArray newobjs = new JArray();

        //    foreach (JObject obj in categoriesObject)
        //    {
        //        try
        //        {
        //            JObject obj1 = new JObject();
        //            obj1["id"] = obj["_id"];
        //            obj1["text"] = obj["name"];
        //            obj1["hasChildren"] = true;

        //            //  obj1["items"] = "[]";
        //            obj1["spriteCssClass"] = "objectimg";
        //            newobjs.Add(obj1);
        //        }
        //        catch { }
        //    }
        //    result["hijos"] = newobjs;

        //    //   return Json(JsonConvert.SerializeObject(newobjs), JsonRequestBehavior.AllowGet);
        //    return Json(JsonConvert.SerializeObject(result), JsonRequestBehavior.AllowGet);
        //}
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


        public ActionResult AssignmentAsset(string assignmentId)
        {

            BsonDocument assignment = _assignmentsTable.getRow(assignmentId);
            string employeeDetail = "";
            string imagePath = "";
            if (assignment != "" && assignment != null)
            {
                String employeeId = assignment.GetElement("employee").Value.ToString();
                BsonDocument employee = _employeeTable.getRow(employeeId);
                if (employee != null && employee != "")
                {
                    employeeDetail = employee.GetElement("name").Value.ToString() + " " + employee.GetElement("lastname").Value.ToString() + " " + employee.GetElement("motherlastname").Value.ToString() + " " + employee.GetElement("employee").Value.ToString();

                    string relativepath = "/Uploads/Images/";
                    string absolutepathdir = Server.MapPath(relativepath);
                    string filename = employee.GetElement("_id").Value.ToString() + "." + employee.GetElement("imgext").Value.ToString();
                    string fileabsolutepath = absolutepathdir + filename;


                    if (System.IO.File.Exists(fileabsolutepath))
                    {
                        imagePath = Url.Content(relativepath + filename);

                    }

                }


            }
            ViewBag.ImagePath = imagePath;
            ViewBag.employeeDetail = employeeDetail;
            ViewBag.assignmentId = assignmentId;
            return View();
        }
        public ActionResult AssignmentAssetList(String assignmentId)
        {
            BsonDocument assignmentlist = _assignmentsTable.getRow(assignmentId);
            JArray resultReturn = new JArray();
            if (assignmentlist != null)
            {
                String assignObject = assignmentlist.GetElement("objectIds").Value.ToJson();
                JArray assignment = JsonConvert.DeserializeObject<JArray>(assignObject);
                foreach (var oitem in assignment)
                {
                    String _asset = _objectReal.getRow(oitem["id"].ToString(), "");
                    if (_asset != "" && _asset != null)
                    {
                        JObject _assetobject = JsonConvert.DeserializeObject<JObject>(_asset);
                        resultReturn.Add(_assetobject);
                    }
                }
            }
            return View(resultReturn);
        }

        public ActionResult filterAssetList(String data, string locationId)
        {
            bool isconjunt = false;
            try
            {
                JObject location1 = JsonConvert.DeserializeObject<JObject>(locationTable.GetRow(locationId));

                JObject categories = JsonConvert.DeserializeObject<JObject>(locationProfileTable.GetRow(location1["profileId"].ToString()));

                if (categories["name"].ToString().ToLower().Contains("ubicacion") || categories["name"].ToString().ToLower().Contains("sub-ubicaciones"))
                {
                    isconjunt = true;
                }

                //results = _objectTable.GetSubObjects(locationId, filter);
                //JArray objectList = JsonConvert.DeserializeObject<JArray>(results);
                //Dictionary<string, string> namesdict = new Dictionary<string, string>();

            }
            catch { }

            JArray resultReturn = new JArray();

            if (isconjunt)
            {
                //string Objectstxt = _objectReal.FilterObjectsByText(data, locationId);
                string Objectstxt = _objectReal.GetSubObjects(locationId);
                JArray objectsArray = JsonConvert.DeserializeObject<JArray>(Objectstxt);

                foreach (var oitem in objectsArray)
                {
                    resultReturn.Add(oitem);
                }
            }
            return View(resultReturn);
        }

        //public String GetAllSubObjects(string locationId, int skip = 0, bool filter = false)
        //{
        //    string results = "";
        //    String categoryString = "";
        //    JArray categories = new JArray();
        //    try
        //    {
        //        results = _objectTable.GetSubObjects(locationId, filter);
        //        JArray objectList = JsonConvert.DeserializeObject<JArray>(results);
        //        Dictionary<string, string> namesdict = new Dictionary<string, string>();
        //        try
        //        {
        //            List<string> objreflist = (from obj in objectList select (string)obj["objectReference"]).ToList();
        //            RivkaAreas.Reports.Models.ObjectsRealReport custom = new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
        //            JArray objref = JsonConvert.DeserializeObject<JArray>(custom.GetbyCustom("_id", objreflist, "ReferenceObjects"));
        //            foreach (JObject o in objreflist)
        //            {
        //                try
        //                {
        //                    namesdict.Add(o["_id"].ToString(), o["name"].ToString());
        //                }
        //                catch { }
        //            }

        //        }
        //        catch { }
        //        Dictionary<string, JObject> listrefobj = new Dictionary<string, JObject>();

        //        try
        //        {
        //            List<string> listref = (from obj2 in objectList select (string)obj2["objectReference"]).ToList();
        //            JArray jaref = JsonConvert.DeserializeObject<JArray>(_objectTable.getRefObj(listref));

        //            foreach (JObject item in jaref)
        //            {
        //                try
        //                {
        //                    listrefobj.Add(item["_id"].ToString().ToLower(), item);
        //                }
        //                catch { }
        //            }
        //        }
        //        catch { }
        //        categoryString = categoryTable.GetRows();
        //        //doing changes to objects array
        //        categories = JsonConvert.DeserializeObject<JArray>(categoryString);

        //        Dictionary<string, string> listCategories = new Dictionary<string, string>();
        //        foreach (JObject items in categories)
        //        {
        //            listCategories.Add(items["_id"].ToString(), items["name"].ToString());
        //        }
        //        int take = 0;
        //        totalglobal = objectList.Count();
        //        JArray paginationja = new JArray();
        //        List<string> idsact1 = new List<string>();
        //        Dictionary<string, string> actin = new Dictionary<string, string>();
        //        Dictionary<string, string> actout = new Dictionary<string, string>();
        //        JArray getdemandin = new JArray();
        //        JArray getdemandout = new JArray();
        //        try
        //        {
        //            idsact1 = (from id in objectList select (string)id["_id"]).ToList();
        //            try
        //            {
        //                getdemandin = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 0));
        //            }
        //            catch { }
        //            try
        //            {
        //                getdemandout = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 1));

        //            }
        //            catch { }
        //        }
        //        catch
        //        {

        //        }
        //        List<string> idsact = new List<string>();
        //        foreach (JObject document in objectList) //for each profile we create an option element with id as value and the name as the text
        //        {
        //            // if (objectList.IndexOf(document) < skip)
        //            /* if(indexfor>skip)
        //             {
        //                 //continue;
        //                 return;
        //             }
        //             else
        //             {
        //                 /*if (take == 5000)

        //                     break;
        //                   take++;*/
        //            /*   if (indexfor >= 5000)
        //                   pls.Stop();

        //           }*/
        //            try
        //            {
        //                idsact.Add(document["_id"].ToString());
        //            }
        //            catch
        //            {

        //            }
        //            try
        //            {
        //                JObject objo = new JObject();
        //                JToken tk;
        //                if (listrefobj.TryGetValue(document["objectReference"].ToString().ToLower(), out objo))
        //                {
        //                    if (!document.TryGetValue("name", out tk))
        //                        document.Add("name", "");
        //                    if (!document.TryGetValue("modelo", out tk))
        //                        document.Add("modelo", "");
        //                    if (!document.TryGetValue("marca", out tk))
        //                        document.Add("marca", "");
        //                    if (!document.TryGetValue("nameassetType", out tk))
        //                        document.Add("nameassetType", "");
        //                    if (!document.TryGetValue("object_id", out tk))
        //                        document.Add("object_id", "");
        //                    if (!document.TryGetValue("proveedor", out tk))
        //                        document.Add("proveedor", "");
        //                    if (!document.TryGetValue("object_id_ref", out tk))
        //                        document.Add("object_id_ref", "");
        //                    try
        //                    {

        //                        document["object_id_ref"] = objo["object_id"].ToString();
        //                    }
        //                    catch { }
        //                    try
        //                    {
        //                        document["nameassetType"] = objo["nameassetType"].ToString();
        //                    }
        //                    catch { }
        //                    try
        //                    {

        //                        document["name"] = objo["name"].ToString();
        //                    }
        //                    catch { }
        //                    try
        //                    {

        //                        document["marca"] = objo["marca"].ToString();
        //                    }
        //                    catch { }
        //                    try
        //                    {
        //                        document["modelo"] = objo["modelo"].ToString();
        //                    }
        //                    catch { }
        //                    try
        //                    {
        //                        document["proveedor"] = objo["proveedor"].ToString();
        //                    }
        //                    catch { }
        //                    string mm = "";
        //                    if (namesdict.TryGetValue(document["objectReference"].ToString(), out mm))
        //                    {
        //                        document["name"] = mm;
        //                    }
        //                }
        //            }
        //            catch { }
        //            try
        //            {
        //                if (listCategories.ContainsKey(document["parentCategory"].ToString()))
        //                    document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);
        //            }
        //            catch { }
        //            document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();

        //            try
        //            {
        //                //    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
        //                document["currentmove"] = "";
        //                foreach (JObject item in getdemandin)
        //                {
        //                    try
        //                    {
        //                        foreach (JObject obj in item["objects"])
        //                        {
        //                            try
        //                            {
        //                                if (obj["id"].ToString() == document["_id"].ToString())
        //                                {
        //                                    document["currentmove"] = item["folio"].ToString() + " " + item["namemov"].ToString();
        //                                    break;
        //                                }
        //                            }
        //                            catch
        //                            {

        //                            }
        //                        }
        //                    }
        //                    catch
        //                    {

        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                document["currentmove"] = "";
        //            }

        //            try
        //            {
        //                //  document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
        //                document["allmoves"] = "";
        //                List<string> folioslist = new List<string>();
        //                foreach (JObject item in getdemandout)
        //                {
        //                    try
        //                    {
        //                        foreach (JObject obj in item["objects"])
        //                        {
        //                            try
        //                            {
        //                                if (obj["id"].ToString() == document["_id"].ToString())
        //                                {
        //                                    folioslist.Add(item["folio"].ToString() + " " + item["namemov"].ToString());
        //                                    break;
        //                                }
        //                            }
        //                            catch
        //                            {

        //                            }
        //                        }
        //                    }
        //                    catch
        //                    {

        //                    }
        //                }
        //                try
        //                {
        //                    document["allmoves"] = String.Join(",\n ", folioslist);
        //                }
        //                catch
        //                {
        //                    document["allmoves"] = ".";
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                document["allmoves"] = "";
        //            }

        //            try
        //            {
        //                if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
        //                {
        //                    document["status"] = "En movimiento";
        //                    numMov++;
        //                }
        //                else
        //                {
        //                    if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
        //                    {
        //                        document["status"] = "Dado de baja";
        //                        numBaja++;
        //                    }
        //                    else
        //                    {
        //                        document["status"] = "Está en tu oficina";
        //                        numActivos++;
        //                    }

        //                }

        //            }
        //            catch
        //            {
        //                document["status"] = "Está en tu oficina";
        //                numActivos++;
        //            }
        //            try
        //            {
        //                if (document["label"].ToString() == "normal")
        //                    document["etiquetado"] = "Normal";
        //                else
        //                    document["etiquetado"] = "No Etiquetable";
        //            }
        //            catch { }
        //            try
        //            {
        //                /*  if (document["assetType"].ToString().ToLower().Contains("system"))
        //                      document["nameassetType"] = "Sistemas";
        //                  else if (document["assetType"].ToString().ToLower().Contains("maintenance"))
        //                      document["nameassetType"] = "Mantenimiento";
        //                 else
        //                  {
        //                      document["nameassetType"] = "Vinos";
        //                  }*/
        //            }
        //            catch { }
        //            if (departs != null)
        //            {
        //                try
        //                {
        //                    if (departs.ContainsKey(document["department"].ToString()))
        //                        document["departmentName"] = departs[document["department"].ToString()];
        //                }
        //                catch
        //                {
        //                    document["departmentName"] = "";
        //                }
        //            }
        //            else
        //            {
        //                document["departmentName"] = "";
        //            }
        //            if (document["ext"].ToString() != "")
        //            {
        //                document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
        //                document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]);
        //            }

        //            paginationja.Add(document);
        //        }
        //        objectList = paginationja;
        //        try
        //        {
        //            RivkaAreas.Reports.Models.ObjectsRealReport objdc = new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
        //            JArray actjo = JsonConvert.DeserializeObject<JArray>(objdc.GetbyCustom("_id", idsact, "ObjectReal"));
        //            Dictionary<string, string> namesdict2 = actjo.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
        //            Dictionary<string, string> namesdict4 = actjo.ToDictionary(x => (string)x["_id"], x => (string)x["assetType"]);

        //            JArray newja = new JArray();
        //            foreach (JObject obj in objectList)
        //            {
        //                try
        //                {

        //                    JToken tk;
        //                    if (namesdict2.ContainsKey(obj["_id"].ToString()))
        //                    {
        //                        string name = namesdict2[obj["_id"].ToString()];
        //                        if (name.Length > 0)
        //                        {
        //                            //obj["name"] = name;
        //                        }
        //                    }
        //                }
        //                catch { }
        //                try
        //                {


        //                    if (namesdict4.ContainsKey(obj["_id"].ToString()))
        //                    {
        //                        string nameasset = namesdict4[obj["_id"].ToString()];
        //                        //if (nameasset.Length > 0)
        //                        //   obj["nameassetType"] = nameasset;
        //                    }
        //                }
        //                catch { }

        //                newja.Add(obj);
        //            }
        //            objectList = newja;
        //        }
        //        catch
        //        {

        //        }

        //        return objectList.ToString();
        //    }
        //    catch (Exception e)
        //    {
        //        Error.Log(e, "Trying to get all Objects");
        //    }


        //    return results;
        //}

        public String GetEmployeeDetail(String assignmentId)
        {
            if (this.Request.IsAjaxRequest())
            {
                try
                {

                    BsonDocument assignment = _assignmentsTable.getRow(assignmentId);
                    if (assignment != null && assignment != "")
                    {
                        String employeeID = assignment.GetElement("employee").Value.ToString();
                        BsonDocument doc = _employeeTable.getRow(employeeID); //getting the user's data
                        if (doc != null && doc != "")
                        {
                            //the next is the photo's information
                            string relativepath = "/Uploads/Images/";
                            string absolutepathdir = Server.MapPath(relativepath);
                            string filename = doc["_id"].ToString() + "." + doc["imgext"].ToString();
                            string fileabsolutepath = absolutepathdir + filename;





                            BsonDocument perfil = _employeeProfileTable.getRow(doc.GetElement("profileId").Value.ToString());
                            //document.Set("Creator", "YO");//
                            doc.Set("profileId", perfil.GetElement("name").Value);

                            if (doc == null)
                                return "null";
                            doc.Remove("_id");

                            if (System.IO.File.Exists(fileabsolutepath))
                            {
                                string url = Url.Content(relativepath + filename);
                                doc.Add(new BsonElement("ImgUrl", url)); //adding the image's url to the document
                            }
                            else
                            {
                                doc.Add(new BsonElement("ImgUrl", "NoImage")); //adding the image's url to the document

                            }
                            return doc.ToJson(); //returns the json
                        }
                        return null;

                    }

                    return null;


                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public ActionResult UnAssignAsset(String assignmentId, String assetId, String PartialViewName)
        {
            BsonDocument assignmentlist = _assignmentsTable.getRow(assignmentId);
            JArray resultReturn = new JArray();

            if (assignmentlist != null)
            {
                String assignObject = assignmentlist.GetElement("objectIds").Value.ToJson();
                JArray objectList = JsonConvert.DeserializeObject<JArray>(assignObject);
                String jsonData = "{'employee':'" + assignmentlist.GetElement("employee").Value.ToString() + "";
                jsonData += "','creatorId':'";
                jsonData += this.Session["_id"];
                jsonData += "'";
                jsonData += ", 'objectIds':[";
                foreach (var oitem in objectList)
                {
                    if (assetId != oitem["id"].ToString())
                    {
                        String _asset = _objectReal.getRow(oitem["id"].ToString(), "");
                        if (_asset != "" && _asset != null)
                        {
                            JObject _assetobject = JsonConvert.DeserializeObject<JObject>(_asset);
                            resultReturn.Add(_assetobject);
                        }
                    }
                }
                int cont = 0;
                foreach (var item in resultReturn)
                {
                    jsonData += "{ 'id' : '" + item["_id"].ToString() + "'}";
                    cont++;
                    if (cont < resultReturn.Count())
                    {
                        jsonData += ", ";
                    }
                }
                jsonData += "]}";
                _assignmentsTable.saveRow(jsonData, assignmentId);
            }

            return PartialView(PartialViewName, resultReturn);
        }

        public String SaveAssignment(string assignmentId, string employee, string[] AssetIds)
        {
            try
            {
                String jsonData = "{'employee':'" + employee + "";
                jsonData += "','creatorId':'";
                jsonData += this.Session["_id"];
                jsonData += "'";
                jsonData += ", 'objectIds':[";
                BsonDocument assignmentlist = new BsonDocument();
                try
                {
                    if (assignmentId == "")
                    {
                        assignmentlist = _assignmentsTable.get("employee", employee).FirstOrDefault();
                    }
                    else
                    {
                        assignmentlist = _assignmentsTable.getRow(assignmentId);
                    }
                    assignmentId = assignmentlist.GetElement("_id").Value.ToString();
                    String assignObject = assignmentlist.GetElement("objectIds").Value.ToJson();
                    JArray objectList = JsonConvert.DeserializeObject<JArray>(assignObject);
                    foreach (var oitem in objectList)
                    {
                        jsonData += "{ 'id' : '" + oitem["id"].ToString() + "'}";
                        jsonData += ", ";
                    }
                }
                catch (Exception ex)
                {
                }
                string AssetList = "";
                for (var i = 0; i < AssetIds.Count(); i++)
                {
                    if (!jsonData.Contains(AssetIds[i]))
                    {
                        List<BsonDocument> IsAsset_Assign_to_other_User = _assignmentsTable.get("objectIds.id", AssetIds[i]);
                        if (IsAsset_Assign_to_other_User.Count == 0)
                        {
                            AssetList += AssetIds[i] + ",";
                        }

                    }

                }
                AssetList = AssetList.TrimEnd(',');

                string[] _newAssetList = AssetList.Split(',');

                int cont = 0;
                for (var i = 0; i < _newAssetList.Count(); i++)
                {
                    if (_newAssetList[i] != "")
                    {
                        jsonData += "{ 'id' : '" + _newAssetList[i] + "'}";
                        cont++;
                        if (cont < _newAssetList.Count())
                        {
                            jsonData += ", ";
                        }
                    }


                }
                jsonData += "]}";
                if (assignmentId == "" || assignmentId == "null" || assignmentId == null)
                { assignmentId = _assignmentsTable.saveRow(jsonData, null); }
                else { _assignmentsTable.saveRow(jsonData, assignmentId); }


                return assignmentId;
            }
            catch
            {
                return null;
            }

        }

        //public String GetAssetDetail(String AssetId)
        //{
        //    if (this.Request.IsAjaxRequest())
        //    {
        //        try
        //        {
        //            BsonDocument doc = _objectReal.getbasonRow(AssetId); //getting the user's data
        //            if (doc != null && doc != "")
        //            {
        //                //the next is the photo's information
        //                string relativepath = "/Uploads/Images/";
        //                string absolutepathdir = Server.MapPath(relativepath);
        //                string filename = doc["_id"].ToString() + "." + doc["imgext"].ToString();
        //                string fileabsolutepath = absolutepathdir + filename;





        //                BsonDocument perfil = _employeeProfileTable.getRow(doc.GetElement("profileId").Value.ToString());
        //                //document.Set("Creator", "YO");//
        //                doc.Set("profileId", perfil.GetElement("name").Value);

        //                if (doc == null)
        //                    return "null";
        //                doc.Remove("_id");

        //                if (System.IO.File.Exists(fileabsolutepath))
        //                {
        //                    string url = Url.Content(relativepath + filename);
        //                    doc.Add(new BsonElement("ImgUrl", url)); //adding the image's url to the document
        //                }
        //                else
        //                {
        //                    doc.Add(new BsonElement("ImgUrl", "NoImage")); //adding the image's url to the document

        //                }
        //                return doc.ToJson(); //returns the json
        //            }
        //            return null;




        //        }
        //        catch (Exception e)
        //        {
        //            return null;
        //        }
        //    }
        //    return null;
        //}
    }
}
