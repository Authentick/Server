using Authentick.DotNetSaml.Helper;

namespace Authentick.DotNetSaml.Response
{
    public class ResponseEncoder
    {
        public EncodedSamlResponse Encode(EncoderSettings encoderSettings, SamlResponseModel model)
        {
            ResponseEncoderInternal internalEncoder = new ResponseEncoderInternal(new GuidService());
            return internalEncoder.Encode(encoderSettings, model);            
        }
    }
}
