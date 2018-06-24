namespace iri.core.service.dto
{
    public class ErrorResponse : AbstractResponse
    {
        public string Error { get; private set; }

        public static AbstractResponse Create(string error)
        {
            var res = new ErrorResponse {Error = error};
            return res;
        }
    }
}