using Microsoft.EntityFrameworkCore;
using UserManagementService.Core.DTOs;
using UserManagementService.Data.Context;
using UserManagementService.Data.Entities;
using UserManagementService.Services.Implementations;
using UserManagementService.Services.Exceptions;

namespace UserManagementService.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private AppDbContext _context = null!;
    private UserService _userService = null!;
    private PasswordHasher _passwordHasher = null!;

    //seeded users
    private User client1User1 = null!;
    private User client1User2 = null!;
    private User client2User1 = null!;

    private int client1Id = 1;
    private int client2Id = 2;

    [SetUp]
    public void Setup()
    {
        // Setup in-memory database with unique name per test run
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _passwordHasher = new PasswordHasher();

        // Seed test data
        SeedTestData();

        // Create service
        _userService = new UserService(_context, _passwordHasher);
    }

    private void SeedTestData()
    {

        var (hash1, salt1) = _passwordHasher.HashPassword("Password123!");
        var (hash2, salt2) = _passwordHasher.HashPassword("AnotherPass456!");
        var (hash3, salt3) = _passwordHasher.HashPassword("SecurePass789!");
        
        client1User1 = new User
        {
            Id = 1,
            ApiClientId = client1Id,
            UserName = "client1user1",
            FullName = "Client 1 User 1",
            Email = "user1@client1.com",
            MobileNumber = "+1234567890",
            Language = "en",
            Culture = "en-US",
            PasswordHash = hash1,
            PasswordSalt = salt1,
        };

        client1User2 = new User
        {
            Id = 2,
            ApiClientId = client1Id,
            UserName = "client1user2",
            FullName = "Client 1 User 2",
            Email = "user2@client1.com",
            MobileNumber = "+1234567891",
            Language = "en",
            Culture = "en-US",
            PasswordHash = hash2,
            PasswordSalt = salt2,
        };

        client2User1 = new User
        {
            Id = 3,
            ApiClientId = client2Id,
            UserName = "client2user1",
            FullName = "Client 2 User 1",
            Email = "user1@client2.com",
            MobileNumber = "+0987654321",
            Language = "es",
            Culture = "es-ES",
            PasswordHash = hash3,
            PasswordSalt = salt3,
        };

        _context.Users.AddRange(
            client1User1,
            client1User2,
            client2User1
        );

        _context.SaveChanges();
    }

    #region Get User Tests

    [Test]
    public async Task GetUserAsync_ExistingUser_ReturnsUser()
    {
        var result = await _userService.GetUserAsync(client1User1.Id, client1User1.ApiClientId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(client1User1.Id));
        Assert.That(result.UserName, Is.EqualTo(client1User1.UserName));
        Assert.That(result.Email, Is.EqualTo(client1User1.Email));
    }

    [Test]
    public void GetUserAsync_NonExistentUser_ThrowsUserNotFoundException()
    {
        var exception = Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _userService.GetUserAsync(-1, client1Id)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("User with ID -1 not found"));
    }

    [Test]
    public void GetUserAsync_DifferentClient_ThrowsUserNotFoundException()
    {
        // Client 1 trying to access Client 2's user
        Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _userService.GetUserAsync(client2User1.Id, client1Id)
        );
    }

    #endregion

    #region Create User Tests

    [Test]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        var request = new CreateUserRequest
        {
            UserName = "newuser",
            FullName = "New User",
            Email = "newuser@test.com",
            MobileNumber = "+1111111111",
            Language = "en",
            Culture = "en-US",
            Password = "SecurePass123!"
        };

        var result = await _userService.CreateUserAsync(request, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserName, Is.EqualTo("newuser"));
        Assert.That(result.Email, Is.EqualTo("newuser@test.com"));

        // Verify in database
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "newuser");
        Assert.That(userInDb, Is.Not.Null);
        Assert.That(userInDb!.ApiClientId, Is.EqualTo(1));
        Assert.That(userInDb.PasswordHash, Is.Not.EqualTo(request.Password));
    }

    [Test]
    public void CreateUserAsync_DuplicateUsername_SameClient_ThrowsDuplicateException()
    {
        var request = new CreateUserRequest
        {
            UserName = "client1user1", // Already exists
            FullName = "Duplicate User",
            Email = "different@email.com",
            MobileNumber = "+2222222222",
            Language = "en",
            Culture = "en-US",
            Password = "SecurePass123!"
        };

        
        var exception = Assert.ThrowsAsync<DuplicateUserException>(
            async () => await _userService.CreateUserAsync(request, client1Id)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("Username already exists"));
    }

    [Test]
    public void CreateUserAsync_DuplicateEmail_SameClient_ThrowsDuplicateException()
    {
        var request = new CreateUserRequest
        {
            UserName = "uniqueuser",
            FullName = "New User",
            Email = "user1@client1.com", // Already exists
            MobileNumber = "+3333333333",
            Language = "en",
            Culture = "en-US",
            Password = "SecurePass123!"
        };

        var exception = Assert.ThrowsAsync<DuplicateUserException>(
            async () => await _userService.CreateUserAsync(request, client1Id)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("Email already exists"));
    }

    [Test]
    public async Task CreateUserAsync_DuplicateUsername_DifferentClient_Succeeds()
    {
        // Client 2 creating user with same username as Client 1
        var request = new CreateUserRequest
        {
            UserName = "client1user1", // Same as Client 1's user
            FullName = "Client 2 User",
            Email = "unique@client2.com",
            MobileNumber = "+4444444444",
            Language = "en",
            Culture = "en-US",
            Password = "SecurePass123!"
        };

        
        var result = await _userService.CreateUserAsync(request, client2Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserName, Is.EqualTo("client1user1"));

        // Verify both users exist
        var usersWithSameName = await _context.Users
            .Where(u => u.UserName == "client1user1")
            .ToListAsync();

        Assert.That(usersWithSameName, Has.Count.EqualTo(2));
        Assert.That(usersWithSameName.Any(u => u.ApiClientId == client1Id), Is.True);
        Assert.That(usersWithSameName.Any(u => u.ApiClientId == client2Id), Is.True);
    }

    [Test]
    public async Task CreateUserAsync_PasswordIsHashed()
    {
        var plainPassword = "MySecretPassword123!";
        var request = new CreateUserRequest
        {
            UserName = "hasheduser",
            FullName = "Hashed User",
            Email = "hashed@test.com",
            MobileNumber = "+6666666666",
            Language = "en",
            Culture = "en-US",
            Password = plainPassword
        };

        await _userService.CreateUserAsync(request, 1);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "hasheduser");
        Assert.That(user, Is.Not.Null);
        Assert.That(user!.PasswordHash, Is.Not.EqualTo(plainPassword));
        Assert.That(_passwordHasher.VerifyPassword(plainPassword, user.PasswordHash, user.PasswordSalt), Is.True);
    }

    #endregion

    #region Update User Tests

    [Test]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        var request = new UpdateUserRequest
        {
            FullName = "Updated Name",
            Email = "updated@client1.com",
            MobileNumber = "+9999999999"
        };

        var result = await _userService.UpdateUserAsync(client1User1.Id, request, client1Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FullName, Is.EqualTo("Updated Name"));
        Assert.That(result.Email, Is.EqualTo("updated@client1.com"));
        Assert.That(result.MobileNumber, Is.EqualTo("+9999999999"));

        // Verify in database
        var userInDb = await _context.Users.FindAsync(client1User1.Id);
        Assert.That(userInDb, Is.Not.Null);
        Assert.That(userInDb!.FullName, Is.EqualTo("Updated Name"));
    }

    [Test]
    public void UpdateUserAsync_NonExistentUser_ThrowsUserNotFoundException()
    {
        var request = new UpdateUserRequest { FullName = "New Name" };

        var exception = Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _userService.UpdateUserAsync(-1, request, client1Id)
        );
    }

    [Test]
    public void UpdateUserAsync_DuplicateEmail_ThrowsDuplicateException()
    {
        // try to update user 1 with email from user 2
        var request = new UpdateUserRequest
        {
            Email = "user2@client1.com" 
        };

        
        var exception = Assert.ThrowsAsync<DuplicateUserException>(
            async () => await _userService.UpdateUserAsync(client1User1.Id, request, client1Id)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Does.Contain("Email already exists"));
    }

    [Test]
    public async Task UpdateUserAsync_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // only update FullName
        var originalUser = await _context.Users.FindAsync(client1User1.Id);
        var originalEmail = originalUser!.Email;

        var request = new UpdateUserRequest
        {
            FullName = "Only Name Changed"
        };

        var result = await _userService.UpdateUserAsync(client1User1.Id, request, client1Id);

        Assert.That(result.FullName, Is.EqualTo("Only Name Changed"));
        Assert.That(result.Email, Is.EqualTo(originalEmail)); 
    }

    #endregion

    #region Delete User Tests

    [Test]
    public async Task DeleteUserAsync_ExistingUser_DeletesUser()
    {
        await _userService.DeleteUserAsync(client1User2.Id, client1Id);

        var deletedUser = await _context.Users.FindAsync(client1User2.Id);
        Assert.That(deletedUser, Is.Null);
    }

    [Test]
    public void DeleteUserAsync_NonExistentUser_ThrowsUserNotFoundException()
    {
        Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _userService.DeleteUserAsync(-1, client1Id)
        );
    }

    [Test]
    public void DeleteUserAsync_DifferentClient_ThrowsUserNotFoundException()
    {
        // client 1 trying to delete Client 2's user
        Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _userService.DeleteUserAsync(client2User1.Id, client1Id)
        );
    }

    #endregion

    #region Validate Password Tests

    [Test]
    public async Task ValidatePasswordAsync_CorrectPassword_ReturnsTrue()
    {
        var result = await _userService.ValidatePasswordAsync(client1User1.Id, "Password123!", client1Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task ValidatePasswordAsync_IncorrectPassword_ReturnsFalse()
    {
        var result = await _userService.ValidatePasswordAsync(client1User1.Id, "WrongPassword!", client1Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void ValidatePasswordAsync_NonExistentUser_ThrowsUserNotFoundException()
    {
        Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _userService.ValidatePasswordAsync(-1, "Password123!", client1Id)
        );
    }

    [Test]
    public void ValidatePasswordAsync_DifferentClient_ThrowsUserNotFoundException()
    {
        //client 1 trying to validate Client 2's user password
        Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _userService.ValidatePasswordAsync(client2User1.Id, "SecurePass789!", client1Id)
        );
    }

    #endregion

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
