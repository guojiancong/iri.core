using System;

namespace iri.core.exception
{
    public class StaleTimestampException : Exception
    {
        public StaleTimestampException(string message) : base(message)
        {
        }
    }
}
