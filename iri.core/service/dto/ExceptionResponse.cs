namespace iri.core.service.dto
{
    public class ExceptionResponse : AbstractResponse
    {
        public string Exception { get; private set; }

        public static AbstractResponse Create(string exception)
        {
            var res = new ExceptionResponse {Exception = exception};
            return res;
        }
    }
}