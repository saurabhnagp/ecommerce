using AmCart.UserService.Application.DTOs;
using AmCart.UserService.Application.Interfaces;
using AmCart.UserService.Domain.Entities;
using AmCart.UserService.UnitTests.TestFixtures;
using Moq;
using Xunit;

namespace AmCart.UserService.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Application.Services.UserService _sut;

    public UserServiceTests()
    {
        _userRepo = new Mock<IUserRepository>();
        _sut = new Application.Services.UserService(_userRepo.Object);
    }

    [Fact]
    public async Task GetByIdAsync_UserExists_ReturnsUserDto()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal("John Doe", result.Name);
        Assert.True(result.IsVerified);
    }

    [Fact]
    public async Task GetByIdAsync_UserNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserExists_UpdatesAndReturnsDto()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var request = new UpdateProfileRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Phone = "+9876543210",
            Gender = "male"
        };

        var result = await _sut.UpdateProfileAsync(user.Id, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Name", result.LastName);
        Assert.Equal("+9876543210", result.Phone);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _sut.UpdateProfileAsync(id, new UpdateProfileRequest { FirstName = "X" }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeactivateAccountAsync_UserExists_SetsStatusAndDeletedAt()
    {
        var user = UserFixtures.CreateActiveVerifiedUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.DeactivateAccountAsync(user.Id, CancellationToken.None);

        Assert.True(result);
        Assert.Equal("deactivated", user.Status);
        Assert.NotNull(user.DeletedAt);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAccountAsync_UserNotFound_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _sut.DeactivateAccountAsync(id, CancellationToken.None);

        Assert.False(result);
    }
}
