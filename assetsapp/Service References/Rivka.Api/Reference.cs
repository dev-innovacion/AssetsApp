﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.0
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace autobuildHTK_client.Rivka.Api {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Status", Namespace="http://schemas.datacontract.org/2004/07/WCFServiceHost")]
    [System.SerializableAttribute()]
    public partial class Status : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AddAntennaField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AddPrinterField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool CloseLabelField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ConnectField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DisconnectField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string GetAntennaField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string GetPrinterField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool GetVariablesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool OpenLabelField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PrintField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int QuantityField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ReadField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string SetEPCField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string[] VariablesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string[] WriteField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AddAntenna {
            get {
                return this.AddAntennaField;
            }
            set {
                if ((object.ReferenceEquals(this.AddAntennaField, value) != true)) {
                    this.AddAntennaField = value;
                    this.RaisePropertyChanged("AddAntenna");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AddPrinter {
            get {
                return this.AddPrinterField;
            }
            set {
                if ((object.ReferenceEquals(this.AddPrinterField, value) != true)) {
                    this.AddPrinterField = value;
                    this.RaisePropertyChanged("AddPrinter");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool CloseLabel {
            get {
                return this.CloseLabelField;
            }
            set {
                if ((this.CloseLabelField.Equals(value) != true)) {
                    this.CloseLabelField = value;
                    this.RaisePropertyChanged("CloseLabel");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Connect {
            get {
                return this.ConnectField;
            }
            set {
                if ((object.ReferenceEquals(this.ConnectField, value) != true)) {
                    this.ConnectField = value;
                    this.RaisePropertyChanged("Connect");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Disconnect {
            get {
                return this.DisconnectField;
            }
            set {
                if ((object.ReferenceEquals(this.DisconnectField, value) != true)) {
                    this.DisconnectField = value;
                    this.RaisePropertyChanged("Disconnect");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string GetAntenna {
            get {
                return this.GetAntennaField;
            }
            set {
                if ((object.ReferenceEquals(this.GetAntennaField, value) != true)) {
                    this.GetAntennaField = value;
                    this.RaisePropertyChanged("GetAntenna");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string GetPrinter {
            get {
                return this.GetPrinterField;
            }
            set {
                if ((object.ReferenceEquals(this.GetPrinterField, value) != true)) {
                    this.GetPrinterField = value;
                    this.RaisePropertyChanged("GetPrinter");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool GetVariables {
            get {
                return this.GetVariablesField;
            }
            set {
                if ((this.GetVariablesField.Equals(value) != true)) {
                    this.GetVariablesField = value;
                    this.RaisePropertyChanged("GetVariables");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool OpenLabel {
            get {
                return this.OpenLabelField;
            }
            set {
                if ((this.OpenLabelField.Equals(value) != true)) {
                    this.OpenLabelField = value;
                    this.RaisePropertyChanged("OpenLabel");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Print {
            get {
                return this.PrintField;
            }
            set {
                if ((object.ReferenceEquals(this.PrintField, value) != true)) {
                    this.PrintField = value;
                    this.RaisePropertyChanged("Print");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Quantity {
            get {
                return this.QuantityField;
            }
            set {
                if ((this.QuantityField.Equals(value) != true)) {
                    this.QuantityField = value;
                    this.RaisePropertyChanged("Quantity");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Read {
            get {
                return this.ReadField;
            }
            set {
                if ((object.ReferenceEquals(this.ReadField, value) != true)) {
                    this.ReadField = value;
                    this.RaisePropertyChanged("Read");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string SetEPC {
            get {
                return this.SetEPCField;
            }
            set {
                if ((object.ReferenceEquals(this.SetEPCField, value) != true)) {
                    this.SetEPCField = value;
                    this.RaisePropertyChanged("SetEPC");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string[] Variables {
            get {
                return this.VariablesField;
            }
            set {
                if ((object.ReferenceEquals(this.VariablesField, value) != true)) {
                    this.VariablesField = value;
                    this.RaisePropertyChanged("Variables");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string[] Write {
            get {
                return this.WriteField;
            }
            set {
                if ((object.ReferenceEquals(this.WriteField, value) != true)) {
                    this.WriteField = value;
                    this.RaisePropertyChanged("Write");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AntennaClass", Namespace="http://schemas.datacontract.org/2004/07/WCFServiceHost")]
    [System.SerializableAttribute()]
    public partial class AntennaClass : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IPField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ModelField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string IP {
            get {
                return this.IPField;
            }
            set {
                if ((object.ReferenceEquals(this.IPField, value) != true)) {
                    this.IPField = value;
                    this.RaisePropertyChanged("IP");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Model {
            get {
                return this.ModelField;
            }
            set {
                if ((object.ReferenceEquals(this.ModelField, value) != true)) {
                    this.ModelField = value;
                    this.RaisePropertyChanged("Model");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="Rivka.Api.IPrintService")]
    public interface IPrintService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Connect", ReplyAction="http://tempuri.org/IPrintService/ConnectResponse")]
        bool Connect(string Printer);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Connect", ReplyAction="http://tempuri.org/IPrintService/ConnectResponse")]
        System.Threading.Tasks.Task<bool> ConnectAsync(string Printer);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Disconnect", ReplyAction="http://tempuri.org/IPrintService/DisconnectResponse")]
        bool Disconnect(string Printer);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Disconnect", ReplyAction="http://tempuri.org/IPrintService/DisconnectResponse")]
        System.Threading.Tasks.Task<bool> DisconnectAsync(string Printer);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/OpenLabel", ReplyAction="http://tempuri.org/IPrintService/OpenLabelResponse")]
        bool OpenLabel();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/OpenLabel", ReplyAction="http://tempuri.org/IPrintService/OpenLabelResponse")]
        System.Threading.Tasks.Task<bool> OpenLabelAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/CloseLabel", ReplyAction="http://tempuri.org/IPrintService/CloseLabelResponse")]
        bool CloseLabel();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/CloseLabel", ReplyAction="http://tempuri.org/IPrintService/CloseLabelResponse")]
        System.Threading.Tasks.Task<bool> CloseLabelAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Print", ReplyAction="http://tempuri.org/IPrintService/PrintResponse")]
        bool Print(string Printer, int Quantity);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Print", ReplyAction="http://tempuri.org/IPrintService/PrintResponse")]
        System.Threading.Tasks.Task<bool> PrintAsync(string Printer, int Quantity);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetVariables", ReplyAction="http://tempuri.org/IPrintService/GetVariablesResponse")]
        string[] GetVariables();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetVariables", ReplyAction="http://tempuri.org/IPrintService/GetVariablesResponse")]
        System.Threading.Tasks.Task<string[]> GetVariablesAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/SetEPC", ReplyAction="http://tempuri.org/IPrintService/SetEPCResponse")]
        bool SetEPC(string EPC);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/SetEPC", ReplyAction="http://tempuri.org/IPrintService/SetEPCResponse")]
        System.Threading.Tasks.Task<bool> SetEPCAsync(string EPC);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetStatus", ReplyAction="http://tempuri.org/IPrintService/GetStatusResponse")]
        autobuildHTK_client.Rivka.Api.Status GetStatus();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetStatus", ReplyAction="http://tempuri.org/IPrintService/GetStatusResponse")]
        System.Threading.Tasks.Task<autobuildHTK_client.Rivka.Api.Status> GetStatusAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetStatus2", ReplyAction="http://tempuri.org/IPrintService/GetStatus2Response")]
        autobuildHTK_client.Rivka.Api.Status GetStatus2();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetStatus2", ReplyAction="http://tempuri.org/IPrintService/GetStatus2Response")]
        System.Threading.Tasks.Task<autobuildHTK_client.Rivka.Api.Status> GetStatus2Async();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/AddPrinter", ReplyAction="http://tempuri.org/IPrintService/AddPrinterResponse")]
        void AddPrinter(string Printer);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/AddPrinter", ReplyAction="http://tempuri.org/IPrintService/AddPrinterResponse")]
        System.Threading.Tasks.Task AddPrinterAsync(string Printer);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetPrinter", ReplyAction="http://tempuri.org/IPrintService/GetPrinterResponse")]
        string GetPrinter();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetPrinter", ReplyAction="http://tempuri.org/IPrintService/GetPrinterResponse")]
        System.Threading.Tasks.Task<string> GetPrinterAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/QuitPrinter", ReplyAction="http://tempuri.org/IPrintService/QuitPrinterResponse")]
        void QuitPrinter();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/QuitPrinter", ReplyAction="http://tempuri.org/IPrintService/QuitPrinterResponse")]
        System.Threading.Tasks.Task QuitPrinterAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Play", ReplyAction="http://tempuri.org/IPrintService/PlayResponse")]
        bool Play(string IP);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Play", ReplyAction="http://tempuri.org/IPrintService/PlayResponse")]
        System.Threading.Tasks.Task<bool> PlayAsync(string IP);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Pause", ReplyAction="http://tempuri.org/IPrintService/PauseResponse")]
        bool Pause(string IP);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Pause", ReplyAction="http://tempuri.org/IPrintService/PauseResponse")]
        System.Threading.Tasks.Task<bool> PauseAsync(string IP);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Stop", ReplyAction="http://tempuri.org/IPrintService/StopResponse")]
        bool Stop(string IP);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Stop", ReplyAction="http://tempuri.org/IPrintService/StopResponse")]
        System.Threading.Tasks.Task<bool> StopAsync(string IP);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Read", ReplyAction="http://tempuri.org/IPrintService/ReadResponse")]
        string[] Read(string IP);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Read", ReplyAction="http://tempuri.org/IPrintService/ReadResponse")]
        System.Threading.Tasks.Task<string[]> ReadAsync(string IP);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Write", ReplyAction="http://tempuri.org/IPrintService/WriteResponse")]
        void Write(string IP, string[] tags);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/Write", ReplyAction="http://tempuri.org/IPrintService/WriteResponse")]
        System.Threading.Tasks.Task WriteAsync(string IP, string[] tags);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/AddAntenna", ReplyAction="http://tempuri.org/IPrintService/AddAntennaResponse")]
        void AddAntenna(string Antenna, string Model);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/AddAntenna", ReplyAction="http://tempuri.org/IPrintService/AddAntennaResponse")]
        System.Threading.Tasks.Task AddAntennaAsync(string Antenna, string Model);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetAntenna", ReplyAction="http://tempuri.org/IPrintService/GetAntennaResponse")]
        autobuildHTK_client.Rivka.Api.AntennaClass[] GetAntenna();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/GetAntenna", ReplyAction="http://tempuri.org/IPrintService/GetAntennaResponse")]
        System.Threading.Tasks.Task<autobuildHTK_client.Rivka.Api.AntennaClass[]> GetAntennaAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/QuitAntenna", ReplyAction="http://tempuri.org/IPrintService/QuitAntennaResponse")]
        void QuitAntenna();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPrintService/QuitAntenna", ReplyAction="http://tempuri.org/IPrintService/QuitAntennaResponse")]
        System.Threading.Tasks.Task QuitAntennaAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IPrintServiceChannel : autobuildHTK_client.Rivka.Api.IPrintService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PrintServiceClient : System.ServiceModel.ClientBase<autobuildHTK_client.Rivka.Api.IPrintService>, autobuildHTK_client.Rivka.Api.IPrintService {
        
        public PrintServiceClient() {
        }
        
        public PrintServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public PrintServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PrintServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PrintServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool Connect(string Printer) {
            return base.Channel.Connect(Printer);
        }
        
        public System.Threading.Tasks.Task<bool> ConnectAsync(string Printer) {
            return base.Channel.ConnectAsync(Printer);
        }
        
        public bool Disconnect(string Printer) {
            return base.Channel.Disconnect(Printer);
        }
        
        public System.Threading.Tasks.Task<bool> DisconnectAsync(string Printer) {
            return base.Channel.DisconnectAsync(Printer);
        }
        
        public bool OpenLabel() {
            return base.Channel.OpenLabel();
        }
        
        public System.Threading.Tasks.Task<bool> OpenLabelAsync() {
            return base.Channel.OpenLabelAsync();
        }
        
        public bool CloseLabel() {
            return base.Channel.CloseLabel();
        }
        
        public System.Threading.Tasks.Task<bool> CloseLabelAsync() {
            return base.Channel.CloseLabelAsync();
        }
        
        public bool Print(string Printer, int Quantity) {
            return base.Channel.Print(Printer, Quantity);
        }
        
        public System.Threading.Tasks.Task<bool> PrintAsync(string Printer, int Quantity) {
            return base.Channel.PrintAsync(Printer, Quantity);
        }
        
        public string[] GetVariables() {
            return base.Channel.GetVariables();
        }
        
        public System.Threading.Tasks.Task<string[]> GetVariablesAsync() {
            return base.Channel.GetVariablesAsync();
        }
        
        public bool SetEPC(string EPC) {
            return base.Channel.SetEPC(EPC);
        }
        
        public System.Threading.Tasks.Task<bool> SetEPCAsync(string EPC) {
            return base.Channel.SetEPCAsync(EPC);
        }
        
        public autobuildHTK_client.Rivka.Api.Status GetStatus() {
            return base.Channel.GetStatus();
        }
        
        public System.Threading.Tasks.Task<autobuildHTK_client.Rivka.Api.Status> GetStatusAsync() {
            return base.Channel.GetStatusAsync();
        }
        
        public autobuildHTK_client.Rivka.Api.Status GetStatus2() {
            return base.Channel.GetStatus2();
        }
        
        public System.Threading.Tasks.Task<autobuildHTK_client.Rivka.Api.Status> GetStatus2Async() {
            return base.Channel.GetStatus2Async();
        }
        
        public void AddPrinter(string Printer) {
            base.Channel.AddPrinter(Printer);
        }
        
        public System.Threading.Tasks.Task AddPrinterAsync(string Printer) {
            return base.Channel.AddPrinterAsync(Printer);
        }
        
        public string GetPrinter() {
            return base.Channel.GetPrinter();
        }
        
        public System.Threading.Tasks.Task<string> GetPrinterAsync() {
            return base.Channel.GetPrinterAsync();
        }
        
        public void QuitPrinter() {
            base.Channel.QuitPrinter();
        }
        
        public System.Threading.Tasks.Task QuitPrinterAsync() {
            return base.Channel.QuitPrinterAsync();
        }
        
        public bool Play(string IP) {
            return base.Channel.Play(IP);
        }
        
        public System.Threading.Tasks.Task<bool> PlayAsync(string IP) {
            return base.Channel.PlayAsync(IP);
        }
        
        public bool Pause(string IP) {
            return base.Channel.Pause(IP);
        }
        
        public System.Threading.Tasks.Task<bool> PauseAsync(string IP) {
            return base.Channel.PauseAsync(IP);
        }
        
        public bool Stop(string IP) {
            return base.Channel.Stop(IP);
        }
        
        public System.Threading.Tasks.Task<bool> StopAsync(string IP) {
            return base.Channel.StopAsync(IP);
        }
        
        public string[] Read(string IP) {
            return base.Channel.Read(IP);
        }
        
        public System.Threading.Tasks.Task<string[]> ReadAsync(string IP) {
            return base.Channel.ReadAsync(IP);
        }
        
        public void Write(string IP, string[] tags) {
            base.Channel.Write(IP, tags);
        }
        
        public System.Threading.Tasks.Task WriteAsync(string IP, string[] tags) {
            return base.Channel.WriteAsync(IP, tags);
        }
        
        public void AddAntenna(string Antenna, string Model) {
            base.Channel.AddAntenna(Antenna, Model);
        }
        
        public System.Threading.Tasks.Task AddAntennaAsync(string Antenna, string Model) {
            return base.Channel.AddAntennaAsync(Antenna, Model);
        }
        
        public autobuildHTK_client.Rivka.Api.AntennaClass[] GetAntenna() {
            return base.Channel.GetAntenna();
        }
        
        public System.Threading.Tasks.Task<autobuildHTK_client.Rivka.Api.AntennaClass[]> GetAntennaAsync() {
            return base.Channel.GetAntennaAsync();
        }
        
        public void QuitAntenna() {
            base.Channel.QuitAntenna();
        }
        
        public System.Threading.Tasks.Task QuitAntennaAsync() {
            return base.Channel.QuitAntennaAsync();
        }
    }
}
