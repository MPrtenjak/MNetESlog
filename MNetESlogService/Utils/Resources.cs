// <copyright file="Resources.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MNet.ESlog.Service.Utils
{
  internal class Resources
  {
    public static Stream Get(string name)
    {
      return typeof(Resources).Assembly.GetManifestResourceStream(@"MNet.ESlog.Service.Resources." + name);
    }
  }
}
