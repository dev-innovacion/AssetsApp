using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rivka.Mail;
using Rivka.Db;
using Rivka.Db.MongoDb;
using RivkaAreas.Tickets.Controllers;
namespace adminMain.Controllers
{
    public class apiController : ApiController
    {
        protected Messenger messenger = new Messenger();
        protected TicketsController ticket = new TicketsController();
        protected MongoModel Ticketsdb = new MongoModel("Tickets");
        public string Post([FromBody]string value)
        {
            JObject query = JsonConvert.DeserializeObject<JObject>(value);
            switch (query["function"].ToString())
            {
                case "saveTicket":
                    return "";
                case "saveMessages":
                    return saveMsg(query);
                case "closeTicket":
                    return closeTicket(query);
                case "openTicket":
                    return openTicket(query);

            }
            return null;
        }

        
        private string saveMsg(JObject query)
        {
            int countmsjs = 0;
            JArray ent = new JArray();

            try
            {
                foreach (var x in query["document"]["messages"])
                {
                    countmsjs++;
                    ent.Add(x);
                }

                countmsjs++;
            }
            catch (Exception ex)
            {

            }
            string idtick = "";
            string ticket = Ticketsdb.GetRow( query["document"]["ticketId"].ToString());
            JObject ticketx = JsonConvert.DeserializeObject<JObject>(ticket);
            try
            {
             
                idtick = ticketx["_id"].ToString();

                foreach (var x in ticketx["messages"])
                {
                    countmsjs++;
                    ent.Add(x);
                }

                countmsjs++;
            }
            catch (Exception ex)
            {

            }

           
           // string idmsgtime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string idmsgtime = query["document"]["idmessage"].ToString();
            string imageuser = "/Content/Images/imgPerfil/hs.jpg";
            JObject jobjectnew = JsonConvert.DeserializeObject<JObject>("{ \"idmessage\":'" + idmsgtime+ "', \"username\":'" + query["document"]["userName"] + "',\"userid\":'" + query["document"]["userId"] + "', \"message\":'" + query["document"]["messages"] + "',  \"image\":'"+imageuser+"'}");
            ent.Add(jobjectnew);

            String jsonchat = "{'userId':'" + ticketx["userId"] + "','userName':'" + ticketx["userName"] + "','title':'" + ticketx["title"] + "','category':'" + ticketx["category"]
                  + "','bodyticket':'" + ticketx["bodyticket"] + "','readed':'true','attachments':" + ticketx["attachments"] + ",'messages':" + ent + ",'comments':'"+ticketx["comments"]+"','status': '"+ticketx["status"]+"'}";


            string idchat = Ticketsdb.SaveRow(jsonchat, idtick);
            messenger.sendTicketPush("agent", idtick, idmsgtime, query["document"]["userName"].ToString(), query["document"]["messages"].ToString(), imageuser);
            messenger.notTicketPush("agent", idtick, query["document"]["userName"].ToString(), query["document"]["messages"].ToString(), imageuser, query["document"]["idto"].ToString());

            return "send";


        }

        private string closeTicket(JObject query)
        {
            try
            {
                string tickets = Ticketsdb.GetRow(query["document"]["ticketId"].ToString());
                JObject ticket = JsonConvert.DeserializeObject<JObject>(tickets);



                String jsonchat = "{'userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','title':'" + ticket["title"] + "','category':'" + ticket["category"]
                  + "','bodyticket':'" + ticket["bodyticket"] + "','readed':'true','attachments':" + ticket["attachments"] + ",'messages':" + ticket["messages"] + ",'comments':'" + query["document"]["comments"] + "','status': 'closed'}";


                string idchat = Ticketsdb.SaveRow(jsonchat, ticket["_id"].ToString());
                messenger.closeTicketPush(ticket["_id"].ToString());
                return "closed";

            }
            catch (Exception ex)
            {
                return null;
            }
           
        }

        private string openTicket(JObject query)
        {
            try
            {
                string tickets = Ticketsdb.GetRow(query["document"]["ticketId"].ToString());
                JObject ticket = JsonConvert.DeserializeObject<JObject>(tickets);



                String jsonchat = "{'userId':'" + ticket["userId"] + "','userName':'" + ticket["userName"] + "','title':'" + ticket["title"] + "','category':'" + ticket["category"]
                  + "','bodyticket':'" + ticket["bodyticket"] + "','readed':'true','attachments':" + ticket["attachments"] + ",'messages':" + ticket["messages"] + ",'comments':'','status': 'activo'}";


                string idchat = Ticketsdb.SaveRow(jsonchat, ticket["_id"].ToString());
                messenger.openTicketPush(ticket["_id"].ToString());
                return "opened";

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private string deleteMsg(JObject query)
        {


            return null;
        }

    }
}
