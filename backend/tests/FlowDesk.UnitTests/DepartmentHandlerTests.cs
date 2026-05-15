using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using Xunit;

namespace FlowDesk.UnitTests;

public class DepartmentHandlerTests
{
    private class FakeRepository : IDepartmentRepository
    {
        public Department? LastAdded;
        public Department? Stored;
        public bool AddCalled = false;
        public bool UpdateCalled = false;
        public bool DeleteCalled = false;
        public Guid? DeletedId = null;
        public List<Department> All = new();

        public Task AddAsync(Department department)
        {
            AddCalled = true;
            LastAdded = department;
            All.Add(department);
            Stored = department;
            return Task.CompletedTask;
        }

        public Task<List<Department>> GetAllAsync()
        {
            return Task.FromResult(All);
        }

        public Task<Department> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Stored != null && Stored.Id == id ? Stored : null!);
        }

        public Task UpdateAsync(Department department)
        {
            UpdateCalled = true;
            if (Stored != null && Stored.Id == department.Id)
            {
                Stored = department;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            DeleteCalled = true;
            DeletedId = id;
            All.RemoveAll(d => d.Id == id);
            if (Stored != null && Stored.Id == id) Stored = null;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task AddAsync_ShouldCreateDepartmentAndCallRepository()
    {
        var repo = new FakeRepository();
        var handler = new DepartmentHandler(repo);

        var dept = await handler.AddAsync("HR", "Human Resources");

        Assert.NotNull(dept);
        Assert.Equal("HR", dept.Name);
        Assert.Equal("Human Resources", dept.Description);
        Assert.True(repo.AddCalled);
        Assert.Equal(dept.Id, repo.LastAdded?.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_ShouldUpdateAndCallUpdate()
    {
        var repo = new FakeRepository();
        var original = new Department("Dev", "Development");
        repo.Stored = original;
        repo.All.Add(original);

        var handler = new DepartmentHandler(repo);
        var updated = await handler.UpdateAsync(original.Id, "DevOps", "Development & Ops");

        Assert.NotNull(updated);
        Assert.Equal("DevOps", updated!.Name);
        Assert.Equal("Development & Ops", updated.Description);
        Assert.True(repo.UpdateCalled);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldReturnNull()
    {
        var repo = new FakeRepository();
        var handler = new DepartmentHandler(repo);

        var result = await handler.UpdateAsync(Guid.NewGuid(), "X", "Y");

        Assert.Null(result);
        Assert.False(repo.UpdateCalled);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ShouldReturnTrueAndCallDelete()
    {
        var repo = new FakeRepository();
        var dept = new Department("Sales", "Sales Dept");
        repo.Stored = dept;
        repo.All.Add(dept);

        var handler = new DepartmentHandler(repo);
        var result = await handler.DeleteAsync(dept.Id);

        Assert.True(result);
        Assert.True(repo.DeleteCalled);
        Assert.Equal(dept.Id, repo.DeletedId);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ShouldReturnFalse()
    {
        var repo = new FakeRepository();
        var handler = new DepartmentHandler(repo);

        var result = await handler.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
        Assert.False(repo.DeleteCalled);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnListFromRepository()
    {
        var repo = new FakeRepository();
        repo.All.Add(new Department("A", "a"));
        repo.All.Add(new Department("B", "b"));

        var handler = new DepartmentHandler(repo);
        var list = await handler.GetAllAsync();

        Assert.Equal(2, list.Count);
    }
}