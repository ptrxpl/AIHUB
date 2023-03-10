using AIHUB_Server.Common.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AIHUB_Server.Controllers
{
    [ApiController]
    [Route("[controller]")] // "api/[controller]"
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender _mediator = null!;

        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    }
}



