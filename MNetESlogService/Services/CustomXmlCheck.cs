using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MNet.ESlog.Service.Services
{
	public class CustomXmlCheck: ICheck
	{
		public IList<string> Warnings { get; private set; }

		public IList<string> Errors { get; private set; }

		public XmlValidationResult ValidationResult { get; private set; }

		public bool IsSigned { get; private set; }

		public bool Execute(string xmlFileName)
		{
			// try verification without local schemas
			if (this.tryExecute(xmlFileName))
				return true;

			return false;
		}

		private bool tryExecute(string xmlFileName)
		{
			this.init();

			try
			{
				using (XmlReader reader = XmlReader.Create(xmlFileName))
				{
					while (reader.Read())
					{
						if (reader.LocalName == "Signature")
						{
							if (reader.IsStartElement())
							{
								this.IsSigned = true;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.ValidationResult = XmlValidationResult.Errors;
				this.Errors.Add(ex.Message);
				return false;
			}

			if (this.Errors.Count > 0) this.ValidationResult = XmlValidationResult.Errors;
			else if (this.Warnings.Count > 0) this.ValidationResult = XmlValidationResult.Warnings;
			else this.ValidationResult = XmlValidationResult.Valid;

			bool valid = this.ValidationResult == XmlValidationResult.Valid;
			this.IsSigned = this.IsSigned && valid;

			return valid;
		}

		private void init()
		{
			this.Warnings = new List<string>();
			this.Errors = new List<string>();
			this.IsSigned = false;
		}

		private class LoadLocalSchema
		{
			public string Namespace { get; private set; }

			public int Version { get; private set; }

			public LoadLocalSchema(string ns, int version)
			{
				this.Namespace = ns;
				this.Version = version;
			}
		}
	}
}