using Gabriel.Cat.Binaris;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Extension
{
   public static class ClaseExtension
    {
       public static void SetBytes( this IElementoBinario elemento, byte[] bytes)
       {
           MemoryStream ms = new MemoryStream(bytes);
           elemento.SetBytes(ms);
           ms.Close();

       }
    }
}
