using System;

namespace DBUpdater
{
    public class UpdateScript
    {
        public string File { get; set; }
        public string FullFileName { get; set; }
        public String Description { get; set; }
        public String Author { get; set; }
        public DateTime FileModifiedDate { get; set; }
        public UpdateScriptStatus? Status { get; set; }
        public DateTime Executed { get; set; }

        public bool DoNotRegisterInDatabase { get; set; }

        public bool DoNotStoreFileExtensionInDatabase { get; set; }
    }

    public class UpdateScriptRegisteredData
    {
        public string File { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public UpdateScriptStatus? Status { get; set; }
    }

    public enum UpdateScriptStatus
    {
        Ok,
        Error,
        InProgress
    }
}
