using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RivkaAreas.User.Models
{
    public class User
    {
        [JsonProperty("userId")]
        public string _id { get; set; }
        [JsonProperty("profileId")]
        public string profileId { get; set; }
    }
}