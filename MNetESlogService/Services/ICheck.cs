// <copyright file="Check.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace MNet.ESlog.Service.Services
{
	public interface ICheck
	{
		IList<string> Errors { get; }
		bool IsSigned { get; }
		XmlValidationResult ValidationResult { get; }
		IList<string> Warnings { get; }

		bool Execute(string xmlFileName);
	}
}