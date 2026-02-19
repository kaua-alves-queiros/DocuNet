using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Dtos.User;
using DocuNet.Web.Models;
using DocuNet.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocuNet.Test.Services
{
    public class UserServiceTests
    {
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole<Guid>>> _roleManagerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _loggerMock = new Mock<ILogger<UserService>>();
            
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole<Guid>>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole<Guid>>>(
                roleStoreMock.Object, null!, null!, null!, null!);

            _userService = new UserService(_loggerMock.Object, _userManagerMock.Object, _roleManagerMock.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnSuccess_WhenDataIsValidAndUserIsAdmin()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId, Email = "admin@test.com" };
            var dto = new CreateUserDto(adminId, "newuser@test.com", "Password123!", "Password123!");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString()))
                .ReturnsAsync(adminUser);

            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser))
                .ReturnsAsync(false);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.CreateUserAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Usuário criado com sucesso.", result.Message);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), dto.Password), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnError_WhenUserIsAdminButLockedOut()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId, Email = "admin@test.com" };
            var dto = new CreateUserDto(adminId, "newuser@test.com", "Password123!", "Password123!");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString()))
                .ReturnsAsync(adminUser);

            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.CreateUserAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Acesso negado: Você não tem permissão ou sua conta está desativada.", result.Message);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnError_WhenUserIsNotAdmin()
        {
            // Arrange
            var requesterId = Guid.NewGuid();
            var requesterUser = new User { Id = requesterId, Email = "user@test.com" };
            var dto = new CreateUserDto(requesterId, "newuser@test.com", "Password123!", "Password123!");

            _userManagerMock.Setup(x => x.FindByIdAsync(requesterId.ToString()))
                .ReturnsAsync(requesterUser);

            _userManagerMock.Setup(x => x.IsInRoleAsync(requesterUser, SystemRoles.SystemAdministrator))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.CreateUserAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Acesso negado: Você não tem permissão ou sua conta está desativada.", result.Message);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnError_WhenValidationFails()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            // Senhas não conferem -> Falha de validação via DataAnnotations
            var dto = new CreateUserDto(adminId, "invalid-email", "pass", "different-pass");

            // Act
            var result = await _userService.CreateUserAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Dados inválidos", result.Message);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnError_WhenIdentityFails()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId, Email = "admin@test.com" };
            var dto = new CreateUserDto(adminId, "newuser@test.com", "Password123!", "Password123!");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString()))
                .ReturnsAsync(adminUser);

            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator))
                .ReturnsAsync(true);

            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser))
                .ReturnsAsync(false);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Erro de teste." }));

            // Act
            var result = await _userService.CreateUserAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Erro ao criar usuário: Erro de teste.", result.Message);
        }

        [Fact]
        public async Task AddToRoleAsync_ShouldReturnSuccess_WhenAdminAddsValidRole()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var targetUser = new User { Id = targetUserId };
            var dto = new ManageUserRoleDto(adminId, targetUserId, "SomeRole");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.FindByIdAsync(targetUserId.ToString())).ReturnsAsync(targetUser);
            _roleManagerMock.Setup(x => x.RoleExistsAsync("SomeRole")).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.AddToRoleAsync(targetUser, "SomeRole")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.AddToRoleAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Papel adicionado com sucesso.", result.Message);
        }

        [Fact]
        public async Task RemoveFromRoleAsync_ShouldReturnSuccess_WhenAdminRemovesRole()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var targetUser = new User { Id = targetUserId };
            var dto = new ManageUserRoleDto(adminId, targetUserId, "SomeRole");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.FindByIdAsync(targetUserId.ToString())).ReturnsAsync(targetUser);
            _userManagerMock.Setup(x => x.RemoveFromRoleAsync(targetUser, "SomeRole")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.RemoveFromRoleAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Papel removido com sucesso.", result.Message);
        }

        [Fact]
        public async Task DisableUserAsync_ShouldReturnSuccess_WhenAdminDisablesUser()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var targetUser = new User { Id = targetUserId };
            var dto = new DisableUserDto(adminId, targetUserId);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.FindByIdAsync(targetUserId.ToString())).ReturnsAsync(targetUser);
            _userManagerMock.Setup(x => x.SetLockoutEnabledAsync(targetUser, true)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.SetLockoutEndDateAsync(targetUser, DateTimeOffset.MaxValue)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.DisableUserAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Usuário desabilitado com sucesso.", result.Message);
            _userManagerMock.Verify(x => x.SetLockoutEndDateAsync(targetUser, DateTimeOffset.MaxValue), Times.Once);
            _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(targetUser), Times.Once);
        }

        [Fact]
        public async Task EnableUserAsync_ShouldReturnSuccess_WhenAdminEnablesUser()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var targetUser = new User { Id = targetUserId };
            var dto = new EnableUserDto(adminId, targetUserId);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.FindByIdAsync(targetUserId.ToString())).ReturnsAsync(targetUser);
            _userManagerMock.Setup(x => x.SetLockoutEndDateAsync(targetUser, null)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.EnableUserAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Usuário habilitado com sucesso.", result.Message);
            _userManagerMock.Verify(x => x.SetLockoutEndDateAsync(targetUser, null), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnSuccess_WhenUserChangesOwnPassword()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "user@test.com" };
            var dto = new ChangePasswordDto(userId, "user@test.com", "OldPass123!", "NewPass123!", "NewPass123!");

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, SystemRoles.SystemAdministrator)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass123!")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ChangePasswordAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Senha alterada com sucesso.", result.Message);
            _userManagerMock.Verify(x => x.ChangePasswordAsync(user, "OldPass123!", "NewPass123!"), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnSuccess_WhenAdminResetsUserPassword()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var adminUser = new User { Id = adminId, Email = "admin@test.com" };
            var targetUser = new User { Id = targetUserId, Email = "user@test.com" };
            var dto = new ChangePasswordDto(adminId, "user@test.com", null, "NewPass123!", "NewPass123!");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.FindByEmailAsync("user@test.com")).ReturnsAsync(targetUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(targetUser)).ReturnsAsync("token");
            _userManagerMock.Setup(x => x.ResetPasswordAsync(targetUser, "token", "NewPass123!")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ChangePasswordAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Senha alterada com sucesso.", result.Message);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(targetUser, "token", "NewPass123!"), Times.Once);
        }
    }
}
