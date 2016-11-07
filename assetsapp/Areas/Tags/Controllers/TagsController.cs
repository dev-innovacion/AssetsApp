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
using Rivka.Db.MongoDb;
using Rivka.Mail;
using RivkaAreas.Rule;
using Rivka.Error;
using System.Reflection;
using System.Dynamic;
 

namespace RivkaAreas.Tags.Controllers
{
      [Authorize]
    public class TagsController : Controller
    {
     protected MongoModel objReals;
     protected MongoModel objReferencedb;
     protected LocationTable locationsdb;
     protected UserTable usersdb;
     protected Notifications Notificationsclass;
     protected ObjectTable _objectTable;
     protected MongoModel locationsProfilesdb ;
     protected RivkaAreas.Tags.Models.ObjectReal objectrealdb;
     protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
     protected MongoModel _locationTable;
          public TagsController(){

              objReals=new MongoModel("ObjectReal");
              locationsdb = new LocationTable();
              objReferencedb=new MongoModel("ReferenceObjects");
              Notificationsclass = new Notifications();
              usersdb = new UserTable();
              locationsProfilesdb = new MongoModel("LocationProfiles");
              objectrealdb = new RivkaAreas.Tags.Models.ObjectReal("ObjectReal");
              _logTable = new LogBook.Controllers.LogBookController();
              _locationTable = new MongoModel("Locations");
              _objectTable = new ObjectTable();
          }
        public ActionResult Index()
        {
            Dictionary<string, string> locations = new Dictionary<string, string>();
            string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
            string idprofile="";
            JObject conjuntja = new JObject();
            try
            {
                conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt).First() as JObject;
                idprofile=conjuntja["_id"].ToString();
            }
            catch
            {

            }
            try
            {
                JArray getlocatins = JsonConvert.DeserializeObject<JArray>(locationsdb.Get("profileId",idprofile));
                locations = getlocatins.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
            }
            catch
            {

            }
            ViewData["locations"] = locations;
            return View();
        }
        public ActionResult getsdf(HttpPostedFileBase sdfFile)
        {
            try
            {

                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                string filename = "";
                if (sdfFile != null)
                {
                    filename = sdfFile.FileName.ToString();

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
                string url = "";
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;
                string relativepath = @"\Uploads\Tags\" + userid;
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
                        Error.Log(e, "Cannot delete folder");
                    }
                }

                if (sdfFile != null)
                {
                    ext = sdfFile.FileName.Split('.').Last(); //getting the extension
                }
                if (sdfFile != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }

                        sdfFile.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        url = absolutepath + "\\" + patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                        sdfFile.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        url = absolutepath + "\\" + patch + "." + ext;

                    }

                    JObject datajo = new JObject();

