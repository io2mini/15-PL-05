﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.34014
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------

using Common.Messages;
using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.33440.
// 


/// <uwagi/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mini.pw.edu.pl/ucc/")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.mini.pw.edu.pl/ucc/", IsNullable=false)]
public partial class SolutionRequest : Message {
    
    private ulong idField;
    
    /// <uwagi/>
    public ulong Id {
        get {
            return this.idField;
        }
        set {
            this.idField = value;
        }
    }
}