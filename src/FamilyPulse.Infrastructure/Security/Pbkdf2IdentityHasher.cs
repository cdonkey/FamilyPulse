using System.Security.Cryptography;
using System.Text;
using FamilyPulse.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FamilyPulse.Infrastructure.Security;

public class Pbkdf2IdentityHasher : IIdentityHasher
{
    private readonly byte[] _pepperBytes;
    private const int Iterations = 210_000; // OWASP recommendation for PBKDF2-HMAC-SHA256
    private const int KeySize = 32;          // 256 bits

    public Pbkdf2IdentityHasher(IConfiguration configuration)
    {
        // Global server secret used as a pepper to ensure hashes cannot be cracked offline without it
        var pepper = configuration["Security:IdentityPepper"] 
                     ?? "FamilyPulse_Default_System_Pepper_Change_In_Production_32bytes!";
        
        _pepperBytes = Encoding.UTF8.GetBytes(pepper);
    }

  public string HashIdentity(string passphrase, string virtualLandmarkId)
    {
        if (string.IsNullOrWhiteSpace(passphrase))
            throw new ArgumentException("Passphrase cannot be empty.", nameof(passphrase));

        if (string.IsNullOrWhiteSpace(virtualLandmarkId))
            throw new ArgumentException("Virtual landmark ID cannot be empty.", nameof(virtualLandmarkId));

        // Normalize casing and trim leading/trailing whitespace
        var normalizedInput = $"{passphrase.Trim().ToLowerInvariant()}:{virtualLandmarkId.Trim().ToLowerInvariant()}";
        var inputBytes = Encoding.UTF8.GetBytes(normalizedInput);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            inputBytes,
            _pepperBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize
        );

        return Convert.ToBase64String(hash);
    }

    public bool VerifyIdentity(string passphrase, string virtualLandmarkId, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        var computedHash = HashIdentity(passphrase, virtualLandmarkId);

        // Fixed-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(storedHash)
        );
    }
}