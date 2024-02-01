using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager
{
    // List of every inventory in the game.
    public Dictionary<string, List<Inventory>> inventories;

    public InventoryManager () {
        inventories = new Dictionary<string, List<Inventory>>();
    }

    /// <summary>
    /// Transfer inventory from source to receiver.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="inv"></param>
    /// <returns></returns>
    public bool PlaceInventory(Tile tile, Inventory inv, int amount = -1) {
        // TODO: Separe this into Character -> Tile function and a generic one

        bool tileWasEmpty = tile.inventory == null;

        if (tile.PlaceInventory(inv) == false) {
            // The tile did not accept the inventory for whatever reason, therefore stop.
            return false;
        }

        // At this point, "inv" might be an empty stack if it was merged to another stack.
        CleanUpInventory(inv);


        //Create a new stack if tile was empty.
        if (tileWasEmpty) {
            if (inventories.ContainsKey(tile.inventory.objectType) == false) {
                inventories[tile.inventory.objectType] = new List<Inventory>();
            }
            inventories[tile.inventory.objectType].Add(tile.inventory);

            tile.world.OnInventoryCreated(tile.inventory);
        }

        return true;
    }

    /// <summary>
    /// Transfer inventory from source to receiver.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="charInventory"></param>
    /// <returns></returns>
    public bool PlaceInventory(Character character, Job job, int amount = -1) {
        // CHARACTER -> JOB

        Inventory charInventory = character.inventory;
        if (amount < 0) {
            amount = charInventory.stackSize;
        } else {
            amount = Mathf.Min(amount, charInventory.stackSize);
        }
        if (job.inventoryRequirements.ContainsKey(charInventory.objectType) == false) {
            Debug.LogError("Trying to add inventory to a job that it doesn't want.");
            return false;
        }

        Inventory jobInventory = job.inventoryRequirements[charInventory.objectType];
        jobInventory.stackSize += amount;

        if (jobInventory.stackSize > jobInventory.maxStackSize) {

            charInventory.stackSize = jobInventory.stackSize - jobInventory.maxStackSize;
            jobInventory.stackSize = jobInventory.maxStackSize;

        } else {

            charInventory.stackSize -= amount;

        }

        // At this point, "inv" might be an empty stack if it was merged to another stack.
        CleanUpInventory(charInventory);

        return true;
    }

    /// <summary>
    /// Transfer inventory from source to receiver.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="tileInventory"></param>
    /// <returns></returns>
    public bool PlaceInventory(Tile tile, Character character, int amount = -1) {
        // TILE -> CHARACTER

        Inventory tileInventory = tile.inventory;
        if (amount < 0) {
            amount = tileInventory.stackSize;
        }
        else {
            amount = Mathf.Min(amount, tileInventory.stackSize);
        }

        if (character.inventory == null) {
            character.inventory = tile.inventory.Clone();
            character.inventory.stackSize = 0;
            inventories[character.inventory.objectType].Add(character.inventory);
        } 
        else if (character.inventory.objectType != tileInventory.objectType) {
            Debug.LogError("Character is trying to pick up a mismatched inventory object type.");
            return false;
        }

        character.inventory.stackSize += amount;
        
        if (character.inventory.stackSize > character.inventory.maxStackSize) {
            tileInventory.stackSize = character.inventory.stackSize - character.inventory.maxStackSize;
            character.inventory.stackSize = character.inventory.maxStackSize;
        } else {
            tileInventory.stackSize -= amount;
        }

        // At this point, "inv" might be an empty stack if it was merged to another stack.
        CleanUpInventory(tileInventory);

        return true;
    }

    void CleanUpInventory(Inventory inv) {
        if (inv.stackSize == 0) {
            if (inventories.ContainsKey(inv.objectType)) {
                inventories[inv.objectType].Remove(inv);
            }
            if (inv.tile != null) {
                inv.tile.DeleteInventory();
                inv.tile = null;
            }
            if (inv.character != null) {
                inv.character.inventory = null;
                inv.character = null;
            }
        }
    }


    /// <summary>
    /// Gets the closest inventory of a given type and amount.
    /// </summary>
    public Inventory GetClosestInventoryOfType (string objectType, int desiredAmount, Tile tile, bool canTakeFromStockpile) {
        // FIXME: 
        //        a) We are lying about returning the closest item
        //        b) We can only return the closest item in an optimal manner
        //           when our "inventories" database gets more sophisticated.

        if (inventories.ContainsKey(objectType) == false) {
            Debug.LogError("GetClosestInventoryOfType -- No items of desired type.");
            return null;
        }

        foreach (Inventory inv in inventories[objectType]) {
            if (inv.tile != null && (canTakeFromStockpile || inv.tile.furniture == null || inv.tile.furniture.IsStockpile() == false)) {
                return inv;
            }
        }
        Debug.LogError("Something Is Wrong -- GetClosestInventoryOfType");
        return null;
    }
}
