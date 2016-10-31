using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RivkaAreas.Inventory.Models;
using System.Data.SqlServerCe;
using System.Data;
using System.Net;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using iTextSharp.text.html.simpleparser;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.CustomXmlDataProperties;
using System.IO;
//using System.Windows.Forms;
using System.IO.Compression;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Diagnostics;
//using System.Windows.Forms;
using Rivka.Error;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Inventory.Controllers
{
    [Authorize]
    public class SceneInventoryController : Controller
    {
        //
        // GET: /Inventory/ScenceInventory/
        protected InventoryTable _inventoryTable;
        protected UserTable _userTable;
        protected LocationTable _locationTable;
        protected ObjectTable _objectTable;
        protected ObjectRealTable _objectRealTable;
        protected UserTable usersdb;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        protected MongoModel locationsProfilesdb = new MongoModel("LocationProfiles");

        public SceneInventoryController()
        {
            this._inventoryTable = new InventoryTable();
            this._userTable = new UserTable();
            this._locationTable = new LocationTable();
            this._objectTable = new ObjectTable();
            this._objectRealTable = new ObjectRealTable();
            this.usersdb = new UserTable();
            this._logTable = new LogBook.Controllers.LogBookController();
            bindingSessions();
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///     This method allows to get the Inventory of the user sigin
        /// </summary>
        /// <param name="selectOption"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns Json with the information
        /// </returns>

        public ActionResult getInventoryTable(String selectOption)
        {
            string userid = Session["_id"].ToString();
            DateTime thisDay = DateTime.Today.Date;

            String userString = _userTable.GetRow(userid);
            JObject user = JsonConvert.DeserializeObject<JObject>(userString);

            String inventoryArray = _inventoryTable.Get("profile", user["profileId"].ToString(), "dateStart");
            JArray inventory = JsonConvert.DeserializeObject<JArray>(inventoryArray);

            JArray result = new JArray();

            foreach (JObject inventoryString in inventory)
            {

                //Check if it is a user asiggned to do the inventory
                //**All profile
                JToken userList = inventoryString["userList"];
                if (userList.Count().ToString() == "0")
                {
                    if (selectOption == "today")
                    {
                        String endDate = inventoryString["dateEnd"].ToString();
                        String startDate = inventoryString["dateStart"].ToString();

                        DateTime dateEnd = Convert.ToDateTime(endDate).Date;
                        DateTime dateStart = Convert.ToDateTime(startDate).Date;

                        if (dateStart <= thisDay && thisDay <= dateEnd)
                        {
                            result.Add(inventoryString);
                        }
                    }
                    else if (selectOption == "outDate")
                    {
                        String endDate = inventoryString["dateEnd"].ToString();
                        try
                        {
                            String[] endDate2 = endDate.Split('-');
                            endDate = endDate2[2] + "/" + endDate2[1] + "/" + endDate2[0];
                        }
                        catch (Exception e) { }

                        DateTime dateEnd = Convert.ToDateTime(endDate).Date;
                        if (dateEnd < thisDay && inventoryString["status"].ToString() == "Pendiente")
                        {
                            result.Add(inventoryString);
                        }
                    }
                    else if (selectOption == "assgined") { if (inventoryString["status"].ToString() == "Pendiente") result.Add(inventoryString); }
                    else if (selectOption == "completed") { if (inventoryString["status"].ToString() == "Completado") result.Add(inventoryString); }
                    else if (selectOption == "all") { result.Add(inventoryString); }
                }
                else
                {
                    //** Espesific users
                    foreach (string users in userList)
                    {
                        if (users == userid)
                        {
                            if (selectOption == "today")
                            {
                                String endDate = inventoryString["dateEnd"].ToString();
                                String startDate = inventoryString["dateStart"].ToString();

                                DateTime dateEnd = Convert.ToDateTime(endDate).Date;
                                DateTime dateStart = Convert.ToDateTime(startDate).Date;

                                if (dateStart <= thisDay && thisDay <= dateEnd)
                                {
                                    result.Add(inventoryString);
                                }
                            }
                            else if (selectOption == "outDate")
                            {
                                String endDate = inventoryString["dateEnd"].ToString();
                                DateTime dateEnd = Convert.ToDateTime(endDate).Date;
                                if (dateEnd < thisDay && inventoryString["status"].ToString() == "Pendiente")
                                {
                                    result.Add(inventoryString);
                                }
                            }
                            else if (selectOption == "assgined") { if (inventoryString["status"].ToString() == "Pendiente") result.Add(inventoryString); }
                            else if (selectOption == "completed") { if (inventoryString["status"].ToString() == "Completado") result.Add(inventoryString); }
                            else if (selectOption == "all") { result.Add(inventoryString); }

                            break;
                        }
                    }
                }
            }

            //Get location name and creator
            foreach (JObject resu in result)
            {
                try
                {
                    String locationString = _locationTable.GetRow(resu["location"].ToString());
                    JObject location = JsonConvert.DeserializeObject<JObject>(locationString);
                    resu["locationName"] = location["name"];
                }
                catch (Exception e) { resu["locationName"] = "¡Error!"; }

                try
                {
                    String creatorString = _userTable.GetRow(resu["Creator"].ToString());
                    JObject creator = JsonConvert.DeserializeObject<JObject>(creatorString);
                    resu["creatorName"] = creator["name"] + " " + creator["lastname"];
                }
                catch (Exception e) { resu["creatorName"] = "¡Error!"; }


                String endDate = resu["dateEnd"].ToString();
                DateTime dateEnd = Convert.ToDateTime(endDate).Date;
                if (dateEnd < thisDay && resu["status"].ToString() == "Pendiente")
                {
                    resu["outDate"] = "true";
                }
                else
                {
                    resu["outDate"] = "false";
                }
            }

            return View(result);
        }

        /// <summary>
        ///     This method allows to change Inventory status to complete
        ///     and delete the Inventory directory     
        /// </summary>
        /// <param name="idInventory"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     Returns Json with the information
        /// </returns>
        public void setComplete(String idInventory)
        {
            try
            {
                String inventoryRow = _inventoryTable.GetRow(idInventory);
                JObject inventory = JsonConvert.DeserializeObject<JObject>(inventoryRow);
                inventory["status"] = "Completado";

                _inventoryTable.SaveRow(JsonConvert.SerializeObject(inventory), idInventory);
                _logTable.SaveLog(Session["_id"].ToString(), "Inventario", "Update: cambiar a completado inventario", "Inventory", DateTime.Now.ToString());
            }
            catch (Exception e) { }

            try
            {
                //**Delete complete file of the espesific inventory
                string pathDelete = @"\Uploads\Inventory\" + idInventory + "\\";
                string absolutepathDelete = Server.MapPath(pathDelete);

                System.IO.Directory.Delete(absolutepathDelete, true);
            }
            catch (Exception e) { }
        }

        /// <summary>
        ///     This method allows to get the information of the especific inventory
        /// </summary>
        /// <param name="idInventory"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        /// </returns>

        public JsonResult getInventoryRow(String idInventory)
        {
            String inventoryRow = _inventoryTable.GetRow(idInventory);


            return Json(inventoryRow);
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
        public ActionResult setSdf(String idInventory)
        {

            string relativepath = @"\Uploads\Inventory\" + idInventory + @"\upload\";
            string absolutepath = Server.MapPath(relativepath);
            string relativepathdown = @"\Uploads\Inventory\" + idInventory + @"\download\";
            string absolutepathdown = Server.MapPath(relativepathdown);
            bool isadmin = false;

            List<List<List<List<string>>>> datacomplete = new List<List<List<List<string>>>>();
            List<string> heads = new List<string>();
            string iduser = Session["_id"].ToString();
            if (!System.IO.Directory.Exists(absolutepathdown))
            {
                return PartialView("errors", "No se han Descargado los Archivos(Descargarlos e intentarlo de nuevo)");
            }
            List<string> profilesvalid = new List<string>();
            try {

              JArray  profiles = JsonConvert.DeserializeObject<JArray>(usersdb.getByProfile("Administrador de sistema"));
              profilesvalid = (from prof in profiles select (string)prof["_id"]).ToList();
              JObject userjo = JsonConvert.DeserializeObject<JObject>(usersdb.GetRow(iduser));
                isadmin = (profilesvalid.Contains(userjo["profileId"].ToString())) ? true : false;
            
            }
            catch { }
            try
            {
                //Create de upload directory
                if (!System.IO.Directory.Exists(absolutepath))
                    System.IO.Directory.CreateDirectory(absolutepath);

                if (!System.IO.Directory.Exists(absolutepath + "\\" + idInventory))
                {
                    return PartialView("errors", "Actualmente no existen documentos de inventario");

                }
                string[] files = System.IO.Directory.GetFiles(absolutepath + "\\" + idInventory + "\\", "*.sdf");


                // Get the root directory and print out some information about it.


                // Get the files in the directory and print out some information about them.
                //  System.IO.FileInfo[] fileNames = dirInfo.GetFiles("*.sdf");
                List<string> namefiles = new List<string>();
                namefiles.Add("Catalogo_Activos_Etiq.sdf");
                foreach (var s in files)
                {
                    System.IO.FileInfo fi = null;
                    try
                    {
                        fi = new System.IO.FileInfo(s);
                        namefiles.Add(fi.Name.ToString());
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        // To inform the user and continue is
                        // sufficient for this demonstration.
                        // Your application may require different behavior.
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
                List<string> nametabledbs = new List<string>();

                string conectioinventory = "";

                string conectioncatalogo = "";
                int geturlcatalogo = 0;
                int totalRows = 0;
                List<List<string>> listgetnot = new List<List<string>>();
                List<List<string>> listgetfound = new List<List<string>>();
               
                foreach (var namefile in namefiles)
                {
                    /*  SqlCeEngine engine = new SqlCeEngine(@"Data Source = " + absolutepath + "\\" + idInventory + "\\" + namefile + ";");
                     engine.CreateDatabase();
                     engine.Dispose();*/
                    string conection = "";
                    if (geturlcatalogo == 0)
                    {

                        conection = @"Data Source = " + absolutepathdown + "\\" + idInventory + "\\" + namefile + ";";
                        geturlcatalogo++;
                    }
                    else
                    {
                        conection = @"Data Source = " + absolutepath + "\\" + idInventory + "\\" + namefile + ";";

                    }
                    try
                    {
                        SqlCeEngine engine = new SqlCeEngine(conection);
                        engine.Upgrade(conection);
                    }
                    catch (Exception ex)
                    {

                    }
                    SqlCeConnection vCon = new SqlCeConnection(conection);

                    SqlCeCommand VComandoSQL = vCon.CreateCommand();
                    SqlCeDataReader rdr = null;
                    SqlCeCommand VComandoSQL2 = vCon.CreateCommand();
                    SqlCeDataReader rdr2 = null;
                    List<List<List<string>>> listtables = new List<List<List<string>>>();
                    try
                    {

                        vCon.Open();

                        VComandoSQL2.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE'";
                        rdr2 = VComandoSQL2.ExecuteReader();
                        //String nametable = "";
                        List<string> nametables = new List<string>();
                        while (rdr2.Read())
                        {
                            // nametable = rdr2.GetString(0);
                            if ((rdr2.GetString(0) != "htk_Inventariostemp") && (rdr2.GetString(0) != "htk_Inv_NP") && (rdr2.GetString(0) != "htk_Sesiones_Inventario"))
                            {

                                nametables.Add(rdr2.GetString(0));
                                nametabledbs.Add(rdr2.GetString(0));
                            }
                        }
                       
                        foreach (string nametable in nametables)
                        {

                            List<List<string>> listdata = new List<List<string>>();
                            if (nametable == "htk_Inventariostemp")
                            {

                            }
                            else
                            {
                                if (nametable == "htk_Catalogo_Activos_Etiquetado")
                                {

                                    conectioncatalogo = conection;


                                }
                                if (nametable == "htk_Inventarios")
                                {

                                    createtemporaltable(idInventory, conection);
                                    conectioinventory = conection;

                                }
                                VComandoSQL.CommandText = "SELECT * FROM " + nametable;
                                rdr = VComandoSQL.ExecuteReader();
                                /*   DataTable schemaTable = rdr.GetSchemaTable();
                                    foreach (DataRow row in schemaTable.Rows)
                                    {
                                        foreach (DataColumn column in schemaTable.Columns)
                                        {
                                           System.Windows.Forms.MessageBox.Show(String.Format("{0} = {1}",
                                               column.ColumnName, row[column]));
                                        }
                                    }*/

                                int colname = 0;
                               
                               
                                while (rdr.Read())
                                {
                                    bool found = false;
                                    List<string> column = new List<string>();
                                    List<string> heads1 = new List<string>();
                                    if (colname == 0)
                                    {
                                        for (int i = 0; i < rdr.FieldCount; i++)
                                        {
                                            heads1.Add(rdr.GetName(i));
                                           
                                        }
                                        colname++;
                                        listdata.Add(heads1);
                                        listgetfound.Add(heads1);
                                        listgetnot.Add(heads1);
                                   
                                    }


                                    for (int i = 0; i < rdr.FieldCount; i++)
                                    {
                                        column.Add(rdr[i].ToString());
                                        if (rdr.GetName(i) == "ENCONTRADO")
                                        {
                                            if (rdr[i].ToString().Trim() == "1")
                                            {
                                                found = true;
                                            }
                                        }
                                    }

                                    listdata.Add(column);
                                    if (found)
                                        listgetfound.Add(column);
                                    else
                                        listgetnot.Add(column);
                                    if (nametable == "htk_Catalogo_Activos_Etiquetado" && namefile == "Catalogo_Activos_Etiq.sdf")
                                        totalRows = listdata.Count() - 1;
                                    /* String hlp = rdr.GetString(0);
                                     String hlp2 = rdr.GetString(1);*/

                                }
                                listtables.Add(listdata);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                        vCon.Close();
                        vCon.Dispose();
                        vCon = null;
                    }

                    datacomplete.Add(listtables);
                }

                List<List<List<string>>> listtablefound = new List<List<List<string>>>();
                List<List<List<string>>> listtablenot = new List<List<List<string>>>();
                List<List<List<string>>> listtablenew = new List<List<List<string>>>();
                List<List<List<string>>> listtableloc = new List<List<List<string>>>();
                //List<List<string>> listgetnot = notExistsQuery(conectioncatalogo);
               // List<List<string>> listgetfound = ExistsQuery(conectioncatalogo);
                List<List<string>> listgetnew = newsQuery(conectioncatalogo);
                List<List<string>> listgetloc = locsQuery(conectioncatalogo);

                listtablenew.Add(listgetnew);
                listtablenot.Add(listgetnot);
                listtableloc.Add(listgetloc);
                listtablefound.Add(listgetfound);
                int numnot = listgetnot.Count() - 1;
                int numnews = listgetnew.Count() - 1;
                int numlocs = listgetloc.Count() - 1;
                int numfounds = listgetfound.Count() - 1;
                if (numnot < 0) { numnot = 0; }
                if (numnews < 0) { numnews = 0; }
                if (numlocs < 0) { numlocs = 0; }
                if (numfounds < 0) { numfounds = 0; }
                int numtotal = totalRows - numnot;

                //   numtotal = founds(conectioncatalogo);
                if (numtotal < 0) { numtotal = 0; }
                ViewData["notfound"] = numnot.ToString();
                ViewData["numnews"] = numnews.ToString();
                ViewData["numlocs"] = numlocs.ToString();
                ViewData["numtotal"] = numfounds.ToString();
                ViewData["exists"] = listtablefound;
                ViewData["notexists"] = listtablenot;
                ViewData["newact"] = listtablenew;
                ViewData["locstable"] = listtableloc;
                ViewData["heads"] = namefiles;
                ViewData["nametablsdbs"] = nametabledbs;
                ViewData["idinventory"] = idInventory;
                ViewData["valid"] = isadmin;
                return View(datacomplete);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string generatePdf(String idInventory)
        {

            string relativepath = @"\Uploads\Inventory\" + idInventory + @"\upload\";
            string absolutepath = Server.MapPath(relativepath);
            string relativepathdown = @"\Uploads\Inventory\" + idInventory + @"\download\";
            string absolutepathdown = Server.MapPath(relativepathdown);

            List<List<List<List<string>>>> datacomplete = new List<List<List<List<string>>>>();
            List<string> heads = new List<string>();
            if (!System.IO.Directory.Exists(absolutepathdown))
            {
                return "No se han Descargado los Archivos(Descargarlos e intentarlo de nuevo)";
            }
            try
            {
                //Create de upload directory
                if (!System.IO.Directory.Exists(absolutepath))
                    System.IO.Directory.CreateDirectory(absolutepath);

                if (!System.IO.Directory.Exists(absolutepath + "\\" + idInventory))
                {
                    return "No se ha Subido Ningun Archivo(Subir uno e intentarlo de nuevo)";

                }
                string[] files = System.IO.Directory.GetFiles(absolutepath + "\\" + idInventory + "\\", "*.sdf");


                // Get the root directory and print out some information about it.


                // Get the files in the directory and print out some information about them.
                //  System.IO.FileInfo[] fileNames = dirInfo.GetFiles("*.sdf");
                List<string> namefiles = new List<string>();
                namefiles.Add("Catalogo_Activos_Etiq.sdf");
                foreach (var s in files)
                {
                    System.IO.FileInfo fi = null;
                    try
                    {
                        fi = new System.IO.FileInfo(s);
                        namefiles.Add(fi.Name.ToString());
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        // To inform the user and continue is
                        // sufficient for this demonstration.
                        // Your application may require different behavior.
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
                List<string> nametabledbs = new List<string>();

                string conectioinventory = "";

                string conectioncatalogo = "";
                int geturlcatalogo = 0;
                List<List<string>> listgetfound = new List<List<string>>();
                List<List<string>> listgetnot = new List<List<string>>(); ;
                
                foreach (var namefile in namefiles)
                {
                    /*  SqlCeEngine engine = new SqlCeEngine(@"Data Source = " + absolutepath + "\\" + idInventory + "\\" + namefile + ";");
                     engine.CreateDatabase();
                     engine.Dispose();*/
                    string conection = "";
                    if (geturlcatalogo == 0)
                    {

                        conection = @"Data Source = " + absolutepathdown + "\\" + idInventory + "\\" + namefile + ";";
                        geturlcatalogo++;
                    }
                    else
                    {
                        conection = @"Data Source = " + absolutepath + "\\" + idInventory + "\\" + namefile + ";";

                    }
                    try
                    {
                        SqlCeEngine engine = new SqlCeEngine(conection);
                        engine.Upgrade(conection);
                    }
                    catch (Exception ex)
                    {

                    }
                    SqlCeConnection vCon = new SqlCeConnection(conection);

                    SqlCeCommand VComandoSQL = vCon.CreateCommand();
                    SqlCeDataReader rdr = null;
                    SqlCeCommand VComandoSQL2 = vCon.CreateCommand();
                    SqlCeDataReader rdr2 = null;
                    List<List<List<string>>> listtables = new List<List<List<string>>>();
                   
                    try
                    {

                        vCon.Open();

                        VComandoSQL2.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE'";
                        rdr2 = VComandoSQL2.ExecuteReader();
                        //String nametable = "";
                        List<string> nametables = new List<string>();
                        while (rdr2.Read())
                        {
                            // nametable = rdr2.GetString(0);
                            if (rdr2.GetString(0) != "htk_Inventariostemp")
                            {
                                nametables.Add(rdr2.GetString(0));
                                nametabledbs.Add(rdr2.GetString(0));
                            }
                        }

                        foreach (string nametable in nametables)
                        {

                            List<List<string>> listdata = new List<List<string>>();
                            if (nametable == "htk_Inventariostemp")
                            {

                            }
                            else
                            {
                                if (nametable == "htk_Catalogo_Activos_Etiquetado")
                                {

                                    conectioncatalogo = conection;


                                }
                                if (nametable == "htk_Inventarios")
                                {

                                    createtemporaltable(idInventory, conection);
                                    conectioinventory = conection;




                                }
                                VComandoSQL.CommandText = "SELECT * FROM " + nametable;
                                rdr = VComandoSQL.ExecuteReader();
                                /*   DataTable schemaTable = rdr.GetSchemaTable();
                                    foreach (DataRow row in schemaTable.Rows)
                                    {
                                        foreach (DataColumn column in schemaTable.Columns)
                                        {
                                           System.Windows.Forms.MessageBox.Show(String.Format("{0} = {1}",
                                               column.ColumnName, row[column]));
                                        }
                                    }*/
                                int colname = 0;
                                while (rdr.Read())
                                {
                                    bool found = false;
                                    List<string> column = new List<string>();
                                    List<string> heads1 = new List<string>();
                                    if (colname == 0)
                                    {
                                        for (int i = 0; i < rdr.FieldCount; i++)
                                        {
                                            heads1.Add(rdr.GetName(i));
                                        }
                                        colname++;
                                        listdata.Add(heads1);
                                        if (nametable == "htk_Inventarios")
                                        {
                                            listgetfound.Add(heads1);
                                            listgetnot.Add(heads1);
                                        }
                                    }


                                    for (int i = 0; i < rdr.FieldCount; i++)
                                    {
                                        column.Add(rdr[i].ToString());
                                        
                                            if (rdr.GetName(i) == "ENCONTRADO")
                                            {
                                                if (rdr[i].ToString().Trim() == "1")
                                                {
                                                    found = true;
                                                }
                                            }
                                       
                                    }

                                    listdata.Add(column);
                                    if (nametable == "htk_Inventarios")
                                    {
                                        if (found)
                                            listgetfound.Add(column);
                                        else
                                            listgetnot.Add(column);
                                    }
                                    /* String hlp = rdr.GetString(0);
                                     String hlp2 = rdr.GetString(1);*/

                                }
                                listtables.Add(listdata);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    finally
                    {
                        rdr.Close();
                        rdr.Dispose();
                        vCon.Close();
                        vCon.Dispose();
                        vCon = null;
                    }

                    datacomplete.Add(listtables);
                }

                List<List<List<string>>> listtablefound = new List<List<List<string>>>();
                List<List<List<string>>> listtablenot = new List<List<List<string>>>();
                List<List<List<string>>> listtablenew = new List<List<List<string>>>();
                List<List<List<string>>> listtableloc = new List<List<List<string>>>();
                //List<List<string>> listgetfound = ExistsQuery(conectioncatalogo);
               // List<List<string>> listgetnot = notExistsQuery(conectioncatalogo);
                List<List<string>> listgetnew = newsQuery(conectioncatalogo);
                List<List<string>> listgetloc = locsQuery(conectioncatalogo);

                listtablenew.Add(listgetnew);
                listtablenot.Add(listgetnot);
                listtablefound.Add(listgetfound);
                listtableloc.Add(listgetloc);

                int numnot = listgetnot.Count() - 1;
                int numnews = listgetnew.Count() - 1;
                int numlocs = listgetloc.Count() - 1;


                int numtotal = 0;

                numtotal = founds(conectioncatalogo);
                ViewData["notfound"] = numnot.ToString();
                ViewData["numnews"] = numnews.ToString();
                ViewData["numlocs"] = numlocs.ToString();
                ViewData["numtotal"] = numtotal.ToString();
                ViewData["notexists"] = listtablenot;
                ViewData["newact"] = listtablenew;
                ViewData["locstable"] = listtableloc;
                ViewData["heads"] = namefiles;
                ViewData["nametablsdbs"] = nametabledbs;
                string urlpdf = exportExcel(datacomplete, listtablenew, listtablenot, listtableloc, listtablefound, nametabledbs);
                return urlpdf;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        static void SetColumnWidth(Worksheet worksheet, uint Index, DoubleValue dwidth)
        {
            DocumentFormat.OpenXml.Spreadsheet.Columns cs = worksheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Columns>();
            if (cs != null)
            {
                IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Column> ic = cs.Elements<DocumentFormat.OpenXml.Spreadsheet.Column>().Where(r => r.Min == Index).Where(r => r.Max == Index);
                if (ic.Count() > 0)
                {
                    DocumentFormat.OpenXml.Spreadsheet.Column c = ic.First();
                    c.Width = dwidth;
                }
                else
                {
                    DocumentFormat.OpenXml.Spreadsheet.Column c = new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = Index, Max = Index, Width = dwidth, CustomWidth = true };
                    cs.Append(c);
                }
            }
            else
            {
                cs = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                DocumentFormat.OpenXml.Spreadsheet.Column c = new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = Index, Max = Index, Width = dwidth, CustomWidth = true };
                cs.Append(c);
                worksheet.InsertAfter(cs, worksheet.GetFirstChild<SheetFormatProperties>());
            }
        }

        private static double GetWidth(string font, int fontSize, string text)
        {
            System.Drawing.Font stringFont = new System.Drawing.Font(font, fontSize);
            return GetWidth(stringFont, text);
        }

        private static double GetWidth(System.Drawing.Font stringFont, string text)
        {

            /*Size textSize = TextRenderer.MeasureText(text, stringFont);
            double width = (double)(((textSize.Width / (double)7) * 256) - (128 / 7)) / 256;
            width = (double)decimal.Round((decimal)width + 0.2M, 2);
     
            return width;*/
            return 0;
        }

        private static Column CreateColumnData(UInt32 StartColumnIndex, UInt32 EndColumnIndex, double ColumnWidth)
        {
            Column column;
            column = new Column();
            column.Min = StartColumnIndex;
            column.Max = EndColumnIndex;
            column.Width = ColumnWidth;
            column.CustomWidth = true;
            return column;
        }

        // Given a Worksheet and a cell name, verifies that the specified cell exists.
        // If it does not exist, creates a new cell. 
        private static DocumentFormat.OpenXml.Spreadsheet.Cell CreateSpreadsheetCellIfNotExist(Worksheet worksheet, string cellName)
        {
            string columnName = GetColumnName(cellName);
            uint rowIndex = GetRowIndex(cellName);

            IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Row> rows = worksheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>().Where(r => r.RowIndex.Value == rowIndex);
            DocumentFormat.OpenXml.Spreadsheet.Cell cell;

            // If the Worksheet does not contain the specified row, create the specified row.
            // Create the specified cell in that row, and insert the row into the Worksheet.
            if (rows.Count() == 0)
            {
                DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = new UInt32Value(rowIndex) };
                cell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = new StringValue(cellName) };
                row.Append(cell);
                worksheet.Descendants<SheetData>().First().Append(row);
                worksheet.Save();
            }
            else
            {
                DocumentFormat.OpenXml.Spreadsheet.Row row = rows.First();

                IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Cell> cells = row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>().Where(c => c.CellReference.Value == cellName);

                // If the row does not contain the specified cell, create the specified cell.
                if (cells.Count() == 0)
                {
                    cell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = new StringValue(cellName) };
                    row.Append(cell);
                    worksheet.Save();
                }
                else
                    cell = cells.First();
            }

            return cell;
        }

        // Given a cell name, parses the specified cell to get the column name.
        private static string GetColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);

            return match.Value;
        }

        // Given a cell name, parses the specified cell to get the row index.
        private static uint GetRowIndex(string cellName)
        {
            // Create a regular expression to match the row index portion the cell name.
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(cellName);

            return uint.Parse(match.Value);
        }


        public string exportExcel(List<List<List<List<string>>>> tables, List<List<List<string>>> tablenews, List<List<List<string>>> tablenotfounds, List<List<List<string>>> tablelocs, List<List<List<string>>> tablefounds, List<string> nametables)
        {

            // Dictionary<string, string> head =(Dictionary<string,string>) Session["headuser"];


            HttpContext context = System.Web.HttpContext.Current;

            System.IO.StringWriter stringWrite = new StringWriter();
            System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);

            // StringReader reader = new StringReader(inputhtml);

            //Create PDF document
            string file = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string file1 = DateTime.Now.ToString("yyyyMMddHHmmss") + "test.xlsx";

            string pdfurl = Server.MapPath("~") + "\\Uploads\\Reports\\" + file;
            string pdfurl1 = Server.MapPath("~") + "\\Uploads\\Reports\\" + file1;

            string relativepath = "\\Uploads\\Reports\\";
            string absoluteurl = Server.MapPath(relativepath);

            if (System.IO.Directory.Exists(absoluteurl))
            {
                try
                {
                    System.IO.Directory.Delete(absoluteurl, true);
                    // System.IO.Directory.CreateDirectory(absoluteurl);
                }
                catch (Exception ex)
                {

                }
            }

            if (!System.IO.Directory.Exists(absoluteurl))
            {

                System.IO.Directory.CreateDirectory(absoluteurl);
            }


            try
            {

                var workbooktest = SpreadsheetDocument.Create(pdfurl1, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                workbooktest.Close();
            }
            catch (Exception ex)
            {
                if (!System.IO.Directory.Exists(absoluteurl))
                {

                    System.IO.Directory.CreateDirectory(absoluteurl);
                }


            }







            using (var workbook = SpreadsheetDocument.Create(pdfurl, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();


                //style
                var stylesPart = workbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet();


                // blank font list
                DocumentFormat.OpenXml.Spreadsheet.Color newColor = new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = HexBinaryValue.FromString("FFFFFF") };
                DocumentFormat.OpenXml.Spreadsheet.Bold bold = new DocumentFormat.OpenXml.Spreadsheet.Bold();

                stylesPart.Stylesheet.Fonts = new Fonts();
                stylesPart.Stylesheet.Fonts.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Font());
                stylesPart.Stylesheet.Fonts.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Font(newColor, bold));

                stylesPart.Stylesheet.Fonts.Count = 2;

                // create fills
                stylesPart.Stylesheet.Fills = new Fills();

                // create a solid red fill
                var solidRed = new PatternFill() { PatternType = PatternValues.Solid };
                solidRed.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("0072c6") }; // blue fill
                solidRed.BackgroundColor = new BackgroundColor { Indexed = 64 };

                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel
                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = solidRed });

                stylesPart.Stylesheet.Fills.Count = 3;

                // blank border list
                stylesPart.Stylesheet.Borders = new Borders();
                stylesPart.Stylesheet.Borders.Count = 1;
                stylesPart.Stylesheet.Borders.AppendChild(new Border());


                // blank cell format list
                stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats();
                stylesPart.Stylesheet.CellStyleFormats.Count = 1;
                stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());

                // cell format list
                stylesPart.Stylesheet.CellFormats = new CellFormats();
                // empty one for index 0, seems to be required
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FontId = 0 });
                // cell format references style format 0, font 0, border 0, fill 2 and applies the fill
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat { FormatId = 0, FontId = 1, BorderId = 0, FillId = 2, ApplyFill = true }).AppendChild(new Alignment { Horizontal = HorizontalAlignmentValues.Center });
                stylesPart.Stylesheet.CellFormats.Count = 2;
                TableStyles tableStyles1 = new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium9", DefaultPivotStyle = "PivotStyleLight16" };
                stylesPart.Stylesheet.Append(tableStyles1);
                stylesPart.Stylesheet.Save();


                //
                int indexnametable = 0;
                string[] nametable = nametables.ToArray();
                int colorh = 0;
                foreach (var tables1 in tables)
                {
                    colorh = 0;
                    foreach (var table in tables1)
                    {
                        var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                        var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                        sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);



                        // width columns

                        /*    
                         *   sheetPart.Worksheet = new Worksheet();
                         * string strText = "This is some really, really long text to display.";
                            double width = GetWidth("Calibri", 11, strText);

                            string strText2 = "123";
                            double width2 = GetWidth("Calibri", 11, strText2);

                            Columns columns = new Columns();
                            columns.Append(CreateColumnData(2, 2, width));
                            columns.Append(CreateColumnData(3, 3, width2));
                            sheetPart.Worksheet.Append(columns);
                               */
                        // end width columns

                        DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                        string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                        uint sheetId = 1;
                        if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                        {
                            sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                        }
                        DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = nametable[indexnametable] };
                        sheets.Append(sheet);

                        foreach (var rows in table)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                            foreach (string col in rows)
                            {
                                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                                //   cell = CreateSpreadsheetCellIfNotExist(sheetPart.Worksheet, cell.CellReference.ToString());
                                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                                cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(col); //

                                //  cell.Append(borders1);

                                if (colorh == 0)
                                {

                                    cell.StyleIndex = 1;



                                }
                                newRow.AppendChild(cell);
                            }
                            colorh++;
                            sheetData.AppendChild(newRow);


                        }
                        indexnametable++;
                        SetColumnWidth(sheetPart.Worksheet, 55, 55.0);

                    }



                }
                colorh = 0;
                foreach (var table in tablefounds)
                {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;

                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }
                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = "Activos Encontrados" };
                    sheets.Append(sheet);

                    foreach (var rows in table)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        foreach (string col in rows)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(col); //

                            //  cell.Append(borders1);
                            if (colorh == 0)
                            {
                                cell.StyleIndex = 1;

                            }

                            newRow.AppendChild(cell);
                        }
                        colorh++;
                        sheetData.AppendChild(newRow);


                    }
                    indexnametable++;
                }
                colorh = 0;
                foreach (var table in tablenotfounds)
                {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;

                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }
                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = "Activos No Encontrados" };
                    sheets.Append(sheet);

                    foreach (var rows in table)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        foreach (string col in rows)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(col); //

                            //  cell.Append(borders1);
                            if (colorh == 0)
                            {
                                cell.StyleIndex = 1;

                            }

                            newRow.AppendChild(cell);
                        }
                        colorh++;
                        sheetData.AppendChild(newRow);


                    }
                    indexnametable++;
                }
                colorh = 0;
             /*   foreach (var table in tablenews)
                {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }
                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = "Activos Nuevos" };
                    sheets.Append(sheet);

                    foreach (var rows in table)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        foreach (string col in rows)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(col); //
                            BackgroundColor backgroundColor2 = new BackgroundColor() { Indexed = (UInt32Value)64U };
                            //cell.Append(backgroundColor2);
                            //  cell.Append(borders1);
                            if (colorh == 0)
                            {
                                cell.StyleIndex = 1;

                            }

                            newRow.AppendChild(cell);
                        }
                        colorh++;
                        sheetData.AppendChild(newRow);


                    }
                    indexnametable++;
                }*/


                colorh = 0;

              /*  foreach (var table in tablelocs)
                {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }
                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = "Activos Reubicados" };
                    sheets.Append(sheet);

                    foreach (var rows in table)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        foreach (string col in rows)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(col); //

                            //  cell.Append(borders1);
                            if (colorh == 0)
                            {
                                cell.StyleIndex = 1;

                            }
                            newRow.AppendChild(cell);
                        }
                        colorh++;
                        sheetData.AppendChild(newRow);


                    }
                    indexnametable++;
                }*/



                workbook.Close();
            }

            System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Reports/" + file);


            //  Downloadpdf(file);
            return toDownload.Name;

        }

        public string expPdf(List<List<List<List<string>>>> tables, List<List<List<string>>> tablenews, List<List<List<string>>> tablenotfounds, List<List<List<string>>> tablelocs, List<string> nametables)
        {

            try
            {

                /*   byte[] toEncodeAsBytes= System.Text.ASCIIEncoding.ASCII.GetBytes(graph);
                   string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
                  // byte[] bytes = Convert.FromBase64String(returnValue);*/





                // Dictionary<string, string> head =(Dictionary<string,string>) Session["headuser"];

                HttpContext context = System.Web.HttpContext.Current;

                System.IO.StringWriter stringWrite = new StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);


                //Create PDF document
                string file = DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                string pdfurl = Server.MapPath("~") + "\\Uploads\\Reports\\" + file;
                string relativepath = "\\Uploads\\Reports\\";
                string absoluteurl = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absoluteurl))
                {
                    System.IO.Directory.Delete(absoluteurl, true);
                    // System.IO.Directory.CreateDirectory(absoluteurl);
                }

                if (!System.IO.Directory.Exists(absoluteurl))
                {

                    System.IO.Directory.CreateDirectory(absoluteurl);
                }
                Document doc = new Document(PageSize.A4);
                HTMLWorker parser = new HTMLWorker(doc);
                try
                {

                    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Reports/" + file,

                    FileMode.Create));
                }
                catch (Exception ex)
                {
                    if (!System.IO.Directory.Exists(absoluteurl))
                    {

                        System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~") + "/Uploads/Reports/" + file,

               FileMode.Create));

                }
                doc.Open();
                iTextSharp.text.Image imgpfd = iTextSharp.text.Image.GetInstance(Server.MapPath("~") + "/Content/Images/newLogoLogin.png"); //Dirreccion a la imagen que se hace referencia
                // imgpfd.SetAbsolutePosition(0,0); //Posicion en el eje carteciano de X y Y
                imgpfd.ScaleAbsolute(100, 50);//Ancho y altura de la imagen
                doc.Add(imgpfd);
                doc.Add(new Paragraph("\n"));
                Paragraph paragraph = new Paragraph();
                paragraph.Alignment = Element.ALIGN_RIGHT;
                paragraph.Font = FontFactory.GetFont("Arial", 9);
                paragraph.Add("");
                /********************************************************************************/
                var interfaceProps = new Dictionary<string, Object>();
                var TextFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Color.WHITE);
                var TextHeads = FontFactory.GetFont("Arial", 12, iTextSharp.text.Color.WHITE);
                var TextRow = FontFactory.GetFont("Arial", 10, iTextSharp.text.Color.BLACK);
                var Textdate = FontFactory.GetFont("Arial", 10, iTextSharp.text.Color.BLACK);
                //    var ih = new ImageHander() { BaseUri = Request.Url.ToString() };

                //  interfaceProps.Add(HTMLWorker.IMG_PROVIDER, ih);
                // table resumen

                /*          PdfPTable tableres = new PdfPTable(headres); // count fields heads**************

                          var TextFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Color.WHITE);
                          var TextHeads = FontFactory.GetFont("Arial", 12, iTextSharp.text.Color.WHITE);
                          var TextRow = FontFactory.GetFont("Arial", 10, iTextSharp.text.Color.BLACK);
                          var Textdate = FontFactory.GetFont("Arial", 10, iTextSharp.text.Color.BLACK);

                          int border = 0;
                          var titleResumen = new Chunk("Inventario", TextFont);


                          PdfPCell cellres = new PdfPCell(new Phrase(titleResumen));
                          cellres.BackgroundColor = new iTextSharp.text.Color(24, 116, 205);

                          cellres.Colspan = headres; /// num columns
                          cellres.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                          tableres.AddCell(cellres);

                          var headtitle = new Chunk("#" + namereport, TextHeads);
                          PdfPCell cellhres = new PdfPCell(new Phrase(headtitle));
                          cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);
                          cellhres.BorderWidthRight = 0;
                          tableres.AddCell(cellhres);
      
                          headtitle = new Chunk("Fecha Inicial", TextHeads);

                          cellhres = new PdfPCell(new Phrase(headtitle));
                          cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);

                          cellhres.BorderWidthRight = 0;
                          cellhres.BorderWidthLeft = 0;
                          tableres.AddCell(cellhres);
                          headtitle = new Chunk("Fecha Final", TextHeads);

                          cellhres = new PdfPCell(new Phrase(headtitle));
                          cellhres.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);

                          cellhres.BorderWidthLeft = 0;

                          tableres.AddCell(cellhres);

                          headtitle = new Chunk(numtotal, TextRow);
                          cellhres = new PdfPCell(new Phrase(headtitle));
                          cellhres.BorderWidthRight = 0;

                          tableres.AddCell(cellhres);
                          headtitle = new Chunk(getdates[0], TextRow);
                          cellhres = new PdfPCell(new Phrase(headtitle));

                          cellhres.BorderWidthRight = 0;
                          cellhres.BorderWidthLeft = 0;
                          tableres.AddCell(cellhres);
                          headtitle = new Chunk(getdates[1], TextRow);
                          cellhres = new PdfPCell(new Phrase(headtitle));
                          cellhres.BorderWidthLeft = 0;
                          tableres.AddCell(cellhres);


                          doc.Add(tableres);
                          doc.Add(new Paragraph("\n"));
                          /////end
                          */

                var datetime = new Chunk("Generado el : " + DateTime.Now.ToString("dd/MM/yyyy"), Textdate);

                doc.Add(new Paragraph(datetime));




                //table report
                string[] nametable = nametables.ToArray();
                int num = 0;
                List<string> head = new List<string>();
                foreach (var tables1 in tables)
                {
                    foreach (var item in tables1)
                    {
                        var titlereport = new Chunk("Reporte de " + nametable[num], TextFont);

                        foreach (var x in item)
                        {
                            foreach (string row in x)
                            {

                                head.Add(row);
                            }
                            break;
                        }

                        PdfPTable table = new PdfPTable(head.Count);

                        PdfPCell cell = new PdfPCell(new Phrase(titlereport));
                        cell.BackgroundColor = new iTextSharp.text.Color(24, 116, 205);
                        cell.Colspan = head.Count;
                        cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                        table.AddCell(cell);
                        int border = 0;
                        foreach (var th in head)
                        {
                            var headtitle = new Chunk(th, TextHeads);

                            PdfPCell cellh = new PdfPCell(new Phrase(headtitle));///////////
                            cellh.BackgroundColor = new iTextSharp.text.Color(28, 134, 238);

                            if (border == 0)
                            {
                                cellh.BorderWidthRight = 0;



                            }
                            else
                            {

                                if (border + 1 < head.Count())
                                {
                                    cellh.BorderWidthLeft = 0;
                                    cellh.BorderWidthRight = 0;
                                }
                                else
                                {
                                    cellh.BorderWidthLeft = 0;

                                }
                            }
                            table.AddCell(cellh);
                            border++;
                        }

                        int rowcolor = 0;
                        int index = 0;
                        foreach (var rows in item)
                        {
                            border = 0;
                            index = 0;
                            foreach (string row in rows)
                            {
                                var rowtitle = new Chunk(row, TextRow);


                                PdfPCell cellrow = new PdfPCell(new Phrase(rowtitle));/////////
                                if ((rowcolor % 2) == 0)
                                {
                                    cellrow.BackgroundColor = new iTextSharp.text.Color(201, 201, 201);

                                }
                                else
                                {


                                }
                                if (border == 0)
                                {
                                    cellrow.BorderWidthRight = 0;
                                    cellrow.BorderWidthTop = 0;
                                    cellrow.BorderWidthBottom = 0;

                                    border++;
                                }
                                else
                                {

                                    if (index + 1 < head.Count())
                                    {
                                        cellrow.Border = 0;
                                    }
                                    else
                                    {
                                        cellrow.BorderWidthLeft = 0;
                                        cellrow.BorderWidthTop = 0;
                                        cellrow.BorderWidthBottom = 0;
                                    }


                                }

                                if (rowcolor + 1 == rows.Count())
                                {
                                    cellrow.BorderWidthBottom = 1;
                                }
                                table.AddCell(cellrow);

                                index++;
                            }
                            rowcolor++;

                        }
                        doc.Add(new Paragraph("\n"));

                        doc.Add(table);
                        num++;
                    }
                }
                doc.Close();

                System.IO.FileInfo toDownload = new System.IO.FileInfo(Server.MapPath("~") + "/Uploads/Reports/" + file);


                //  Downloadpdf(file);
                return toDownload.Name;
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

        public int founds(String conection)
        {
            try
            {

                SqlCeConnection vCon = new SqlCeConnection(conection);
                vCon.Open();
                SqlCeCommand VComandoSQL = vCon.CreateCommand();
                SqlCeDataReader rdr = null;
                String ids = "";
                int index = 0;

                VComandoSQL.CommandText = "select count(EPC_ACTIVO) from htk_Inventariostemp Inv where Inv.EPC_ACTIVO  in (select AF_EPC_COMPLETO FROM htk_Catalogo_Activos_Etiquetado )";
                rdr = VComandoSQL.ExecuteReader();
                int total = 0;
                while (rdr.Read())
                {

                    total = rdr.GetInt32(0);

                }
                rdr.Close();
                rdr.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;

                return total;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public List<List<string>> locsQuery(String conection)
        {
            List<List<string>> listdata = new List<List<string>>();

            try
            {
                SqlCeConnection vCon = new SqlCeConnection(conection);
                vCon.Open();
                SqlCeCommand VComandoSQL = vCon.CreateCommand();
                SqlCeDataReader rdr = null;
                String ids = "";
                int index = 0;
                VComandoSQL.CommandText = "SELECT inv.EPC_ACTIVO,inv.AF_ID_ARTICULO,Ca.AF_UBICACION UBICACION_ANTERIOR,inv.AF_UBICACION UBICACION_ACTUAL FROM htk_Catalogo_Activos_Etiquetado Ca,htk_Inventariostemp inv WHERE Ca.AF_EPC_COMPLETO = inv.EPC_ACTIVO AND Ca.UB_ID_UBICACION <> inv.UB_ID_UBICACION";
                rdr = VComandoSQL.ExecuteReader();
                int colname = 0;
                while (rdr.Read())
                {
                    List<string> column = new List<string>();
                    List<string> heads1 = new List<string>();
                    if (colname == 0)
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            heads1.Add(rdr.GetName(i));
                        }
                        colname++;
                        listdata.Add(heads1);

                    }


                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        column.Add(rdr[i].ToString());
                    }

                    listdata.Add(column);
                    /* String hlp = rdr.GetString(0);
                     String hlp2 = rdr.GetString(1);*/

                }
                rdr.Close();
                rdr.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;

                return listdata;
            }
            catch (Exception ex)
            {
                return listdata;
            }
        }
        public List<List<string>> newsQuery(String conection)
        {
            List<List<string>> listdata = new List<List<string>>();

            try
            {
                SqlCeConnection vCon = new SqlCeConnection(conection);
                vCon.Open();
                SqlCeCommand VComandoSQL = vCon.CreateCommand();
                SqlCeDataReader rdr = null;
                String ids = "";
                int index = 0;

                //  VComandoSQL.CommandText = "SELECT * FROM " + nametable + " WHERE EPC_ACTIVO NOT IN (" + ids + ")";
                VComandoSQL.CommandText = "select Inv.* from htk_Inventariostemp Inv where Inv.EPC_ACTIVO not in (select AF_EPC_COMPLETO FROM htk_Catalogo_Activos_Etiquetado )";
                rdr = VComandoSQL.ExecuteReader();
                int colname = 0;
                while (rdr.Read())
                {
                    List<string> column = new List<string>();
                    List<string> heads1 = new List<string>();
                    if (colname == 0)
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            heads1.Add(rdr.GetName(i));
                        }
                        colname++;
                        listdata.Add(heads1);

                    }


                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        column.Add(rdr[i].ToString());
                    }

                    listdata.Add(column);
                    /* String hlp = rdr.GetString(0);
                     String hlp2 = rdr.GetString(1);*/

                }

                rdr.Close();
                rdr.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;

                return listdata;
            }
            catch (Exception ex)
            {
                return listdata;
            }

        }

        public List<List<string>> ExistsQuery(String conection)
        {
            List<List<string>> listdata = new List<List<string>>();

            try
            {
                SqlCeConnection vCon = new SqlCeConnection(conection);
                vCon.Open();
                SqlCeCommand VComandoSQL = vCon.CreateCommand();
                SqlCeDataReader rdr = null;
                String ids = "";
                int index = 0;

                // VComandoSQL.CommandText = "SELECT * FROM " + nametable + " WHERE AF_EPC_COMPLETO NOT IN (" + ids + ")";
               // VComandoSQL.CommandText = "SELECT Ca.* FROM htk_Catalogo_Activos_Etiquetado Ca INNER JOIN htk_Inventariostemp ON Ca.AF_EPC_COMPLETO = EPC_ACTIVO  WHERE ENCONTRADO='1' ";
                VComandoSQL.CommandText = "SELECT * FROM htk_Inventariostemp  WHERE ENCONTRADO='1' ";// 
              
                rdr = VComandoSQL.ExecuteReader();
                int colname = 0;
                while (rdr.Read())
                {
                    List<string> column = new List<string>();
                    List<string> heads1 = new List<string>();
                    if (colname == 0)
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            heads1.Add(rdr.GetName(i));
                        }
                        colname++;
                        listdata.Add(heads1);

                    }


                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        column.Add(rdr[i].ToString());
                    }

                    listdata.Add(column);
                    /* String hlp = rdr.GetString(0);
                     String hlp2 = rdr.GetString(1);*/

                }
                rdr.Close();
                rdr.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;

                return listdata;
            }
            catch (Exception ex)
            {
                return listdata;
            }
        }
        public List<List<string>> notExistsQuery(String conection)
        {
            List<List<string>> listdata = new List<List<string>>();

            try
            {
                SqlCeConnection vCon = new SqlCeConnection(conection);
                vCon.Open();
                SqlCeCommand VComandoSQL = vCon.CreateCommand();
                SqlCeDataReader rdr = null;
                String ids = "";
                int index = 0;

                // VComandoSQL.CommandText = "SELECT * FROM " + nametable + " WHERE AF_EPC_COMPLETO NOT IN (" + ids + ")";
              //  VComandoSQL.CommandText = "SELECT Ca.* FROM htk_Catalogo_Activos_Etiquetado Ca INNER JOIN htk_Inventariostemp ON Ca.AF_EPC_COMPLETO = EPC_ACTIVO  WHERE ENCONTRADO='0' ";// 
               VComandoSQL.CommandText = "SELECT * FROM htk_Inventariostemp  WHERE ENCONTRADO='0' ";// 
               
                rdr = VComandoSQL.ExecuteReader();
                int colname = 0;
               /* if (!rdr.Read())
                {
                    VComandoSQL.CommandText = "SELECT Ca.* FROM htk_Catalogo_Activos_Etiquetado Ca WHERE Ca.AF_EPC_COMPLETO NOT IN (select EPC_ACTIVO FROM htk_Inventariostemp)";
                    rdr = VComandoSQL.ExecuteReader();
                }*/
                
                while (rdr.Read())
                {
                    List<string> column = new List<string>();
                    List<string> heads1 = new List<string>();
                    if (colname == 0)
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            heads1.Add(rdr.GetName(i));
                        }
                        colname++;
                        listdata.Add(heads1);
                    }


                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        column.Add(rdr[i].ToString());
                    }

                    listdata.Add(column);
                    /* String hlp = rdr.GetString(0);
                     String hlp2 = rdr.GetString(1);*/

                }
                rdr.Close();
                rdr.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;

                return listdata;
            }
            catch (Exception ex)
            {
                return listdata;
            }
        }
        /// <summary>
        ///     This method helps to save the file ".rar" or ".zip"
        /// </summary>
        /// <param name="idInventory"></param>
        /// <param name="file"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        [HttpPost]
        public void saveFiles(String idInventory, IEnumerable<HttpPostedFileBase> files)
        {
            string relativepath = @"\Uploads\Inventory\" + idInventory + @"\upload\";
            string absolutepath = Server.MapPath(relativepath);

            if (System.IO.Directory.Exists(absolutepath + "\\" + idInventory + "\\"))
            {
                System.IO.Directory.Delete(absolutepath + "\\" + idInventory + "\\", true);
            }
            //Create de upload directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);


            string ext = null;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i]; //Uploaded file
                string fileName = file.FileName;
                ext = file.FileName.Split('.').Last(); //getting the extension   

                if (ext == "rar" || ext == "zip")
                {
                    try
                    {
                        //Delete de ".rar" or ".zip" file
                        if (System.IO.File.Exists(absolutepath + "\\" + idInventory + "." + ext))
                            System.IO.File.Delete(absolutepath + "\\" + idInventory + "." + ext);

                        //Saves de compress file
                        file.SaveAs(absolutepath + "\\" + idInventory + "." + ext);

                        //If the compress file exist.. Delete and then extract
                        if (System.IO.Directory.Exists(absolutepath + "\\" + idInventory + "\\"))
                            System.IO.Directory.Delete(absolutepath + "\\" + idInventory + "\\", true);
                        ZipFile.ExtractToDirectory(absolutepath + "\\" + idInventory + "." + ext, absolutepath + "\\" + idInventory + "\\");
                    }
                    catch (Exception e) { }
                }
                else
                {
                    //Create directory for the idInventory
                    if (!System.IO.Directory.Exists(absolutepath + "\\" + idInventory + "\\"))
                        System.IO.Directory.CreateDirectory(absolutepath + "\\" + idInventory + "\\");


                    //Save each ".sdf" file
                    file.SaveAs(absolutepath + "\\" + idInventory + "\\" + fileName);
                    break;
                }
            }

            //////**** Work with  the information of Location
            /*  SqlCeConnection vCon = new SqlCeConnection(@"Data Source = " + absolutepath + "\\" + idInventory + "\\Ubicaciones.sdf" + ";");
              SqlCeCommand VComandoSQL = vCon.CreateCommand();
              SqlCeCommand VComandoSQL2 = vCon.CreateCommand();
              SqlCeDataReader rdr = null;
              SqlCeDataReader rdr2 = null;
              try
            {
                  vCon.Open();
                  
                   
                  VComandoSQL.CommandText = "SELECT * FROM htk_Ubicaciones";  
                  rdr = VComandoSQL.ExecuteReader();

                  while (rdr.Read())
                  {
                      String hlp = rdr.GetString(0);
                      String hlp2 = rdr.GetString(1);
                  }
              }
              catch (Exception e)
              {
                  throw e;
              }
              finally
              {
                  rdr.Close();
                  rdr.Dispose();
                  vCon.Close();
                  vCon.Dispose();
                  vCon = null;
              }
           
           */

            //** Record Upload
            String inventoryString = _inventoryTable.GetRow(idInventory);
            JObject inventory = JsonConvert.DeserializeObject<JObject>(inventoryString);

            inventory["Upload"] = "true";
            _inventoryTable.SaveRow(JsonConvert.SerializeObject(inventory), idInventory);
            _logTable.SaveLog(Session["_id"].ToString(), "Inventario", "Insert: guardar archivos", "Inventory", DateTime.Now.ToString());
        }

        /// <summary>
        ///     This method helps to determine the type of SQL Server CE to create
        /// </summary>
        /// <param name="idInventory"></param>
        /// <param name="dataInfo"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>

        public JsonResult SubCrearDB(String idInventory, String dataInfo)
        {

            //**Delete complet file of the espesific inventory
            string pathDelete = @"\Uploads\Inventory\" + idInventory + "\\";
            string absolutepathDelete = Server.MapPath(pathDelete);

            try
            {
                if (System.IO.Directory.Exists(absolutepathDelete))
                    System.IO.Directory.Delete(absolutepathDelete, true);
            }
            catch (Exception e) { }

            string filesName = "";
            JObject datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);

            if (datainfoString["location"].ToString() == "true")
            {
                createLocationDB(idInventory, dataInfo);
                createConjuntoDB(idInventory,dataInfo);
                createSubLocationDB(idInventory, dataInfo);
                filesName += "Ubicaciones.sdf,Conjuntos.sdf,SubUbicaciones.sdf";
            }

            if (datainfoString["activeReference"].ToString() == "true")
            {
                createReference(idInventory, dataInfo);
                filesName += "Referencia.sdf,";
            }

            if (datainfoString["inventory"].ToString() == "true")
            {
                // Create the real inventory in the system
                // createInventoryReference(idInventory, dataInfo);

                createInventory(idInventory, dataInfo);
                filesName += "Inventarios.sdf,";
            }

            if (datainfoString["activeInventory"].ToString() == "true")
            {
                createActiveInventory(idInventory, dataInfo);
                filesName += "Catalogo_Activos_Etiq.sdf,";
            }

            if (filesName == "") { return null; }

            //TODO :: Condicionar 
            createUserDB(idInventory);
            
            filesName += "Usuarios.sdf";


            //**Compresss to .rar or .zip && Download
            String ext = "";
            if (datainfoString["rarOption"].ToString() == "true") ext = ".rar"; else ext = ".zip";

            string path = Server.MapPath(@"\Uploads\Inventory\" + idInventory + @"\download\" + idInventory + ext); //get physical file path from server
            string startPath = Server.MapPath(@"\Uploads\Inventory\" + idInventory + @"\download\" + idInventory);
            string name = Path.GetFileName(path); //get file name
            string zipPath = path;
            FileInfo file = new FileInfo(path);

            if (System.IO.File.Exists(zipPath))
                System.IO.File.Delete(zipPath);

            ZipFile.CreateFromDirectory(startPath, zipPath);

            String result = "{'type':'compress','url':'/Uploads/Inventory/" + idInventory + "/download/','data':'" + name + "'}";
            JObject resultObj = JsonConvert.DeserializeObject<JObject>(result);

            return Json(JsonConvert.SerializeObject(resultObj));

        }

        /// <summary>
        ///     This method allows to create a file .sdf 
        ///     for Active Inventory
        ///     SQL SERVER CE Version 4.0
        /// </summary>
        /// <param name="idInventory"></param>
        /// <param name="dataInfo"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        public void createActiveInventory(String idInventory, String dataInfo)
        {
            //check if the path exist
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\" + idInventory + "\\";
            String absolutepath = Server.MapPath(relativepath);

            JObject datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);

            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "Catalogo_Activos_Etiq.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;

            /* SqlCeResultSet rs = null;
             SqlCeUpdatableRecord rec = null;
             SqlCeEngine DBDatabase = new SqlCeEngine(@"Data Source = " + curFile + ";");
             SqlCeConnection vCon = new SqlCeConnection(@"Data Source = " + curFile + ";");
             SqlCeCommand VComandoSQL = vCon.CreateCommand();*/

            Dictionary<string, string> conjuntos = new Dictionary<string, string>();
            Dictionary<string, JObject> conjuntosdict = new Dictionary<string, JObject>();
            Dictionary<string, JObject> ubicacionesdict = new Dictionary<string, JObject>();
            Dictionary<string, string> uexplotacion = new Dictionary<string, string>();
            string getconjunt =  locationsProfilesdb.Get("name", "Conjunto");
            string getubic = locationsProfilesdb.Get("name", "Ubicacion");
            JArray conjuntja = new JArray();
            JArray ubija = new JArray();
            string idprof = "";
            string idprof2 = "";
            try
            {
                conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
            }
            catch (Exception ex) { }
            try
            {
                ubija = JsonConvert.DeserializeObject<JArray>(getubic);
                idprof2 = (from mov in ubija select (string)mov["_id"]).First().ToString();
            }
            catch (Exception ex) { }
            String rowArray = _locationTable.Get("profileId", idprof);
            JArray locatList = JsonConvert.DeserializeObject<JArray>(rowArray);
            conjuntos=locatList.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
            conjuntosdict = locatList.ToDictionary(x => (string)x["_id"], x => (JObject)x);
            uexplotacion = locatList.ToDictionary(x => (string)x["_id"], x => (string)x["number"]);
            String rowArray2 = _locationTable.Get("profileId", idprof2);
            JArray locatList2 = JsonConvert.DeserializeObject<JArray>(rowArray2);
            ubicacionesdict= locatList2.ToDictionary(x => (string)x["_id"], x => (JObject)x);
            try
            {
                //*** Eliminated if there is already a DB  ***//
                /* if (System.IO.File.Exists(curFile))
                     System.IO.File.Delete(curFile);*/

                //*** Create a DB
                /* DBDatabase.CreateDatabase();
                 vCon.Open();

                 //*** Insert table 
                 VComandoSQL.CommandText = @"CREATE TABLE [htk_Catalogo_Activos_Etiquetado] (
                                 ID_REGISTRO INT NULL,
                                 AF_UNIDAD_EXPLOTACION NVARCHAR(15) NULL,
                                 AF_NOMBRE_CONJUNTO NVARCHAR(50) NULL,
                                 AF_DEPARTAMENTO NVARCHAR(50) NULL,
                                 AF_UBICACION NVARCHAR(50) NULL,
                                 AF_ID_ARTICULO NVARCHAR(15) NULL,
                                 AF_DESC_ARTICULO NVARCHAR(100) NULL,
                                 AF_MARCA NVARCHAR(30) NULL,
                                 AF_MODELO NVARCHAR(30) NULL,
                                 AF_NUM_SERIE NVARCHAR(30) NULL,
                                 AF_EPC_COMPLETO NVARCHAR(30) PRIMARY KEY NOT NULL,
                                 AF_STATUS_ETIQUETADO NVARCHAR(50) NULL,
                                 AF_FECHA_ETIQUETADO NVARCHAR(20) NULL,
                                 AF_USUARIO_ETIQUETADO NVARCHAR(50) NULL,
                                 AF_CANTIDAD INT NULL,
                                 AF_ID_PAQUETE BIT NULL)";
                 VComandoSQL.ExecuteNonQuery();*/

                String path = rootPath + relativepath;
                string path1 = path.Replace("\\", "\\\\");

                string query = @"CREATE TABLE [htk_Catalogo_Activos_Etiquetado] (
	                            ID_REGISTRO NVARCHAR(25) NULL,
	                            AF_UNIDAD_EXPLOTACION NVARCHAR(15) NULL,
	                            AF_NOMBRE_CONJUNTO NVARCHAR(80) NULL,
	                            AF_DEPARTAMENTO NVARCHAR(80) NULL,
	                            AF_UBICACION NVARCHAR(80) NULL,
	                            AF_ID_ARTICULO NVARCHAR(24) NULL,
	                            AF_DESC_ARTICULO NVARCHAR(100) NULL,
	                            AF_MARCA NVARCHAR(80) NULL,
	                            AF_MODELO NVARCHAR(80) NULL,
	                            AF_NUM_SERIE NVARCHAR(30) NULL,
	                            AF_EPC_COMPLETO NVARCHAR(30) NULL,
	                            AF_STATUS_ETIQUETADO NVARCHAR(50) NULL,
	                            AF_FECHA_ETIQUETADO NVARCHAR(20) NULL,
	                            AF_USUARIO_ETIQUETADO NVARCHAR(50) NULL,
	                            AF_CANTIDAD INT NULL,
	                            AF_ID_PAQUETE BIT NULL,
                                UB_ID_UBICACION NVARCHAR(30) NULL,
                                AF_SUBUBICACION NVARCHAR(80) NULL,
                                UB_ID_SUBUBICACION NVARCHAR(30) NULL,
                                ID_USUARIO NVARCHAR(30) NULL)";

                string fields = "'_id','objectReference','objectReference_name','EPC','serie','location','location_name','marca','modelo','Creator','number','conjuntoName','department','label','date','quantity','sublocationname','sublocation'";
               
                string values = "'0','5','6','10','9','16','4','7','8','19','1','2','3','11','12','14','17','18'";
                String inventoryString = _inventoryTable.GetRow(idInventory);
                JObject inventoryRow = new JObject();
                try
                {
                    inventoryRow = JsonConvert.DeserializeObject<JObject>(inventoryString);
                }
                catch (Exception ex) { }

                String idLocation = inventoryRow["location"].ToString();
                JArray listLocation = new JArray();
                if (idLocation == "") idLocation = "null";
                if (idLocation == "null")
                {
                    string locationrows = _locationTable.GetRows();
                    listLocation = JsonConvert.DeserializeObject<JArray>(locationrows);
                }
                else
                {
                    listLocation = getSubLocation(idLocation);
                }

                List<string> idslocation = new List<string>();
                if (listLocation == null)
                {
                    listLocation = new JArray();
                }
                foreach (JObject item in listLocation)
                {
                    try
                    {
                        idslocation.Add(item["_id"].ToString());
                    }
                    catch (Exception ex) { continue; }
                }
                idslocation.Add(idLocation);
                String objectsRef = _objectTable.GetObjectsRefTable(idslocation);
                JArray objectsreal = JsonConvert.DeserializeObject<JArray>(objectsRef);
                JArray objectsall = new JArray();
                JArray profloc = JsonConvert.DeserializeObject<JArray>(locationsProfilesdb.GetRows());
                Dictionary<string, string> profdict = profloc.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                foreach (JObject row in objectsreal)
                {
                    try {

                        JToken jtoken;
                        row.Add("sublocation", "");
                        row.Add("sublocationname", "");
                        try
                        {
                            string thisprof = profdict[row["profileId"].ToString()];
                            if (thisprof.Contains("Sub-Ubicaciones"))
                            {
                                row["sublocation"] = row["location"].ToString();
                                row["sublocationname"] = row["location_name"].ToString();
                                try
                                {
                                    JObject loc = ubicacionesdict[row["conjunto"].ToString()];
                                    row["location"] = loc["_id"].ToString();
                                    row["location_name"] = loc["name"].ToString();
                                    row["conjunto"] = loc["parent"].ToString();
                                    row["number"] = loc["number"].ToString();
                                }
                                catch { }
                            }

                        }
                        catch { }
                        if (!row.TryGetValue("marca", out jtoken))
                        {
                            row.Add("marca", "");
                        }
                        if (!row.TryGetValue("modelo", out jtoken))
                        {
                            row.Add("modelo", "");
                        }
                        if (!row.TryGetValue("EPC", out jtoken))
                        {
                            row.Add("EPC", "");
                        }
                        if (!row.TryGetValue("serie", out jtoken))
                        {
                            row.Add("serie", "");
                        }
                        if (!row.TryGetValue("Creator", out jtoken))
                        {
                            row.Add("Creator", "");
                        }
                        if (!row.TryGetValue("number", out jtoken))
                        {
                            row.Add("number", "");
                        }
                        if (row.TryGetValue("conjunto", out jtoken))
                        {
                            try {
                                row.Add("conjuntoName", conjuntos[row["conjunto"].ToString()]);
                                if (row.TryGetValue("number", out jtoken))
                                {
                                    row["number"] = uexplotacion[row["conjunto"].ToString()];
                                }
                            }
                            catch
                            {

                            }

                        }
                        else { row.Add("conjuntoName", ""); }
                        if (!row.TryGetValue("conjuntoName", out jtoken))
                        {
                            row.Add("conjuntoName", "");
                        }
                        if (!row.TryGetValue("department", out jtoken))
                        {
                            row.Add("department", "");
                        }
                        if (!row.TryGetValue("label", out jtoken))
                        {
                            row.Add("label", "");
                        }
                        if (!row.TryGetValue("date", out jtoken))
                        {
                            row.Add("date", "");
                        }
                        if (!row.TryGetValue("quantity", out jtoken))
                        {
                            row.Add("quantity", "");
                        }
                        objectsall.Add(row);
                    }
                    catch
                    {

                    }
                }
                String datainfor = JsonConvert.SerializeObject(objectsall);
                datainfor = datainfor.Replace("\r", "").Replace("\t", "").Replace("\n", "");
                string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                string exe = Server.MapPath(pathexe);

                String JsonData = "{\"path\":\"" + path1 + "\",\"namefile\":\"" + fileName + "\",\"tables\":[{\"nametable\":\"htk_Catalogo_Activos_Etiquetado\",\"query\":\"" + query + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]}]}";

                // JsonData = JsonConvert.SerializeObject(JsonData);
                JsonData = JsonData.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                string urlfiletxt = path + "Catalogo.txt";
                System.IO.File.WriteAllText(urlfiletxt, JsonData);
                String Jsonfile = "{'url':'" + urlfiletxt + "'}";
                Jsonfile = JsonConvert.SerializeObject(Jsonfile);
                ProcessStartInfo procceesstar = new ProcessStartInfo();
                procceesstar.FileName = exe;
                procceesstar.Arguments = Jsonfile;

                Process procces = new Process();
                procces.StartInfo = procceesstar;
                procces.Start();
                procces.WaitForExit();

                System.IO.File.Delete(urlfiletxt);
                //.........................................
                //VComandoSQL.CommandText = "htk_Activos_Referencia";
                //VComandoSQL.CommandType = CommandType.TableDirect;

                //rs = VComandoSQL.ExecuteResultSet(ResultSetOptions.Updatable);
                //rec = rs.CreateRecord();

                //String objectString = _objectTable.GetRows(idInventory);
                //JArray objects = JsonConvert.DeserializeObject<JArray>(objectString);

                //foreach (JObject objec in objects)
                //{
                //    rec.SetValue(0, objec["_id"].ToString());
                //    rec.SetValue(3, objec["name"].ToString());
                //    rs.Insert(rec);
                //}
            }
            catch (Exception VError)
            {
                throw VError;
            }
            finally
            {
                //rs.Close();
                //rs.Dispose();
                /* VComandoSQL.Dispose();
                 vCon.Close();
                 vCon.Dispose();
                 vCon = null;
                 DBDatabase.Dispose();*/
            }
        }


        public void createtemporaltable(string idInventory, string conection)
        {
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\" + idInventory + "\\";
            String absolutepath = Server.MapPath(relativepath);
            String fileName = "Catalogo_Activos_Etiq.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;
            SqlCeConnection vCon = new SqlCeConnection(@"Data Source = " + curFile + ";");
            SqlCeCommand VComandoSQL = vCon.CreateCommand();
            SqlCeResultSet rs = null;
            SqlCeUpdatableRecord rec = null;
            SqlCeConnection vCon2 = new SqlCeConnection(conection);
            SqlCeCommand VComandoSQL2 = vCon2.CreateCommand();
            try
            {
                //*** Eliminated if there is already a DB  ***//
                /*  if (System.IO.File.Exists(curFile))
                      System.IO.File.Delete(curFile);*/

                //*** Create a DB

                vCon.Open();

                //*** Insert table 
                try
                {
                    VComandoSQL.CommandText = "DROP TABLE htk_Inventariostemp";
                    VComandoSQL.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                }
                VComandoSQL.CommandText = @"CREATE TABLE [htk_Inventariostemp] (
	                    ID_SESION_INVENTARIO NVARCHAR(30)  NOT NULL,
	                    EPC_ACTIVO NVARCHAR(30) PRIMARY KEY NOT NULL,
	                    FECHA_REGISTRO DATETIME NULL,
	                    USUARIO_REGISTRO NVARCHAR(50) NULL,
	                    ENCONTRADO NVARCHAR(1) NULL,
	                    AF_CONJUNTO NVARCHAR(50) NULL,
	                    AF_UBICACION NVARCHAR(50) NULL,
	                    AF_DESC_ARTICULO NVARCHAR(100) NULL,
	                    AF_MARCA NVARCHAR(30) NULL,
	                    AF_MODELO NVARCHAR(30) NULL,
	                    AF_NUM_SERIE NVARCHAR(30) NULL,
	                    AF_CANTIDAD FLOAT NULL,
	                    AF_ID_ARTICULO NVARCHAR(24) NULL,
	                    UB_ID_UBICACION NVARCHAR(24) NULL)";
                VComandoSQL.ExecuteNonQuery();





                VComandoSQL.CommandText = "htk_Inventariostemp";
                VComandoSQL.CommandType = CommandType.TableDirect;

                rs = VComandoSQL.ExecuteResultSet(ResultSetOptions.Updatable);
                rec = rs.CreateRecord();


                // insert table inventory
                SqlCeDataReader rdr = null;
                vCon2.Open();
                VComandoSQL2.CommandText = "SELECT * FROM htk_Inventarios";
                rdr = VComandoSQL2.ExecuteReader();


                while (rdr.Read())
                {

                    try
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            rec.SetValue(i, rdr[i]);
                        }

                        rs.Insert(rec);
                    }
                    catch (Exception ex) { continue; }
                }



            }
            catch (Exception VError)
            {
                throw VError;
            }
            finally
            {
                rs.Close();
                rs.Dispose();
                VComandoSQL2.Dispose();
                VComandoSQL.Dispose();
                vCon.Close();
                vCon2.Close();
                vCon.Dispose();
                vCon2.Dispose();
                vCon = null;
                vCon2 = null;

            }
        }
        /// <summary>
        ///     This method allows to create a file .sdf 
        ///     for Inventory
        ///     SQL SERVER CE Version 4.0
        /// </summary>
        /// <param name="idInventory"></param>
        /// <param name="dataInfo"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        public void createInventory(String idInventory, String dataInfo)
        {
            //check if the path exist
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\" + idInventory + "\\";
            String absolutepath = Server.MapPath(relativepath);

            JObject datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);

            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "Inventarios.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;

            /*   SqlCeResultSet rs = null;
               SqlCeUpdatableRecord rec = null; ;
               SqlCeEngine DBDatabase = new SqlCeEngine(@"Data Source = " + curFile + ";");
               SqlCeConnection vCon = new SqlCeConnection(@"Data Source = " + curFile + ";");
               SqlCeCommand VComandoSQL = vCon.CreateCommand();*/

            try
            {
                //*** Eliminated if there is already a DB  ***//
                /* if (System.IO.File.Exists(curFile))
                     System.IO.File.Delete(curFile);*/

                //*** Create a DB
                /*   DBDatabase.CreateDatabase();
                   vCon.Open();

                   //*** Insert table 
                   VComandoSQL.CommandText = @"CREATE TABLE [htk_Inv_NP] 
                           (ID_SESION_INVENTARIO NVARCHAR(30) NULL,
                            EPC_ACTIVO NVARCHAR(30) NULL,
                            FECHA_REGISTRO DATETIME NULL,
                            USUARIO_REGISTRO NVARCHAR(50) NULL,
                            AF_CONJUNTO NVARCHAR(50) NULL,
                            AF_UBICACION NVARCHAR(50) NULL,
                            AF_DESC_ARTICULO NVARCHAR(100) NULL,
                            AF_MARCA NVARCHAR(30) NULL,
                            AF_MODELO NVARCHAR(30) NULL,
                            AF_NUM_SERIE NVARCHAR(30) NULL,
                            AF_CANTIDAD FLOAT NULL,
                            AF_ID_ARTICULO NVARCHAR(15) NULL)";
                   VComandoSQL.ExecuteNonQuery();

                   VComandoSQL.CommandText = @"CREATE TABLE [htk_Inventarios] (
                           ID_SESION_INVENTARIO NVARCHAR(30) PRIMARY KEY NOT NULL,
                           EPC_ACTIVO NVARCHAR(30) NOT NULL,
                           FECHA_REGISTRO DATETIME NULL,
                           USUARIO_REGISTRO NVARCHAR(50) NULL,
                           ENCONTRADO NVARCHAR(1) NULL,
                           AF_CONJUNTO NVARCHAR(50) NULL,
                           AF_UBICACION NVARCHAR(50) NULL,
                           AF_DESC_ARTICULO NVARCHAR(100) NULL,
                           AF_MARCA NVARCHAR(30) NULL,
                           AF_MODELO NVARCHAR(30) NULL,
                           AF_NUM_SERIE NVARCHAR(30) NULL,
                           AF_CANTIDAD FLOAT NULL,
                           AF_ID_ARTICULO NVARCHAR(15) NULL)";
                   VComandoSQL.ExecuteNonQuery();

                   VComandoSQL.CommandText = @"CREATE TABLE [htk_Sesiones_Inventario] (
                           ID_SESION NVARCHAR(30) PRIMARY KEY NOT NULL,
                           DESCRIPCION NVARCHAR(100) NULL,
                           CATEGORIA_INVENTARIO NVARCHAR(30) NULL,
                           USUARIO_CREACION NVARCHAR(50) NULL,
                           TIPO_INVENTARIO NVARCHAR(15) NULL,
                           UNIDAD_EXPLOTACION NVARCHAR(15) NULL,
                           NOMBRE_CONJUNTO NVARCHAR(50) NULL,
                           ID_DEPARTAMENTO NVARCHAR(20) NULL,
                           NOMBRE_DEPARTAMENTO NVARCHAR(50) NULL,
                           ID_UBICACION NTEXT NULL,
                           NOMBRE_UBICACION NTEXT NULL,
                           HH_INVOLUCRADOS NVARCHAR(255) NULL,
                           FECHA_INICIO DATETIME NULL,
                           FECHA_FINALIZACION DATETIME NULL,
                           FECHA_APERTURA DATETIME NULL,
                           FECHA_CIERRE DATETIME NULL,
                           STATUS NVARCHAR(1) NULL)";
                   VComandoSQL.ExecuteNonQuery();
                   */

                //.........................................
                //VComandoSQL.CommandText = "htk_Activos_Referencia";
                //VComandoSQL.CommandType = CommandType.TableDirect;

                //rs = VComandoSQL.ExecuteResultSet(ResultSetOptions.Updatable);
                //rec = rs.CreateRecord();

                //String objectString = _objectTable.GetRows(idInventory);
                //JArray objects = JsonConvert.DeserializeObject<JArray>(objectString);

                //foreach (JObject objec in objects)
                //{
                //    rec.SetValue(0, objec["_id"].ToString());
                //    rec.SetValue(3, objec["name"].ToString());
                //    rs.Insert(rec);
                //}

                String path = rootPath + relativepath;
                string path1 = path.Replace("\\", "\\\\");

                string query = @"CREATE TABLE [htk_Inventarios] (
	                    ID_SESION_INVENTARIO NVARCHAR(30),
	                    EPC_ACTIVO NVARCHAR(30) NOT NULL,
	                    FECHA_REGISTRO DATETIME NULL,
	                    USUARIO_REGISTRO NVARCHAR(50) NULL,
	                    ENCONTRADO NVARCHAR(1) NULL,
	                    AF_CONJUNTO NVARCHAR(50) NULL,
	                    AF_UBICACION NVARCHAR(50) NULL,
	                    AF_DESC_ARTICULO NVARCHAR(100) NULL,
	                    AF_MARCA NVARCHAR(30) NULL,
	                    AF_MODELO NVARCHAR(30) NULL,
	                    AF_NUM_SERIE NVARCHAR(30) NULL,
	                    AF_CANTIDAD FLOAT NULL,
	                    AF_ID_ARTICULO NVARCHAR(24) NULL,
	                    UB_ID_UBICACION NVARCHAR(24) NULL)";

                string query2 = @"CREATE TABLE [htk_Inv_NP] 
                        (ID_SESION_INVENTARIO NVARCHAR(30) NULL,
	                     EPC_ACTIVO NVARCHAR(30) NULL,
	                     FECHA_REGISTRO DATETIME NULL,
	                     USUARIO_REGISTRO NVARCHAR(50) NULL,
	                     AF_CONJUNTO NVARCHAR(50) NULL,
	                     AF_UBICACION NVARCHAR(50) NULL,
	                     AF_DESC_ARTICULO NVARCHAR(100) NULL,
	                     AF_MARCA NVARCHAR(30) NULL,
	                     AF_MODELO NVARCHAR(30) NULL,
	                     AF_NUM_SERIE NVARCHAR(30) NULL,
	                     AF_CANTIDAD FLOAT NULL,
	                     AF_ID_ARTICULO NVARCHAR(24) NULL)";

                string query3 = @"CREATE TABLE [htk_Sesiones_Inventario] (
	                    ID_SESION NVARCHAR(30) PRIMARY KEY NOT NULL,
	                    DESCRIPCION NVARCHAR(100) NULL,
	                    CATEGORIA_INVENTARIO NVARCHAR(30) NULL,
	                    USUARIO_CREACION NVARCHAR(50) NULL,
	                    TIPO_INVENTARIO NVARCHAR(15) NULL,
	                    UNIDAD_EXPLOTACION NVARCHAR(15) NULL,
	                    NOMBRE_CONJUNTO NVARCHAR(50) NULL,
	                    ID_DEPARTAMENTO NVARCHAR(24) NULL,
	                    NOMBRE_DEPARTAMENTO NVARCHAR(50) NULL,
	                    ID_UBICACION NTEXT NULL,
	                    NOMBRE_UBICACION NTEXT NULL,
	                    HH_INVOLUCRADOS NVARCHAR(255) NULL,
	                    FECHA_INICIO DATETIME NULL,
	                    FECHA_FINALIZACION DATETIME NULL,
	                    FECHA_APERTURA DATETIME NULL,
	                    FECHA_CIERRE DATETIME NULL,
	                    STATUS NVARCHAR(1) NULL)";

                string fields = "";
                string values = "";

                JArray datainfor = new JArray();
                string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                string exe = Server.MapPath(pathexe);
                String JsonData = "{\"path\":\"" + path1 + "\",\"namefile\":\"" + fileName + "\",\"tables\":[{\"nametable\":\"htk_Inventarios\",\"query\":\"" + query + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]},{\"nametable\":\"htk_Inv_NP\",\"query\":\"" + query2 + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]},{\"nametable\":\"htk_Sesiones_Inventario\",\"query\":\"" + query3 + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]}]}";
                // JsonData = JsonConvert.SerializeObject(JsonData);
                JsonData = JsonData.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                string urlfiletxt = path + "Inventory.txt";
                System.IO.File.WriteAllText(urlfiletxt, JsonData);
                String Jsonfile = "{'url':'" + urlfiletxt + "'}";
                Jsonfile = JsonConvert.SerializeObject(Jsonfile);


                ProcessStartInfo procceesstar = new ProcessStartInfo();
                procceesstar.FileName = exe;
                procceesstar.Arguments = Jsonfile;

                Process procces = new Process();
                procces.StartInfo = procceesstar;
                procces.Start();
                procces.WaitForExit();
                System.IO.File.Delete(urlfiletxt);
            }
            catch (Exception VError)
            {
                throw VError;
            }
            finally
            {
                //rs.Close();
                //rs.Dispose();
                /*   VComandoSQL.Dispose();
                   vCon.Close();
                   vCon.Dispose();
                   vCon = null;
                   DBDatabase.Dispose();*/
            }
        }

        /// <summary>
        ///     This method allows to create a file .sdf 
        ///     for Inventory
        ///     SQL SERVER CE Version 4.0
        /// </summary>
        /// <param name="idInventory"></param>
        /// <param name="dataInfo"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        public void createInventoryReference(String idInventory, String dataInfo)
        {
            //check if the path exist
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\";
            String absolutepath = Server.MapPath(relativepath);

            JObject datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);

            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "InventariosReferencia.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;

            SqlCeResultSet rs = null;
            SqlCeUpdatableRecord rec = null; ;
            SqlCeEngine DBDatabase = new SqlCeEngine(@"Data Source = " + curFile + ";");
            SqlCeConnection vCon = new SqlCeConnection(@"Data Source = " + curFile + ";");
            SqlCeCommand VComandoSQL = vCon.CreateCommand();

            try
            {
                //*** Eliminated if there is already a DB  ***//
                /* if (System.IO.File.Exists(curFile))
                     System.IO.File.Delete(curFile);*/

                //*** Create a DB
                DBDatabase.CreateDatabase();
                vCon.Open();

                //*** Insert table 
                VComandoSQL.CommandText = @"CREATE TABLE [htk_Inv_NP] 
                           (ID_SESION_INVENTARIO NVARCHAR(30) NULL,
                            EPC_ACTIVO NVARCHAR(30) NULL,
                            FECHA_REGISTRO DATETIME NULL,
                            USUARIO_REGISTRO NVARCHAR(50) NULL,
                            AF_CONJUNTO NVARCHAR(50) NULL,
                            AF_UBICACION NVARCHAR(50) NULL,
                            AF_DESC_ARTICULO NVARCHAR(100) NULL,
                            AF_MARCA NVARCHAR(30) NULL,
                            AF_MODELO NVARCHAR(30) NULL,
                            AF_NUM_SERIE NVARCHAR(30) NULL,
                            AF_CANTIDAD FLOAT NULL,
                            AF_ID_ARTICULO NVARCHAR(24) NULL)";
                VComandoSQL.ExecuteNonQuery();

                VComandoSQL.CommandText = @"CREATE TABLE [htk_Inventarios] (
                           ID_SESION_INVENTARIO NVARCHAR(30) PRIMARY KEY NOT NULL,
                           EPC_ACTIVO NVARCHAR(30) NOT NULL,
                           FECHA_REGISTRO DATETIME NULL,
                           USUARIO_REGISTRO NVARCHAR(50) NULL,
                           ENCONTRADO NVARCHAR(1) NULL,
                           AF_CONJUNTO NVARCHAR(50) NULL,
                           AF_UBICACION NVARCHAR(50) NULL,
                           AF_DESC_ARTICULO NVARCHAR(100) NULL,
                           AF_MARCA NVARCHAR(30) NULL,
                           AF_MODELO NVARCHAR(30) NULL,
                           AF_NUM_SERIE NVARCHAR(30) NULL,
                           AF_CANTIDAD FLOAT NULL,
                           AF_ID_ARTICULO NVARCHAR(24) NULL,
                           UB_ID_UBICACION NVARCHAR(24) NULL)";
                VComandoSQL.ExecuteNonQuery();

                VComandoSQL.CommandText = @"CREATE TABLE [htk_Sesiones_Inventario] (
                           ID_SESION NVARCHAR(30) PRIMARY KEY NOT NULL,
                           DESCRIPCION NVARCHAR(100) NULL,
                           CATEGORIA_INVENTARIO NVARCHAR(30) NULL,
                           USUARIO_CREACION NVARCHAR(50) NULL,
                           TIPO_INVENTARIO NVARCHAR(15) NULL,
                           UNIDAD_EXPLOTACION NVARCHAR(15) NULL,
                           NOMBRE_CONJUNTO NVARCHAR(50) NULL,
                           ID_DEPARTAMENTO NVARCHAR(20) NULL,
                           NOMBRE_DEPARTAMENTO NVARCHAR(50) NULL,
                           ID_UBICACION NTEXT NULL,
                           NOMBRE_UBICACION NTEXT NULL,
                           HH_INVOLUCRADOS NVARCHAR(255) NULL,
                           FECHA_INICIO DATETIME NULL,
                           FECHA_FINALIZACION DATETIME NULL,
                           FECHA_APERTURA DATETIME NULL,
                           FECHA_CIERRE DATETIME NULL,
                           STATUS NVARCHAR(1) NULL)";
                VComandoSQL.ExecuteNonQuery();


                //.........................................
                VComandoSQL.CommandText = "htk_Inventarios";
                VComandoSQL.CommandType = CommandType.TableDirect;

                rs = VComandoSQL.ExecuteResultSet(ResultSetOptions.Updatable);
                rec = rs.CreateRecord();

                String objectString = _objectRealTable.GetRows();
                JArray objects = JsonConvert.DeserializeObject<JArray>(objectString);

                foreach (JObject objec in objects)
                {
                    String rowEpc = "";
                    String rowName = "";
                    String rowId = "";
                    String rowLocation = "";

                    try { rowEpc = objec["EPC"].ToString(); }
                    catch (Exception e) { }
                    try { rowName = objec["name"].ToString(); }
                    catch (Exception e) { }
                    try { rowId = objec["_id"].ToString(); }
                    catch (Exception e) { }
                    try { rowLocation = objec["location"].ToString(); }
                    catch (Exception e) { }


                    rec.SetValue(0, rowId);
                    rec.SetValue(1, rowEpc);
                    rec.SetValue(7, rowName);
                    rec.SetValue(12, rowId);
                    rec.SetValue(13, rowLocation);
                    rs.Insert(rec);
                }

            }
            catch (Exception VError)
            {
                throw VError;
            }
            finally
            {
                rs.Close();
                rs.Dispose();
                VComandoSQL.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;
                DBDatabase.Dispose();
            }
        }

        /// <summary>
        ///     This method allows to create a file .sdf 
        ///     for Reference
        ///     SQL SERVER CE Version 4.0
        /// </summary>
        /// <param name="idInventory"></param>
        /// <param name="dataInfo"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        public void createReference(String idInventory, String dataInfo)
        {
            //check if the path exist
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\" + idInventory + "\\";
            String absolutepath = Server.MapPath(relativepath);
            JObject datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);

            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "Referencia.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;

            /*      SqlCeResultSet rs = null;
                  SqlCeUpdatableRecord rec = null;
                  SqlCeEngine DBDatabase = new SqlCeEngine(@"Data Source = " + curFile + ";");
                  SqlCeConnection vCon = new SqlCeConnection(@"Data Source = " + curFile + ";");
                  SqlCeCommand VComandoSQL = vCon.CreateCommand();*/

            try
            {
                //*** Eliminated if there is already a DB  ***//
                /*      if (System.IO.File.Exists(curFile))
                          System.IO.File.Delete(curFile);

                      //*** Create a DB
                      DBDatabase.CreateDatabase();
                      vCon.Open();

                      //*** Insert table 
                      VComandoSQL.CommandText = @"CREATE TABLE [htk_Activos_Referencia] (
                              ID_ARTICULO NVARCHAR(25) NULL,
                              DEPARTAMENTO NVARCHAR(50) NULL,
                              UBICACION NVARCHAR(50) NULL,
                              DESC_ARTICULO NVARCHAR(100) NULL,
                              MARCA NVARCHAR(30) NULL,
                              MODELO NVARCHAR(30) NULL,
                              ID_PERFIL_ACTIVO NVARCHAR(50) NULL,
                              PRECIO_COMPRA FLOAT NULL)";
                      VComandoSQL.ExecuteNonQuery();
                      //.........................................

                      VComandoSQL.CommandText = "htk_Activos_Referencia";
                      VComandoSQL.CommandType = CommandType.TableDirect;

                      rs = VComandoSQL.ExecuteResultSet(ResultSetOptions.Updatable);
                      rec = rs.CreateRecord();*/

                String objectString = "";
                JArray objects = new JArray();
                try
                {
                    objectString = _objectTable.GetRows(idInventory);
                    objects = JsonConvert.DeserializeObject<JArray>(objectString);
                    JArray objectsall = new JArray();
                    foreach (JObject row in objects)
                    {
                        JToken jk;
                        if (!row.TryGetValue("marca", out jk))
                            row.Add("marca", "");
                        if (!row.TryGetValue("modelo", out jk))
                            row.Add("modelo", "");
                        if (!row.TryGetValue("photo", out jk))
                            row.Add("photo", null);



                        //Image to byte[]
                        string relativepath2 = "\\Uploads\\Images\\ObjectReferences\\";
                        string absolutepathdir = Server.MapPath(relativepath2);
                        string filename = "thumb_" + row["_id"].ToString() + "." + row["ext"].ToString();
                        string fileabsolutepath = absolutepathdir + filename;

                        if (System.IO.File.Exists(fileabsolutepath))
                        {
                            try
                            {
                                System.Drawing.Image myImage = System.Drawing.Image.FromFile(fileabsolutepath);
                                System.IO.MemoryStream imgMemoryStream = new System.IO.MemoryStream();
                                myImage.Save(imgMemoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                byte[] imgByteData = imgMemoryStream.GetBuffer();
                                row["photo"] = imgByteData;
                            }
                            catch (Exception e)
                            {
                                Error.Log(e, "Image not found");
                            }
                        }

                        foreach (JProperty joincustoms in row["profileFields"])
                        {
                            string namekey = joincustoms.Name;
                            namekey = namekey.Replace("_HTKField", "");
                            switch (namekey)
                            {
                                case "marca":
                                    row["marca"] = joincustoms.Value;
                                    break;
                                case "modelo":
                                    row["modelo"] = joincustoms.Value;
                                    break;


                            }

                        }
                        row.Remove("profileFields");
                        objectsall.Add(row);
                    }

                    objects = objectsall;
                }
                catch (Exception ex)
                {

                }

                /*  foreach (JObject objec in objects)
                  {
                      rec.SetValue(0, objec["_id"].ToString());
                      rec.SetValue(3, objec["name"].ToString());
                      rs.Insert(rec);
                  }*/

                String path = rootPath + relativepath;
                string path1 = path.Replace("\\", "\\\\");

                string query = @"CREATE TABLE [htk_Activos_Referencia] (
	                    ID_ARTICULO NVARCHAR(25) NULL,
	                    DEPARTAMENTO NVARCHAR(50) NULL,
	                    UBICACION NVARCHAR(50) NULL,
	                    DESC_ARTICULO NVARCHAR(100) NULL,
	                    MARCA NVARCHAR(30) NULL,
	                    MODELO NVARCHAR(30) NULL,
	                    ID_PERFIL_ACTIVO NVARCHAR(50) NULL,
	                    PRECIO_COMPRA FLOAT NULL,
                        FOTO IMAGE NULL)";

                string fields = "'_id','name','photo'";
                string values = "'0','3','8'";
                string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                string exe = Server.MapPath(pathexe);
                String datainfor = "";

                datainfor = JsonConvert.SerializeObject(objects);
                // datainfor = datainfor.Replace("\"", "'");
                datainfor = datainfor.Replace("\r", "").Replace("\t", "").Replace("\n", "");
                objects = JsonConvert.DeserializeObject<JArray>(datainfor);
                String JsonData = "{\"path\":\"" + path1 + "\",\"namefile\":\"" + fileName + "\",\"tables\":[{\"nametable\":\"htk_Activos_Referencia\",\"query\":\"" + query + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]}]}";
                JsonData = JsonData.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                // JsonData = JsonConvert.SerializeObject(JsonData);
                string urlfiletxt = path + "Reference.txt";
                System.IO.File.WriteAllText(urlfiletxt, JsonData);
                String Jsonfile = "{'url':'" + urlfiletxt + "'}";
                Jsonfile = JsonConvert.SerializeObject(Jsonfile);
                ProcessStartInfo procceesstar = new ProcessStartInfo();
                procceesstar.FileName = exe;
                procceesstar.Arguments = Jsonfile;

                Process procces = new Process();
                procces.StartInfo = procceesstar;
                procces.Start();
                procces.WaitForExit();
                System.IO.File.Delete(urlfiletxt);
            }
            catch (Exception VError)
            {
                throw VError;
            }
            finally
            {
                /*   rs.Close();
                   rs.Dispose();
                   VComandoSQL.Dispose();
                   vCon.Close();
                   vCon.Dispose();
                   vCon = null;
                   DBDatabase.Dispose();*/
            }

        }

        public void createUserDB(String idInventory)
        {
            //check if the path exist
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\" + idInventory + "\\";
            String absolutepath = Server.MapPath(relativepath);



            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "Usuarios.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;
            String path = rootPath + relativepath;
            string path1 = path.Replace("\\", "\\\\");

            try
            {

                JArray listUser = new JArray();

                string userrows = usersdb.GetRows();
                listUser = JsonConvert.DeserializeObject<JArray>(userrows);


                if (listUser == null)
                {
                    listUser = new JArray();
                }
                try
                {
                    /*  foreach (JObject location in listLocation)
                      {
                          rec.SetValue(0, location["_id"].ToString());
                          rec.SetValue(1, location["name"].ToString());
                          rs.Insert(rec);
                      }*/
                }
                catch (Exception ex)
                {

                }
                JArray list2 = new JArray();
                foreach (JObject obj in listUser)
                {
                    JObject obj1 = new JObject();
                    obj1["_id"] = obj["_id"].ToString();
                    obj1["name"] = obj["name"].ToString();
                    obj1["lastname"] = obj["lastname"].ToString();
                    obj1["photo"] = null;

                    //Image to byte[]
                    string relativepath2 = "/Uploads/Images/";
                    string absolutepathdir = Server.MapPath(relativepath2);
                    string filename = "thumb_" + obj["_id"].ToString() + "." + obj["imgext"].ToString();
                    string fileabsolutepath = absolutepathdir + filename;

                    if (System.IO.File.Exists(fileabsolutepath))
                    {
                        try
                        {
                            System.Drawing.Image myImage = System.Drawing.Image.FromFile(fileabsolutepath);
                            System.IO.MemoryStream imgMemoryStream = new System.IO.MemoryStream();
                            myImage.Save(imgMemoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            byte[] imgByteData = imgMemoryStream.GetBuffer();

                            string StringImage = Convert.ToBase64String(imgByteData);

                            byte[] bytes = Convert.FromBase64String(StringImage);


                            obj1["photo"] = StringImage;
                        }
                        catch (Exception e)
                        {
                            Error.Log(e, "Image not found");
                        }
                    }

                    list2.Add(obj1);
                }

                string query = @"CREATE TABLE [htk_Usuarios] (ID_USUARIO NVARCHAR(25) NULL, NOMBRE NVARCHAR(50) NULL, APELLIDO NVARCHAR(50) NULL, FOTO IMAGE NULL)";
                string fields = "'_id','name','lastname','photo'";
                string values = "'0','1','2','3'";
                string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                string exe = Server.MapPath(pathexe);

                String datainfor = JsonConvert.SerializeObject(list2);
                datainfor = datainfor.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                //  datainfor = datainfor.Replace("\"", "'");
                list2 = JsonConvert.DeserializeObject<JArray>(datainfor);
                String JsonData = "{\"path\":\"" + path1 + "\",\"namefile\":\"" + fileName + "\",\"tables\":[{\"nametable\":\"htk_Usuarios\",\"query\":\"" + query + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]}]}";
                JsonData = JsonData.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                // JsonData = JsonConvert.SerializeObject(JsonData);
                string urlfiletxt = path + "Users.txt";
                System.IO.File.WriteAllText(urlfiletxt, JsonData);
                String Jsonfile = "{'url':'" + urlfiletxt + "'}";
                Jsonfile = JsonConvert.SerializeObject(Jsonfile);
                ProcessStartInfo procceesstar = new ProcessStartInfo();
                procceesstar.FileName = exe;
                procceesstar.Arguments = Jsonfile;
                Process procces = new Process();
                procces.StartInfo = procceesstar;
                procces.Start();
                procces.WaitForExit();
                System.IO.File.Delete(urlfiletxt);
            }
            catch (Exception VError)
            {
                //throw VError;
            }
            finally
            {
                /* rs.Close();
                 rs.Dispose();
                 VComandoSQL.Dispose();
                 vCon.Close();
                 vCon.Dispose();
                 vCon = null;
                 DBDatabase.Dispose();*/
            }
        }

        /// <summary>
        ///     This method allows to create a file .sdf 
        ///     for Location
        ///     SQL SERVER CE Version 4.0
        /// </summary>
        /// <param name="idInventory"></param>
        /// <param name="dataInfo"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        public void createLocationDB(String idInventory, String dataInfo)
        {

            /*   string pathexe = "\\Uploads\\ConsoleApplication1.exe";
                   string exe=Server.MapPath(pathexe);*/



            //check if the path exist
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\" + idInventory + "\\";
            String absolutepath = Server.MapPath(relativepath);

            JObject datainfoString = new JObject();
            try
            {
                datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);
            }
            catch (Exception ex) { }

            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "Ubicaciones.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;
            String path = rootPath + relativepath;
            string path1 = path.Replace("\\", "\\\\");

            /*SqlCeResultSet rs = null;
            SqlCeUpdatableRecord rec = null;
            SqlCeEngine DBDatabase = new SqlCeEngine(@"Data Source = " + curFile + ";");
         
           SqlCeConnection vCon = new SqlCeConnection(@"Data Source = " + curFile + ";");
         
            SqlCeCommand VComandoSQL = vCon.CreateCommand();*/

            try
            {
                //*** Eliminated if there is already a DB  ***//
                /*  if (System.IO.File.Exists(curFile))
                      System.IO.File.Delete(curFile);

                  //*** Create a DB
                  DBDatabase.CreateDatabase();
                  vCon.Open();

                  //*** Insert table 
                  VComandoSQL.CommandText = @"CREATE TABLE [htk_Ubicaciones] (
                              UB_ID_UBICACION NVARCHAR(25) NULL,
                              UB_DESCRIPCION NVARCHAR(50) NULL)";
                  VComandoSQL.ExecuteNonQuery();
                  //.........................................

                  VComandoSQL.CommandText = "htk_Ubicaciones";
                  VComandoSQL.CommandType = CommandType.TableDirect;

                  rs = VComandoSQL.ExecuteResultSet(ResultSetOptions.Updatable);
                  rec = rs.CreateRecord();*/

                String inventoryString = _inventoryTable.GetRow(idInventory);
                JObject inventoryRow = new JObject();
                try
                {
                    inventoryRow = JsonConvert.DeserializeObject<JObject>(inventoryString);
                }
                catch (Exception ex) { }

                String idLocation = inventoryRow["location"].ToString();

                JArray listLocation = getSubLocation(idLocation);
                if (listLocation == null)
                {
                    listLocation = new JArray();
                }
                try
                {
                    /*  foreach (JObject location in listLocation)
                      {
                          rec.SetValue(0, location["_id"].ToString());
                          rec.SetValue(1, location["name"].ToString());
                          rs.Insert(rec);
                      }*/
                }
                catch (Exception ex)
                {

                }


                string query = @"CREATE TABLE [htk_Ubicaciones] (UB_ID_UBICACION NVARCHAR(25) NULL,UB_DESCRIPCION NVARCHAR(50) NULL)";
                string fields = "'_id','name'";
                string values = "'0','1'";
                string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                string exe = Server.MapPath(pathexe);
                String datainfor = JsonConvert.SerializeObject(listLocation);
                datainfor = datainfor.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                //  datainfor = datainfor.Replace("\"", "'");
                listLocation = JsonConvert.DeserializeObject<JArray>(datainfor);
                String JsonData = "{\"path\":\"" + path1 + "\",\"namefile\":\"" + fileName + "\",\"tables\":[{\"nametable\":\"htk_Ubicaciones\",\"query\":\"" + query + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]}]}";
                JsonData = JsonData.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                // JsonData = JsonConvert.SerializeObject(JsonData);
                string urlfiletxt = path + "Locations.txt";
                System.IO.File.WriteAllText(urlfiletxt, JsonData);
                String Jsonfile = "{'url':'" + urlfiletxt + "'}";
                Jsonfile = JsonConvert.SerializeObject(Jsonfile);
                ProcessStartInfo procceesstar = new ProcessStartInfo();
                procceesstar.FileName = exe;
                procceesstar.Arguments = Jsonfile;
                Process procces = new Process();
                procces.StartInfo = procceesstar;
                procces.Start();
                procces.WaitForExit();
                System.IO.File.Delete(urlfiletxt);
            }
            catch (Exception VError)
            {
                //throw VError;
            }
            finally
            {
                /* rs.Close();
                 rs.Dispose();
                 VComandoSQL.Dispose();
                 vCon.Close();
                 vCon.Dispose();
                 vCon = null;
                 DBDatabase.Dispose();*/
            }
        }
        public void createSubLocationDB(String idInventory, String dataInfo)
        {

            /*   string pathexe = "\\Uploads\\ConsoleApplication1.exe";
                   string exe=Server.MapPath(pathexe);*/



            //check if the path exist
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\" + idInventory + "\\";
            String absolutepath = Server.MapPath(relativepath);

            JObject datainfoString = new JObject();
            try
            {
                datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);
            }
            catch (Exception ex) { }

            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "SubUbicaciones.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;
            String path = rootPath + relativepath;
            string path1 = path.Replace("\\", "\\\\");

            /*SqlCeResultSet rs = null;
            SqlCeUpdatableRecord rec = null;
            SqlCeEngine DBDatabase = new SqlCeEngine(@"Data Source = " + curFile + ";");
         
           SqlCeConnection vCon = new SqlCeConnection(@"Data Source = " + curFile + ";");
         
            SqlCeCommand VComandoSQL = vCon.CreateCommand();*/

            try
            {
                //*** Eliminated if there is already a DB  ***//
                /*  if (System.IO.File.Exists(curFile))
                      System.IO.File.Delete(curFile);

                  //*** Create a DB
                  DBDatabase.CreateDatabase();
                  vCon.Open();

                  //*** Insert table 
                  VComandoSQL.CommandText = @"CREATE TABLE [htk_Ubicaciones] (
                              UB_ID_UBICACION NVARCHAR(25) NULL,
                              UB_DESCRIPCION NVARCHAR(50) NULL)";
                  VComandoSQL.ExecuteNonQuery();
                  //.........................................

                  VComandoSQL.CommandText = "htk_Ubicaciones";
                  VComandoSQL.CommandType = CommandType.TableDirect;

                  rs = VComandoSQL.ExecuteResultSet(ResultSetOptions.Updatable);
                  rec = rs.CreateRecord();*/

                String inventoryString = _inventoryTable.GetRow(idInventory);
                JObject inventoryRow = new JObject();
                try
                {
                    inventoryRow = JsonConvert.DeserializeObject<JObject>(inventoryString);
                }
                catch (Exception ex) { }

                String idLocation = inventoryRow["location"].ToString();

                JArray listLocation = getSubLocation(idLocation);
                if (listLocation == null)
                {
                    listLocation = new JArray();
                }
                try
                {
                    /*  foreach (JObject location in listLocation)
                      {
                          rec.SetValue(0, location["_id"].ToString());
                          rec.SetValue(1, location["name"].ToString());
                          rs.Insert(rec);
                      }*/
                }
                catch (Exception ex)
                {

                }
                Dictionary<string, JObject> dictlocs = new Dictionary<string, JObject>();
                try
                {
                    dictlocs = listLocation.ToDictionary(x => (string)x["_id"], x => (JObject)x);
                }
                catch { }
                string getconjunt = locationsProfilesdb.Get("name", "Sub-Ubicaciones");
                string idprof = "";
                try
                {
                    JArray conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                    JArray auxlocs = new JArray();
                    JToken ko;
                    Dictionary<string, string> oficinaname = new Dictionary<string, string>();
                    foreach (JObject loc in listLocation)
                    {
                        if (idprof.Contains(loc["profileId"].ToString()))
                        {

                            if (loc.TryGetValue("parent", out ko))
                            {
                                if (loc["parent"].ToString() == "none")
                                    continue;
                                try
                                {
                                    JObject ubicacion = dictlocs[loc["parent"].ToString()];

                                    loc.Add("ubicacion", ubicacion["name"].ToString());
                                    loc.Add("ubicacion_id", ubicacion["_id"].ToString());
                                    loc.Add("oficina_id", ubicacion["parent"].ToString());
                                }
                                catch { }
                                try
                                {

                                   if(oficinaname.Count()==0)
                                    {
                                        JObject ofi = dictlocs[loc["oficina_id"].ToString()];
                                        oficinaname.Add(ofi["_id"].ToString(), ofi["name"].ToString());
                                    }

                                    loc.Add("oficina", oficinaname[loc["oficina_id"].ToString()]);

                                }
                                catch { }
                                JToken tk;
                                if (!loc.TryGetValue("ubicacion", out tk))
                                {
                                    loc.Add("ubicacion", "");
                                }
                                if (!loc.TryGetValue("ubicacion_id", out tk))
                                {
                                    loc.Add("ubicacion_id", "");
                                }
                                if (!loc.TryGetValue("oficina", out tk))
                                {
                                    loc.Add("oficina", "");
                                }
                                if (!loc.TryGetValue("oficina_id", out tk))
                                {
                                    loc.Add("oficina_id", "");
                                }
                                auxlocs.Add(loc);
                            }
                            continue;
                        }
                    }

                    listLocation = auxlocs;

                }
                catch
                {

                }

                string query = @"CREATE TABLE [htk_SubUbicaciones] (ID_SUBUBICACION NVARCHAR(25) NULL,SUB_DESCRIPCION NVARCHAR(50) NULL,UBICACION_DESCRIPCION NVARCHAR(50) NULL,OFICINA_DESCRIPCION NVARCHAR(50) NULL,ID_UBICACION NVARCHAR(25) NULL,ID_OFICINA NVARCHAR(25) NULL)";
                string fields = "'_id','name','ubicacion','oficina','ubicacion_id','oficina_id'";
                string values = "'0','1','2','3','4','5'";
                string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                string exe = Server.MapPath(pathexe);
                String datainfor = JsonConvert.SerializeObject(listLocation);
                datainfor = datainfor.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                //  datainfor = datainfor.Replace("\"", "'");
                listLocation = JsonConvert.DeserializeObject<JArray>(datainfor);
                String JsonData = "{\"path\":\"" + path1 + "\",\"namefile\":\"" + fileName + "\",\"tables\":[{\"nametable\":\"htk_SubUbicaciones\",\"query\":\"" + query + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]}]}";
                JsonData = JsonData.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                // JsonData = JsonConvert.SerializeObject(JsonData);
                string urlfiletxt = path + "SubLocations.txt";
                System.IO.File.WriteAllText(urlfiletxt, JsonData);
                String Jsonfile = "{'url':'" + urlfiletxt + "'}";
                Jsonfile = JsonConvert.SerializeObject(Jsonfile);
                ProcessStartInfo procceesstar = new ProcessStartInfo();
                procceesstar.FileName = exe;
                procceesstar.Arguments = Jsonfile;
                Process procces = new Process();
                procces.StartInfo = procceesstar;
                procces.Start();
                procces.WaitForExit();
                System.IO.File.Delete(urlfiletxt);
            }
            catch (Exception VError)
            {
                //throw VError;
            }
            finally
            {
                /* rs.Close();
                 rs.Dispose();
                 VComandoSQL.Dispose();
                 vCon.Close();
                 vCon.Dispose();
                 vCon = null;
                 DBDatabase.Dispose();*/
            }
        }
        //sacar cpnjuntos.sdf
        public void createConjuntoDB(String idInventory, String dataInfo)
        {

            /*   string pathexe = "\\Uploads\\ConsoleApplication1.exe";
                   string exe=Server.MapPath(pathexe);*/



            //check if the path exist
            String relativepath = "\\Uploads\\Inventory\\" + idInventory + "\\download\\" + idInventory + "\\";
            String absolutepath = Server.MapPath(relativepath);

            JObject datainfoString = new JObject();
            try
            {
                datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);
            }
            catch (Exception ex) { }

            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "Conjuntos.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;
            String path = rootPath + relativepath;
            string path1 = path.Replace("\\", "\\\\");

            try
            {
                String inventoryString = _inventoryTable.GetRow(idInventory);
                JObject inventoryRow = new JObject();
                try
                {
                    inventoryRow = JsonConvert.DeserializeObject<JObject>(inventoryString);
                }
                catch (Exception ex) { }

                String idLocation = inventoryRow["location"].ToString();

                JArray listLocation = getConjuntos();
                if (listLocation == null)
                {
                    listLocation = new JArray();
                }
                try
                {
                    /*  foreach (JObject location in listLocation)
                      {
                          rec.SetValue(0, location["_id"].ToString());
                          rec.SetValue(1, location["name"].ToString());
                          rs.Insert(rec);
                      }*/
                }
                catch (Exception ex)
                {

                }


                string query = @"CREATE TABLE [htk_Conjuntos] (CJ_UNIDAD_EXPLOTACION NVARCHAR(15) NULL,CJ_NOMBRE_CONJUNTO NVARCHAR(50) NULL,ID_CONJUNTO NVARCHAR(25) NULL)";
                string fields = "'number','name','_id'";
                string values = "'0','1','2'";
                string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                string exe = Server.MapPath(pathexe);
                String datainfor = JsonConvert.SerializeObject(listLocation);
                datainfor = datainfor.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                //  datainfor = datainfor.Replace("\"", "'");
                listLocation = JsonConvert.DeserializeObject<JArray>(datainfor);
                String JsonData = "{\"path\":\"" + path1 + "\",\"namefile\":\"" + fileName + "\",\"tables\":[{\"nametable\":\"htk_Conjuntos\",\"query\":\"" + query + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]}]}";
                JsonData = JsonData.Replace("\r", "").Replace("\t", "").Replace("\n", "");

                // JsonData = JsonConvert.SerializeObject(JsonData);
                string urlfiletxt = path + "Conjuntos.txt";
                System.IO.File.WriteAllText(urlfiletxt, JsonData);
                String Jsonfile = "{'url':'" + urlfiletxt + "'}";
                Jsonfile = JsonConvert.SerializeObject(Jsonfile);
                ProcessStartInfo procceesstar = new ProcessStartInfo();
                procceesstar.FileName = exe;
                procceesstar.Arguments = Jsonfile;
                Process procces = new Process();
                procces.StartInfo = procceesstar;
                procces.Start();
                procces.WaitForExit();
                System.IO.File.Delete(urlfiletxt);
            }
            catch (Exception VError)
            {
                //throw VError;
            }
            finally
            {
                /* rs.Close();
                 rs.Dispose();
                 VComandoSQL.Dispose();
                 vCon.Close();
                 vCon.Dispose();
                 vCon = null;
                 DBDatabase.Dispose();*/
            }
        }


        /// <summary>
        ///     This method gets the subLocation of a location
        /// </summary>
        /// <param name="idInventory"></param>
        /// <param name="dataInfo"></param>
        /// <author>
        ///     Abigail Rodriguez
        /// </author>
        /// <returns>
        ///     A JArray with the locations list
        /// </returns>
        public JArray getSubLocation(String idLocation)
        {
            String locationArray = _locationTable.GetRow(idLocation);

            try
            {
                JObject locationRow = JsonConvert.DeserializeObject<JObject>(locationArray);
                JArray listLocation = new JArray();
                listLocation.Add(locationRow);

                for (int i = 0; i < listLocation.Count; i++)
                {
                    locationArray = _locationTable.Get("parent", listLocation[i]["_id"].ToString());
                    if (locationArray != null || locationArray != "[]")
                    {
                        JArray locationString = JsonConvert.DeserializeObject<JArray>(locationArray);
                        foreach (JObject locations in locationString)
                        {
                            listLocation.Add(locations);
                        }
                    }
                }
                return listLocation;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public JArray getConjuntos() {
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
            return locatList;
        }
    }
}
