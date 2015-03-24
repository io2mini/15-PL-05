using System.Xml.Serialization;

namespace Common
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mini.pw.edu.pl/ucc/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.mini.pw.edu.pl/ucc/", IsNullable=false)]
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

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mini.pw.edu.pl/ucc/")]
    public enum ErrorErrorType {
        UnknownSender,
        InvalidOperation,
        ExceptionOccured,
    }
}