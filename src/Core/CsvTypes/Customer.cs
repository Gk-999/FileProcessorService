using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace Gk.Core.CsvTypes
{
    [IgnoreFirst(1)]
    [DelimitedRecord(",")]
    [IgnoreEmptyLines]
    public class Customer
    {
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string Customer_Salutation { get; set; }
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string Customer_Name { get; set; }
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string Customer_Surname { get; set; }
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string Customer_Company { get; set; }
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        public string Customer_Email { get; set; }
        [FieldQuoted('"', QuoteMode.OptionalForBoth)]
        [FieldConverter(typeof(DateFormatter))]
        public DateTime? Customer_DateOfBirth { get; set; }
    }

    public class DateFormatter : ConverterBase
    {
        public override object StringToField(string from)
        {
            return Convert.ToDateTime(from);
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null)
                return string.Empty;

            return ((DateTime)fieldValue).ToString("dd'/'MM'/'yyyy");
        }

    }
}
