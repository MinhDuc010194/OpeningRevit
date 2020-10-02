using System;

namespace CommonUtils
{
    public class Log
    {
        public enum Status
        {
            Failure,
            Success
        }

        public Guid Id { get; set; }

        public string ProductCode { get; set; }

        public DateTime CreatedDate { get; set; }

        public Status CheckedStatus { get; set; }

        public string Remark { get; set; }
    }
}