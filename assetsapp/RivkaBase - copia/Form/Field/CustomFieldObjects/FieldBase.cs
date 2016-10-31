using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class FieldBase
    {
        [HiddenInput(DisplayValue = false)]
        public string _id { get; set; }

        [RegularExpression("([a-zA-Z0-9-_.]*)", ErrorMessage = "El nombre de campo custom no debe contener espacios")]
        [MinLength(4, ErrorMessage = "El nombre del campo custom debe ser mínimo de cuatro dígitos")]
        [Required(ErrorMessage="Este campo es requerido")]
        [Display(Name = "Nombre: ", Order = 1)]
        public string name { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Etiqueta: ", Order = 2)]
        public string label { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string type { get; set; }


        [Display(Name = "Tooltip: ", Order = 3)]
        public string toolTip { get; set; }
    }
}
