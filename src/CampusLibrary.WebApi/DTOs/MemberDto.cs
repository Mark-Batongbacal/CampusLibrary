using CampusLibrary.Data;

namespace CampusLibrary.WebApi.DTOs;

public record MemberDto(int Id, string FullName, string Email)
{
    public static MemberDto From(Member member) => new(member.Id, member.FullName, member.Email);
}