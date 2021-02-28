namespace Authentick.DotNetSaml.Response
{
    public class EncodedSamlResponse
    {
        readonly string Response;

        public EncodedSamlResponse(string response)
        {
            Response = response;
        }

        public string GetResponseRaw()
        {
            return Response;
        }

        public string GetResponseBase64Encoded()
        {
            return Response;
        }
    }
}
