using DiscordBotBuilder.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace DiscordBotBuilder.Services
{
    public class ConfigurationService
    {
        private const string EncryptionKey = "maclesupersecret"; // À changer !

        // Sauvegarde la configuration dans un fichier JSON
        public void SaveConfiguration(BotConfiguration config, string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la sauvegarde : {ex.Message}");
            }
        }

        // Charge une configuration depuis un fichier JSON
        public BotConfiguration LoadConfiguration(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("Le fichier de configuration n'existe pas.");
                }

                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<BotConfiguration>(json) ?? new BotConfiguration();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du chargement : {ex.Message}");
            }
        }

        // Chiffre le token Discord
        public string EncryptToken(string token)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                    aes.IV = new byte[16]; // IV simple pour cet exemple

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        var tokenBytes = Encoding.UTF8.GetBytes(token);
                        cs.Write(tokenBytes, 0, tokenBytes.Length);
                        cs.FlushFinalBlock();
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur de chiffrement : {ex.Message}");
            }
        }

        // Déchiffre le token Discord
        public string DecryptToken(string encryptedToken)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                    aes.IV = new byte[16]; // Même IV que pour le chiffrement

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(Convert.FromBase64String(encryptedToken)))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur de déchiffrement : {ex.Message}");
            }
        }

        // Export vers Python (discord.py)
        public void ExportToPython(BotConfiguration config, string decryptedToken, string filePath)
        {
            var sb = new StringBuilder();

            sb.AppendLine("import discord");
            sb.AppendLine("from discord.ext import commands");
            sb.AppendLine();
            sb.AppendLine($"# Bot généré par Discord Bot Builder");
            sb.AppendLine($"# Nom du bot: {config.BotName}");
            sb.AppendLine();
            sb.AppendLine("intents = discord.Intents.default()");
            sb.AppendLine("intents.message_content = True");
            sb.AppendLine("intents.members = True");
            sb.AppendLine();
            sb.AppendLine($"bot = commands.Bot(command_prefix='{config.BotPrefix}', intents=intents)");
            sb.AppendLine();

            // Event ready
            sb.AppendLine("@bot.event");
            sb.AppendLine("async def on_ready():");
            sb.AppendLine($"    print(f'Bot connecté en tant que {{bot.user.name}}')");
            sb.AppendLine();

            // Messages de bienvenue
            if (config.WelcomeMessagesEnabled)
            {
                sb.AppendLine("@bot.event");
                sb.AppendLine("async def on_member_join(member):");
                sb.AppendLine($"    channel = bot.get_channel({config.WelcomeConfig.WelcomeChannelId})");
                sb.AppendLine("    if channel:");
                sb.AppendLine($"        await channel.send('{config.WelcomeConfig.WelcomeMessage}'.replace('{{user}}', member.mention))");

                if (config.WelcomeConfig.SendPrivateMessage)
                {
                    sb.AppendLine("    try:");
                    sb.AppendLine($"        await member.send('{config.WelcomeConfig.PrivateWelcomeMessage}')");
                    sb.AppendLine("    except:");
                    sb.AppendLine("        pass");

                    sb.AppendLine();

                    sb.AppendLine("@bot.event");
                    sb.AppendLine("async def on_member_remove(member):");
                    sb.AppendLine($"    channel = bot.get_channel({config.WelcomeConfig.WelcomeChannelId})");
                    sb.AppendLine("    if channel:");
                    sb.AppendLine($"        await channel.send('{config.WelcomeConfig.GoodbyeMessage}'.replace('{{user}}', member.name))");
                    sb.AppendLine();
                }

                // Commandes personnalisées
                if (config.CustomCommandsEnabled && config.CustomCommands.Count > 0)
                {
                    foreach (var cmd in config.CustomCommands)
                    {
                        sb.AppendLine($"@bot.command(name='{cmd.Trigger}')");
                        sb.AppendLine($"async def {cmd.Trigger.Replace("-", "_")}(ctx):");

                        if (cmd.IsEmbed)
                        {
                            sb.AppendLine($"    embed = discord.Embed(title='{cmd.EmbedTitle}', description='{cmd.Response}', color=discord.Color.from_str('{cmd.EmbedColor}'))");
                            sb.AppendLine("    await ctx.send(embed=embed)");
                        }
                        else
                        {
                            sb.AppendLine($"    await ctx.send('{cmd.Response}')");
                        }
                        sb.AppendLine();
                    }
                }

                // Modération anti-spam
                if (config.ModerationEnabled && config.ModerationConfig.AntiSpamEnabled)
                {
                    sb.AppendLine("user_messages = {}");
                    sb.AppendLine("from datetime import datetime, timedelta");
                    sb.AppendLine();
                    sb.AppendLine("@bot.event");
                    sb.AppendLine("async def on_message(message):");
                    sb.AppendLine("    if message.author.bot:");
                    sb.AppendLine("        return");
                    sb.AppendLine();
                    sb.AppendLine("    # Anti-spam");
                    sb.AppendLine("    user_id = message.author.id");
                    sb.AppendLine("    now = datetime.now()");
                    sb.AppendLine("    if user_id not in user_messages:");
                    sb.AppendLine("        user_messages[user_id] = []");
                    sb.AppendLine();
                    sb.AppendLine("    user_messages[user_id] = [t for t in user_messages[user_id] if now - t < timedelta(minutes=1)]");
                    sb.AppendLine("    user_messages[user_id].append(now)");
                    sb.AppendLine();
                    sb.AppendLine($"    if len(user_messages[user_id]) > {config.ModerationConfig.MaxMessagesPerMinute}:");
                    sb.AppendLine("        await message.delete()");
                    sb.AppendLine("        await message.channel.send(f'{message.author.mention}, ralentis un peu ! (anti-spam)')");
                    sb.AppendLine("        return");
                    sb.AppendLine();
                    sb.AppendLine("    await bot.process_commands(message)");
                    sb.AppendLine();
                }

                // Token et démarrage
                sb.AppendLine($"bot.run('{decryptedToken}')");

                File.WriteAllText(filePath, sb.ToString());
            }
        }

        // Valide un token Discord
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Un token Discord valide a généralement cette forme
            // Il y a 3 parties séparées par des points
            var parts = token.Split('.');
            return parts.Length == 3;
        }
    }
}