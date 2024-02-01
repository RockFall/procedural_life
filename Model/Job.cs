using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// This class holds info for a queued up job
/// </summary>
public class Job {
    // In what tile the job's happening
    public Tile tile;

    // Time to complete the job
    public float jobTime { get; protected set; }

    public string jobObjectType { get; protected set; }

    public Furniture furniturePrototype;

    // Callback for a completed ou cancelled job
    Action<Job> cbJobComplete;
    Action<Job> cbJobCancel;
    Action<Job> cbJobWorked;

    public bool acceptsAnyInventoryItem = false;
    public bool canTakeFromStockpile = true;

    public Dictionary<string, Inventory> inventoryRequirements;

    /// <summary>
    /// Initializes a new instance of the <see cref="Job"/> class.
    /// </summary>
    /// <param name="tile">The tile where the Job happens.</param>
    /// <param name="cbJobComplete">A callback for the completed Job.</param>
    /// <param name="jobTime">The time it takes for a job to complete.</param>
    public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirementsList) {
        this.tile = tile;
        this.jobObjectType = jobObjectType;
        this.cbJobComplete += cbJobComplete;
        this.jobTime = jobTime;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirementsList != null) {
            foreach (Inventory inv in inventoryRequirementsList) {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
                
            }
        }
    }

    protected Job(Job other) {
        this.tile = other.tile;
        this.jobObjectType = other.jobObjectType;
        this.cbJobComplete = other.cbJobComplete;
        this.jobTime = other.jobTime;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (other.inventoryRequirements != null) {
            foreach (Inventory inv in other.inventoryRequirements.Values) {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    virtual public Job Clone() {
        return new Job(this);
    }

    /// <summary>
    /// Start this Job.
    /// </summary>
    /// <param name="workTime"></param>
    public void DoWork(float workTime) {
        // FIXME: Improve this later
        if (workTime == 999)
        {
            if (cbJobComplete != null)
            {
                cbJobComplete(this);
            }
        }

        if (HasAllMaterials() == false) {
            //Debug.LogError("Trying to do work on a job that doesn't have all the material");

            // Job can't actually be worked, but still call the callbacks so everything gets updated.
            if (cbJobWorked != null) {
                cbJobWorked(this);
            }

            return;
        }

        jobTime -= workTime;

        if (cbJobWorked != null) {
            cbJobWorked(this);
        }

        if (jobTime <= 0) {
            if (cbJobComplete != null) {
                cbJobComplete(this);
            }
        }
    }

    /// <summary>
    /// Cancel this Job.
    /// </summary>
    public void CancelJob() {
        if (cbJobCancel != null) {
            cbJobCancel(this);
        }

        tile.world.jobQueue.Remove(this);
    }

    /// <summary>
    /// Returns true if the job has all the materials it needs.
    /// </summary>
    /// <returns></returns>
    public bool HasAllMaterials() {
        foreach (Inventory inv in inventoryRequirements.Values) {
            if (inv.maxStackSize > inv.stackSize) {
                // Still needs materials.
                return false;
            }
        }
        // Has all needed materials.
        return true;
    }

    public Inventory GetFirstDesiredInventory() {
        foreach (Inventory inv in inventoryRequirements.Values) {
            if (inv.maxStackSize > inv.stackSize) {
                return inv;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns true if a given Inventory matchs the one the job desires.
    /// </summary>
    /// <param name="inv"></param>
    /// <returns></returns>
    public int DesiresInventoryType(Inventory inv) {
        if (acceptsAnyInventoryItem) {
            return inv.maxStackSize;
        }

        if (inventoryRequirements.ContainsKey(inv.objectType) == false) {
            return 0;
        }

        if (inventoryRequirements[inv.objectType].stackSize >= inventoryRequirements[inv.objectType].maxStackSize) {
            // We already have all that we need!
            return 0;
        }

        // The inventory is of a type we want, and we still need more.
        return inventoryRequirements[inv.objectType].maxStackSize - inventoryRequirements[inv.objectType].stackSize;
    }

    /*public bool DesiresInventoryType(Inventory inv) {
        if (acceptsAnyInventoryItem)
            return true;

        if (inventoryRequirements.ContainsKey(inv.objectType) == false) {
            return false;
        }

        if (inventoryRequirements[inv.objectType].stackSize >= inventoryRequirements[inv.objectType].maxStackSize) {
            // We already have all that we need!
            return false;
        }

        // The inventory is of a type we want and we still need more.
        return true;
    }

    /// <summary>
    /// Returns the amount of a certain Inventory this job desires.
    /// </summary>
    /// <param name="inv"></param>
    /// <returns></returns>
    public int DesiresInventoryAmount(Inventory inv) {
        if (inventoryRequirements.ContainsKey(inv.objectType) == false) {
            return 0;
        }

        if (inventoryRequirements[inv.objectType].stackSize >= inventoryRequirements[inv.objectType].maxStackSize) {
            // We already have all that we need!
            return 0;
        }

        // The inventory is of a type we want and we still need more.
        return inventoryRequirements[inv.objectType].maxStackSize - inventoryRequirements[inv.objectType].stackSize;
    }*/


    /// <summary>
    /// Register a function to be called back when our job is completed.
    /// </summary>
    public void RegisterJobCompleteCallback(Action<Job> cb) {
        cbJobComplete += cb;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterJobCompleteCallback(Action<Job> cb) {
        cbJobComplete -= cb;
    }

    /// <summary>
    /// Register a function to be called back when our job is cancelled.
    /// </summary>
    public void RegisterJobCancelCallback(Action<Job> cb) {
        cbJobCancel += cb;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterJobCancelCallback(Action<Job> cb) {
        cbJobCancel -= cb;
    }

    /// <summary>
    /// Register a function to be called back when our job is worked.
    /// </summary>
    public void RegisterJobWorkedCallback(Action<Job> cb) {
        cbJobWorked += cb;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnegisterJobWorkedCallback(Action<Job> cb) {
        cbJobWorked -= cb;
    }
}
