namespace ITweakMyBuild
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using JetBrains.Annotations;

    using TomsToolbox.Core;

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CustomTargetsFile
    {
        [NotNull]
        private readonly Tracer _tracer;
        private static readonly string _filePath = Path.Combine(VSPackage.ConfigurationFolder, @"ITweakMyBuild.targets");
        private static readonly XNamespace _xmlns = @"http://schemas.microsoft.com/developer/msbuild/2003";
        private static readonly XName _projectName = _xmlns + @"Project";
        private static readonly XName _propertyGroupName = _xmlns + @"PropertyGroup";

        [NotNull]
        private readonly XDocument _document;
        [NotNull]
        private readonly XElement _propertyGroup;

        [ImportingConstructor]
        private CustomTargetsFile([NotNull] Tracer tracer)
        {
            _tracer = tracer;
            try
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        _document = XDocument.Load(_filePath);
                        return;
                    }
                    catch (Exception ex)
                    {
                        _tracer.TraceError(ex.ToString());
                    }
                }

                _document = new XDocument(new XElement(_projectName, new XElement(_propertyGroupName)));
            }
            finally
            {
                if (_document.Root == null)
                    _document.Add(new XElement(_projectName));

                Contract.Assume(_document.Root != null);

                _propertyGroup = _document.Root.Descendants(_propertyGroupName).FirstOrDefault();

                if (_propertyGroup == null)
                {
                    _propertyGroup = new XElement(_propertyGroupName);
                    _document.Root.Add(_propertyGroup);
                }
            }
        }

        [NotNull]
        public IReadOnlyDictionary<string, string> Properties
        {
            get
            {
                return _propertyGroup.Descendants()
                    .Where(item => item.Parent == _propertyGroup)
                    .ToDictionary(item => item.Name.LocalName, item => item.Value);
            }
            set
            {
                try
                {
                    var itemsToRemove = _propertyGroup.Descendants()
                        .Where(item => item.Parent == _propertyGroup)
                        .Where(item => !value.ContainsKey(item.Name.LocalName))
                        .ToArray();

                    itemsToRemove.ForEach(item => item.Remove());

                    var existingItems = Properties;

                    var itemsToAdd = value.Where(item => !existingItems.ContainsKey(item.Key))
                        .ToArray();

                    itemsToAdd.ForEach(item => _propertyGroup.Add(new XElement(_xmlns.GetName(item.Key), new XText(item.Value))));

                    _document.Save(_filePath);
                }
                catch (Exception ex)
                {
                    _tracer.TraceError(ex.ToString());
                }
            }
        }
    }
}
