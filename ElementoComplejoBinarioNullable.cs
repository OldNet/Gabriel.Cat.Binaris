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
	public delegate IList GetPartsObjectMethod(object obj);
	public delegate object GetObjectMethod(object[] partsObj);
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
		
		public static ElementoComplejoBinarioNullable GetElement<T>()where T:new()
		{
			
			GetPartsObjectMethod getPartsObj=(obj)=>{
				UsoPropiedad usoNecesario=UsoPropiedad.Get|UsoPropiedad.Set;
				Propiedad[] propiedades=obj.GetProperties();
				List<object> partes=new List<object>();
				for(int i=0;i<propiedades.Length;i++)
				{
					if(propiedades[i].Tipo.Uso==usoNecesario&&ElementoBinario.IsCompatible(propiedades[i].Tipo.Tipo))
						partes.Add(propiedades[i].Objeto);
				}
				return partes;
			};
			GetObjectMethod getObject=(partes)=>{
				UsoPropiedad usoNecesario=UsoPropiedad.Get|UsoPropiedad.Set;
				T obj=new T();
				Propiedad[] propiedades=obj.GetProperties();
				for(int i=0,j=0;i<propiedades.Length;i++)
				{
					if(propiedades[i].Tipo.Uso==usoNecesario&&ElementoBinario.IsCompatible(partes[j].GetType()))
						obj.SetProperty(propiedades[i].Tipo.Nombre,partes[j++]);
				}
				return obj;
			};
			
			IList partesObj=getPartsObj(new T());
			ElementoBinario[] elementos=new ElementoBinario[partesObj.Count];
			
			for(int i=0;i<partesObj.Count;i++)
			{
				elementos[i]=ElementoBinario.GetElementoBinario(partesObj[i]); 
			}
			
			return new ElementoComplejoBinarioNullableExt(elementos,getPartsObj,getObject);
		}
	}
	public class ElementoComplejoBinarioNullableExt:ElementoComplejoBinarioNullable
	{
		GetPartsObjectMethod GetPartsObjectDelegate;
		GetObjectMethod GetObjectDelegate;
		public ElementoComplejoBinarioNullableExt(IEnumerable<ElementoBinario> partes,GetPartsObjectMethod getPartsObjectDelegate,GetObjectMethod getObjectDelegate):base(partes)
		{
			if(getObjectDelegate==null||getPartsObjectDelegate==null)
				throw new ArgumentNullException();
			GetPartsObjectDelegate=getPartsObjectDelegate;
			GetObjectDelegate=getObjectDelegate;
		}
		#region implemented abstract members of ElementoBinarioNullable
		protected override object IGetObject(System.IO.MemoryStream bytes)
		{
			return GetObjectDelegate(GetPartsObject(bytes));
		}
		#endregion
		#region implemented abstract members of ElementoComplejoBinarioNullable
		protected override IList GetPartsObject(object obj)
		{
			return GetPartsObjectDelegate(obj);
		}
		#endregion
		
	}
}
