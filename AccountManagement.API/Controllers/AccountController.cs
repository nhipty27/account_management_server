using AccountManagement.Application.Account.Commands;
using AccountManagement.Application.Account.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _accessor;

        public AccountController(IMediator mediator, IHttpContextAccessor accessor) 
        {
            _accessor = accessor;
            _mediator = mediator;
        }

        [HttpPost]
        [Route("accounts")]
        public async Task<ActionResult> getAccounts([FromBody] getAccountsRequest rp)
        {
            var data = await _mediator.Send(new GetAccount.Command
            {
                Data = rp
            });
            return Ok(data);
        }

        [HttpPost]
        [Route("update-profile")]
        public async Task<ActionResult> updateProfile([FromBody] UserRequest user)
        {
            var data = await _mediator.Send(new UpdateUser.Command
            {
                Data = user
            });
            return Ok(data);
        }

        [HttpPost]
        [Route("update-account")]
        [Authorize(Roles = "ADMIN, MANAGER")]
        public async Task<ActionResult> updateAccountExRole([FromBody] UserRequest user)
        {
            HttpContext context = _accessor.HttpContext;
            context.Request.Headers.TryGetValue("Authorization", out var value);
            var token = value.ToString().Replace("bearer ", "");
            var data = await _mediator.Send(new UpdateUser.Command
            {
                Data = user,
                Token = token
            });
            return Ok(data);
        }

        [HttpPost]
        [Route("update-role")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> updateAccountRole([FromBody] decenRoleRequest datarq)
        {
            var data = await _mediator.Send(new decenRole.Command
            {
                Data = datarq
            });
            return Ok(data);
        }

        [HttpDelete]
        [Route("delete-account")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> deleteAccountRole([FromBody] int id)
        {
            var data = await _mediator.Send(new DeleteAccount.Command
            {
                id = id
            });
            return Ok(data);
        }

        [HttpPost]
        [Route("add-new-account")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> addAccount([FromBody] UserRequest user)
        {
            HttpContext context = _accessor.HttpContext;
            context.Request.Headers.TryGetValue("Authorization", out var value);
            var token = value.ToString().Replace("bearer ", "");
            var data = await _mediator.Send(new AddNewUser.Command
            {
                Data = user,
                Token = token
            });
            return Ok(data);
        }
    }
}
