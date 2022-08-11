// <copyright file="Sign.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MNet.ESlog.Service.Utils;

namespace MNet.ESlog.Service.Services
{
	public class CustomXmlSign : ISign
	{
		public XmlDocument Execute(string xmlFileName, X509Certificate2 certificate, DateTime timeStamp)
		{
			this.xmlDocument = new XmlDocument
			{
				PreserveWhitespace = false
			};
			this.xmlDocument.Load(new XmlTextReader(xmlFileName));

			this.certificate = certificate;
			this.timeStamp = timeStamp;

			this.removeCurrentSignature();

			SignedXml signedXml = this.getSignedXml();
			signedXml.ComputeSignature();
			this.xmlDocument.DocumentElement.AppendChild(this.xmlDocument.ImportNode(signedXml.GetXml(), true));

			SignedXml signedXmlCheck = new SignedXml(this.xmlDocument);
			XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");
			signedXmlCheck.LoadXml((XmlElement)nodeList[0]);
			if (!signedXmlCheck.CheckSignature())
				throw new Exception("Podpis ni veljaven / Signature not valid");

			return this.xmlDocument;
		}

		public bool Execute(string xmlInputFileName, string xmlOutputFileName, X509Certificate2 certificate, DateTime timeStamp)
		{
			ISign sign = new CustomXmlSign();
			XmlDocument doc = sign.Execute(xmlInputFileName, certificate, timeStamp);
			if (doc == null) return false;

			if (doc.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
				doc.RemoveChild(doc.FirstChild);

			XmlTextWriter xmltw = new XmlTextWriter(xmlOutputFileName, new UTF8Encoding(false));
			doc.WriteTo(xmltw);
			xmltw.Close();

			return true;
		}

		private void removeCurrentSignature()
		{
			var signature = this.xmlDocument.DocumentElement.SelectSingleNode("//*[local-name()='Signature']");
			if (signature != null)
				signature.ParentNode.RemoveChild(signature);
		}

		private SignedXml getSignedXml()
		{
			var signedXml = new SignedXml(this.xmlDocument);
			signedXml.Signature.Id = "SignatureId";

			signedXml.SigningKey = this.certificate.PrivateKey;

			var xmlSignature = signedXml.Signature;
			Reference reference = new Reference("");
			XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
			reference.AddTransform(env);
			xmlSignature.SignedInfo.AddReference(reference);
			xmlSignature.KeyInfo = this.getKeyInfo();

			signedXml.AddObject(this.createQualifyingProperties());

			return signedXml;
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

		private XmlDocument xmlDocument;
		private X509Certificate2 certificate;
		private DateTime timeStamp;
	}
}