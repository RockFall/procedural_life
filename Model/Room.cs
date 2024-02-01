using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
    public float atmosO2 = 0;
    public float atmosN = 0;
    public float atmosCO2 = 0;

    // Tiles that belong to this room
    List<Tile> tiles;

    public Room() {
        tiles = new List<Tile>();
    }

    /// <summary>
    /// Assigns a tile to this room.
    /// </summary>
    /// <param name="t">Tile to be assigned to this room</param>
    public void AssignTile(Tile t) {
        if (tiles.Contains(t)) {
            // Tile already in this room.
            return;
        }

        if (t.room != null) {
            // Tile already belongs to a room, so we're re-assigning it.
            t.room.tiles.Remove(t);
        }

        t.room = this;
        tiles.Add(t);
    }

    /// <summary>
    /// Assign all tiles of this room to outside and clear it's tiles list.
    /// </summary>
    public void UnAssignAllTiles() {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].room = tiles[i].world.GetOutsideRoom();    // Assign to outside
        }
        tiles = new List<Tile>();
    }

    /// <summary>
    /// Starts FloodFill effect to create new rooms.
    /// </summary>
    /// <param name="sourceFurniture"></param>
    public static void DoRoomFloodFill(Furniture sourceFurniture) {
        World world = sourceFurniture.tile.world;

        Room oldRoom = sourceFurniture.tile.room;

        // Try building a new room for each of our NESW directions
        foreach (Tile neighbour in sourceFurniture.tile.GetNeighbours()) {
            ActualFloodFill(neighbour, oldRoom);
        }

        sourceFurniture.tile.room = null;
        oldRoom.tiles.Remove(sourceFurniture.tile);

        if (oldRoom != world.GetOutsideRoom()) {
            // We know all tiles now point to another room so remove woom from world's list.
            if (oldRoom.tiles.Count > 0) {
                Debug.LogError("Deleting a room that has tiles in it");
            }
            world.DeleteRoom(oldRoom);
        }
    }

    protected static void ActualFloodFill(Tile tile, Room oldRoom) {
        if (tile == null) {
            // Trying to flood fill off the map
            return;
        }

        if (tile.room != oldRoom) {
            // This tile was already assigned to another "new" room, which means
            // that the direction picked isn't isolated, we can just return.
            return;
        }

        if (tile.furniture != null && tile.furniture.roomEnclosure) {
            // There is a wall/door/ whatever here so return.
            return;
        }

        if (tile.Type == TileType.Empty) {
            return;
        }

        // FloodFill starts.
        Room newRoom = new Room();

        //Create a queue of tiles.
        Queue<Tile> tilesToCheck = new Queue<Tile>();

        // Add the first tile to the queue.
        tilesToCheck.Enqueue(tile);

        // Loop through tiles until no more valid neighbours get find.
        while (tilesToCheck.Count > 0) {
            Tile currentTile = tilesToCheck.Dequeue();

            // If not yet assigned, assign.
            if (currentTile.room == oldRoom) {
                newRoom.AssignTile(currentTile);

                // Enqueue valid neighbours.
                Tile[] neighbours = currentTile.GetNeighbours();
                foreach (Tile possibleTile in neighbours) {
                    if (possibleTile == null || possibleTile.Type == TileType.Empty) {
                        // We have hit open space so all tiles in this room should be assigned to outside as well.
                        newRoom.UnAssignAllTiles();
                        return;
                    }

                    // We know possibleTile is not null nither empty tile. Check if it is wall.
                    if (possibleTile.room == oldRoom && (possibleTile.furniture == null || possibleTile.furniture.roomEnclosure == false))
                        tilesToCheck.Enqueue(possibleTile);
                }
            }
        }

        newRoom.atmosCO2 = oldRoom.atmosCO2;
        newRoom.atmosN = oldRoom.atmosN;
        newRoom.atmosO2 = oldRoom.atmosO2;

        // Tell the world that a new room has been formed.
        tile.world.AddRoom(newRoom);
    }
}
