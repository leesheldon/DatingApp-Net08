using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberByIdAsync(int id, bool isCurrentUser);
    Task<MemberDto?> GetMemberByUsernameAsync(string username, bool isCurrentUser);
    Task<AppUser?> GetUserByPhotoId(int photoId);
}
