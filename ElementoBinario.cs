using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public abstract class ElementoBinario
	{
		public byte[] GetBytes()
		{
			return GetBytes(this);
		}
		public abstract byte[] GetBytes(object obj);

		public object GetObject(byte[] bytes)
		{
			return GetObject(new MemoryStream(bytes));
		}

		public abstract object GetObject(MemoryStream bytes);

		public static ElementoBinario ElementosTipoAceptado(Serializar.TiposAceptados tipo)
		{
			ElementoBinario elemento;
			switch (tipo) {
				case Serializar.TiposAceptados.String:
					elemento = new StringBinario();
					break;
				case Serializar.TiposAceptados.Bitmap:
					elemento = new BitmapBinario();
					break;
				default:
					elemento = new ElementoBinarioTamañoFijo(tipo);
					break;
			}
			return elemento;
		}
	}
}


