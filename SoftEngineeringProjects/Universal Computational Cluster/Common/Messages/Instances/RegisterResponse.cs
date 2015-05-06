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
    public partial class RegisterResponse : Message
    {

        private ulong idField;

        private uint timeoutField;

        private RegisterResponseBackupCommunicationServers backupCommunicationServersField;

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

        public uint Timeout
        {
            get
            {
                return this.timeoutField;
            }
            set
            {
                this.timeoutField = value;
            }
        }

        public RegisterResponseBackupCommunicationServers BackupCommunicationServers
        {
            get
            {
                return this.backupCommunicationServersField;
            }
            set
            {
                this.backupCommunicationServersField = value;
            }
        }
    }

    [GeneratedCode("xsd", "4.0.30319.18020")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class RegisterResponseBackupCommunicationServers
    {

        private RegisterResponseBackupCommunicationServersBackupCommunicationServer backupCommunicationServerField;

        public RegisterResponseBackupCommunicationServersBackupCommunicationServer BackupCommunicationServer
        {
            get
            {
                return this.backupCommunicationServerField;
            }
            set
            {
                this.backupCommunicationServerField = value;
            }
        }
    }

    [GeneratedCode("xsd", "4.0.30319.18020")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class RegisterResponseBackupCommunicationServersBackupCommunicationServer
    {

        private string addressField;

        private ushort portField;

        private bool portFieldSpecified;

        [XmlAttribute(DataType = "anyURI")]
        public string address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }

        [XmlAttribute()]
        public ushort port
        {
            get
            {
                return this.portField;
            }
            set
            {
                this.portField = value;
            }
        }

        [XmlIgnore()]
        public bool portSpecified
        {
            get
            {
                return this.portFieldSpecified;
            }
            set
            {
                this.portFieldSpecified = value;
            }
        }
    }
}