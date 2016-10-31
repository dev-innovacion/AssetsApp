
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MongoDB.Bson;
using MongoDB.Driver;

using Rivka.Db.MongoDb;

namespace Rivka.User
{

    public class Permissions
    {



        public static List<string> getPermissions(string id, string tipo)
        {

            MongoModel userTable = new MongoModel("Users");//user's model
            MongoModel profileTable = new MongoModel("Profiles");// profile's model
            String usuarioid = id;

            String profileid = userTable.GetRow(usuarioid);
            JObject rowArray = JsonConvert.DeserializeObject<JObject>(profileid);
            string idpro = rowArray["profileId"].ToString();
            String profiles = profileTable.GetRow(idpro);
            JObject rowArraypro = JsonConvert.DeserializeObject<JObject>(profiles);
            string arraypermisos = rowArraypro["permissionsHTK"].ToString();
            JObject allp = JsonConvert.DeserializeObject<JObject>(arraypermisos);
            var permisos = allp[tipo];
            string datajson = JsonConvert.SerializeObject(permisos);

            JObject allp2 = JsonConvert.DeserializeObject<JObject>(datajson);
            var arraypermisos2 = allp2["grant"].ToArray();

            List<string> access = new List<string>();
            for (int i = 0; i < arraypermisos2.Length; i++)
            {
                access.Add(arraypermisos2[i].ToString());
            }

            return access;
        }
    }
}