using AccountManagement.Application.Account.Commands;
using AccountManagement.Application.Account.Dtos;
using AccountManagement.Application.Auth.Commands;
using AccountManagement.Application.Auth.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.API.Controllers
{

    [Route("api/auth")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator, IHttpContextAccessor accessor)
        {
            _mediator = mediator;
            _accessor = accessor;
        }

        [HttpGet]
        public async Task<ActionResult> getUserInfoByToken()
        {
            HttpContext context = _accessor.HttpContext;
            context.Request.Headers.TryGetValue("Authorization", out var value);
            var token = value.ToString().Replace("bearer ", "");
            var data = await _mediator.Send(new GetUserInfo.Command
            {
                token = token
            });  
            return Ok(data);
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register([FromBody] UserRequest Data)
        {
            var data = await _mediator.Send(new AddNewUser.Command{
                Data= Data,
            });
            return Ok(data);
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(Login.Command command)
        {
            var data = await _mediator.Send(command);
            return Ok(data);
        }

        [HttpPost]
        [Route("logout")]
        public async Task<ActionResult> Logout()
        {
            HttpContext context = _accessor.HttpContext;
            context.Request.Headers.TryGetValue("Authorization", out var value);
            var token = value.ToString().Replace("bearer ","");
            var data = await _mediator.Send(new Logout.Command
            {
                token = token,
            });
            return Ok(data);
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] changePasswordRequest Data )
        {
            HttpContext context = _accessor.HttpContext;
            context.Request.Headers.TryGetValue("Authorization", out var value);
            var token = value.ToString().Replace("bearer ", "");
            var data = await _mediator.Send(new ChangePassword.Command{
                Data = Data,
                curToken = token
            });
            return Ok(data);
        }

        [HttpPost]
        [Route("test")]
        [AllowAnonymous]
        public async Task<ActionResult> Test([FromBody] DateTest Data)
        {
            var data = await _mediator.Send(new Test.Command
            {
                FromDate = Data.FromDate,
                ToDate = Data.ToDate,
            }); ;
            return Ok(data);
        }
    }
}

public class DateTest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}