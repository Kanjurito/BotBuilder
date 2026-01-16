using System;
using System.Collections.Generic;

namespace DiscordBotBuilder.Models
{
    // Configuration principale du bot
    public class BotConfiguration
    {
        public string BotName { get; set; } = "Mon Bot Discord";
        public string EncryptedToken { get; set; } = "";
        public string BotPrefix { get; set; } = "!";

        // Fonctionnalités activées
        public bool WelcomeMessagesEnabled { get; set; } = false;
        public bool ModerationEnabled { get; set; } = false;
        public bool AutoRolesEnabled { get; set; } = false;
        public bool CustomCommandsEnabled { get; set; } = false;
        public bool AutoResponseEnabled { get; set; } = false;
        public bool LoggingEnabled { get; set; } = false;

        // Configurations spécifiques
        public WelcomeMessageConfig WelcomeConfig { get; set; } = new WelcomeMessageConfig();
        public ModerationConfig ModerationConfig { get; set; } = new ModerationConfig();
        public List<CustomCommand> CustomCommands { get; set; } = new List<CustomCommand>();
        public List<AutoResponse> AutoResponses { get; set; } = new List<AutoResponse>();
        public List<AutoRole> AutoRoles { get; set; } = new List<AutoRole>();
        public LoggingConfig LoggingConfig { get; set; } = new LoggingConfig();
    }

    // Configuration des messages de bienvenue
    public class WelcomeMessageConfig
    {
        public string WelcomeMessage { get; set; } = "Bienvenue {user} sur le serveur !";
        public string GoodbyeMessage { get; set; } = "Au revoir {user} !";
        public ulong WelcomeChannelId { get; set; } = 0;
        public bool SendPrivateMessage { get; set; } = false;
        public string PrivateWelcomeMessage { get; set; } = "Bienvenue sur notre serveur !";
    }

    // Configuration de la modération
    public class ModerationConfig
    {
        public bool AntiSpamEnabled { get; set; } = false;
        public int MaxMessagesPerMinute { get; set; } = 5;
        public bool AntiInviteEnabled { get; set; } = false;
        public bool BadWordsFilterEnabled { get; set; } = false;
        public List<string> BadWords { get; set; } = new List<string>();
        public ulong ModLogChannelId { get; set; } = 0;
    }

    // Commande personnalisée
    public class CustomCommand
    {
        public string Trigger { get; set; } = "";
        public string Response { get; set; } = "";
        public bool IsEmbed { get; set; } = false;
        public string EmbedTitle { get; set; } = "";
        public string EmbedColor { get; set; } = "#7289DA";
        public bool RequirePermission { get; set; } = false;
        public string RequiredRole { get; set; } = "";
    }

    // Réponse automatique
    public class AutoResponse
    {
        public string Trigger { get; set; } = "";
        public string Response { get; set; } = "";
        public bool ExactMatch { get; set; } = false;
        public bool IgnoreCase { get; set; } = true;
    }

    // Rôle automatique
    public class AutoRole
    {
        public ulong RoleId { get; set; } = 0;
        public string RoleName { get; set; } = "";
        public bool AssignOnJoin { get; set; } = true;
    }

    // Configuration des logs
    public class LoggingConfig
    {
        public ulong LogChannelId { get; set; } = 0;
        public bool LogMessageDeletes { get; set; } = true;
        public bool LogMessageEdits { get; set; } = true;
        public bool LogMemberJoins { get; set; } = true;
        public bool LogMemberLeaves { get; set; } = true;
        public bool LogBans { get; set; } = true;
        public bool LogRoleChanges { get; set; } = true;
    }
}