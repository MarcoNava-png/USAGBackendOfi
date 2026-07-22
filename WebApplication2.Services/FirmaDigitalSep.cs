using System.Security.Cryptography;
using System.Text;

namespace WebApplication2.Services
{
    public static class FirmaDigitalSep
    {
        public static string GenerarSello(string cadenaOriginal, byte[] llavePrivada, string password)
        {
            using var rsa = RSA.Create();

            try
            {
                rsa.ImportEncryptedPkcs8PrivateKey(password.AsSpan(), llavePrivada, out _);
            }
            catch
            {
                var pem = Encoding.UTF8.GetString(llavePrivada);
                rsa.ImportFromEncryptedPem(pem, password.AsSpan());
            }

            var datos = Encoding.UTF8.GetBytes(cadenaOriginal);
            var firma = rsa.SignData(datos, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(firma);
        }
    }
}
