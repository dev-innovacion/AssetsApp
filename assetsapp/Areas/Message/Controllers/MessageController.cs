using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Db.MongoDb;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using Rivka.Form.Field;

using Rivka.Form;
using Rivka.User;
using Rivka.Mail;
using System.Web.Security;
using System.Reflection;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
namespace RivkaAreas.Message.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        //
        // GET: /Message/Message/

        // protected MongoModel messageModel = new MongoModel("Messages");
        protected MongoModel usersModel = new MongoModel("Users");
        protected MongoModel messageModel = new MongoModel("Messages");
        protected MongoModel usermessageModel = new MongoModel("UserMessage");
        protected Messenger messenger = new Messenger();
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;

        public MessageController() {
            this._logTable = new LogBook.Controllers.LogBookController();
        }

        public ActionResult Index()
        {
            string rowArray = usersModel.GetRows();

            MongoModel.JoinCollections newJoin = new MongoModel.JoinCollections();

            //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
            JArray users = JsonConvert.DeserializeObject<JArray>(rowArray);
            Dictionary<string,string[]> data = new Dictionary<string, string[]>();
            foreach (JObject items in users)
            {

                string[] userlistarray = { items["user"].ToString(), items["email"].ToString() };
           
                data.Add(items["_id"].ToString(), userlistarray);
                // data.Add(items["email"].ToString());
            }

            ViewData["Contacts"] = data;



            return View();
        }

        public string SendMail(String to, String subject, String message, String attachment,String iduserset=null,String logguserset=null,String shortsubject="")
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);
            try
            {
                
                String Id = "";
                String IdUserM = "";

                string userid = "";
                if (iduserset == "Sistema")
                {
                    userid = "1";
                    logguserset = "Sistema";
                }else
                 if (iduserset != null)
                {
                    userid = iduserset;
                }
                else
                {
                    userid = Session["_id"].ToString();
                }
                 
                
                string username="";
                if (logguserset != null)
                {
                    username = logguserset;
                }
                else
                {
                    username = Session["LoggedUser"].ToString();
                }
               

                string attachfiles = attachment.ToString();
                JArray arrayattach = JsonConvert.DeserializeObject<JArray>(attachfiles);

                string recipient = to.ToString();
                JArray arrayrecipient = JsonConvert.DeserializeObject<JArray>(recipient);
                string rowArray = usersModel.GetRows();
                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray users = JsonConvert.DeserializeObject<JArray>(rowArray);
                List<string> list_recipients = new List<string>();
                List<string> list_emails = new List<string>();
                string from1 = null;
                foreach (JObject items in users)
                {

                    foreach (JValue x in arrayrecipient)
                    {
                        if (items["email"].ToString() == x.ToString() || items["_id"].ToString() == x.ToString())
                        {
                            list_recipients.Add(items["_id"].ToString());
                            list_emails.Add(items["email"].ToString());

                        }
                        if (items["_id"].ToString()==userid)
                        {
                            from1 = items["email"].ToString();
                        }
                    }
                    //  items["email"].ToString());

                }

                string[] arrayrec = list_recipients.ToArray();
                string jdatos = JsonConvert.SerializeObject(arrayrec);
                JArray jarray = JsonConvert.DeserializeObject<JArray>(jdatos);
                String jsonData = "{'userId':'" + userid + "','userName':'" + username + "','recipients':" + jarray + ",'subject':'" + subject + ",'shortsubject':'" + shortsubject
                   + "','bodymessage':'" + message + "','readed':'false','attachments':" + arrayattach + ",'status': 'activo'}";

                JObject jsonmail = new JObject();
                jsonmail.Add("userId", userid);
                jsonmail.Add("userName", username);
                jsonmail.Add("recipients", jarray);
                jsonmail.Add("subject", subject);
                jsonmail.Add("bodymessage", message);
                jsonmail.Add("readed", "false");
                jsonmail.Add("attachments", arrayattach);
                jsonmail.Add("status", "activo");
               


                Id = messageModel.SaveRow(JsonConvert.SerializeObject(jsonmail), Id);
                _logTable.SaveLog(userid, "Mensajes", "Insert: creación de mensaje", "Message", DateTime.Now.ToString());
                Task task = new Task(() => SendExternalMail(from1, username, list_emails, message, arrayattach, subject));
                 
                if (Id != "")
                {
                     task.Start();
                    
                   // string result = SendExternalMail(from1,username,list_emails, message, arrayattach, subject);

                }
                string rowArrayrelation = usermessageModel.GetRows();
                JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                List<string> list_relation = new List<string>();
                bool empity = false;
                foreach (JObject items in userMessages)
                {
                    empity = true;
                    foreach (JValue x in jarray)
                    {
                        if (items["userId"].ToString() == x.ToString())
                        {
                            list_relation.Add(x.ToString());
                            JArray mailsexist = new JArray();
                            foreach (JObject mails in items["mails"])
                            {
                                mailsexist.Add(mails);

                            }


                            JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + Id + "', \"readed\":\"false\", \"status\":\"activo\" }");
                            mailsexist.Add(jobjectnew);
                            String jsonUserMessageedit = "{'userId':'" + items["userId"] + "','mails':" + mailsexist + "}";

                            IdUserM = usermessageModel.SaveRow(jsonUserMessageedit, items["_id"].ToString());
                            _logTable.SaveLog(userid, "Mensajes", "Insert: creación de mensaje para usuario", "userMessage", DateTime.Now.ToString());
                        }


                    }

                    /*   if (!list_relation.Contains(items["userId"].ToString()))
                       {
                           JArray ent = new JArray();
                           JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + Id + "', \"readed\":\"false\", \"status\":\"activo\" }");
                           ent.Add(jobjectnew);

                           String jsonUserMessageedit = "{'userId':'" + items["userId"] + "','mails':" + ent+ "}";

                           IdUserM = usermessageModel.SaveRow(jsonUserMessageedit, "");

                       }*/


                }

                if (empity == true)
                {
                    foreach (JValue x in jarray)
                    {

                        if (!list_relation.Contains(x.ToString()))
                        {
                            list_relation.Add(x.ToString());
                            JArray ent = new JArray();
                            JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + Id + "', \"readed\":\"false\", \"status\":\"activo\" }");
                            ent.Add(jobjectnew);

                            String jsonUserMessageedit = "{'userId':'" + x + "','mails':" + ent + "}";

                            IdUserM = usermessageModel.SaveRow(jsonUserMessageedit, "");
                            _logTable.SaveLog(userid, "Mensajes", "Insert: creación de mensaje para usuario", "userMessage", DateTime.Now.ToString());
                        }



                    }
                }
                if (empity == false)
                {
                    foreach (JValue x in jarray)
                    {


                        JArray ent = new JArray();
                        JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + Id + "', \"readed\":\"false\", \"status\":\"activo\" }");
                        ent.Add(jobjectnew);


                        String jsonUserMessageedit = "{'userId':'" + x + "','mails':" + ent + "}";

                        IdUserM = usermessageModel.SaveRow(jsonUserMessageedit, "");
                        _logTable.SaveLog(userid, "Mensajes", "Insert: creación de mensaje para usuario", "userMessage", DateTime.Now.ToString());
                    }

                }


                messenger.sendMailPush("99");
                if (Id != "")
                {
                    task.Wait();
                }
                return Id;

            }
            catch (Exception ex)
            {
                return null;
            }

        }
       public string SendExternalMail(string from,string username,List<string> to,string msg,JArray attach,string subject)
        {

            try{
            MailMessage email = new MailMessage();
            foreach (string recipient in to)
            {
                email.To.Add(new MailAddress(recipient));
            }
            try
            {


                email.From = new MailAddress(from, username);

            }
            catch (Exception ex)
            {
                email.From = new MailAddress("Sistema@sistema.com", username);
                from = "Sistema@sistema.com";
            }
            email.Subject =username+"("+from+"): "+subject;
            email.Body = msg;
            email.IsBodyHtml =true;
              string relativepath = "\\Uploads\\Images\\Mail\\";
              try
              {
                  string absolutepath = Server.MapPath(relativepath);
                  foreach (var item in attach)
                  {
                      email.Attachments.Add(new Attachment(absolutepath + "\\" + item.ToString()));
                  }
              }
              catch (Exception ex) { 
              
              }
          
            email.Priority = MailPriority.High;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
           
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("edwin.b@rivka.mx", "edwinb2014");
 
            string output = null;
              try{
                 smtp.Send(email);
                 email.Dispose();
                 output = "Corre electrónico fue enviado satisfactoriamente.";
                }
                catch (Exception ex)
                {
                 output = "Error enviando correo electrónico: " + ex.Message;
                }
              return output;
        }catch(Exception ex){
        return null;
        }
        }
        public ActionResult getMail(String idMail)
        {

         try{
            //  string id = "";
            string message = messageModel.GetRow(idMail);
            JObject mail = JsonConvert.DeserializeObject<JObject>(message);
            string UserId = Session["_id"].ToString();
            string rowArrayrelation = usermessageModel.GetRows();
            JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
            /* String jsonData = "{'userId':'" + mail["userId"] + "','userName':'" + mail["userName"] + "','recipients':" + mail["recipients"] + ",'subject':'" + mail["subject"]
             + "','bodymessage':'" + mail["bodymessage"] + "','readed':'false','attachments':" + mail["attachments"]
              + ",'status': '"+mail["status"]+"'}";
           
             id = messageModel.SaveRow(jsonData, mail["_id"].ToString());*/
            bool viewAll = getpermissions("messages", "a");
          //  if (viewAll == false) {

                foreach (JObject items in userMessages)
                {
                    JArray mailsexist = new JArray();
                    if (UserId == items["userId"].ToString())
                    {
                        foreach (JObject x in items["mails"])
                        {



                            if (idMail == x["idmessage"].ToString())
                            {





                                JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + idMail + "', \"readed\":\"true\", \"status\":\"activo\" }");
                                mailsexist.Add(jobjectnew);

                            }
                            else
                            {
                                mailsexist.Add(x);
                            }

                        }

                        String jsonUserMessageedit = "{'userId':'" + items["userId"] + "','mails':" + mailsexist + "}";
                        messenger.sendMailPush("99");
                        string Idusermesssage = usermessageModel.SaveRow(jsonUserMessageedit, items["_id"].ToString());
                        _logTable.SaveLog(UserId, "Mensajes", "Update: modificación de mensaje para usuario", "userMessage", DateTime.Now.ToString());
                    }

                }
           // }
               
               
                    string folio = "";
                    try
                    {

                        JObject urljo = getfolio(mail["bodymessage"].ToString(), mail["_id"].ToString());


                        JToken token;
                        if (urljo.TryGetValue("url", out token))
                        {
                            mail.Add("url",urljo["url"].ToString());
                        }
                        else
                        {
                            //item.Add("url", "/Message/Message/getMail?idMail=" + item["_id"].ToString());
                        }
                    }
                    catch (Exception ex) { }
             
            return View(mail);
         }
         catch (Exception ex)
         {
             return Redirect("~/User/Login");
         }
        }

        public ActionResult deleteMail(String idMail)
        {

            string id = "";

            string UserId = Session["_id"].ToString();
            bool viewAll = getpermissions("messages", "a");
             
            string rowArrayrelation = usermessageModel.GetRows();
            JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
            string message = messageModel.GetRow(idMail);
            JObject mail = JsonConvert.DeserializeObject<JObject>(message);

            List<string> list_relation = new List<string>();


            if (UserId == mail["userId"].ToString())
            {
                String jsonData = "{'userId':'" + mail["userId"] + "','userName':'" + mail["userName"] + "','recipients':" + mail["recipients"] + ",'subject':'" + mail["subject"]
                + "','bodymessage':'" + mail["bodymessage"] + "','readed':'true','attachments':" + mail["attachments"]
                 + ",'status': 'recycled'}";
                id = messageModel.SaveRow(jsonData, mail["_id"].ToString());
                _logTable.SaveLog(UserId, "Mensajes", "Delete: eliminación de mensaje para usuario", "Message", DateTime.Now.ToString());
            }


            foreach (JObject items in userMessages)
            {
                JArray mailsexist = new JArray();
                if (UserId == items["userId"].ToString())
                {
                    foreach (JObject x in items["mails"])
                    {
                        if (idMail == x["idmessage"].ToString())
                        {

                            JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + idMail + "', \"readed\":\"true\", \"status\":\"recycled\" }");
                            mailsexist.Add(jobjectnew);
                        }
                        else
                        {
                            mailsexist.Add(x);
                        }

                    }

                    String jsonUserMessageedit = "{'userId':'" + items["userId"] + "','mails':" + mailsexist + "}";

                    string Idusermesssage = usermessageModel.SaveRow(jsonUserMessageedit, items["_id"].ToString());
                    _logTable.SaveLog(UserId, "Mensajes", "Delete: eliminación de mensaje para usuario", "userMessage", DateTime.Now.ToString());
                }

            }

            return this.Redirect("/Message/Message/deleteTable");

        }

        public ActionResult activateMail(String idMail)
        {

            string id = "";
            string UserId = Session["_id"].ToString();
            string rowArrayrelation = usermessageModel.GetRows();
            JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
            string message = messageModel.GetRow(idMail);
            JObject mail = JsonConvert.DeserializeObject<JObject>(message);

            List<string> list_relation = new List<string>();


            if (UserId == mail["userId"].ToString())
            {
                String jsonData = "{'userId':'" + mail["userId"] + "','userName':'" + mail["userName"] + "','recipients':" + mail["recipients"] + ",'subject':'" + mail["subject"]
                + "','bodymessage':'" + mail["bodymessage"] + "','readed':'true','attachments':" + mail["attachments"]
                 + ",'status': 'activo'}";
                id = messageModel.SaveRow(jsonData, mail["_id"].ToString());
            }


            foreach (JObject items in userMessages)
            {
                JArray mailsexist = new JArray();
                if (UserId == items["userId"].ToString())
                {
                    foreach (JObject x in items["mails"])
                    {



                        if (idMail == x["idmessage"].ToString())
                        {





                            JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + idMail + "', \"readed\":\"true\", \"status\":\"activo\" }");
                            mailsexist.Add(jobjectnew);

                        }
                        else
                        {
                            mailsexist.Add(x);
                        }

                    }

                    String jsonUserMessageedit = "{'userId':'" + items["userId"] + "','mails':" + mailsexist + "}";

                    string Idusermesssage = usermessageModel.SaveRow(jsonUserMessageedit, items["_id"].ToString());
                }

            }

            return this.Redirect("/Message/Message/getMailsTable");


        }
        public ActionResult removeMail(String idMail)
        {

            //  string id = "";
            string message = messageModel.GetRow(idMail);
            JObject mail = JsonConvert.DeserializeObject<JObject>(message);
            /*  + "','bodymessage':'" + mail["bodymessage"] + "','readed':'true','attachments':" + mail["attachments"]
               + ",'status': 'removed'}";
              id = messageModel.SaveRow(jsonData, mail["_id"].ToString());*/
            string UserId = Session["_id"].ToString();
            string rowArrayrelation = usermessageModel.GetRows("CreatedDate");
            JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);

            foreach (JObject items in userMessages)
            {
                JArray mailsexist = new JArray();
                if (UserId == items["userId"].ToString())
                {
                    foreach (JObject x in items["mails"])
                    {



                        if (idMail == x["idmessage"].ToString())
                        {





                            JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + idMail + "', \"readed\":\"true\", \"status\":\"removed\" }");
                            mailsexist.Add(jobjectnew);

                        }
                        else
                        {
                            mailsexist.Add(x);
                        }

                    }

                    String jsonUserMessageedit = "{'userId':'" + items["userId"] + "','mails':" + mailsexist + "}";

                    string Idusermesssage = usermessageModel.SaveRow(jsonUserMessageedit, items["_id"].ToString());
                    _logTable.SaveLog(UserId, "Mensajes", "Delete: eliminación de mensaje para usuario", "userMessage", DateTime.Now.ToString());
                }

            }
            return this.Redirect("/Message/Message/deleteTable");

        }
        public ActionResult outboxTable()
        {

            if (this.Request.IsAjaxRequest())
            {
                 string userid = Session["_id"].ToString();
                string messages = messageModel.GetRows("CreatedDate");
                JArray mails = JsonConvert.DeserializeObject<JArray>(messages);
                JArray mailbox = new JArray();
                string UserId = Session["_id"].ToString();
                string rowArrayrelation = usermessageModel.GetRows();
                JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                string sta = "";
                string read = "";


                foreach (JObject items in userMessages)
                {
                    JArray mailsexist = new JArray();
                    foreach (JObject x in items["mails"])
                    {
                        sta = x["status"].ToString();
                        read = x["readed"].ToString();
                    }
                }

                List<string> f = new List<string>();
                int i = 0;
                foreach (JObject mine in mails)
                {
                    string y = mine["status"].ToString();

                    if (y == "activo")
                    {
                        if (userid == mine["userId"].ToString())
                        {


                            mailbox.Add(mine);
                            f.Add("true");

                        }
                   }
                }

                ViewData["readed"] = f;
                JArray recipientsname = new JArray();
                foreach (JObject item in mailbox)
                {
                    List<string> listrecip = new List<string>();
                    string[] arrayrecip;
                    JValue valuerecp ;
                    foreach (var x in item["recipients"])
                    {
                        try {
                            string usersdata = usersModel.GetRow(x.ToString());
                            JObject usernm = JsonConvert.DeserializeObject<JObject>(usersdata);
                            listrecip.Add(usernm["user"].ToString());
                        }
                        catch {
                            continue;
                        }
                        

                    }
                    arrayrecip=listrecip.ToArray();
                     string jdatos = JsonConvert.SerializeObject(arrayrecip);
                  JArray jarray = JsonConvert.DeserializeObject<JArray>(jdatos);
                    item["recipients"]= jarray;
                    recipientsname.Add(item);
                }
                mailbox = recipientsname;
                return View(mailbox);
            }
            return this.Redirect("/Message/Message");
        }
        public ActionResult updateStatusAlls(String ids,string status)
        {
            if (this.Request.IsAjaxRequest())
            {
                try
                {
                    List<string> idlist = new List<string>();

                    try
                    {
                        JArray idsja = JsonConvert.DeserializeObject<JArray>(ids);
                        idlist = idsja.Values<string>().ToList();

                    }
                    catch (Exception ex)
                    {

                    }
                    if (Session["_id"] != null)
                    {
                        JArray mailbox = new JArray();

                        string UserId = Session["_id"].ToString();
                        string rowArrayrelation = usermessageModel.Get("userId", UserId);
                        JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                        JObject update = new JObject();

                        update = (from usm in userMessages.Children() select (JObject)usm).First();

                        try
                        {
                            int i = 0;
                            foreach (JObject x in update["mails"])
                            {
                                if (x["status"].ToString() == "activo" && idlist.Contains(x["idmessage"].ToString()))
                                {
                                    try
                                    {
                                        update["mails"].Children().ElementAt(i)["status"] = status;
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                i++;
                            }


                        }
                        catch (Exception ex)
                        {

                        }

                        String jsonsave = JsonConvert.SerializeObject(update);

                        string id = usermessageModel.SaveRow(jsonsave, update["_id"].ToString());
                        _logTable.SaveLog(UserId, "Mensajes", "Update: cambio de estado de mensajes", "userMessage", DateTime.Now.ToString());
                        return this.Redirect("/Message/Message/deleteTable");
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
            } return null;
        }
        public String readAlls(string ids,string status,string iduserthis=null)
        {
            
                try
                {
                    
                    try
                    {
                        if (iduserthis == null)
                        {
                            iduserthis = Session["_id"].ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Request.Cookies["_id2"] != null)
                        {
                            Session["_id"] = Request.Cookies["_id2"].Value;
                            iduserthis = Session["_id"].ToString();
                        }
                    }
                    List<string> idlist=new List<string>();
                   
                    try{
                        JArray idsja=JsonConvert.DeserializeObject<JArray>(ids);
                        idlist=idsja.Values<string>().ToList();
                     
                    }catch(Exception ex){

                    }
                    if (iduserthis != null)
                    {
                        JArray mailbox = new JArray();

                        string UserId = iduserthis;
                        string rowArrayrelation = usermessageModel.Get("userId", UserId);
                        JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                        JObject update = new JObject();
                       
                        update = (from usm in userMessages.Children() select (JObject)usm).First();
                       
                            try
                            {
                                int i = 0;
                                foreach (JObject x in update["mails"])
                                {
                                    if (x["status"].ToString() == "activo" && idlist.Contains(x["idmessage"].ToString()))
                                    {
                                        try
                                        {
                                            update["mails"].Children().ElementAt(i)["readed"] = status;
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    i++;
                                }


                            }
                            catch (Exception ex)
                            {

                            }

                            String jsonsave = JsonConvert.SerializeObject(update);

                            string id = usermessageModel.SaveRow(jsonsave, update["_id"].ToString());
                            _logTable.SaveLog(UserId, "Mensajes", "Update: cambio de estado de mensajes", "userMessage", DateTime.Now.ToString());
                          return "true";
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
            return null;
        }
        public String refreshMail()
        {
            if (this.Request.IsAjaxRequest())
            {
                try {
                    if (Session["_id"] != null)
                    {
                        JArray mailbox = new JArray();

                        string UserId = Session["_id"].ToString();
                        string rowArrayrelation = usermessageModel.GetRows();
                        JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);

                        foreach (JObject items in userMessages)
                        {
                            JArray mailsexist = new JArray();
                            if (UserId == items["userId"].ToString())
                            {
                                foreach (JObject x in items["mails"])
                                {
                                    if (x["status"].ToString() == "activo" && x["readed"].ToString() == "false")
                                    {
                                        mailbox.Add(x);
                                    }
                                }
                            }
                        }

                        int count = mailbox.Count();
                        string newmails = Convert.ToString(count);
                        return newmails;
                    }
                       }catch(Exception ex){
                           return "0";
                       }
                }return null;
            }
        public String forward(string idmail,string mail)
        {
            try
            {
                string mailrow = messageModel.GetRow(idmail);
                JObject mailjo = JsonConvert.DeserializeObject<JObject>(mailrow);
                string username= username = Session["LoggedUser"].ToString();
                string iduser = Session["_id"].ToString();
                string userrow=usersModel.GetRow(iduser);
                JObject userjo=JsonConvert.DeserializeObject<JObject>(userrow);
                List<string> listmails=new List<string>();
                listmails.Add(mail);
                JArray attach = new JArray();

                string mstring = messageModel.Get("old_message", idmail);
                JArray mss = JsonConvert.DeserializeObject<JArray>(mstring);
                if (mss.Count() == 0) {
                    JObject mail2 = new JObject();
                    mail2 = mailjo;
                    mail2["old_message"] = idmail;
                    JArray recipientes = JsonConvert.DeserializeObject<JArray>(mail2["recipients"].ToString());
                    recipientes.Add(mail);
                    // 
                    mail2["recipients"] = recipientes;
                    mail2["userId"] = iduser;
                    mail2.Remove("_id");
                    messageModel.SaveRow(JsonConvert.SerializeObject(mail2));
                    _logTable.SaveLog(iduser, "Mensajes", "Insert: envio de mensaje", "Message", DateTime.Now.ToString());
                }
                try
                {
                    attach = JsonConvert.DeserializeObject<JArray>(mailjo["attachments"].ToString());
                }
                catch (Exception ex)
                {

                }

                string result = SendExternalMail(userjo["email"].ToString(), username, listmails, mailjo["bodymessage"].ToString(), attach, mailjo["subject"].ToString());

                return "true";
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public JObject getfolio(string msg,string id)
        {
            JObject url = new JObject();
            try
            {
              string[]  folioa = msg.Split('#');
              
              string folio = "";
              if (folioa.Length > 1)
              {
                  folio = folioa[1];
              }
              

                if (folio != msg && folioa.Length>1)
                {
                    folio = folio.Substring(0, 10);
                    int number;
                    if (Int32.TryParse(folio, out number))
                    {
                        url.Add("url", "/ObjectAdmin/Demand/Index?folio=" + folio + "&idmail=" + id);
                    }
                    else
                    {
                        return url;
                    }
                }
                else
                {
                    return url;
                }
                return url;
            }
            catch (Exception ex)
            {
                return new JObject();
            }
        }
        public ActionResult newMails()
        {
            try
            {

                if (this.Request.IsAjaxRequest())
                {
                    if (Session["_id"] != null)
                    {
                        string userid = Session["_id"].ToString();
                        string messages = messageModel.GetRows("CreatedDate");
                        JArray mails = JsonConvert.DeserializeObject<JArray>(messages);
                        JObject mine = new JObject();
                        JArray mailbox = new JArray();
                        string UserId = Session["_id"].ToString();
                        string rowArrayrelation = usermessageModel.Get("userId",UserId);
                        JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                        string sta = "";
                        string read = "";
                        List<string> statux = new List<string>();
                        List<string> readeds = new List<string>();
                        int i = 0;
                        List<string> listpicture = new List<string>();
                        foreach (JObject items in userMessages)
                        {
                            JArray mailsexist = new JArray();
                            //if (UserId == items["userId"].ToString())
                            //{
                            int cont = 0;
                            foreach (JObject x in items["mails"].Reverse())
                                {
                                    if (cont == 6) break;
                                    sta = x["status"].ToString();
                                    statux.Add(sta);
                                    read = x["readed"].ToString();
                                    readeds.Add(read);
                                    cont++;

                                    messages = messageModel.GetRow(x["idmessage"].ToString());
                                    mine = JsonConvert.DeserializeObject<JObject>(messages);
                                    try
                                    {
                                        // if (auxs[i] == "activo" && auxr[i] == "false")
                                        // {
                                        mailbox.Add(mine);
                                        JObject userjobject = new JObject();
                                        try
                                        {
                                            string rowsuser = usersModel.GetRow(mine["userId"].ToString());
                                            userjobject = JsonConvert.DeserializeObject<JObject>(rowsuser);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        JToken value = "";
                                        if (userjobject.TryGetValue("imgext", out value))
                                        {
                                            //Relative path to save images
                                            string relativepath = string.Format("\\Uploads\\Images\\{0}.{1}",
                                                Session["_id"].ToString(), userjobject["imgext"].ToString());

                                            //Check if profile picture file exists on the server
                                            if (System.IO.File.Exists(Server.MapPath(relativepath)))
                                            {
                                                //if it exist, sets the profile picture url
                                                listpicture.Add(Url.Content(relativepath));
                                            }
                                            else
                                            {
                                                //Set picture to default
                                                listpicture.Add("avatar");
                                            }
                                        }
                                        else
                                        {
                                            //Set picture to default
                                            listpicture.Add("avatar");
                                        }

                                        //  }
                                        i++;
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }
                                }
                        //    }
                        }
                        string[] auxs = statux.ToArray();
                        string[] auxr = readeds.ToArray();
                        
                        
                       // foreach (JObject mine in mails.Reverse())
                        //{

                        //    foreach (string x in mine["recipients"])
                        //    {
                        //        if (x.Contains(userid))
                        //        {
                        //            /* string mailcopy=JsonConvert.SerializeObject(mine);
                        //                mailbox.Add(mailcopy);*/
                        //            try
                        //            {
                        //               // if (auxs[i] == "activo" && auxr[i] == "false")
                        //               // {
                        //                    mailbox.Add(mine);
                        //                    JObject userjobject = new JObject();
                        //                    try
                        //                    {
                        //                        string rowsuser = usersModel.GetRow(mine["userId"].ToString());
                        //                        userjobject = JsonConvert.DeserializeObject<JObject>(rowsuser);
                        //                    }
                        //                    catch (Exception ex)
                        //                    {

                        //                    }
                        //                    JToken value = "";
                        //                    if (userjobject.TryGetValue("imgext", out value))
                        //                    {
                        //                        //Relative path to save images
                        //                        string relativepath = string.Format("\\Uploads\\Images\\{0}.{1}",
                        //                            Session["_id"].ToString(), userjobject["imgext"].ToString());

                        //                        //Check if profile picture file exists on the server
                        //                        if (System.IO.File.Exists(Server.MapPath(relativepath)))
                        //                        {
                        //                            //if it exist, sets the profile picture url
                        //                            listpicture.Add(Url.Content(relativepath));
                        //                        }
                        //                        else
                        //                        {
                        //                            //Set picture to default
                        //                            listpicture.Add("avatar");
                        //                        }
                        //                    }
                        //                    else
                        //                    {
                        //                        //Set picture to default
                        //                        listpicture.Add("avatar");
                        //                    }

                        //              //  }
                        //                i++;
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                continue;
                        //            }
                        //        }



                        //    }


                        //}


                        ViewData["Picture"] = listpicture;
                        foreach (JObject item in mailbox)
                        {
                            string folio = "";
                            try
                            {

                                JObject urljo = getfolio(item["bodymessage"].ToString(), item["_id"].ToString());


                                JToken token;
                                if (urljo.TryGetValue("url",out token))
                                {
                                    item.Add("url",urljo["url"].ToString());
                                }
                                else
                                {
                                    item.Add("url", "/Message/Message/getMail?idMail=" + item["_id"].ToString());
                                }
                            }
                            catch (Exception ex) { }
                        }
                        return View(mailbox);
                    }
                }
            }
            catch (Exception)
            {
                return this.Redirect("/Home");
            }
            return this.Redirect("/Home");
        }
        public ActionResult getMailsTable()
        {
            if (this.Request.IsAjaxRequest())
            {
                bool viewAll = getpermissions("messages", "a");

                string userid = Session["_id"].ToString();
                string messages = messageModel.GetRows("CreatedDate");
                JArray mails = JsonConvert.DeserializeObject<JArray>(messages);

                JArray mailbox = new JArray();
                string UserId = Session["_id"].ToString();
                string rowArrayrelation = usermessageModel.Get("userId", userid);
                JObject userMessages = new JObject();
                string sta = "";
                string read = "";
                List<string> readeds = new List<string>();
                List<string> statux = new List<string>();

                List<string> readedlist = new List<string>();
                JArray mailsexist = new JArray();
                try
                {
                    userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation).First() as JObject;
                    
                  
                     readedlist = (from um in userMessages["mails"].Children() where (string)um["readed"] == "true" select (string)um["idmessage"]).ToList();

                     if (UserId == userMessages["userId"].ToString() || viewAll == true)
                    {
                        foreach (JObject x in userMessages["mails"])
                        {



                            sta = x["status"].ToString();
                            statux.Add(sta);
                            read = x["readed"].ToString();
                            readeds.Add(read);

                        }

                    }

                }
                catch (Exception ex) { }
                string[] aux = statux.ToArray();
                string[] auxr = readeds.ToArray();
                List<string> f = new List<string>();
                int i = 0;
                List<string> repeat = new List<string>();
                foreach (JObject mine in mails)
                {
                    foreach (string x in mine["recipients"])
                    {
                        if (x.Contains(userid) || viewAll==true)
                        {
                            /* string mailcopy=JsonConvert.SerializeObject(mine);
                                mailbox.Add(mailcopy);*/
                            try
                            {
                                if (aux[i] == "activo")
                                {
                                    if (!repeat.Contains(mine["_id"].ToString()))
                                    {
                                        mailbox.Add(mine);
                                        f.Add(auxr[i]);
                                        repeat.Add(mine["_id"].ToString());
                                    }
                                }
                            }
                            catch (Exception ex) { }
                            i++;
                        }

                    }
                }

                 if (viewAll == true) { ViewData["viewAll"] = "access"; } else { ViewData["viewAll"] = "no"; }
                ViewData["readed"] = f;
                ViewData["idsread"] = readedlist;
                foreach (JObject item in mailbox)
                {
                   
                    try
                    {

                        JObject urljo = getfolio(item["bodymessage"].ToString(), item["_id"].ToString());


                        JToken token;
                        if (urljo.TryGetValue("url", out token))
                        {
                            item.Add(urljo);
                        }
                        else
                        {
                           // item.Add("url", "/Message/Message/getMail?idMail=" + item["_id"].ToString());
                        }
                    }
                    catch (Exception ex) { }
                }
                return View(mailbox);
            }
            return this.Redirect("/Message/Message");

        }

        public ActionResult searchMail(String data)
        {
            if (this.Request.IsAjaxRequest())
            {
                string userid = Session["_id"].ToString();
                string messages = messageModel.GetRows("CreatedDate");
                JArray mails = JsonConvert.DeserializeObject<JArray>(messages);
                string search = data.ToLower();
                JArray mailbox = new JArray();
                string UserId = Session["_id"].ToString();
                string rowArrayrelation = usermessageModel.GetRows();
                JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                string sta = "";
                string read = "";
                List<string> readeds = new List<string>();
                List<string> statux = new List<string>();
                  bool viewAll = getpermissions("messages", "a");
             

                string rowArray = usersModel.GetRows();
                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray users = JsonConvert.DeserializeObject<JArray>(rowArray);
                List<string> list_recipients = new List<string>();
             
                foreach (JObject items in users)
                {



                    if (items["email"].ToString().ToLower().IndexOf(search) != -1)
                        {
                            list_recipients.Add(items["_id"].ToString());
                       

                        }
                    else if (items["user"].ToString().ToLower().IndexOf(search) != -1)
                        {
                            list_recipients.Add(items["_id"].ToString());
                        
                        }
                    else if (items["name"].ToString().ToLower().IndexOf(search) != -1)
                    {
                        list_recipients.Add(items["_id"].ToString());
                       
                    }
                    else if (items["lastname"].ToString().ToLower().IndexOf(search) != -1)
                    {
                        list_recipients.Add(items["_id"].ToString());
                       
                    }

                   
                    //  items["email"].ToString());

                }



                foreach (JObject items in userMessages)
                {
                    JArray mailsexist = new JArray();
                    if (UserId == items["userId"].ToString()  || viewAll==true)
                    {
                        foreach (JObject x in items["mails"])
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
                foreach (JObject mine in mails)
                {




                    foreach (string x in mine["recipients"])
                    {
                        if (x.Contains(userid) || (userid == mine["Creator"].ToString() && mine["userId"].ToString() !="1") || viewAll == true)
                        {
                            /* string mailcopy=JsonConvert.SerializeObject(mine);
                                mailbox.Add(mailcopy);*/
                            if (aux[i] == "activo")
                            {

                                if (list_recipients.Count == 0)
                                {
                                    if (mine["subject"].ToString().ToLower().IndexOf(search) != -1)
                                    {
                                        if (!repeat.Contains(mine["_id"].ToString()))
                                        {
                                            mailbox.Add(mine);
                                            f.Add(auxr[i]);
                                            repeat.Add(mine["_id"].ToString());
                                        }
                                    }
                                }
                                else if(list_recipients.Contains(mine["userId"].ToString().ToLower()))
                                {
                                    if (!repeat.Contains(mine["_id"].ToString()))
                                    {
                                        mailbox.Add(mine);
                                        f.Add(auxr[i]);
                                        repeat.Add(mine["_id"].ToString());
                                    }
                                }
                                else 
                                {
                                    foreach (string j in mine["recipients"])
                                    {
                                        if (list_recipients.Contains(j.ToLower()))
                                        {
                                            if (!repeat.Contains(mine["_id"].ToString()))
                                            {
                                                mailbox.Add(mine);
                                                f.Add(auxr[i]);
                                                repeat.Add(mine["_id"].ToString());
                                            }
                                        }

                                    }


                                }
                            
                            }
                            i++;
                        }

                    }





                }

                //   string[] arrayrec = mailbox.ToArray();
                //  string jdatos = JsonConvert.SerializeObject(arrayrec);
                //  JArray jarray = JsonConvert.DeserializeObject<JArray>(jdatos);

                ViewData["readed"] = f;
                return View(mailbox);
            }
            return this.Redirect("/Message/Message");

        }

        public ActionResult deleteTable()
        {

            if (this.Request.IsAjaxRequest())
            {
                string userid = Session["_id"].ToString();
                string messages = messageModel.GetRows("CreatedDate");
                JArray mails = JsonConvert.DeserializeObject<JArray>(messages);
                List<string> repeat = new List<string>();
                JArray mailbox = new JArray();
                string UserId = Session["_id"].ToString();
                string rowArrayrelation = usermessageModel.GetRows();
                JArray userMessages = JsonConvert.DeserializeObject<JArray>(rowArrayrelation);
                string sta = "";
                string read = "";
                List<string> readeds = new List<string>();
                List<string> statux = new List<string>();


                foreach (JObject items in userMessages)
                {
                    JArray mailsexist = new JArray();
                    if (UserId == items["userId"].ToString())
                    {
                        foreach (JObject x in items["mails"])
                        {



                            sta = x["status"].ToString();
                            read = x["readed"].ToString();
                            statux.Add(sta);
                            readeds.Add(read);

                        }

                    }

                }
                string[] aux = statux.ToArray();
                string[] auxr = readeds.ToArray();
                List<string> f = new List<string>();
                int i = 0;
                foreach (JObject mine in mails)
                {
                    //string y = mine["status"].ToString();


                    /*  if (userid == mine["userId"].ToString())
                      {
                          mailbox.Add(mine);
                          repeat.Add(mine["_id"].ToString());
                      }*/
                    if (userid == mine["userId"].ToString())
                    {
                        if (mine["status"].ToString() == "recycled")
                        {
                            if (!repeat.Contains(mine["_id"].ToString()))
                            {
                                mailbox.Add(mine);
                                //  f.Add(auxr[i]);

                            }

                        }
                        foreach (string x in mine["recipients"])
                        {
                            if (x.Contains(userid))
                            {
                                i++;
                            }
                        }
                    }
                    else
                    {

                        foreach (string x in mine["recipients"])
                        {
                            if (x.Contains(userid))
                            {
                                /* string mailcopy=JsonConvert.SerializeObject(mine);
                                    mailbox.Add(mailcopy);*/
                                if (aux[i] == "recycled")
                                {
                                    if (!repeat.Contains(mine["_id"].ToString()))
                                    {
                                        mailbox.Add(mine);
                                        f.Add(auxr[i]);

                                    }

                                }
                                i++;

                            }


                        }
                    }

                }
                ViewData["readed"] = f;

                return View(mailbox);
            }
            return this.Redirect("/Message/Message");
        }
        public string attachSet(HttpPostedFileBase attachment)
        {
            //JObject message = JsonConvert.DeserializeObject<JObject>(maildata);

            String Id = "";
            string userid = Session["_id"].ToString();
            string username = Session["LoggedUser"].ToString();
            string filename = "none";


            //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);

            if (attachment != null)
            {
                filename = attachment.FileName.ToString();

            }
            /*  String jsonData = "{'userId':'" + userid + "','userName':'" + username + "','recipients':[none],'subject':none'" +
                 "','bodymessage':none','readed':'false','attachments':'" + filename
                 + "','status': 'wait'}";*/
            /*  message["recipients"] = jarray; 
              message["userId"] = userid;
              message["userName"] = username;
              message["readed"] = "false";
              message["attachments"] = "none";
              message["status"] = "activo";
              String jsonData = JsonConvert.SerializeObject(message);*/


            //Id = messageModel.SaveRow(jsonData, Id);
            string ext = null;
            string patch = "";
            var fecha = DateTime.Now.Ticks;
            patch = userid + fecha;


            if (attachment != null)
            {
                ext = attachment.FileName.Split('.').Last(); //getting the extension
            }
            if (attachment != null)
            {
                string relativepath = "\\Uploads\\Images\\Mail\\";
                string absolutepath = Server.MapPath(relativepath);
                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }
                attachment.SaveAs(absolutepath + "\\" + patch + "." + ext);
                //  patch = relativepath + "\\" + patch + "." + ext;
                patch = patch + "." + ext;
            }
            return patch;



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
    }

}
