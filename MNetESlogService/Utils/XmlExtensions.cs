// <copyright file="XmlExtensions.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace MNet.ESlog.Service.Utils
{
  public static class XmlExtensions
  {
    public static XElement GetXElement(this XmlNode node)
    {
      XDocument xDoc = new XDocument();
      using (XmlWriter xmlWriter = xDoc.CreateWriter())
        node.WriteTo(xmlWriter);
      return xDoc.Root;
    }

    public static XmlNode GetXmlNode(this XElement element)
    {
      using (XmlReader xmlReader = element.CreateReader())
      {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlReader);
        return xmlDoc.DocumentElement;
      }
    }
  }
}
