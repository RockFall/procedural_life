using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.CompilerServices;

public class Furniture : IXmlSerializable {

    /// <summary>
    /// A furniture's unique parameters.
    /// </summary>
    protected Dictionary<string, float> furnParameters;

    /// <summary>
    /// These actions are called every update and needs the furniture it belongs to and deltaTime.
    /// </summary>
    protected Action<Furniture, float> updateActions;

    public Func<Furniture, ENTERABILITY> IsEnterable;

    List<Job> jobs;

    public void Update(float deltaTime) {
        if (updateActions != null) {
            updateActions(this, deltaTime);
        }
    }
    
    // This represents the BASE  tile of the object -- large objects may actually occupy multile tiles
    public Tile tile { get; protected set; }

    // This will be queried by the visual system to know what sprite to render for this object
    public string objectType { get; protected set; }


    // This is a multipler, so a value of "2" means you move half speed.
    // Tile types and other environmental effects may be combined
    // For example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
    // Would have a total movement cost of 8. 1/8th normal speed
    // SPECIAL: If movementCost = 0, then this tile is impassible (e.g. a wall)
    public float movementCost { get; protected set; }

    public bool roomEnclosure { get; protected set; }

    // Size it covers in the ground (e.g. a sofa might be 3x2 but only covers 3x1 and the exttra row is for leg room)
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public Color tint = Color.white;

    public bool linksToNeighbour { get; protected set; }

    // Called whenever there is some sort of change in the GameObject
    public Action<Furniture> cbOnChanged;

    Func<Tile , bool> funcPositionValidation;

    // TODO: Implement larger Objects
    // TODO: Implement Objects Rotation

    // Empty constructor is used for serialization
    public Furniture() {
        furnParameters = new Dictionary<string, float>();
        jobs = new List<Job>();
    }

    // Copy Constructor -- don't call this directly, unless we never do ANY sub-classing. Instead, use Clone()
    protected Furniture( Furniture other ) {
        this.objectType = other.objectType;
        this.movementCost = other.movementCost;
        this.roomEnclosure = other.roomEnclosure;
        this.Width = other.Width;
        this.Height = other.Height;
        this.tint = other.tint;
        this.linksToNeighbour = other.linksToNeighbour;

        furnParameters = new Dictionary<string, float>(other.furnParameters);
        this.jobs = new List<Job>(other.jobs);

        if (other.updateActions!= null)
            this. updateActions = (Action<Furniture, float>)other.updateActions.Clone();

        if (other.funcPositionValidation != null)
            this.funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();

        this.IsEnterable = other.IsEnterable;
    }

    /// <summary>
    /// Makes a copy of the current furniture.
    /// </summary>
    /// <returns></returns>
    virtual public Furniture Clone() {
        return new Furniture( this );
    }


    public static Furniture CreatePrototype (string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false) {
        Furniture furn = new Furniture();

        furn.objectType = objectType;
        furn.movementCost = movementCost;
        furn.roomEnclosure = roomEnclosure;
        furn.Width = width;
        furn.Height = height;
        furn.linksToNeighbour = linksToNeighbour;

        furn.funcPositionValidation = furn.__IsValidPosition;

        furn.furnParameters = new Dictionary<string, float>();

        return furn;
    }


    static public Furniture PlaceInstance (Furniture prototype, Tile tile) {
        if (prototype.funcPositionValidation(tile) == false) {
            Debug.LogError("PlaceInstance -- Position Validity Function returned FALSE.");
            return null;
        }

        // Place of destination is valid at this point.

        Furniture furn = prototype.Clone();
        furn.tile = tile;

        // FIXME: This assumes we are 1x1
        if (tile.PlaceFurniture(furn) == false) {
            // Probably already occupied
            return null;
        }

        if (furn.linksToNeighbour) {
            // Must inform neighbours there is a new furniture coming and they should trigger their OnChangedCallback
            furn.UpdateNeighbours(furn);
        }


        return furn;
    }

