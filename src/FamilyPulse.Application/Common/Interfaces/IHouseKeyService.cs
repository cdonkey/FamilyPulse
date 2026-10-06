namespace FamilyPulse.Application.Common.Interfaces;

public interface IHouseKeyService
{
    /// <summary>
    /// Generates a signed, tamper-proof JSON string representing the family house key file.
    /// </summary>
    string ExportHouseKeyJson(Guid familyId);

    /// <summary>
    /// Validates the HMAC signature of an imported house key file and extracts the FamilyId if valid.
    /// </summary>
    bool TryValidateHouseKey(string jsonContent, out Guid familyId);
}