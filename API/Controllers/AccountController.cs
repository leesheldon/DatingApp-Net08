using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<SignedInUserDto>> Register(RegisterUserDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken!");
        return Ok();

        // using var hmac = new HMACSHA512();

        // var newUser = new AppUser
        // {
        //     UserName = registerDto.Username.Trim().ToLower(),
        //     PwdHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        //     PwdSalt = hmac.Key
        // };

        // context.Users.Add(newUser);
        // await context.SaveChangesAsync();

        // return new SignedInUserDto
        // {
        //     Username = newUser.UserName,
        //     Token = tokenService.CreateToken(newUser)
        // };
    }

    [HttpPost("login")]
    public async Task<ActionResult<SignedInUserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(
            x => x.UserName == loginDto.Username.ToLower());
        
        if (user == null) return Unauthorized("Invalid username!");

        using var hmac = new HMACSHA512(user.PwdSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PwdHash[i]) return Unauthorized("Invalid password!");
        }

        return new SignedInUserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(x => x.UserName.Trim().ToLower() == username.Trim().ToLower());
    }
}
