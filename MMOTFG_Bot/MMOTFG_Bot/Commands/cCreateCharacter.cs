﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MMOTFG_Bot.Navigation;

namespace MMOTFG_Bot.Commands
{
	/// <summary>
	/// Shows the available directions from a given node.
	/// </summary>
	class cCreateCharacter : ICommand
	{
		public override void SetDescription()
		{
			commandDescription = @"Creates a new character. Can only be used if you haven't created one already.
Use: create [character name]";
		}
		public override void SetKeywords()
		{
			key_words = new string[] {
				"create"
			};
		}

		internal override async Task Execute(string command, string chatId, string[] args = null)
		{

			string charName = args[0];

			Dictionary<string,object>tempDict = await DatabaseManager.GetDocumentByUniqueValue(DbConstants.PLAYER_FIELD_NAME, charName, DbConstants.COLLEC_DEBUG);

			if(tempDict != null)
			{
				await Program.Communicator.SendText(chatId, "That name is already in use");
				return;
			}

			tempDict = await DatabaseManager.GetDocumentByUniqueValue(DbConstants.PLAYER_FIELD_TELEGRAM_ID, chatId.ToString(), DbConstants.COLLEC_DEBUG);

			if (tempDict != null)
			{
				await Program.Communicator.SendText(chatId, "You can only have one character per player");
				return;
			}

			Dictionary<string, object> dict = new Dictionary<string, object>
			{
				{ DbConstants.PLAYER_FIELD_NAME , charName},
				{ DbConstants.PLAYER_FIELD_TELEGRAM_ID, chatId.ToString()}
			};

			bool created = await DatabaseManager.AddDocumentToCollection(dict, chatId.ToString(), DbConstants.COLLEC_DEBUG);

			if (!created)
			{
				await Program.Communicator.SendText(chatId, "Error: Cant add that character to the database");
				Console.WriteLine("Error when trying to add character with telegramId {} to the database", chatId);
				return;
			}

			await Program.Communicator.SendText(chatId, String.Format("Hello {0}, ready for a good time?", charName));
			Console.WriteLine("Telegram user {0} just created characater with name {1}", chatId, charName);

			await InventorySystem.CreatePlayerInventory(chatId);
			await BattleSystem.CreatePlayerBattle(chatId);
			await ProgressKeeper.CreateProgressKeeper(chatId);
			await Map.CreatePlayerPosition(chatId);

		}

		internal override bool IsFormattedCorrectly(string[] args)
		{
			if (args.Length == 1) return true;
			return false;
		}
	}
}

