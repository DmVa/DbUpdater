using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DBUpdater.Configuration;
using LogWrapper;

namespace DBUpdater
{
    public class UpdateManager
    {
        private DbUpdaterConfigurationSection _settings;
        private IFileProvider _file;
        private int _executedCount;
        public ILogger _log;
        public UpdateManager(DbUpdaterConfigurationSection settings, ILogger log)
        {
            _log = log;
            if (_log == null)
                throw new ArgumentNullException("log");
            _settings = settings;
            if (settings == null)
                throw new ArgumentNullException("settings");
        }

        public event EventHandler<UpdateProgressEventArgs> UpdateProgress;

        public void Update()
        {
            _log.Log(LogLevel.Info, String.Format("Start update at {0}", DateTime.Now));
            string resultMessage;
            try
            {
                _executedCount = 0;
                ReadFile();

                bool isExistsVerifingTable = CheckIfExistsVerificationTable();
                if (isExistsVerifingTable)
                {
                    RunScripts(_file.InitVerifingTable);
                }
                else
                {
                    RunScriptsWithoutVerification(_file.InitVerifingTable);
                }

                RunScripts(_file.Scripts);


                resultMessage = "Ok. " + string.Format(" Executed {0} scripts.", _executedCount);
                _log.Log(LogLevel.Info, resultMessage + string.Format("Total {0} scripts", _file.InitVerifingTable.Count + _file.Scripts.Count));
            }
            catch (Exception ex)
            {
                _log.LogError("Error:", ex);
                resultMessage = "Error: " + ex.Message;
            }

            RaiseUpdateProgerss(UpdateProgressStatus.Completed, resultMessage);
        }

        private void ReadFile()
        {
            if (_settings.ScriptSource == ScriptSource.XmlFile)
                _file = new UpdateFile(_settings.ScriptsFiles);
            if (_settings.ScriptSource == ScriptSource.Directory)
                _file = new UpdateDirectory(_settings.ScriptDirectory);

            if (_file == null)
                throw new ApplicationException("Scripts source not defined");

            _log.Log(LogLevel.Info, String.Format("Read source {0} FullName {1}", _settings.ScriptSource, _file.FullSourceName));
            _file.Read();
            var scriptsCount = _file.Scripts.Count + _file.InitVerifingTable.Count;
            _log.Log(LogLevel.Info, String.Format("Got {0} scripts", scriptsCount ));
            RaiseUpdateProgerss(String.Format("Source {0} read. Registered {1} scripts", _file.FullSourceName, scriptsCount));
        }

        private void RaiseUpdateProgerss(string template, object value)
        {
            RaiseUpdateProgerss(string.Format(template, value));
        }

        private void RaiseUpdateProgerss(string message)
        {
            RaiseUpdateProgerss(UpdateProgressStatus.None, message);
        }

        private void RaiseUpdateProgerss(UpdateProgressStatus status, string message)
        {
            if (UpdateProgress != null)
            {
                UpdateProgress(this, new UpdateProgressEventArgs(status, message));
            }
        }

        private void RunScriptsWithoutVerification(List<UpdateScript> list)
        {
            UpdateScript currentScript = null;
            try
            {
                foreach (var initScript in list)
                {
                    currentScript = initScript;
                    ExecuteScript(currentScript);
                }

                foreach (var initScript in list)
                {
                    currentScript = initScript;
                    currentScript.Status = UpdateScriptStatus.Ok;
                    AddNewScript(initScript);
                }
            }
            catch (Exception ex)
            {
                var error = new ApplicationException(
                        string.Format("Initial script execution failed file:{0}, {1}.", currentScript.FullFileName, ex.Message), ex);
                throw error;
            }
        }

        private void RunScripts(List<UpdateScript> scripts)
        {
            foreach (var script in scripts)
            {
                RunScript(script);
            }
        }

