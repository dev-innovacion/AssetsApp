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
using System.Web.SessionState;
using MongoDB.Driver;
using System.Net;
using Rivka.Form.Field;
using Rivka.Form;
using Rivka.User;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Infrastructure;
using System.Threading;

namespace Rivka.Mail
{
    [HubName("messenger")]
    public class Messenger : Hub
    {

        private IHubContext hubContext;

        /// <summary>
        ///     Initializes the models and the hub
        /// </summary>
        public Messenger()
        {
            hubContext = GlobalHost.ConnectionManager.GetHubContext<Messenger>(); //used to send message to the clients
        }

        /// <summary>
        ///     Allows to push a message to the clients
        /// </summary>
        /// <param name="messageJson">Message to send</param>
        /// <param name="methodName">Method name in the client</param>
        /// <author>Quijada Romero Luis Gonzalo</author>
        public void pushMessage(String messageJson, String methodName)
        {
            hubContext.Clients.All.Invoke(methodName, messageJson);
        }

        /*-------------------------------------------------------------------------------*/
        public void sendMailPush(string message)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Messenger>();

            hubContext.Clients.All.enviar(message);

        }

        public void sendTicketPush(string userid, string idt, string id, string username, string message, string image)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Messenger>();


            hubContext.Clients.All.ticket(userid, idt, id, username, message, image);

        }
        public void closeTicketPush(string idticketclosed)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Messenger>();


            hubContext.Clients.All.ticketclose(idticketclosed);

        }
        public void closeConfirmTicketPush(string idticketclosed)
        {
            hubContext.Clients.All.ticketcloseconfirm(idticketclosed);
        }
        public void closeClientTicketPush(string idticketclosed)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Messenger>();


            hubContext.Clients.All.ticketcloseclient(idticketclosed);

        }
        public void openTicketPush(string idticketopened)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Messenger>();


            hubContext.Clients.All.ticketopen(idticketopened);

        }
        public void notTicketPush(string userid, string idt, string username, string message, string image, string idto)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<Messenger>();


            hubContext.Clients.All.ticketnot(userid, idt, username, message, image, idto);

        }
    }
}