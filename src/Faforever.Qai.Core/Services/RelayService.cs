using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Faforever.Qai.Core.Database;
using Faforever.Qai.Core.Structures.Configurations;
using Faforever.Qai.Core.Structures.Webhooks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Faforever.Qai.Core.Services
{
	// Test Webhook URL
	// https://discord.com/api/webhooks/773214715328725012/Kj0P_CMUooloZTyfIOP37QD1GLuXp_LVRS3ax2FqxT5mP2dOvLoP_xMZiJ8L6Sf0jYgq

	public class RelayService : IDisposable
	{
		private readonly IServiceProvider _services;
		private readonly ILogger _logger;
		private bool initalized;
		private bool disposedValue;

		private ConcurrentDictionary<string, HashSet<string>> IRCtoWebhookRelations { get; set; }

		private HttpClient Http { get; set; }

		public RelayService(IServiceProvider services, ILogger<RelayService> logger)
		{
			this._services = services;
			this.initalized = false;
			this._logger = logger;

			Http = new HttpClient();
		}

		private bool Initalize()
		{
			try
			{
				IRCtoWebhookRelations = new ConcurrentDictionary<string, HashSet<string>>();

				var _database = _services.GetRequiredService<QAIDatabaseModel>();

				var relays = _database.RelayConfigurations
					.AsNoTracking()
					.ToList();

				foreach (var r in relays)
				{
					foreach (var hook in r.Webhooks)
					{
						AddToWebhookDict(hook.Key, hook.Value.WebhookUrl);
					}
				}

				this.initalized = true;
			}
			catch (Exception ex)
			{
				this._logger.LogError(ex, "Relay Service failed to initalize.");
				this.initalized = false;
			}

			return this.initalized;
		}

		private void AddToWebhookDict(string key, string value)
		{
			if (IRCtoWebhookRelations.ContainsKey(key))
			{
				IRCtoWebhookRelations[key].Add(value);
			}
			else
			{
				IRCtoWebhookRelations[key] = new HashSet<string>() { value };
			}
		}

		public async Task<bool> AddRelayAsync(ulong discordGuild, DiscordWebhookData hook, string ircChannel)
		{
			try
			{
				if (!this.initalized)
					if (!Initalize())
						throw new Exception("Failed to Initalize the RelayService.");

				var _database = _services.GetRequiredService<QAIDatabaseModel>();

				AddToWebhookDict(ircChannel, hook.WebhookUrl);
				// This method should not be passed values that dont have a configuration value created for them.
				var cfg = await _database.FindAsync<RelayConfiguration>(discordGuild);
				if (cfg is null)
					throw new Exception("Failed to get valid relay configuration.");

				_database.Update(cfg);

				cfg.Webhooks[ircChannel] = hook;
				cfg.DiscordToIRCLinks[hook.Id] = ircChannel;

				await _database.SaveChangesAsync();

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to add new Relay.");
				return false;
			}
		}

		public async Task<DiscordWebhookData?> RemoveRelayAsync(ulong discordGuild, ulong webhookId)
		{
			try
			{
				if (!this.initalized)
					if (!Initalize())
						throw new Exception("Failed to Initalize the RelayService.");


				var _database = _services.GetRequiredService<QAIDatabaseModel>();

				// This method should not be passed values that dont have a configuration vlaue created for them.
				var cfg = await _database.FindAsync<RelayConfiguration>(discordGuild);
				if (cfg is null)
					throw new Exception("Failed to get valid relay configuration.");
				DiscordWebhookData? hook = null;
				if (cfg.DiscordToIRCLinks.TryRemove(webhookId, out string? ircChannel))
				{
					// At least one thing changed, so tell the database to save changes.
					_database.Update(cfg);

					if (cfg.Webhooks.TryRemove(ircChannel, out hook))
					{
						IRCtoWebhookRelations[ircChannel]?.Remove(hook.WebhookUrl);
					}
				}

				await _database.SaveChangesAsync();

				return hook;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to remove Relay.");
				return null;
			}
		}

		public async Task SendFromDiscordAsync(ulong discordChannel, string author, string message)
		{
			if (!this.initalized)
				if (!Initalize())
					throw new Exception("Failed to Initalize the RelayService.");
		}

		public async Task SendFromIRCAsync(string ircChannel, string author, string message)
		{
			if (!this.initalized)
				if (!Initalize())
					throw new Exception("Failed to Initalize the RelayService.");

			if (IRCtoWebhookRelations.TryGetValue(ircChannel, out var hooks))
			{
				foreach (var h in hooks)
				{
					try
					{
						var data = new DiscordWebhookContent()
						{
							Content = message,
							Username = author,
						};

						var json = JsonConvert.SerializeObject(data, settings: new JsonSerializerSettings
						{
							NullValueHandling = NullValueHandling.Ignore
						});

						var request = new HttpRequestMessage()
						{
							RequestUri = new Uri(h),
							Method = HttpMethod.Post
						};

						request.Content = new StringContent(json, Encoding.UTF8, "application/json");

						// The response should be a 204, so we dont care about reading it.
						_ = await Http.SendAsync(request);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Send from IRC errored.");
						continue;
					}
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				IRCtoWebhookRelations = null;
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~RelayService()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
