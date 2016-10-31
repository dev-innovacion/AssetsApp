using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class DateTimeField : FieldBase
    {
        [Display(Name = "Ingresar fecha actual: ")]
        public bool defaultDate { get; set; }
        [Display(Name = "Ingresar Hora actual: ")]
        public bool defaultHour { get; set; }


    }
}