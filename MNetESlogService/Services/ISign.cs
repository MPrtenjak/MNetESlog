// <copyright file="Sign.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace MNet.ESlog.Service.Services
{
	public interface ISign
	{
		bool Execute(string xmlInputFileName, string xmlOutputFileName, X509Certificate2 certificate, DateTime timeStamp);
		XmlDocument Execute(string xmlFileName, X509Certificate2 certificate, DateTime timeStamp);
	}
}