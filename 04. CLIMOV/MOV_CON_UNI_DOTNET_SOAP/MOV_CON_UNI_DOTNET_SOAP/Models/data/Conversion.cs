using MOV_CON_UNI_DOTNET_SOAP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.data
{
    public class Conversion(Magnitud magnitude, double value, string from, string to)
    {
        public Magnitud Magnitude { get; } = magnitude;
        public double Value { get; } = value;
        public string From { get; } = from;
        public string To { get; } = to;
        public double Result { get; set; }
    }
}
