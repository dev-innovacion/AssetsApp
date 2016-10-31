using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class FileField : FieldBase
    {
        //TODO: Falta declarar FileField
        [Display(Name="Tamaño máximo (Megabytes)")]
        [Range(0,double.MaxValue,ErrorMessage="El valor debe ser mayor que cero")]
        [Required]
        public double MaxSize { get; set; }
    }
}