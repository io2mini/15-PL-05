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
    public partial class Solutions : Message
    {

        private string problemTypeField;

        private ulong idField;

        private byte[] commonDataField;

        private SolutionsSolution[] solutions1Field;

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

        [XmlElement(DataType = "base64Binary")]
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

        [XmlArray("Solutions")]
        [XmlArrayItem("Solution", IsNullable = false)]
        public SolutionsSolution[] Solutions1
        {
            get
            {
                return this.solutions1Field;
            }
            set
            {
                this.solutions1Field = value;
            }
        }
    }

    [GeneratedCode("xsd", "4.0.30319.18020")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class SolutionsSolution
    {

        private ulong taskIdField;

        private bool taskIdFieldSpecified;

        private bool timeoutOccuredField;

        private SolutionsSolutionType typeField;

        private ulong computationsTimeField;

        private byte[] dataField;

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

        [XmlIgnore()]
        public bool TaskIdSpecified
        {
            get
            {
                return this.taskIdFieldSpecified;
            }
            set
            {
                this.taskIdFieldSpecified = value;
            }
        }

        public bool TimeoutOccured
        {
            get
            {
                return this.timeoutOccuredField;
            }
            set
            {
                this.timeoutOccuredField = value;
            }
        }

        public SolutionsSolutionType Type
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

        public ulong ComputationsTime
        {
            get
            {
                return this.computationsTimeField;
            }
            set
            {
                this.computationsTimeField = value;
            }
        }

        [XmlElement(DataType = "base64Binary")]
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
    }

    [GeneratedCode("xsd", "4.0.30319.18020")]
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public enum SolutionsSolutionType
    {
        Ongoing,
        Partial,
        Final,
    }
}