// <copyright file="Check.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using MNet.ESlog.Service.Utils;

namespace MNet.ESlog.Service.Services
{
  public class Check
  {
    public enum XmlValidationResult
    {
      Valid,
      Warnings,
      Errors,
    }

    public IList<string> Warnings { get; private set; }

    public IList<string> Errors { get; private set; }

    public XmlValidationResult ValidationResult { get; private set; }

    public bool IsSigned { get; private set; }

    public bool Execute(string xmlFileName)
    {
      // try verification without local schemas
      if (this.tryExecute(xmlFileName))
        return true;

      // try with local schemas
      // eSlog2.0
      this.schemaID = 20;
      if (this.tryExecute(xmlFileName))
        return true;

      // eSlog1.6
      this.schemaID = 6;
      if (this.tryExecute(xmlFileName))
        return true;

      // eSlog1.5
      this.schemaID = 5;
      if (this.tryExecute(xmlFileName))
        return true;

      return false;
    }

    private bool tryExecute(string xmlFileName)
    {
      this.init();
      this.addLocalSchema();

      try
      {
        using (XmlReader reader = XmlReader.Create(xmlFileName, this.settings))
        {
          readerInSignatureElement = false;
          while (reader.Read())
          {
            if (reader.LocalName == "Signature")
            {
              if (reader.IsStartElement())
              {
                this.IsSigned = true;
                readerInSignatureElement = true;
              }

              if (reader.NodeType == XmlNodeType.EndElement)
                readerInSignatureElement = false;
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

      this.settings = new XmlReaderSettings
      {
        ValidationType = ValidationType.Schema
      };
      this.settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
      this.settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
      this.settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
      this.settings.ValidationEventHandler += new ValidationEventHandler(this.validationCallBack);
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

    private void addLocalSchema()
    {
      if (this.schemaID == null)
        return;

      XmlSchemaSet schemas = new XmlSchemaSet();

      if (this.schemaID == 20)
      {
        schemas.Add("urn:eslog:2.00", new XmlTextReader(Resources.Get("eSLOG20_INVOIC_v200.xsd")));
        schemas.Add("http://www.w3.org/2000/09/xmldsig#", new XmlTextReader(Resources.Get("xmldsig-core-schema.xsd")));
      }
      else if (this.schemaID == 6)
        schemas.Add("", new XmlTextReader(Resources.Get("eSLOG_1-6_EnostavniRacun.xsd")));
      else
        schemas.Add("", new XmlTextReader(Resources.Get("eSLOG_1-5_EnostavniRacun.xsd")));

      this.settings.Schemas = schemas;
    }

    private void validationCallBack(object sender, ValidationEventArgs args)
    {
      if (args.Severity == XmlSeverityType.Warning)
      {
        // ignoring warnings stating that schemas can't be found
        // and ignoring all warnings in Signature element
        if ((!args.Message.StartsWith("Cannot load the schema")) && (!readerInSignatureElement))
          this.Warnings.Add(args.Message);
      }
      else
        this.Errors.Add(args.Message);
    }

    private bool readerInSignatureElement;
    private XmlReaderSettings settings = new XmlReaderSettings();
    private int? schemaID = null;
  }
}