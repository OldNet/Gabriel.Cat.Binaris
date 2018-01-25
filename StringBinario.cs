using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public class StringBinario : ElementoIListBinario
	{
		public StringBinario() : base(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.Char), LongitudBinaria.UInt)
		{
		}

		public StringBinario(byte[] marcaFin) : base(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.Char), marcaFin)
		{
		}

		public override object GetObject(MemoryStream bytes)
		{
			object[] caracteresObj = (object[])base.GetObject(bytes);
			char[] caracteres;
			if (caracteresObj != null) {
				caracteres = new char[caracteresObj.Length];
				unsafe {
					char* ptrCaracteres;
					fixed (char* ptCaracteres = caracteres) {
						ptrCaracteres = ptCaracteres;
						for (int i = 0; i < caracteres.Length; i++) {
							*ptrCaracteres = (char)caracteresObj[i];
							ptrCaracteres++;
						}
					}
				}
			}
			else
				caracteres = new char[0];
			return new string(caracteres);
		}

		public override byte[] GetBytes(object obj)
		{
			List<object> caracteres = new List<object>();
			string str = obj as string;
			if (str != null)
				unsafe {
					char* ptrStr;
					fixed (char* ptStr = str) {
						ptrStr = ptStr;
						for (int i = 0; i < str.Length; i++) {
							caracteres.Add(*ptrStr);
							ptrStr++;
						}
					}
				}
			return base.GetBytes(caracteres);
		}
	}
}


