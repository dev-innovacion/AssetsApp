using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class DecimalField : FieldBase
    {
        [Display(Name = "Valor Predeterminado: ")]
        public int defaultValue { get; set; }

        [Display(Name = "Tamaño máximo: ")]
        public int maxSize { get; set; }

        [Display(Name = "Precisión: ")]
        public int precision { get; set; }

        //Setting the default values
        public DecimalField()
        {
            maxSize = 128;
        }

    }
}