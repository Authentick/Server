namespace Gatekeeper.Server.Web.Services.Authentication.DeviceCookie
{
    public class EncryptedDeviceCookie
    {
        public readonly string EncryptedValue;

        public EncryptedDeviceCookie(string encryptedCookieValue)
        {
            EncryptedValue = encryptedCookieValue;
        }
    }
}
