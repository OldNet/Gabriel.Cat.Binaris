using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public class ElementoIListBinario : ElementoBinario
	{
		public enum LongitudBinaria
		{
			Byte,
			UShort,
			UInt,
			Long,
			MarcaFin
		}

		byte[] marcaFin;

		LongitudBinaria longitud;

		ElementoBinario elemento;

		public ElementoIListBinario(ElementoBinario elemento, LongitudBinaria unidadCantidadElementos = LongitudBinaria.Long)
		{
			Elemento = elemento;
			MarcaFin = null;
			Longitud = unidadCantidadElementos;
		}

		public ElementoIListBinario(ElementoBinario elemento, byte[] marcaFin) : this(elemento, LongitudBinaria.MarcaFin)
		{
			MarcaFin = marcaFin;
		}

		/// <summary>
		/// Sirve para acabar la lectura sin saber cuantos elementos abran, si es null la marcaFin es 0x00
		/// </summary>
		public byte[] MarcaFin {
			get {
				return marcaFin;
			}
			set {
				if (value == null)
					value = new byte[] {
						0x00
					};
				marcaFin = value;
				Longitud = LongitudBinaria.MarcaFin;
			}
		}

		public LongitudBinaria Longitud {
			get {
				return longitud;
			}
			set {
				longitud = value;
			}
		}

		public ElementoBinario Elemento {
			get {
				return elemento;
			}
			set {
				if (value == null)
					throw new ArgumentNullException();
				elemento = value;
			}
		}

		/// <summary>
		/// Obtiene los bytes de la lista de elementos
		/// </summary>
		/// <param name="obj">IList</param>
		/// <returns></returns>
		public override byte[] GetBytes(object obj)
		{
			IList lst=obj as IList;
			if (lst==null)
				throw new TipoException("El objeto no es IList");
			
			List<byte> bytesObj = new List<byte>();
			long numItems = 0;
			
			
			for(int i=0;i<lst.Count;i++)
			{
				numItems++;
				bytesObj.AddRange(Elemento.GetBytes(lst[i]));
			}
			
			switch (Longitud) {
				case LongitudBinaria.Byte:
					bytesObj.InsertRange(0, Serializar.GetBytes(Convert.ToByte(numItems)));
					break;
				case LongitudBinaria.UShort:
					bytesObj.InsertRange(0, Serializar.GetBytes(Convert.ToUInt16(numItems)));
					break;
				case LongitudBinaria.UInt:
					bytesObj.InsertRange(0, Serializar.GetBytes(Convert.ToUInt32(numItems)));
					break;
				case LongitudBinaria.Long:
					bytesObj.InsertRange(0, Serializar.GetBytes(Convert.ToInt64(numItems)));
					break;
				case LongitudBinaria.MarcaFin:
					if (bytesObj.Contains(MarcaFin))
						throw new Exception("Se ha encontrado los bytes de la marca de fin en los bytes a guardar");
					bytesObj.AddRange(MarcaFin);
					break;
			}
			return bytesObj.ToArray();
		}

		public override object GetObject(MemoryStream bytes)
		{
			//la marca fin y la longitud Que Se usara  y el elemento es el minimo...
			long numItems = -1;
			List<object> objects = new List<object>();
			Llista<byte> compruebaBytes = new Llista<byte>();
			List<byte> bytesElementoMarcaFin = new List<byte>();
			byte[] bufferStreamBytes, bytesObj;
			ByteArrayStream bs = bytes;
			Object objHaPoner = null;
			// String marcaFiHex;
			Object[] partes = null;
			switch (Longitud) {
				case LongitudBinaria.Byte:
					numItems = bytes.ReadByte();
					break;
				case LongitudBinaria.UShort:
					numItems = Serializar.ToUShort(bytes.Read(2));
					break;
				case LongitudBinaria.UInt:
					numItems = Serializar.ToUInt(bytes.Read(4));
					;
					break;
				case LongitudBinaria.Long:
					numItems = Serializar.ToLong(bytes.Read(8));
					break;
			}
			if (numItems >= 0) {
				partes = new Object[numItems];
				for (long i = 0; i < numItems; i++) {
					partes[i] = Elemento.GetObject(bytes);
				}
			}
			else {
				//usa marca fin
				//pongo el byte en la cola
				//miro si coincide con la marca fin
				//si no coincide cojo el primer byte
				//si coincide dejo de añadir bytes
				bufferStreamBytes = bytes.GetBuffer();
				try {
					bytesObj = bufferStreamBytes.SubArray((int)bytes.Position, (int)bufferStreamBytes.SearchArray((int)bytes.Position, marcaFin));
					//ahora tengo los bytes tengo que obtener los elementos
					bytes = new MemoryStream(bytesObj);
					do {
						objHaPoner = Elemento.GetObject(bytes);
						if (objHaPoner != null)
							objects.Add(objHaPoner);
					}
					while (objHaPoner != null && !bytes.EndOfStream());
					if (objects.Count != 0)
						partes = objects.ToArray();
				}
				catch {
					throw new FormatException("No se ha encontrado la marca de fin");
				}
			}
			if (partes == null)
				partes = new object[0];
			return partes;
		}

		public static new ElementoIListBinario ElementosTipoAceptado(Serializar.TiposAceptados tipo)
		{
			return new ElementoIListBinario(ElementoBinario.ElementosTipoAceptado(tipo));
		}
	}
}


