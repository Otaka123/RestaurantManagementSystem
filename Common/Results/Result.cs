namespace UsersApp.Common.Results
{
    public class Result<T>
    {
        public bool Success { get; private set; }
        public T Data { get; private set; }
        public string Message { get; private set; }

        public static Result<T> CreateSuccess(T data, string message = "Operation succeeded.") => new Result<T> { Success = true, Data = data, Message = message };
        public static Result<T> Failure(string message) => new Result<T> { Success = false, Message = message };
    }

}