                  //  string desingc = objReals.GetRows();
                    

                }

              List<ExpandoObject> rows = getRows(url);
              List<string> Epcs = new List<string>();
              dynamic Conjuntos = null;
              List<string> profileslist = new List<string>();
              List<string> Idslist = new List<string>();
              List<dynamic> resultepcs = new List<dynamic>();
              List<dynamic> resultids = new List<dynamic>();
                Dictionary<string, string> profubi = new Dictionary<string, string>();
                Dictionary<string, string> profsububi = new Dictionary<string, string>();
                try
              {
                  
                  JArray profiles = JsonConvert.DeserializeObject<JArray>(locationsProfilesdb.Get("name", "Conjunto"));
                  profileslist = (from prof in profiles select (string)prof["_id"]).ToList();
                  JArray conjuntresult = JsonConvert.DeserializeObject<JArray>(objectrealdb.Getby("profileId", profileslist));
                  // rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");
                  Conjuntos = (from conj in conjuntresult select new { id = (string)conj["_id"], name = (string)conj["name"], number = (string)conj["number"] }).ToList();
                    JObject profiles2 = JsonConvert.DeserializeObject<JArray>(locationsProfilesdb.Get("name", "Ubicacion")).First() as JObject;
                    profubi.Add(profiles2["_id"].ToString(), profiles2["name"].ToString());
                    JObject profiles3 = JsonConvert.DeserializeObject<JArray>(locationsProfilesdb.Get("name", "Sub-Ubicaciones")).First() as JObject;
                    profsububi.Add(profiles3["_id"].ToString(), profiles3["name"].ToString());
                }
                catch (Exception ex) { Error.Log(ex, " - - "); }
              try
              {
                  Epcs = (from row in rows.Cast<dynamic>() select (string)row.af_epc_completo).ToList();
                  JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.ValidEpcs(Epcs));
                 // rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");
                  dynamic resultepcs1 = (from objectsx in objects select new { epc = (string)objectsx["EPC"], name = (string)objectsx["name"], serie = (string)objectsx["serie"], marca = (string)objectsx["marca"], modelo = (string)objectsx["modelo"], location = (string)objectsx["location"], location_name = (string)objectsx["location_name"], conjunto = (string)objectsx["conjunto"], number = "", conjuntoname = "", profile = (string)objectsx["profileId"] }).ToList();
                 
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
                        string prof = "";
                        string conjuntx = obj.conjunto;
                     try
                        {
                            prof = obj.profile;
                            if (prof == profsububi.ElementAt(0).Key)
                            {
                                JObject conj = JsonConvert.DeserializeObject<JObject>(locationsdb.GetRow(conjuntx));
                                myobj.conjunto = conj["parent"].ToString();
                                 conjuntx = conj["parent"].ToString();

                                myobj.number = conj["number"].ToString();
                                myobj.conjuntoname= conj["name"].ToString();
                                prof = conj["profileId"].ToString();
                            }
                            if (prof== profubi.ElementAt(0).Key)
                            {
                                JObject conj = JsonConvert.DeserializeObject<JObject>(locationsdb.GetRow(conjuntx));
                               // myobj.conjunto = conj["parent"].ToString();
                                myobj.number = conj["number"].ToString();
                                myobj.conjuntoname = conj["name"].ToString();
                            }
                        }
                        catch (Exception ex) { Error.Log(ex, " - - "); }



                        resultepcs.Add(myobj);

                 }
                  if (Conjuntos != null) {
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
                                  catch (Exception ex) { Error.Log(ex, " - - "); }
                                }
                          }
                          catch
                          {

                          }
                      }
                  }
              
              }
              catch (Exception ex) { Error.Log(ex, " - - "); }
                try
              {
                  Idslist = (from row in rows.Cast<dynamic>() select (string)row.id_registro).ToList();
                  JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.ValidIds(Idslist));
                  //rows.Cast<dynamic>().Where(c => Idslist.Contains(c.id_registro)).ToList().ForEach(cc => cc.analisis = "Activo con Id de Registro ya Existente");
                  dynamic resultids1 = (from objectsx in objects select new { id = (string)objectsx["_id"], name = (string)objectsx["name"], serie = (string)objectsx["serie"], marca = (string)objectsx["marca"], modelo = (string)objectsx["modelo"], location = (string)objectsx["location"], location_name = (string)objectsx["location_name"], conjunto = (string)objectsx["conjunto"], number = "", conjuntoname = "", profile = (string)objectsx["profileId"] }).ToList();
                
                 foreach (dynamic obj in resultids1)
                 {
                     dynamic myobj = new ExpandoObject();
                     myobj.id = obj.id;
                     myobj.name = obj.name;
                     myobj.serie = obj.serie;
                     myobj.marca = obj.marca;
                     myobj.modelo = obj.modelo;
                        string prof = "";
                        try
                        {
                            prof = profubi[obj.profile];
                            if (prof.Contains("Ubicacion"))
                            {
                                JObject conj = JsonConvert.DeserializeObject<JObject>(locationsdb.GetRow(obj.conjunto));
                                obj.conjunto = conj["parent"].ToString();
                                obj.number = conj["number"].ToString();
                            }
                        }
                        catch (Exception ex) { Error.Log(ex, " - - "); }
                        myobj.location = obj.location;
                     myobj.location_name = obj.location_name;
                     myobj.conjunto = obj.conjunto;
                     myobj.number = obj.number;
                     myobj.conjuntoname = obj.conjuntoname;
                     resultids.Add(myobj);

                 }
                  if (Conjuntos != null)
                  {
                      foreach (dynamic item in resultids)
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
                                  catch (Exception ex) { Error.Log(ex, " - - "); }
                                }
                          }
                          catch
                          {

                          }
                      }
                  }
              }
              catch { }
              try
              {
                  string label = "<label style='color: orange;font-weight: bold;'>";
                  string labelend = "</label>";
                  foreach (dynamic row in rows)
                  {
                      string observacion = "";
                      if (resultepcs!=null)
                      {

                          foreach (dynamic item in resultepcs)
                          {   
                             
                              try
                              {
                                  if (item.epc == row.af_epc_completo)
                                  {
                                      if (row.id_registro == "0")
                                      {
                                          observacion = "A pesar de que no se cuenta con ID REGISTRO,el EPC existe en IOS, ";
                                          row.check = "";
                                      }
                                      else
                                      {

                                          observacion = "A pesar de que el ID de REGISTRO no existe en IOS, el activo se actualizará usando el EPC, ";
                                          row.check = "";
                                      }
                                      if (item.number == row.af_unidad_explotacion)
                                      {
                                          observacion += " el conjunto es el mismo en IOS(" + item.conjuntoname + "),";
                                          row.check = "checked";
                                      }
                                      else
                                      {
                                          observacion += " el conjunto es diferente en IOS(" + item.conjuntoname + "),";
                                          row.check = "";
                                      }
                                    /*  if (item.location == row.ub_id_ubicacion)
                                      {
                                          observacion += " y la ubicacion es igual en IOSWEB(" + item.location_name + "),";

                                      }
                                      else
                                      {
                                          observacion += "y la ubicacion es diferente en IOSWEB(" + item.location_name + "),";

                                      }*/
                                      try
                                      {
                                          
                                          if (row.af_desc_articulo != item.name)
                                          {
                                              row.af_desc_articulo = label+row.af_desc_articulo+labelend;
                                             
                                          }
                                          if (row.af_marca != item.marca)
                                          {
                                              row.af_marca = label+row.af_marca+labelend;

                                          }
                                          if (row.af_modelo != item.modelo)
                                          {
                                              row.af_modelo = label + row.af_modelo + labelend;

                                          }
                                          if (row.af_num_serie != item.serie)
                                          {
                                              row.af_num_serie = label + row.af_num_serie + labelend;

                                          }
                                          
                                      }
                                      catch { }
                                      observacion += " por lo tanto se actualizara la información";
                                      row.analisis = observacion;
                                      row.color = "border: solid 5px red;";
                                      break;

                                  }
                              }
                              catch { }
                          }
                         
                      }
                        if (resultids!=null)
                          {
                             
                              foreach (dynamic item in resultids)
                                  
                                   try {
                                        
                                      if (item.id == row.id_registro)
                                      {
                                          
                                          if (row.id_registro == "0")
                                          {
                                             
                                              continue;
                                          }
                                          observacion = "El ID UNICO existe en IOS,";
                                          row.color = "border: solid 5px red;";
                                          if (item.number == row.af_unidad_explotacion)
                                          {
                                              observacion += " el conjunto es el mismo en IOS(" + item.conjuntoname + "),";

                                              row.check = "checked";
                                              row.color = "border: solid 5px rgb(42, 180, 11);";
                                          }
                                          else
                                          {
                                              observacion += " sin embargo el conjunto es diferente en IOS(" + item.conjuntoname + "),";
                                              row.check = "";
                                             
                                          }
                                        /*  if (item.location == row.ub_id_ubicacion)
                                          {
                                              observacion += " y la ubicacion es igual en IOSWEB(" + item.location_name + "),";

                                          }
                                          else
                                          {
                                              observacion += "y la ubicacion es diferente en IOSWEB(" + item.location_name + "),";

                                          }*/
                                          try
                                          {

                                              if (row.af_desc_articulo != item.name)
                                              {
                                                  row.af_desc_articulo = label + row.af_desc_articulo + labelend;

                                              }
                                              if (row.af_marca != item.marca)
                                              {
                                                  row.af_marca = label + row.af_marca + labelend;

                                              }
                                              if (row.af_modelo != item.modelo)
                                              {
                                                  row.af_modelo = label + row.af_modelo + labelend;

                                              }
                                              if (row.af_num_serie != item.serie)
                                              {
                                                  row.af_num_serie = label + row.af_num_serie + labelend;

                                              }

                                          }
                                          catch { }
                                          observacion += " por lo tanto se actualizara la información";
                                          row.analisis = observacion;
                                         
                                          break;

                                      }
                                  }catch{ }
                         }
                          if(observacion=="")
                          {
                              row.analisis = "Se creará un nuevo activo en IOS";
                              row.color = "border: solid 5px rgb(42, 180, 11);";
                              row.check = "checked";
                          }
                  }
              }
              catch { }
                ViewData["url"] = url;
                ViewData["rows"] = rows;
               

                return View();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(404, ex.Message);
            }

        }
        public ActionResult setSdf2(HttpPostedFileBase sdfFile){
            try
            {
               
                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                string filename = "";
                if (sdfFile != null)
                {
                    filename = sdfFile.FileName.ToString();

                }
                string userid = "";
                try
                {
                   userid = Session["_id"].ToString();
                }
                catch(Exception e)
                {
                    userid = Request.Cookies["_id2"].Value;
                }

                string ext = null;
                string patch = "";
                string url = "";
                List<string> epcList=new List<string>();
                List<string> epcList1 = new List<string>();
                List<string> idslist = new List<string>();
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;
                string relativepath = @"\Uploads\Tags\" + userid;
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
                        Error.Log(e, "Cannot delete folder");
                    }
                }

                if (sdfFile != null)
                {
                    ext = sdfFile.FileName.Split('.').Last(); //getting the extension
                }
                if (sdfFile != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }

                        sdfFile.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        url = absolutepath + "\\" + patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                       sdfFile.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        url = absolutepath + "\\" + patch + "." + ext;

                    }

                JObject datajo = new JObject();
               
                string desingc = objReals.GetRows();
                 try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                    foreach (JObject row in rows)
                    {
                        try{
                        if(row["EPC"].ToString()!="" && row["EPC"].ToString()!=null){
                            epcList.Add(row["EPC"].ToString());
                            idslist.Add("'"+row["_id"].ToString()+"'");
                            string completeList = row["EPC"].ToString();
                            try
                            {
                                completeList += "|" + row["location"].ToString();
                            }
                            catch(Exception e){
                                completeList += "|";
                            }

                            try
                            {
                                completeList += "|" + row["userid"].ToString();
                            }
                            catch (Exception e)
                            {
                                completeList += "|";
                            }


                            epcList1.Add(completeList);

                        }
                        }catch(Exception ex){}
                    }

                  
                }catch(Exception ex){}

                }

                 List<List<string>> newEpcs = new List<List<string>>();
                 List<List<string>> DuplicateEpcs = new List<List<string>>();
                 List<List<string>> duplicateids = new List<List<string>>();
                 newEpcs= getEpcsnews(url,epcList,idslist);
                 duplicateids = getEpcsnews(url, epcList, idslist, 1);
                 DuplicateEpcs = getEpcsDuplicate(url, epcList1);
                 ViewData["url"] = url;
                 ViewData["newsEpc"] = newEpcs;
                 ViewData["duplicateids"] = duplicateids;
                 ViewData["duplicateEpc"] = DuplicateEpcs;
                
                 return View();
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public JsonResult getNodeContent(String parentCategory)
        {
            if (parentCategory == "") parentCategory = "null";
            String categoriesString = locationsdb.get("parent", parentCategory);

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
        public ActionResult generateObjs2(string url, string selects)
        {


            try
            {
                List<string> listselects = new List<string>();
                List<dynamic> newObjs = new List<dynamic>();
                List<dynamic> editEpcs = new List<dynamic>();
                List<dynamic> editIds = new List<dynamic>();
                try
                {
                    JArray selectsact = JsonConvert.DeserializeObject<JArray>(selects);
                    listselects = (from selects1 in selectsact select (string)selects1).ToList();

                }
                catch { }
                JObject datajo = new JObject();

                List<ExpandoObject> rows = getRows(url);
                List<string> Epcs = new List<string>();
                List<string> Idslist = new List<string>();
                try
                {
                    Epcs = (from row in rows.Cast<dynamic>() select (string)row.af_epc_completo).ToList();
                    JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.ValidEpcs(Epcs));
                    Epcs.Clear();
                    Epcs = (from obj in objects select (string)obj["EPC"]).ToList();
                    //  rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");

                }
                catch { }
                try
                {
                    Idslist = (from row in rows.Cast<dynamic>() select (string)row.id_registro).ToList();
                    JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.ValidIds(Idslist));
                    Idslist.Clear();
                    Idslist = (from obj in objects select (string)obj["_id"]).ToList();
                    // rows.Cast<dynamic>().Where(c => Idslist.Contains(c.id_registro)).ToList().ForEach(cc => cc.analisis = "Activo con Id de Registro ya Existente");

                }
                catch { }
               
                JArray locationslist = new JArray();
                try
                {
                    locationslist = JsonConvert.DeserializeObject<JArray>(locationsdb.GetRows());
                }
                catch { }
                
               try
                {
                  
                      int countobjs = 0;
                foreach(dynamic row in rows)
                  {
                       if(listselects.Contains(countobjs.ToString()))
                        {
                            try
                            {
                               
                                string heads1 = "";
                                string nameobjs = row.af_desc_articulo;
                                string locations = row.ub_id_ubicacion;
                                string sublocations = row.ub_id_sububicacion;
                                string epcx = row.af_epc_completo;
                                string referenceobj = row.af_id_articulo;
                                string numserie = row.af_num_serie;
                                string conjunto = row.af_unidad_explotacion;
                                string namelocation = row.af_ubicacion;
                                string namesublocation = row.af_sububicacion;
                                JObject objectJo = new JObject();
                                if (locations.Length < 23)
                                {
                                    try
                                    {
                                        JObject locationjo2 = JsonConvert.DeserializeObject<JArray>(locationsdb.Get("number", locations)).First() as JObject;
                                        locations = locationjo2["_id"].ToString();
                                    }
                                    catch { }
                                }

                                if (sublocations.Length < 23)
                                {
                                    try
                                    {
                                        JObject sublocationsjo = JsonConvert.DeserializeObject<JArray>(locationsdb.Get("number", sublocations)).First() as JObject;
                                        sublocations = sublocationsjo["_id"].ToString();
                                    }
                                    catch { }
                                }
                                string assettype = "";
                                try
                                {
                                    JObject refobj = JsonConvert.DeserializeObject<JObject>(objReferencedb.GetRow(referenceobj));
                                    assettype = refobj["assetType"].ToString();

                                }
                                catch
                                {

                                }
                                try
                                {
                                    string locationf = "";
                                    string conjuntoid ="";
                                    string sublocationf = "";
                                    try
                                    {
                                        conjuntoid = (from conj in locationslist where (string)conj["number"] == conjunto select (string)conj["_id"]).First().ToString();
                                    }
                                    catch {
                                        locationf = "";
                                    }
                                       try
                                    {
                                       // JArray getlocations = JsonConvert.DeserializeObject<JArray>(locationsdb.Get("parent", conjuntoid));
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
                                        locationf = (from locs in getlocations where (string)locs["_id"] == locations select (string)locs["_id"]).First().ToString();
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

                                            try
                                            {
                                                String idslocs = locationsProfilesdb.Get("name", "Ubicacion");
                                                JObject idprof = JsonConvert.DeserializeObject<JArray>(idslocs).First() as JObject;
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
                                    try
                                    {
                                        // JArray getlocations = JsonConvert.DeserializeObject<JArray>(locationsdb.Get("parent", conjuntoid));
                                        JArray getlocations = new JArray();
                                        if (locations != "null")
                                        {
                                            foreach (JObject loc in locationslist)
                                            {
                                                try
                                                {
                                                    if (loc["parent"].ToString() == locations)
                                                    {
                                                        getlocations.Add(loc);
                                                    }
                                                }
                                                catch { }
                                            }
                                            sublocationf = (from locs in getlocations where (string)locs["_id"] == sublocations select (string)locs["_id"]).First().ToString();

                                        } else
                                        {
                                            sublocationf = "null";
                                        }
                                   }
                                    catch
                                    {

                                    }
                                    if (sublocations == "")
                                    {
                                        sublocations = locations;
                                    }
                                    else
                                    if (sublocationf == "" && (locations != "" && locations!="null"))
                                    {
                                        try
                                        {
                                            JObject newlocation = new JObject();
                                            newlocation.Add("name", namesublocation);
                                            newlocation.Add("parent", locations);
                                            newlocation.Add("number", "");
                                            newlocation.Add("tipo", "1");
                                            try
                                            {
                                                String idslocs = locationsProfilesdb.Get("name", "Sub-Ubicaciones");
                                                JObject idprof = JsonConvert.DeserializeObject<JArray>(idslocs).First() as JObject;
                                                newlocation.Add("profileId", idprof["_id"].ToString());

                                            }
                                            catch { }
                                            string idlocnew = locationsdb.SaveRow(JsonConvert.SerializeObject(newlocation), sublocations);
                                            newlocation.Add("_id", idlocnew);
                                            locationslist.Add(newlocation);
                                            sublocationf = (idlocnew != null && idlocnew != "") ? idlocnew : "";

                                        }
                                        catch
                                        {

                                        }
                                        sublocations = (sublocationf != "") ? sublocationf : "null";
                                    }
                                }
                                catch { }
                              
                                objectJo.Add("objectReference", referenceobj);
                                objectJo.Add("name", nameobjs);
                                objectJo.Add("location", sublocations);
                                objectJo.Add("EPC", epcx);
                                objectJo.Add("serie", numserie);
                                objectJo.Add("assetType", assettype);
                                objectJo.Add("label", "normal");
                                string item = JsonConvert.SerializeObject(objectJo);
                                string idr="";
                                string idregis=row.id_registro;
                                if ((Epcs.Contains(row.af_epc_completo) && row.af_epc_completo != "") && (!Idslist.Contains(row.id_registro)))
                                {
                                    try
                                    {
                                        objectJo.Add("system_status", true);
                                        item = JsonConvert.SerializeObject(objectJo);
                                        String objsresult = objReals.Get("EPC", row.af_epc_completo);
                                        JObject objedit = JsonConvert.DeserializeObject<JArray>(objsresult).First() as JObject;
                                        idr = objReals.SaveRow(item, objedit["_id"].ToString());
                                        editEpcs.Add(row);
                                        if(epcx!="")
                                          Epcs.Add(epcx);
                                        if(idregis.Count()>2)
                                         Idslist.Add(row.id_registro);
                                    }
                                    catch { }
                                }
                                else if (Idslist.Contains(row.id_registro) && row.id_registro!="")
                                {
                                    try
                                    {
                                        objectJo.Add("system_status", true);
                                        item = JsonConvert.SerializeObject(objectJo);
                                        JObject objedit = JsonConvert.DeserializeObject<JObject>(objReals.GetRow(row.id_registro));
                                        idr = objReals.SaveRow(item, objedit["_id"].ToString());
                                        editIds.Add(row);
                                        if (epcx != "")
                                            Epcs.Add(epcx);
                                        if (idregis.Count() > 2)
                                            Idslist.Add(row.id_registro);
                                    }
                                    catch { }
                                }
                                else
                                {
                                    try
                                    {
                                        idr = objReals.SaveRow(item);
                                        newObjs.Add(row);
                                        if (epcx != "")
                                            Epcs.Add(epcx);
                                        if (idregis.Count() > 2)
                                            Idslist.Add(row.id_registro);
                                 
                                    }
                                    catch { }
                                }
                              
                                bool ok = true;
                                try
                                {
                                    ok = RulesChecker.isValidToLocation(referenceobj, locations);
                                    if (ok == false)
                                    {
                                        Notificationsclass.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicacion no valida");
                                        // return "problem";
                                    }
                                }
                                catch { }
                                
                            }
                            catch { }
                        }
                            countobjs++;
                        }

                        Notificationsclass.saveNotification("Objects", "Create", "Se han creado " + countobjs + " Activos");
                        try
                        {
                            _logTable.SaveLog(Session["_id"].ToString(), "Etiquetado", "Insert: Se han creado " + countobjs + " activos", "ObjectReal", DateTime.Now.ToString());
                        }
                        catch { }


                }
                catch (Exception ex)
                {

                }

              
                //  modifyObjs(url);
               ViewData["news"] = newObjs;
               ViewData["epcs"] = editEpcs;
               ViewData["ids"] = editIds;
               return View();



            }
            catch (Exception ex)
            {

                return null;
            }

        }
        public string generateObjs(string url,string selects)
        {
            try
            {
                List<string> listselects = new List<string>();
                try
                {
                    JArray selectsact = JsonConvert.DeserializeObject<JArray>(selects);
                    listselects = (from selects1 in selectsact select (string)selects1).ToList();

                }
                catch { }
                JObject datajo = new JObject();
                List<string> epclist = new List<string>();
                string desingc = objReals.GetRows();
                try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                    foreach (JObject row in rows)
                    {
                        try
                        {
                            if (row["EPC"].ToString() != "" && row["EPC"].ToString() != null)
                            {

                                epclist.Add(row["EPC"].ToString());

                            }
                        }
                        catch (Exception ex) { }
                    }


                }
                catch (Exception ex) { }
                // read sdf
                string conection = @"Data Source = " + url + ";";
                String Epcs = null;
                JArray locationslist = new JArray();
                try
                {
                    locationslist = JsonConvert.DeserializeObject<JArray>(locationsdb.GetRows());
                }
                catch { }
                foreach (string epc in epclist)
                {
                    if (epc == epclist.First().ToString())
                    {
                        Epcs = "'" + epc + "'";
                    }
                    else
                    {

                        Epcs += ",'" + epc + "'";
                    }

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
                SqlCeCommand VComandoSQL2 = vCon.CreateCommand();
                SqlCeDataReader rdr = null;
                SqlCeDataReader rdr2 = null;

                string nametable = "";
                try
                {
                    vCon.Open();

                    VComandoSQL2.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE'";
                    rdr2 = VComandoSQL2.ExecuteReader();
                    //String nametable = "";
                    while (rdr2.Read())
                    {
                        // validate name table from sdf file
                        if (rdr2.GetString(0) == "htk_Catalogo_Activos_Etiquetado")
                        {
                            nametable = rdr2.GetString(0);

                        }
                    }

                    if (nametable != "")
                    {
                        string query = "SELECT * FROM " + nametable ;
                       
                        VComandoSQL.CommandText = query;
                        rdr = VComandoSQL.ExecuteReader();
                        int countobjs = 0;
                       while (rdr.Read())
                        {
                               try{
                            string heads1 = "";
                            string nameobjs = "";
                            string locations = "null";
                            string epcx = "";
                            string referenceobj = "";
                            string numserie = "";
                            string conjunto = "";
                            string namelocation = "";
                            JObject objectJo = new JObject();
                       
                           
                                for (int i = 0; i < rdr.FieldCount; i++)
                                {
                                    heads1=rdr.GetName(i);

                                    switch (heads1)
                                    {
                                        case "AF_DESC_ARTICULO":

                                            nameobjs = rdr[i].ToString();
                                            break;
                                        case "UB_ID_UBICACION":
                                            locations = rdr[i].ToString();
                                            if (locations == "")
                                                locations = "null";
                                            break;
                                        case "AF_EPC_COMPLETO":
                                           epcx = rdr[i].ToString();
                                           break;
                                        case "AF_UBICACION":
                                           namelocation= rdr[i].ToString();
                                           break;
                                        case "AF_ID_ARTICULO":
                                           referenceobj = rdr[i].ToString();
                                           break;
                                        case "AF_UNIDAD_EXPLOTACION":
                                           conjunto = rdr[i].ToString();
                                           break;
                                        case "AF_NUM_SERIE":
                                           numserie = rdr[i].ToString();
                                           break;
                                    }
                                   
                                }
                                string assettype = "";
                                try
                                {
                                    JObject refobj = JsonConvert.DeserializeObject<JObject>(objReferencedb.GetRow(referenceobj));
                                    assettype = refobj["assetType"].ToString();

                                }
                                catch
                                {

                                }
                                try
                                {
                                    string locationf = "";
                                    string conjuntoid = (from conj in locationslist where (string)conj["number"] == conjunto select (string)conj["_id"]).First().ToString();
                                     try
                                    {
                                        JArray getlocations = JsonConvert.DeserializeObject<JArray>(locationsdb.Get("parent", conjuntoid));
                                   
                                        locationf = (from locs in getlocations where (string)locs["number"] == locations select (string)locs["_id"]).First().ToString();
                                    }
                                    catch
                                    {

                                    }
                                    if (locationf == "" && conjuntoid!="")
                                    {
                                        try
                                        {
                                            JObject newlocation = new JObject();
                                            newlocation.Add("name", namelocation);
                                            newlocation.Add("parent", conjuntoid);
                                            newlocation.Add("number", locations);
                                          
                                            try
                                            {
                                                String idslocs = locationsProfilesdb.Get("name", "Ubicacion");
                                                JObject idprof = JsonConvert.DeserializeObject<JArray>(idslocs).First() as JObject;
                                                newlocation.Add("profileId", idprof["_id"].ToString());
                                            
                                            }
                                            catch { }
                                            string idlocnew = locationsdb.SaveRow(JsonConvert.SerializeObject(newlocation));
                                            locationf = (idlocnew != null && idlocnew != "") ? idlocnew : "";
                                        
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    locations = (locationf != "") ? locationf :"null";
                                }
                                catch { }
                                objectJo.Add("objectReference", referenceobj);
                                objectJo.Add("name", nameobjs);
                                objectJo.Add("location", locations);
                                objectJo.Add("EPC", epcx);
                                objectJo.Add("serie", numserie);
                                objectJo.Add("assetType", assettype);
                                objectJo.Add("label", "normal");
                                string item = JsonConvert.SerializeObject(objectJo);


                                string idr = objReals.SaveRow(item, "");
                                bool ok = true;
                                try
                                {
                                    ok = RulesChecker.isValidToLocation(referenceobj, locations);
                                    if (ok == false)
                                    {
                                        Notificationsclass.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicacion no valida");
                                        // return "problem";
                                    }
                                }
                                catch { }
                                countobjs++;
                           }catch{  }
                        }

                           Notificationsclass.saveNotification("Objects", "Create", "Se han creado " + countobjs + " Activos");
                           try
                           {
                               _logTable.SaveLog(Session["_id"].ToString(), "Etiquetado", "Insert: Se han creado " + countobjs + " activos", "ObjectReal", DateTime.Now.ToString());
                           }
                           catch { }
                    }
                    else
                    {

                    }


                }
                catch (Exception ex)
                {

                }

                rdr.Close();
                rdr.Dispose();
                rdr2.Close();
                rdr2.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;
              //  modifyObjs(url);
                return "success";

               

            }
            catch (Exception ex)
            {

                return "error";
            }
            
        }     
        //MODIFY OBJECTS FROM SDF   
        public string modifyObjs(string url) {
            try
            {
                JObject datajo = new JObject();
                List<string> epclist = new List<string>();
                string desingc = objReals.GetRows();
                try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                    foreach (JObject row in rows)
                    {
                        try
                        {
                            if (row["EPC"].ToString() != "" && row["EPC"].ToString() != null)
                            {
                                string completeList = row["EPC"].ToString();
                                try
                                {
                                    completeList += "|" + row["location"].ToString();
                                }
                                catch (Exception e)
                                {
                                    completeList += "|";
                                }

                                try
                                {
                                    completeList += "|" + row["userid"].ToString();
                                }
                                catch (Exception e)
                                {
                                    completeList += "|";
                                }
                                epclist.Add(completeList);

                            }
                        }
                        catch (Exception ex) { }
                    }


                }
                catch (Exception ex) { }
                // read sdf
                string conection = @"Data Source = " + url + ";";
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
                SqlCeCommand VComandoSQL2 = vCon.CreateCommand();
                SqlCeCommand VComandoSQL3 = vCon.CreateCommand();
                SqlCeDataReader rdr = null;
                SqlCeDataReader rdr2 = null;
                SqlCeResultSet rs = null;
                SqlCeUpdatableRecord rec = null;

                //============================================================
                try
                {
                    vCon.Open();
                    VComandoSQL3.CommandText = "DROP TABLE htk_tabletemp";
                    VComandoSQL3.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                }
                VComandoSQL3.CommandText = @"CREATE TABLE [htk_tabletemp] (
	                    EPC NVARCHAR(25) NULL,
	                    ID_UBICACION NVARCHAR(24) NULL,
	                    ID_USUARIO NVARCHAR(24) NULL)";
                VComandoSQL3.ExecuteNonQuery();
                VComandoSQL3.CommandText = "htk_tabletemp";
                VComandoSQL3.CommandType = CommandType.TableDirect;

                rs = VComandoSQL3.ExecuteResultSet(ResultSetOptions.Updatable);
                rec = rs.CreateRecord();
                foreach (string epc in epclist)
                {
                    string[] eps = epc.Split('|');
                    rec.SetValue(0, eps[0]);
                    if (eps.Length > 1)
                        rec.SetValue(1, eps[1]);
                    else
                        rec.SetValue(1, "");
                    if (eps.Length > 2)
                        rec.SetValue(2, eps[2]);
                    else
                        rec.SetValue(2, "");
                    rs.Insert(rec);
                }
                //==========================================================
                string nametable = "";
                try
                {
                    VComandoSQL2.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE'";
                    rdr2 = VComandoSQL2.ExecuteReader();
                    //String nametable = "";
                    while (rdr2.Read())
                    {
                        // validate name table from sdf file
                        if (rdr2.GetString(0) == "htk_Catalogo_Activos_Etiquetado")
                        {
                            nametable = rdr2.GetString(0);

                        }
                    }

                    if (nametable != "")
                    {
                        string query = "SELECT   T1.*, T1.ID_USUARIO AS NUEVO_USUARIO FROM " + nametable + " T1 INNER JOIN htk_tabletemp T2 ON AF_EPC_COMPLETO=EPC WHERE UB_ID_UBICACION<>ID_UBICACION OR T1.ID_USUARIO<>T2.ID_USUARIO";
                        
                        VComandoSQL.CommandText = query;
                        rdr = VComandoSQL.ExecuteReader();
                        int countobjs = 0;
                        while (rdr.Read())
                        {
                            string heads1 = "";
                            string nameobjs = "";
                            string locations = "null";
                            string epcx = "";
                            string usuario = "";
                            JObject objectJo = new JObject();


                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                heads1 = rdr.GetName(i);

                                switch (heads1)
                                {
                                    case "AF_DESC_ARTICULO":

                                        nameobjs = rdr[i].ToString();
                                        break;
                                    case "UB_ID_UBICACION":
                                        locations = rdr[i].ToString();
                                        if (locations == "")
                                            locations = "null";
                                        break;
                                    case "AF_EPC_COMPLETO":
                                        epcx = rdr[i].ToString();
                                        break;
                                    case "NUEVO_USUARIO":
                                        usuario = rdr[i].ToString();
                                        if (usuario == "")
                                            usuario = "null";
                                        break;
                                }

                            }

                            string objarray = objReals.Get("EPC", epcx);
                            JArray objs = JsonConvert.DeserializeObject<JArray>(objarray);

                            foreach (JObject i in objs) {
                                objectJo = i;
                            }
                            

                            objectJo["location"]= locations;
                            objectJo["userid"] = usuario;

                            string item = JsonConvert.SerializeObject(objectJo);


                            string idr = objReals.SaveRow(item, objectJo["_id"].ToString());
                            bool ok = true;
                            ok = RulesChecker.isValidToLocation(objectJo["objectReference"].ToString(), locations);
                            if (ok == false)
                            {
                                Notificationsclass.saveNotification("Rules", "Invalid", "Objetos se han movido a Ubicacion no valida");
                                // return "problem";
                            }

                            countobjs++;

                        }

                        Notificationsclass.saveNotification("Objects", "Create", "Se han creado " + countobjs + " Activos");
                        try
                        {
                            _logTable.SaveLog(Session["_id"].ToString(), "Etiquetado", "Insert: Se han creado " + countobjs + " activos", "ObjectReal", DateTime.Now.ToString());
                        }
                        catch { }
                    }
                    else
                    {

                    }


                }
                catch (Exception ex)
                {

                }

                rs.Close();
                rs.Dispose();
                rdr.Close();
                rdr.Dispose();
                rdr2.Close();
                rdr2.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;

                return "success";



            }
            catch (Exception ex)
            {

                return "error";
            }
        }
        //Read SDF
        public List<ExpandoObject> getRows(string url)
        {
            List<ExpandoObject> myobjlist = new List<ExpandoObject>();
            try
            {
               
                // read sdf
                string conection = @"Data Source = " + url + ";";
               
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
                SqlCeCommand VComandoSQL2 = vCon.CreateCommand();
                SqlCeDataReader rdr = null;
                SqlCeDataReader rdr2 = null;

                string nametable = "";
                try
                {
                    vCon.Open();

                    VComandoSQL2.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE'";
                    rdr2 = VComandoSQL2.ExecuteReader();
                    //String nametable = "";
                    while (rdr2.Read())
                    {
                        // validate name table from sdf file
                        if (rdr2.GetString(0) == "htk_Catalogo_Activos_Etiquetado")
                        {
                            nametable = rdr2.GetString(0);

                        }
                    }

                    if (nametable != "")
                    {
                        string query = "SELECT * FROM " + nametable + " order by ID_REGISTRO";
                        
                        VComandoSQL.CommandText = query;
                        rdr = VComandoSQL.ExecuteReader();

                        int colname = 0;
                 
                      
                        try
                        {
                           // var obj = (from rows in rdr.Cast<IDataRecord>() select new { name = rows.GetString(0) }).ToList();

                        }
                        catch { }
                        while (rdr.Read())
                        {
                            List<string> column = new List<string>();
                            List<string> heads1 = new List<string>();
                            dynamic myobj = CatalogoModel();
                            
                            if (colname == 0)
                            {
                                for (int i = 0; i < rdr.FieldCount; i++)
                                {
                                    heads1.Add(rdr.GetName(i));


                                }
                                colname++;
                               

                            }


                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                column.Add(rdr[i].ToString());
                                try
                                {
                                    ((IDictionary<string, object>)myobj)[rdr.GetName(i).ToLower()] = rdr[i].ToString();

                                }
                                catch
                                {

                                }
                            }
                            myobjlist.Add(myobj);
                       

                        }


                    }
                    else
                    {

                    }


                }
                catch (Exception ex)
                {

                }

                rdr.Close();
                rdr.Dispose();
                rdr2.Close();
                rdr2.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;

                return myobjlist;

                //end read sdf

            }
            catch (Exception ex)
            {

                return myobjlist;
            }
        }
        //Asset modeling
        public ExpandoObject CatalogoModel()
        {
            dynamic model = new ExpandoObject();
            model.id_registro="";
            model.af_unidad_explotacion="";
            model.af_nombre_conjunto="";
            model.af_departamento="";
            model.af_ubicacion="";
            model.af_id_articulo="";
            model.af_desc_articulo="";
            model.af_marca="";
            model.af_modelo="";
            model.af_num_serie="";
            model.af_epc_completo="";
            model.af_status_etiquetado="";
            model.af_fecha_etiquetado="";
            model.af_usuario_etiquetado="";
            model.af_cantidad="";
            model.af_id_paquete="";
            model.ub_id_ubicacion="";
            model.af_sububicacion = "";
            model.ub_id_sububicacion = "";
            model.id_usuario="";
            model.analisis = "";
            model.color = "";
            model.check = "";
	       return model;
        }

            public List<List<string>> getEpcsnews(string url,List<string> epcslist,List<string> idslist,int exists=0){


                 List<List<string>> listdata = new List<List<string>>();
                 String ids = "";
                try{
                    ids = String.Join(",", idslist);
                     // read sdf
                   string  conection = @"Data Source = " + url +";";
                   String Epcs=null;    
                     foreach(string epc in epcslist){
                         if(epc==epcslist.First().ToString()){
                             Epcs="'"+epc+"'";
                         }else{

                             Epcs+=",'"+epc+"'";
                         }

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
                     SqlCeCommand VComandoSQL2 = vCon.CreateCommand();
                     SqlCeDataReader rdr = null;
                     SqlCeDataReader rdr2 = null;
                    
                     string nametable = "";
                     try
                     {
                         vCon.Open();

                         VComandoSQL2.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE'";
                         rdr2 = VComandoSQL2.ExecuteReader();
                         //String nametable = "";
                          while (rdr2.Read())
                         {
                             // validate name table from sdf file
                             if (rdr2.GetString(0) == "htk_Catalogo_Activos_Etiquetado")
                             {
                                 nametable=rdr2.GetString(0);
                               
                             }
                         }

                         if (nametable != "")
                         {
                             string query = "SELECT * FROM " + nametable + " WHERE AF_EPC_COMPLETO NOT IN (" + Epcs + ") AND ID_REGISTRO NOT IN("+ids+")";
                             if (exists == 1)
                             {
                                 query = "SELECT * FROM " + nametable + " WHERE AF_EPC_COMPLETO NOT IN (" + Epcs + ") AND ID_REGISTRO  IN(" + ids + ")";
                             }
                             if (Epcs == null)
                             {
                                 query="SELECT * FROM " + nametable;
                             }
                             VComandoSQL.CommandText = query;
                             rdr = VComandoSQL.ExecuteReader();
                        
                              int colname = 0;
                             
                              

                             

                            //  prop.SetValue(myobj, propertyValue, null);

                              List<dynamic> myobjlist = new List<dynamic>();
                                while (rdr.Read())
                                {
                                    List<string> column = new List<string>();
                                    List<string> heads1 = new List<string>();
                                    dynamic myobj = new ExpandoObject() ;
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
                                        try
                                        {
                                            ((IDictionary<string, object>)myobj)[rdr.GetName(i)] = rdr[i].ToString();
                                           
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    myobjlist.Add(myobj);
                                    listdata.Add(column);
                                    /* String hlp = rdr.GetString(0);
                                     String hlp2 = rdr.GetString(1);*/

                                  }

               
                         }
                         else
                         {
                             
                         }


                     }
                     catch (Exception ex)
                     {

                     }
                    
                rdr.Close();
                rdr.Dispose();
                rdr2.Close();
                rdr2.Dispose();
                vCon.Close();
                vCon.Dispose();
                vCon = null;

                return listdata;
                    
                    //end read sdf

                }catch(Exception ex){

                    return listdata;
                }
            }

            public List<List<string>> getEpcsDuplicate(string url, List<string> epcslist)
            {


                List<List<string>> listdata = new List<List<string>>();

                try
                {
                    // read sdf
                    string conection = @"Data Source = " + url + ";";

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
                    SqlCeCommand VComandoSQL2 = vCon.CreateCommand();
                    SqlCeCommand VComandoSQL3 = vCon.CreateCommand();
                    SqlCeResultSet rs = null;
                    SqlCeDataReader rdr = null;
                    SqlCeDataReader rdr2 = null;
                    SqlCeUpdatableRecord rec = null;

                    string nametable = "";
                    //============================================================
                    try
                    {
                        vCon.Open();
                        VComandoSQL3.CommandText = "DROP TABLE htk_tabletemp";
                        VComandoSQL3.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {

                    }
                    VComandoSQL3.CommandText = @"CREATE TABLE [htk_tabletemp] (
	                    EPC NVARCHAR(25) NULL,
	                    ID_UBICACION NVARCHAR(24) NULL,
	                    ID_USUARIO NVARCHAR(24) NULL)";
                    VComandoSQL3.ExecuteNonQuery();
                    VComandoSQL3.CommandText = "htk_tabletemp";
                    VComandoSQL3.CommandType = CommandType.TableDirect;

                    rs = VComandoSQL3.ExecuteResultSet(ResultSetOptions.Updatable);
                    rec = rs.CreateRecord();
                    foreach (string epc in epcslist)
                    {
                        string[]eps=epc.Split('|');
                        rec.SetValue(0, eps[0]);
                        if(eps.Length>1)
                            rec.SetValue(1, eps[1]);
                        else
                            rec.SetValue(1, "");
                        if (eps.Length > 2)
                            rec.SetValue(2, eps[2]);
                        else
                            rec.SetValue(2, "");
                        rs.Insert(rec);
                    }
                    //==========================================================
                    try
                    {
                       
                       
                        VComandoSQL2.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE'";
                        rdr2 = VComandoSQL2.ExecuteReader();
                        //String nametable = "";
                        while (rdr2.Read())
                        {
                            // validate name table from sdf file
                            if (rdr2.GetString(0) == "htk_Catalogo_Activos_Etiquetado")
                            {
                                nametable = rdr2.GetString(0);

                            }
                        }

                        if (nametable != "")
                        {
                            VComandoSQL.CommandText = "SELECT   T1.* FROM " + nametable + " T1 INNER JOIN htk_tabletemp T2 ON AF_EPC_COMPLETO=EPC"; //WHERE UB_ID_UBICACION<>ID_UBICACION OR T1.ID_USUARIO<>T2.ID_USUARIO"; // 
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


                        }
                        else
                        {

                        }
                        rs.Close();
                        rs.Dispose();
                        rdr.Close();
                        rdr.Dispose();
                        rdr2.Close();
                        rdr2.Dispose();
                        vCon.Close();
                        vCon.Dispose();
                        vCon = null;

                    }
                    
                    catch (Exception ex)
                    {

                    }

                   

                    return listdata;

                    //end read sdf

                }
                catch (Exception ex)
                {

                    return listdata;
                }
            }

            public JsonResult generateSdfs(String iduser, String idlocation)
            {
                try
                {
                    String relativepath = "\\Uploads\\Tags\\" + iduser + "\\download\\" + iduser + "\\";
                    String absolutepath = Server.MapPath(relativepath);

                    //  JObject datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);

                    //Create if not exist the directory
                    try
                    {
                        if (System.IO.Directory.Exists(absolutepath))
                            System.IO.Directory.Delete(absolutepath, true);
                        if (!System.IO.Directory.Exists(absolutepath))
                            System.IO.Directory.CreateDirectory(absolutepath);
                    }
                    catch (Exception ex) { }
                    createCatalog(iduser,idlocation);
                    createLocationDB(idlocation, iduser);
                    createSubLocationDB(idlocation, iduser);
                    createConjuntoDB(idlocation, iduser);
                    createUserDB( iduser);
                    createReference(iduser);
                    createInventory(iduser);
                    string pathx = Server.MapPath(@"\Uploads\Tags\" + iduser + @"\download\" + iduser + ".rar"); //get physical file path from server
                    string startPath = Server.MapPath(@"\Uploads\Tags\" + iduser + @"\download\" + iduser);
                    string name = Path.GetFileName(pathx); //get file name
                    string zipPath = pathx;
                    FileInfo file = new FileInfo(pathx);

                    if (System.IO.File.Exists(zipPath))
                        System.IO.File.Delete(zipPath);

                    ZipFile.CreateFromDirectory(startPath, zipPath);

                    String result = "{'type':'compress','url':'/Uploads/Tags/" + iduser + "/download/','data':'" + name + "'}";
                    JObject resultObj = JsonConvert.DeserializeObject<JObject>(result);
                    return Json(JsonConvert.SerializeObject(resultObj));
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            public void createCatalog(String iduser,String idlocation)
            {
                //check if the path exist
                String relativepath = "\\Uploads\\Tags\\" + iduser+ "\\download\\" + iduser + "\\";
                String absolutepath = Server.MapPath(relativepath);

              //  JObject datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);

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
                string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
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
                conjuntos = locatList.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                conjuntosdict = locatList.ToDictionary(x => (string)x["_id"], x => (JObject)x);
                uexplotacion = locatList.ToDictionary(x => (string)x["_id"], x => (string)x["number"]);
                String rowArray2 = _locationTable.Get("profileId", idprof2);
                JArray locatList2 = JsonConvert.DeserializeObject<JArray>(rowArray2);
                ubicacionesdict = locatList2.ToDictionary(x => (string)x["_id"], x => (JObject)x);
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
	                            AF_NOMBRE_CONJUNTO NVARCHAR(50) NULL,
	                            AF_DEPARTAMENTO NVARCHAR(50) NULL,
	                            AF_UBICACION NVARCHAR(100) NULL,
	                            AF_ID_ARTICULO NVARCHAR(24) NULL,
	                            AF_DESC_ARTICULO NVARCHAR(100) NULL,
	                            AF_MARCA NVARCHAR(30) NULL,
	                            AF_MODELO NVARCHAR(30) NULL,
	                            AF_NUM_SERIE NVARCHAR(30) NULL,
	                            AF_EPC_COMPLETO NVARCHAR(30) PRIMARY KEY NOT NULL,
	                            AF_STATUS_ETIQUETADO NVARCHAR(50) NULL,
	                            AF_FECHA_ETIQUETADO NVARCHAR(20) NULL,
	                            AF_USUARIO_ETIQUETADO NVARCHAR(50) NULL,
	                            AF_CANTIDAD INT NULL,
	                            AF_ID_PAQUETE BIT NULL,
                                UB_ID_UBICACION NVARCHAR(25) NULL,
                                AF_SUBUBICACION NVARCHAR(50) NULL,
                                UB_ID_SUBUBICACION NVARCHAR(25) NULL,
                                ID_USUARIO NVARCHAR(25) NULL,
                                AF_ULTIMO_MANT NVARCHAR(25) NULL,
                                AF_PROX_MANT NVARCHAR(25) NULL,
                                AF_COM_MANT NVARCHAR(250) NULL)";

                    string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                    string exe = Server.MapPath(pathexe);
                    string fields = "'_id','objectReference','objectReference_name','EPC','serie','location','location_name','marca','modelo','Creator','number','conjuntoName','department','label','date','quantity','sublocationname','sublocation','lastmaintenance','nextmaintenance','comments'";
                    string values = "'0','5','6','10','9','16','4','7','8','19','1','2','3','11','12','14','17','18','20','21','22'";
                   

                    String idLocation =idlocation;
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
                        try
                        {

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
                                try
                                {
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
                        //'lastmaintenance','nextmaintenance','comments'";
                            if (!row.TryGetValue("lastmaintenance", out jtoken))
                            {
                                row.Add("lastmaintenance", "");
                            }
                            if (!row.TryGetValue("nextmaintenance", out jtoken))
                            {
                                row.Add("nextmaintenance", "");
                            }
                            if (!row.TryGetValue("comments", out jtoken))
                            {
                                row.Add("comments", "");
                            }
                        objectsall.Add(row);
                        }
                        catch
                        {

                        }
                    }
                    String datainfor = JsonConvert.SerializeObject(objectsall);
                    datainfor = datainfor.Replace("\r", "").Replace("\t", "").Replace("\n", "");
                   
                    String JsonData = "{\"path\":\"" + path1 + "\",\"namefile\":\"" + fileName + "\",\"tables\":[{\"nametable\":\"htk_Catalogo_Activos_Etiquetado\",\"query\":\"" + query + "\",\"data\":" + datainfor + ",\"fields\":[" + fields + "],\"valuerow\":[" + values + "]}]}";

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
            public void createInventory(String iduser)
            {
                //check if the path exist
                String relativepath = "\\Uploads\\Tags\\" + iduser + "\\download\\" + iduser + "\\";
                String absolutepath = Server.MapPath(relativepath);

                //  JObject datainfoString = JsonConvert.DeserializeObject<JObject>(dataInfo);

                //Create if not exist the directory
                if (!System.IO.Directory.Exists(absolutepath))
                    System.IO.Directory.CreateDirectory(absolutepath);

              

                String fileName = "Inventarios.sdf";
                String rootPath = Server.MapPath("~");
                String curFile = rootPath + relativepath + fileName;

              

                try
                {
                   

                    String path = rootPath + relativepath;
                    string path1 = path.Replace("\\", "\\\\");

                    string query = @"CREATE TABLE [htk_Inventarios] (
	                    ID_SESION_INVENTARIO NVARCHAR(30),
	                    EPC_ACTIVO NVARCHAR(30) NOT NULL,
	                    FECHA_REGISTRO DATETIME NULL,
	                    USUARIO_REGISTRO NVARCHAR(50) NULL,
	                    ENCONTRADO NVARCHAR(1) NULL,
	                    AF_CONJUNTO NVARCHAR(50) NULL,
	                    AF_UBICACION NVARCHAR(100) NULL,
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
	                     AF_UBICACION NVARCHAR(100) NULL,
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
            public void createConjuntoDB(String idlocation, String iduser)
            {

                /*   string pathexe = "\\Uploads\\ConsoleApplication1.exe";
                       string exe=Server.MapPath(pathexe);*/

                if (idlocation == "") idlocation = "null";

                //check if the path exist
                String relativepath = "\\Uploads\\Tags\\" + iduser + "\\download\\" + iduser + "\\";
                String absolutepath = Server.MapPath(relativepath);



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


                    String locationsString = locationsdb.GetRow(idlocation);
                    JObject locationRow = new JObject();
                    try
                    {
                        locationRow = JsonConvert.DeserializeObject<JObject>(locationsString);
                        locationRow = JsonConvert.DeserializeObject<JObject>(locationsdb.GetRow(locationRow["parent"].ToString()));
                     
                    }
                    catch (Exception ex) { }
                    JArray listLocation = new JArray();
                
                    if (idlocation == "null")
                    {
                        string locationrows = locationsdb.GetRows();
                        listLocation = JsonConvert.DeserializeObject<JArray>(locationrows);
                    }
                    else
                    {
                        listLocation = getSubLocation(idlocation);
                    }
                    if (listLocation == null)
                    {
                        listLocation = new JArray();
                    }
                    try
                    {
                        string getconjunt = locationsProfilesdb.Get("name", "Conjunto");
                        JArray conjuntja = new JArray();
                        string idprof = "";
                        try
                        {
                            conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                            idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                            JArray auxlocs = new JArray();
                            listLocation.Add(locationRow);
                            foreach (JObject loc in listLocation)
                            {
                                try
                                {
                                    if (idprof.Contains(loc["profileId"].ToString()))
                                    {
                                        auxlocs.Add(loc);
                                        continue;
                                    }
                                }
                                catch { }
                            }

                            listLocation = auxlocs;
                        }
                        catch
                        {

                        }
                    }
                    catch (Exception ex)
                    {

                    }


                    string query = @"CREATE TABLE [htk_Conjuntos] (CJ_UNIDAD_EXPLOTACION NVARCHAR(25) NULL,CJ_NOMBRE_CONJUNTO NVARCHAR(50) NULL,ID_CONJUNTO NVARCHAR(50) NULL)";
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
        public void createSubLocationDB(String idlocation, String iduser)
        {

            /*   string pathexe = "\\Uploads\\ConsoleApplication1.exe";
                   string exe=Server.MapPath(pathexe);*/

            if (idlocation == "") idlocation = "null";

            //check if the path exist
            String relativepath = "\\Uploads\\Tags\\" + iduser + "\\download\\" + iduser + "\\";
            String absolutepath = Server.MapPath(relativepath);



            //Create if not exist the directory
            if (!System.IO.Directory.Exists(absolutepath))
                System.IO.Directory.CreateDirectory(absolutepath);

            String fileName = "SubUbicaciones.sdf";
            String rootPath = Server.MapPath("~");
            String curFile = rootPath + relativepath + fileName;
            String path = rootPath + relativepath;
            string path1 = path.Replace("\\", "\\\\");

            try
            {


                /*  String locationsString = locationsdb.GetRow(idlocation);
                  JObject locationRow = new JObject();
                  try
                  {
                      locationRow = JsonConvert.DeserializeObject<JObject>(locationsString);
                  }
                  catch (Exception ex) { }
                 JArray listLocation = new JArray();
                  if (idlocation == "null")
                  {
                      string locationrows = locationsdb.GetRows();
                      listLocation = JsonConvert.DeserializeObject<JArray>(locationrows);
                  }
                  else
                  {
                       listLocation = getSubLocation(idlocation);
                  }
                  if (listLocation == null)
                  {
                      listLocation = new JArray();
                  }*/
                JArray listLocation = new JArray();
                try
                {
                    String locationsString = locationsdb.GetRows();
                   
                    listLocation = JsonConvert.DeserializeObject<JArray>(locationsString);
                    Dictionary<string, JObject> dictlocs = new Dictionary<string, JObject>();
                    try
                    {
                        dictlocs = listLocation.ToDictionary(x => (string)x["_id"], x => (JObject)x);
                    }
                    catch { }
                    string getconjunt = locationsProfilesdb.Get("name", "Sub-Ubicaciones");
                    JArray conjuntja = new JArray();
                    string idprof = "";
                    try
                    {
                        conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                        idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                        JArray auxlocs = new JArray();
                        JToken ko;
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
                                        JObject oficina = dictlocs[loc["oficina_id"].ToString()];

                                        loc.Add("oficina", oficina["name"].ToString());
                                       
                                    }
                                    catch { }
                                    JToken tk;
                                    if(!loc.TryGetValue("ubicacion",out tk))
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
                }
                catch (Exception ex)
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
        public void createLocationDB(String idlocation,String iduser)
            {

                /*   string pathexe = "\\Uploads\\ConsoleApplication1.exe";
                       string exe=Server.MapPath(pathexe);*/

                if (idlocation == "") idlocation = "null";

                //check if the path exist
                String relativepath = "\\Uploads\\Tags\\" + iduser+ "\\download\\" + iduser + "\\";
                String absolutepath = Server.MapPath(relativepath);

               

                //Create if not exist the directory
                if (!System.IO.Directory.Exists(absolutepath))
                    System.IO.Directory.CreateDirectory(absolutepath);

                String fileName = "Ubicaciones.sdf";
                String rootPath = Server.MapPath("~");
                String curFile = rootPath + relativepath + fileName;
                String path = rootPath + relativepath;
                string path1 = path.Replace("\\", "\\\\");

                try
                {
                  

                  /*  String locationsString = locationsdb.GetRow(idlocation);
                    JObject locationRow = new JObject();
                    try
                    {
                        locationRow = JsonConvert.DeserializeObject<JObject>(locationsString);
                    }
                    catch (Exception ex) { }
                   JArray listLocation = new JArray();
                    if (idlocation == "null")
                    {
                        string locationrows = locationsdb.GetRows();
                        listLocation = JsonConvert.DeserializeObject<JArray>(locationrows);
                    }
                    else
                    {
                         listLocation = getSubLocation(idlocation);
                    }
                    if (listLocation == null)
                    {
                        listLocation = new JArray();
                    }*/
                     JArray listLocation = new JArray();
                     try
                    {
                        String locationsString = locationsdb.GetRows();
                  
                        listLocation = JsonConvert.DeserializeObject<JArray>(locationsString);
                   
                        string getconjunt = locationsProfilesdb.Get("name", "Ubicacion");
                        JArray conjuntja = new JArray();
                        string idprof = "";
                        try
                        {
                            conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                            idprof = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                            JArray auxlocs = new JArray();
                            JToken ko;
                            foreach (JObject loc in listLocation)
                            {
                                if (idprof.Contains(loc["profileId"].ToString()))
                                {
                                    if(loc.TryGetValue("parent",out ko)){
                                     if(loc["parent"].ToString()=="none")
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
                    }
                    catch (Exception ex)
                    {

                    }


                    string query = @"CREATE TABLE [htk_Ubicaciones] (UB_ID_UBICACION NVARCHAR(25) NULL,UB_DESCRIPCION NVARCHAR(50) NULL)";
                    string fields = "'number','name'";
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

          //*********
            public void createUserDB(String iduser)
            {
                //check if the path exist
                String relativepath = "\\Uploads\\Tags\\" + iduser + "\\download\\" + iduser + "\\";
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
                    foreach (JObject obj in listUser) {
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
            public JArray getSubLocation(String idLocation)
            {
                String locationArray = locationsdb.GetRow(idLocation);

                try
                {
                    JObject locationRow = JsonConvert.DeserializeObject<JObject>(locationArray);
                    JArray listLocation = new JArray();
                    listLocation.Add(locationRow);

                    for (int i = 0; i < listLocation.Count; i++)
                    {
                        locationArray = locationsdb.Get("parent", listLocation[i]["_id"].ToString());
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

            public void createReference(String iduser)
        {
            //check if the path exist
             String relativepath = "\\Uploads\\Tags\\" + iduser+ "\\download\\" + iduser + "\\";
                String absolutepath = Server.MapPath(relativepath);
            
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
                   
                    objectString = objReferencedb.GetRows();
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
                        UB_ID_UBICACION NVARCHAR(25) NULL,
                        FOTO IMAGE NULL)";

                string fields = "'_id','name','location_id','marca','modelo','photo'";
                string values = "'0','3','8','4','5','9'";
                string pathexe = "\\bin\\sdf\\ConsoleApplication1.exe";
                string exe = Server.MapPath(pathexe);
                String datainfor = "";
               
                datainfor=JsonConvert.SerializeObject(objects);
               // datainfor = datainfor.Replace("\"", "'");
                datainfor = datainfor.Replace("\r","").Replace("\t","").Replace("\n","");
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

            
        }

    }

