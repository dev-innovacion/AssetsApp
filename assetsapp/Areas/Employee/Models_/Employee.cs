using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RivkaAreas.Employee.Models
{
    public class Employee
    {
        [JsonProperty("employeeId")]
        public string _id { get; set; }
        [JsonProperty("profileId")]
        public string profileId { get; set; }
    }
}