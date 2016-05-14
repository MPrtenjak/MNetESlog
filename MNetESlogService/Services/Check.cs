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

      // try with local schema
      if (this.loadschema != null)
        return this.tryExecute(xmlFileName);

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
          this.continueProcess = true;
          while (reader.Read() && this.continueProcess)
          {
            if (reader.IsStartElement())
            {
              if (reader.LocalName == "Signature")
                this.IsSigned = true;
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

      this.settings = new XmlReaderSettings();
      this.settings.ValidationType = ValidationType.Schema;
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
      if (this.loadschema == null)
        return;

      XmlSchemaSet schemas = new XmlSchemaSet();

      if (this.loadschema.Version == 5)
        schemas.Add(this.loadschema.Namespace, new XmlTextReader(Resources.Get("eSLOG_1-5_EnostavniRacun.xsd")));
      else
        schemas.Add(this.loadschema.Namespace, new XmlTextReader(Resources.Get("eSLOG_1-6_EnostavniRacun.xsd")));

      this.settings.Schemas = schemas;
    }

    private void validationCallBack(object sender, ValidationEventArgs args)
    {
      if (args.Severity == XmlSeverityType.Warning)
      {
        // if eslog schema not found, try local schema
        if (args.Message.Contains("eSLOG"))
        {
          if (this.loadschema != null) return;

          int version = args.Message.Contains("eSLOG_1-5") ? 5 : 6;
          this.loadschema = new LoadLocalSchema(string.Empty, version);
          this.continueProcess = false;
          return;
        }

        this.Warnings.Add(args.Message);
      }
      else
        this.Errors.Add(args.Message);
    }

    private bool continueProcess = true;
    private XmlReaderSettings settings = new XmlReaderSettings();
    private LoadLocalSchema loadschema = null;
  }
}
