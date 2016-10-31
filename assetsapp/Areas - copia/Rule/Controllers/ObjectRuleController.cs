using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rivka.Db.MongoDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.OleDb;

using System.Xml;
using Rivka.Security;
using System.Configuration;
namespace RivkaAreas.Rule.Controllers
{
    [Authorize]
    public class ObjectRuleController : Controller
    {
        //
        // GET: /Rule/Rule/
        protected MongoModel objectModel = new MongoModel("ReferenceObjects");
        protected MongoModel HwModel = new MongoModel("Hardware");
        protected MongoModel ProccessModel = new MongoModel("Processes");
        protected MongoModel locationModel = new MongoModel("Locations");
        protected MongoModel HwRulesModel = new MongoModel("HardwareRules");
        protected MongoModel RfRulesModel = new MongoModel("ReferenceObjectsRules");
        protected MongoModel LocRulesModel = new MongoModel("LocationRules");
        protected validatePermissions validatepermissions = new validatePermissions();
        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("rules", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("rules", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                return View();
            }
            else
            {
                return Redirect("~/Home");
            }
        }


        public ActionResult ImpExcel(HttpPostedFileBase file)
        {
            DataSet ds = new DataSet();
            List<List<string>> tr = new List<List<string>>();
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
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {

                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

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
                }
                if (fileExtension.ToString().ToLower().Equals(".xml"))
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



                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    List<string> td = new List<string>();

                    for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                    {
                        td.Add(ds.Tables[0].Rows[i][j].ToString());
                    }

                    tr.Add(td);
                }


            }

