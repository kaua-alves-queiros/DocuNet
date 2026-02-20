using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Dtos.Device;
using DocuNet.Web.Enumerators;
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
    public class DeviceServiceTests : IDisposable
    {
        private readonly Mock<ILogger<DeviceService>> _loggerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ApplicationDatabaseContext _context;
        private readonly SqliteConnection _connection;
        private readonly DeviceService _deviceService;

        public DeviceServiceTests()
        {
            _loggerMock = new Mock<ILogger<DeviceService>>();
            
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

            _deviceService = new DeviceService(_context, _loggerMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task CreateDeviceAsync_ShouldReturnSuccess_WhenAdminCreatesDevice()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId, Email = "admin@test.com" };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org 1", IsActive = true };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new CreateDeviceDto(adminId, "Device 1", "192.168.1.1", EDeviceTypes.Router, org.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _deviceService.CreateDeviceAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Dispositivo criado com sucesso.", result.Message);
            Assert.NotEqual(Guid.Empty, result.Data);
            
            var deviceInDb = await _context.Devices.FirstOrDefaultAsync(d => d.Name == "Device 1");
            Assert.NotNull(deviceInDb);
            Assert.Equal(EDeviceTypes.Router, deviceInDb.Type);
            Assert.Equal("192.168.1.1", deviceInDb.IpAddress);
        }

        [Fact]
        public async Task CreateDeviceAsync_ShouldReturnSuccess_WhenIpAddressIsNull()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId, Email = "admin@test.com" };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org 1", IsActive = true };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new CreateDeviceDto(adminId, "Device No IP", null, EDeviceTypes.Router, org.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _deviceService.CreateDeviceAsync(dto);

            Assert.True(result.Success);
            
            var deviceInDb = await _context.Devices.FirstOrDefaultAsync(d => d.Name == "Device No IP");
            Assert.NotNull(deviceInDb);
            Assert.Null(deviceInDb.IpAddress);
        }

        [Fact]
        public async Task CreateDeviceAsync_ShouldReturnSuccess_WhenMemberCreatesDeviceInTheirOrg()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "user@test.com" };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org 1", IsActive = true };
            org.Users.Add(user);
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new CreateDeviceDto(userId, "Device User", "10.0.0.1", EDeviceTypes.Router, org.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, SystemRoles.SystemAdministrator)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);

            var result = await _deviceService.CreateDeviceAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Dispositivo criado com sucesso.", result.Message);
        }

        [Fact]
        public async Task CreateDeviceAsync_ShouldReturnError_WhenUserIsNotMemberOfOrg()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = "user@test.com" };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org 1", IsActive = true };
            // User is NOT added to Org
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new CreateDeviceDto(userId, "Device Intruder", "10.0.0.1", EDeviceTypes.Router, org.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, SystemRoles.SystemAdministrator)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);

            var result = await _deviceService.CreateDeviceAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Acesso negado: Você não tem permissão para adicionar dispositivos nesta organização.", result.Message);
        }

        [Fact]
        public async Task CreateDeviceAsync_ShouldReturnError_WhenOrgIsInactive()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Inactive Org", IsActive = false };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new CreateDeviceDto(adminId, "Device Inactive", "1.1.1.1", EDeviceTypes.Router, org.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _deviceService.CreateDeviceAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Não é possível adicionar dispositivos a uma organização inativa.", result.Message);
        }

        [Fact]
        public async Task GetDevicesAsync_ShouldReturnAllDevices_WhenAdmin()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            
            var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1" };
            var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org 2" };
            _context.Organizations.AddRange(org1, org2);

            _context.Devices.Add(new Device { Id = Guid.NewGuid(), Name = "D1", OrganizationId = org1.Id });
            _context.Devices.Add(new Device { Id = Guid.NewGuid(), Name = "D2", OrganizationId = org2.Id });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(adminUser)).ReturnsAsync(false);

            var result = await _deviceService.GetDevicesAsync(adminId);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
        }

        [Fact]
        public async Task GetDevicesAsync_ShouldReturnOnlyOrgDevices_WhenMember()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            
            var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1" };
            org1.Users.Add(user); // User is member of Org 1
            
            var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org 2" };
            // User NOT member of Org 2

            _context.Organizations.AddRange(org1, org2);

            _context.Devices.Add(new Device { Id = Guid.NewGuid(), Name = "D1", OrganizationId = org1.Id });
            _context.Devices.Add(new Device { Id = Guid.NewGuid(), Name = "D2", OrganizationId = org2.Id });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, SystemRoles.SystemAdministrator)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);

            var result = await _deviceService.GetDevicesAsync(userId);

            Assert.True(result.Success);
            Assert.Single(result.Data!); // Should only see D1
            Assert.Equal("D1", result.Data![0].Name);
        }

        [Fact]
        public async Task DeleteDeviceAsync_ShouldReturnSuccess_WhenAdminDeletes()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org" };
            var device = new Device { Id = Guid.NewGuid(), Name = "D1", OrganizationId = org.Id };
            
            _context.Organizations.Add(org);
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            var dto = new DeleteDeviceDto(adminId, device.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);

            var result = await _deviceService.DeleteDeviceAsync(dto);

            Assert.True(result.Success);
            Assert.Null(await _context.Devices.FindAsync(device.Id));
        }

        [Fact]
        public async Task DeleteDeviceAsync_ShouldReturnError_WhenUserDeletesDeviceFromOtherOrg()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1" }; // User NOT member
            var device = new Device { Id = Guid.NewGuid(), Name = "D1", OrganizationId = org1.Id };
            
            _context.Organizations.Add(org1);
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            var dto = new DeleteDeviceDto(userId, device.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, SystemRoles.SystemAdministrator)).ReturnsAsync(false);

            var result = await _deviceService.DeleteDeviceAsync(dto);

            Assert.False(result.Success);
            Assert.NotNull(await _context.Devices.FindAsync(device.Id)); // Needs to remain
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
