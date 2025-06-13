namespace LearnQuestV1.Api.DTOs.Common
{
    public class SecureAuthResponse<T>
    {
        public bool Success { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static SecureAuthResponse<T> SuccessResponse(T data, string code, string message)
        {
            return new SecureAuthResponse<T>
            {
                Success = true,
                Code = code,
                Message = message,
                Data = data
            };
        }

        public static SecureAuthResponse<T> ErrorResponse(string code, string message)
        {
            return new SecureAuthResponse<T>
            {
                Success = false,
                Code = code,
                Message = message,
                Data = default
            };
        }
    }

    public static class SecureAuthResponse
    {
        public static SecureAuthResponse<object> Success(string code, string message)
        {
            return new SecureAuthResponse<object>
            {
                Success = true,
                Code = code,
                Message = message,
                Data = null
            };
        }

        public static SecureAuthResponse<object> Error(string code, string message)
        {
            return new SecureAuthResponse<object>
            {
                Success = false,
                Code = code,
                Message = message,
                Data = null
            };
        }
    }
}
