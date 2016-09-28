namespace ITweakMyBuild
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class CustomTargetsFile
    {
        private static readonly string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"tom-englert.de", @"ITweakMyBuild", @"ITweakMyBuild.targets");
        private static readonly XNamespace _xmlns = @"http://schemas.microsoft.com/developer/msbuild/2003";
        private static readonly XName _projectName = _xmlns + @"Project";
        private static readonly XName _propertyGroupName = _xmlns + @"PropertyGroup";

        private readonly XDocument _document;
        private readonly XElement _propertyGroup;

        public CustomTargetsFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        _document = XDocument.Load(_filePath);
                        return;
                    }
                    catch
                    {
                        // document is corrupt...
                    }
                }

                _document = new XDocument(new XElement(_projectName, new XElement(_propertyGroupName)));
            }
            finally
            {
                if (_document.Root == null)
                    _document.Add(new XElement(_projectName));

                Contract.Assume(_document.Root != null);

                _propertyGroup = _document.Root.Ancestors(_projectName).FirstOrDefault();

                if (_propertyGroup == null)
                {
                    _propertyGroup = new XElement(_propertyGroupName);
                    _document.Root.Add( _propertyGroup);
                }
            }
        }

        public void Save()
        {
            _document.Save(_filePath);
        }

        public IReadOnlyDictionary<string, string> Properties { get; } 
    }
}
