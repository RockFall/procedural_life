using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using UnityEngine;

public class CoreEntity //: IXmlSerializable
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public string ID { get; protected set; }
    public EPosition Position { get; protected set; }

    ////////////////////////////////////////////////////////////////////////////////////
    ///                                                                              ///
    ///                            SAVING & LOADING                                  ///
    ///                                                                              ///
    ////////////////////////////////////////////////////////////////////////////////////

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // Serialize Name
        writer.WriteAttributeString("Name", Name);

        // Serialize Description
        writer.WriteAttributeString("Description", Description);

        // Serialize ID
        writer.WriteAttributeString("ID", ID);

        // Serialize Position
        writer.WriteStartElement("EPosition");
        Position.WriteXml(writer);
        writer.WriteEndElement();
    }
}
