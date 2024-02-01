using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using UnityEngine;

public class EPosition : IXmlSerializable
{
    public Vector3 Position 
    {  
        get
        {
            return Position;
        }
        set
        {
            //coord = value.position;
            //rotation = value.rotation;
            Tile = WorldController.Instance.GetTileAtWorldCoord(new Vector3(value.x, value.y));
            Position = value;
        }
    }
    //public Vector3 coord
    //{
    //    get
    //    {
    //        return coord;
    //    }
    //    set
    //    {
    //        coord = value;
    //        transform.position = value;
    //        Tile = WorldController.Instance.world.GetTileAt(Mathf.FloorToInt(coord.x), Mathf.FloorToInt(coord.y));
    //    }
    //}
    //public Quaternion rotation
    //{
    //    get
    //    {
    //        return rotation;
    //    }
    //    set
    //    {
    //        rotation = value;
    //        transform.rotation = value;
    //    }
    //}
    public Tile Tile
    {
        get
        {
            return Tile;
        }
        set
        {
            Tile = value;
            Position = WorldController.Instance.GetWorldPositionOfTile(value);
        }
    }

    public EPosition(Vector3 position)
    {
        Position = position;
        //coord = transform.position;
        //rotation = transform.rotation;
        Tile = WorldController.Instance.GetTileAtWorldCoord(new Vector3(Position.x, Position.y));
    }


    ////////////////////////////////////////////////////////////////////////////////////
    ///                                                                              ///
    ///                            SAVING & LOADING                                  ///
    ///                                                                              ///
    ////////////////////////////////////////////////////////////////////////////////////
    
    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        //coord = new Vector3(float.Parse(reader.GetAttribute("x")), float.Parse(reader.GetAttribute("y")), float.Parse(reader.GetAttribute("z")));
        //rotation = new Quaternion(float.Parse(reader.GetAttribute("x")), float.Parse(reader.GetAttribute("y")), float.Parse(reader.GetAttribute("z")), float.Parse(reader.GetAttribute("w")));
        int x = int.Parse(reader.GetAttribute("x"));
        int y = int.Parse(reader.GetAttribute("y"));
        int z = int.Parse(reader.GetAttribute("z"));
        Position = new Vector3(x, y, z);
        Tile = WorldController.Instance.GetTileAtWorldCoord(new Vector3(Position.x, Position.y));
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("x", Position.x.ToString());
        writer.WriteAttributeString("y", Position.y.ToString());
        writer.WriteAttributeString("z", Position.z.ToString());
    }

}
