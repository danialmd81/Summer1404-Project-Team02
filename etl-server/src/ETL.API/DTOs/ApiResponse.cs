namespace ETL.API.DTOs;
public class ApiResponse<T>
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse(int statusCode = 200, string message = "Success", T? data = default)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }
}
