using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FamilyPulse.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FamilyPulse.Infrastructure.Security;

public class HouseKeyService : IHouseKeyService
{
    private readonly byte[] _hmacSecret;

    public HouseKeyService(IConfiguration config)
    {
        // 1. Read static secret from config or fallback to dev key (prevents key changing on app restart)
        string secret = config["Security:HouseKeySecret"] 
            ?? "FamilyPulse_Static_HouseKey_HMAC_Secret_987654321!";
        
        _hmacSecret = Encoding.UTF8.GetBytes(secret);
    }

    public string ExportHouseKeyJson(Guid familyId)
    {
        int version = 1;
        // ISO-8601 round-trip string
        string issuedAtUtc = DateTime.UtcNow.ToString("O");
        
        string payloadToSign = CreateSignablePayload(familyId, version, issuedAtUtc);
        string signature = ComputeHmacSignature(payloadToSign);

        var key = new HouseKeyExportDto(
            FamilyId: familyId,
            Version: version,
            IssuedAtUtc: issuedAtUtc,
            Signature: signature
        );

        return JsonSerializer.Serialize(key, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    public bool TryValidateHouseKey(string houseKeyJson, out Guid familyId)
    {
        familyId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(houseKeyJson))
            return false;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var key = JsonSerializer.Deserialize<HouseKeyExportDto>(houseKeyJson, options);
            if (key is null || key.FamilyId == Guid.Empty || string.IsNullOrWhiteSpace(key.Signature))
            {
                return false;
            }

            // 2. Re-create signable payload using exact string representation from JSON
            string payloadToSign = CreateSignablePayload(key.FamilyId, key.Version, key.IssuedAtUtc);
            string expectedSignature = ComputeHmacSignature(payloadToSign);

            // 3. Constant-time byte comparison prevents timing attacks
            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(key.Signature),
                Encoding.UTF8.GetBytes(expectedSignature)))
            {
                return false; // Signature mismatch
            }

            familyId = key.FamilyId;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string CreateSignablePayload(Guid familyId, int version, string issuedAtUtc)
    {
        return $"{familyId}:{version}:{issuedAtUtc}";
    }

    private string ComputeHmacSignature(string payload)
    {
        using var hmac = new HMACSHA256(_hmacSecret);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }

    // Treat IssuedAtUtc as string to preserve byte-level equality
    private record HouseKeyExportDto(
        Guid FamilyId,
        int Version,
        string IssuedAtUtc,
        string Signature
    );
}