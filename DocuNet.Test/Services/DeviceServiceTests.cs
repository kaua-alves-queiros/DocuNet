using System.ComponentModel.DataAnnotations;
using DocuNet.Web.Constants;
using DocuNet.Web.Data;
using DocuNet.Web.Dtos.Device;
using DocuNet.Web.Dtos.Connection;
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

        #region Device Tests

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
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var dto = new CreateDeviceDto(userId, "Device Intruder", "10.0.0.1", EDeviceTypes.Router, org.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, SystemRoles.SystemAdministrator)).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);

            var result = await _deviceService.CreateDeviceAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region Connection Tests

        [Fact]
        public async Task CreateConnectionAsync_ShouldReturnSuccess_WhenAdminCreatesValidConnection()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org 1", IsActive = true };
            var devA = new Device { Id = Guid.NewGuid(), Name = "DevA", OrganizationId = org.Id };
            var devB = new Device { Id = Guid.NewGuid(), Name = "DevB", OrganizationId = org.Id };
            _context.Organizations.Add(org);
            _context.Devices.AddRange(devA, devB);
            await _context.SaveChangesAsync();

            var dto = new CreateConnectionDto(adminId, devA.Id, devB.Id, EConnectionTypes.Ethernet, "Gi0/1", "Gi0/1", "1 Gbps", org.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);

            var result = await _deviceService.CreateConnectionAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Conexão criada com sucesso.", result.Message);
            
            var connInDb = await _context.Connections.FirstOrDefaultAsync(c => c.Id == result.Data);
            Assert.NotNull(connInDb);
            Assert.Equal("Gi0/1", connInDb.SourceInterface);
            Assert.Equal("1 Gbps", connInDb.Speed);
        }

        [Fact]
        public async Task CreateConnectionAsync_ShouldReturnError_WhenDevicesInDifferentOrgs()
        {
            var adminId = Guid.NewGuid();
            var adminUser = new User { Id = adminId };
            var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1", IsActive = true };
            var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org 2", IsActive = true };
            var devA = new Device { Id = Guid.NewGuid(), Name = "DevA", OrganizationId = org1.Id };
            var devB = new Device { Id = Guid.NewGuid(), Name = "DevB", OrganizationId = org2.Id };
            _context.Organizations.AddRange(org1, org2);
            _context.Devices.AddRange(devA, devB);
            await _context.SaveChangesAsync();

            var dto = new CreateConnectionDto(adminId, devA.Id, devB.Id, EConnectionTypes.Fiber, null, null, null, org1.Id);

            _userManagerMock.Setup(x => x.FindByIdAsync(adminId.ToString())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.IsInRoleAsync(adminUser, SystemRoles.SystemAdministrator)).ReturnsAsync(true);

            var result = await _deviceService.CreateConnectionAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Os dispositivos devem pertencer à mesma organização.", result.Message);
        }

        [Fact]
        public async Task CreateConnectionAsync_ShouldReturnError_WhenDeviceConnectsToSelf()
        {
            var adminId = Guid.NewGuid();
            var devId = Guid.NewGuid();
            var dto = new CreateConnectionDto(adminId, devId, devId, EConnectionTypes.Ethernet, null, null, null, Guid.NewGuid());

            var result = await _deviceService.CreateConnectionAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Não é possível conectar um dispositivo a ele mesmo.", result.Message);
        }

        [Fact]
        public async Task GetConnectionsAsync_ShouldFilterByOrganization_WhenMember()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var org1 = new Organization { Id = Guid.NewGuid(), Name = "Org 1" };
            org1.Users.Add(user);
            var org2 = new Organization { Id = Guid.NewGuid(), Name = "Org 2" };
            
            var dev1 = new Device { Id = Guid.NewGuid(), Name = "D1", OrganizationId = org1.Id };
            var dev2 = new Device { Id = Guid.NewGuid(), Name = "D2", OrganizationId = org1.Id };
            var dev3 = new Device { Id = Guid.NewGuid(), Name = "D3", OrganizationId = org2.Id };
            var dev4 = new Device { Id = Guid.NewGuid(), Name = "D4", OrganizationId = org2.Id };

            var conn1 = new Connection { Id = Guid.NewGuid(), SourceDeviceId = dev1.Id, DestinationDeviceId = dev2.Id, OrganizationId = org1.Id };
            var conn2 = new Connection { Id = Guid.NewGuid(), SourceDeviceId = dev3.Id, DestinationDeviceId = dev4.Id, OrganizationId = org2.Id };

            _context.Organizations.AddRange(org1, org2);
            _context.Devices.AddRange(dev1, dev2, dev3, dev4);
            _context.Connections.AddRange(conn1, conn2);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, SystemRoles.SystemAdministrator)).ReturnsAsync(false);

            var result = await _deviceService.GetConnectionsAsync(userId);

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal(conn1.Id, result.Data![0].Id);
        }

        [Fact]
        public async Task DeleteConnectionAsync_ShouldReturnError_WhenUserNotFromOrg()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var org = new Organization { Id = Guid.NewGuid(), Name = "Org" }; // User NOT member
            var devA = new Device { Id = Guid.NewGuid(), Name="DevA", OrganizationId = org.Id };
            var devB = new Device { Id = Guid.NewGuid(), Name="DevB", OrganizationId = org.Id };
            var conn = new Connection { Id = Guid.NewGuid(), SourceDeviceId = devA.Id, DestinationDeviceId = devB.Id, OrganizationId = org.Id };

            _context.Organizations.Add(org);
            _context.Devices.AddRange(devA, devB);
            _context.Connections.Add(conn);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, SystemRoles.SystemAdministrator)).ReturnsAsync(false);

            var result = await _deviceService.DeleteConnectionAsync(userId, conn.Id);

            Assert.False(result.Success);
            Assert.Equal("Acesso negado: Você não tem permissão para remover esta conexão.", result.Message);
        }

        #endregion

        #region Helper Methods

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

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        #endregion
    }
}
