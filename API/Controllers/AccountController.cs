using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, 
    ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<SignedInUserDto>> Register(RegisterUserDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken!");
        
        using var hmac = new HMACSHA512();

        var newUser = mapper.Map<AppUser>(registerDto);

        newUser.UserName = registerDto.Username.Trim().ToLower();
        newUser.PwdHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        newUser.PwdSalt = hmac.Key;

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        return new SignedInUserDto
        {
            Username = newUser.UserName,
            Token = tokenService.CreateToken(newUser),
            KnownAs = newUser.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<SignedInUserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(
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
            Token = tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(x => x.UserName.Trim().ToLower() == username.Trim().ToLower());
    }
}
