using System.Collections.Generic;

namespace OrderManagement.DTOsModels
{
    public class UploadOrderResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<UploadError> Errors { get; set; } = new();
    }

    public class UploadError
    {
        public int Line { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
