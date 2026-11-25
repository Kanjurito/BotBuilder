# 🤖 Discord Bot Maker

Ever wanted your own Discord bot but didn't want to write a single line of code? That's exactly why this app exists. Built in **C#**, Discord Bot Maker lets you create, configure, and host your own bot from a simple desktop interface.

---

## What it does

* **Host it yourself**: Start your bot and keep it online straight from the app. No extra setup needed.
* **Export to Python**: Want to move your bot to a server later? Generate the `.py` source code in one click.
* **Save your setup**: Store your settings in a file and pick up right where you left off.
* **Easy to navigate**: Each feature lives in its own tab, so you only deal with one thing at a time.

---

## Getting started / installation

### 1. Get your Discord token
Your **token** is your bot's secret key, so keep it to yourself and never share it with anyone!

1. Head over to the [Discord Developer Portal](https://discord.com/developers/applications).
2. Click **"New Application"** and give it a name.
3. Open the **"Bot"** tab, click **"Reset Token"**, and copy the token that appears.
4. **Don't skip this one:** scroll down and turn on the **"Privileged Gateway Intents"** (Presence, Server Members, and Message Content). Without them, your bot won't be able to read messages or see who's in your server.

### 2. Pick a command prefix
The prefix is the symbol people type to get your bot's attention.
> **For example:** if you choose `!`, your help command will be `!help`.

### 3. Find a channel ID
The Welcome and Logs features need to know which channel to post in. Here's how to grab its ID:

1. Open your **Discord Settings** > **Advanced** and turn on **Developer Mode**.
2. Right-click the channel you want in your server.
3. Click **"Copy Channel ID"**.

---

## A quick tour of the tabs

| Tab | What it's for |
| :--- | :--- |
| **Configuration** | Set your bot's name, prefix, and token. |
| **Welcome** | Write a custom greeting for new members and choose where it's sent. |
| **Moderation** | Turn on anti-spam, block invite links (anti-raid), and filter insults. |
| **Commands** | Build your own custom commands. |
| **Auto-Reply** | Have your bot answer automatically when it spots a certain word. |
| **Logs** | Keep an eye on server activity in a dedicated channel (needs a channel ID). |

---

## 🐍 Moving to Python

Planning to run your bot on a VPS or a Linux server? You can export your project as a `.py` file.

### What you'll need
Make sure [Python](https://www.python.org/) is installed, then add the **discord.py** library from your terminal:

```bash
pip install discord.py
```

## Keep It Simple Stupid
