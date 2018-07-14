using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using McMaster.Extensions.CommandLineUtils;

namespace Vanguard.ServerManager.Node.Abstractions
{
    public class EncryptionHelpers
    {
        public static X509Certificate2 GenerateServerNodeCertificate(string nodeName, string password)
        {
            using (var rsa = RSA.Create(4096))
            {
                var request = new CertificateRequest($"CN={nodeName}", rsa, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

                request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                var sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddIpAddress(IPAddress.Loopback);
                sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
                sanBuilder.AddDnsName("localhost");
                sanBuilder.AddDnsName(Environment.MachineName);
                request.CertificateExtensions.Add(sanBuilder.Build());

                var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
                certificate.FriendlyName = "VanguardServerManagerNodeCertificate";

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx, password), password, X509KeyStorageFlags.MachineKeySet);
            }
        }

        public static X509Certificate2 LoadServerNodeCertificate(string certFilePath, string keyFilePath = default)
        {
            if (!string.IsNullOrEmpty(keyFilePath))
            {
                byte[] privateKeyBytes;
                using (var fileStream = File.Open(keyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    privateKeyBytes = new byte[fileStream.Length];
                    fileStream.Read(privateKeyBytes, 0, privateKeyBytes.Length);
                }

                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportCspBlob(privateKeyBytes);
                    return new X509Certificate2(certFilePath) {PrivateKey = rsa};
                }
            }

            var password = Prompt.GetPassword("Provide the certificate password:");
            var confirmPassword = Prompt.GetPassword("Confirm the certificate password:");
            if (password != confirmPassword)
            {
                throw new Exception("Passwords don't match");
            }

            return new X509Certificate2(certFilePath, password);
        }
    }
}