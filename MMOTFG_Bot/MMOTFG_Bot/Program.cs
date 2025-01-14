﻿using MMOTFG_Bot.Commands;
using MMOTFG_Bot.Navigation;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace MMOTFG_Bot
{
	class Program
	{
		private static bool processedNewEvents = false;
		private static long launchTime;
		static cHelp helpCommand = new cHelp();
		static cCreateCharacter createCommand = new cCreateCharacter();

		public static Communicator Communicator;

		public static List<ICommand> commandList = new List<ICommand> { new cDeleteCharacter(), new cDebug(), new cCreateCharacter(), new cUseItem(), new cAddItem(), new cThrowItem(),
            new cShowInventory(), new cEquipItem(), new cUnequipItem(), new cInfo(), new cStatus(), new cFight(),
			new cNavigate(), new cDirections(), new cInspectRoom(), new cAttack(), helpCommand};

		static async Task Main(string[] args)
		{
			//en args se puede poner [nombre del archivo de input] [nombre del archivo de output]

			/*Si estamos depurando en visual studio, tenemos que cambiar la ruta relativa en PC
			* para que funcione igual que en el contenedor de Docker*/
			if (Environment.GetEnvironmentVariable("PLATFORM_PC") != null || args[0] == "-t")
			{
				Console.WriteLine("Estamos en PC");
				Directory.SetCurrentDirectory("./../../..");
			}
			else
			{
				Console.WriteLine("Estamos en Docker");
			}

			launchTime = DateTime.UtcNow.Ticks;

			InventorySystem.Init();
			Map.Init("assets/map.json", "assets/directionSynonyms.json");
			JSONSystem.Init("assets/enemies.json", "assets/player.json", "assets/attacks.json", "assets/items.json");
			BattleSystem.Init();
			DatabaseManager.Init();
			RNG.Init();
			foreach (ICommand c in commandList)
			{
				c.SetKeywords();
				c.SetDescription();
			}
			createCommand.SetKeywords();
			helpCommand.setCommandList(new List<ICommand>(commandList));

			if (args.Length > 0 && args[0] == "-t")
            {
				//File Communicator
				Communicator = new FileCommunicator();

				FileCommunicator f = Communicator as FileCommunicator;

				if (f != null)
				{
					await OnFileRead(f);
				}
			}
            else
            {
				string token = "";
				try
				{
					token = System.IO.File.ReadAllText("assets/private/token.txt");
				}
				catch (FileNotFoundException)
				{
					Console.WriteLine("No se ha encontrado el archivo token.txt en la carpeta assets.");
					Environment.Exit(-1);
				}

				var botClient = new TelegramBotClient(token);
				var me = await botClient.GetMeAsync();

				Communicator = new TelegramCommunicator();
				(Communicator as TelegramCommunicator).Init(botClient);
				Console.WriteLine("Hello World! I am user " + me.Id + " and my name is " + me.FirstName);

				using var cts = new CancellationTokenSource();

				botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), null, cts.Token);
				BotCommand command = new BotCommand();

				Console.WriteLine($"Start listening for @{me.Username}");

				Thread.Sleep(Timeout.Infinite);

				cts.Cancel();
			}
		}

		static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			var handler = update.Type switch
			{
				// UpdateType.Unknown:
				// UpdateType.ChannelPost:
				// UpdateType.EditedChannelPost:
				// UpdateType.ShippingQuery:
				// UpdateType.PreCheckoutQuery:
				// UpdateType.Poll:
				UpdateType.MyChatMember => onChatMemeberUpdateReceived(update),
				UpdateType.Message => BotOnMessageReceived(botClient, update.Message),
				UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage),
				//UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery),
				UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery),
				//UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult),
				//_ => UnknownUpdateHandlerAsync(botClient, update)
			};

			try
			{
				await handler;
			}
			catch (Exception exception)
			{
				await HandleErrorAsync(botClient, exception, cancellationToken);
			}
		}

		static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery query)
		{
			InlineQueryResult[] results = {
					//// displayed result
					//new InlineQueryResultArticle(
					//	id: "0",
					//	title: "dumbify your message!",
					//	inputMessageContent: new InputTextMessageContent(
					//		DumbifyText(query.Query)
					//	)
					//)
				};

			await botClient.AnswerInlineQueryAsync(
				inlineQueryId: query.Id,
				results: results,
				isPersonal: true,
				cacheTime: 0);
		}

		static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
		{
			string chatId = message.Chat.Id.ToString();
			string senderName = message.From.FirstName;

			if (!processedNewEvents) //Don't process messages with a date prior to the program's launch date
			{
				if (message.Date.Ticks < launchTime) return;
				else processedNewEvents = true;
			}

			Console.WriteLine("Received message: " + message.Text + " from " + senderName);

			if (message.Type == MessageType.Text) //Si le mandas una imagen explota ahora mismo
			{
				await ReceiveMessage(message.Text, chatId);
			}
		}

		static private async Task ReceiveMessage(string message, string chatId = "1")
        {
			List<string> subStrings = ProcessMessage(message);
			string command = subStrings[0];
			string[] args = new string[subStrings.Count - 1];
			subStrings.CopyTo(1, args, 0, args.Length);

			bool recognizedCommand = false;

			if (!await canUseCommand(chatId, command))
			{
				await Communicator.SendText(chatId, "You need a character to play, use /create to create a new character");
				return;
			}
			foreach (ICommand c in commandList)
			{
				if (c.ContainsKeyWord(command))
				{
					recognizedCommand = true;
					if (await c.TryExecute(command, chatId, args)) break;

					else await Communicator.SendText(chatId, "Incorrect use of that command.\nUse /help_" + command + " for further information.");
				}
			}
			if (!recognizedCommand) await Communicator.SendText(chatId, "Unrecognized command.\n Try /help if you don't know what to use");
		}

		static private async Task OnFileRead(FileCommunicator f)
        {
			var paths = Directory.GetDirectories(f.BasePath);
			foreach(string path in paths){
				f.Init(path + "/Input.txt", path + "/Output.txt", path + "/Seed.txt");
				//if seed is needed
				try
				{
					StreamReader seedFile = new StreamReader(f.SeedPath);
					int seed = Convert.ToInt32(seedFile.ReadLine());
					RNG.Init(seed);
				}
                catch
                {

                }

				StreamReader inputFile = new StreamReader(f.InputPath);
				string line;
				while ((line = inputFile.ReadLine()) != null)
				{
					await ReceiveMessage(line);
				}
				await f.Flush();	
			}
			Environment.Exit(0);
		}

		/// <summary>
		/// Processes the message recieved from the user by filtering out certain chars and splitting it into words
		/// </summary>
		private static List<string> ProcessMessage(string message)
		{
			List<string> processedMsg = message.ToLower().Split(' ').ToList();

			//If we want to process a hyperlink type command (/equip_sulfuras_hand_of_ragnaros), we only need to split it by the first '_'
			if (processedMsg[0].Contains('_') && processedMsg[0][0] != '_')
			{
				List<string> aux = processedMsg[0].Split('_', 2).ToList();
				processedMsg.RemoveAt(0);
				processedMsg.Insert(0, aux[1]);
				processedMsg.Insert(0, aux[0]);
			}
			if (processedMsg[0][0] == '/') processedMsg[0] = processedMsg[0].Substring(1);
			return processedMsg;
		}

		static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			var errorMessage = exception switch
			{
				ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => exception.ToString()
			};
			Console.WriteLine(errorMessage);
			return Task.CompletedTask;
		}

		static async Task<bool> canUseCommand(string chatId, string command)
		{

			bool characterExists = await DatabaseManager.IsDocumentInCollection(chatId, DbConstants.COLLEC_DEBUG);

			bool isIntroductoryCommand = createCommand.ContainsKeyWord(command) ||
											helpCommand.ContainsKeyWord(command);

			return characterExists || isIntroductoryCommand;

		}

		static async Task onChatMemeberUpdateReceived(Update update)
		{
			//If the update is from a new /start
			if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Member) { 

				string response = @"Welcome to MMOTFG, please create a character with /create to start playing.
Remember that you can get help with /help .";

				string chatId = update.MyChatMember.From.Id.ToString();

				await Communicator.SendText(chatId, response);
			}
		}
	}
}