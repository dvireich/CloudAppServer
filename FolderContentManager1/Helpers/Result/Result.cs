using System;
using Void = ContentManager.Helpers.Result.InternalTypes.Void;

namespace ContentManager.Helpers.Result
{
    public class Result<T> : IResult<T>
    {
        public Result(T data, bool isSuccess, Exception exception)
        {
            Data = data;
            IsSuccess = isSuccess;
            Exception = exception;
        }

        public bool IsSuccess { get; }
        public Exception Exception { get; }
        public T Data { get; }
    }

    public class Result : IResult<InternalTypes.Void>
    {
        public Result(bool isSuccess, Exception exception)
        {
            Data = null;
            IsSuccess = isSuccess;
            Exception = exception;
        }

        public bool IsSuccess { get; }
        public Exception Exception { get; }
        public InternalTypes.Void Data { get; }
    }
}
