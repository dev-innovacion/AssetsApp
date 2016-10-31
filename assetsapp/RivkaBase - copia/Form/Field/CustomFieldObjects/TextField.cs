using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class TextField : FieldBase
    {
        [Display(Name = "PlaceHolder: ")]
        public string placeHolder { get; set; }

        [Display(Name = "Tamaño máximo: ")]
        [DefaultValue((int)128)]
        public int maxSize { get; set; }

        [Display(Name = "Expresión regular: ")]
        public string regex { get; set; }

        //Setting the default values
        public TextField()
        {
            maxSize = 128;
        }
    }
}