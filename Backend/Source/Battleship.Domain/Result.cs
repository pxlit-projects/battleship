namespace Battleship.Domain
{
    //DO NOT TOUCH THIS FILE!
    public class Result
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public string Message { get; }

        private Result(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static Result CreateSuccessResult()
        {
            return new Result(true, string.Empty);
        }

        public static Result CreateFailureResult(string message)
        {
            return new Result(false, message);
        }

    }
}