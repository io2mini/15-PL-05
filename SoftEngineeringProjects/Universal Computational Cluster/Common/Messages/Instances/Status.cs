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
    public partial class Status : Message
    {

        private ulong idField;

        private StatusThread[] threadsField;

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

        [XmlArrayItem("Thread", IsNullable = false)]
        public StatusThread[] Threads
        {
            get
            {
                return this.threadsField;
            }
            set
            {
                this.threadsField = value;
            }
        }
    }

    [GeneratedCode("xsd", "4.0.30319.18020")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class StatusThread
    {

        private StatusThreadState stateField;

        private ulong howLongField;

        private bool howLongFieldSpecified;

        private ulong problemInstanceIdField;

        private bool problemInstanceIdFieldSpecified;

        private ulong taskIdField;

        private bool taskIdFieldSpecified;

        private string problemTypeField;

        public StatusThreadState State
        {
            get
            {
                return this.stateField;
            }
            set
            {
                this.stateField = value;
            }
        }

        public ulong HowLong
        {
            get
            {
                return this.howLongField;
            }
            set
            {
                this.howLongField = value;
            }
        }

        [XmlIgnore()]
        public bool HowLongSpecified
        {
            get
            {
                return this.howLongFieldSpecified;
            }
            set
            {
                this.howLongFieldSpecified = value;
            }
        }

        public ulong ProblemInstanceId
        {
            get
            {
                return this.problemInstanceIdField;
            }
            set
            {
                this.problemInstanceIdField = value;
            }
        }

        [XmlIgnore()]
        public bool ProblemInstanceIdSpecified
        {
            get
            {
                return this.problemInstanceIdFieldSpecified;
            }
            set
            {
                this.problemInstanceIdFieldSpecified = value;
            }
        }

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
    }

    [GeneratedCode("xsd", "4.0.30319.18020")]
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public enum StatusThreadState
    {
        Idle,
        Busy,
    }
}