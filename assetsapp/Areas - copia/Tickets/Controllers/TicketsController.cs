using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Db.MongoDb;
using RivkaAreas.Reports.Models;
using System.Drawing;
using MongoDB.Driver;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using iTextSharp.text.html.simpleparser;
using System.IO;
using System.util;
using System.Net;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using Rivka.Mail;
using Rivka.Api;
using Rivka.Security;

namespace RivkaAreas.Tickets.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        //
        // GET: /Tickets/Tickets/
        protected MongoModel Ticketsdb;
        protected MongoModel UsersTicketsdb;
        protected MongoModel Usersdb;
        protected MongoModel TicketChatdb;
        protected Agent agent;
        protected Messenger messenger;
        protected ValidateLimits validatelim;
        protected validatePermissions validatepermissions;
        /// <summary>
        ///     Initializes the models and the agent
        /// </summary>
        public TicketsController()
        {
            Ticketsdb = new MongoModel("Tickets");
            UsersTicketsdb = new MongoModel("UsersTickets");
            Usersdb = new MongoModel("Users");
            TicketChatdb = new MongoModel("ChatTickets");
            agent = null;
            messenger = new Messenger();
            validatelim = new ValidateLimits();
            validatepermissions = new validatePermissions();
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

        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
          //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("tickets", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("tickets", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                return View();
            }
             else
             {

                 return Redirect("~/Home");
             }
        }
        public string validateLimit()
        {
            try
            {
               string result= validatelim.validate("Tickets",null,null, "Tickets",1);
                
               
                return result;
            }
            catch (Exception ex)
            {
                return "error";
            }

        }
        public string SendTicket(String title, String category, String bodyticket, String attachment = null)
        {
            try
            {
                String Id = "";
                String IdUserM = "";
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }

                string userid = Session["_id"].ToString();
                string username = Session["LoggedUser"].ToString();
                string message = bodyticket;
                string attachfiles = attachment.ToString();
                JArray arrayattach = JsonConvert.DeserializeObject<JArray>(attachfiles);
                string categoryx = category;
                string titlex = title;
                List<string> list_recipients = new List<string>();

                String jsonData = "{'userId':'" + userid + "','userName':'" + username + "','title':'" + titlex + "','category':'" + categoryx
                  + "','bodyticket':'" + message + "','readed':'false','attachments':" + arrayattach + ",'messages':[],'comments':'','status': 'activo'}";

                Id = Ticketsdb.SaveRow(jsonData, Id);
                string rowArrayrelation = UsersTicketsdb.GetRows();
                JArray userTickets = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                List<string> list_relation = new List<string>();
                bool empity = false;
                foreach (JObject items in userTickets)
                {
                    empity = true;
                    if (items["userId"].ToString() == userid)
                    {
                        list_relation.Add(userid);
                        JArray exist = new JArray();
                        foreach (JObject Tickets in items["tickets"])
                        {
                            exist.Add(Tickets);
                        }
                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idticket\":'" + Id + "', \"readed\":\"false\", \"status\":\"activo\" }");
                        exist.Add(jobjectnew);
                        String jsonUserTicketsedit = "{'userId':'" + items["userId"] + "','tickets':" + exist + "}";

                        IdUserM = UsersTicketsdb.SaveRow(jsonUserTicketsedit, items["_id"].ToString());
                    }
                }
                if (empity == true)
                {
                    if (!list_relation.Contains(userid))
                    {
                        list_relation.Add(userid);
                        JArray ent = new JArray();
                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idticket\":'" + Id + "', \"readed\":\"false\", \"status\":\"activo\" }");
                        ent.Add(jobjectnew);

                        String jsonUserTicketsedit = "{'userId':'" + userid + "','tickets':" + ent + "}";

                        IdUserM = UsersTicketsdb.SaveRow(jsonUserTicketsedit, "");
                    }
                }
                if (empity == false)
                {
                    JArray ent = new JArray();
                    JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idticket\":'" + Id + "', \"readed\":\"false\", \"status\":\"activo\" }");
                    ent.Add(jobjectnew);


                    String jsonUserTicketsedit = "{'userId':'" + userid + "','tickets':" + ent + "}";

                    IdUserM = UsersTicketsdb.SaveRow(jsonUserTicketsedit, "");
                }
                // messenger.sendMailPush("99");
                String jsonticket = "{'function':'saveTicket',document:{'_id':'" + Id + "','userId':'" + userid + "','userName':'" + username + "','title':'" + titlex + "','category':'" + categoryx
                  + "','bodyticket':'" + message + "','readed':'false','attachments':" + arrayattach + ",'messages':[],'comments':'','status': 'activo','Source':'cinepolis'}}";

                String jsonAgent = "{'action':'spreadToFather','collection':'Tickets','document':" + jsonticket + ",'Source':'cinepolis'}";
                initAgent();
                string respo = agent.sendMessage(jsonAgent);

                return "Enviado";
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult getTicketsTable()
        {
            if (this.Request.IsAjaxRequest())
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string userid = Session["_id"].ToString();
                string tickets = Ticketsdb.GetAll("status", "activo");
                JArray ticket = JsonConvert.DeserializeObject<JArray>(tickets);

                JArray mailbox = new JArray();
                string UserId = Session["_id"].ToString();
                string rowArrayrelation = UsersTicketsdb.GetRows();
                JArray userTickets = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                string sta = "";
                string read = "";
                List<string> readeds = new List<string>();
                List<string> statux = new List<string>();
                foreach (JObject items in userTickets)
                {
                    JArray mailsexist = new JArray();
                    if (UserId == items["userId"].ToString())
                    {
                        foreach (JObject x in items["tickets"])
                        {
                            sta = x["status"].ToString();
                            statux.Add(sta);
                            read = x["readed"].ToString();
                            readeds.Add(read);

                        }
                    }
                }
                string[] aux = statux.ToArray();
                string[] auxr = readeds.ToArray();
                List<string> f = new List<string>();
                int i = 0;
                List<string> repeat = new List<string>();
                foreach (JObject mine in ticket)
                {
                    if (mine["userId"].ToString() == userid)
                    {
                        /* string mailcopy=JsonConvert.SerializeObject(mine);
                            mailbox.Add(mailcopy);*/
                        if (aux[i] == "activo")
                        {
                            if (!repeat.Contains(mine["_id"].ToString()))
                            {
                                mailbox.Add(mine);
                                f.Add(auxr[i]);
                                repeat.Add(mine["_id"].ToString());
                            }
                        }
                        i++;
                    }
                }
                //   string[] arrayrec = mailbox.ToArray();
                //  string jdatos = JsonConvert.SerializeObject(arrayrec);
                //  JArray jarray = JsonConvert.DeserializeObject<JArray>(jdatos);
                var viewAll = false;
                if (viewAll == true) { ViewData["viewAll"] = "access"; } else { ViewData["viewAll"] = "no"; }
                ViewData["readed"] = f;

                return View(mailbox);
            }
            return this.Redirect("/Tickets/Tickets");
        }
        public ActionResult getTicketsClosed()
        {
            if (this.Request.IsAjaxRequest())
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }
                string userid = Session["_id"].ToString();
                string tickets = Ticketsdb.GetAll("status", "closed");
                JArray ticket = JsonConvert.DeserializeObject<JArray>(tickets);

                JArray mailbox = new JArray();
                string UserId = Session["_id"].ToString();
                string rowArrayrelation = UsersTicketsdb.GetRows();
                JArray userTickets = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                string sta = "";
                string read = "";
                List<string> readeds = new List<string>();
                List<string> statux = new List<string>();
                foreach (JObject items in userTickets)
                {
                    JArray mailsexist = new JArray();
                    if (UserId == items["userId"].ToString())
                    {
                        foreach (JObject x in items["tickets"])
                        {
                            sta = x["status"].ToString();
                            statux.Add(sta);
                            read = x["readed"].ToString();
                            readeds.Add(read);
                        }
                    }
                }
                string[] aux = statux.ToArray();
                string[] auxr = readeds.ToArray();
                List<string> f = new List<string>();
                int i = 0;
                List<string> repeat = new List<string>();
                foreach (JObject mine in ticket)
                {
                    if (mine["userId"].ToString() == userid)
                    {
                        /* string mailcopy=JsonConvert.SerializeObject(mine);
                            mailbox.Add(mailcopy);*/
                        if (aux[i] == "activo")
                        {
                            if (!repeat.Contains(mine["_id"].ToString()))
                            {
                                mailbox.Add(mine);
                                f.Add(auxr[i]);
                                repeat.Add(mine["_id"].ToString());
                            }
                        }
                        i++;
                    }
                }

                //   string[] arrayrec = mailbox.ToArray();
                //  string jdatos = JsonConvert.SerializeObject(arrayrec);
                //  JArray jarray = JsonConvert.DeserializeObject<JArray>(jdatos);
                var viewAll = false;
                if (viewAll == true) { ViewData["viewAll"] = "access"; } else { ViewData["viewAll"] = "no"; }
                ViewData["readed"] = f;

                return View(mailbox);
            }
            return this.Redirect("/Tickets/Tickets");
        }
        public ActionResult getTicket(String idTicket)
        {

            try
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }

                string UserId = Session["_id"].ToString();

                string ticket = Ticketsdb.GetRow(idTicket);
                JObject ticketx = JsonConvert.DeserializeObject<JObject>(ticket);

                if (ticketx["Creator"].ToString() == UserId)
                {
                    string rowArrayrelation = UsersTicketsdb.GetRows();
                    JArray userTickets = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                    /* String jsonData = "{'userId':'" + mail["userId"] + "','userName':'" + mail["userName"] + "','recipients':" + mail["recipients"] + ",'subject':'" + mail["subject"]
                     + "','bodymessage':'" + mail["bodymessage"] + "','readed':'false','attachments':" + mail["attachments"]
                      + ",'status': '"+mail["status"]+"'}";
           
                     id = messageModel.SaveRow(jsonData, mail["_id"].ToString());*/
                    // bool viewAll = getpermissions("messages", "a");
                    bool viewAll = false;
                    if (viewAll == false)
                    {

                        foreach (JObject items in userTickets)
                        {
                            JArray exist = new JArray();
                            if (UserId == items["userId"].ToString())
                            {
                                foreach (JObject x in items["tickets"])
                                {
                                    if (idTicket == x["idticket"].ToString())
                                    {
                                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idticket\":'" + idTicket + "', \"readed\":\"true\", \"status\":\"activo\" }");
                                        exist.Add(jobjectnew);

                                    }
                                    else
                                    {
                                        exist.Add(x);
                                    }
                                }

                                String jsonUserTicketedit = "{'userId':'" + items["userId"] + "','tickets':" + exist + "}";
                                // messenger.sendMailPush("99");
                                string Idusermesssage = UsersTicketsdb.SaveRow(jsonUserTicketedit, items["_id"].ToString());
                            }
                        }
                    }
                    return View(ticketx);
                }
                else
                {
                    return this.Redirect("/Tickets/Tickets/");
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ActionResult deleteTicket(String idMail)
        {

            string id = "";

            string UserId = Session["_id"].ToString();
            //   bool viewAll = getpermissions("messages", "a");

            string rowArrayrelation = UsersTicketsdb.GetRows();
            JArray userTickets = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
            string tickets = Ticketsdb.GetRow(idMail);
            JObject ticket = JsonConvert.DeserializeObject<JObject>(tickets);

            List<string> list_relation = new List<string>();

            if (UserId == ticket["userId"].ToString())
            {

                String jsonData = "{'userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','title':'" + ticket["title"] + "','category':'" + ticket["category"]
                  + "','bodyticket':'" + ticket["bodyticket"] + "','readed':'true','attachments':" + ticket["attachments"] + ",'messages':" + ticket["messages"] + ",'comments':'" + ticket["comments"] + "','status': 'closed'}";

                id = Ticketsdb.SaveRow(jsonData, ticket["_id"].ToString());
            }

            foreach (JObject items in userTickets)
            {
                JArray exist = new JArray();
                if (UserId == items["userId"].ToString())
                {
                    foreach (JObject x in items["tickets"])
                    {
                        if (idMail == x["idticket"].ToString())
                        {
                            JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idticket\":'" + idMail + "', \"readed\":\"true\", \"status\":\"closed\" }");
                            exist.Add(jobjectnew);
                        }
                        else
                        {
                            exist.Add(x);
                        }

                    }
                    String jsonUserMessageedit = "{'userId':'" + items["userId"] + "','tickets':" + exist + "}";
                    string Iduserticket = UsersTicketsdb.SaveRow(jsonUserMessageedit, items["_id"].ToString());
                }

            }
            return this.Redirect("/Tickets/Tickets/");
        }

        public string deletemsg(String idticket, String idmsg)
        {
            try
            {
                string tickets = Ticketsdb.GetRow(idticket);
                JObject ticket = JsonConvert.DeserializeObject<JObject>(tickets);
                // String jsonchat = "{'idTicket':'" + idticket + "','username':'" + name + "','message':'" + message + "','image':'" + image + "'}";

                JArray ent = new JArray();
                try
                {
                    foreach (var x in ticket["messages"])
                    {

                        if (x["idmessage"].ToString() == idmsg)
                        {

                        }
                        else
                        {
                            ent.Add(x);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                String jsonchat = "{'userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','title':'" + ticket["title"] + "','category':'" + ticket["category"]
                  + "','bodyticket':'" + ticket["bodyticket"] + "','readed':'true','attachments':" + ticket["attachments"] + ",'messages':" + ent + ",'comments':'" + ticket["comments"] + "','status': 'activo'}";
                string idchat = Ticketsdb.SaveRow(jsonchat, ticket["_id"].ToString());
                return "deleted";
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string sendMg(String idticket, String username, String message)
        {
            try
            {
                if (Request.Cookies["_id2"] != null)
                {
                    Session["_id"] = Request.Cookies["_id2"].Value;
                }

                string userid = Session["_id"].ToString();
                string user = Usersdb.GetRow(username);
                JObject userdata = JsonConvert.DeserializeObject<JObject>(user);
                string tickets = Ticketsdb.GetRow(idticket);
                JObject ticket = JsonConvert.DeserializeObject<JObject>(tickets);

                string relativepath = string.Format("\\Uploads\\Images\\{0}.{1}",
                               userdata["_id"].ToString(), userdata["imgext"].ToString());
                string name = userdata["user"].ToString();
                string image = "";
                if (userdata["imgext"].ToString() != "")
                {
                    if (System.IO.File.Exists(Server.MapPath(relativepath)))
                    {
                        //if it exist, sets the profile picture url
                        image = "/Uploads/Images/" + userdata["_id"].ToString() + "." + userdata["imgext"].ToString() + "";
                    }
                    else
                    {
                        //Set picture to default
                        image = "/Content/Images/imgPerfil/avatar_06.png";
                    }
                }
                else
                {

                    image = "/Content/Images/imgPerfil/avatar_06.png";
                }
                // String jsonchat = "{'idTicket':'" + idticket + "','username':'" + name + "','message':'" + message + "','image':'" + image + "'}";
                int countmsjs = 0;
                JArray ent = new JArray();

                try
                {
                    foreach (var x in ticket["messages"])
                    {
                        countmsjs++;
                        ent.Add(x);
                    }

                    countmsjs++;
                }
                catch (Exception ex)
                {

                }

                string idmsgtime = DateTime.Now.ToString("yyyyMMddHHmmss");
                JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + idmsgtime + "', \"username\":'" + name + "',\"userid\":'" + userid + "', \"message\":'" + message + "',  \"image\":'" + image + "'}");
                ent.Add(jobjectnew);

                String jsonchat = "{'userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','title':'" + ticket["title"] + "','category':'" + ticket["category"]
                  + "','bodyticket':'" + ticket["bodyticket"] + "','readed':'true','attachments':" + ticket["attachments"] + ",'messages':" + ent + ",'comments':'" + ticket["comments"] + "','status': 'activo'}";


                string idchat = Ticketsdb.SaveRow(jsonchat, ticket["_id"].ToString());
                messenger.sendTicketPush(userid, idticket, idmsgtime, name, message, image);

                String jsonticket = "{'function':'saveMessages',document:{'_id':'" + ticket["_id"].ToString() + "','userId':'" + ticket["userId"] + "','userName':'" + name + "','messages':'" + message + "','image':'" + image + "','idmessage':'" + idmsgtime + "'}}";

                String jsonAgent = "{'action':'spreadToFather','collection':'Tickets','document':" + jsonticket + ",'Source':'cinepolis'}";
                initAgent();
                string respo = agent.sendMessage(jsonAgent);

                return "send";
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string closeTicket(String idticket)
        {
            try
            {
                string userid = Session["_id"].ToString();
                string tickets = Ticketsdb.GetRow(idticket);
                JObject ticket = JsonConvert.DeserializeObject<JObject>(tickets);

                String jsonchat = "{'userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','title':'" + ticket["title"] + "','category':'" + ticket["category"]
                  + "','bodyticket':'" + ticket["bodyticket"] + "','readed':'end','attachments':" + ticket["attachments"] + ",'messages':" + ticket["messages"] + ",'comments':'" + ticket["comments"] + "','status': 'closed'}";

                string idchat = Ticketsdb.SaveRow(jsonchat, ticket["_id"].ToString());
                messenger.closeClientTicketPush(idticket);
                String jsonticket = "{'function':'closeTicket',document:{'_id':'" + ticket["_id"] + "','userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','readed':'end','status': 'closed'}}";

                String jsonAgent = "{'action':'spreadToFather','collection':'Tickets','document':" + jsonticket + ",'Source':'cinepolis'}";
                initAgent();
                string respo = agent.sendMessage(jsonAgent);
                return "closed";
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string openTicket(String idticket)
        {
            try
            {
                string userid = Session["_id"].ToString();
                string tickets = Ticketsdb.GetRow(idticket);
                JObject ticket = JsonConvert.DeserializeObject<JObject>(tickets);
                // String jsonchat = "{'idTicket':'" + idticket + "','username':'" + name + "','message':'" + message + "','image':'" + image + "'}";

                String jsonchat = "{'userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','title':'" + ticket["title"] + "','category':'" + ticket["category"]
                  + "','bodyticket':'" + ticket["bodyticket"] + "','readed':'true','attachments':" + ticket["attachments"] + ",'messages':" + ticket["messages"] + ",'comments':'" + ticket["comments"] + "','status': 'activo'}";

                string idchat = Ticketsdb.SaveRow(jsonchat, ticket["_id"].ToString());
                messenger.openTicketPush(idticket);
                String jsonticket = "{'function':'openTicket',document:{'_id':'" + ticket["_id"] + "','userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','readed':'true','status': 'activo'}}";

                String jsonAgent = "{'action':'spreadToFather','collection':'Tickets','document':" + jsonticket + ",'Source':'cinepolis'}";
                initAgent();
                string respo = agent.sendMessage(jsonAgent);
                return "opened";
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
