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
	/// ermite serializar y deserializar partes de un elemento
	/// </summary>
	public abstract class ElementoComplejoBinario : ElementoComplejoBinarioNullable
	{
		public ElementoComplejoBinario(IEnumerable<ElementoBinario> partes=null):base(partes)
		{}
		public override byte[] GetBytes(object obj)
		{
			return IGetBytes(obj);
		}
		public override object GetObject(MemoryStream bytes)
		{
			return IGetObject(bytes);
		}

	}
}


