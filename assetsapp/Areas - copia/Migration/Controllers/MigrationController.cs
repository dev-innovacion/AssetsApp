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
//using iTextSharp.text;
//using iTextSharp.text.html;
//using iTextSharp.text.pdf;
//using iTextSharp.text.xml;
//using iTextSharp.text.html.simpleparser;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.CustomXmlDataProperties;
using System.IO;
using System.Timers;
using Rivka.Mail;
//using System.Windows.Forms;
using System.IO.Compression;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Rivka.Db.MongoDb;
using Rivka.Mail;
using RivkaAreas.Rule;
using Rivka.Error;
using System.Reflection;
using System.Dynamic;

using Rivka.Form;
using Rivka.User;
using Rivka.Mail;
using System.Web.Security;
using System.Reflection;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using RivkaAreas.Inventory.Models;

namespace RivkaAreas.Migration.Controllers
{
    [Authorize]
    public class MigrationController : Controller
    {
        //
        // GET: /Migration/Migration/
        protected MongoModel objReals;
        protected MongoModel objReferencedb;
        protected LocationTable locationsdb;
        protected UserTable usersdb;
        protected Notifications Notificationsclass;
        protected MongoModel locationsProfilesdb;
        protected RivkaAreas.Tags.Models.ObjectReal objectrealdb;
        protected Messenger push;
        public int countimer;
        public MigrationController()
        {
            objReals = new MongoModel("ObjectReal");
            locationsdb = new LocationTable();
            objReferencedb = new MongoModel("ReferenceObjects");
            Notificationsclass = new Notifications();
            usersdb = new UserTable();
            locationsProfilesdb = new MongoModel("LocationProfiles");
            objectrealdb = new RivkaAreas.Tags.Models.ObjectReal("ObjectReal");
            push = new Messenger();
            countimer = 0;
        }
        public ActionResult Index()
        {
            return View();
        }
        public String saveFile(HttpPostedFileBase excelFile)
        {
            try
            {

                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                string url = "";
                string filename = "";
                if (excelFile != null)
                {
                    filename = excelFile.FileName.ToString();

                }
                string userid = "";
                try
                {
                    userid = Session["_id"].ToString();
                }
                catch (Exception e)
                {
                    userid = Request.Cookies["_id2"].Value;
                }

                string ext = null;
                string patch = "";
               
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;
                string relativepath = @"\Uploads\Migration\" + userid;
                string absolutepath = Server.MapPath(relativepath);
                string absolutepathdown = Server.MapPath(relativepath);

                if (System.IO.Directory.Exists(absolutepath))
                {
                    try
                    {
                        System.IO.Directory.Delete(absolutepath, true);
                    }
                    catch (Exception e)
                    {
                       
                    }
                }

                if (excelFile != null)
                {
                    ext = excelFile.FileName.Split('.').Last(); //getting the extension
                }
                if (excelFile != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }

                        excelFile.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        url = absolutepath + "\\" + patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                        excelFile.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        url = absolutepath + "\\" + patch + "." + ext;

                    }

                   

                    //  string desingc = objReals.GetRows();


                }



