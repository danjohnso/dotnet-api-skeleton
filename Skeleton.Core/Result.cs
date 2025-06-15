using Skeleton.Core.Enums;

namespace Skeleton.Core
{
    public class Result<T>
    {
        protected Result() { }

        public Result(T value)
        {
            Value = value;
        }

        protected Result(ResultStatusType statusType)
        {
            StatusType = statusType;
        }

        public T? Value { get; }
        public bool IsSuccess => StatusType == ResultStatusType.Ok;
        public ResultStatusType StatusType { get; protected set; } = ResultStatusType.Ok;
        public Dictionary<string, List<Problem>> Problems { get; protected set; } = [];

        public static Result<T> Success(T value)
        {
            return new Result<T>(value);
        }

        public static Result<T> Error(Problem problem, string field = "")
        {
            return new Result<T>(ResultStatusType.Error) {
                Problems = new()
                {
                    { field, [ problem ] }
                }
            };
        }

        public static Result<T> Error(List<Problem> problems, string field = "")
        {
            return new Result<T>(ResultStatusType.Error) { 
                Problems = new()
                {
                    { field, problems }
                }
            };
        }

        public static Result<T> Error(IDictionary<string, List<Problem>> errors)
        {
            return new Result<T>(ResultStatusType.Error)
            {
                Problems = new(errors)
            };
        }

        public static Result<T> Invalid(Problem problem, string field = "")
        {
            return new Result<T>(ResultStatusType.Invalid) {
                Problems = new()
                {
                    { field, [ problem ] }
                }
            };
        }

        public static Result<T> Invalid(List<Problem> problems, string field = "")
        {
            return new Result<T>(ResultStatusType.Invalid)
            {
                Problems = new()
                {
                    { field, problems }
                }
            };
        }

        public static Result<T> Invalid(IDictionary<string, List<Problem>> errors)
        {
            return new Result<T>(ResultStatusType.Invalid)
            {
                Problems = new(errors)
            };
        }

        public static Result<T> NotFound(Problem problem, string field = "")
        {
            return new Result<T>(ResultStatusType.NotFound) {
                Problems = new()
                {
                    { field, [ problem ] }
                }
            };
        }

        public static Result<T> Conflict(Problem problem, string field = "")
        {
            return new Result<T>(ResultStatusType.Conflict) {
                Problems = new()
                {
                    { field, [ problem ] }
                }
            };
        }
    }
}
