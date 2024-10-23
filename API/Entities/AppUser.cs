using System;

namespace API.Entities;

public class AppUser
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required byte[] PwdHash { get; set; }
    public required byte[] PwdSalt { get; set; }
}
