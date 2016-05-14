// <copyright file="XmlHelperFunctions.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System.Xml;

namespace MNet.ESlog.Service.Utils
{
  internal class XmlHelperFunctions
  {
    public static XmlNode GetSubNode(XmlElement element, string fullNodeName)
    {
      XmlNodeList nodeList = element.GetElementsByTagName(fullNodeName);
      if (nodeList.Count != 1) return null;

      return nodeList[0];
    }

    public static XmlDocument CreateNewXmlDocument(string fileName = null)
    {
      XmlDocument xml = new XmlDocument();
      xml.PreserveWhitespace = true;

      if (!string.IsNullOrEmpty(fileName))
        xml.Load(fileName);

      return xml;
    }
  }
}