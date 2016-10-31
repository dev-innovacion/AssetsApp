using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class CheckBoxField:FieldBase
    {
        [Display(Name="Marcar por default: ")]   
        public bool check { get; set; }

    }
}