using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class IntegerField : FieldBase
    {
        [Display(Name="Valor predeterminado: ")]
        public int defaultValue { get; set; }
        [Display(Name="Valor mínimo: ")]
        public int minValue { get; set; }
        [Display(Name="Valor máximo: ")]
        public int maxValue { get; set; }
        [Display(Name="Número de caracteres:")]
        public int maxSize { get; set; }

        //Setting the default values
        public IntegerField()
        {
            maxValue = 999999;
            maxSize = 9;
        }

    }
}