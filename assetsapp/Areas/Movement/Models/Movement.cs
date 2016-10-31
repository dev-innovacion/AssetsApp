using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RivkaAreas.Movement.Models
{
    public class Movement
    {
        [JsonProperty("_id")]
        public string _id { get; set; }
        
        [JsonProperty("profileId")]
        public string profileId { get; set; }

        [JsonProperty("accountType")]
        public string accountType { get; set; }
        
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("dba")]
        public string dba { get; set; }

    }
}