    public void UpdateNeighbours(Furniture furn) {
        int x = furn.tile.X;
        int y = furn.tile.Y;
        Tile t;

        // Tile at North
        t = furn.tile.world.GetTileAt(x, y + 1);
        if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == furn.objectType) {
            t.furniture.cbOnChanged(t.furniture);
        }
        // Tile at East
        t = furn.tile.world.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == furn.objectType) {
            t.furniture.cbOnChanged(t.furniture);
        }
        // Tile at South
        t = furn.tile.world.GetTileAt(x, y - 1);
        if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == furn.objectType) {
            t.furniture.cbOnChanged(t.furniture);
        }
        // Tile at West
        t = furn.tile.world.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == furn.objectType) {
            t.furniture.cbOnChanged(t.furniture);
        }
    }

    public bool IsValidPosition (Tile t) {
        return funcPositionValidation(t);
    }

    // FIXME: shouldn't be public
    public bool __IsValidPosition(Tile t) {
        for (int x_off = t.X; x_off < (t.X + Width); x_off++) {
            for (int y_off = t.Y; y_off < (t.Y + Height); y_off++) {
                Tile t2 = t.world.GetTileAt(x_off, y_off);

                // Make sure tile has a valid floor
                if (t2.Type != TileType.Floor) {
                    return false;
                }


                // Make sure tile doesn't already have furniture
                if (t2.furniture != null) {
                    return false;
                }
            
            }
        }

        return true;
    }

    // FIXME: This will be replaced by some LUA validation check
    private bool DEFAULT__IsValidPosition_Door(Tile t) {
        // Make sure we have a pair of E/W or N/S walls

        if (__IsValidPosition(t) == false) {
            return false;
        }

        return true;
    }

    public bool IsStockpile() {
        if (objectType == "Stockpile") {
            return true;
        }

        return false;
    }

    public int JobCount() {
        return jobs.Count;
    }

    public void AddJob(Job j) {
        jobs.Add(j);
        tile.world.jobQueue.Enqueue(j);
    }

    public void RemoveJob (Job j) {
        jobs.Remove(j);
        j.CancelJob();
        tile.world.jobQueue.Remove(j);
    }

    public void ClearJobs() {
        foreach (Job j in jobs) {
            RemoveJob(j);
        }

        jobs = new List<Job>();
    }


    /// <summary>
    /// Get a furniture's parameter.
    /// </summary>
    /// <param name="key">Parameter's name</param>
    /// <param name="default_value">Returned if parameter's not found</param>
    /// <returns></returns>
    public float GetParameter(string key, float default_value = 0) {
        if (furnParameters.ContainsKey(key) == false) {
            return default_value;
        }
        return furnParameters[key];
    }

    /// <summary>
    /// Set a furniture's parameter certain value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetParameter(string key, float value) {
        furnParameters[key] = value;
    }

    /// <summary>
    /// Change an existing furniture's parameter.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void ChangeParameter(string key, float value) {
        if (furnParameters.ContainsKey(key) == false) {
            furnParameters[key] = value;
        } else {
            furnParameters[key] += value;
        }
    }

    /// <summary>
    /// Registers a Furniture Action to be called each frame.
    /// </summary>
    /// <param name="a"></param>
    public void RegisterUpdateAction(Action<Furniture, float> a) {
        updateActions += a;
    }

    /// <summary>
    /// Unregisters a Furniture Update Action.
    /// </summary>
    /// <param name="a"></param>
    public void UnregisterUpdateAction(Action<Furniture, float> a) {
        updateActions -= a;
    }

    /// <summary>
    /// Register a function to be called back when our object changes.
    /// </summary>
    public void RegisterOnChangedCallback(Action<Furniture> callbackFunc) {
        cbOnChanged += callbackFunc;
    }

    /// <summary>
    /// Unregister a callback
    /// </summary>
    public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc) {
        cbOnChanged -= callbackFunc;
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
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("ObjType", objectType);
        //writer.WriteAttributeString("MovCost", movementCost.ToString());

        foreach (string k in furnParameters.Keys) {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", furnParameters[k].ToString());
            writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader reader) {
        //width = int.Parse(reader.GetAttribute("Width"));
        //height = int.Parse(reader.GetAttribute("Height"));
        //movementCost = int.Parse(reader.GetAttribute("MovCost"));

        if (reader.ReadToDescendant("Param")) {
            do {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
                furnParameters[k] = v;
            } while (reader.ReadToNextSibling("Param"));
        }
    }
}
