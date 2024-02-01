using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using UnityEngine.UIElements;
using System.Runtime.InteropServices;

public class Character
{
    // World Position
    public float X {
        get {
            return Mathf.Lerp(currTile.X, nextTile.X, movementPercentage);
        }
    }
    public float Y {
        get {
            return Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage);
        }
    }

    // Movement management
    public Tile currTile { get; protected set; }
    Tile _destTile;
    Tile destTile {
        get { return _destTile; }
        set {
            if (_destTile != value) {
                _destTile = value;
                pathAStar = null;   // If this is a new destination then we need to destroy the current pathfinding.
            }
        } 
    }
    Tile nextTile; // Next tile given by the pathfinding system.
    Path_AStar pathAStar;
    float movementPercentage; // Goes from 0 to 1 as we move from currTile to destTile.
    float speed = 4f; // Tiles per second

    // Callbacks
    Action<Character> cbCharacterChanged;

    // Current assigned job.
    Job myJob;

    // The item we are carrying (not gear/equipament)
    public Inventory inventory;

    // Only for serialization.
    public Character() {
    }

    // Creates a new instance of this class.
    public Character(Tile tile) {
        currTile = destTile = nextTile =tile;
    }
    
    /// <summary>
    /// Get a job from the world list and tests it reachability.
    /// </summary>
    /// <returns>True if succeeded</returns>
    void GetNewJob() {
        myJob = currTile.world.jobQueue.Dequeue();
        if (myJob == null) {
            // There was no job on the queue for us, so just return false.
            return;
        }
        // We have a Job!

        // Immediately check if the job tile is reachable
        // NOTE: We might not be pathing to it right away.

        // Set job's nearest walkable tile as destination
        //destTile = GetNearestWalkableNeighbour(myJob.tile);
        destTile = myJob.tile;
        // Job Ended Callback
        myJob.RegisterJobCancelCallback(OnJobEnded);
        myJob.RegisterJobCompleteCallback(OnJobEnded);

        // Try to get a pathfinding from character to destTile.
        pathAStar = new Path_AStar(WorldController.Instance.world, currTile, destTile);
        if (pathAStar.Lenght() == 0) {
            // Pathfinding couldn't get a path to the destination.
            Debug.LogError("Path_AStar returned no path to target job tile!");

            // Re-enqueue Job, set current to null and return false.
            AbandonJob();
            return;
        }
        return;
    }

    void Update_DoJob(float deltaTime) {
        // Check if has Job
        if (myJob == null) {
            // Gets Job!
            GetNewJob();

                if (myJob == null) {
                // If there was no job on the queue or job is unreachable, just return.
                destTile = currTile;
                return;
            }
        }

        bool godMode = false;

        if (godMode == true)
        {
            myJob.DoWork(999);
            return;
        }

        // We have a created and reachable job!

        // STEP 1: Does the job have all the materials it needs?
        if (myJob.HasAllMaterials() == false) {
            // We are missing something
            //Debug.Log("%%%%% We need materials %%%%%");
            // STEP 2: Get the materials.
            // Are we CARRYING anything that the job location wants?
            if (inventory != null) {
                if (myJob.DesiresInventoryType(inventory) > 0) { // Yeah we are!
                    // Walk to the job.
                    if (currTile == myJob.tile) {
                        // We are at the job's site so drop the inventory.
                        currTile.world.inventoryManager.PlaceInventory(this, myJob);
                        myJob.DoWork(0); // This will call all cbJobWorked callbacks, because even though
                                         // we aren't progressing, it might want to do something with the fact
                                         // that the requirements are being met.
                        if (myJob == null)
                        if (myJob != null && myJob.jobObjectType != null && myJob.jobObjectType != "Stockpile")
                            GameObject.FindObjectOfType<JobSpriteController>().SetToBuildSprite(myJob); // FIXME: Only temporary for visuals

                        // Are we still carrying things?
                        if (inventory.stackSize == 0) {
                            inventory = null;
                        } else {
                            Debug.LogError("Character is still carrying inventory, which shouldn't be. Just setting to NULL for now, but this means we are LEAKING inventory!");
                            inventory = null;
                        }
                    } else {
                        // We still need to walk to the job site.
                        //destTile = GetNearestWalkableNeighbour(myJob.tile);
                        destTile = myJob.tile;
                        return;
                    }

                } else {
                    // Still carrying something but the job doesn't need it.
                    if (currTile.world.inventoryManager.PlaceInventory(currTile, inventory)) {
                        Debug.LogError("Character trying to dump inventory in an invalid tile.");
                        // FIXME: Dump inventory implementation
                        inventory = null;
                    }
                }
            } else { // Nope, need to get new stuff!

                // At this point, the job still needs materials and we're not carrying it.
                // Are we over a tile that has what the job needs?
                if (currTile.inventory != null &&
                    myJob.DesiresInventoryType(currTile.inventory) > 0 &&
                    (myJob.canTakeFromStockpile || currTile.furniture == null || currTile.furniture.IsStockpile() == false)) {

                    // Yeah! Pick up the stuff!
                    currTile.world.inventoryManager.PlaceInventory(currTile, this, myJob.DesiresInventoryType(currTile.inventory));

                } else {
                    // Nope! Walk towards a tile containing the required goods.
                    // Find the first thing in the job that isn't satisfied.
                    Inventory desired = myJob.GetFirstDesiredInventory();

                    // FIXME: This is a unoptimal initial setup
                    Inventory supplier = currTile.world.inventoryManager.GetClosestInventoryOfType(desired.objectType, desired.maxStackSize - desired.stackSize, currTile, myJob.canTakeFromStockpile);

                    // Set the desired Inventory as destination.
                    if (supplier == null) {
                        Debug.Log("No tile contains objects of type '" + desired.objectType + "' to satisfy job requirements.");
                        AbandonJob();
                        return;
                    }
                    destTile = supplier.tile;
                    return;
                }
            }

            return; // We can't continue untill all materials are satisfied.
        }
        //  If we get here then the job has all the material that it needs, so set it as destTile.
        if (myJob == null)
            Debug.LogError("Job is NULL right before setting it as destination");
        //destTile = GetNearestWalkableNeighbour(myJob.tile);
        destTile = myJob.tile;

        // Are we there yet?
        if (currTile == destTile) {
            // We are at the correct tile for our job, so execute the job's DoWork,
            // which will mostly countdown jobTime and call it's "Job Complete" callback
            myJob.DoWork(deltaTime);
        }
    }
    
    Tile GetNearestWalkableNeighbour(Tile tile) {
        float distance = Mathf.Infinity;
        Tile nearestTile = tile;
        foreach (Tile neigh in tile.GetNeighbours()) {
            if (DistanceFromTileToCharacter(this, neigh) < distance && neigh.movementCost > 0) {
                distance = DistanceFromTileToCharacter(this, neigh);
                nearestTile = neigh;
            }
        }
        if (nearestTile == null)
            return tile;
        return nearestTile;
    }

    public void AbandonJob() {
        nextTile = destTile = currTile;
        currTile.world.jobQueue.Enqueue(myJob);
        myJob = null;
    }

    void Update_DoMovement(float deltaTime) {
        if (currTile == destTile) {
            pathAStar = null;
            return; // We're already where we want to be.
        }

        // currTile = Tile I'm in and in process of leaving.
        // nextTile = Tile I'm currently entering.
        // destTile = Our final destination -- we never walk here directly but use it for the pathfinding.

        if (nextTile == null || nextTile == currTile) {
            // I don't have a nextTile so I get it from the pathfinder.
            if (pathAStar == null || pathAStar.Lenght() == 0){
                // I don't have a pathfinder so I create one.
                pathAStar = new Path_AStar(WorldController.Instance.world,currTile, destTile);
                if (pathAStar.Lenght() == 0) {
                    // Pathfinding couldn't get a path to the destination.
                    Debug.LogError("Path_AStar returned no path to destination!");

                    // Re-enqueue Job and set current to null.
                    AbandonJob();
                    return;
                }
                // Let's ignore the first tile, because that's the tile we're currently in.
                nextTile = pathAStar.Dequeue();
            } // Now we have a path.

            // Grab next waypoint from the path system
            nextTile = pathAStar.Dequeue();

            if (nextTile == currTile) {
                Debug.LogError("Update_DoMovement -- nextTile is currTile?");
            }
        }

        // At this point we have a valid nextTile

        // Total distance.
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - nextTile.X, 2f) + Mathf.Pow(currTile.Y - nextTile.Y, 2f));

        if (nextTile.IsEnterable() == ENTERABILITY.Never) {
            // Most likely a wall got built after path creation.
            // FIXME: When a wall gets spawned we should invalidate our pathfinding imediatale
            Debug.LogError("Update_DoMovement -- Trying to move to an unwalkable tile or walkable tile with moveCost of 0.");
            nextTile = null;   // Our next tile is a no-go.
            pathAStar = null;  // clearly our pathfinding info is out of date.
            return;
        } else if (nextTile.IsEnterable() == ENTERABILITY.Soon) {
            // The tile we are trying to enter is technically walkable (i.e. not a wall),
            // but are we allowed to enter it right now.
            // We return without processing the movement.
            return;
        }

        // Distance travelled this update.
        float distThisFrame = speed/nextTile.movementCost * deltaTime;

        // Percentage to destination this frame.
        float percThisFrame = distThisFrame / distToTravel;

        // Add that to overall percentage travelled.
        movementPercentage += percThisFrame;

        if (movementPercentage >= 1) {
            // We have reached our destination

            // TODO: Get next destination tile from pathfinding system. 
            // TODO: If there are no more tiles, we are in the real destination
            currTile = nextTile;
            movementPercentage = 0;
        }

        if (cbCharacterChanged != null) {
            cbCharacterChanged(this);
        }
    }

    public void Update(float deltaTime) {

        Update_DoJob(deltaTime);

        Update_DoMovement(deltaTime);
        
        if (cbCharacterChanged != null) {
            cbCharacterChanged(this);
        }
    }

    public void SetDestination(Tile tile) {
        if (currTile.IsNeighbour(tile, true) == false) {
            Debug.LogError("Character::SetDestination -- Our destination tile isn't our neighbour.");
        }

        destTile = tile;
    }

    public void RegisterOnChangedCallback (Action<Character> cb) {
        cbCharacterChanged += cb;
    }
    public void UnregisterOnChangedCallback(Action<Character> cb) {
        cbCharacterChanged -= cb;
    }

    void OnJobEnded(Job j) {
        // Job completed or cancelled.

        j.UnregisterJobCancelCallback(OnJobEnded);
        j.UnregisterJobCompleteCallback(OnJobEnded);

        if (j != myJob) {
            Debug.LogError("Character being told about job that isn't his. You forgot to unregister something.");
            return;
        }

        myJob = null;
    }

    float DistanceFromTileToCharacter(Character ch, Tile t) {
        return Mathf.Sqrt(Mathf.Pow(ch.X - t.X, 2f) + Mathf.Pow(ch.Y - t.Y, 2f));
    }








    ////////////////////////////////////////////////////////////////////////////////////
    ///                                                                              ///
    ///                            SAVING & LOADING                                  ///
    ///                                                                              ///
    ////////////////////////////////////////////////////////////////////////////////////


    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", currTile.X.ToString());
        writer.WriteAttributeString("Y", currTile.Y.ToString());
    }

    public void ReadXml(XmlReader reader) {

    }
}
