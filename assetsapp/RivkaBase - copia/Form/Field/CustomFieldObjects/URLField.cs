using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Remoting.Activation;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class URLField : FieldBase
    {
        [Display(Name="Valor Predeterminado: ")]
        public string defaultValue { get; set; }

        [Display(Name="Tamaño máximo: ")]
        public int maxSize { get; set; }

        [Display(Name="Abrir en nueva ventana: ")]
        public bool openonnewwindow { get; set; }

        //Setting the default values
        public URLField()
        {
            maxSize = 128;
        }

    }
}