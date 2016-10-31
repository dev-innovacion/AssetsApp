using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rivka.Form.Field
{
    public class CurrencyField : FieldBase
    {
        [Display(Name = "Valor Predeterminado: ")]
        public int defaultValue { get; set; }

        [Display(Name = "Tipo de moneda: ",Order=6)]
        public string currencyType { get; set; }

        [Display(Name = "Simbolo de moneda: ",Order=7)]
        public string currencySymbol { get; set; }



    }
}