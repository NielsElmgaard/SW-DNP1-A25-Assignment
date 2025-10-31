using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class UserInMemoryRepository : IUserRepository
{
    private List<User> users;

    public UserInMemoryRepository()
    {
        users = new List<User>();
        AddDummyData();
    }

    private void AddDummyData()
    {
        users.AddRange(new List<User>
        {
            new User(1, "username1", "password1"),
            new User(2, "username2", "password2"),
            new User(3, "username3", "password3")
        });
    }

    public Task<User> AddAsync(User user)
    {
        user.Id = users.Any()
            ? users.Max(p => p.Id) +
              1
            : 1;
        users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        User? existingUser =
            users.SingleOrDefault(p =>
                p.Id == user
                    .Id);
        if (existingUser is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{user.Id}' not found");
        }

        users.Remove(existingUser);
        users.Add(user);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        User? userToRemove = users.SingleOrDefault(p => p.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        users.Remove(userToRemove);
        return Task.CompletedTask;
    }

    public Task<User> GetSingleAsync(int id)
    {
        User? user = users.SingleOrDefault(p => p.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User with ID '{id}' not found");
        }

        return Task.FromResult(user);
    }

    public IQueryable<User> GetMany()
    {
        return users.AsQueryable();
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        User? user = users.SingleOrDefault(p => p.Username.Equals(username));
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User with username '{username}' not found");
        }

        return await Task.FromResult(user);
    }
}