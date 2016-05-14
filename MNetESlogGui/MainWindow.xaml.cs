// <copyright file="MainWindow.xaml.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using MNet.ESlog.Service.Services;

namespace MNet.ESlog.Gui
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      this.InitializeComponent();
      this.init();

      this.DataContext = this;
    }

    private void init()
    {
      this.tbInput_alert.Visibility = Visibility.Collapsed;
      this.tbOutput_alert.Visibility = Visibility.Collapsed;
    }

    private string getFileName(bool newFile)
    {
      Microsoft.Win32.FileDialog dlg = null;

      if (newFile)
        dlg = new Microsoft.Win32.SaveFileDialog();
      else
        dlg = new Microsoft.Win32.OpenFileDialog();

      dlg.DefaultExt = ".XML";
      dlg.Filter = "XML Files (*.xml)|*.xml";

      bool? result = dlg.ShowDialog();
      if ((result.HasValue) && (result.Value))
        return dlg.FileName;

      return string.Empty;
    }

    private string saveFile()
    {
      Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

      dlg.DefaultExt = ".XML";
      dlg.Filter = "XML Files (*.xml)|*.xml";

      bool? result = dlg.ShowDialog();
      if ((result.HasValue) && (result.Value))
        return dlg.FileName;

      return string.Empty;
    }

    private X509Certificate2 LetUserChooseCertificate()
    {
      X509Certificate2 cert = null;

      try
      {
        // Open the store of personal certificates.
        X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        X509Certificate2Collection collection = store.Certificates;
        X509Certificate2Collection fcollection = collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
        X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "XAdES sample", "Choose a certificate", X509SelectionFlag.SingleSelection);

        if (scollection != null && scollection.Count == 1)
        {
          cert = scollection[0];

          if (cert.HasPrivateKey == false)
          {
            MessageBox.Show("To digitalno potrdilo nima privatnega ključa / This certificate does not have a private key associated with it");
            cert = null;
          }
        }

        store.Close();
      }
      catch (Exception)
      {
        MessageBox.Show("Ne morem pridobiti privatnega ključa / Unable to get the private key");
        cert = null;
      }

      return cert;
    }

    #region EventHandlers
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
      e.Handled = true;
    }

    private void btnSelectClick(object sender, RoutedEventArgs e)
    {
      if (sender == this.btnInput)
        this.tbInput.Text = this.getFileName(false);

      if (sender == this.btnOutput)
        this.tbOutput.Text = this.getFileName(true);

      if (sender == this.btnCertificate)
      {
        this.certificate = this.LetUserChooseCertificate();
        this.tbCertificate.Text = (this.certificate == null) ? string.Empty : this.certificate.Subject;
      }
    }

    private void btnSignClick(object sender, RoutedEventArgs e)
    {
      string xmlInput = this.tbInput.Text;
      Check check = new Check();
      if ((string.IsNullOrEmpty(xmlInput)) || (!File.Exists(xmlInput)) || (!check.Execute(xmlInput)))
      {
        MessageBox.Show("Napačna vhodna datoteka / Wrong input file");
        return;
      }

      string xmlOutput = this.tbOutput.Text;
      if (string.IsNullOrEmpty(xmlOutput))
      {
        MessageBox.Show("Napačna izhodna datoteka / Wrong output file");
        return;
      }

      if (this.certificate == null)
      {
        MessageBox.Show("Izberite digitalno potrdilo / Select certificate");
        return;
      }

      try
      {
        Sign sign = new Sign();
        if (sign.Execute(xmlInput, xmlOutput, this.certificate, DateTime.Now))
        {
          Process.Start(new ProcessStartInfo($"file:///{xmlOutput}"));
          MessageBox.Show("Dokument je digitalno podpisan / Document is signed");
        }
        else
          MessageBox.Show("Prišlo je do neznane napake / Unknown error occurred");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void btnCloseClick(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void tbTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
      if (sender == this.tbInput)
      {
        string selectedFileName = this.tbInput.Text;
        this.tbInput_alert.Visibility = Visibility.Collapsed;
        if ((!string.IsNullOrEmpty(selectedFileName)) && (File.Exists(selectedFileName)))
        {
          Check check = new Check();
          bool valid = check.Execute(selectedFileName);
          this.tbInput_alert.Foreground = (valid) ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.Red);
          this.tbInput_alert.Text = (valid) ? "Dokument je že podpisan / The document is already signed" : "To ni ustrezen dokument / Not a valid file";
          if ((!valid) || ((valid) && (check.IsSigned)))
            this.tbInput_alert.Visibility = (File.Exists(selectedFileName)) ? Visibility.Visible : Visibility.Collapsed;
        }
      }

      if (sender == this.tbOutput)
      {
        string selectedFileName = this.tbOutput.Text;
        if (string.IsNullOrEmpty(selectedFileName))
          this.tbOutput_alert.Visibility = Visibility.Collapsed;
        else
          this.tbOutput_alert.Visibility = (File.Exists(selectedFileName)) ? Visibility.Visible : Visibility.Collapsed;
      }
    }
    #endregion EventHandlers

    private X509Certificate2 certificate = null;
  }
}
