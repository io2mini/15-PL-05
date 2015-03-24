using System.Xml.Serialization;


namespace Common.Messages
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
    public partial class Register : Message
    {

        private RegisterType typeField;

        private string[] solvableProblemsField;

        private byte parallelThreadsField;

        private bool deregisterField;

        private bool deregisterFieldSpecified;

        private ulong idField;

        private bool idFieldSpecified;

        public RegisterType Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("ProblemName", IsNullable = false)]
        public string[] SolvableProblems
        {
            get
            {
                return this.solvableProblemsField;
            }
            set
            {
                this.solvableProblemsField = value;
            }
        }

        public byte ParallelThreads
        {
            get
            {
                return this.parallelThreadsField;
            }
            set
            {
                this.parallelThreadsField = value;
            }
        }

        public bool Deregister
        {
            get
            {
                return this.deregisterField;
            }
            set
            {
                this.deregisterField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DeregisterSpecified
        {
            get
            {
                return this.deregisterFieldSpecified;
            }
            set
            {
                this.deregisterFieldSpecified = value;
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

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public enum RegisterType
    {
        TaskManager,
        ComputationalNode,
        CommunicationServer,
    }
}