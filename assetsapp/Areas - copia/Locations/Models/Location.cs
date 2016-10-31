using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RivkaAreas.Locations.Models
{
    public class Location
    {
        [JsonProperty("locationId")]
        public string _id { get; set; }
        [JsonProperty("profileId")]
        public string profileId { get; set; }
    }
}