using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Authentick.DotNetSaml.Response.Signign
{
    internal class ResponseSigner
    {
        private readonly X509Certificate2 _certificate;

        internal ResponseSigner(X509Certificate2 certificate)
        {
            _certificate = certificate;
        }

        internal string SignDocument(string unsignedXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(unsignedXml);

            SignedXml signedXml = new SignedXml(doc);
            signedXml.SigningKey = _certificate.GetRSAPrivateKey();

            Reference reference = new Reference();
            reference.Uri = "";

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            signedXml.AddReference(reference);

            signedXml.ComputeSignature();

            XmlElement xmlDigitalSignature = signedXml.GetXml();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(doc.NameTable);
            xmlNamespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            XmlNode? assertionNode = doc.SelectSingleNode("/samlp:Response", xmlNamespaceManager);
            XmlNode? issuerNode = doc.SelectSingleNode("/samlp:Response/saml:Issuer", xmlNamespaceManager);
            if (issuerNode == null)
            {
                throw new Exception("Issuer node is null in " + unsignedXml);
            }
            if (assertionNode == null)
            {
                throw new Exception("Assertion node is null in " + unsignedXml);
            }

            assertionNode.InsertAfter(doc.ImportNode(xmlDigitalSignature, true), issuerNode);

            if (doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }

            return doc.OuterXml;
        }
    }
}

