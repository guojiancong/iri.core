using System;
using System.Collections.Generic;
using System.Text;

namespace iri.core.utils
{
    public class TimeStamp
    {
        public static long Now()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }
    }
}
