# 🤖 Discord Bot Maker (C#)

Une application desktop intuitive conçue en **C#** pour créer, configurer et auto-héberger votre propre bot Discord sans aucune connaissance en programmation.

---

## ✨ Fonctionnalités

* **Auto-hébergement** : Lancez et gardez votre bot en ligne directement depuis l'application.
* **Exportation Python** : Générez instantanément le code source en `.py` pour une migration facile.
* **Gestion de Configuration** : Sauvegardez vos réglages dans un fichier pour les recharger plus tard.
* **Interface Intuitive** : Une gestion par onglets pour configurer chaque module séparément.

---

## 🛠️ Guide de Configuration

### 1. Obtenir son Token Discord
Le **Token** est la clé secrète de votre bot. Ne le partagez jamais !
1. Rendez-vous sur le [Discord Developer Portal](https://discord.com/developers/applications).
2. Cliquez sur **"New Application"** et donnez-lui un nom.
3. Allez dans l'onglet **"Bot"**, cliquez sur **"Reset Token"** pour l'afficher et copiez-le.
4. **Important** : Faites défiler vers le bas et activez les **"Privileged Gateway Intents"** (Presence, Server Members, et Message Content) pour que le bot puisse lire les messages et voir les membres.

### 2. Le Préfixe des commandes
C'est le symbole utilisé pour interpeller le bot. 
> **Exemple :** Si vous choisissez `!`, la commande d'aide sera `!help`.

### 3. Récupérer l'ID d'un salon (Channel ID)
Pour les fonctions de Bienvenue ou de Logs, l'application demande un ID numérique :
1. Allez dans vos **Paramètres Discord** > **Avancés** > Activez le **Mode Développeur**.
2. Faites un clic droit sur le salon souhaité dans votre serveur.
3. Cliquez sur **"Copier l'identifiant"**.

---

## 📂 Présentation des Onglets

| Onglet | Description |
| :--- | :--- |
| **Configuration** | Définissez le nom, le préfixe et le token du bot. |
| **Bienvenue** | Configurez un message d'accueil personnalisé et le salon de destination. |
| **Modération** | Activez l'anti-spam, bloquez les invitations (anti-raid) et filtrez les insultes. |
| **Commandes** | Créez vos propres commandes personnalisées. |
| **Réponse Auto** | Le bot répond automatiquement lorsqu'il détecte un mot spécifique. |
| **Logs** | Suivez les événements du serveur dans un salon dédié (nécessite un ID de salon). |

---

## 🐍 Exportation vers Python

Si vous souhaitez héberger votre bot sur un VPS ou un serveur Linux, vous pouvez exporter le code en `.py`.

### Installation des prérequis
Assurez-vous d'avoir [Python](https://www.python.org/) installé, puis installez la bibliothèque **discord.py** via votre terminal :

```bash
pip install discord.py
```
## 💾 Sauvegarde et Export
L'application vous permet d'exporter toute la configuration de votre bot. Cela crée un fichier que vous pouvez réimporter plus tard pour retrouver tous vos réglages, commandes et filtres de modération en un clic.
