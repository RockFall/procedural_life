using System.Collections;
using System.Collections.Generic;
using UnityEngine; // Only for Debug and Mathf
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;


public class LivingEntity : IXmlSerializable
{

    public CoreEntity CoreData { get; protected set; }
    public DNA DNA { get; protected set; }


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
        // Serialize BaseData
        if (CoreData != null)
        {
            writer.WriteStartElement("CoreData");
            CoreData.WriteXml(writer);
            writer.WriteEndElement();
        }

        // Serialize DNA
        if (DNA != null)
        {
            writer.WriteStartElement("DNA");
            DNA.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    public void ReadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.IsStartElement())
            {
                switch (reader.Name)
                {
                    case "BaseData":
                        CoreData = new CoreEntity();
                        //BaseData.ReadXml(reader);
                        break;
                    case "DNA":
                        DNA = new DNA();
                        //DNA.ReadXml(reader);
                        break;
                }
            }
        }
    }
}
