namespace ImportExportCsvAPI.Domain.Abstractions
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public List<string> Errors { get; }

        private Result(T data)
        {
            IsSuccess = true;
            Data = data;
            Errors = [];
        }

        private Result(List<string> errors)
        {
            IsSuccess = false;
            Errors = errors;
        }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(List<string> errors) => new(errors);
    }
}
