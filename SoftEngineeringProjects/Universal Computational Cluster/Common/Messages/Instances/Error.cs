using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Common.Messages
{
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType=true, Namespace="http://www.mini.pw.edu.pl/ucc/")]
    [XmlRoot(Namespace="http://www.mini.pw.edu.pl/ucc/", IsNullable=false)]
    public partial class Error : Message {
    
        private ErrorErrorType errorTypeField;
    
        private string errorMessageField;
    
        public ErrorErrorType ErrorType {
            get {
                return this.errorTypeField;
            }
            set {
                this.errorTypeField = value;
            }
        }

        public string ErrorMessage {
            get {
                return this.errorMessageField;
            }
            set {
                this.errorMessageField = value;
            }
        }
    }

    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [XmlType(AnonymousType=true, Namespace="http://www.mini.pw.edu.pl/ucc/")]
    public enum ErrorErrorType {
        UnknownSender,
        InvalidOperation,
        ExceptionOccured,
    }
}