using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.InteropServices;

public class World : IXmlSerializable {

    // A two-dimensional array to hold our tile data
    Tile[,] tiles;


    // Temporary list of all charactes in the game.
    public List<Character>  characters;
    // All furniture in the game.
    public List<Furniture>  furnitures;
    // All inventory items in the game.
    public InventoryManager inventoryManager;
    // All rooms in the game.
    public List<Room>       rooms;

    // The graph of our tile system for pathfinding purposes.
    public Path_TileGraph tileGraph;

    // Prototypes of every Furniture
    public Dictionary<string, Furniture> furniturePrototypes;
    public Dictionary<string, Job> furnitureJobPrototypes;

    // The tile number width of the world
    public int Width { get; protected set; }

    // The tile number height of the world
    public int Height { get; protected set; }

    // Callbacks
    Action<Furniture> cbFurnitureCreated;
    Action<Character> cbCharacterCreated;
    Action<Inventory> cbInventoryCreated;
    Action<Tile> cbTileChanged;

    // Job Queue
    public JobQueue jobQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// </summary>
    /// <param name="width">Width in tiles.</param>
    /// <param name="height">Height in tiles.</param>
    public World(int width = 100, int height = 100) {
        // Creates an empty world.
        SetupWorld(width, height);

        // Makes a dummy character.
        CreateCharacter(GetTileAt(Width / 2, Height / 2));
    }

    /// <summary>
    /// Default constructor, used when loading a world from a file.
    /// </summary>
    public World() {

    }

    public Room GetOutsideRoom() {
        return rooms[0];
    }

    public void AddRoom (Room r) {
        rooms.Add(r);
    }

    public void DeleteRoom (Room r) {
        if (r == GetOutsideRoom()) {
            Debug.LogError("Tried to delete outside room.");
            return;
        }

        //Remove this room from our rooms list.
        rooms.Remove(r);

        // All tiles of the room re-assigned to outside
        r.UnAssignAllTiles();
    }

