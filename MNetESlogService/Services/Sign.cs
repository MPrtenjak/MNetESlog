// <copyright file="Sign.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using MNet.ESlog.Service.Utils;

namespace MNet.ESlog.Service.Services
{
  public class Sign
  {
    public XmlDocument Execute(string xmlFileName, X509Certificate2 certificate, DateTime timeStamp)
    {
      this.xmlDocument = new XmlDocument
      {
        PreserveWhitespace = true
      };
      this.xmlDocument.Load(xmlFileName);

      this.certificate = certificate;
      this.timeStamp = timeStamp;

      this.removeCurrentSignature();
      string racunID = this.addIdAttribute();

      int step = 1;
      SignedXml signedXml = this.getSignedXml(racunID, step);
      signedXml.ComputeSignature();
      this.xmlDocument.DocumentElement.AppendChild(this.xmlDocument.ImportNode(signedXml.GetXml(), true));
      if (!signedXml.CheckSignature())
        return null;

      step = 2;
      signedXml = this.getSignedXml(racunID, step);
      signedXml.ComputeSignature();
      var oldNode = this.xmlDocument.DocumentElement.SelectSingleNode("//*[local-name()='Signature']");
      var newNode = this.xmlDocument.ImportNode(signedXml.GetXml(), true);
      this.xmlDocument.DocumentElement.ReplaceChild(newNode, oldNode);
      if (!signedXml.CheckSignature())
        return null;

      return this.xmlDocument;
    }

    public bool Execute(string xmlInputFileName, string xmlOutputFileName, X509Certificate2 certificate, DateTime timeStamp)
    {
      Sign sign = new Sign();
      XmlDocument doc = sign.Execute(xmlInputFileName, certificate, timeStamp);
      if (doc == null) return false;

      doc.Save(xmlOutputFileName);
      return true;
    }

    private void removeCurrentSignature()
    {
      var signature = this.xmlDocument.DocumentElement.SelectSingleNode("//*[local-name()='Signature']");
      if (signature != null)
        signature.ParentNode.RemoveChild(signature);
    }

    private string addIdAttribute()
    {
      var racun = this.xmlDocument.DocumentElement.SelectSingleNode("//*[local-name()='Racun']");
      if (racun == null)
      {
        racun = this.xmlDocument.DocumentElement.SelectSingleNode("//*[local-name()='M_INVOIC']");
        if (racun == null)
          throw new Exception("Dokument je neustrezen / Document is not valid");
      }

      var Id = racun.Attributes["Id"];
      if (Id != null)
        return Id.Value;

      var idAttr = this.xmlDocument.CreateAttribute("Id");
      idAttr.Value = "data";
      racun.Attributes.Append(idAttr);

      return idAttr.Value;
    }

    private SignedXml getSignedXml(string idReference, int step)
    {
      SignedXml signedXml = new SignedXml(this.xmlDocument);
      signedXml.Signature.Id = "SignatureId";

      signedXml.SigningKey = this.certificate.PrivateKey;
      signedXml.KeyInfo = this.getKeyInfo();
      signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
      signedXml.AddReference(this.addRacunReference(idReference));

      if (step == 1)
        signedXml.AddObject(this.createQualifyingProperties());
      else
      {
        signedXml.AddObject(this.readQualifyingProperties());
        signedXml.AddReference(this.addSignedPropertiesReference());
      }

      return signedXml;
    }

    private Reference addSignedPropertiesReference()
    {
      return this.addReference("http://uri.etsi.org/01903/v1.1.1#SignedProperties", "#SignedPropertiesId");
    }

    private Reference addRacunReference(string idReference)
    {
      return this.addReference("http://www.gzs.si/shemas/eslog/racun/1.6#Racun", $"#{idReference}");
    }

    private Reference addReference(string type, string uri)
    {
      return new Reference
      {
        Type = type,
        Uri = uri
      };
    }

    private KeyInfo getKeyInfo()
    {
      KeyInfoX509Data keyInfoX509Data = new KeyInfoX509Data(this.certificate, X509IncludeOption.ExcludeRoot);
      if (keyInfoX509Data.Certificates.Count != 1)
        keyInfoX509Data = new KeyInfoX509Data(this.certificate, X509IncludeOption.EndCertOnly);

      KeyInfo keyInfo = new KeyInfo();
      keyInfo.AddClause(keyInfoX509Data);
      return keyInfo;
    }

    private DataObject createQualifyingProperties()
    {
      SHA1 cryptoServiceProvider = new SHA1CryptoServiceProvider();
      byte[] sha1 = cryptoServiceProvider.ComputeHash(this.certificate.RawData);

      XNamespace xds = "http://uri.etsi.org/01903/v1.1.1#";
      XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
      XElement qualifyingProperties = new XElement("Object",
        new XAttribute(XNamespace.Xmlns + "xds", xds),
        new XAttribute(XNamespace.Xmlns + "ds", ds),
        new XElement(xds + "QualifyingProperties",
          new XAttribute("Target", "#SignatureId"),
          new XElement(xds + "SignedProperties",
            new XAttribute("Id", "SignedPropertiesId"),
            new XElement(xds + "SignedSignatureProperties",
              new XElement(xds + "SigningTime", this.timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
              new XElement(xds + "SigningCertificate",
                new XElement(xds + "Cert",
                  new XElement(xds + "CertDigest",
                    new XElement(xds + "DigestMethod", new XAttribute("Algorithm", SignedXml.XmlDsigSHA1Url)),
                    new XElement(xds + "DigestValue", Convert.ToBase64String(sha1))),
                  new XElement(xds + "IssuerSerial",
                    new XElement(ds + "X509IssuerName", this.certificate.IssuerName.Name),
                    new XElement(ds + "X509SerialNumber", Hex2Dec.Convert(this.certificate.SerialNumber))))),
              new XElement(xds + "SignaturePolicyIdentifier",
                new XElement(xds + "SignaturePolicyImplied", string.Empty))))));

      DataObject dataObject = new DataObject
      {
        Data = qualifyingProperties.GetXmlNode().FirstChild.SelectNodes("."),
      };

      return dataObject;
    }

    private DataObject readQualifyingProperties()
    {
      var qualifyingProperties = this.xmlDocument.DocumentElement.SelectSingleNode("//*[local-name()='QualifyingProperties']");

      DataObject dataObject = new DataObject
      {
        Data = qualifyingProperties.SelectNodes("."),
      };

      return dataObject;
    }

    private XmlDocument xmlDocument;
    private X509Certificate2 certificate;
    private DateTime timeStamp;
  }
}