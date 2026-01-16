using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBotBuilder.Models;
using System.Collections.Generic;

namespace DiscordBotBuilder.Services
{
    public class BotService
    {
        private DiscordSocketClient _client;
        private BotConfiguration _config;
        private bool _isRunning = false;

        // Dictionnaire pour tracker le spam
        private Dictionary<ulong, List<DateTime>> _userMessageTimes = new Dictionary<ulong, List<DateTime>>();

        public event Action<string> OnLogMessage;
        public event Action OnBotStarted;
        public event Action OnBotStopped;

        public bool IsRunning => _isRunning;

        public BotService()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
            };
            _client = new DiscordSocketClient(config);

            _client.Log += LogAsync;
            _client.Ready += OnReadyAsync;
        }

        private Task LogAsync(LogMessage log)
        {
            OnLogMessage?.Invoke($"[{log.Severity}] {log.Message}");
            return Task.CompletedTask;
        }

        private Task OnReadyAsync()
        {
            OnLogMessage?.Invoke($"Bot connecté en tant que {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");
            return Task.CompletedTask;
        }

        public async Task StartBotAsync(BotConfiguration config, string decryptedToken)
        {
            if (_isRunning)
            {
                OnLogMessage?.Invoke("Le bot est déjà en cours d'exécution.");
                return;
            }

            _config = config;

            // Abonnement aux événements selon la configuration
            if (_config.WelcomeMessagesEnabled)
            {
                _client.UserJoined += OnUserJoinedAsync;
                _client.UserLeft += OnUserLeftAsync;
            }

            if (_config.CustomCommandsEnabled || _config.AutoResponseEnabled || _config.ModerationEnabled)
            {
                _client.MessageReceived += OnMessageReceivedAsync;
            }

            if (_config.LoggingEnabled)
            {
                _client.MessageDeleted += OnMessageDeletedAsync;
                _client.MessageUpdated += OnMessageUpdatedAsync;
                _client.UserJoined += OnUserJoinedLogAsync;
                _client.UserLeft += OnUserLeftLogAsync;
            }

            if (_config.AutoRolesEnabled)
            {
                _client.UserJoined += OnUserJoinedAutoRoleAsync;
            }

            try
            {
                await _client.LoginAsync(TokenType.Bot, decryptedToken);
                await _client.StartAsync();
                _isRunning = true;
                OnBotStarted?.Invoke();
                OnLogMessage?.Invoke("Bot démarré avec succès !");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Erreur lors du démarrage : {ex.Message}");
                throw;
            }
        }

        public async Task StopBotAsync()
        {
            if (!_isRunning)
            {
                OnLogMessage?.Invoke("Le bot n'est pas en cours d'exécution.");
                return;
            }

            // Désabonnement de tous les événements
            _client.UserJoined -= OnUserJoinedAsync;
            _client.UserLeft -= OnUserLeftAsync;
            _client.MessageReceived -= OnMessageReceivedAsync;
            _client.MessageDeleted -= OnMessageDeletedAsync;
            _client.MessageUpdated -= OnMessageUpdatedAsync;
            _client.UserJoined -= OnUserJoinedLogAsync;
            _client.UserLeft -= OnUserLeftLogAsync;
            _client.UserJoined -= OnUserJoinedAutoRoleAsync;

            await _client.StopAsync();
            await _client.LogoutAsync();
            _isRunning = false;
            OnBotStopped?.Invoke();
            OnLogMessage?.Invoke("Bot arrêté.");
        }

        // === ÉVÉNEMENTS - MESSAGES DE BIENVENUE ===
        private async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            if (!_config.WelcomeMessagesEnabled) return;

            var message = _config.WelcomeConfig.WelcomeMessage.Replace("{user}", user.Mention);

            if (_config.WelcomeConfig.WelcomeChannelId != 0)
            {
                var channel = user.Guild.GetTextChannel(_config.WelcomeConfig.WelcomeChannelId);
                if (channel != null)
                {
                    await channel.SendMessageAsync(message);
                }
            }

            if (_config.WelcomeConfig.SendPrivateMessage)
            {
                try
                {
                    await user.SendMessageAsync(_config.WelcomeConfig.PrivateWelcomeMessage);
                }
                catch
                {
                    OnLogMessage?.Invoke($"Impossible d'envoyer un MP à {user.Username}");
                }
            }
        }

        private async Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            if (!_config.WelcomeMessagesEnabled || _config.WelcomeConfig.WelcomeChannelId == 0) return;

            var message = _config.WelcomeConfig.GoodbyeMessage.Replace("{user}", user.Username);
            var channel = guild.GetTextChannel(_config.WelcomeConfig.WelcomeChannelId);

            if (channel != null)
            {
                await channel.SendMessageAsync(message);
            }
        }

        // === ÉVÉNEMENTS - MESSAGES ===
        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            var userMessage = message as SocketUserMessage;
            if (userMessage == null) return;

            // Modération anti-spam
            if (_config.ModerationEnabled && _config.ModerationConfig.AntiSpamEnabled)
            {
                if (await CheckSpamAsync(userMessage)) return;
            }

            // Modération anti-invite
            if (_config.ModerationEnabled && _config.ModerationConfig.AntiInviteEnabled)
            {
                if (await CheckInviteAsync(userMessage)) return;
            }

            // Filtre de mots interdits
            if (_config.ModerationEnabled && _config.ModerationConfig.BadWordsFilterEnabled)
            {
                if (await CheckBadWordsAsync(userMessage)) return;
            }

            // Réponses automatiques
            if (_config.AutoResponseEnabled)
            {
                await CheckAutoResponsesAsync(userMessage);
            }

            // Commandes personnalisées
            if (_config.CustomCommandsEnabled)
            {
                await CheckCustomCommandsAsync(userMessage);
            }
        }

        private async Task<bool> CheckSpamAsync(SocketUserMessage message)
        {
            var userId = message.Author.Id;
            var now = DateTime.UtcNow;

            if (!_userMessageTimes.ContainsKey(userId))
            {
                _userMessageTimes[userId] = new List<DateTime>();
            }

            var times = _userMessageTimes[userId];
            times.RemoveAll(t => (now - t).TotalMinutes > 1);
            times.Add(now);

            if (times.Count > _config.ModerationConfig.MaxMessagesPerMinute)
            {
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync($"{message.Author.Mention}, ralentis un peu ! (anti-spam)");
                OnLogMessage?.Invoke($"Spam détecté de {message.Author.Username}");
                return true;
            }

            return false;
        }

        private async Task<bool> CheckInviteAsync(SocketUserMessage message)
        {
            if (message.Content.Contains("discord.gg/") || message.Content.Contains("discord.com/invite/"))
            {
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync($"{message.Author.Mention}, les invitations Discord ne sont pas autorisées ici !");
                return true;
            }
            return false;
        }

        private async Task<bool> CheckBadWordsAsync(SocketUserMessage message)
        {
            foreach (var badWord in _config.ModerationConfig.BadWords)
            {
                if (message.Content.ToLower().Contains(badWord.ToLower()))
                {
                    await message.DeleteAsync();
                    await message.Channel.SendMessageAsync($"{message.Author.Mention}, attention à ton langage !");
                    return true;
                }
            }
            return false;
        }

        private async Task CheckAutoResponsesAsync(SocketUserMessage message)
        {
            foreach (var autoResponse in _config.AutoResponses)
            {
                bool matches = autoResponse.ExactMatch
                    ? message.Content.Equals(autoResponse.Trigger, autoResponse.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
                    : message.Content.Contains(autoResponse.Trigger, autoResponse.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

                if (matches)
                {
                    await message.Channel.SendMessageAsync(autoResponse.Response);
                    break;
                }
            }
        }

        private async Task CheckCustomCommandsAsync(SocketUserMessage message)
        {
            foreach (var cmd in _config.CustomCommands)
            {
                if (message.Content.StartsWith(_config.BotPrefix + cmd.Trigger))
                {
                    if (cmd.IsEmbed)
                    {
                        var embed = new EmbedBuilder()
                            .WithTitle(cmd.EmbedTitle)
                            .WithDescription(cmd.Response)
                            .WithColor(ParseColor(cmd.EmbedColor))
                            .Build();

                        await message.Channel.SendMessageAsync(embed: embed);
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync(cmd.Response);
                    }
                    break;
                }
            }
        }

        // === ÉVÉNEMENTS - LOGGING ===
        private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
        {
            if (!_config.LoggingEnabled || !_config.LoggingConfig.LogMessageDeletes) return;

            var logChannel = _client.GetChannel(_config.LoggingConfig.LogChannelId) as ITextChannel;
            if (logChannel != null && cachedMessage.HasValue)
            {
                var msg = cachedMessage.Value;
                await logChannel.SendMessageAsync($"📝 Message supprimé de {msg.Author.Username} : {msg.Content}");
            }
        }

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            if (!_config.LoggingEnabled || !_config.LoggingConfig.LogMessageEdits) return;

            var logChannel = _client.GetChannel(_config.LoggingConfig.LogChannelId) as ITextChannel;
            if (logChannel != null && before.HasValue)
            {
                await logChannel.SendMessageAsync($"✏️ Message édité de {after.Author.Username}\nAvant : {before.Value.Content}\nAprès : {after.Content}");
            }
        }

        private async Task OnUserJoinedLogAsync(SocketGuildUser user)
        {
            if (!_config.LoggingEnabled || !_config.LoggingConfig.LogMemberJoins) return;

            var logChannel = _client.GetChannel(_config.LoggingConfig.LogChannelId) as ITextChannel;
            if (logChannel != null)
            {
                await logChannel.SendMessageAsync($"➕ {user.Username} a rejoint le serveur");
            }
        }

        private async Task OnUserLeftLogAsync(SocketGuild guild, SocketUser user)
        {
            if (!_config.LoggingEnabled || !_config.LoggingConfig.LogMemberLeaves) return;

            var logChannel = _client.GetChannel(_config.LoggingConfig.LogChannelId) as ITextChannel;
            if (logChannel != null)
            {
                await logChannel.SendMessageAsync($"➖ {user.Username} a quitté le serveur");
            }
        }

        // === ÉVÉNEMENTS - RÔLES AUTOMATIQUES ===
        private async Task OnUserJoinedAutoRoleAsync(SocketGuildUser user)
        {
            if (!_config.AutoRolesEnabled) return;

            foreach (var autoRole in _config.AutoRoles.Where(r => r.AssignOnJoin))
            {
                var role = user.Guild.GetRole(autoRole.RoleId);
                if (role != null)
                {
                    await user.AddRoleAsync(role);
                    OnLogMessage?.Invoke($"Rôle {role.Name} attribué à {user.Username}");
                }
            }
        }

        // === UTILITAIRES ===
        private Color ParseColor(string hexColor)
        {
            try
            {
                hexColor = hexColor.TrimStart('#');
                return new Color(
                    Convert.ToByte(hexColor.Substring(0, 2), 16),
                    Convert.ToByte(hexColor.Substring(2, 2), 16),
                    Convert.ToByte(hexColor.Substring(4, 2), 16)
                );
            }
            catch
            {
                return Color.Blue;
            }
        }
    }
}