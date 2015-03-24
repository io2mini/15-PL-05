using System.Xml.Serialization;

namespace Common.Messages
{

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
    public partial class SolveRequest : Message
    {

        private string problemTypeField;

        private ulong solvingTimeoutField;

        private bool solvingTimeoutFieldSpecified;

        private byte[] dataField;

        private ulong idField;

        private bool idFieldSpecified;

        public string ProblemType
        {
            get
            {
                return this.problemTypeField;
            }
            set
            {
                this.problemTypeField = value;
            }
        }

        public ulong SolvingTimeout
        {
            get
            {
                return this.solvingTimeoutField;
            }
            set
            {
                this.solvingTimeoutField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SolvingTimeoutSpecified
        {
            get
            {
                return this.solvingTimeoutFieldSpecified;
            }
            set
            {
                this.solvingTimeoutFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] Data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }

        public ulong Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IdSpecified
        {
            get
            {
                return this.idFieldSpecified;
            }
            set
            {
                this.idFieldSpecified = value;
            }
        }
    }
}