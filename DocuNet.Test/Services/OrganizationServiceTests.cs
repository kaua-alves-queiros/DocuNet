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
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId, Email = "admin@test.com" };
            var dto = new CreateOrganizationDto(adminId, "New Organization");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _organizationService.CreateOrganizationAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Organização criada com sucesso.", result.Message);
            Assert.NotEqual(Guid.Empty, result.Data);
            
            var orgInDb = await _context.Organizations.FirstOrDefaultAsync(o => o.Name == "New Organization");
            Assert.NotNull(orgInDb);
        }

        [Fact]
        public async Task CreateOrganizationAsync_ShouldReturnError_WhenUserIsNotAdmin()
        {
            var userId = Guid.NewGuid();
            var normalUser = new User { Id = userId, Email = "user@test.com" };
            var dto = new CreateOrganizationDto(userId, "Normal Org");

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(normalUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(normalUser, SystemRoles.SystemAdministrator)).ReturnsAsync(false);

            var result = await _organizationService.CreateOrganizationAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Acesso negado: Você não tem permissão para criar organizações.", result.Message);
        }

        [Fact]
        public async Task CreateOrganizationAsync_ShouldReturnError_WhenNameAlreadyExists()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var existingOrg = new Organization { Id = Guid.NewGuid(), Name = "Existing" };
            _context.Organizations.Add(existingOrg);
            await _context.SaveChangesAsync();

            var dto = new CreateOrganizationDto(adminId, "existing");
            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _organizationService.CreateOrganizationAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Já existe uma organização com este nome.", result.Message);
        }

        [Fact]
        public async Task RenameOrganizationAsync_ShouldReturnSuccess_WhenAdminRenamesToValidName()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Old Name" };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new RenameOrganizationDto(adminId, org.Id, "New Name");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _organizationService.RenameOrganizationAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Organização renomeada com sucesso.", result.Message);
            
            var updatedOrg = await _context.Organizations.FindAsync(org.Id);
            Assert.Equal("New Name", updatedOrg.Name);
        }

        [Fact]
        public async Task RenameOrganizationAsync_ShouldReturnError_WhenNewNameAlreadyExistsInOtherOrg()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1" };
            var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org 2" };
            _context.Organizations.AddRange(org1, org2);
            await _context.SaveChangesAsync();

            var dto = new RenameOrganizationDto(adminId, org1.Id, "Org 2");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _organizationService.RenameOrganizationAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Já existe outra organização com este nome.", result.Message);
        }

        [Fact]
        public async Task RenameOrganizationAsync_ShouldReturnError_WhenOrganizationDoesNotExist()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var dto = new RenameOrganizationDto(adminId, Guid.NewGuid(), "New Name");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _organizationService.RenameOrganizationAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Organização não encontrada.", result.Message);
        }

        [Fact]
        public async Task RenameOrganizationAsync_ShouldReturnError_WhenRequesterIsNotAdmin()
        {
            var userId = Guid.NewGuid();
            var normalUser = new User { Id = userId };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Some Org" };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new RenameOrganizationDto(userId, org.Id, "New Name");

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(normalUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(normalUser, SystemRoles.SystemAdministrator)).ReturnsAsync(false);

            var result = await _organizationService.RenameOrganizationAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Acesso negado: Você não tem permissão para gerenciar organizações.", result.Message);
        }

        [Fact]
        public async Task RenameOrganizationAsync_ShouldReturnError_WhenOrganizationIsInactive()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Inactive org", IsActive = false };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new RenameOrganizationDto(adminId, org.Id, "New Name");

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _organizationService.RenameOrganizationAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Acesso negado: Não é possível renomear uma organização desativada.", result.Message);
        }

        [Fact]
        public async Task ManageOrganizationStatusAsync_ShouldReturnSuccess_WhenAdminDeactivatesOrg()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org to Deactivate", IsActive = true };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new ManageOrganizationStatusDto(adminId, org.Id, false);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _organizationService.ManageOrganizationStatusAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Organização desabilitada com sucesso.", result.Message);
            
            var updatedOrg = await _context.Organizations.FindAsync(org.Id);
            Assert.False(updatedOrg.IsActive);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
