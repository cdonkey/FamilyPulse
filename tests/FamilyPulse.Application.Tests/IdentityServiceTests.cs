using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Identity.Commands;
using FamilyPulse.Application.Identity.Queries;
using FamilyPulse.Application.Identity.Services;
using FamilyPulse.Infrastructure.Data;
using FamilyPulse.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FamilyPulse.UnitTests.Application;

public class IdentityServiceTests
{
    private readonly IIdentityHasher _identityHasher;
    private readonly IHouseKeyService _houseKeyService;
    private readonly IPassphraseGenerator _passphraseGenerator;

    public IdentityServiceTests()
    {
        // 1. Setup real fast infrastructure components (no need to mock pure logic)
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:IdentityPepper"] = "Test_Identity_Pepper_123456789!",
                ["Security:HouseKeySecret"] = "Test_HouseKey_Secret_987654321!"
            })
            .Build();

        _identityHasher = new Pbkdf2IdentityHasher(config);
        _houseKeyService = new HouseKeyService(config);
        _passphraseGenerator = new PassphraseGenerator();
    }

    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Fresh isolated DB per test
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task RegisterAsync_WithGeneratedPassphrase_CreatesAccountAndReturnsHouseKey()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sut = new IdentityService(db, _identityHasher, _houseKeyService, _passphraseGenerator);

        var command = new RegisterAnonymousFamilyCommand(
            VirtualLandmarkId: "Central Park West",
            InitialMembers: [new CreateInitialMemberDto("Alex", "Parent")]
        );

        // Act
        var result = await sut.RegisterAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.FamilyId);
        Assert.False(string.IsNullOrWhiteSpace(result.Passphrase));
        Assert.False(string.IsNullOrWhiteSpace(result.HouseKeyJson));

        // Verify database persistence
        var account = await db.FamilyAccounts
            .Include(a => a.Members)
            .FirstOrDefaultAsync(a => a.Id == result.FamilyId);

        Assert.NotNull(account);
        Assert.Single(account.Members);
        Assert.Equal("Alex", account.Members.First().Name);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateCustomPassphrase_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sut = new IdentityService(db, _identityHasher, _houseKeyService, _passphraseGenerator);

        var command = new RegisterAnonymousFamilyCommand(
            VirtualLandmarkId: "Eiffel Tower Area",
            CustomPassphrase: "river-stone-falcon-bright"
        );

        // First registration succeeds
        await sut.RegisterAsync(command);

        // Act & Assert: Duplicate registration at the same landmark should throw
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.RegisterAsync(command));
    }

    [Fact]
    public async Task RecoverAsync_WithValidPassphraseAndLandmark_ReturnsAccountData()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sut = new IdentityService(db, _identityHasher, _houseKeyService, _passphraseGenerator);

        var regCommand = new RegisterAnonymousFamilyCommand(
            VirtualLandmarkId: "Brooklyn Bridge",
            CustomPassphrase: "haven-cedar-summit-delta"
        );

        var registration = await sut.RegisterAsync(regCommand);

        var recoverQuery = new RecoverAccountQuery(
            Passphrase: "haven-cedar-summit-delta",
            VirtualLandmarkId: "Brooklyn Bridge"
        );

        // Act
        var result = await sut.RecoverAsync(recoverQuery);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registration.FamilyId, result.FamilyId);
    }

    [Fact]
    public async Task RecoverAsync_WithValidHouseKeyJson_ReturnsAccountData()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sut = new IdentityService(db, _identityHasher, _houseKeyService, _passphraseGenerator);

        var regCommand = new RegisterAnonymousFamilyCommand("Empire State Building");
        var registration = await sut.RegisterAsync(regCommand);

        var recoverQuery = new RecoverAccountQuery(HouseKeyJson: registration.HouseKeyJson);

        // Act
        var result = await sut.RecoverAsync(recoverQuery);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registration.FamilyId, result.FamilyId);
    }

    [Fact]
    public async Task RecoverAsync_WithInvalidCredentials_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sut = new IdentityService(db, _identityHasher, _houseKeyService, _passphraseGenerator);

        var recoverQuery = new RecoverAccountQuery(
            Passphrase: "wrong-word-pass-phrase",
            VirtualLandmarkId: "Unknown Landmark"
        );

        // Act
        var result = await sut.RecoverAsync(recoverQuery);

        // Assert
        Assert.Null(result);
    }
}