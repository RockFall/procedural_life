using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FurnitureActions
{
    public static void Door_UpdateAction(Furniture furn, float deltaTime) {

        if (furn.GetParameter("is_opening") >= 1) {
            furn.SetParameter("openness" , furn.GetParameter("openness") + (deltaTime * 4));
            if (furn.GetParameter("openness") >= 1.5f) {
                furn.SetParameter("is_opening", 0);
            }
        } else {
            furn.SetParameter("openness", furn.GetParameter("openness") - deltaTime * 4);
        }

        furn.SetParameter("openness", Mathf.Clamp(furn.GetParameter("openness"),0,1.5f));

        if (furn.cbOnChanged != null)
            furn.cbOnChanged(furn);
    }

    public static ENTERABILITY Door_IsEnterable(Furniture furn) {
        furn.SetParameter("is_opening", 1); // Door opening

        if (furn.GetParameter("openness") >= 1) {
            return ENTERABILITY.Enterable; // Door is open.
        }

        return ENTERABILITY.Soon;
    }


    public static void Stockpile_UpdateAction(Furniture furn, float deltaTime) {
        // We need to ensure that we have a job ont the queue asking:
        //  If we are empty      -> Bring any loose inventory to us
        //  If we have something -> If we are bellow max stack, bring more of it

        // TODO: This furniture doesn't need to run each update, only when:
        //                                                              -- It gets created
        //                                                              -- A good gets delivered
        //                                                              -- A good gets picked up
        //                                                              -- The UI's filter of allowed items get changed

        if (furn.tile.inventory != null && furn.tile.inventory.stackSize >= furn.tile.inventory.maxStackSize) {
            // We are full!
            furn.ClearJobs();
            return;
        }


        // We have already a job queued up?
        if (furn.JobCount() > 0) {
            return;
        }

        // Currently not full but don't have a job yet.
        // TWO possibilities: Either we have SOME inventory, or we have NONE

        // SomethingIsDeeplyWrong_Test
        if (furn.tile.inventory != null && furn.tile.inventory.stackSize == 0) {
            Debug.LogError("Stockpile has a zero-size stack. Clearly WRONG!");
            furn.ClearJobs();
            return;
        }

        Inventory[] itemsDesired;

        if (furn.tile.inventory == null) {
            // We are empty! -- Ask for aything.
            itemsDesired = Stockpile_GetItemFromFilter();
        } else {
            // We aren't empty but we don't have a job! -- Create a job.
            Inventory desInv = furn.tile.inventory.Clone();
            desInv.maxStackSize -= desInv.stackSize;
            desInv.stackSize = 0;

            itemsDesired = new Inventory[] { desInv };
        }

        Job j = new Job(
                    furn.tile,
                    null,
                    null,
                    0,
                    itemsDesired
            );
        // TODO: Add stockpile priorities.
        j.canTakeFromStockpile = false;

        j.RegisterJobWorkedCallback(Stockpile_JobWorked);
        furn.AddJob(j);
    }

    static void Stockpile_JobWorked (Job j) {
        j.tile.furniture.RemoveJob(j);

        foreach (Inventory inv in j.inventoryRequirements.Values) {
            if (inv.stackSize > 0) {
                j.tile.world.inventoryManager.PlaceInventory(j.tile, inv);
                return;
            }
        }
        // TODO: change this when we make our all/any pickup job.
    }


    public static Inventory[] Stockpile_GetItemFromFilter() {
        // TODO: Reads from some UI of this stockpile

        return new Inventory[1] { new Inventory("Wood", 64, 0) };
    }

    public static void JobComplete_FurnitureBuilding(Job theJob) {

        WorldController.Instance.world.PlaceFurniture(theJob.jobObjectType, theJob.tile);
        theJob.tile.pendingFurnitureJob = null;

    }
}
