using DBUpdater.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUpdater
{
    public class UpdateDirectory : IFileProvider
    {
        private ScriptsDirectoryConfigurationElement _directoryConfig;
        private List<UpdateScript> _scripts = new List<UpdateScript>();
        private List<UpdateScript> _initVerifingTable = new List<UpdateScript>();
        public UpdateDirectory(ScriptsDirectoryConfigurationElement config)
        {
            _directoryConfig = config;
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
        public string FullSourceName
        {
            get
            {
                var info = new DirectoryInfo(_directoryConfig.DirectoryName);
                return info.FullName;
            }
        }

        /// <summary>
        /// Start read source file. Raises exceptions in case of not correct situations.
        /// </summary>
        public void Read()
        {
            _scripts.Clear();
            var info = new DirectoryInfo(_directoryConfig.DirectoryName);
            if (!info.Exists)
                throw new DirectoryNotFoundException(string.Format("Script directory {0} not found", info.FullName));
            FileInfo[] files = info.GetFiles(_directoryConfig.FileMask);
            if (files == null || files.Length == 0)
                return;
            FileInfo[] filesOrdered = files.OrderBy(x => x.Name).ToArray();
            foreach (FileInfo fileInfo in filesOrdered)
            {
                AddScriptNode(_scripts, fileInfo);
            }
        }

        private void AddScriptNode(List<UpdateScript> source, FileInfo info)
        {
            UpdateScript scriptFile = new UpdateScript();
            scriptFile.DoNotRegisterInDatabase = _directoryConfig.DoNotRegisterScriptInDatabase;
            scriptFile.DoNotStoreFileExtensionInDatabase = _directoryConfig.DoNotStoreFileExtensionInDatabase;
            FileHelper.FillUpdateScriptFromFileInto(scriptFile, info);
            
            source.Add(scriptFile);
        }
    }
}
