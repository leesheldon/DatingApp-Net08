using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper,
     IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberDto>> GetUser(int id)
    {
        var user = await unitOfWork.UserRepository.GetMemberByIdAsync(id);

        if (user == null) return NotFound();

        return user;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await unitOfWork.UserRepository.GetMemberByUsernameAsync(username);

        if (user == null) return NotFound();

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("Could not find user!");

        mapper.Map(memberUpdateDto, user);

        if (await unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to update the user!");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("Cannot get user in adding photo!");

        var result = await photoService.AddPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0) photo.IsMain = true;

        user.Photos.Add(photo);

        if (await unitOfWork.Complete()) 
            return CreatedAtAction(nameof(GetUser), 
                new {username = user.UserName}, mapper.Map<PhotoDto>(photo));

        return BadRequest("Problem in adding photo in saving to database!");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("Could not find user to set main photo!");

        var photo = user.Photos.SingleOrDefault(x => x.Id == photoId);

        if (photo == null) return BadRequest("Could not find photo to set main photo!");

        if (photo.IsMain) return BadRequest("Cannot use this photo as Main photo!");

        var currentMainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMainPhoto != null) currentMainPhoto.IsMain = false;

        photo.IsMain = true;

        if (await unitOfWork.Complete()) return NoContent();

        return BadRequest("Problem in setting main photo!");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("Could not find user to delete photo!");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null) return BadRequest("Could not find photo to delete!");
        if (photo.IsMain) return BadRequest("This main photo cannot be deleted!");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);
        if (await unitOfWork.Complete()) return Ok();

        return BadRequest("Problem in deleting photo!");
    }
}