    void SetupWorld(int width, int height) {
        jobQueue = new JobQueue();

        Width = width;
        Height = height;

        tiles = new Tile[width, height];

        rooms = new List<Room>();
        rooms.Add(new Room()); // Create the outside

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
                tiles[x, y].room = rooms[0]; // rooms[0] is always going to be outside.
            }
        }

        //Debug.Log("World created with " + (width * height) + " tiles.");

        CreateFurniturePrototypes();

        characters  = new List<Character>();
        furnitures  = new List<Furniture>();
        inventoryManager = new InventoryManager();
    }
    
    public void Update(float deltaTime) {
        foreach (Character c in characters) {
            c.Update(deltaTime);
        }
        foreach (Furniture furn in furnitures) {
            furn.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile t) {
        Character c = new Character(t);

        characters.Add(c);

        if (cbCharacterCreated != null) {
            cbCharacterCreated(c);
        }

        return c;
    }



    void CreateFurniturePrototypes() {
        // TODO: Replace this by a function that reads our furniture data from a text file.

        furniturePrototypes = new Dictionary<string, Furniture>();
        furnitureJobPrototypes = new Dictionary<string, Job>();

        furniturePrototypes.Add("Wood Wall", Furniture.CreatePrototype(
                                                    "Wood Wall", // Name
                                                    0,      // Impassable
                                                    1,      // Width
                                                    1,      // Height
                                                    true,   // Links to neighbour and "sort of" becomes a large object
                                                    true    // Enclose rooms
                                                )
            );
        furnitureJobPrototypes.Add("Wood Wall",
            new Job(null, "Wood Wall", FurnitureActions.JobComplete_FurnitureBuilding, .2f, new Inventory[] { new Inventory("Wood", 5, 0) })
            );

        furniturePrototypes.Add("Lamp", Furniture.CreatePrototype(
                                                    "Lamp", // Name
                                                    1,      // Impassable
                                                    1,      // Width
                                                    1,      // Height
                                                    false,   // Links to neighbour and "sort of" becomes a large object
                                                    false    // Enclose rooms
                                                )
            );
        //furnitureJobPrototypes.Add("Wall",
        //    new Job(null, "Wall", FurnitureActions.JobComplete_FurnitureBuilding, .2f, new Inventory[] { new Inventory("Steel_Plate", 5, 0) })
        //    );

        furniturePrototypes.Add("Door", Furniture.CreatePrototype(
                                                    "Door", // Name
                                                    1.5f,   // Pathfinding movecost
                                                    1,      // Width
                                                    1,      // Height
                                                    false,  // Links to neighbour and "sort of" becomes a large object
                                                    true    // Enclose rooms
                                                )
            );
        furnitureJobPrototypes.Add("Door",
            new Job(null, "Door", FurnitureActions.JobComplete_FurnitureBuilding, .4f, new Inventory[] { new Inventory("Wood", 25, 0) })
            );

        // What if the objects behavioues were scriptable? And therefore were part of the text file we are reading now

        furniturePrototypes["Door"].SetParameter("openness",0);
        furniturePrototypes["Door"].SetParameter("is_opening", 0);

        furniturePrototypes["Door"].RegisterUpdateAction(FurnitureActions.Door_UpdateAction);
        furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;


        furniturePrototypes.Add("Stockpile", Furniture.CreatePrototype(
                                                    "Stockpile", // Name
                                                    1,      // Impassable
                                                    1,      // Width
                                                    1,      // Height
                                                    true,   // Links to neighbour and "sort of" becomes a large object
                                                    false    // Enclose rooms
                                                )
            );
        furniturePrototypes["Stockpile"].RegisterUpdateAction(FurnitureActions.Stockpile_UpdateAction);
        furnitureJobPrototypes.Add("Stockpile",
            new Job(null, "Stockpile", FurnitureActions.JobComplete_FurnitureBuilding, -1f, null)
            );

        furniturePrototypes.Add("Oxygen Generator", Furniture.CreatePrototype(
                                                    "Oxygen Generator", // Name
                                                    10,      // Impassable
                                                    2,      // Width
                                                    2,      // Height
                                                    false,   // Links to neighbour and "sort of" becomes a large object
                                                    false    // Enclose rooms
                                                )
            );
    }


    public void SetupPathfindingExample() {
        Debug.Log("SetupPathfindingExample");

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l-5; x < l+15; x++) {
            for (int y = b-5; y < b + 15; y++) {
                tiles[x, y].Type = TileType.Floor;

                if (x == l || x == (l+9) || y == b || y == (b+9)) {
                    if (x != (l+9) && y != (b + 4)) {
                        PlaceFurniture("Wall", tiles[x, y]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the tile data at x and y.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile GetTileAt(int x, int y) {
        if (x >= Width || x < 0 || y >= Height || y < 0) {
            //Debug.LogError("Tile ("+x+","+y+") is out of range.");
            return null;
        }
        return tiles[x,y];
    }

    public Furniture PlaceFurniture(string objectType, Tile t) {
        // TODO: This function only assumes 1x1 tiles -- change this late!

        if (furniturePrototypes.ContainsKey(objectType) == false) {
            Debug.LogError("furniturePrototypes doesn't contain a prototype for key: " + objectType);
            return null;
        }

        Furniture furn = Furniture.PlaceInstance(furniturePrototypes[objectType], t);

        if (furn == null) {
            // Failed to place object -- most likely there was already something there
            return null;
        }
        furnitures.Add(furn);

        // Do we need to recalculate our room?
        if (furn.roomEnclosure) {
            Room.DoRoomFloodFill(furn);
        }

        if (cbFurnitureCreated != null) {
            // Furniture correctly placed.
            cbFurnitureCreated(furn);

            if (furn.movementCost != 1) {
                InvalidateTileGraph(); // Reset the pathfinding system.
            }
        }
        return furn;
    }

    /// <summary>
    /// Register a function to be called back when our furniture is created..
    /// </summary>
    public void RegisterFurnitureCreated(Action<Furniture> callbackfunc) {
        cbFurnitureCreated += callbackfunc;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc) {
        cbFurnitureCreated -= callbackfunc;
    }

    /// <summary>
    /// Register a function to be called back when our character is created..
    /// </summary>
    public void RegisterCharacterCreated(Action<Character> callbackfunc) {
        cbCharacterCreated += callbackfunc;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterCharacterCreated(Action<Character> callbackfunc) {
        cbCharacterCreated -= callbackfunc;
    }

    /// <summary>
    /// Register a function to be called back when our inventory is created..
    /// </summary>
    public void RegisterInventoryCreated(Action<Inventory> callbackfunc) {
        cbInventoryCreated += callbackfunc;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterInventoryCreated(Action<Inventory> callbackfunc) {
        cbInventoryCreated -= callbackfunc;
    }

    public void OnInventoryCreated (Inventory inv) {
        if (cbInventoryCreated != null) {
            cbInventoryCreated(inv);
        }
    }

    /// <summary>
    /// Register a function to be called back when our tile changes..
    /// </summary>
    public void RegisterTileChanged(Action<Tile> callbackfunc) {
        cbTileChanged += callbackfunc;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterTileChanged(Action<Tile> callbackfunc) {
        cbTileChanged -= callbackfunc;
    }

    /// <summary>
    /// Called whenever ANY tile's type changes.
    /// </summary>
    /// <param name="t"></param>
    void OnTileChanged (Tile t) {
        if (cbTileChanged == null)
            return;
        cbTileChanged(t);

        InvalidateTileGraph();
    }

    /// <summary>
    /// Destroys our tile graph
    /// </summary>
    public void InvalidateTileGraph() {
        tileGraph = null;
    }

    public bool IsFurniturePlacementValid (string furnitureType, Tile t) {
        return furniturePrototypes[furnitureType].IsValidPosition(t);
    }

    public Furniture GetFurniturePrototype (string furnitureType) {
        if (furniturePrototypes.ContainsKey(furnitureType) == false) {
            Debug.LogError("No furniture with type: "+ furnitureType);
        }
        return furniturePrototypes[furnitureType];
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
        // SAVE WORLD SIZE
        writer.WriteAttributeString( "Width", Width.ToString() );
        writer.WriteAttributeString( "Height", Height.ToString() );

        // SAVE TILES
        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                if (tiles[x, y].Type != TileType.Empty) {
                    writer.WriteStartElement("Tile");
                    tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        // SAVE FURNITURES
        writer.WriteStartElement("Furnitures");
        foreach(Furniture furniture in furnitures) {
            writer.WriteStartElement("Furniture");
            furniture.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        // SAVE CHARACTERS
        writer.WriteStartElement("Characters");
        foreach (Character c in characters) {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        // SAVE INVENTORIES
        //writer.WriteStartElement("Inventories");
        //foreach (Inventory inv in inventoryManager.inventories.Values)
        //{
        //    writer.WriteStartElement("Inventory");
        //    inv.WriteXml(writer);
        //    writer.WriteEndElement();
        //}
    }

    public void ReadXml(XmlReader reader) {
        // Load info here.

        Width = int.Parse( reader.GetAttribute("Width") );
        Height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(Width,Height);

        while (reader.Read()) {
            switch(reader.Name) {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }
        }

        // Todo: DEBUGGING ONLY!! REMOVEME LATER
        // Create an Inventory Item
        Inventory inv = new Inventory("Wood", 64, 64);
        Tile t = GetTileAt(Width / 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null) {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory("Wood", 64, 42);
        t = GetTileAt(Width / 2 + 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null) {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory("Wood", 64, 17);
        t = GetTileAt(Width / 2 + 1, Height / 2 + 2 );
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null) {
            cbInventoryCreated(t.inventory);
        }
    }

    void ReadXml_Tiles(XmlReader reader) {
        // We are in the "Tiles" element, so read elements until we run out of "Tile" nodes.
        if (reader.ReadToDescendant("Tile")) {
            // We have at least one tile, so do something with it.
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                tiles[x, y].ReadXml(reader);

            } while (reader.ReadToNextSibling("Tile"));
        }
    }

    void ReadXml_Furnitures(XmlReader reader) {
        // We are in the "Furnitures" element, so read elements until we run out of "Furniture" nodes.
        if ( reader.ReadToDescendant("Furniture") ) {

            do {
                //int width = int.Parse(reader.GetAttribute("Width"));
                //int height = int.Parse(reader.GetAttribute("Heigh"));
                //float movCost = float.Parse(reader.GetAttribute("MovCost"));
                string objType = reader.GetAttribute("ObjType");

                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Furniture furn = PlaceFurniture(objType, tiles[x, y]);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furniture"));

        }
    }

    void ReadXml_Characters(XmlReader reader) {
        // We are in the "Furnitures" element, so read elements until we run out of "Furniture" nodes.
        if ( reader.ReadToDescendant("Character") ){

            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Character c = CreateCharacter(tiles[x, y]);
                c.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));

        }
    }
}
