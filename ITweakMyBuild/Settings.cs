namespace ITweakMyBuild
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.Serialization;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    [DataContract]
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class Settings
    {
        [NotNull]
        private readonly Tracer _tracer;
        [NotNull]
        private static readonly string _filePath = Path.Combine(VSPackage.ConfigurationFolder, @"ITweakMyBuild.settings");

        private DateTime _fileTime;

        [ImportingConstructor]
        private Settings([NotNull] Tracer tracer)
        {
            _tracer = tracer;

            try
            {
                if (File.Exists(_filePath))
                {
                    JsonConvert.PopulateObject(File.ReadAllText(_filePath), this);
                    _fileTime = File.GetLastWriteTime(_filePath);
                }
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Serialized")]
        [DataMember]
        public Property[] Properties { get; set; }

        public bool HasExternalChanges => !File.Exists(_filePath) || _fileTime != File.GetLastWriteTime(_filePath);

        public void Save()
        {
            try
            {
                File.WriteAllText(_filePath, JsonConvert.SerializeObject(this));
                _fileTime = File.GetLastWriteTime(_filePath);
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
        }
    }

    [DataContract]
    public class Property
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string Comment { get; set; }
    }
}
