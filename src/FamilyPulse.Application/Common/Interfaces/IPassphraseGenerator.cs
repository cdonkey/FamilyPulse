using System.Security.Cryptography;

namespace FamilyPulse.Application.Common.Interfaces;

public interface IPassphraseGenerator
{
    string Generate4WordPassphrase();
}

public class PassphraseGenerator : IPassphraseGenerator
{
    private static readonly string[] WordList = [
        "anchor", "beacon", "bridge", "canyon", "castle", "cedar", "compass", "crest",
        "crystal", "delta", "eagle", "ember", "falcon", "forest", "harbor", "haven",
        "island", "jasper", "lagoon", "meadow", "mountain", "ocean", "orbit", "path",
        "pine", "planet", "river", "shadow", "shield", "sierra", "silver", "solar",
        "summit", "timber", "valley", "vessel", "village", "vista", "wave", "willow"
    ];

    public string Generate4WordPassphrase()
    {
        Span<byte> randomBytes = stackalloc byte[4 * 4];
        RandomNumberGenerator.Fill(randomBytes);

        var words = new string[4];
        for (int i = 0; i < 4; i++)
        {
            uint index = BitConverter.ToUInt32(randomBytes.Slice(i * 4, 4));
            words[i] = WordList[index % WordList.Length];
        }

        return string.Join("-", words);
    }
}