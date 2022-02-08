﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMOTFG_Bot
{
    static class InventorySystem
    {
        private const int MAX_SLOTS_INVENTORY = 10;
        private static List<InventoryRecord> InventoryRecords = new List<InventoryRecord>();

        public static async void AddItem(long chatId, ObtainableItem item, int quantityToAdd)
        {
            int quantityToAddAux = quantityToAdd;
            while (quantityToAddAux > 0)
            {
                // If an object of this item type already exists in the inventory, and has room to stack more items,
                // then add as many as we can to that stack.
                if (InventoryRecords.Exists(x => (x.InventoryItem.iD == item.iD) && (x.Quantity < item.maxStackQuantity)))
                {
                    InventoryRecord inventoryRecord = InventoryRecords.First(x => (x.InventoryItem.iD == item.iD) && (x.Quantity < item.maxStackQuantity));

                    // Calculate how many more can be added to this stack
                    int maximumQuantityYouCanAddToThisStack = (item.maxStackQuantity - inventoryRecord.Quantity);

                    // Add to the stack (either the full quanity, or the amount that would make it reach the stack maximum)
                    int quantityToAddToStack = Math.Min(quantityToAddAux, maximumQuantityYouCanAddToThisStack);

                    inventoryRecord.AddToQuantity(quantityToAddToStack);

                    // Decrease the quantityToAdd by the amount we added to the stack.
                    // If we added the total quantityToAdd to the stack, then this value will be 0, and we'll exit the 'while' loop.
                    quantityToAddAux -= quantityToAddToStack;
                }
                else
                {
                    // We don't already have an existing inventoryRecord for this ObtainableItem object,
                    // so, add one to the list, if there is room.
                    if (InventoryRecords.Count < MAX_SLOTS_INVENTORY)
                    {
                        // Don't set the quantity value here.
                        // The 'while' loop will take us back to the code above, which will add to the quantity.
                        InventoryRecords.Add(new InventoryRecord(item, 0));
                    }
                    else
                    {
                        //There is no available space in the inventory
                        await TelegramCommunicator.SendText(chatId, "Item " + item.name + " couldn't be added because inventory is full.");
                        break;
                    }
                }
            }
            if (quantityToAdd == 0) await TelegramCommunicator.SendText(chatId, "Item " + item.name + " was added to the inventory.");
            else await TelegramCommunicator.SendText(chatId, "Item " + item.name + " was added " + (quantityToAdd - quantityToAddAux) + " times");
        }

        public static async void ConsumeItems(long chatId, ObtainableItem item, int quantityToConsume)
        {
            //If the item isn't contained in the inventory, there is no point in continuing.
            if (!InventoryRecords.Exists(x => x.InventoryItem.iD == item.iD))
            {
                await TelegramCommunicator.SendText(chatId, "Item " + item.name + " couldn't be consumed as it was not found in your inventory");
                return;
            }

            int quantityToConsumeAux = quantityToConsume;
            if (quantityToConsumeAux == -1)
            {
                quantityToConsume = GetNumberOfItemsInInventory(chatId, item); //-1 = Every single item of that type
                quantityToConsumeAux = quantityToConsume;
            }
            while (quantityToConsumeAux > 0 && InventoryRecords.Exists(x => (x.InventoryItem.iD == item.iD))) {
                // If an object of this item type already exists in the inventory, and has room to stack more items,
                // then add as many as we can to that stack.
                InventoryRecord inventoryRecord = InventoryRecords.First(x => (x.InventoryItem.iD == item.iD));

                // Add to the stack (either the full quanity, or the amount that would make it reach the stack maximum)
                int quantityToConsumeToStack = Math.Min(quantityToConsumeAux, inventoryRecord.Quantity);

                for (int k = 0; k < quantityToConsumeToStack; k++)
                {
                    //TO-DO: Aplicamos el efecto del item en cuestion
                }
                inventoryRecord.AddToQuantity(-quantityToConsumeToStack);

                //If the current stack has been deplenished, it's removed from the list
                if (inventoryRecord.Quantity == 0) InventoryRecords.Remove(inventoryRecord);

                // Decrease the quantityToConsume by the amount we added to the stack.
                // If we added the total quantityToConsume to the stack, then this value will be 0, and we'll exit the 'while' loop.
                quantityToConsumeAux -= quantityToConsumeToStack;
            }
            if(quantityToConsumeAux > 0)
            {
                //Couldn't consume every item.
            }
            if (quantityToConsume == 1) await TelegramCommunicator.SendText(chatId, "Item " + item.name + " was consumed.");
            else await TelegramCommunicator.SendText(chatId, "Item " + item.name + " was consumed " + (quantityToConsume - quantityToConsumeAux) + " times");
        }

        public static async void ThrowAwayItem(long chatId, ObtainableItem item, int quantityToThrowAway)
        {
            //If the item isn't contained in the inventory, there is no point in continuing.
            if (!InventoryRecords.Exists(x => x.InventoryItem.iD == item.iD))
            {
                await TelegramCommunicator.SendText(chatId, "Item " + item.name + " couldn't be thrown away as it was not found in your inventory");
                return;
            }

            int quantityToThrowAwayAux = quantityToThrowAway;
            if (quantityToThrowAwayAux == -1)
            {
                quantityToThrowAway = GetNumberOfItemsInInventory(chatId, item); //-1 = Every single item of that type
                quantityToThrowAwayAux = quantityToThrowAway;
            }

            while (quantityToThrowAwayAux > 0 && InventoryRecords.Exists(x => (x.InventoryItem.iD == item.iD)))
            {
                // If an object of this item type already exists in the inventory, and has room to stack more items,
                // then add as many as we can to that stack.
                InventoryRecord inventoryRecord = InventoryRecords.First(x => (x.InventoryItem.iD == item.iD));

                // Add to the stack (either the full quanity, or the amount that would make it reach the stack maximum)
                int quantityToAddToStack = Math.Min(quantityToThrowAwayAux, inventoryRecord.Quantity);

                inventoryRecord.AddToQuantity(-quantityToAddToStack);

                //If the current stack has been deplenished, it's removed from the list
                if (inventoryRecord.Quantity == 0) InventoryRecords.Remove(inventoryRecord);

                // Decrease the quantityToThrowAway by the amount we added to the stack.
                // If we added the total quantityToThrowAway to the stack, then this value will be 0, and we'll exit the 'while' loop.
                quantityToThrowAwayAux -= quantityToAddToStack;
            }
            if (quantityToThrowAwayAux > 0)
            {
                //Couldn't consume every item.
            }
            if(quantityToThrowAway == 1) await TelegramCommunicator.SendText(chatId, "Item " + item.name + " was thrown away.");
            else await TelegramCommunicator.SendText(chatId, "Item " + item.name + " was thrown away " + (quantityToThrowAway - quantityToThrowAwayAux) + " times");
        }

        public static int GetNumberOfItemsInInventory(long chatId, ObtainableItem item)
        {
            int numItems = 0;
            List<InventoryRecord> auxRecord = InventoryRecords.FindAll(x => x.InventoryItem.iD == item.iD); //TO-DO: Cuando se haga un refactor de los items, comprar por ID's, no por nombres
            foreach (InventoryRecord i in auxRecord)
            {
                numItems += i.Quantity;
            }

            return numItems;
        }

        public static async void ShowInventory(long chatId)
        {
            string message = "User inventory:\n";
            foreach (InventoryRecord i in InventoryRecords)
            {
                message += i.InventoryItem.name + " x" + i.Quantity + "\n";
            }

            if (message != "") await TelegramCommunicator.SendText(chatId, message);
        }

        public class InventoryRecord
        {
            public ObtainableItem InventoryItem { get; private set; }
            public int Quantity { get; private set; }
            public InventoryRecord(ObtainableItem item, int quantity)
            {
                InventoryItem = item;
                Quantity = quantity;
            }
            public void AddToQuantity(int amountToAdd)
            {
                Quantity += amountToAdd;
            }

        }
    }
}