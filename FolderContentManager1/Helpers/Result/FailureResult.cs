using System;

namespace ContentManager.Helpers.Result
{
    class FailureResult<T> : Result<T>
    {
        public FailureResult(Exception exception) : base(default(T), false, exception)
        {
        }
    }

    class FailureResult : Result
    {
        public FailureResult(Exception exception) : base(false, exception)
        {
        }
    }
}
