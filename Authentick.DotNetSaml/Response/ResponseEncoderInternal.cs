using Authentick.DotNetSaml.Extensions;
using Authentick.DotNetSaml.Helper;
using System;
using System.IO;
using System.Xml;

namespace Authentick.DotNetSaml.Response
{
    internal class ResponseEncoderInternal
    {
        private readonly IGuidService _guidService;

        internal ResponseEncoderInternal(IGuidService guidService) {
            _guidService = guidService;
        }

        internal EncodedSamlResponse Encode(EncoderSettings encoderSettings, SamlResponseModel model)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;

            string samlIdentifier = "id-" + _guidService.NewGuid();
            string samlAssertionIdentifier = "id-" + _guidService.NewGuid();

            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("samlp", "Response", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("xmlns", "saml", null, "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteAttributeString("ID", samlIdentifier);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("InResponseTo", model.InResponseTo);
                    xw.WriteAttributeString("IssueInstant", model.TimeStamp.ToSamlInstant());
                    xw.WriteAttributeString("Destination", model.Destination);

                    xw.WriteStartElement("saml", "Issuer", null);
                    xw.WriteString(model.Issuer);
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "Status", null);
                    xw.WriteStartElement("samlp", "StatusCode", null);
                    xw.WriteAttributeString("Value", "urn:oasis:names:tc:SAML:2.0:status:Success");
                    xw.WriteEndElement();
                    xw.WriteEndElement();

                    xw.WriteStartElement("saml", "Assertion", null);
                    xw.WriteAttributeString("xmlns", "saml", null, "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteAttributeString("ID", samlAssertionIdentifier);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", model.TimeStamp.ToSamlInstant());
                    xw.WriteStartElement("saml", "Issuer", null);
                    xw.WriteString(model.Issuer);
                    xw.WriteEndElement();
                    xw.WriteStartElement("saml", "Subject", null);
                    xw.WriteStartElement("saml", "NameID", null);
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:transient");
                    xw.WriteString(model.NameId);
                    xw.WriteEndElement();
                    xw.WriteStartElement("saml", "SubjectConfirmation", null);
                    xw.WriteAttributeString("Method", "urn:oasis:names:tc:SAML:2.0:cm:bearer");
                    xw.WriteStartElement("saml", "SubjectConfirmationData", null);
                    xw.WriteAttributeString("InResponseTo", model.InResponseTo);
                    xw.WriteAttributeString("Recipient", model.Destination);
                    xw.WriteAttributeString("NotOnOrAfter", model.TimeStamp.Add(model.ValidityTimeSpan).ToSamlInstant());
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteStartElement("saml", "Conditions", null);
                    xw.WriteAttributeString("NotBefore", model.TimeStamp.ToSamlInstant());
                    xw.WriteAttributeString("NotOnOrAfter", model.TimeStamp.Add(model.ValidityTimeSpan).ToSamlInstant());
                    xw.WriteStartElement("saml", "AudienceRestriction", null);
                    xw.WriteStartElement("saml", "Audience", null);
                    xw.WriteString(model.Audience);
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteStartElement("saml", "AuthnStatement", null);
                    xw.WriteAttributeString("AuthnInstant", model.TimeStamp.ToSamlInstant());
                    xw.WriteAttributeString("SessionIndex", samlAssertionIdentifier.ToString());
                    xw.WriteStartElement("saml", "AuthnContext", null);
                    xw.WriteStartElement("saml", "AuthnContextClassRef", null);
                    xw.WriteString("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport");
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                return new EncodedSamlResponse(sw.ToString());
            }
        }
    }
}