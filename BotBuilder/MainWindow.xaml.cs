using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DiscordBotBuilder.Models;
using DiscordBotBuilder.Services;

namespace DiscordBotBuilder
{
    public partial class MainWindow : Window
    {
        private BotConfiguration _currentConfig;
        private BotService _botService;
        private ConfigurationService _configService;

        public MainWindow()
        {
            InitializeComponent();
            _currentConfig = new BotConfiguration();
            _botService = new BotService();
            _configService = new ConfigurationService();

            // Abonnement aux événements du bot
            _botService.OnLogMessage += LogMessage;
            _botService.OnBotStarted += () => Dispatcher.Invoke(() => {
                btnStartBot.IsEnabled = false;
                btnStopBot.IsEnabled = true;
            });
            _botService.OnBotStopped += () => Dispatcher.Invoke(() => {
                btnStartBot.IsEnabled = true;
                btnStopBot.IsEnabled = false;
            });

            LogMessage("Application démarrée. Configurez votre bot et cliquez sur 'Démarrer'.");
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() => {
                txtConsole.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                txtConsole.ScrollToEnd();
            });
        }

        // === MENU FICHIER ===
        private void NewConfig_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Créer une nouvelle configuration ? Les données non sauvegardées seront perdues.",
                "Nouvelle configuration", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _currentConfig = new BotConfiguration();
                LoadConfigToUI();
                LogMessage("Nouvelle configuration créée.");
            }
        }

        private void LoadConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Fichiers JSON|*.json|Tous les fichiers|*.*",
                Title = "Charger une configuration"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _currentConfig = _configService.LoadConfiguration(dialog.FileName);
                    LoadConfigToUI();
                    LogMessage($"Configuration chargée depuis {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement : {ex.Message}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveUIToConfig();

            var dialog = new SaveFileDialog
            {
                Filter = "Fichiers JSON|*.json|Tous les fichiers|*.*",
                Title = "Sauvegarder la configuration",
                FileName = "bot_config.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _configService.SaveConfiguration(_currentConfig, dialog.FileName);
                    LogMessage($"Configuration sauvegardée dans {dialog.FileName}");
                    MessageBox.Show("Configuration sauvegardée avec succès !", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportPython_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtToken.Password))
            {
                MessageBox.Show("Veuillez entrer un token Discord avant d'exporter.", "Token manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveUIToConfig();

            var dialog = new SaveFileDialog
            {
                Filter = "Fichiers Python|*.py|Tous les fichiers|*.*",
                Title = "Exporter en Python",
                FileName = "bot.py"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _configService.ExportToPython(_currentConfig, txtToken.Password, dialog.FileName);
                    LogMessage($"Bot exporté en Python vers {dialog.FileName}");
                    MessageBox.Show($"Bot exporté avec succès !\n\nPour l'utiliser :\n1. Installez discord.py : pip install discord.py\n2. Lancez : python {dialog.FileName}",
                        "Export réussi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'export : {ex.Message}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (_botService.IsRunning)
            {
                if (MessageBox.Show("Le bot est en cours d'exécution. Voulez-vous vraiment quitter ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
                _botService.StopBotAsync().Wait();
            }
            Application.Current.Shutdown();
        }

        // === MENU AIDE ===
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://docs.discord.net/",
                UseShellExecute = true
            });
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Discord Bot Builder v1.0\n\nCréez et configurez votre bot Discord facilement !\n\nDéveloppé avec Discord.Net",
                "À propos", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // === CONTRÔLE DU BOT ===
        private async void StartBot_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtToken.Password))
            {
                MessageBox.Show("Veuillez entrer un token Discord valide.", "Token manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_configService.ValidateToken(txtToken.Password))
            {
                MessageBox.Show("Le format du token semble invalide. Vérifiez votre token.", "Token invalide",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveUIToConfig();

            try
            {
                await _botService.StartBotAsync(_currentConfig, txtToken.Password);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du démarrage du bot : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage($"ERREUR: {ex.Message}");
            }
        }

        private async void StopBot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _botService.StopBotAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'arrêt du bot : {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === COMMANDES PERSONNALISÉES ===
        private void AddCommand_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCommandTrigger.Text) || string.IsNullOrWhiteSpace(txtCommandResponse.Text))
            {
                MessageBox.Show("Veuillez remplir le déclencheur et la réponse.", "Champs manquants",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var command = new CustomCommand
            {
                Trigger = txtCommandTrigger.Text.Trim(),
                Response = txtCommandResponse.Text.Trim(),
                IsEmbed = chkIsEmbed.IsChecked == true
            };

            _currentConfig.CustomCommands.Add(command);
            lstCommands.Items.Add($"{command.Trigger} → {command.Response}");

            txtCommandTrigger.Clear();
            txtCommandResponse.Clear();
            chkIsEmbed.IsChecked = false;

            LogMessage($"Commande ajoutée: {command.Trigger}");
        }

        private void RemoveCommand_Click(object sender, RoutedEventArgs e)
        {
            if (lstCommands.SelectedIndex >= 0)
            {
                int index = lstCommands.SelectedIndex;
                _currentConfig.CustomCommands.RemoveAt(index);
                lstCommands.Items.RemoveAt(index);
                LogMessage("Commande supprimée.");
            }
        }

        // === RÉPONSES AUTOMATIQUES ===
        private void AddAutoResponse_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAutoTrigger.Text) || string.IsNullOrWhiteSpace(txtAutoResponse.Text))
            {
                MessageBox.Show("Veuillez remplir le déclencheur et la réponse.", "Champs manquants",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var autoResponse = new AutoResponse
            {
                Trigger = txtAutoTrigger.Text.Trim(),
                Response = txtAutoResponse.Text.Trim(),
                ExactMatch = chkExactMatch.IsChecked == true,
                IgnoreCase = chkIgnoreCase.IsChecked == true
            };

            _currentConfig.AutoResponses.Add(autoResponse);
            lstAutoResponses.Items.Add($"{autoResponse.Trigger} → {autoResponse.Response}");

            txtAutoTrigger.Clear();
            txtAutoResponse.Clear();

            LogMessage($"Réponse automatique ajoutée: {autoResponse.Trigger}");
        }

        private void RemoveAutoResponse_Click(object sender, RoutedEventArgs e)
        {
            if (lstAutoResponses.SelectedIndex >= 0)
            {
                int index = lstAutoResponses.SelectedIndex;
                _currentConfig.AutoResponses.RemoveAt(index);
                lstAutoResponses.Items.RemoveAt(index);
                LogMessage("Réponse automatique supprimée.");
            }
        }

        // === UTILITAIRES ===
        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            txtConsole.Clear();
        }

        private void SaveUIToConfig()
        {
            // Configuration générale
            _currentConfig.BotName = txtBotName.Text;
            _currentConfig.BotPrefix = txtPrefix.Text;

            if (!string.IsNullOrWhiteSpace(txtToken.Password))
            {
                _currentConfig.EncryptedToken = _configService.EncryptToken(txtToken.Password);
            }

            // Fonctionnalités
            _currentConfig.WelcomeMessagesEnabled = chkWelcome.IsChecked == true;
            _currentConfig.ModerationEnabled = chkModeration.IsChecked == true;
            _currentConfig.AutoRolesEnabled = chkAutoRoles.IsChecked == true;
            _currentConfig.CustomCommandsEnabled = chkCustomCommands.IsChecked == true;
            _currentConfig.AutoResponseEnabled = chkAutoResponse.IsChecked == true;
            _currentConfig.LoggingEnabled = chkLogging.IsChecked == true;

            // Messages de bienvenue
            if (ulong.TryParse(txtWelcomeChannelId.Text, out ulong welcomeChannelId))
                _currentConfig.WelcomeConfig.WelcomeChannelId = welcomeChannelId;

            _currentConfig.WelcomeConfig.WelcomeMessage = txtWelcomeMessage.Text;
            _currentConfig.WelcomeConfig.GoodbyeMessage = txtGoodbyeMessage.Text;
            _currentConfig.WelcomeConfig.SendPrivateMessage = chkPrivateMessage.IsChecked == true;
            _currentConfig.WelcomeConfig.PrivateWelcomeMessage = txtPrivateWelcome.Text;

            // Modération
            _currentConfig.ModerationConfig.AntiSpamEnabled = chkAntiSpam.IsChecked == true;
            if (int.TryParse(txtMaxMessages.Text, out int maxMessages))
                _currentConfig.ModerationConfig.MaxMessagesPerMinute = maxMessages;

            _currentConfig.ModerationConfig.AntiInviteEnabled = chkAntiInvite.IsChecked == true;
            _currentConfig.ModerationConfig.BadWordsFilterEnabled = chkBadWords.IsChecked == true;
            _currentConfig.ModerationConfig.BadWords = txtBadWords.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();

            if (ulong.TryParse(txtModLogChannel.Text, out ulong modLogChannelId))
                _currentConfig.ModerationConfig.ModLogChannelId = modLogChannelId;

            // Logging
            if (ulong.TryParse(txtLogChannel.Text, out ulong logChannelId))
                _currentConfig.LoggingConfig.LogChannelId = logChannelId;

            _currentConfig.LoggingConfig.LogMessageDeletes = chkLogDeletes.IsChecked == true;
            _currentConfig.LoggingConfig.LogMessageEdits = chkLogEdits.IsChecked == true;
            _currentConfig.LoggingConfig.LogMemberJoins = chkLogJoins.IsChecked == true;
            _currentConfig.LoggingConfig.LogMemberLeaves = chkLogLeaves.IsChecked == true;
            _currentConfig.LoggingConfig.LogBans = chkLogBans.IsChecked == true;
            _currentConfig.LoggingConfig.LogRoleChanges = chkLogRoles.IsChecked == true;
        }

        private void LoadConfigToUI()
        {
            // Configuration générale
            txtBotName.Text = _currentConfig.BotName;
            txtPrefix.Text = _currentConfig.BotPrefix;

            if (!string.IsNullOrWhiteSpace(_currentConfig.EncryptedToken))
            {
                try
                {
                    txtToken.Password = _configService.DecryptToken(_currentConfig.EncryptedToken);
                }
                catch { }
            }

            // Fonctionnalités
            chkWelcome.IsChecked = _currentConfig.WelcomeMessagesEnabled;
            chkModeration.IsChecked = _currentConfig.ModerationEnabled;
            chkAutoRoles.IsChecked = _currentConfig.AutoRolesEnabled;
            chkCustomCommands.IsChecked = _currentConfig.CustomCommandsEnabled;
            chkAutoResponse.IsChecked = _currentConfig.AutoResponseEnabled;
            chkLogging.IsChecked = _currentConfig.LoggingEnabled;

            // Messages de bienvenue
            txtWelcomeChannelId.Text = _currentConfig.WelcomeConfig.WelcomeChannelId.ToString();
            txtWelcomeMessage.Text = _currentConfig.WelcomeConfig.WelcomeMessage;
            txtGoodbyeMessage.Text = _currentConfig.WelcomeConfig.GoodbyeMessage;
            chkPrivateMessage.IsChecked = _currentConfig.WelcomeConfig.SendPrivateMessage;
            txtPrivateWelcome.Text = _currentConfig.WelcomeConfig.PrivateWelcomeMessage;

            // Modération
            chkAntiSpam.IsChecked = _currentConfig.ModerationConfig.AntiSpamEnabled;
            txtMaxMessages.Text = _currentConfig.ModerationConfig.MaxMessagesPerMinute.ToString();
            chkAntiInvite.IsChecked = _currentConfig.ModerationConfig.AntiInviteEnabled;
            chkBadWords.IsChecked = _currentConfig.ModerationConfig.BadWordsFilterEnabled;
            txtBadWords.Text = string.Join(Environment.NewLine, _currentConfig.ModerationConfig.BadWords);
            txtModLogChannel.Text = _currentConfig.ModerationConfig.ModLogChannelId.ToString();

            // Logging
            txtLogChannel.Text = _currentConfig.LoggingConfig.LogChannelId.ToString();
            chkLogDeletes.IsChecked = _currentConfig.LoggingConfig.LogMessageDeletes;
            chkLogEdits.IsChecked = _currentConfig.LoggingConfig.LogMessageEdits;
            chkLogJoins.IsChecked = _currentConfig.LoggingConfig.LogMemberJoins;
            chkLogLeaves.IsChecked = _currentConfig.LoggingConfig.LogMemberLeaves;
            chkLogBans.IsChecked = _currentConfig.LoggingConfig.LogBans;
            chkLogRoles.IsChecked = _currentConfig.LoggingConfig.LogRoleChanges;

            // Listes
            lstCommands.Items.Clear();
            foreach (var cmd in _currentConfig.CustomCommands)
            {
                lstCommands.Items.Add($"{cmd.Trigger} → {cmd.Response}");
            }

            lstAutoResponses.Items.Clear();
            foreach (var resp in _currentConfig.AutoResponses)
            {
                lstAutoResponses.Items.Add($"{resp.Trigger} → {resp.Response}");
            }
        }
    }
}