using AuthService.Models;
using System.Collections.Generic;

namespace AuthService.Services;

public static class UserStore
{
    public static List<User> Users = new()
    {
        new User { Username = "alice", Password = "password", Email = "alice@example.com", Name = "Alice Smith" }
    };

    public static User? Validate(string username, string password) =>
        Users.FirstOrDefault(u => u.Username == username && u.Password == password);
}