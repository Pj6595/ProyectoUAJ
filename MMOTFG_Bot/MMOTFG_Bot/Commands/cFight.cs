using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MMOTFG_Bot.Commands
{
    class cFight : ICommand
    {
        public override void SetDescription()
        {
            commandDescription = @"There is no specific information on this command";
        }
        public override void SetKeywords()
        {
            key_words = new string[] {
                "fight"
            };
        }

        internal override async Task Execute(string command, string chatId, string[] args = null)
        {
            await BattleSystem.StartBattle(chatId, JSONSystem.GetEnemy(args[0]));
        }

        internal override bool IsFormattedCorrectly(string[] args)
        {
            //Format: /fight [enemy name]
            return args.Length == 1;
        }
    }
}
