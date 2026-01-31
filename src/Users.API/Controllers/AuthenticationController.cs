using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Users.API.Dtos.Requests;
using Users.API.Dtos.Responses;
using Users.API.Services;

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
}