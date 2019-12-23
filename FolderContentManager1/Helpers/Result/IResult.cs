using System;

namespace ContentManager.Helpers.Result
{
    public interface IResult<T>
    {
        bool IsSuccess { get; }
        Exception Exception { get; }
        T Data { get; }
    }
}
