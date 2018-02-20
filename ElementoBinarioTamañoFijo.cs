using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	/// <summary>
	/// los tipos que no son variables se serializan con esta clase
	/// </summary>
	public class ElementoBinarioTamañoFijo : ElementoBinario
	{
		Serializar.TiposAceptados tipoDatos;

		public ElementoBinarioTamañoFijo(Serializar.TiposAceptados tipo)
		{
			TipoDatos = tipo;
		}

		public Serializar.TiposAceptados TipoDatos {
			get {
				return tipoDatos;
			}
			set {
				if (value < Serializar.TiposAceptados.Null)
					//lo menores son de longitud variable
					throw new TipoException(String.Format("el tipo {0} no es un tipo de longitud fija ", value.ToString()));
				tipoDatos = value;
			}
		}

		public override byte[] GetBytes(object obj)
		{
			return Serializar.GetBytes(obj);
		}

		public override object GetObject(MemoryStream bytes)
		{
			return Serializar.ToObjetoAceptado(TipoDatos, bytes);
		}
		public override string ToString()
		{
			return string.Format("TipoDatos={0}", tipoDatos);
		}

	}
}


