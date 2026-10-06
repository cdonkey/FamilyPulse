namespace FamilyPulse.Application.Common.Interfaces;

public interface IIdentityHasher
{
    /// <summary>
    /// Computes a deterministic identity hash from a passphrase and landmark.
    /// </summary>
    string HashIdentity(string passphrase, string virtualLandmarkId);

    /// <summary>
    /// Verifies if a given passphrase and landmark match a stored hash.
    /// </summary>
    bool VerifyIdentity(string passphrase, string virtualLandmarkId, string storedHash);
}