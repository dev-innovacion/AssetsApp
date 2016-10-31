using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Db.MongoDb;
using MongoDB.Driver;
using Rivka.Security;
namespace RivkaAreas.Design.Controllers
{

    [Authorize]
    public class DesignController : Controller
    {
        //
        // GET: /Design/Design/

        protected MongoModel Desingdb;
        protected validatePermissions validatepermissions = new validatePermissions();
       /* public DesignController()
        {
            bindsessions();
        }*/
  public void bindsessions(){
     try
    {
       
        if (Request.Cookies["_loggeduser"] != null)
        {
            Session["LoggedUser"] = Request.Cookies["_loggeduser"].Value;
        }
   
        if (Request.Cookies["_id2"] != null)
        {
            Session["_id"] = Request.Cookies["_id2"].Value;
            
        }
   
          if (Request.Cookies["permissions"] != null)
          {
              Session["Permissions"] = Request.Cookies["permissions"].Value;

          }

      

     
          if (Request.Cookies["permissionsclient"] != null)
          {
              Session["PermissionsClient"] = Request.Cookies["permissionsclient"].Value;

          }

            }catch{


            }

        }
        public ActionResult Index()
        {
            bindsessions();
         String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("designs", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("designs", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                JObject desingjo = new JObject();

                try
                {
                    string desingc = Desingdb.GetRows();
                    string idrow = "";
                    JArray backgrounds = new JArray();
                    string logo = "";
                    try
                    {
                        JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                        foreach (JObject row in rows)
                        {
                            desingjo = row;
                        }

                    }
                    catch (Exception ex) { }
                }
                catch (Exception ex)
                {

                }

                return View(desingjo);
            }
            else
            {

                return Redirect("~/Home");
            }
        }
       
        public DesignController(){

            Desingdb = new MongoModel("Design");
            bindingSessions();
        }
        public string savetext(string text)
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);
            try
            {
                String Id = "";
              
                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
               
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    if (desingjo.TryGetValue("text", out j))
                    {
                        desingjo["text"] = text;
                    }
                    else
                    {
                        desingjo.Add("text", text);

                    }
                    idrow = desingjo["_id"].ToString();
                }
                catch (Exception ex) { }
               
               

