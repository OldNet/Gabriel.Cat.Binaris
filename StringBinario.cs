using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public class StringBinario : ElementoIListBinario<char>
	{
		public StringBinario() : base(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.Char), LongitudBinaria.ULong)
		{
		}

		public StringBinario(byte[] marcaFin) : base(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.Char), marcaFin)
		{
		}

		public override object GetObject(MemoryStream bytes)
		{
			return new string((char[])base.GetObject(bytes));
		}

		public override byte[] GetBytes(object obj)
		{
			string str = obj as string;
			if(str==null)
				throw new ArgumentException(String.Format("Se tiene que serializar {0}","".GetType().FullName));
			return base.GetBytes(str.ToCharArray());
		}
		public override string ToString()
		{
			return "TipoDatos=String";
		}
	}
}


