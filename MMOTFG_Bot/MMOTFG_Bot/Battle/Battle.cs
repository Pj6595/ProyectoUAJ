﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MMOTFG_Bot
{
    class Battle
    {
        Player player;
        Enemy enemy;
        Random rnd;

        public Battle(Player p, Enemy e)
        {
            player = p;
            enemy = e;
            rnd = new Random();
            Console.WriteLine("tiroriroriroriro chan chan chan");
        }
        
        public async void setPlayerOptions(long chatId)
        {
            await TelegramCommunicator.SendButtons(chatId, player.attackNum, player.attackNames.ToArray());
        }

        public async void playerAttack(long chatId, string attackName)
        {
            attackName = char.ToUpper(attackName[0]) + attackName.Substring(1);
            int attack = player.attackNames.IndexOf(attackName);
            if (attack == -1) return;
            float damage = player.stats[(int)StatNames.ATK] * player.attacks[attack].power;
            await TelegramCommunicator.SendText(chatId, "Player used " + player.attacks[attack].name + "! Enemy took " + damage + " damage.");
            enemy.stats[(int)StatNames.HP] -= damage;
            await TelegramCommunicator.SendText(chatId, "Enemy has " + enemy.stats[(int)StatNames.HP] + " HP left");
            if (enemy.stats[(int)StatNames.HP] <= 0)
                await TelegramCommunicator.SendText(chatId, "Enemy died!");
            else enemyAttack(chatId);
        }

        private async void enemyAttack(long chatId)
        {
            int attack = rnd.Next(0, enemy.attackNum);
            float damage = enemy.stats[(int)StatNames.ATK] * enemy.attacks[attack].power;
            await TelegramCommunicator.SendText(chatId, "Enemy used " + enemy.attacks[attack].name + "! Player took " + damage + " damage.");
            player.stats[(int)StatNames.HP] -= damage;
            await TelegramCommunicator.SendText(chatId, "You have " + player.stats[(int)StatNames.HP] + " HP left");
            if (player.stats[(int)StatNames.HP] <= 0)
                await TelegramCommunicator.SendText(chatId, "You died!");
        }
    }
}
