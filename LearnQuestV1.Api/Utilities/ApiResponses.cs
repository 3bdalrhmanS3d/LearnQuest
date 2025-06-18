using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LearnQuestV1.Api.Utilities
{
    public static class ApiResponses
    {
        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        public static ApiResponse<T> Success<T>(T data, string message = "Request completed successfully")
            => ApiResponseHelper.SuccessWithData(data, message);

        /// <summary>
        /// Creates a successful response without data
        /// </summary>
        public static ApiResponse Success(string message = "Request completed successfully")
            => ApiResponseHelper.Success(message);

        /// <summary>
        /// Creates an error response with message
        /// </summary>
        public static ApiResponse ErrorMessage(
            string message,
            int statusCode = 400,
            object? errors = null)
            => ApiResponseHelper.ErrorMessage(message, statusCode, errors);

        /// <summary>
        /// Creates an error response with ModelState
        /// </summary>
        public static ApiResponse ValidationError(
            string message,
            ModelStateDictionary modelState,
            int statusCode = 400)
            => ApiResponseHelper.ValidationError(message, modelState, statusCode);

        /// <summary>
        /// Creates an error response with typed data
        /// </summary>
        public static ApiResponse<T> ErrorWithData<T>(
            string message,
            T data,
            int statusCode = 400,
            object? errors = null)
            => ApiResponseHelper.ErrorWithData(message, data, statusCode, errors);

        /// <summary>
        /// Creates a not found response
        /// </summary>
        public static ApiResponse NotFound(string resource = "Resource", object? resourceId = null)
            => ApiResponseHelper.NotFound(resource, resourceId);

        /// <summary>
        /// Creates an unauthorized response
        /// </summary>
        public static ApiResponse Unauthorized(string message = "You are not authorized to access this resource")
            => ApiResponseHelper.Unauthorized(message);

        /// <summary>
        /// Creates a forbidden response
        /// </summary>
        public static ApiResponse Forbidden(string message = "You don't have permission to access this resource")
            => ApiResponseHelper.Forbidden(message);

        /// <summary>
        /// Creates a conflict response
        /// </summary>
        public static ApiResponse Conflict(
            string message,
            object? errors = null)
            => ApiResponseHelper.Conflict(message, errors);

        /// <summary>
        /// Creates an internal server error response
        /// </summary>
        public static ApiResponse InternalServerError(
            string message = "An internal server error occurred",
            object? errors = null)
            => ApiResponseHelper.InternalServerError(message, errors);

        /// <summary>
        /// Creates a paged response
        /// </summary>
        public static ApiResponse<T> Paged<T>(T data, int totalCount, int pageNumber, int pageSize, string message = "Request completed successfully")
            => ApiResponseHelper.Paged(data, totalCount, pageNumber, pageSize, message);
    }
}
