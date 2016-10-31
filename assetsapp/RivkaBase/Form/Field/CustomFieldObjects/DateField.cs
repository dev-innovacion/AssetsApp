using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class DateField : FieldBase
    {
        [Display(Name = "Ingresar fecha actual: ")]
        public bool defaultValue { get; set; }

    }
}