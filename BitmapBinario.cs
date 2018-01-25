using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public class BitmapBinario : ElementoComplejoBinario
	{
		public BitmapBinario()
		{
			PartesElemento.Add(new ElementoIListBinario(ElementosTipoAceptado(Serializar.TiposAceptados.Byte), ElementoIListBinario.LongitudBinaria.Long));
		}

		public override object GetObject(MemoryStream bytes)
		{
			Object obj = null;
			if (bytes.ReadByte() != (byte)0x00) {
				bytes.Position--;
				obj = base.GetObject(bytes);
			}
			return obj;
		}

		protected override object GetObject(object[] parts)
		{
			Bitmap bmp = null;
			if (parts.Length == 1 && parts[0] is object[])
				bmp = Serializar.ToBitmap(((object[])parts[0]).CastingToByte());
			return bmp;
		}

		public override byte[] GetBytes(object obj)
		{
			List<byte> bytes = new List<byte>();
			Bitmap bmp = obj as Bitmap;
			if (bmp != null) {
				bytes.AddRange(Serializar.GetBytes(bmp));
				bytes.InsertRange(0, Serializar.GetBytes(Convert.ToInt64(bytes.Count)));
			}
			else
				bytes.Add((byte)0x0);
			return bytes.ToArray();
		}
	}
}


