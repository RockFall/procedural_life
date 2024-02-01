using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using static TMPro.Examples.TMP_ExampleScript_01;

public class Inventory{

    public string objectType;
    public int    maxStackSize = 50;

    protected int _stackSize = 1;
    public int stackSize {
        get { return _stackSize; }
        set { if (_stackSize != value) {
                _stackSize = value;
                if (cbInventoryChanged != null) {
                    cbInventoryChanged(this);
                }
            } 
        }
    }

    public Tile      tile;
    public Character character;

    // The function we callback any time our inventory's data changes.
    public Action<Inventory> cbInventoryChanged;

    // Empty constructor for loading and saving reasons.
    public Inventory() {

    }

    // Normal constructor.
    public Inventory(string objectType, int maxStackSize, int stackSize) {
        this.objectType   = objectType;
        this.maxStackSize = maxStackSize;
        this.stackSize    = stackSize;
    }

    // Copy constructor.
    protected Inventory (Inventory other) {
        objectType   = other.objectType;
        maxStackSize = other.maxStackSize;
        stackSize    = other.stackSize;
    }
    virtual public Inventory Clone() {
        return new Inventory(this);

    }

    /// <summary>
    /// Register a function to be called back when our inventory type changes.
    /// </summary>
    public void RegisterInventoryChangedCallback(Action<Inventory> callback) {
        cbInventoryChanged += callback;
    }
    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterInventoryChangedCallback(Action<Inventory> callback) {
        cbInventoryChanged -= callback;
    }


    ////////////////////////////////////////////////////////////////////////////////////
    ///                                                                              ///
    ///                            SAVING & LOADING                                  ///
    ///                                                                              ///
    ////////////////////////////////////////////////////////////////////////////////////


    //public XmlSchema GetSchema()
    //{
    //    return null;
    //}
    //
    //public void WriteXml(XmlWriter writer)
    //{
    //    writer.WriteAttributeString("X", tile.X.ToString());
    //    writer.WriteAttributeString("Y", tile.Y.ToString());
    //    writer.WriteAttributeString("Type", objectType);
    //    writer.WriteAttributeString("StackSize", stackSize.ToString());
    //    //writer.WriteAttributeString("MovCost", movementCost.ToString());
    //}
    //
    //public void ReadXml(XmlReader reader)
    //{
    //    //width = int.Parse(reader.GetAttribute("Width"));
    //    //height = int.Parse(reader.GetAttribute("Height"));
    //    //movementCost = int.Parse(reader.GetAttribute("MovCost"));
    //
    //    
    //}
}




