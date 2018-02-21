using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;

namespace Gabriel.Cat.Binaris
{
	public enum LongitudBinaria
	{
		Byte,
		UShort,
		UInt,
		ULong,
		MarcaFin
	}
	public interface IElementoBinarioIList
	{
		ElementoBinario Elemento
		{get;set;}
	}
	public class ElementoIListBinario<T> : ElementoBinarioNullable,IElementoBinarioIList
	{
		

		byte[] marcaFin;

		LongitudBinaria longitud;

		ElementoBinario elemento;

	
		public ElementoIListBinario(ElementoBinario elemento, LongitudBinaria unidadCantidadElementos = LongitudBinaria.ULong)
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

		#region implemented abstract members of ElementoBinarioNullable
		protected override byte[] IGetBytes(object obj)
		{
			return GetBytes((IList<T>)obj);
		}

		#endregion
		
		/// <summary>
		/// Obtiene los bytes de la lista de elementos
		/// </summary>
		/// <param name="obj">IList</param>
		/// <returns></returns>
		public 	byte[] GetBytes(IList<T> lst)
		{
			
			object[] partes =new object[lst.Count];
			byte[] longitud=null;
			byte[] bytesObjs=null;
			
			for(int i=0;i<lst.Count;i++)
			{
				
				partes[i]=Elemento.GetBytes(lst[i]);
			}
			
			switch (Longitud) {
				case LongitudBinaria.Byte:
					longitud= Serializar.GetBytes(Convert.ToByte(partes.Length));
					break;
				case LongitudBinaria.UShort:
					longitud=  Serializar.GetBytes(Convert.ToUInt16(partes.Length));
					break;
				case LongitudBinaria.UInt:
					longitud=  Serializar.GetBytes(Convert.ToUInt32(partes.Length));
					break;
				case LongitudBinaria.ULong:
					longitud=  Serializar.GetBytes(Convert.ToInt64(partes.LongLength));
					break;
				case LongitudBinaria.MarcaFin:
					bytesObjs=new byte[0].AddArray(partes.Casting<byte[]>());
					if (bytesObjs.SearchArray(MarcaFin)>0)
						throw new Exception("Se ha encontrado los bytes de la marca de fin en los bytes a guardar");
					bytesObjs=bytesObjs.AddArray(MarcaFin);
					break;
			}
			if(bytesObjs==null)
			{
				bytesObjs=longitud.AddArray(partes.Casting<byte[]>());
			}
			return bytesObjs;
		}
		protected override object IGetObject(MemoryStream bytes)
		{
			
			//la marca fin y la longitud Que Se usara  y el elemento es el minimo...
			ulong? numItems = null;
			List<T> objects = new List<T>();
			Llista<byte> compruebaBytes = new Llista<byte>();
			List<byte> bytesElementoMarcaFin = new List<byte>();
			byte[] bufferStreamBytes, bytesObj;
			object objHaPoner;
			int posMarcaFin;
			T[] partes = null;
			
			switch (Longitud) {
				case LongitudBinaria.Byte:
					numItems =Convert.ToUInt64(bytes.ReadByte());
					break;
				case LongitudBinaria.UShort:
					numItems = Serializar.ToUShort(bytes.Read(2));
					break;
				case LongitudBinaria.UInt:
					numItems = Serializar.ToUInt(bytes.Read(4));
					break;
				case LongitudBinaria.ULong:
					numItems = Serializar.ToULong(bytes.Read(8));
					break;
			}
			if (numItems.HasValue) {
				partes = new T[numItems.Value];
				for (ulong i = 0,f=numItems.Value; i < f; i++) {
					partes[i] =(T) Elemento.GetObject(bytes);
				}
			}
			else {
				//usa marca fin
				//pongo el byte en la cola
				//miro si coincide con la marca fin
				//si no coincide cojo el primer byte
				//si coincide dejo de añadir bytes
				bufferStreamBytes = bytes.GetBuffer();
				
				posMarcaFin=(int)bufferStreamBytes.SearchArray((int)bytes.Position, marcaFin);
				if(posMarcaFin>0){
					bytesObj = bufferStreamBytes.SubArray((int)bytes.Position, posMarcaFin);
					//ahora tengo los bytes tengo que obtener los elementos
					bytes = new MemoryStream(bytesObj);
					do {
						objHaPoner = Elemento.GetObject(bytes);
						if (objHaPoner != null)
							objects.Add((T)objHaPoner);
					}
					while (objHaPoner != null && !bytes.EndOfStream());
					if (objects.Count != 0)
						partes = objects.ToArray();
				}
				else
					throw new FormatException("No se ha encontrado la marca de fin");
				
			}
			if (partes == null)
				partes = new T[0];
			return partes;
		}

		public static new ElementoIListBinario<T> ElementosTipoAceptado<T>(Serializar.TiposAceptados tipo)
		{
			return new ElementoIListBinario<T>(ElementoBinario.ElementosTipoAceptado(tipo));
		}
	}
}


