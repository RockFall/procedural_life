using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

public enum TileType { Empty, Floor };
public enum ENTERABILITY {  Enterable, Never, Soon };


/// <summary>
/// This class holds info for a Tile in the world.
/// </summary>                                                                                
public class Tile : IXmlSerializable {

    TileType type = TileType.Empty;

    public TileType Type {
        get {
            return type;
        }
        set {
            TileType oldType = type;
            type = value;
            // Call the callback.
            if (cbTileChanged != null && oldType != type) {
                cbTileChanged(this);
            }
        }
    }

    // The room this tile belongs to.
    public Room room;

    // An object loosed in this tile (e.g. items on the ground).
    public Inventory inventory { get; protected set; }

    // An object installed in this tile (e.g. sofa, wall).
    public Furniture furniture { get; protected set; }

    // Pending Building Job on this tile
    public Job pendingFurnitureJob;

    // The world in which we exist.
    public World world { get; protected set; }

    public int X { get; protected set; }
    public int Y { get; protected set; }

    // Reminder of something we might want to do in the future.
    float baseTileMovementCost = 1;

    public float movementCost { 
        get { 
            if (Type == TileType.Empty)
                return 0; // 0 is unwalkable
            if (furniture == null)
                return baseTileMovementCost;
            return baseTileMovementCost * furniture.movementCost;
        } 
    }

    // The function we callback any time our tile's data changes.
    public Action<Tile> cbTileChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </summary>
    /// <param name="world">A world instance.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile( World world, int x, int y) {
        this.world = world;
        this.X = x;
        this.Y = y;
    }

    public bool UninstallFurniture() {
        furniture = null;
        return true;
    }

    public bool PlaceFurniture(Furniture furnitureInstance) {

        if (furnitureInstance == null) {
            // We are uninstalling whatever was here before
            return UninstallFurniture();
        }

        if (furnitureInstance.IsValidPosition(this) == false) {
            Debug.LogError("Trying to assign a furniture to a tile that isn't valid!");
            return false;
        }

        for (int x_off = X; x_off < (X + furnitureInstance.Width); x_off++) {
            for (int y_off = Y; y_off < (Y + furnitureInstance.Height); y_off++) {
                Tile t = world.GetTileAt(x_off, y_off);

                t.furniture = furnitureInstance;
            }
        }
        
        return true;
    }


    /// <summary>
    /// Place a inventory in this tile.
    /// </summary>
    /// <param name="inv"></param>
    /// <returns></returns>
    public bool PlaceInventory (Inventory inv) {
        if (inv == null) {
            inventory = null;
            return true;
        }

        if (inventory != null) {
            // There is a inventory in this tile already.

            if (inventory.objectType != inv.objectType) {
                Debug.LogError("Trying to assign a inventory to a tile that already has a inventory of different type!");
                return false;
            }

            int numToMove = inv.stackSize;
            if (inventory.stackSize + numToMove > inventory.maxStackSize) {
                numToMove = inventory.maxStackSize - inventory.stackSize;
            }

            inventory.stackSize += numToMove;
            inv.stackSize -= numToMove;

            Debug.Log("Moved " + numToMove + " of " + inv.objectType + " to stack. Source stack size: " + inv.stackSize);

            return true;
        }

        // At this point we know that our current inventory is null and a new one can be assigned in the Inventory Manager.
        inventory = inv.Clone();
        inventory.tile = this;
        inv.stackSize = 0;

        return true;
    }

    public void DeleteInventory() {
        inventory = null;
    }

    /// <summary>
    /// Returns true if two tiles are adjacents.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="diagOkay"></param>
    /// <returns></returns>
    public bool IsNeighbour(Tile tile, bool diagOkay = false) {
        return
            (Mathf.Abs(this.X - tile.X) + Mathf.Abs(this.Y - tile.Y) == 1) ||                   // Check North, West, South and East tiles
            (diagOkay && (Mathf.Abs(this.X - tile.X) == 1 && Mathf.Abs(this.Y - tile.Y) == 1)); // Check Diagonals
    }

    /// <summary>
    /// Get the neighbours of a tile.
    /// </summary>
    /// <param name="diagOkay">Is diagonal movement okay?</param>
    /// <returns></returns>
    public Tile[] GetNeighbours(bool diagOkay = false) {
        Tile[] neighbours = new Tile[diagOkay? 8 : 4];

        neighbours[0] = world.GetTileAt(X, Y + 1); // N
        neighbours[1] = world.GetTileAt(X - 1, Y); // E
        neighbours[2] = world.GetTileAt(X, Y - 1); // S
        neighbours[3] = world.GetTileAt(X + 1, Y); // W
        if (diagOkay) {
            neighbours[4] = world.GetTileAt(X - 1, Y + 1); // NE
            neighbours[5] = world.GetTileAt(X - 1, Y - 1); // SE
            neighbours[6] = world.GetTileAt(X + 1, Y - 1); // SW
            neighbours[7] = world.GetTileAt(X + 1, Y + 1); // NW
        }

        return neighbours;
    }


    // Usefull fast neighbour getter
    public Tile North() {
        return world.GetTileAt(X, Y + 1);
    }
    public Tile South() {
        return world.GetTileAt(X, Y - 1);
    }
    public Tile East() {
        return world.GetTileAt(X + 1, Y);
    }
    public Tile West() {
        return world.GetTileAt(X - 1, Y);
    }
    public Tile Northwest()
    {
        return world.GetTileAt(X - 1, Y + 1);
    }
    public Tile Northeast()
    {
        return world.GetTileAt(X + 1, Y + 1);
    }
    public Tile Southwest()
    {
        return world.GetTileAt(X - 1, Y - 1);
    }
    public Tile Southeast()
    {
        return world.GetTileAt(X + 1, Y - 1);
    }

    /// <summary>
    /// It returns if a tile is enterable now, never or soon.
    /// </summary>
    /// <returns></returns>
    public ENTERABILITY IsEnterable() {
        // This return true if you can enter this tile right this moment.
        if (movementCost == 0) {
            return ENTERABILITY.Never;
        }

        // Check out furniture if it has a special block on enterability.
        if (furniture != null && furniture.IsEnterable != null) {
            return furniture.IsEnterable(furniture);
        }

        return ENTERABILITY.Enterable;
    }


    /// <summary>
    /// Register a function to be called back when our tile type changes.
    /// </summary>
    public void RegisterTileTypeChangedCallback(Action<Tile> callback) {
        cbTileChanged += callback;
    }
    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback) {
        cbTileChanged -= callback;
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
        writer.WriteAttributeString( "X", X.ToString() );
        writer.WriteAttributeString( "Y", Y.ToString() );
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader) {
        Type = (TileType)int.Parse( reader.GetAttribute("Type") );
    }

}
