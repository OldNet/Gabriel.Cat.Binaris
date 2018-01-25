using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public abstract class ElementoComplejoBinario : ElementoBinario
	{
		Llista<ElementoBinario> partesElemento;

		public ElementoComplejoBinario()
		{
			partesElemento = new Llista<ElementoBinario>();
		}

		public Llista<ElementoBinario> PartesElemento {
			get {
				return partesElemento;
			}
			private set {
				partesElemento = value;
			}
		}

		public override object GetObject(MemoryStream bytes)
		{
			object[] parts = new object[partesElemento.Count];
			long numNull = 0;
			for (int i = 0; i < parts.Length; i++) {
				parts[i] = partesElemento[i].GetObject(bytes);
				if (parts[i] == null)
					numNull++;
			}
			if (numNull != parts.Length)
				return GetObject(parts);
			else
				return null;
		}

		protected abstract object GetObject(Object[] parts);
	}
}


