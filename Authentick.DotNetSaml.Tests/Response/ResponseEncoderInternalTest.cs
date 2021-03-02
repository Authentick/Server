using Authentick.DotNetSaml.Helper;
using Authentick.DotNetSaml.Response;
using Moq;
using System;
using Xunit;

namespace Authentick.DotNetSaml.Tests.Response
{
    public class SamlResponseEncoderInternalTest
    {
        [Fact]
        public void TestEncoding()
        {
            Mock<IGuidService> guidServiceMock = new Mock<IGuidService>();
            guidServiceMock.SetupSequence(m => m.NewGuid())
                .Returns(Guid.Parse("10000000-0000-0000-0000-000000000000"))
                .Returns(Guid.Parse("20000000-0000-0000-0000-000000000000"));

            EncoderSettings encoderSettings = new EncoderSettings();
            ResponseEncoderInternal encoder = new ResponseEncoderInternal(guidServiceMock.Object);
            DateTime.TryParse("2021-01-01", out DateTime dateTime);
            SamlResponseModel responseModel = new SamlResponseModel(
              "https://idp.example.org/SAML2",
              "identifier_1",
              "NameId",
              "https://sp.example.com/SAML2",
              "https://sp.example.com/SAML2/SSO/POST",
              dateTime,
              TimeSpan.FromMinutes(5)
            );
            EncodedSamlResponse response = encoder.Encode(encoderSettings, responseModel);

            Assert.Equal(
                "<samlp:Response xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" ID=\"id-10000000-0000-0000-0000-000000000000\" Version=\"2.0\" InResponseTo=\"identifier_1\" IssueInstant=\"2021-01-01T00:00:00Z\" Destination=\"https://sp.example.com/SAML2/SSO/POST\" xmlns:samlp=\"urn:oasis:names:tc:SAML:2.0:protocol\"><saml:Issuer>https://idp.example.org/SAML2</saml:Issuer><samlp:Status><samlp:StatusCode Value=\"urn:oasis:names:tc:SAML:2.0:status:Success\" /></samlp:Status><saml:Assertion xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" ID=\"id-20000000-0000-0000-0000-000000000000\" Version=\"2.0\" IssueInstant=\"2021-01-01T00:00:00Z\"><saml:Issuer>https://idp.example.org/SAML2</saml:Issuer><saml:Subject><saml:NameID Format=\"urn:oasis:names:tc:SAML:2.0:nameid-format:transient\">NameId</saml:NameID><saml:SubjectConfirmation Method=\"urn:oasis:names:tc:SAML:2.0:cm:bearer\"><saml:SubjectConfirmationData InResponseTo=\"identifier_1\" Recipient=\"https://sp.example.com/SAML2/SSO/POST\" NotOnOrAfter=\"2021-01-01T00:05:00Z\" /></saml:SubjectConfirmation></saml:Subject><saml:Conditions NotBefore=\"2021-01-01T00:00:00Z\" NotOnOrAfter=\"2021-01-01T00:05:00Z\"><saml:AudienceRestriction><saml:Audience>https://sp.example.com/SAML2</saml:Audience></saml:AudienceRestriction></saml:Conditions><saml:AuthnStatement AuthnInstant=\"2021-01-01T00:00:00Z\" SessionIndex=\"id-20000000-0000-0000-0000-000000000000\"><saml:AuthnContext><saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</saml:AuthnContextClassRef></saml:AuthnContext></saml:AuthnStatement></saml:Assertion></samlp:Response>",
                response.GetResponseRaw()
            );
        }
    }
}
