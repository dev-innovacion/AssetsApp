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
using System.IO;
using System.util;
using System.Net; 
using System.Text.RegularExpressions;
using Rivka.Security;
using Rivka.Error;
using System.Reflection;
using System.Text;

namespace RivkaAreas.Semaphore.Controllers
{
    public class SemaphoreController : Controller
    {
        //
        // GET: /Semaphore/Semaphore/
        protected MongoModel semaphoredb;
        protected MongoModel movementprofiledb;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;

          public SemaphoreController()
          {
              semaphoredb = new MongoModel("Semaphore");
              movementprofiledb = new MongoModel("MovementProfiles");
              _logTable = new LogBook.Controllers.LogBookController();
          }
        public ActionResult Index()
        {
           /* string movprofiles = movementprofiledb.GetRows();
            JArray movja = JsonConvert.DeserializeObject<JArray>(movprofiles);
            List<string> listmovements = (from mov in movja select (string)mov["typeMovement"]).ToList();*/

            return View();
        }
        public ActionResult GetSemaphoreTable()
        {
            JArray data = new JArray();
            try
            {
                string semaphoredata = semaphoredb.GetRows();
                data = JsonConvert.DeserializeObject<JArray>(semaphoredata);
            }
            catch (Exception ex)
            {

            }
            return View(data);
        }
        public bool saveSemaphore(string color, string typemov, int days,string id=null)
        {
             
            JObject info = new JObject();
        
            info.Add("color", color);
            info.Add("typeMovement", typemov);
           
            info.Add("days", days.ToString());
            string jsondata = JsonConvert.SerializeObject(info);

            try
            {
                id = semaphoredb.SaveRow(jsondata, id);
                try
                {
                    _logTable.SaveLog(Session["_id"].ToString(), "Semaforizacion", "Insert: Se ha creado o modificado un semaforo.", "Semaphore", DateTime.Now.ToString());
                }
                catch { }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool deleteSemaphore(string id)
        {
            if(id==null || id=="")
                return false;
          try
            {
                semaphoredb.DeleteRow(id);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public String EditSemaphore(string id)
        {
             
              String response ="[]";
            try{
                string semaphoredata = semaphoredb.GetRow(id);
                response = semaphoredata;
            }catch(Exception ex){
                return null;
            }

            return response;
        }
    }
}
