using System.Xml.Serialization;

namespace Common.Messages
{

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
    public partial class SolvePartialProblems : Message
    {

        private string problemTypeField;

        private ulong idField;

        private byte[] commonDataField;

        private ulong solvingTimeoutField;

        private bool solvingTimeoutFieldSpecified;

        private SolvePartialProblemsPartialProblem[] partialProblemsField;

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

        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] CommonData
        {
            get
            {
                return this.commonDataField;
            }
            set
            {
                this.commonDataField = value;
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

        [System.Xml.Serialization.XmlArrayItemAttribute("PartialProblem", IsNullable = false)]
        public SolvePartialProblemsPartialProblem[] PartialProblems
        {
            get
            {
                return this.partialProblemsField;
            }
            set
            {
                this.partialProblemsField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class SolvePartialProblemsPartialProblem
    {

        private ulong taskIdField;

        private byte[] dataField;

        private ulong nodeIDField;

        public ulong TaskId
        {
            get
            {
                return this.taskIdField;
            }
            set
            {
                this.taskIdField = value;
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

        public ulong NodeID
        {
            get
            {
                return this.nodeIDField;
            }
            set
            {
                this.nodeIDField = value;
            }
        }
    }
}