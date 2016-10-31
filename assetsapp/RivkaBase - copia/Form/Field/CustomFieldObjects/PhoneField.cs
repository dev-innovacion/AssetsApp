using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class PhoneField : FieldBase
    {
        [RegularExpression("([0-9-])", ErrorMessage = "El nombre de campo custom no debe contener espacios")]
        [Display(Name="Valor predeterminado: ")]
        public string defaultValue { get; set; }
        [Display(Name="Número de caracteres: ")]
        public int maxSize { get; set; }

        //Setting the default values
        public PhoneField()
        {
            maxSize = 15;
        }

    }
}