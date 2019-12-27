using System;

namespace ContentManager.Helpers.Exceptions
{
    public class CacheMissException : Exception
    {
        public CacheMissException(string message) : base(message)
        {
        }
    }
}
