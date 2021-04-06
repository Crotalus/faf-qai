using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using DSharpPlus;

using Faforever.Qai.Core.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Faforever.Qai.Api
{
	[Route("api/link")]
	[ApiController]
	public class DiscordFafLinkApiController : ControllerBase
	{
		private readonly AccountLinkService _linkService;

		public DiscordFafLinkApiController(AccountLinkService linkService)
		{
			_linkService = linkService;
		}

		[HttpGet("token/{token}")]
		public IActionResult GetTokenEndpoint(string token)
		{
			var t = HttpUtility.HtmlDecode(token);
			switch(_linkService.GetLinkStatus(t))
			{
				case AccountLinkService.LinkStatus.Ready:

					_ = Task.Run(async () => await _linkService.FinalizeLink(token));

					return Ok("Accounts linked successfully.");

				case AccountLinkService.LinkStatus.Waiting:
					Response.Cookies.Append("token", token);
					return Redirect("/api/link/login");

				case AccountLinkService.LinkStatus.Invalid:
					return BadRequest("Token is invalid or not found.");

				default:
					return BadRequest("Token is invalid, not found, or an unkown error occoured.");
			}
		}

		[HttpGet("login")]
		[Authorize(AuthenticationSchemes = "DISCORD")]
		public IActionResult GetLoginEndpoint()
		{
			return Redirect("/api/link/auth");

		}

		[HttpGet("auth")]
		[Authorize(AuthenticationSchemes = "FAF")]
		public IActionResult GetAuthEndpoint()
		{
			if(Request.Cookies.TryGetValue("token", out var token))
			{
				return Redirect($"/api/link/token/{token}");
			}

			return BadRequest("No token found.");
		}

		[HttpGet("denied")]
		public IActionResult GetDeniedEndpoint()
		{
			return BadRequest("User denied account linking.");
		}

		[HttpGet("error/{error}")]
		public IActionResult GetErroredEndpoint(string error)
		{
			return BadRequest(error);
		}
	}
}
