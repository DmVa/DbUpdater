using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using DBUpdater.Configuration;

namespace DBUpdater
{
    public class UpdateFile : IFileProvider
    {
        private ScriptsFileConfigurationElement _fileConfig;

        private List<UpdateScript> _scripts = new List<UpdateScript>();
        private List<UpdateScript> _initVerifingTable = new List<UpdateScript>();


        public UpdateFile(ScriptsFileConfigurationElement config)
        {
            _fileConfig = config;
        }

        public string FullSourceName
        {
            get
            {
                var info = new FileInfo(_fileConfig.FileName);
                return info.FullName;
            }
        }

        /// <summary>
        /// Scripts that should be executed for correctly setup Verifing table.
        /// </summary>
        public List<UpdateScript> InitVerifingTable
        {
            get { return _initVerifingTable; }
        }

        /// <summary>
        /// Scripts that should be executed and registered in Verifing table.
        /// </summary>
        public List<UpdateScript> Scripts
        {
            get { return _scripts; }
        }

        /// <summary>
        /// Start read source file. Raises exceptions in case of not correct situations.
        /// </summary>
        public void Read()
        {
            var folderInfo = new DirectoryInfo(_fileConfig.ScriptsRootFolder);
            if (!(folderInfo.Exists))
                throw new DirectoryNotFoundException(string.Format("script folder {0} not found", folderInfo.FullName));

            var info = new FileInfo(_fileConfig.FileName);
            if (!(info.Exists))
                throw new FileNotFoundException("script file is not found", info.FullName);

            var schemaInfo = new FileInfo(_fileConfig.SchemaFile);
            if (!(schemaInfo.Exists))
                throw new FileNotFoundException("XML schema file is not found", schemaInfo.FullName);

            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("urn:dbupdate-schema", schemaInfo.FullName);

            XmlDocument doc = new XmlDocument();
            doc.Schemas = sc;
            doc.Load(_fileConfig.FileName);
            XmlNode root = doc.DocumentElement;
            if (root == null)
            {
                throw new ApplicationException(string.Format("script file {0} is not valid", info.FullName));
            }

            bool isValid = true;
            string message = "";
            doc.Validate(
                (s, e) =>
                    {
                        isValid = false;
                        message = e.Message;
                    }

                );

    
            if (!isValid)
                throw new ApplicationException(string.Format("script file {0} does not fit to schema {1}.Error:{2} ", info.FullName, schemaInfo.FullName, message));


            foreach (XmlNode node  in root.ChildNodes)
            {
                if (node.Name == "initializationVerificationTable")
                {
                    foreach (XmlNode scriptNode in node.ChildNodes)
                    {
                        if (scriptNode.Name == "sqlscript")
                            AddScriptNode(_initVerifingTable, scriptNode);
                    }
                }

                if (node.Name == "sqlscript")
                {
                    AddScriptNode(_scripts, node);
                }
            }
        }

        private void AddScriptNode(List<UpdateScript> source, XmlNode node)
        {
            if (node.Attributes == null)
                throw new ApplicationException("Wrong XML file. Script node does not have attributes");

            XmlAttributeCollection attributes = node.Attributes;

            XmlAttribute fileNameAttribute = attributes["file"];
            if (fileNameAttribute == null)
                throw new ApplicationException("Wrong XML file. attribute file is missed");

            string fullPath = Path.Combine(_fileConfig.ScriptsRootFolder, fileNameAttribute.Value);
            UpdateScript scriptFile = new UpdateScript();
            FileInfo info = new FileInfo(fullPath);
            FileHelper.FillUpdateScriptFromFileInto(scriptFile, info);

         
            scriptFile.Author = ReadAttributeFromNode("author", attributes);
            scriptFile.Description = ReadAttributeFromNode("description", attributes);
            scriptFile.DoNotRegisterInDatabase = ReadBool(ReadAttributeFromNode("donotregisterscriptindatabase", attributes));
            scriptFile.DoNotStoreFileExtensionInDatabase = ReadBool(ReadAttributeFromNode("donotstorefileextensionindatabase", attributes));

            scriptFile.File = fileNameAttribute.Value; 
            source.Add(scriptFile);
        }

        private bool  ReadBool(string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;
            return Boolean.Parse(value);
        }

        private string ReadAttributeFromNode(string fileField, XmlAttributeCollection attributes)
        {
            if (!string.IsNullOrEmpty(fileField))
            {
                var attribute = attributes[fileField];
                if (attribute != null)
                    return attribute.Value;
            }
            return null;
        }
    }
}
