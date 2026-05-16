using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using System.Collections.Generic;

namespace FlowDesk.UnitTests
{
    public class UserHandlerTests
    {
        [Fact]
        public async Task CreateAsync_CallsRepositoryAdd()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
            var handler = new UserHandler(mockRepo.Object);

            var deptId = Guid.NewGuid();
            var result = await handler.CreateAsync("Alice", "Smith", "alice@example.com", "hash", UserRole.User, deptId);

            Assert.Equal("alice@example.com", result.Email);
            mockRepo.Verify(r => r.ExistsByEmailAsync("alice@example.com"), Times.Once);
            mockRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "alice@example.com" && u.FirstName == "Alice")), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_EmailExists_Throws()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(true);
            var handler = new UserHandler(mockRepo.Object);

            await Assert.ThrowsAsync<Exception>(() => handler.CreateAsync("Bob","Jones","bob@example.com","hash",UserRole.User, Guid.NewGuid()));
            mockRepo.Verify(r => r.ExistsByEmailAsync("bob@example.com"), Times.Once);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_UserNotFound_Throws()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);
            var handler = new UserHandler(mockRepo.Object);

            await Assert.ThrowsAsync<Exception>(() => handler.UpdateAsync(Guid.NewGuid(), UserRole.Admin, Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAndCallsUpdate()
        {
            var existing = new User("Old","User","old@example.com","hash", UserRole.User, Guid.NewGuid());
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetAsync(existing.Id)).ReturnsAsync(existing);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
            var handler = new UserHandler(mockRepo.Object);

            var newDept = Guid.NewGuid();
            var updated = await handler.UpdateAsync(existing.Id, UserRole.Support, newDept);

            mockRepo.Verify(r => r.GetAsync(existing.Id), Times.Once);
            mockRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Role == UserRole.Support && u.DepartmentId == newDept)), Times.Once);
            Assert.Equal(UserRole.Support, updated.Role);
            Assert.Equal(newDept, updated.DepartmentId);
        }
    }
}
