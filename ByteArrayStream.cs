using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Binaris
{//cuando convierto  la stream creo una? o uso la que me pasan??? hago que sean independientes o no....
   public class ByteArrayStream
    {
        MemoryStream ms;
        public ByteArrayStream(byte[] bytes,bool isWritable=false)
        { ms =new MemoryStream(bytes, isWritable); }
        public ByteArrayStream(MemoryStream ms,bool createNewStream=false,bool isWritable=false)
        {
            if (ms == null) throw new ArgumentNullException();
            if (createNewStream)
                this.ms = new MemoryStream(ms.GetBuffer(),isWritable);
            else
                this.ms = ms;
        }
        public byte[] Datos
        {
            get {return ms.GetBuffer(); }
        }
        public MemoryStream StramBase
        {
            get { return ms; }
        }
        /// <summary>
        /// Cierra la stream base
        /// </summary>
        public void Close()
        {
            ms.Close();
        }
        public static implicit operator ByteArrayStream(MemoryStream stream)
        {
            return new ByteArrayStream(stream);
        }
        public static implicit operator MemoryStream(ByteArrayStream stream)
        {
            return stream.StramBase;
        }
        public static explicit operator byte[](ByteArrayStream stream)
        {
            return stream.Datos;
        }
    }
}