        private void RunScript(UpdateScript script)
        {
            UpdateScriptRegisteredData inDb = null;
            bool registerBeforeRun = false;
            try
            {
                inDb = GetRegistedScriptData(script.DoNotStoreFileExtensionInDatabase, script.File);
                if (!IsNeedToRun(inDb))
                    return;

                script.Status = UpdateScriptStatus.InProgress;
                registerBeforeRun = ShouldRegisterBeforeExecution();
                if (registerBeforeRun)
                {
                    if (inDb == null)
                    {
                        AddNewScript(script);
                    }
                    else
                    {
                        inDb.Status = UpdateScriptStatus.InProgress;
                        UpdateStatus(inDb);
                    }
                }


                ExecuteScript(script);
                inDb = GetRegistedScriptData(script.DoNotStoreFileExtensionInDatabase, script.File);

                script.Status = UpdateScriptStatus.Ok;

                if (inDb == null)
                {
                    AddNewScript(script);
                }
                else
                {
                    UpdateScriptInfo(script);
                }

            }
            catch (Exception ex)
            {
                var message = String.Format("Script execution file: {0} failed. {1}", script.FullFileName, ex.Message);
                var error = new ApplicationException(message, ex);

                script.Status = UpdateScriptStatus.Error;
                if (registerBeforeRun && (inDb != null))
                {
                    UpdateScriptInfo(script);
                }

                throw error;
            }
        }

        private bool ShouldRegisterBeforeExecution()
        {
            return !(string.IsNullOrEmpty(_settings.ScriptsDbTable.UpdateStatusField));
        }

        private bool IsNeedToRun(UpdateScriptRegisteredData inDb)
        {
           if (inDb == null)
               return true;

            if (inDb.Status.HasValue && inDb.Status != UpdateScriptStatus.Ok)
                return true;
           
            return false;
        }

        private void AddNewScript(UpdateScript script)
        {
            if (script.DoNotRegisterInDatabase)
                return;

            StringBuilder fieldNames = new StringBuilder();
            AddTextParams(fieldNames, "{0}");

            StringBuilder fieldValues = new StringBuilder();
            AddTextParams(fieldValues, "@{0}");

            string template = " INSERT INTO {0} ({1}) VALUES({2})";
            string commandText = string.Format(template, _settings.ScriptsDbTable.TableName, fieldNames, fieldValues);

            using (SqlConnection conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(commandText, conn);
                script.Executed = DateTime.Now;
                AddSqlParams(command, script);
                command.ExecuteNonQuery();
                command.Dispose();
            }

            if (script.Status.HasValue && !string.IsNullOrEmpty(_settings.ScriptsDbTable.UpdateStatusField))
                RaiseUpdateProgerss(String.Format("{0} registered, status: {1}", script.File, script.Status));
            else
                RaiseUpdateProgerss(String.Format("{0} registered", script.File));

        }

        private void UpdateScriptInfo(UpdateScript script)
        {
            if (script.DoNotRegisterInDatabase)
                return;

            StringBuilder fieldValues = new StringBuilder();
            string updateTemplateLine;

            if (_settings.UpdateFromXMLEvenIfXMLEmpty)
            {
                updateTemplateLine = "{0} = @{0}";
            }
            else
            {
                updateTemplateLine = "{0} = ISNULL(@{0}, {0})";
            }

            AddTextParams(fieldValues, updateTemplateLine);
            string registeredScriptName = GetRegisteredNameByScriptName(script.DoNotStoreFileExtensionInDatabase, script.File);
            string updateTemplate = " Update {0} SET  {1} where {2} = '{3}'";
            string commandText = string.Format(updateTemplate, _settings.ScriptsDbTable.TableName,
                                               fieldValues, _settings.ScriptsDbTable.XmlMappedData.FileField, registeredScriptName);


            using (SqlConnection conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(commandText, conn);
                AddSqlParams(command, script);
                command.ExecuteNonQuery();
                command.Dispose();
            }

            if (script.Status.HasValue && !string.IsNullOrEmpty(_settings.ScriptsDbTable.UpdateStatusField))
                RaiseUpdateProgerss(String.Format("{0} registration updated, new Status: {1}", script.File, script.Status));
            else
                RaiseUpdateProgerss(String.Format("{0} registration updated", script.File));
        }

