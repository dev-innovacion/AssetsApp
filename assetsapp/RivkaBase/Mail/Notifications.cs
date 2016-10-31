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
using Rivka.Files;
using System.Net;
using Rivka.Mail;
using Rivka.Error;
namespace Rivka.Mail
{
   
       public class Notifications
    {

           protected MongoModel notificationsDb;
           protected Messenger push;
           public Notifications()
           {
               notificationsDb = new MongoModel("Notifications");
               push = new Messenger();
           }  

           public void saveNotification(string module, string type="info",string msg="",string icon="icon-info",bool save=true){

               JObject newNotification = new JObject();
               newNotification.Add("module", module);
               newNotification.Add("type", type);
               newNotification.Add("msg", msg);
               newNotification.Add("icon", icon);
               string date=DateTime.Now.ToString("dd/MM/yyyy");
               if (save == true)
               {
                   try
                   {
                       string id = notificationsDb.SaveRow(newNotification.ToString());
                   }
                   catch (Exception e)
                   {
                       Error.Error.Log(e, "Trying to save notification");
                   }
               }
               String jsonData = "{\"module\":\"" + module + "\",\"type\":\"" + type + "\",\"msg\":\"" + msg + "\",\"date\":\"" + date + "\",\"icon\":\"" + icon + "\"}";
               push.pushMessage(jsonData, "newNotification");
           }
    }
}