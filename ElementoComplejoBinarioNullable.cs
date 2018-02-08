/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 07/02/2018
 * Hora: 22:26
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections;
using System.Collections.Generic;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat.Binaris
{
	/// <summary>
	/// Permite serializar y deserializar partes de un elemento
	/// </summary>
	public abstract class ElementoComplejoBinarioNullable:ElementoBinarioNullable
	{
		Llista<ElementoBinario> partes;
		
		public ElementoComplejoBinarioNullable(IEnumerable<ElementoBinario> partes=null)
		{
			this.partes=new Llista<ElementoBinario>();
			if(partes!=null)
				this.partes.AddRange(partes);
		}

		protected Llista<ElementoBinario> Partes {
			get {
				return partes;
			}
		}

		#region implemented abstract members of ElementoBinarioNullable

		protected override byte[] IGetBytes(object obj)
		{
			IList partesObj=GetPartsObject(obj);
			object[] bytesPartes;
			
			if(partesObj.Count!=partes.Count)
				throw new ArgumentException(String.Format("El numero de partes no coincide con las partes del {0}",GetType().FullName),"obj");
			
			bytesPartes=new object[partes.Count];
			for(int i=0;i<partes.Count;i++)
				bytesPartes[i]=partes[i].GetBytes(partesObj[i]);
			return new byte[0].AddArray(bytesPartes.Casting<byte[]>());
			
		}
		protected abstract IList GetPartsObject(object obj);
		

		
		public object[] GetPartsObject(System.IO.MemoryStream ms)
		{
			object[] partesObj=new object[partes.Count];
			for(int i=0;i<partes.Count;i++)
				partesObj[i]=partes[i].GetObject(ms);
			
			return partesObj;
		}
		
		#endregion
	}
}
