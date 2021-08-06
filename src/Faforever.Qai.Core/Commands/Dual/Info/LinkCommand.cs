using System.Threading.Tasks;
using Faforever.Qai.Core.Services;
using Qmmands;

namespace Faforever.Qai.Core.Commands.Dual.Info
{
	public class LinkCommand : DualCommandModule
	{
		private readonly IUrlService _urlService;

		public LinkCommand(IUrlService urlService)
		{
			this._urlService = urlService;
		}

		[Command("link")]
		[Description("Search for a specifik link")]
		public async Task LinkCommandAsync([Remainder] string search)
		{
			var result = _urlService.FindUrl(search);

			if (result is not null)
				await Context.ReplyAsync($"{result.Title} {result.Url}");
			else
				await Context.ReplyAsync("Link not found");
		}

		[Command("wiki")]
		[Description("Search for a specifik wiki link")]
		public async Task WikiCommandAsync([Remainder] string search)
		{
			var result = _urlService.FindWikiUrl(search);

			if (result is not null)
				await Context.ReplyAsync($"{result.Title} {result.Url}");
			else
				await Context.ReplyAsync("Wiki link not found");
		}
	}
}
