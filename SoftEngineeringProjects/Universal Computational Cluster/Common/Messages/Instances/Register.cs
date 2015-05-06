using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Common.Messages
{
    [GeneratedCode("xsd", "4.0.30319.18020")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    [XmlRoot(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
    public partial class Register : Message
    {
        private String typeField;

        private string[] solvableProblemsField;

        private byte parallelThreadsField;

        private bool deregisterField;

        private bool deregisterFieldSpecified;

        private ulong idField;

        private bool idFieldSpecified;

        public String Type
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

        [XmlArrayItem("ProblemName", IsNullable = false)]
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

        [XmlIgnore()]
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

        [XmlIgnore()]
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