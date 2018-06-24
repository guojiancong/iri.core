namespace iri.core.service.dto
{
    public class AccessLimitedResponse : AbstractResponse
    {
        public string Error { get; private set; }

        public static AbstractResponse Create(string error)
        {
            var res = new AccessLimitedResponse {Error = error};
            return res;
        }
    }
}