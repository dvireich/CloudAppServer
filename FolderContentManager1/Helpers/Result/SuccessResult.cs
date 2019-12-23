namespace ContentManager.Helpers.Result
{
    class SuccessResult<T> : Result<T>
    {
        public SuccessResult(T data) : base(data, true, null)
        {
        }
    }

    class SuccessResult : Result
    {
        public SuccessResult() : base(true, null)
        {
        }
    }
}
