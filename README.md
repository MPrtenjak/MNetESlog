# MNetESlog

## English

### Purpose

This is .NET library for signing Slovenian e-documents using e-SLOG version 1.5, 1.6 or 2.0.

From 2022 you can also sign any XML (non eSlog) documents.

With its help you can:

1. Sign e-document
2. Verify that the e-document is compliant with the e-SLOG

### .NET version

You only need .NET 3.5 or later, because the library does not have any other external references

### Using

#### Signing the document

To sign the document, you need an unsigned document that is compliant with the e-SLOG 1.5, e-SLOG 1.6 or e-SLOG 2.0 and the certificate with which you want to sign the document. Usage is extremely simple:

If you need signed XmlDocument object:

```C#
private XmlDocument doSignIntoXmlDoc()
{
  // create signing object
  ISign sign = new ESlogSign();

  // select unsigned document
  string unsignedDoc = @"c:\documents\unsigned.xml";

  // select certificate
  X509Certificate2 certificate = getMyCertificate();

  // sign the document
  return sign.Execute(unsignedDoc, certificate, DateTime.Now);
}
```

If you only need signed document on disk:

```C#
private void doSignIntoFile()
{
  // create signing object
  ISign sign = new ESlogSign();

  // select unsigned document
  string unsignedDoc = @"c:\documents\unsigned.xml";

  // set (output) signed dokument
  string signedDoc = @"c:\documents\signed.xml";

  // select certificate
  X509Certificate2 certificate = getMyCertificate();

  // sign the document
  sign.Execute(unsignedDoc, signedDoc, certificate, DateTime.Now);
}
```

#### Verification of compliance with the e-SLOG

If you want to check whether a document is consistent with the e-SLOG scheme, perform the following:

```C#
private void doCheck()
{
  // create signing object
  ICheck check = new ESlogCheck();

  // select unsigned document
  string someDocument = @"c:\documents\document.xml";

  if (check.Execute(someDocument))
  {
    // Document is valid

    // Use (check.IsSigned) to see if it is signed
  }
  else
  {
    // Dokument is not valid

    Console.WriteLine("Errors : ");
    foreach (var error in check.Errors)
      Console.WriteLine(error);

    Console.WriteLine("Warnings : ");
    foreach (var warning in check.Warnings)
      Console.WriteLine(warning);
  }
}
```

### Test program

For testing purposes, you can use a graphical test program MNetESlogGui:

![Alt desc](https://raw.githubusercontent.com/MPrtenjak/MNetESlog/master/gui.png)

### Testing online

To test documents on the web, you can use two services:

2.	http://proxsign.setcce.si/proXsign/?
3.	http://www.eracuni.si/vizualizacija

### Special thanks

I got the idea for a solution through questions on codeproject: http://www.codeproject.com/Questions/338697/csharp-problem-Signing-XML-documents-with-XAdES

## Slovensko

### Namen

To je .NET knjižnica, ki je namenjena podpisovanju slovenskih e-dokumentov, ki uporabljajo e-SLOG verzije 1.5, 1.6 ali 2.0.

Od leta 2022 lahko podpisujete tudi vse dokumente XML (ne eSlog).

Z njeno pomočjo lahko

1.	Podpišete e-dokument
2.	Preverite ali je e-dokument skladen s shemo e-SLOG

### Verzije .NET

Za uporabo potrebujete samo .NET 3.5 ali novejši, saj knjižnica nima preostalih zunanjih referenc

### Uporaba

#### Podpis dokumenta

Za podpis dokumenta potrebujete nepodpisan dokument, ki je skladen s shemo e-SLOG 1.5, e-SLOG 1.6 ali e-SLOG 2.0, ter certifikat s katerim želite dokument podpisati. Uporaba je skrajno preprosta.

V kolikor potrebujete podpisan objekt XmlDocument izvedete sledečo kodo

```C#
private XmlDocument doSignIntoXmlDoc()
{
  // ustvarite objekt za podpisovanje
  ISign sign = new ESlogSign();

  // določite vhoden, nepodpisan dokument
  string unsignedDoc = @"c:\documents\unsigned.xml";

  // izberete certifikat
  X509Certificate2 certificate = getMyCertificate();

  // podpišete dokument
  return sign.Execute(unsignedDoc, certificate, DateTime.Now);
}
```

V kolikor pa preprosto želite samo podpisati dokument v drugo datoteko, pa poveste še ime izhodne datoteke

```C#
private void doSignIntoFile()
{
  // ustvarite objekt za podpisovanje
  ISign sign = new ESlogSign();

  // določite vhoden, nepodpisan dokument
  string unsignedDoc = @"c:\documents\unsigned.xml";

  // določite izhoden, podpisan dokument
  string signedDoc = @"c:\documents\signed.xml";

  // izberete certifikat
  X509Certificate2 certificate = getMyCertificate();

  // podpišete dokument
  sign.Execute(unsignedDoc, signedDoc, certificate, DateTime.Now);
}
```

#### Preverjanje skladnosti z e-SLOG

V kolikor želite preveriti ali je nek dokument skladen s e-SLOG shemo, izvedite sledeče:

```C#
private void doCheck()
{
  // ustvarite objekt za preverjanje skladnosti sheme
  ICheck check = new ESlogCheck();

  // izberete xml datoteko
  string someDocument = @"c:\documents\document.xml";

  if (check.Execute(someDocument))
  {
    // Dokument je skladen!

    // Ali je podpisan vidite če uporabite check.IsSigned
  }
  else
  {
    // Dokument ni skladen!

    Console.WriteLine("Napake : ");
    foreach (var error in check.Errors)
      Console.WriteLine(error);

    Console.WriteLine("Opozorila : ");
    foreach (var warning in check.Warnings)
      Console.WriteLine(warning);
  }
}
```

### Testni program

Za potrebe testa lahko uporabite tudi testni grafični program MNetESlogGui:

![Alt desc](https://raw.githubusercontent.com/MPrtenjak/MNetESlog/master/gui.png)

### Test na spletu

Za testiranje dokumentov na spletu lahko uporabite dve storitvi:

1.	http://proxsign.setcce.si/proXsign/?
2.	http://www.eracuni.si/vizualizacija

### Posebna zahvala

Idejo za rešitev sem dobil preko vprašanja na codeproject: http://www.codeproject.com/Questions/338697/csharp-problem-Signing-XML-documents-with-XAdES
