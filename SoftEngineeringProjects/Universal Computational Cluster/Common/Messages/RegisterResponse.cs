using System.Xml.Serialization;

namespace Common.Messages
{

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
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

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
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

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class RegisterResponseBackupCommunicationServersBackupCommunicationServer
    {

        private string addressField;

        private ushort portField;

        private bool portFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
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

        [System.Xml.Serialization.XmlAttributeAttribute()]
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

        [System.Xml.Serialization.XmlIgnoreAttribute()]
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