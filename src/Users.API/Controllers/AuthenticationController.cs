using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Users.API.Dtos.Requests;
using Users.API.Dtos.Responses;
using Users.API.Services;
using Users.API.Services.Dtos;

namespace Users.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthenticationController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserRequestDto requestDto)
    {
        var userid = await userService.CreateUserAsync(requestDto);
        return Ok(userid);
    }
    [HttpPost("login")]
    public async Task<ActionResult<LoginUserResponse>> LoginUser([FromBody] LoginUserRequestDto requestDto)
    {
        var loginUserResponse = await userService.LoginUserAsync(requestDto);
        return Ok(loginUserResponse);
    }
    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginUserResponse>> RefreshToken([FromBody] RefreshTokenRequestDto requestDto)
    {
        var loginUserResponse = await userService.RefreshUserAsnc(requestDto);
        return Ok(loginUserResponse);
    }
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDetailsDto?>> GetUserDetails([FromRoute]string userId)
    {
        var userDetails = await userService.GetUserAsync(userId);
        return Ok(userDetails);
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult> UpdateUserDetails([FromBody] UpdateUserDto requestDto,CancellationToken cancellationToken)
    {
        // get user id from token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized("User Id not found in the token.");
        await userService.UpdateUserAsync(requestDto,Guid.Parse(userId),cancellationToken);
        return NoContent();
    }
}