        private void UpdateStatus(UpdateScriptRegisteredData script)
        {
            if (!script.Status.HasValue || string.IsNullOrEmpty(_settings.ScriptsDbTable.UpdateStatusField))
            {
                return;
            }


            string statusText = string.Format("{0} = @{0}", _settings.ScriptsDbTable.UpdateStatusField);
            string updateTemplate = " Update {0} SET  {1} where {2} = '{3}'";
            string commandText = string.Format(updateTemplate, _settings.ScriptsDbTable.TableName,
                                               statusText, _settings.ScriptsDbTable.XmlMappedData.FileField,
                                               script.File);


            using (SqlConnection conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(commandText, conn);
                command.Parameters.AddWithValue("@" + _settings.ScriptsDbTable.UpdateStatusField, GetDbStatusValueByEnum(script.Status.Value));
                command.ExecuteNonQuery();
                command.Dispose();
            }


            RaiseUpdateProgerss(String.Format("{0}, status updated to :{1}", script.File, script.Status));

        }

        private UpdateScriptRegisteredData GetRegistedScriptData(bool donotStoreFileExtension, string name)
        {
            UpdateScriptRegisteredData registeredData = null;
            string registeredName = GetRegisteredNameByScriptName(donotStoreFileExtension, name);
            string selectTemplate = "{0} As FileName, {1} as ModifiedDate, {2} as Status";
            string dateTemplate = "NULL";
            string statusTemplate = "NULL";
            if (!(string.IsNullOrEmpty(_settings.ScriptsDbTable.UpdateStatusField)))
                statusTemplate = _settings.ScriptsDbTable.UpdateStatusField;

            selectTemplate = string.Format(selectTemplate, _settings.ScriptsDbTable.XmlMappedData.FileField, dateTemplate, statusTemplate);
            
            string commandText = string.Format(" SELECT {0} FROM {1} where {2} = '{3}' ",
                selectTemplate, _settings.ScriptsDbTable.TableName, _settings.ScriptsDbTable.XmlMappedData.FileField, registeredName);

            using (SqlConnection conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(commandText, conn);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    registeredData = new UpdateScriptRegisteredData();
                    registeredData.File = reader.GetString(0);
                    object dataObj = reader.GetValue(1);
                    if (dataObj != null && dataObj != DBNull.Value)
                        registeredData.ModifiedDate = (DateTime)dataObj;

                    object statusObj = reader.GetValue(2);
                    if (statusObj != null && statusObj != DBNull.Value)
                    {
                        registeredData.Status = GetStatusEnum(statusObj);
                    }
                }
                reader.Close();
            }

            return registeredData;
        }

        private string GetRegisteredNameByScriptName(bool removeFileExtension, string fileName)
        {
            if (!removeFileExtension)
                return fileName;

            int lastIndexOfDot = fileName.LastIndexOf('.');
            if (lastIndexOfDot >0)
            {
                return fileName.Substring(0, lastIndexOfDot);
            }
            return fileName;

        }

        private UpdateScriptStatus? GetStatusEnum(object statusObj)
        {
            if (statusObj == null || string.IsNullOrEmpty(_settings.ScriptsDbTable.UpdateStatusField) ||
                _settings.ScriptsDbTable.ScriptFileStatus == null)
            {
                return null;
            }

            string statusStr = statusObj.ToString().ToLower();
            if (statusStr == _settings.ScriptsDbTable.ScriptFileStatus.ErrorValue.ToLower())
                return UpdateScriptStatus.Error;
            if (statusStr == _settings.ScriptsDbTable.ScriptFileStatus.OkValue.ToLower())
                return UpdateScriptStatus.Ok;
            if (statusStr == _settings.ScriptsDbTable.ScriptFileStatus.InProgressValue.ToLower())
                return UpdateScriptStatus.InProgress;

            return null;
        }

        private string GetDbStatusValueByEnum(UpdateScriptStatus? statusObj)
        {
            if (statusObj == null || string.IsNullOrEmpty(_settings.ScriptsDbTable.UpdateStatusField) ||
                _settings.ScriptsDbTable.ScriptFileStatus == null)
            {
                return null;
            }

            switch (statusObj.Value)
            {
                case UpdateScriptStatus.Error:
                    return _settings.ScriptsDbTable.ScriptFileStatus.ErrorValue;
                case UpdateScriptStatus.Ok:
                    return _settings.ScriptsDbTable.ScriptFileStatus.OkValue;
                case UpdateScriptStatus.InProgress:
                    return _settings.ScriptsDbTable.ScriptFileStatus.InProgressValue;
            }

            return null;
        }

