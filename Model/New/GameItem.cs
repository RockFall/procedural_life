using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameItem
{
    public string Name { get; protected set; }
    public int Quantity
    {
        get { return this.Quantity; }
        set
        {
            if (this.Quantity != value)
            {
                this.Quantity = value;
                if (cbItemQuantityChanged != null)
                {
                    cbItemQuantityChanged(this);
                }
            }
        }
    }
    public int MaxStackSize { get; protected set; }

    public Tile Tile { get; protected set; }
    // Inventory it belongs to

    // The function we callback any time our inventory's data changes.
    public Action<GameItem> cbItemQuantityChanged;

    public bool IsReal { get; protected set; }

    // Empty constructor for loading and saving reasons.
    public GameItem()
    {
            
    }

    // Normal constructor.
    public GameItem(string name, int amount, int maxStackSize, bool isReal=true)
    {
        this.Name = name;
        this.Quantity = amount;
        this.MaxStackSize = maxStackSize;
        this.IsReal = isReal;
    }

    // Copy constructor.
    protected GameItem(GameItem other)
    {
        Name = other.Name;
        Quantity = other.Quantity;
        MaxStackSize = other.MaxStackSize;
        IsReal = other.IsReal;
    }

    virtual public GameItem Clone()
    {
        return new GameItem(this);
    }

    /// <summary>
    /// Register a function to be called back when our inventory type changes.
    /// </summary>
    public void RegisterItemQuantityChangedCallback(Action<GameItem> callback)
    {
        cbItemQuantityChanged += callback;
    }
    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterItemQuantityChangedCallback(Action<GameItem> callback)
    {
        cbItemQuantityChanged -= callback;
    }



}
