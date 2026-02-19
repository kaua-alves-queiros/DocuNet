using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Dtos.Organization;
using DocuNet.Web.Models;
using DocuNet.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocuNet.Test.Services
{
    public class OrganizationServiceTests : IDisposable
    {
        private readonly Mock<ILogger<OrganizationService>> _loggerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ApplicationDatabaseContext _context;
        private readonly SqliteConnection _connection;
        private readonly OrganizationService _organizationService;

        public OrganizationServiceTests()
        {
            _loggerMock = new Mock<ILogger<OrganizationService>>();
            
            // Setup In-Memory SQLite
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
            
            var options = new DbContextOptionsBuilder<ApplicationDatabaseContext>()
                .UseSqlite(_connection)
                .Options;
            
            _context = new ApplicationDatabaseContext(options);
            _context.Database.EnsureCreated();

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _organizationService = new OrganizationService(_context, _loggerMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task CreateOrganizationAsync_ShouldReturnSuccess_WhenAdminCreatesValidOrganization()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId, Email = "admin@test.com" };
            var dto = new CreateOrganizationDto(adminId, "New Organization");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            // Act
            var result = await _organizationService.CreateOrganizationAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Organização criada com sucesso.", result.Message);
            Assert.NotEqual(Guid.Empty, result.Data);
            
            var orgInDb = await _context.Organizations.FirstOrDefaultAsync(o => o.Name == "New Organization");
            Assert.NotNull(orgInDb);
        }

        [Fact]
        public async Task CreateOrganizationAsync_ShouldReturnError_WhenUserIsNotAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var normalUser = new User { Id = userId, Email = "user@test.com" };
            var dto = new CreateOrganizationDto(userId, "Normal Org");

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(normalUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(normalUser, SystemRoles.SystemAdministrator)).ReturnsAsync(false);

            // Act
            var result = await _organizationService.CreateOrganizationAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Acesso negado: Você não tem permissão para criar organizações.", result.Message);
        }

        [Fact]
        public async Task CreateOrganizationAsync_ShouldReturnError_WhenNameAlreadyExists()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var existingOrg = new Organization { Id = Guid.NewGuid(), Name = "Existing" };
            _context.Organizations.Add(existingOrg);
            await _context.SaveChangesAsync();

            var dto = new CreateOrganizationDto(adminId, "existing"); // Case insensitive check

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            // Act
            var result = await _organizationService.CreateOrganizationAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Já existe uma organização com este nome.", result.Message);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
