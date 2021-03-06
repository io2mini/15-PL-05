﻿

namespace Common.Messages
{

    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
    public partial class NoOperation : Message
    {
        private NoOperationBackupCommunicationServer[] backupCommunicationServersField;
        /// 
        [System.Xml.Serialization.XmlArrayItemAttribute("BackupCommunicationServer", IsNullable = false)]
        public NoOperationBackupCommunicationServer[] BackupCommunicationServers
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
    /// 
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class NoOperationBackupCommunicationServer
    {
        private string addressField;
        private ushort portField;
        private bool portFieldSpecified;
        /// 
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
        /// 
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
        /// 
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