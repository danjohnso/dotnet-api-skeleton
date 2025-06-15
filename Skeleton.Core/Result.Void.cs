using Skeleton.Core.Enums;

namespace Skeleton.Core
{
    public class Result : Result<Result>
    {
        public Result() : base() { }

        protected internal Result(ResultStatusType status) : base(status) { }

        public static Result Success()
        {
            return new Result();
        }

        public static Result<T> Success<T>(T value)
        {
            return new Result<T>(value);
        }

        public new static Result Error(Problem problem, string field = "")
        {
            return new Result(ResultStatusType.Error) {
                Problems = new()
                {
                    { field, [ problem ] }
                }
            };
        }

        public new static Result Error(List<Problem> problems, string field = "")
        {
            return new Result(ResultStatusType.Error)
            {
                Problems = new()
                {
                    { field, problems }
                }
            };
        }

        public new static Result Error(IDictionary<string, List<Problem>> errors)
        {
            return new Result(ResultStatusType.Error)
            {
                Problems = new(errors)
            };
        }

        public new static Result Invalid(Problem problem, string field = "")
        {
            return new Result(ResultStatusType.Invalid) {
                Problems = new()
                {
                    { field, [ problem ] }
                }
            };
        }

        public new static Result Invalid(List<Problem> problems, string field = "")
        {
            return new Result(ResultStatusType.Invalid)
            {
                Problems = new()
                {
                    { field, problems }
                }
            };
        }

        public new static Result Invalid(IDictionary<string, List<Problem>> errors)
        {
            return new Result(ResultStatusType.Invalid)
            {
                Problems = new(errors)
            };
        }

        public new static Result NotFound(Problem problem, string field = "")
        {
            return new Result(ResultStatusType.NotFound) {
                Problems = new()
                {
                    { field, [ problem ] }
                }
            };
        }

        public new static Result Conflict(Problem problem, string field = "")
        {
            return new Result(ResultStatusType.Conflict) {
                Problems = new()
                {
                    { field, [ problem ] }
                }
            };
        }
    }
}