                return url;
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        public JObject CreateJson()
        {
            try
            {
               JObject model = new JObject();

                model.Add("ID_REGISTRO","");
                model.Add("AF_UNIDAD_EXPLOTACION","");
                model.Add("AF_NOMBRE_CONJUNTO","");
                model.Add("AF_DEPARTAMENTO","");
                model.Add("AF_UBICACION","");
                model.Add("AF_CANTIDAD","");
                model.Add("AF_ID_ARTICULO","");
               model.Add("AF_DESC_ARTICULO","");
                model.Add("AF_MARCA","");
                model.Add("AF_MODELO","");
                model.Add("AF_NUM_SERIE","");
                model.Add("AF_ID_PERFIL_ACTIVO","");
                model.Add("AF_NUM_ACTIVO_ERP","");
                model.Add("AF_EPC_COMPLETO","");
                model.Add("AF_NUM_SOLICITUD","");
                model.Add("AF_NUM_PEDIDO","");;
               model.Add("AF_NUM_RECEPCION","");
               model.Add("AF_ID_PROVEEDOR","");
               model.Add("AF_DESC_PROVEEDOR","");
                model.Add("AF_NUM_FACTURA","");
                model.Add("AF_STATUS_ETIQUETADO","");
                model.Add("AF_FECHA_ETIQUETADO","");
                model.Add("AF_FECHA_REGISTRO","");
               model.Add("AF_TIPO_ETIQUETADO","");
                model.Add("AF_USUARIO_REGISTRO","");
                model.Add("AF_USUARIO_ETIQUETADO","");
                model.Add("AF_STATUS_ACTIVO","");
                model.Add("AF_PRECIO_COMPRA","");
                model.Add("AF_FECHA_COMPRA","");
               model.Add("AF_RH","");
               model.Add("AF_C1","");
               model.Add("AF_C2","");
                model.Add("AF_C3","");
                model.Add("AF_C4","");
                model.Add("AF_C5","");
               model.Add("AF_C6","");
                model.Add("AF_C7","");
                model.Add("AF_C8","");
               model.Add("AF_C9","");
                model.Add("AF_C10","");
                model.Add("AF_ARCHIVO_PDF","");
                model.Add("AF_ID_FOTO","");
                model.Add("AF_ID_CONTROL_SYNC","");
                model.Add("AF_TIPO_ACTIVO","");
                model.Add("AF_ID_PAQUETE","");
                return model;
    
            }catch{
                return new JObject();
            
            }
        }
        public Dictionary<int, string> pointer(JObject jo)
        {
            try
            {
              Dictionary<int,string> dictmodel=  new Dictionary<int, string>();
              int index = 0;
              foreach(JProperty item in jo as JToken)
              {

                  try
                  {
                      dictmodel.Add(index, item.Name.ToString());
                      index++;
                  }
                  catch { }

              }
              return dictmodel;
            }
            catch
            {
                return new Dictionary<int, string>();
            }

        }
        public List<CreateModel> match(JArray ja)
        {
            try
            {
                List<CreateModel> listdata = new List<CreateModel>();

                foreach (JToken jt in ja)
                {
                    try
                    {
                        CreateModel datamodel = new CreateModel();
                        foreach (JProperty jp in jt)
                        {
                            try
                            {
                                ((IDictionary<string, object>)datamodel.model)[jp.Name.ToString()] = jp.Value.ToString();
                            }
                            catch { }
                        }

                        listdata.Add(datamodel);
                    }
                    catch
                    {

                    }
                }
                return listdata;
            }
            catch
            {
                return new List<CreateModel>();
            }
        }
        public List<CreateModel> getRows(String url)
        {
            List<CreateModel> ListData = new List<CreateModel>();
            JArray DataJa = new JArray();
            try
            {
                Dictionary<string, int> orderCell = new Dictionary<string, int>();
                string[] arrayalf = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ", "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ", "DA", "DB", "DC" };

                for (int i = 0; i < arrayalf.Length; i++)
                {
                    orderCell.Add(arrayalf[i], i);
                }
                DataSet ds = new DataSet();

                List<List<string>> tr = new List<List<string>>();
                Dictionary<int, string> dictpointer = new Dictionary<int, string>();
                JObject initjo = CreateJson();
                dictpointer = pointer(initjo);
               
                try{
                using (DocumentFormat.OpenXml.Packaging.SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(url, false))
                {
                    try
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;


                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                        var httpCurrent = System.Web.HttpContext.Current;
                     //  string url5= httpCurrent.Request.Url.PathAndQuery.ToString();
                        int rowindex=0;
                     /*   Parallel.ForEach(sheetData.Elements<Row>(),( r,pls,indexfor) =>
                       {*/
                        foreach(Row r in sheetData.Elements<Row>()){
                           try
                           {
                               //pls.Break;

                               if (r.RowIndex == 1)
                                   continue;
                                  // return;
                               List<string> td = new List<string>();
                               int index = 0;
                               //CreateModel Dataobj = new CreateModel();
                               JObject Datajo = CreateJson();
                               
                               int point = 0;
                              
                               // dynamic Dataobj = CreateModel();
                               foreach (Cell c in r.Elements<Cell>())
                               {
                                   try
                                   {

                                      // string cellIndex = c.CellReference.ToString().Substring(0, 1);
                                       bool validate = false;
                                       int numcellx = 0;
                                       string cellIndex = "";
                                       // cellIndex=c.CellReference.ToString().Substring(0, 1);
                                       foreach (char t in c.CellReference.ToString())
                                       {

                                           if (Regex.IsMatch(t.ToString(), "[A-Z]"))
                                           {
                                               cellIndex += t.ToString();
                                           }
                                       }
                                     
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
                                               // td.Add("");
                                               // Dataobj.setValue(index, "");
                                               Datajo[dictpointer[point]] = "";
                                               point++;
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

                                                 
                                                  // Dataobj.setValue(point, item.Text.Text);
                                                   Datajo[dictpointer[point]] = item.Text.Text;

                                               }
                                               else if (item.InnerText != null)
                                               {


                                                   //Dataobj.setValue(point, item.InnerText);
                                                   Datajo[dictpointer[point]] = item.InnerText;
                                               }
                                               else if (item.InnerXml != null)
                                               {


                                                  // Dataobj.setValue(point, item.InnerXml);
                                                   Datajo[dictpointer[point]] = item.InnerXml;
                                               }
                                           }
                                           else
                                           {


                                               //Dataobj.setValue(point, c.CellValue.Text);
                                               Datajo[dictpointer[point]] = c.CellValue.Text;
                                           }
                                       }
                                       else
                                       {

                                           try
                                           {

                                               if (c.CellValue != null)
                                               {
                                                  // Dataobj.setValue(point, c.CellValue.Text);
                                                   Datajo[dictpointer[point]] = c.CellValue.Text;
                                               }
                                               else
                                               {
                                                   ///Dataobj.setValue(point, "");
                                                   Datajo[dictpointer[point]] ="";
                                               }
                                           }
                                           catch (Exception ex)
                                           {

                                              // Dataobj.setValue(point, "");
                                               Datajo[dictpointer[point]] = "";
                                           }

                                       }
                                       index++;
                                   }
                                   catch
                                   {

                                       string error = "Celda incorrecta";
                                   }
                                   point++;
                               }
                              // ListData.Add(Dataobj);
                               if(point>=44)
                                DataJa.Add(Datajo);
                           }
                           catch
                           {

                           }
                          // indexfor++;
                       }
                        //);
                        try
                        {
                            spreadsheetDocument.Close();
                        }
                        catch { }
                    }
                    catch {  }
                }}catch{

                }

               // CreateModel DataModel = new CreateModel();

                ListData = match(DataJa);
                return ListData;
            }
            catch
            {
                ListData = match(DataJa);
                return ListData;
            }
        }
        public List<CreateModel> getRows2(String url)
        {
            List<CreateModel> ListData = new List<CreateModel>();
            try
            {
                Dictionary<string, int> orderCell = new Dictionary<string, int>();
                string[] arrayalf = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                for (int i = 0; i < arrayalf.Length; i++)
                {
                    orderCell.Add(arrayalf[i], i);
                }
                DataSet ds = new DataSet();

                List<List<string>> tr = new List<List<string>>();
               
                using (DocumentFormat.OpenXml.Packaging.SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(url, false))
                {
                    try
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;


                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                        Parallel.ForEach(sheetData.Elements<Row>(), r =>
                       {
                           try
                           {
                               if (r.RowIndex == 1)
                                   return;
                               List<string> td = new List<string>();
                               int index = 0;
                               CreateModel Dataobj = new CreateModel();

                               int point = 0;
                               // dynamic Dataobj = CreateModel();
                               foreach (Cell c in r.Elements<Cell>())
                               {
                                   try
                                   {

                                       string cellIndex = c.CellReference.ToString().Substring(0, 1);
                                       bool validate = false;
                                       int numcellx = 0;
                                       /* foreach (var x in orderCell)
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
                                    
                                               // Dataobj.setValue(index, "");
                                            }
                                            index = index + numcellx;

                                        }*/
                                       Int32 id = -1;


                                       if (c.DataType != null && c.DataType.Value == CellValues.SharedString)
                                       {
                                           if (Int32.TryParse(c.InnerText, out id))
                                           {
                                               SharedStringItem item = GetSharedStringItemById(workbookPart, id);
                                               if (item.Text != null)
                                               {

                                                 
                                                   Dataobj.setValue(point, item.Text.Text);

                                               }
                                               else if (item.InnerText != null)
                                               {


                                                   Dataobj.setValue(point, item.InnerText);
                                               }
                                               else if (item.InnerXml != null)
                                               {


                                                   Dataobj.setValue(point, item.InnerXml);
                                               }
                                           }
                                           else
                                           {


                                               Dataobj.setValue(point, c.CellValue.Text);
                                           }
                                       }
                                       else
                                       {

                                           try
                                           {

                                               if (c.CellValue != null)
                                               {
                                                   Dataobj.setValue(point, c.CellValue.Text);
                                               }
                                               else
                                               {
                                                   Dataobj.setValue(point, "");
                                               }
                                           }
                                           catch (Exception ex)
                                           {

                                               Dataobj.setValue(point, "");
                                            
                                           }

                                       }
                                       index++;
                                   }
                                   catch
                                   {

                                       string error = "Celda incorrecta";
                                   }
                                   point++;
                               }
                               ListData.Add(Dataobj);
                           }
                           catch
                           {

                           }
                       });
                        try
                        {
                            spreadsheetDocument.Close();
                        }
                        catch { }
                    }
                    catch {  }
                }
                return ListData;
            }
            catch
            {
                return ListData;
            }
        }
        public List<dynamic> duplicateEpcs(List<CreateModel> Rows,dynamic Conjuntos)
        {
            try{
            List<dynamic> resultepcs = new List<dynamic>();
            List<string> Epcs = new List<string>();
            Epcs = (from row in Rows.Cast<dynamic>() select (string)row.AF_EPC_COMPLETO).ToList();
            JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.ValidEpcs(Epcs));
            // rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");
            dynamic resultepcs1 = (from objectsx in objects select new { epc = (string)objectsx["EPC"], name = (string)objectsx["name"], serie = (string)objectsx["serie"], marca = (string)objectsx["marca"], modelo = (string)objectsx["modelo"], location = (string)objectsx["location"], location_name = (string)objectsx["location_name"], conjunto = (string)objectsx["conjunto"], number = "", conjuntoname = "" }).ToList();

            foreach (dynamic obj in resultepcs1)
            {
                dynamic myobj = new ExpandoObject();
                myobj.epc = obj.epc;
                myobj.name = obj.name;
                myobj.serie = obj.serie;
                myobj.marca = obj.marca;
                myobj.modelo = obj.modelo;
                myobj.location = obj.location;
                myobj.location_name = obj.location_name;
                myobj.conjunto = obj.conjunto;
                myobj.number = obj.number;
                myobj.conjuntoname = obj.conjuntoname;
                resultepcs.Add(myobj);

            }
            if (Conjuntos != null)
            {
                foreach (dynamic item in resultepcs)
                {
                    try
                    {
                        foreach (dynamic item2 in Conjuntos)
                        {
                            try
                            {
                                if (item.conjunto == item2.id)
                                {
                                    item.number = item2.number;
                                    item.conjuntoname = item2.name;
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    catch
                    {

                    }
                }
            }

            return resultepcs;
        }catch{
            return new List<dynamic>();
       }

        }
        public String migrationInit(String url)
        {
            try
            {

                try
                {
                    List<CreateModel> Rows = getRows(url);
                    List<string> Epcs = new List<string>();
                    dynamic Conjuntos = null;
                    List<string> profileslist = new List<string>();
                    List<string> Idslist = new List<string>();
                    List<dynamic> resultepcs = new List<dynamic>();
                    List<dynamic> resultids = new List<dynamic>();
                   /* try
                    {

                        JArray profiles = JsonConvert.DeserializeObject<JArray>(locationsProfilesdb.Get("name", "Conjunto"));
                        profileslist = (from prof in profiles select (string)prof["_id"]).ToList();
                        JArray conjuntresult = JsonConvert.DeserializeObject<JArray>(objectrealdb.Getby("profileId", profileslist));
                        // rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");
                        Conjuntos = (from conj in conjuntresult select new { id = (string)conj["_id"], name = (string)conj["name"], number = (string)conj["number"] }).ToList();
                    }
                    catch { }*/

                    
                       // List<dynamic> epcsDuplicate = duplicateEpcs(Rows,Conjuntos);

                        return generateObjs(Rows);


                   
                   
                }
                catch(Exception ex)
                {
                    return ex.ToString();
                }

               
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public void timercount()
        {
            countimer = countimer + 10;
        }
        public  void timerEvent(object source, ElapsedEventArgs e){

            timercount();

        }
        public String generateObjs(List<CreateModel> rows)
        {


            try
            {
                Task<string> task1 = Task<string>.Factory.StartNew(() => locationsdb.GetRows());
                  
                List<string> listselects = new List<string>();
                List<dynamic> newObjs = new List<dynamic>();
                List<dynamic> editEpcs = new List<dynamic>();
                List<dynamic> editIds = new List<dynamic>();
               
                JObject datajo = new JObject();

                
                List<string> Epcs = new List<string>();
                List<string> Idslist = new List<string>();
                List<string> objectsid = new List<string>();
                Dictionary<string,string> objidsdict = new Dictionary<string,string>();
                Dictionary<string, string> assets = new Dictionary<string, string>();
                try
                {
                   // Epcs = (from row in rows.Cast<dynamic>() select (string)row.model.AF_EPC_COMPLETO).ToList();
                    foreach (CreateModel row in rows)
                    {
                        try
                        {
                            if (row.model.AF_EPC_COMPLETO != "" && row.model.AF_EPC_COMPLETO!=null)
                             Epcs.Add(row.model.AF_EPC_COMPLETO);
                        }
                        catch {  }
                    }
                    JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.ValidEpcs(Epcs));
                    Epcs.Clear();
                    Epcs = (from obj in objects select (string)obj["EPC"]).ToList();
                    //  rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");

                }
                catch { }
                try
                {
                    objectsid = (from row in rows.Cast<dynamic>() select (string)row.model.AF_ID_ARTICULO).ToList();
                    JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.GetbyCustom("object_id", objectsid, "ReferenceObjects"));

                   // objidsdict = (from obj in objects select new { id = (string)obj["_id"], object_id = (string)obj["object_id"] }).ToList();
                    //  rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");
                    
                  //  objidsdict = objects.ToDictionary(x => (string)x["_id"], x => (string)x["object_id"]);
                    foreach (JObject objid in objects)
                    {
                        try
                        {
                            objidsdict.Add((string)objid["_id"], (string)objid["object_id"]);
                        }
                        catch { objidsdict.Add((string)objid["_id"], ""); }
                        try
                        {
                            assets.Add((string)objid["_id"], (string)objid["assetType"]);
                        }
                        catch { assets.Add((string)objid["_id"], ""); }
                    }
                }
                catch { }
                try
                {
                    Idslist = (from row in rows.Cast<dynamic>() select (string)row.model.ID_REGISTRO).ToList();
                    JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.GetbyCustom("id_registro", Idslist, "ObjectReal"));
                    Idslist.Clear();
                    Idslist = (from obj in objects select (string)obj["id_registro"]).ToList();
                    // rows.Cast<dynamic>().Where(c => Idslist.Contains(c.id_registro)).ToList().ForEach(cc => cc.analisis = "Activo con Id de Registro ya Existente");

                }
                catch { }

               

                try
                {

                    int countobjs = 0;
                    JObject idprof = new JObject();
                    try
                    {
                        String idslocs = locationsProfilesdb.Get("name", "Ubicacion");
                        idprof = JsonConvert.DeserializeObject<JArray>(idslocs).First() as JObject;
                                               
                    }
                    catch { }
                    int total = rows.Count();
                    JObject data=new JObject();
                    string iduser="";
                        try
                    {
                        iduser = Session["_id"].ToString();
                    }
                    catch (Exception e)
                    {
                        iduser = Request.Cookies["_id2"].Value;
                    }
                    data.Add("index","0");
                    data.Add("total",total.ToString());
                    data.Add("progress", "0");
                    data.Add("iduser", iduser);
                    data.Add("timer", "0");
                    int count=1;
                   
                    int indexx = 0;
                   // decimal porcent = total / 100;
                   // int lote = Convert.ToInt16(Math.Round(porcent, MidpointRounding.ToEven));

                    JArray locationslist = new JArray();
                    try
                    {
                        Task.WaitAll(task1);
                        string locat = task1.Result;
                        locationslist = JsonConvert.DeserializeObject<JArray>(locat);
                    }
                    catch { }
                    decimal totalm = Convert.ToDecimal(total);

                    System.Timers.Timer timer = new System.Timers.Timer();
                    countimer = 0;
                    timer.Elapsed += new ElapsedEventHandler(timerEvent);
                    timer.Interval = 10;
                    int flag = 0;
                    foreach (dynamic row in rows)
                    {
                        try
                        {
                            if (flag==0)
                            {
                                timer.Start();
                                flag = 1;
                            }




                            if (flag != 2)
                            {
                                decimal auxindex = Convert.ToDecimal(indexx);

                                decimal progress = Convert.ToDecimal((auxindex / totalm) * 100);
                                int lote = Convert.ToInt16(Math.Round(progress, MidpointRounding.ToEven));

                                if (Convert.ToDecimal(lote) <= progress)
                                {


                                    if (count < lote && flag == 1)
                                    {
                                        timer.Stop();
                                        data["index"] = indexx.ToString();
                                        data["progress"] = lote.ToString();
                                        data["timer"] = countimer.ToString();
                                        push.pushMessage(JsonConvert.SerializeObject(data), "progress");
                                        count = lote;
                                        flag = 2;
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                       //count++;
                      
                            try
                            {
                              

                                string heads1 = "";
                                string nameobjs = row.model.AF_DESC_ARTICULO;
                                string locations = "";// row.ub_id_ubicacion;
                                string epcx = row.model.AF_EPC_COMPLETO;
                                string object_id = row.model.AF_ID_ARTICULO;
                                string numserie = row.model.AF_NUM_SERIE;
                                string conjunto = row.model.AF_UNIDAD_EXPLOTACION;
                                string namelocation = row.model.AF_UBICACION;
                                string statuslabel = (row.model.AF_STATUS_ETIQUETADO == "E") ? "etiquetado" : "";
                                string labeltype = (row.model.AF_TIPO_ETIQUETADO == "N") ? "normal" : "no_etiquetado";
                                string price = row.model.AF_PRECIO_COMPRA;
                               
                                price = (price.Contains("$")) ? price : "$" + price;
                              
                                JObject objectJo = new JObject();
                                string referenceobj = "";
                                try
                                {
                                    referenceobj = (from objid in objidsdict where objid.Value == row.model.AF_ID_ARTICULO select (string)objid.Key).First().ToString();
                                }
                                catch
                                {

                                }
                                


                                string assettype = "";
                                try
                                {
                                   // assettype = assets[referenceobj];
                                    switch ((string)row.model.AF_TIPO_ACTIVO)
                                    {
                                        case "3":
                                            assettype = "sound";
                                            break;
                                        case "2":
                                            assettype = "maintenance";
                                            break;
                                        case "1":
                                            assettype = "system";
                                            break;
                                    }
                                }
                                catch
                                {

                                }
                                string status = "";
                                bool status_system = true;
                                try
                                {
                                    // assettype = assets[referenceobj];
                                    switch ((string)row.model.AF_STATUS_ACTIVO)
                                    {
                                        case "A":
                                            status = "En tu Conjunto";
                                            break;
                                        case "B":
                                            status = "Dado de baja";
                                            status_system = false;
                                            break;
                                        
                                    }
                                }
                                catch
                                {

                                }
                                try
                                {
                                    string locationf = "";
                                    string conjuntoid = "";
                                    try
                                    {
                                        conjuntoid = (from conj in locationslist where (string)conj["number"] == conjunto select (string)conj["_id"]).First().ToString();
                                    }
                                    catch
                                    {
                                        locationf = "";
                                    }
                                    try
                                    {
                                       // JArray getlocations = (from loc in locationslist where (string)loc["parent"] == conjuntoid select loc) as JArray;
                                        JArray getlocations = new JArray();
                                        foreach (JObject loc in locationslist)
                                        {
                                            try
                                            {
                                                if (loc["parent"].ToString() == conjuntoid)
                                                {
                                                    getlocations.Add(loc);
                                                }
                                            }
                                            catch { }
                                        }
                                        string auxname = namelocation.ToLower().Replace(" ", "");
                                        locationf = (from locs in getlocations where locs["name"].ToString().ToLower().Replace(" ","") == auxname select (string)locs["_id"]).First().ToString();
                                        
                                    }
                                    catch
                                    {

                                    }
                                    if (locationf == "" && conjuntoid != "")
                                    {
                                        try
                                        {
                                            JObject newlocation = new JObject();
                                            newlocation.Add("name", namelocation);
                                            newlocation.Add("parent", conjuntoid);
                                            newlocation.Add("number", "");
                                            newlocation.Add("tipo", "1");

                                            try
                                            {
                                                 newlocation.Add("profileId", idprof["_id"].ToString());

                                            }
                                            catch { }
                                            string idlocnew = locationsdb.SaveRow(JsonConvert.SerializeObject(newlocation),locations);
                                            newlocation.Add("_id", idlocnew);
                                            locationslist.Add(newlocation);
                                            locationf = (idlocnew != null && idlocnew != "") ? idlocnew : "";

                                        }
                                        catch
                                        {

                                        }
                                    }
                                    locations = (locationf != "") ? locationf : "null";
                                }
                                catch { }
                                string date_label = row.model.AF_FECHA_ETIQUETADO;
                                try
                                {
                                    date_label = date_label.Replace(" ", "");
                                    date_label = date_label + " 00:00:00";
                                  date_label=Convert.ToString(DateTime.ParseExact((string)row.AF_FECHA_ETIQUETADO, "dd/MM/yyyy HH:mm:ss", null));
                                }
                                catch { }
                                Int64 timestamp=10000101000000;
                                string datecreated = row.model.AF_FECHA_REGISTRO;
                                try
                                {
                                    if (datecreated.Length < 14)
                                    {
                                        datecreated = datecreated.Replace(" ", "");
                                        datecreated = datecreated + " 00:00:00";
                                    }
                                    DateTime dateCreated2 = DateTime.ParseExact(datecreated, "dd/MM/yyyy HH:mm:ss", null);
                                    timestamp = Convert.ToInt64(dateCreated2.ToString("yyyyMMddHHmmss"));
                                }
                                catch
                                {

                                }
                                objectJo.Add("objectReference", referenceobj);
                                objectJo.Add("name", nameobjs);
                                objectJo.Add("location", locations);
                                objectJo.Add("EPC", epcx);
                                objectJo.Add("EPC_conflicto", "");
                                objectJo.Add("cambioEpc", false);
                                objectJo.Add("serie", numserie);
                                objectJo.Add("department", row.model.AF_DEPARTAMENTO);
                                objectJo.Add("quantity", row.model.AF_CANTIDAD);
                                objectJo.Add("assetType", assettype);
                                objectJo.Add("marca", row.model.AF_MARCA);
                                objectJo.Add("modelo", row.model.AF_MODELO);
                                objectJo.Add("perfil", row.model.AF_ID_PERFIL_ACTIVO);
                                objectJo.Add("num_ERP", row.model.AF_NUM_ACTIVO_ERP);
                                objectJo.Add("num_solicitud", row.model.AF_NUM_SOLICITUD);
                                objectJo.Add("num_pedido", row.model.AF_NUM_PEDIDO);
                                objectJo.Add("num_reception", row.model.AF_NUM_RECEPCION);
                                objectJo.Add("proveedor", row.model.AF_ID_PROVEEDOR);
                                objectJo.Add("desc_proveedor", row.model.AF_DESC_PROVEEDOR);
                                objectJo.Add("factura", row.model.AF_NUM_FACTURA);
                                objectJo.Add("status_label", statuslabel);
                                objectJo.Add("status", status);
                                objectJo.Add("user_label", row.model.AF_USUARIO_ETIQUETADO);
                                objectJo.Add("price", price);
                                objectJo.Add("date", row.model.AF_FECHA_COMPRA);
                                objectJo.Add("date_registro", row.model.AF_FECHA_REGISTRO);
                                objectJo.Add("RH", row.model.AF_RH);
                                objectJo.Add("date_label", date_label);
                                objectJo.Add("package_id", row.model.AF_ID_PAQUETE);
                                objectJo.Add("CreatedDate", datecreated);
                                objectJo.Add("label",labeltype);
                                objectJo.Add("object_id", object_id);
                                objectJo.Add("AF_C1", row.model.AF_C1);
                                objectJo.Add("AF_C10", row.model.AF_C10);
                                objectJo.Add("AF_ID_CONTROL_SYNC", row.model.AF_ID_CONTROL_SYNC);
                                objectJo.Add("user_registro", row.model.AF_USUARIO_REGISTRO);
                                objectJo.Add("id_registro", row.model.ID_REGISTRO);
                                objectJo.Add("id_registro_conflicto", "");
                                objectJo.Add("system_status", status_system);
                                objectJo.Add("cambio_id_registro", false);
                                objectJo.Add("CreatedTimeStamp", timestamp);
                                string item = JsonConvert.SerializeObject(objectJo);
                                string idr = "";

                                if ((Epcs.Contains(row.model.AF_EPC_COMPLETO) && (epcx.Length>4 && epcx!=null)))
                                {
                                    try
                                    {
                                      //  objectJo.Add("system_status", true);
                                        objectJo["EPC"] = "";
                                        objectJo["EPC_conflicto"] = epcx;
                                        objectJo["cambioEpc"] = true;
                                      //  item = JsonConvert.SerializeObject(objectJo);
                                     //   String objsresult = objReals.Get("EPC", row.af_epc_completo);
                                     //   JObject objedit = JsonConvert.DeserializeObject<JArray>(objsresult).First() as JObject;
                                      //  idr = objReals.SaveRow(item, objedit["_id"].ToString());
                                      //  idr = objReals.SaveRow(item);
                                       // indexx++;
                                        editEpcs.Add(row);
                                        
                                    }
                                    catch { }
                                }
                                if (Idslist.Contains(row.model.ID_REGISTRO) && row.model.ID_REGISTRO != "")
                                {
                                    try
                                    {
                                      //  objectJo.Add("system_status", true);
                                        objectJo["id_registro"]="";
                                        objectJo["id_registro_conflicto"] = row.model.ID_REGISTRO;
                                        objectJo["cambio_id_registro"]= true;
                                      //  item = JsonConvert.SerializeObject(objectJo);
                                      //  JObject objedit = JsonConvert.DeserializeObject<JObject>(objReals.GetRow(row.id_registro));
                                      //  idr = objReals.SaveRow(item, objedit["_id"].ToString());
                                      //  idr = objReals.SaveRow(item);
                                      //  indexx++;
                                        editIds.Add(row);
                                       
                                    }
                                    catch { }
                                }
                               
                                    try
                                    {
                                        item = JsonConvert.SerializeObject(objectJo);
                                        idr = objReals.SaveRow(item);
                                        if (epcx != "")
                                            Epcs.Add(epcx);
                                        string idregis = row.model.ID_REGISTRO;
                                        if (idregis.Count() > 2)
                                            Idslist.Add(row.model.ID_REGISTRO);

                                        
                                      
                                        indexx++;
                                        newObjs.Add(row);


                                    }
                                    catch { }
                               

                                bool ok = true;
                               /* try
                                {
                                    ok = RulesChecker.isValidToLocation(referenceobj, locations);
                                    if (ok == false)
                                    {
                                        Notificationsclass.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicacion no valida");
                                        // return "problem";
                                    }
                                }
                                catch { }*/
                               
                            }
                            catch { }
                        
                        countobjs++;

                    }

                  //  Notificationsclass.saveNotification("Objects", "Create", "Se han creado " + countobjs + " Activos");



                }
                catch (Exception ex)
                {

                }


                //  modifyObjs(url);
                ViewData["news"] = newObjs;
                ViewData["epcs"] = editEpcs;
                ViewData["ids"] = editIds;
                return "Migración Correcta";



            }
            catch (Exception ex)
            {

                return ex.ToString() ;
            }

        }
        public static SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, Int32 id)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
        }

    }
      public class CreateModel 
        {
          public dynamic model; 
          public CreateModel(){
            try{
               this.model = new ExpandoObject();
            
                this.model.ID_REGISTRO = "";
                this.model.AF_UNIDAD_EXPLOTACION = "";
                this.model.AF_NOMBRE_CONJUNTO = "";
                this.model.AF_DEPARTAMENTO = "";
                this.model.AF_UBICACION = "";
                this.model.AF_CANTIDAD = "";
                this.model.AF_ID_ARTICULO = "";
                this.model.AF_DESC_ARTICULO = "";
                this.model.AF_MARCA = "";
                this.model.AF_MODELO = "";
                this.model.AF_NUM_SERIE = "";
                this.model.AF_ID_PERFIL_ACTIVO = "";
                this.model.AF_NUM_ACTIVO_ERP = "";
                this.model.AF_EPC_COMPLETO = "";
                this.model.AF_NUM_SOLICITUD = "";
                this.model.AF_NUM_PEDIDO = "";
                this.model.AF_NUM_RECEPCION = "";
                this.model.AF_ID_PROVEEDOR = "";
                this.model.AF_DESC_PROVEEDOR = "";
                this.model.AF_NUM_FACTURA = "";
                this.model.AF_STATUS_ETIQUETADO = "";
                this.model.AF_FECHA_ETIQUETADO = "";
                this.model.AF_FECHA_REGISTRO = "";
                this.model.AF_TIPO_ETIQUETADO = "";
                this.model.AF_USUARIO_REGISTRO = "";
                this.model.AF_USUARIO_ETIQUETADO = "";
                this.model.AF_STATUS_ACTIVO = "";
                this.model.AF_PRECIO_COMPRA = "";
                this.model.AF_FECHA_COMPRA = "";
                this.model.AF_RH = "";
                this.model.AF_C1 = "";
                this.model.AF_C2 = "";
                this.model.AF_C3 = "";
                this.model.AF_C4 = "";
                this.model.AF_C5 = "";
                this.model.AF_C6 = "";
                this.model.AF_C7 = "";
                this.model.AF_C8 = "";
                this.model.AF_C9 = "";
                this.model.AF_C10 = "";
                this.model.AF_ARCHIVO_PDF = "";
                this.model.AF_ID_FOTO = "";
                this.model.AF_ID_CONTROL_SYNC = "";
                this.model.AF_TIPO_ACTIVO = "";
                this.model.AF_ID_PAQUETE = "";
               
    
            }catch{
                
            
            }
          }

          public void setValue(int index, String Value)
          {

              try{
                  int auxindex=0;
                  
                  foreach(dynamic item in this.model){
                      if(index==auxindex){
                        ((IDictionary<string,object>)this.model)[item.Key]=Value;
                        break;
                      }
                      auxindex++;
                  }
                     
                  
               }catch{

               }
          }
          public dynamic getValue(int index)
          {

              try
              {
                  int auxindex = 0;

                  foreach (dynamic item in model)
                  {
                      if (index == auxindex)
                      {
                        return ((IDictionary<string, object>)model)[item.Key];
                          
                      }
                      auxindex++;
                  }

                  return null;
              }
              catch
              {
                  return null;
              }
          }
        }
}
