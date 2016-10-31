using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class TextAreaField : FieldBase
    {
        [Display(Name = "PlaceHolder: ")]
        public string placeHolder { get; set; }

        [Range(1, Int32.MaxValue, ErrorMessage = "Debe ser un número entero mayor que uno")]
        [Display(Name = "Filas: ")]
        public int rows { get; set; }

        [Range(1, Int32.MaxValue, ErrorMessage = "Debe ser un número entero mayor que uno")]
        [Display(Name = "Columnas: ")]
        public int columns { get; set; }

        [Display(Name = "Valor predeterminado: ")]
        public string defaultValue { get; set; }


        [Display(Name = "Expresión regular: ")]
        public string regex { get; set; }

        //Setting the default values
        public TextAreaField()
        {
            rows = 3;
            columns = 4;
        }

    }
}