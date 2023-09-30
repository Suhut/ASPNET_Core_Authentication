using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api;

public class HomeController : ControllerBase
{
    [HttpPost("/ctr/login")]
    public IActionResult Login([FromForm] LoginForm form)
    {
        HttpContext.SignInAsync(new ClaimsPrincipal(new[]
            {
                    new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()),
                    })
                }
        ));

        return Ok();
    }
}

