using Authentick.DotNetSaml.Certificate;
using Authentick.DotNetSaml.Response.Signign;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Authentick.DotNetSaml.Tests.Response.Signing
{
    public class ResponseSignerTest
    {
        [Fact]
        public void TestSignDocument()
        {
            string input = "<samlp:Response xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" ID=\"id-10000000-0000-0000-0000-000000000000\" Version=\"2.0\" InResponseTo=\"identifier_1\" IssueInstant=\"2021-01-01T00:00:00Z\" Destination=\"https://sp.example.com/SAML2/SSO/POST\" xmlns:samlp=\"urn:oasis:names:tc:SAML:2.0:protocol\"><saml:Issuer>https://idp.example.org/SAML2</saml:Issuer><samlp:Status><samlp:StatusCode Value=\"urn:oasis:names:tc:SAML:2.0:status:Success\" /></samlp:Status><saml:Assertion xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" ID=\"id-20000000-0000-0000-0000-000000000000\" Version=\"2.0\" IssueInstant=\"2021-01-01T00:00:00Z\"><saml:Issuer>https://idp.example.org/SAML2</saml:Issuer><saml:Subject><saml:NameID Format=\"urn:oasis:names:tc:SAML:2.0:nameid-format:transient\">NameId</saml:NameID><saml:SubjectConfirmation Method=\"urn:oasis:names:tc:SAML:2.0:cm:bearer\"><saml:SubjectConfirmationData InResponseTo=\"identifier_1\" Recipient=\"https://sp.example.com/SAML2/SSO/POST\" NotOnOrAfter=\"2021-01-01T00:05:00Z\" /></saml:SubjectConfirmation></saml:Subject><saml:Conditions NotBefore=\"2021-01-01T00:00:00Z\" NotOnOrAfter=\"2021-01-01T00:05:00Z\"><saml:AudienceRestriction><saml:Audience>https://sp.example.com/SAML2</saml:Audience></saml:AudienceRestriction></saml:Conditions><saml:AuthnStatement AuthnInstant=\"2021-01-01T00:00:00Z\" SessionIndex=\"id-20000000-0000-0000-0000-000000000000\"><saml:AuthnContext><saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</saml:AuthnContextClassRef></saml:AuthnContext></saml:AuthnStatement></saml:Assertion></samlp:Response>";
            string expected = "<samlp:Response xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" ID=\"id-10000000-0000-0000-0000-000000000000\" Version=\"2.0\" InResponseTo=\"identifier_1\" IssueInstant=\"2021-01-01T00:00:00Z\" Destination=\"https://sp.example.com/SAML2/SSO/POST\" xmlns:samlp=\"urn:oasis:names:tc:SAML:2.0:protocol\"><saml:Issuer>https://idp.example.org/SAML2</saml:Issuer><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><SignatureMethod Algorithm=\"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256\" /><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\" /><DigestValue>4xpJYSHYFCLRfHE5MXamrR2ThFKjPoedeTZ9YRLZJaQ=</DigestValue></Reference></SignedInfo><SignatureValue>Jb/QreZLScBID8yulYe/ADuZkgPH7wHb1DZqjYR4VvpO70dmojqZE8zOr0sOnBuP68mqx87ac6nsED2orWPRdhweNSO0Xpwhf70EL88n8la1zbY+4B+gMzR92Ks7fWX9Sifzz7xzXHpXeUgIM7ogSHLsmBmCdYFsQzWXImyr3Zpw8mi9S2VsLo79eH/zmeux1ZMui91YEdSCg9rCjcAU7zHzkgdqFl7i3XzE3k/hrVpX115ADSgDDNVc+ziFcj5xkJlJcEfEtT/qBkZ0Qh9nv/gx4Cx0jsat4Qeowx4jTNzHVx68M1osDNjx3n3JYsrSwUNHE+2lR+bMCrpRZAIeQKA0MFu5p7Ovwoj6rS5H5BX12zJVhzNatCQ/Ht1SiY9DfJrbdiLX7UQbqwAB4eWteeF5qC2RxTPHMgXRZix5Mjxmb9BV7D286NXhaH7vvP1mCSaAL6AperJp4rXpTLRjrTaik4tBnplYtoDVp0I9nUOadOz+AGxmbfCXZIciZqSi7E1cwd+b8Ny3pxhuERJHjwuOQm0t3NVXGnHQsLmFcA2puIu8Ve/+LCq5HA6nRTKlLGH0dEaI0kT2jWQbynVYfNsOLSXzhwUb1vDNM7srMVOu0zTUgjKxc8mSMLrP1FjhybcLWBnaDzBUT6toaOQ6lXKRazCSxJ8SyxCkZnA4+u4=</SignatureValue></Signature><samlp:Status><samlp:StatusCode Value=\"urn:oasis:names:tc:SAML:2.0:status:Success\" /></samlp:Status><saml:Assertion xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" ID=\"id-20000000-0000-0000-0000-000000000000\" Version=\"2.0\" IssueInstant=\"2021-01-01T00:00:00Z\"><saml:Issuer>https://idp.example.org/SAML2</saml:Issuer><saml:Subject><saml:NameID Format=\"urn:oasis:names:tc:SAML:2.0:nameid-format:transient\">NameId</saml:NameID><saml:SubjectConfirmation Method=\"urn:oasis:names:tc:SAML:2.0:cm:bearer\"><saml:SubjectConfirmationData InResponseTo=\"identifier_1\" Recipient=\"https://sp.example.com/SAML2/SSO/POST\" NotOnOrAfter=\"2021-01-01T00:05:00Z\" /></saml:SubjectConfirmation></saml:Subject><saml:Conditions NotBefore=\"2021-01-01T00:00:00Z\" NotOnOrAfter=\"2021-01-01T00:05:00Z\"><saml:AudienceRestriction><saml:Audience>https://sp.example.com/SAML2</saml:Audience></saml:AudienceRestriction></saml:Conditions><saml:AuthnStatement AuthnInstant=\"2021-01-01T00:00:00Z\" SessionIndex=\"id-20000000-0000-0000-0000-000000000000\"><saml:AuthnContext><saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</saml:AuthnContextClassRef></saml:AuthnContext></saml:AuthnStatement></saml:Assertion></samlp:Response>";

            string privateKey = @"-----BEGIN PRIVATE KEY-----
MIIJRAIBADANBgkqhkiG9w0BAQEFAASCCS4wggkqAgEAAoICAQDpYCNlwloAJslK5834KDe/u9KdgUcM2YCOBm4Kt6TyBTZ4QGHSWpdnAp4D4ZoNyY1Y9kM8UcP6isNrWdete8zgH0c9Ehit1WbOkIZsw570c3TKO0AekfnKe3A47/F6cay2dxjzMuMKEeb+MGKlLTQsIhsrE0NQXNCdEXcDMWA6KNBxUqbF31pHNROA0K5Sx3vxtbT+ILEq/AGbrZAeF+STmsq+POXyqas/54xanwosEkeVby0MGqqJYNEvd0uYOOJyCH+pa8mRD1K4JPyjkiQ6cvoGSPLX9URhKTiIaZdoiL1H6Ewzqnu1h5nAypzYDpMWotU50q4yWS/pTgpTIO96wJAcCl9x9GY04m0ij5NXzkvCXb2h1g0l08ySh6Je2wxPdoFPuOgvy2xkMirnYdqaRuGwDgkfGm5J3B7/u8KV7YNkyzzoHf0C4HK8GoOKjJLet/I97o6/N+BNs7saJo1mCsXmz/IOkvzH6hQfaNG0+dtNv/v285sdPJL4gp7kQVVixzPaLp/9ZmMWqFG6rSmA/Qv7a2iYtxI31UFszp1fu3BzUIzmxaq6/fStBc5ZxTDejnN9rVqCFGV76Z/eVzCTUvCzO2Gs1yMKV4Eq66rdGRCzDPY2n+Q7+DcZiZVQC8ZylQe5Zb2PWTEai45TK9r0aNuzHwK0WwxVw4JySroXCQIDAQABAoICAA2slGZh/OQFYcYBzw+7S4jweGbCLGeUVvP7bHc0S3Xi2E4zd4fbxwNCJAAWN/CW3rFXvJjMMa98cfbYTMrddiOrsYDijo+g8WUpErdMvwOTwEQZNKiV5Qz4Lpsnc/64qmYGlG+ri6ILan16WE1VWLk9Rlo/xrHuvkp1u4N0YS8HNS9uqFbvv1pswbfKgmxXQ+vlubUqNTdPjMiSYt9ghwQfDcIXz2Foj6Au8QpeRzpN1+Ppn7oV1eg2xAsD8mn460vDM7e5mRXmz/H6ONNcvMKv+fZkT9ZXiKoowCc+CKEXYf/UnKJqWtVIHLovJQGD2Rp6cAPzVEGe+C+zYud8BowOEm726PkWau2ZTYOQfHKdFTshcvaVxtqkQ0i+756GzMVRbo40P2sF24MI0iaHMvTcIAf1BtL/qY4YFn9ZJnzn7Y7B6Q3jT4kb008TpDDMcAGZXZAZOklfTRNGwDl61j0PwKpeY2LZXx/9QrQUk4au2qmW48TBbwqJaf+xw13nTOZSGsTGhzS9lGRMTxbGwQQ9VYG8iYMx831GL1ewp2eorHCadjZHPvVYP5N5RLBiXgCGUGEY7jbqHrlUcgyHllxogoUfWaj3rHFqVjrVFO3C+w87Hm1giGYDRcRExHqdy5XebWtPQwpWda5wLe871fWlytYLweH2yPLJhzHA1oABAoIBAQD9x6EMi/qgd5cNc1PRvU3FpcrjiPSJw7A81tJDCYZQCHrNZ/yRvu3ufwRRnFrTUqnCaDxamvvOUWbjvvzEy/Ub00B/ruJV16tCrCzojnap47KmaSTCFQlT0zqzFaRUgZja9GNCLvJQv/uDGLrWT4PvF5FR5fU762hwjFrbT47erpc3BJ2PbNbd89pNWryXh0A+w8cCanrfC4lWdOgn6HfGfOIa6uPXZUpJbbqy7DU+7Sb3pRrJ3MnZvWMnfvOvorVv/pE60BV+agJlN4S/pEHvJ2DARfYv3cs6o89UqqHhW5BaTcZLtN9g3OHnSQ1F0YQUukePJR0NjeBM/UDq0IZZAoIBAQDras+zsmZNQ66d1b/xCQhU7OLD7s7ZQfb7XWrJX2o/mFsuLIyQ63Bx0y6Fi2R3f6s+0KV/Pqtl8U2HAJUfG47Klymp96LS+/sgJdUhvVCkw5HX/cz82YmEsBo20ByMu0ZjK4F7x0togYH6AKMFM5MERl+Gy6Fe5l/3dPEi1tKKdvQptj6rM3dH02CTRbhclLhR5iIj+d4lcndzSobI5nw34NC/2FyzQWaf7rLNAX/JknUrAzm8Ef8j4Xd6+FF6j7vUFX2cYhp1KX3k+l8EkzdvuSyN6UkKTomP/XQrzNrQZ3B+xhm5E8feuktzchy2wd+LkKGFvK0f92uyp6QKB2AxAoIBAQCOnwU8RuKoEe17KqgdhFTT0fPu4cYEMky2NEhgUlcAXXOeQmVoBzQyR4HG16cXgipTTj808Eq5TXgUX/4wHyt14sgpucALXDQsORTX8VBw870v1oFI7Yeba2dew7fhoh+kVZn+1OFrTilsKJa/4FoWIVmS/DhQ32CHd+mLvO9e6CGlUtu1ggZTIDs8I9U/F0ycuWv7SJobBaG+S984pmXBz+dGF2HKaGtSTu+QpDzcS20N2eTvMzzhOJUj2i3bNAbhlOBLfCvIKo2EEqpxBT/kxm2SQP81MWGIaA0Z2Pc27j128qSRYWn0Rs5SK/TgmvmuqG2U3dexCV3YBq/YBy6pAoIBAQDdXHohRkbbsCCpWSw6wXuMH5K7Gfp2X4iVo6md4JJPajuSl2E1g60f2quL9x5pHysuzZJQq+UO3m8/2lReA9Oou6r5n1kDet0PxYM7ToBd7Wdd6dCukP62PcDoeZfe3Nm6tlXyxdsqUVyeb6raYTyEcIeygBck3Qqv7M4xLD6c5G2CK1OSN7qZVzEgShapN0559CN2IKW6IIAhcJp8nf1/rW2cdx3zkDOnfxOxoQ8/wu9ZmpbXOTTn0EJA5u32iODc87hBLB4kvPccMDDYZHVkIIK8jfeYMXE4a+KorN+zxxAIpM3bMbZmwFXiIBLP5k0FNuIqltdTj5s9SlT8+SKBAoIBAQDmnP5LyBk+ni3P8gHxEER5o7BQ3A3UFb2F4cVcrtf9X3PxcinodYiB9TbQowUECF0nXQrbEKCOjY+fRYVTItZw7z/vF8Wuyp6lvk/fHCqpKjG7yWKbkdvdKCv5/W7bfohpCQRXPcMYIt2AOdA5icGmKK2pXPPAfKIa2eDhRC/jdHP44hw+E1Ho3OlHVx25AfO9QnmbqlY7SAclsnzOeZEXBoQkfp66ILZ0RuahBEzHuUxnne75YLhJdNwx4glqSXjZQP+P2vZDxGm1bdN5EC30I07oH0unHNpOyYqWiTc3Hm32qOuJ5ohUH8fYL4qeUbtStQzoGK//wSTIAEfwL1sG
-----END PRIVATE KEY-----";

            string publicKey = @"-----BEGIN CERTIFICATE-----
MIIEqjCCApKgAwIBAgIICdgcAr66iZwwDQYJKoZIhvcNAQELBQAwFTETMBEGA1UEAxMKQXV0aGVu
dGljazAeFw0yMTAzMDEyMTQxMTFaFw0zMTAzMDIyMTQxMTFaMBUxEzARBgNVBAMTCkF1dGhlbnRp
Y2swggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQDpYCNlwloAJslK5834KDe/u9KdgUcM
2YCOBm4Kt6TyBTZ4QGHSWpdnAp4D4ZoNyY1Y9kM8UcP6isNrWdete8zgH0c9Ehit1WbOkIZsw570
c3TKO0AekfnKe3A47/F6cay2dxjzMuMKEeb+MGKlLTQsIhsrE0NQXNCdEXcDMWA6KNBxUqbF31pH
NROA0K5Sx3vxtbT+ILEq/AGbrZAeF+STmsq+POXyqas/54xanwosEkeVby0MGqqJYNEvd0uYOOJy
CH+pa8mRD1K4JPyjkiQ6cvoGSPLX9URhKTiIaZdoiL1H6Ewzqnu1h5nAypzYDpMWotU50q4yWS/p
TgpTIO96wJAcCl9x9GY04m0ij5NXzkvCXb2h1g0l08ySh6Je2wxPdoFPuOgvy2xkMirnYdqaRuGw
DgkfGm5J3B7/u8KV7YNkyzzoHf0C4HK8GoOKjJLet/I97o6/N+BNs7saJo1mCsXmz/IOkvzH6hQf
aNG0+dtNv/v285sdPJL4gp7kQVVixzPaLp/9ZmMWqFG6rSmA/Qv7a2iYtxI31UFszp1fu3BzUIzm
xaq6/fStBc5ZxTDejnN9rVqCFGV76Z/eVzCTUvCzO2Gs1yMKV4Eq66rdGRCzDPY2n+Q7+DcZiZVQ
C8ZylQe5Zb2PWTEai45TK9r0aNuzHwK0WwxVw4JySroXCQIDAQABMA0GCSqGSIb3DQEBCwUAA4IC
AQDCPpx+Z/klbORRvDNpb+kN9ib8+bMd8wYf2+IHZp/dcksYHouM/0S+evoFaUYTNkSGSbhxgCGt
9u/XMo0s68IUjNtZ5Ygx5LvosqSFYIhtpRXqkXrZchm3F8Qgku4MeZjfuUUhR9Y3ip6v9Hh8IWUB
dkajsmWzc9i/VkWNNpp4meTVKBfEaS8eDrOBYRP96v1GSxgHGcayqBb35EFaHSrORj/rU37QoUOT
5ybffOow5idt9UdDR5KxlxR+UDJIcGZBhOJFDCKzdMS6uK+hStZ1/1tkpOpc41jicPcZRdUcrnii
XG/cE+dn0vgNQE5vpqmOnnH+FaMuIGeczzlIbYneT6+Bpg6pEXykAbNsKVbXqoUXgoWHd26y+w7Y
Z2oYKg5vUdl/5uSAU89kZSAf+ZF4f6g1T01YFJmZqfaVNUhN173D2298Omc7xcmes0CDOeZvNNAf
hqSzU3H/uWDlkZLoycKYwYWTFrb8u4c+hIcKksll+uwSr5Qm+BqFyTkuEzbXq7+7LQwqYnuw5WXe
HZgwuv5S3u/OoyvXpj9OFgkTJLNtDkTVZDihk/ieKzd2pehrJ6INHdQS0YgETQXIjrcDrZ38Ck+y
tZPBN8QeWaRVeb1/jEPUNs0ff8PR8mXNPKaLK/3UCjeA7oU0Tx1Sns79jKWPxFICER68IPhcTqyn
kA==
-----END CERTIFICATE-----";
            
            PemKeyConverter converter = new PemKeyConverter();
            X509Certificate2 certificate = converter.GetCertificate(publicKey, privateKey);

            ResponseSigner signer = new ResponseSigner(certificate);

            Assert.Equal(
                expected,
                signer.SignDocument(input)
            );
        }
    }
}
