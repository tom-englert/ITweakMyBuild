namespace ITweakMyBuild
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
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
        private static readonly string _filePath = Path.Combine(VSPackage.ConfigurationFolder, @"ITweakMyBuild.targets");
        [NotNull]
        private static readonly XNamespace _xmlns = @"http://schemas.microsoft.com/developer/msbuild/2003";
        [NotNull]
        // ReSharper disable once AssignNullToNotNullAttribute
        private static readonly XName _projectName = _xmlns + @"Project";
        [NotNull]
        // ReSharper disable once AssignNullToNotNullAttribute
        private static readonly XName _propertyGroupName = _xmlns + @"PropertyGroup";
        [NotNull]
        // ReSharper disable once AssignNullToNotNullAttribute
        private static readonly XName _itemGroupName = _xmlns + @"ItemGroup";
        [NotNull]
        // ReSharper disable once AssignNullToNotNullAttribute
        private static readonly XName _analyzerName = _xmlns + @"Analyzer";

        [NotNull]
        private readonly Tracer _tracer;
        [NotNull]
        private readonly XDocument _document;
        [NotNull]
        private readonly XElement _propertyGroup;
        [NotNull]
        private readonly XElement _itemGroup;

        private DateTime _fileTime;

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
                        _fileTime = File.GetLastWriteTime(_filePath);
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
                var documentRoot = _document.Root;

                if (documentRoot == null)
                {
                    documentRoot = new XElement(_projectName);
                    _document.Add(documentRoot);
                }

                _propertyGroup = ForceElement(documentRoot, _propertyGroupName);

                _itemGroup = ForceElement(documentRoot, _itemGroupName);
            }
        }

        [NotNull]
        private static XElement ForceElement([NotNull] XContainer documentRoot, [NotNull] XName groupName)
        {
            var itemGroup = documentRoot.Descendants(groupName).FirstOrDefault();

            if (itemGroup != null)
                return itemGroup;

            itemGroup = new XElement(groupName);
            documentRoot.Add(itemGroup);

            return itemGroup;
        }

        public bool HasExternalChanges => !File.Exists(_filePath) || _fileTime != File.GetLastWriteTime(_filePath);

        [NotNull]
        public IReadOnlyDictionary<string, string> Properties
        {
            get
            {
                return _propertyGroup.Descendants()
                    // ReSharper disable once PossibleNullReferenceException
                    .Where(item => item.Parent == _propertyGroup)
                    .ToDictionary(item => item.Name.LocalName, item => item.Value);
            }
            set
            {
                try
                {
                    var itemsToRemove = _propertyGroup.Descendants()
                        // ReSharper disable once PossibleNullReferenceException
                        .Where(item => item.Parent == _propertyGroup)
                        .Where(item => !value.ContainsKey(item.Name.LocalName))
                        .ToArray();

                    // ReSharper disable once PossibleNullReferenceException
                    itemsToRemove.ForEach(item => item.Remove());

                    var existingItems = Properties;

                    var itemsToAdd = value.Where(item => !existingItems.ContainsKey(item.Key))
                        .ToArray();

                    // ReSharper disable AssignNullToNotNullAttribute
                    itemsToAdd.ForEach(item => _propertyGroup.Add(new XElement(_xmlns.GetName(item.Key), new XText(item.Value))));
                    // ReSharper enable AssignNullToNotNullAttribute

                    _document.Save(_filePath);
                    _fileTime = File.GetLastWriteTime(_filePath);
                }
                catch (Exception ex)
                {
                    _tracer.TraceError(ex.ToString());
                }
            }
        }

        [NotNull]
        public IReadOnlyCollection<string> Analyzers
        {
            get
            {
                return _itemGroup.Descendants(_analyzerName)
                    // ReSharper disable once PossibleNullReferenceException
                    .Where(item => item.Parent == _itemGroup)
                    .Select(item => item.Attribute("Include")?.Value)
                    .Where(item => item != null)
                    .ToArray();
            }
            set
            {
                try
                {
                    var itemsToRemove = _itemGroup.Descendants(_analyzerName)
                        // ReSharper disable once PossibleNullReferenceException
                        .Where(item => item.Parent == _itemGroup)
                        .Where(item => !value.Contains(item.Attribute("Include")?.Value, StringComparer.OrdinalIgnoreCase))
                        .ToArray();

                    // ReSharper disable once PossibleNullReferenceException
                    itemsToRemove.ForEach(item => item.Remove());

                    var existingItems = Analyzers;

                    var itemsToAdd = value.Where(item => !existingItems.Contains(item, StringComparer.OrdinalIgnoreCase))
                        .ToArray();

                    // ReSharper disable AssignNullToNotNullAttribute
                    itemsToAdd.ForEach(item => _itemGroup.Add(new XElement(_analyzerName, new XAttribute("Include", item))));
                    // ReSharper enable AssignNullToNotNullAttribute

                    _document.Save(_filePath);
                    _fileTime = File.GetLastWriteTime(_filePath);
                }
                catch (Exception ex)
                {
                    _tracer.TraceError(ex.ToString());
                }
            }
        }
    }
}
