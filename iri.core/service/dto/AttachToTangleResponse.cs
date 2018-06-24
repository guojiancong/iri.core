using System.Collections.Generic;

namespace iri.core.service.dto
{

    public class AttachToTangleResponse : AbstractResponse
    {
        public List<string> Trytes { get; private set; }

        public static AbstractResponse Create(List<string> elements)
        {
            var res = new AttachToTangleResponse {Trytes = elements};
            return res;
        }
    }
}
