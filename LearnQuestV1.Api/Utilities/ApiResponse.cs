using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LearnQuestV1.Api.Utilities
{
    /// <summary>
    /// Standardized API response wrapper for consistent response format
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Response data payload
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error details if the request failed
        /// </summary>
        public object? Errors { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request correlation ID for tracking
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Non-generic version of ApiResponse for cases where no data is returned
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
    }


}
