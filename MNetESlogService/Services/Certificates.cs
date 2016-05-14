// <copyright file="Certificates.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace MNet.ESlog.Service.Services
{
  public class Certificates
  {
    public X509Certificate2 GetBySerialNumber(string serialNumber)
    {
      var matchingCertificates = this.findAllCertificatesInAllStores(this.allStores(), (store) => { return store.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber, true); });

      return this.getSingleCertificate(matchingCertificates);
    }

    public X509Certificate2 GetBySerialNumber(string serialNumber, StoreLocation storeLocation, StoreName storeName)
    {
      X509Store store = new X509Store(storeName, storeLocation);
      var matchingCertificates = this.findAllCertificatesInStore(store, (s) => { return s.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber, true); });

      return this.getSingleCertificate(matchingCertificates);
    }

    public X509Certificate2 GetFromFile(string certificateFile, string password)
    {
      if (!File.Exists(certificateFile))
        throw new Exception("Ne najdem digitalnega potrdila / Can't find certificate");

      return new X509Certificate2(certificateFile, password);
    }

    public X509Certificate2Collection GetAllCertificates()
    {
      var matchingCertificates = this.findAllCertificatesInAllStores(this.allStores(), (s) => { return s.Certificates; });

      return matchingCertificates;
    }

    private X509Certificate2Collection findAllCertificatesInStore(X509Store store, Func<X509Store, X509Certificate2Collection> searchAction)
    {
      store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
      var matchingCertificates = searchAction(store);
      store.Close();

      return matchingCertificates;
    }

    private X509Certificate2Collection findAllCertificatesInAllStores(IList<X509Store> stores, Func<X509Store, X509Certificate2Collection> searchAction)
    {
      X509Certificate2Collection matchingCertificates = new X509Certificate2Collection();

      foreach (var store in stores)
        matchingCertificates.AddRange(this.findAllCertificatesInStore(store, searchAction));

      return matchingCertificates;
    }

    private X509Certificate2Collection filterCertificates(X509Certificate2Collection certificates, Func<X509Certificate2, bool> filter)
    {
      X509Certificate2Collection matchingCertificates = new X509Certificate2Collection();

      foreach (X509Certificate2 cert in certificates)
      {
        if (filter(cert))
          matchingCertificates.Add(cert);
      }

      return matchingCertificates;
    }

    private X509Certificate2 getSingleCertificate(X509Certificate2Collection certificates)
    {
      if ((certificates == null) || (certificates.Count < 1))
        throw new Exception("Ne najdem digitalnega potrdila / Can't find certificate");

      if (certificates.Count > 1)
        throw new Exception("Digitalno potrdilo ni edinstveno / Certificate not unique");

      return certificates[0];
    }

    private IList<X509Store> allStores()
    {
      return new List<X509Store>()
      {
        new X509Store(StoreLocation.CurrentUser),
        new X509Store(StoreLocation.LocalMachine)
      };
    }
  }
}