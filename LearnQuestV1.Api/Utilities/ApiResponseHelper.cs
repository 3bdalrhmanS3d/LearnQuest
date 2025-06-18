using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LearnQuestV1.Api.Utilities
{
    /// <summary>
    /// Static helper class for creating standardized API responses
    /// </summary>
    public static class ApiResponseHelper
    {
        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="data">Response data</param>
        /// <param name="message">Success message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Successful API response</returns>
        public static ApiResponse<T> SuccessWithData<T>(T data, string message = "Request completed successfully", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow,
                CorrelationId = GetCorrelationId()
            };
        }

        /// <summary>
        /// Creates a successful response without data
        /// </summary>
        /// <param name="message">Success message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Successful API response</returns>
        public static ApiResponse Success(string message = "Request completed successfully", int statusCode = 200)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow,
                CorrelationId = GetCorrelationId()
            };
        }

        /// <summary>
        /// Creates an error response with message
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="errors">Additional error details</param>
        /// <returns>Error API response</returns>
        public static ApiResponse ErrorMessage(string message, int statusCode = 400, object? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow,
                CorrelationId = GetCorrelationId()
            };
        }

        /// <summary>
        /// Creates an error response with ModelState validation errors
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="modelState">ModelState containing validation errors</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Error API response with validation details</returns>
        public static ApiResponse ValidationError(string message, ModelStateDictionary modelState, int statusCode = 400)
        {
            var errors = modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow,
                CorrelationId = GetCorrelationId()
            };
        }

        /// <summary>
        /// Creates an error response with typed data
        /// </summary>
        /// <typeparam name="T">Type of the error data</typeparam>
        /// <param name="message">Error message</param>
        /// <param name="data">Error data</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="errors">Additional error details</param>
        /// <returns>Error API response with typed data</returns>
        public static ApiResponse<T> ErrorWithData<T>(string message, T data, int statusCode = 400, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = data,
                Errors = errors,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow,
                CorrelationId = GetCorrelationId()
            };
        }

        /// <summary>
        /// Creates a not found response
        /// </summary>
        /// <param name="resource">Resource that was not found</param>
        /// <param name="resourceId">ID of the resource that was not found</param>
        /// <returns>Not found API response</returns>
        public static ApiResponse NotFound(string resource = "Resource", object? resourceId = null)
        {
            var message = resourceId != null
                ? $"{resource} with ID '{resourceId}' was not found"
                : $"{resource} was not found";

            return ErrorMessage(message, 404);
        }

        /// <summary>
        /// Creates an unauthorized response
        /// </summary>
        /// <param name="message">Unauthorized message</param>
        /// <returns>Unauthorized API response</returns>
        public static ApiResponse Unauthorized(string message = "You are not authorized to access this resource")
        {
            return ErrorMessage(message, 401);
        }

        /// <summary>
        /// Creates a forbidden response
        /// </summary>
        /// <param name="message">Forbidden message</param>
        /// <returns>Forbidden API response</returns>
        public static ApiResponse Forbidden(string message = "You don't have permission to access this resource")
        {
            return ErrorMessage(message, 403);
        }

        /// <summary>
        /// Creates a conflict response
        /// </summary>
        /// <param name="message">Conflict message</param>
        /// <param name="errors">Additional error details</param>
        /// <returns>Conflict API response</returns>
        public static ApiResponse Conflict(string message, object? errors = null)
        {
            return ErrorMessage(message, 409, errors);
        }

        /// <summary>
        /// Creates an internal server error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errors">Additional error details</param>
        /// <returns>Internal server error API response</returns>
        public static ApiResponse InternalServerError(string message = "An internal server error occurred", object? errors = null)
        {
            return ErrorMessage(message, 500, errors);
        }

        /// <summary>
        /// Creates a response with custom status code
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="success">Success flag</param>
        /// <param name="message">Response message</param>
        /// <param name="data">Response data</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="errors">Error details</param>
        /// <returns>Custom API response</returns>
        public static ApiResponse<T> Custom<T>(bool success, string message, T data, int statusCode, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = success,
                Message = message,
                Data = data,
                Errors = errors,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow,
                CorrelationId = GetCorrelationId()
            };
        }

        /// <summary>
        /// Creates a paged response with pagination metadata
        /// </summary>
        /// <typeparam name="T">Type of the data items</typeparam>
        /// <param name="data">Paged data</param>
        /// <param name="totalCount">Total number of items</param>
        /// <param name="pageNumber">Current page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="message">Response message</param>
        /// <returns>Paged API response</returns>
        public static ApiResponse<T> Paged<T>(
            T data,
            int totalCount,
            int pageNumber,
            int pageSize,
            string message = "Request completed successfully")
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var hasNextPage = pageNumber < totalPages;
            var hasPreviousPage = pageNumber > 1;

            var metadata = new Dictionary<string, object>
            {
                ["pagination"] = new
                {
                    totalCount,
                    pageNumber,
                    pageSize,
                    totalPages,
                    hasNextPage,
                    hasPreviousPage
                }
            };

            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow,
                CorrelationId = GetCorrelationId(),
                Metadata = metadata
            };
        }

        /// <summary>
        /// Gets or generates a correlation ID for request tracking
        /// </summary>
        /// <returns>Correlation ID</returns>
        private static string GetCorrelationId()
        {
            // In a real application, this would typically come from the HTTP context
            // or be generated at the start of the request pipeline
            return Guid.NewGuid().ToString("N")[..8];
        }
    }



}
