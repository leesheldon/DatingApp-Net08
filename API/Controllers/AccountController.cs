using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, 
    ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<SignedInUserDto>> Register(RegisterUserDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken!");

        var newUser = mapper.Map<AppUser>(registerDto);

        newUser.UserName = registerDto.Username.Trim().ToLower();
        
        var result = await userManager.CreateAsync(newUser, registerDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        return new SignedInUserDto
        {
            Username = newUser.UserName,
            Token = await tokenService.CreateToken(newUser),
            KnownAs = newUser.KnownAs,
            Gender = newUser.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<SignedInUserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(
                x => x.NormalizedUserName == loginDto.Username.ToUpper());
        
        if (user == null || user.UserName == null) return Unauthorized("Invalid username!");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized();

        return new SignedInUserDto
        {
            Username = user.UserName,
            Token = await tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
    }
}
