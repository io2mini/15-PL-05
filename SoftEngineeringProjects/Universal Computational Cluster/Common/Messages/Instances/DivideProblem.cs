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
    public partial class DivideProblem : Message
    {

        private string problemTypeField;

        private ulong idField;

        private byte[] dataField;

        private ulong computationalNodesField;

        private ulong nodeIDField;

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

        public ulong ComputationalNodes
        {
            get
            {
                return this.computationalNodesField;
            }
            set
            {
                this.computationalNodesField = value;
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