using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat.Binaris
{
   public static class ReadBloque
    {
       public static byte[] ReadBytes(this FileInfo archivo, long posicionInicio, long cantidad)
       {
           FileStream fs = new FileStream(archivo.FullName, FileMode.Open, FileAccess.Read);
           byte[] buffer;
           fs.Position = posicionInicio;
           buffer = fs.Read(cantidad);
           fs.Close();
           return buffer;
       }
    }
}
