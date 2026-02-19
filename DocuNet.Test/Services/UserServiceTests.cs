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
            Assert.Equal("Você não tem permissão para criar usuários.", result.Message);
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

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Erro de teste." }));

            // Act
            var result = await _userService.CreateUserAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Erro ao criar usuário: Erro de teste.", result.Message);
        }
    }
}