        private void AddTextParams(StringBuilder sb, string template)
        {
            AddTextParamIfRequired(sb, _settings.ScriptsDbTable.UpdateStatusField, template);
            AddTextParamIfRequired(sb, _settings.ScriptsDbTable.XmlMappedData.FileField, template);
            AddTextParamIfRequired(sb, _settings.ScriptsDbTable.XmlMappedData.AuthorField, template);
            AddTextParamIfRequired(sb, _settings.ScriptsDbTable.XmlMappedData.DescriptionField, template);
            AddTextParamIfRequired(sb, _settings.ScriptsDbTable.RunDateField, template);
        }

        private void AddSqlParams(SqlCommand command, UpdateScript script)
        {
             AddCommandParamIfRequired(command.Parameters, _settings.ScriptsDbTable.UpdateStatusField, GetDbStatusValueByEnum(script.Status));
             string registeredScriptName = GetRegisteredNameByScriptName(script.DoNotStoreFileExtensionInDatabase,script.File);
             AddCommandParamIfRequired(command.Parameters, _settings.ScriptsDbTable.XmlMappedData.FileField, registeredScriptName);
             AddCommandParamIfRequired(command.Parameters, _settings.ScriptsDbTable.XmlMappedData.AuthorField, script.Author);
             AddCommandParamIfRequired(command.Parameters, _settings.ScriptsDbTable.XmlMappedData.DescriptionField, script.Description);
             AddCommandParamIfRequired(command.Parameters, _settings.ScriptsDbTable.RunDateField, script.Executed);
        }

        private void AddCommandParamIfRequired(SqlParameterCollection sqlParameterCollection, string dbFieldName, object value)
        {
            if (!ShouldAddDbField(dbFieldName))
                return;

            sqlParameterCollection.AddWithValue("@" + dbFieldName, value ?? DBNull.Value);
        }

        private void AddTextParamIfRequired(StringBuilder lineBuilder, string dbFieldName, string pattern)
        {
            if (!ShouldAddDbField(dbFieldName)) 
                return;

            if (lineBuilder.Length > 0)
                lineBuilder.Append(",");

            lineBuilder.AppendFormat(pattern, dbFieldName);
        }

        private bool ShouldAddDbField(string dbFieldName)
        {
            return !String.IsNullOrEmpty(dbFieldName);
        }

        private void ExecuteScript(UpdateScript scriptInfo)
        {
            FileInfo file = new FileInfo(scriptInfo.FullFileName);
            StreamReader fileStream = file.OpenText();
            string scriptText = fileStream.ReadToEnd();
            fileStream.Close();

            using (SqlConnection conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();
                IEnumerable<string> statements = SplitSqlStatements(scriptText);
                foreach (string statement in statements)
                {
                    using (SqlCommand command = new SqlCommand(statement, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            scriptInfo.Executed = DateTime.Now;
            RaiseUpdateProgerss("{0} Executed", scriptInfo.File);
            _log.Log(LogLevel.Info, String.Format("{0} Executed. Full file name: {1}", scriptInfo.File, scriptInfo.FullFileName));
            _executedCount++;
        }

        private static IEnumerable<string> SplitSqlStatements(string sqlScript)
        {
            // Split by "GO" statements
            var statements = Regex.Split(
                    sqlScript,
                    @"^\s*GO\s* ($ | \-\- .*$)",
                    RegexOptions.Multiline |
                    RegexOptions.IgnorePatternWhitespace |
                    RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim(' ', '\r', '\n'));
        }
     
        private bool CheckIfExistsVerificationTable()
        {
            string commandText = string.Format(" SELECT CASE WHEN OBJECT_ID('{0}', 'U') IS NULL THEN 0 ELSE 1  END", _settings.ScriptsDbTable.TableName);
            bool isExists;
            using (SqlConnection conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(commandText, conn);
                isExists = (int)command.ExecuteScalar() > 0;
            }
            return isExists; 
        }
    }
}
