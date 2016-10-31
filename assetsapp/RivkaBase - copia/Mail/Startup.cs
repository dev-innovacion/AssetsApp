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

using Microsoft.Owin;
using Owin;
[assembly: OwinStartup(typeof(Rivka.Mail.Startup))]
namespace Rivka.Mail
{
    public class Startup
    {
         
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            
            app.MapSignalR();
         
        }
       
    }
}