                string item = JsonConvert.SerializeObject(desingjo);
                string Idimg = Desingdb.SaveRow(item, idrow);
                return "success";
            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public string attachSet(HttpPostedFileBase attachment)
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);
            string error = "";
            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";
                   

                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);

                if (attachment != null)
                {
                    filename = attachment.FileName.ToString();

                }

                string ext = null;
                string patch = "";
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;

                string relativepath = "\\Uploads\\Images\\Design\\Backgrounds";
                string absolutepath = Server.MapPath(relativepath);
               
                if (attachment != null)
                {
                    ext = attachment.FileName.Split('.').Last(); //getting the extension
                }
                if (attachment != null)
                {
                      if (!System.IO.Directory.Exists(absolutepath))
                    {
                        System.IO.Directory.CreateDirectory(absolutepath);
                    }
                    attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                    //  patch = relativepath + "\\" + patch + "." + ext;
                    patch = patch + "." + ext;
                }

                JObject datajo=new JObject();

                string desingc = Desingdb.GetRows();
                 JObject desingjo = new JObject();
                string idrow = "";
                JArray backgrounds = new JArray();
                string logo="";
                try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);
              
                    foreach (JObject row in rows)
                    {
                        desingjo=row;
                    }
                    
                    idrow = desingjo["_id"].ToString();
                    foreach (var item1 in desingjo["backgrounds"])
                    {
                        backgrounds.Add(item1);
                    }
                   
                }
                catch (Exception ex) { error = "/n " + ex.ToString(); }
                JToken j;
                backgrounds.Add(""+patch + "");
                if (desingjo.TryGetValue("backgrounds", out j))
                {
                    desingjo["backgrounds"] = backgrounds;
                }
                else
                {
                    desingjo.Add("backgrounds", backgrounds);
                }


                string item = JsonConvert.SerializeObject(desingjo);
                string Idimg = Desingdb.SaveRow(item, idrow);
                return patch;
            }
            catch (Exception ex)
            {
                return error = "/n " + ex.ToString();
            }


        }
        public string setSlider(string slider = "0")
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);

            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();


                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);




                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                JArray backgrounds = new JArray();
                string logo = "";
               

                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    if (desingjo.TryGetValue("slider", out j))
                    {
                        desingjo["slider"] = slider;
                    }
                    else
                    {
                        desingjo.Add("slider", slider);
                    }
                    
                    if (desingjo.TryGetValue("iduser", out j))
                    {
                        desingjo["iduser"] = userid;
                    }
                    else
                    {
                        desingjo.Add("iduser", userid);

                    }
                    idrow = desingjo["_id"].ToString();
                }
                catch (Exception ex) { }

                string item = JsonConvert.SerializeObject(desingjo);



                string Idimg = Desingdb.SaveRow(item, idrow);
                return "save";
            }
            catch (Exception ex)
            {
                return null;
            }


        }
       
        public string setColorlogin(string color1 = "#003773", string color2 = "#01509b", string color3 = "#003773")
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);
         
            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();


                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);




                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                JArray backgrounds = new JArray();
                string logo = "";
                string topbarcolor = "#000000";
                string widgetcolor = "#000000";
                string titlecolor = "#ffffff";

                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    if (desingjo.TryGetValue("color1", out j))
                    {
                        desingjo["color1"] = color1;
                    }
                    else
                    {
                        desingjo.Add("color1", color1);

                    }
                    if (desingjo.TryGetValue("color2", out j))
                    {
                        desingjo["color2"] = color2;
                    }
                    else
                    {
                        desingjo.Add("color2", color2);

                    }
                    if (desingjo.TryGetValue("color3", out j))
                    {
                        desingjo["color3"] = color3;
                    }
                    else
                    {
                        desingjo.Add("color3", color3);

                    }
                    if (desingjo.TryGetValue("iduser", out j))
                    {
                        desingjo["iduser"] = userid;
                    }
                    else
                    {
                        desingjo.Add("iduser", userid);

                    }
                    idrow = desingjo["_id"].ToString();
                }
                catch (Exception ex) { }

                string item = JsonConvert.SerializeObject(desingjo);
              

                
                string Idimg = Desingdb.SaveRow(item, idrow);
                return "save";
            }
            catch (Exception ex)
            {
                return null;
            }


        }
       
        public string setColor(string topbarcolor="#000000",string widgetcolor="#000000",string titlecolor="#ffffff")
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);
            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
               

                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);

             
                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                JArray backgrounds = new JArray();
                string logo = "";
                string logologin = "";
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    if (desingjo.TryGetValue("topbar", out j))
                    {
                        desingjo["topbar"] = topbarcolor;
                    }
                    else
                    {
                        desingjo.Add("topbar", topbarcolor);

                    }
                    if (desingjo.TryGetValue("widget", out j))
                    {
                        desingjo["widget"] = widgetcolor;
                    }
                    else
                    {
                        desingjo.Add("widget", widgetcolor);

                    }
                    if (desingjo.TryGetValue("title", out j))
                    {
                        desingjo["title"] = titlecolor;
                    }
                    else
                    {
                        desingjo.Add("title", titlecolor);

                    }
                    if (desingjo.TryGetValue("iduser", out j))
                    {
                        desingjo["iduser"] = userid;
                    }
                    else
                    {
                        desingjo.Add("iduser", userid);

                    }
                    idrow = desingjo["_id"].ToString();
                }
                catch (Exception ex) { }

                string item = JsonConvert.SerializeObject(desingjo);
                string Idimg = Desingdb.SaveRow(item, idrow);
                return "save";
            }
            catch (Exception ex)
            {
                return null;
            }


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
        
        public string attachSetLogo3(HttpPostedFileBase attachment)
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);

            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);

                if (attachment != null)
                {
                    filename = attachment.FileName.ToString();

                }

                string ext = null;
                string patch = "";
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;
                string relativepath = "\\Uploads\\Images\\Design\\Logomenu";
                string absolutepath = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.Delete(absolutepath, true);
                }

                if (attachment != null)
                {
                    ext = attachment.FileName.Split('.').Last(); //getting the extension
                }
                if (attachment != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }

                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;

                    }
                }

                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                JArray backgrounds = new JArray();
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    idrow = desingjo["_id"].ToString();
                    if (desingjo.TryGetValue("iduser", out j))
                    {
                        desingjo["iduser"] = userid;
                    }
                    else
                    {
                        desingjo.Add("iduser", userid);

                    }
                    if (desingjo.TryGetValue("logomenu", out j))
                    {
                        desingjo["logomenu"] = "" + patch + "";
                    }
                    else
                    {
                        desingjo.Add("logomenu", "" + patch + "");

                    }

                }
                catch (Exception ex) { }

                string item = JsonConvert.SerializeObject(desingjo);


                string Idimg = Desingdb.SaveRow(item, idrow);

                try
                {
                    updateLogo1();
                }
                catch { }
                return patch;


            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public string attachSetLogo2(HttpPostedFileBase attachment)
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);

            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);

                if (attachment != null)
                {
                    filename = attachment.FileName.ToString();

                }

                string ext = null;
                string patch = "";
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;
                string relativepath = "\\Uploads\\Images\\Design\\Logologin";
                string absolutepath = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.Delete(absolutepath, true);
                }

                if (attachment != null)
                {
                    ext = attachment.FileName.Split('.').Last(); //getting the extension
                }
                if (attachment != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }

                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;

                    }
                }

                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                JArray backgrounds = new JArray();
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    idrow = desingjo["_id"].ToString();
                    if (desingjo.TryGetValue("iduser", out j))
                    {
                        desingjo["iduser"] = userid;
                    }
                    else
                    {
                        desingjo.Add("iduser", userid);

                    }
                    if (desingjo.TryGetValue("logologin", out j))
                    {
                        desingjo["logologin"] = "" + patch + "";
                    }
                    else
                    {
                        desingjo.Add("logologin", "" + patch + "");

                    }
                    
                }
                catch (Exception ex) { }

                 string item = JsonConvert.SerializeObject(desingjo);
                string Idimg = Desingdb.SaveRow(item, idrow);
                return patch;


            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public string attachSetLogo(HttpPostedFileBase attachment)
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);

            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);

                if (attachment != null)
                {
                    filename = attachment.FileName.ToString();

                }

                string ext = null;
                string patch = "";
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;
                string relativepath = "\\Uploads\\Images\\Design\\Logo";
                string absolutepath = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.Delete(absolutepath, true);
                }

                if (attachment != null)
                {
                    ext = attachment.FileName.Split('.').Last(); //getting the extension
                }
                if (attachment != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }

                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;

                    }
                }

                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                JArray backgrounds = new JArray();
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    idrow = desingjo["_id"].ToString();
                    if (desingjo.TryGetValue("iduser", out j))
                    {
                        desingjo["iduser"] = userid;
                    }
                    else
                    {
                        desingjo.Add("iduser", userid);

                    }
                    if (desingjo.TryGetValue("logo", out j))
                    {
                        desingjo["logo"] = "" + patch + "";
                    }
                    else
                    {
                        desingjo.Add("logo", "" + patch + "");

                    }

                }
                catch (Exception ex) { }

                string item = JsonConvert.SerializeObject(desingjo);


                string Idimg = Desingdb.SaveRow(item, idrow);

                try
                {
                    updateLogo();
                }
                catch { }
                return patch;


            }
            catch (Exception ex)
            {
                return null;
            }


        }
        //Funcion que adjunta el Favicon
        public string attachSetFavicon(HttpPostedFileBase attachment)
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);

            try
            {
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);

                if (attachment != null)
                {
                    filename = attachment.FileName.ToString();

                }

                string ext = null;
                string patch = "favicon";
                var fecha = DateTime.Now.Ticks;
                //patch = userid + fecha;
                string relativepath = "\\Uploads\\Images\\Design\\Favicon";
                string absolutepath = Server.MapPath(relativepath);
                if (System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.Delete(absolutepath,true);
                }

                if (attachment != null)
                {
                    ext = attachment.FileName.Split('.').Last(); //getting the extension
                }
                if (attachment != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                   
                    attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                    //  patch = relativepath + "\\" + patch + "." + ext;
                    patch = patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                        attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        patch = patch + "." + ext;
               
                    }
                }

                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                JArray backgrounds=new JArray();
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    idrow = desingjo["_id"].ToString();
                    if (desingjo.TryGetValue("iduser", out j))
                    {
                        desingjo["iduser"] = userid;
                    }
                    else
                    {
                        desingjo.Add("iduser", userid);

                    }
                    if (desingjo.TryGetValue("favicon", out j))
                    {
                        desingjo["favicon"] = "" + patch + "";
                    }
                    else
                    {
                        desingjo.Add("favicon", "" + patch + "");

                    }

                }
                catch (Exception ex) { }

                string item = JsonConvert.SerializeObject(desingjo);
               
              
                string Idimg = Desingdb.SaveRow(item, idrow);

                try
                {
                    updateLogo();
                }
                catch { }
                return patch;

              
            }catch(Exception ex){
                return null;
            }


        }
        
        public string deleteLogo(string idimg)
        {

            try
            {
                string idi = idimg;
                String Id = "";
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                string logo = "";
                JArray backgrounds = new JArray();
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    idrow = desingjo["_id"].ToString();

                    if (desingjo.TryGetValue("logo", out j))
                    {
                        logo = desingjo["logo"].ToString();
                    }
                    else
                    {
                        datajo.Add("logo", "");
                    }


                }
                catch (Exception ex) { }

                desingjo["logo"] = "";
                string item = JsonConvert.SerializeObject(datajo);
                string Idimg = Desingdb.SaveRow(item, idrow);
                string relativepath = "\\Uploads\\Images\\Design\\Logo";
                string absolutepath = Server.MapPath(relativepath);
                string urlimg = logo;
                if (System.IO.File.Exists(absolutepath + "\\" + urlimg))
                {
                    System.IO.File.Delete(absolutepath + "\\" + urlimg);
                }

                return "success";
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idimg"></param>
        /// <returns></returns>
        public string deletebackground(string idimg)
        {

            try
            {
                string idi = idimg;
                String Id = "";
                bindsessions();
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                JObject datajo = new JObject();
                JToken jt;
                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                string logo = "";
                JArray backgrounds = new JArray();
                try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                    foreach (JObject row in rows)
                    {
                        desingjo = row;
                    }
                    JArray backgroundsaux = new JArray();
                    backgrounds =desingjo["backgrounds"] as JArray;
                    idrow = desingjo["_id"].ToString();
                    if (desingjo.TryGetValue("logo", out jt))
                    {
                        logo = desingjo["logo"].ToString();
                    }
                    else
                    {
                        desingjo.Add("logo", "");
                       
                    }
                    
                    foreach (var item1 in desingjo["backgrounds"])
                    {
                       string name = item1.ToString().Split('.').First(); //getting the extension
                        try
                        {
                            if (name != idi)
                            {
                                backgroundsaux.Add(item1);
                            }
                            else
                            {
                                try {
                                    string relativepath = "\\Uploads\\Images\\Design\\Backgrounds";
                                    string absolutepath = Server.MapPath(relativepath);
                                    string urlimg = item1.ToString();
                                    if (System.IO.File.Exists(absolutepath + "\\" + urlimg))
                                    {
                                        System.IO.File.Delete(absolutepath + "\\" + urlimg);
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                    backgrounds = backgroundsaux;
                }
                catch (Exception ex) { }
               
                desingjo["backgrounds"] = backgrounds;
                string item = JsonConvert.SerializeObject(desingjo);
                string Idimg = Desingdb.SaveRow(item, idrow);


                return "success";
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        public string deleteLogo2(string idimg)
        {

            try
            {
                string idi = idimg;
                String Id = "";
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                string logo = "";
                JArray backgrounds = new JArray();
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    idrow = desingjo["_id"].ToString();

                    if (desingjo.TryGetValue("logologin", out j))
                    {
                        logo = desingjo["logologin"].ToString();
                    }
                    else
                    {
                        datajo.Add("logologin", "");
                    }


                }
                catch (Exception ex) { }

                desingjo["logologin"] = "";
                string item = JsonConvert.SerializeObject(datajo);
                string Idimg = Desingdb.SaveRow(item, idrow);
                string relativepath = "\\Uploads\\Images\\Design\\Logologin";
                string absolutepath = Server.MapPath(relativepath);
                string urlimg = logo;
                if (System.IO.File.Exists(absolutepath + "\\" + urlimg))
                {
                    System.IO.File.Delete(absolutepath + "\\" + urlimg);
                }

                return "success";
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        //Funcion que borra el Favicon
        public string deleteFavicon(string idimg)
        {

            try
            {
                string idi = idimg;
                String Id = "";
                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string filename = "none";


                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string idrow = "";
                string logo = "";
                JArray backgrounds = new JArray();
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;
                    JToken j;
                    idrow = desingjo["_id"].ToString();

                    if (desingjo.TryGetValue("favicon", out j))
                    {
                        logo = desingjo["favicon"].ToString();
                    }
                    else
                    {
                        datajo.Add("favicon", "");
                    }


                }
                catch (Exception ex) { }

                desingjo["favicon"] = "";
                string item = JsonConvert.SerializeObject(datajo);
                string Idimg = Desingdb.SaveRow(item, idrow);
                string relativepath = "\\Uploads\\Images\\Design\\Favicon";
                string absolutepath = Server.MapPath(relativepath);
                string urlimg = logo;
                if (System.IO.File.Exists(absolutepath + "\\" + urlimg))
                {
                    System.IO.File.Delete(absolutepath + "\\" + urlimg);
                }

                return "success";
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        
        public string saveslider(string slider)
        {

            try
            {
                setSlider(slider);
                string relativepath = "\\Content\\CSS\\Design\\";
                string absolutepath = Server.MapPath(relativepath);
                string url = "slidercustom.css";
                string logo = getLogo();
                string size=(slider=="0")?"50":"100";
                string classsize = ".sliderdefault" +
                   " {" +
                     "background-size: "+size+"% 100% !important;"+
                      
                   " }";
                string[] lines = { classsize };

                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }
                if (System.IO.File.Exists(absolutepath + "\\" + url))
                {
                    System.IO.File.Delete(absolutepath + "\\" + url);
                }

                System.IO.File.WriteAllLines(absolutepath + "\\" + url, lines);


                return "saved";
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string saveColorlogin(string color1, string color2, string color3)
        {

            try
            {
                setColorlogin(color1, color2, color3);
                string relativepath = "\\Content\\CSS\\Design\\";
                string absolutepath = Server.MapPath(relativepath);
                string url = "loginform.css";
               // string logo = getLogo();
               
                string formlogin = "#gradiente1"+
                   " {"+
                     "   background: -webkit-linear-gradient(left top, " + color1 + "," + color2 + "," + color3 + ");" + /* For Safari 5.1 to 6.0 */
                      "  background: -o-linear-gradient(bottom right,  " + color1 + "," + color2 + "," + color3 + ");" + /* For Opera 11.1 to 12.0 */
                      "  background: -moz-linear-gradient(bottom right,  " + color1 + "," + color2 + "," + color3 + ");" + /* For Firefox 3.6 to 15 */
                      "  background: linear-gradient(to bottom right,  " + color1 + "," + color2 + "," + color3 + ");" + /* Standard syntax (must be last) */
                   " }";
                string[] lines = { formlogin };

                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }
                if (System.IO.File.Exists(absolutepath + "\\" + url))
                {
                    System.IO.File.Delete(absolutepath + "\\" + url);
                }

                System.IO.File.WriteAllLines(absolutepath + "\\" + url, lines);


                return "saved";
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string saveColor(string topbar, string widget, string title)
        {

            try
            {
                setColor(topbar, widget, title);
                string relativepath = "\\Content\\CSS\\Design\\";
                string absolutepath = Server.MapPath(relativepath);
                string url = "default.css";
                string logo = getLogo();
                string topbarc = ".topbarclient {background:" + topbar + " !important;}";
                string menuactive = "#main_navigation.blue ul.main > li > a:active,#main_navigation.blue ul.main > li.active > a,#main_navigation.blue ul.main > li > a.subOpened {" +
                    "background: " + topbar + " !important;" +
                   " -webkit-box-shadow: none;" +
                    "-moz-box-shadow: none;" +
                   " box-shadow: none;" +
                    "font-weight: normal;" +
                   " color: #fff;" +
                    "border: 1px solid " + topbar + "!important;" +
                   " }";
                string logoc = ".logoclient {" +
                            "width: 129px;" +
                            "display: block;" +
                            "height: 44px;" +
                           " float: left;" +
                            "line-height: 44px;" +
                           " padding-left: 80px;" +
                           " font-size: 16px;" +
                           " color: #fff;" +
                           " font-weight: 300;" +
                           " text-transform: uppercase;" +
                           " background: url(" + logo + ") no-repeat left center;" +
                           " margin-left: 10px !important;" +
                           "background-size: contain;" +
                           "}";
                string titlec = ".titleclient{color:" + title + " !important;}";
                string widgetc = ".widgetclient{background:" + widget + " !important;}";
                string[] lines = { topbarc, titlec, widgetc, menuactive, logoc };

                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }
                if (System.IO.File.Exists(absolutepath + "\\" + url))
                {
                    System.IO.File.Delete(absolutepath + "\\" + url);
                }

                System.IO.File.WriteAllLines(absolutepath + "\\" + url, lines);


                return "saved";
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        public string updateLogo1()
        {

            try
            {

                string topbar = "#000000";
                string widget = "#ABABAB";
                string title = "#ffffff";

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string logomenu = "/Content/Images/logo2.png";
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;

                    logomenu = "/Uploads/Images/Design/Logomenu/" + desingjo["logomenu"].ToString();
                   
                }
                catch { }
                string relativepath = "\\Content\\CSS\\Design\\";
                string absolutepath = Server.MapPath(relativepath);
                string url = "logomenudefault.css";
                string logo = getLogo();

                string logoc = ".logomenu {" +
                            "  width: 129px;2"+
                             " display: block;"+
                            "  height: 44px;"+
                             " float: left;"+
                            "  line-height: 44px;"+
                             " padding-left: 80px;"+
                             " font-size: 16px;"+
                             " color: #fff;"+
                             " font-weight: 300;"+
                            "  text-transform: uppercase;"+
                             "  background: url("+logomenu+") no-repeat left center !important;" +
                             " margin-left: 10px;"+
                             " background-size: 50% 95%;"+
                             " }";
                string[] lines = { logoc };

                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }
                if (System.IO.File.Exists(absolutepath + "\\" + url))
                {
                    System.IO.File.Delete(absolutepath + "\\" + url);
                }

                System.IO.File.WriteAllLines(absolutepath + "\\" + url, lines);


                return "saved";
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string updateLogo()
        {

            try
            {
                
                string topbar = "#000000";
                string widget = "#ABABAB";
                string title = "#ffffff";
               
                 string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
               
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;

                  topbar= desingjo["topbar"].ToString();
                  widget = desingjo["widget"].ToString();
                  title = desingjo["title"].ToString();
                }
                catch { }
                string relativepath = "\\Content\\CSS\\Design\\";
                string absolutepath = Server.MapPath(relativepath);
                string url = "default.css";
                string logo = getLogo();
                string topbarc = ".topbarclient {background:" + topbar + " !important;}";
                string menuactive = "#main_navigation.blue ul.main > li > a:active,#main_navigation.blue ul.main > li.active > a,#main_navigation.blue ul.main > li > a.subOpened {" +
                    "background: " + topbar + " !important;" +
                   " -webkit-box-shadow: none;" +
                    "-moz-box-shadow: none;" +
                   " box-shadow: none;" +
                    "font-weight: normal;" +
                   " color: #fff;" +
                    "border: 1px solid " + topbar + "!important;" +
                   " }";
                string logoc = ".logoclient {" +
                            "width: 129px;" +
                            "display: block;" +
                            "height: 44px;" +
                           " float: left;" +
                            "line-height: 44px;" +
                           " padding-left: 80px;" +
                           " font-size: 16px;" +
                           " color: #fff;" +
                           " font-weight: 300;" +
                           " text-transform: uppercase;" +
                           " background: url(" + logo + ") no-repeat left center;" +
                           " margin-left: 10px !important;" +
                           "background-size: contain;" +
                           "}";
                string titlec = ".titleclient{color:" + title + " !important;}";
                string widgetc = ".widgetclient{background:" + widget + " !important;}";
                string[] lines = { topbarc, titlec, widgetc, menuactive, logoc };

                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }
                if (System.IO.File.Exists(absolutepath + "\\" + url))
                {
                    System.IO.File.Delete(absolutepath + "\\" + url);
                }

                System.IO.File.WriteAllLines(absolutepath + "\\" + url, lines);


                return "saved";
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        public List<string> getBackgrounds()
        {
            try
            {
                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                 List<string> backgrounds = new List<string>();
                
               
                try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                    foreach (JObject row in rows)
                    {
                        desingjo = row;
                    }
                      foreach (var item1 in desingjo["backgrounds"])
                    {
                        backgrounds.Add("/Uploads/Images/Design/Backgrounds/"+item1);

                    }
                }
                catch (Exception ex)
                {
                    backgrounds.Add("/Content/Images/slider/slide_01.jpg");
                    backgrounds.Add("/Content/Images/slider/slide_02.jpg");
                    backgrounds.Add("/Content/Images/slider/slide_03.jpg");
               
                }
                
               
                return backgrounds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string getLogoMenu()
        {
            try
            {
                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string logo = "";


                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;


                    logo = "/Uploads/Images/Design/Logomenu/" + desingjo["logomenu"].ToString();

                }
                catch (Exception ex)
                {
                    logo = "/Content/Images/logo2.png";

                }


                return logo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string getLogologin()
        {
            try
            {
                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string logo = "";


                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;


                    logo = "/Uploads/Images/Design/Logologin/" + desingjo["logologin"].ToString();

                }
                catch (Exception ex)
                {
                    logo = "/Content/Images/CinepolislogoLogin.png";

                }


                return logo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //Funcion que obtiene el favicon
        public string getFavicon()
        {
                        try
            {
                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string logo = "favicon.png";


                try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                    foreach (JObject row in rows)
                    {
                        desingjo = row;
                    }


                    logo = "/Uploads/Images/Design/Favicon/" + desingjo["favicon"].ToString();

                }
                catch (Exception ex)
                {
                     logo="";
                   
                }


                return logo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string getLogo()
        {
            try
            {
                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string logo = "";


                try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                    foreach (JObject row in rows)
                    {
                        desingjo = row;
                    }


                    logo="/Uploads/Images/Design/Logo/" + desingjo["logo"].ToString();

                }
                catch (Exception ex)
                {
                     logo="/Content/Images/cinepolis-01.png";
                   
                }


                return logo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public JObject getColorslogin()
        {
            try
            {
                JObject datajo = new JObject();
              
                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string color1 = "#003773";
                string color2 = "#01509b";
                string color3 = "#003773";
                JObject colors = new JObject();
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;

                   
                    try { color1 = desingjo["color1"].ToString(); }
                    catch (Exception ex) { }
                    try { color2 = desingjo["color2"].ToString(); }
                    catch (Exception ex) { }
                    try { color3 = desingjo["color3"].ToString(); }
                    catch (Exception ex) { }


                }
                catch (Exception ex)
                {


                }

                colors.Add("color1", color1);
                colors.Add("color2", color2);
                colors.Add("color3", color3);

                return colors;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string getText()
        {
            try
            {
                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();

                string text = "Acceso a Assetsapp";
                try
                {
                    desingjo = JsonConvert.DeserializeObject<JArray>(desingc).First() as JObject;


                    try { text = desingjo["text"].ToString(); }
                    catch (Exception ex) { }
                    

                }
                catch (Exception ex)
                {


                }

                

                return text;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public JObject getColors()
        {
            try
            {
                JObject datajo = new JObject();

                string desingc = Desingdb.GetRows();
                JObject desingjo = new JObject();
                string topbarcolor = "#000000";
                string widgetcolor = "#ABABAB";
                string titlecolor = "#ffffff";
                JObject colors = new JObject();
                try
                {
                    JArray rows = JsonConvert.DeserializeObject<JArray>(desingc);

                    foreach (JObject row in rows)
                    {
                        desingjo = row;
                    }

                    try { topbarcolor = desingjo["topbar"].ToString(); }
                    catch (Exception ex) { }
                    try { widgetcolor = desingjo["widget"].ToString(); }
                    catch (Exception ex) { }
                    try { titlecolor = desingjo["title"].ToString(); }
                    catch (Exception ex) { }

                   
                }
                catch (Exception ex)
                {
                  

                }

                colors.Add("topbar", topbarcolor);
                colors.Add("widget", widgetcolor);
                colors.Add("title", titlecolor);

                return colors;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
