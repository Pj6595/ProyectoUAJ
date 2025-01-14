﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MMOTFG_Bot.Navigation;

namespace MMOTFG_Bot.Events
{
    /// <summary>
    /// Sends a text message to the user.
    /// </summary>
    class eSetFlag : Event
    {
        public string Name
        {
            get;
            set;
        }

        public bool SetAs
        {
            get;
            set;
        }

        public async override Task Execute(string chatId)
        {
            await ProgressKeeper.LoadSerializable(chatId);
            ProgressKeeper.SetFlagAs(chatId, Name, SetAs);
            await ProgressKeeper.SaveSerializable(chatId);
        }
    }
}
