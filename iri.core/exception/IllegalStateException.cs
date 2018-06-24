using System;

namespace iri.core.exception
{
    public class IllegalStateException: Exception
    {
        public IllegalStateException(string error) : base(error)
        {
        }
    }
}