            return View(tr);
        }
        public ActionResult getType(string type)
        {

            try
            {
                string rowsArray = "";

                if (type == "1") { rowsArray = HwModel.GetRows(); }
                if (type == "2") { rowsArray = objectModel.GetRows(); }
                if (type == "3") { rowsArray = locationModel.GetRows(); }

                //  rowsArray = locationModel.GetRows();
                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray dataarray = JsonConvert.DeserializeObject<JArray>(rowsArray);
                Dictionary<string, string> data = new Dictionary<string, string>();

                foreach (JObject items in dataarray)
                {


                    data.Add(items["_id"].ToString(), items["name"].ToString());
                    // data.Add(items["email"].ToString());
                }

                ViewData["type"] = type;
                return View(data);
            }
            catch (Exception ex)
            {

                return null;
            }

        }
        public ActionResult getProccess(string id)
        {

            try
            {
                string rowsArray = "";
                string rowsHwrules = "";

                rowsHwrules = HwRulesModel.GetRows();
                JArray datahwrules = JsonConvert.DeserializeObject<JArray>(rowsHwrules);
                rowsArray = ProccessModel.GetRows();
                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray dataarray = JsonConvert.DeserializeObject<JArray>(rowsArray);
                Dictionary<string, string> data = new Dictionary<string, string>();
                List<List<string>> locationsA = new List<List<string>>();

                // List<string> locationsj = new List< string>();

                List<Dictionary<string, string>> getcheckproccessA = new List<Dictionary<string, string>>();
                foreach (JObject item in datahwrules)
                {
                    if (id == item["IdHardware"].ToString())
                    {
                        foreach (JObject hw in item["Rules"])
                        {
                            Dictionary<string, string> getcheckProccess = new Dictionary<string, string>();

                            foreach (var location in hw["locations"])
                            {

                                getcheckProccess.Add(location.ToString(), hw["idproccess"].ToString());
                            }

                            if (getcheckProccess.Count() != 0)
                            {
                                getcheckproccessA.Add(getcheckProccess);
                            }
                        }

                    }
                }

                foreach (JObject items in dataarray)
                {
                    List<string> locationsj = new List<string>();
                    try
                    {
                        foreach (JObject hw in items["rules"])
                        {


                            if (id == hw["Hardware"].ToString())
                            {
                                data.Add(items["_id"].ToString(), items["name"].ToString());
                                foreach (var location in hw["Locations"])
                                {

                                    locationsj.Add(location.ToString());
                                }
                            }
                        }
                        if (locationsj.Count() != 0)
                        {
                            locationsA.Add(locationsj);
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    // data.Add(items["email"].ToString());
                }
                List<Dictionary<string, string>> getlocationsA = new List<Dictionary<string, string>>();
                int idrepeat = 0;
                foreach (List<string> bilist in locationsA)
                {
                    try
                    {
                        Dictionary<string, string> getlocations = new Dictionary<string, string>();
                        idrepeat = 0;
                        foreach (string x in bilist)
                        {
                            string y = locationModel.GetRow(x);
                            //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                            JObject locationarray1 = JsonConvert.DeserializeObject<JObject>(y);


                            getlocations.Add(locationarray1["_id"].ToString(), locationarray1["name"].ToString());


                        }
                        getlocationsA.Add(getlocations);
                    }
                    catch (Exception ex)
                    {

                        continue;
                    }
                }

                ViewData["checks"] = getcheckproccessA;
                ViewData["locations"] = getlocationsA;

                return View(data);

            }
            catch (Exception ex)
            {

                return null;
            }

        }

        public ActionResult getProccessObj(string id)
        {

            try
            {
                string rowsArray = "";

                string rowsRfrules = "";

                rowsRfrules = RfRulesModel.GetRows();
                JArray datahwrules = JsonConvert.DeserializeObject<JArray>(rowsRfrules);
                List<Dictionary<string, string>> getcheckproccessA = new List<Dictionary<string, string>>();
                foreach (JObject item in datahwrules)
                {
                    if (id == item["IdReferenceObj"].ToString())
                    {
                        foreach (JObject Rf in item["Rules"])
                        {
                            Dictionary<string, string> getcheckProccess = new Dictionary<string, string>();

                            foreach (var location in Rf["locations"])
                            {

                                getcheckProccess.Add(location.ToString(), Rf["idproccess"].ToString());
                            }

                            getcheckproccessA.Add(getcheckProccess);
                        }

                    }
                }

                rowsArray = ProccessModel.GetRows();
                JArray dataarray = JsonConvert.DeserializeObject<JArray>(rowsArray);
                Dictionary<string, string> data = new Dictionary<string, string>();
                List<List<string>> locationsA = new List<List<string>>();

                // List<string> locationsj = new List< string>();

                foreach (JObject items in dataarray)
                {
                    List<string> locationsj = new List<string>();
                    try
                    {
                        foreach (JObject hw in items["rules"])
                    {
                        foreach (var location in hw["locations"])
                        {

                            if (!locationsj.Contains(location.ToString()))
                            {
                                locationsj.Add(location.ToString());
                            }
                        }
                    }
                        data.Add(items["_id"].ToString(), items["name"].ToString());

                    locationsA.Add(locationsj);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    // data.Add(items["email"].ToString());
                }
                List<Dictionary<string, string>> getlocationsA = new List<Dictionary<string, string>>();
                foreach (List<string> bilist in locationsA)
                {
                    Dictionary<string, string> getlocations = new Dictionary<string, string>();
                    try
                    {
                        foreach (string x in bilist)
                        {
                            string y = locationModel.GetRow(x);
                            if (y == null) continue;
                            //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                            JObject locationarray1 = JsonConvert.DeserializeObject<JObject>(y);

                            try
                            {
                                getlocations.Add(locationarray1["_id"].ToString(), locationarray1["name"].ToString());
                            }
                            catch (Exception ex) { continue; }
                        }
                        getlocationsA.Add(getlocations);
                    }
                    catch (Exception ex)
                    {

                        continue;
                    }

                }

                ViewData["locations"] = getlocationsA;
                ViewData["checks"] = getcheckproccessA;
               
                return View(data);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public ActionResult getProccessLoc(string id)
        {
            try
            {
                string rowsArray = "";
                string rowsLocrules = LocRulesModel.GetRows();
                JArray dataLocrules = JsonConvert.DeserializeObject<JArray>(rowsLocrules);
                rowsArray = objectModel.GetRows();

                JArray dataarray = JsonConvert.DeserializeObject<JArray>(rowsArray);
                Dictionary<string, string> data = new Dictionary<string, string>();

                // List<string> locationsj = new List< string>();
                Dictionary<string, string> getcheckRf = new Dictionary<string, string>();

                string settypelist = "";
                foreach (JObject item in dataLocrules)
                {
                    if (id == item["IdLocation"].ToString())
                    {
                        settypelist = item["Type"].ToString();
                        foreach (var Loc in item["RefObjects"])
                        {
                            getcheckRf.Add(Loc.ToString(), item["IdLocation"].ToString());
                        }
                    }
                }

                foreach (JObject items in dataarray)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString());
                }
                ViewData["typelist"] = settypelist;
                ViewData["idlocation"] = id;
                ViewData["checks"] = getcheckRf;
                if (data.Count > 0)
                {
                    return View(data);
                }
                else
                {

                    return null;
                }

            }
            catch (Exception ex)
            {

                return null;
            }

        }

        public string saveHwRules(string id, string data)
        {
            string rowsHwrules = HwRulesModel.GetRows();
            JArray datahwrules = JsonConvert.DeserializeObject<JArray>(rowsHwrules);
            string idhw = "";
            foreach (JObject item in datahwrules)
            {
                if (id == item["IdHardware"].ToString())
                {
                    idhw = item["_id"].ToString();
                    //string delete = HwRulesModel.DeleteRow(item["_id"].ToString());
                }
            }
            String json = "{'IdHardware':'" + id + "','Rules':" + data + "}";

            string idrule = HwRulesModel.SaveRow(json, idhw);



            return "saved";


        }


        public string saveObjRules(string id, string data)
        {
            string rowsRfrules = RfRulesModel.GetRows();
            JArray dataRfrules = JsonConvert.DeserializeObject<JArray>(rowsRfrules);
            string idrf = "";

            foreach (JObject item in dataRfrules)
            {
                if (id == item["IdReferenceObj"].ToString())
                {
                    idrf = item["_id"].ToString();
                    /// string delete = RfRulesModel.DeleteRow(item["_id"].ToString());
                }
            }

            String json = "{'IdReferenceObj':'" + id + "','Rules':" + data + "}";
            string idrule = RfRulesModel.SaveRow(json, idrf);
            return "saved";
        }


        public string saveLocRules(string id, string data, string type)
        {




            string rowsLocrules = LocRulesModel.GetRows();
            JArray dataLocrules = JsonConvert.DeserializeObject<JArray>(rowsLocrules);
            string x = "";
            if (type == "0")
            {
                x = "Permitidos";
            }
            else
            {
                x = "Denegados";
            }
            string idruleedit = "";
            foreach (JObject item in dataLocrules)
            {
                if (id == item["IdLocation"].ToString())
                {
                    idruleedit = item["_id"].ToString();
                    // string delete = LocRulesModel.DeleteRow(item["_id"].ToString());
                }
            }
            String json = "{'IdLocation':'" + id + "','Type':'" + x + "','RefObjects':" + data + "}";

            string idrule = LocRulesModel.SaveRow(json, idruleedit);



            return "saved";


        }

    }

}


