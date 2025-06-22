namespace Server.Utils
{
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public static OperationResult Success(string message = "") => new() { IsSuccess = true, Message = message };
        public static OperationResult Fail(string message) => new() { IsSuccess = false, Message = message };
    }
}
