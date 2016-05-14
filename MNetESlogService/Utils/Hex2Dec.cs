// <copyright file="Hex2Dec.cs" company="MNet">
//     Copyright (c) Matjaz Prtenjak All rights reserved.
// </copyright>
// <author>Matjaz Prtenjak</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace MNet.ESlog.Service.Utils
{
  internal class Hex2Dec
  {
    // function from https://stackoverflow.com/questions/16965915
    public static string Convert(string hex)
    {
      List<int> dec = new List<int> { 0 };   // decimal result

      foreach (char c in hex)
      {
        int carry = System.Convert.ToInt32(c.ToString(), 16);

        // initially holds decimal value of current hex digit;
        // subsequently holds carry-over for multiplication
        for (int i = 0; i < dec.Count; ++i)
        {
          int val = (dec[i] * 16) + carry;
          dec[i] = val % 10;
          carry = val / 10;
        }

        while (carry > 0)
        {
          dec.Add(carry % 10);
          carry /= 10;
        }
      }

      var chars = dec.Select(d => (char)('0' + d));
      var cArr = chars.Reverse().ToArray();
      return new string(cArr);
    }
  }
}
