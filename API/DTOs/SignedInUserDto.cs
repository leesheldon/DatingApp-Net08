namespace API.DTOs;

public class SignedInUserDto
{
    public required string Username { get; set; }
    public required string Token { get; set; }
}
