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
			const UsoPropiedad USONECESARIO=UsoPropiedad.Get|UsoPropiedad.Set;
			const UsoPropiedad USOILISTNECESARIO=UsoPropiedad.Get;
			GetPartsObjectMethod getPartsObj=(obj)=>{
				
				Propiedad[] propiedades=obj.GetProperties();
				List<object> partes=new List<object>();
				for(int i=0;i<propiedades.Length;i++)
				{
					if(propiedades[i].Info.Uso==USONECESARIO&&ElementoBinario.IsCompatible(propiedades[i].Info.Tipo)||propiedades[i].Objeto is IList&&propiedades[i].Info.Uso==USOILISTNECESARIO&&ElementoBinario.IsCompatible(propiedades[i].Objeto))
						partes.Add(propiedades[i].Objeto);
				}
				return partes;
			};
			GetObjectMethod getObject=(partes)=>{
				
				T obj=new T();
				Propiedad[] propiedades=obj.GetProperties();
				IList lstAPoner;
				IList lstObj;
				for(int i=0,j=0;i<propiedades.Length;i++)
				{
					if(propiedades[i].Info.Uso==USONECESARIO&&ElementoBinario.IsCompatible(partes[j].GetType()))
						obj.SetProperty(propiedades[i].Info.Nombre,partes[j++]);
					else if(propiedades[i].Info.Uso==USOILISTNECESARIO &&partes[j].GetType().GetInterface("IList")!=null&&ElementoBinario.IsCompatible(partes[j]))
					{
						lstAPoner=partes[j++] as IList;
						//cojo la lista del objeto y le añado la nueva
						lstObj=obj.GetPropteryValue(propiedades[i].Info.Nombre) as IList;
						
						for(int k=0;k<lstAPoner.Count;k++)
							lstObj.Add(lstAPoner[k]);
					}
				}
				return obj;
			};
			

			PropiedadTipo[] properties=typeof(T).GetPropiedades();
			List<ElementoBinario> elementos=new List<ElementoBinario>();
			IList list;
			for(int i=0;i<properties.Length;i++)
			{
				if(properties[i].Uso==USONECESARIO&&ElementoBinario.IsCompatible(properties[i].Tipo))
					elementos.Add(ElementoBinario.GetElementoBinario(properties[i].Tipo));
				else if(properties[i].Uso==USOILISTNECESARIO&&properties[i].Tipo.GetInterface("IList")!=null)
				{
					
	
					list = (IList)Activator.CreateInstance(properties[i].Tipo);//mirar si lo hace correctamente 
					if(ElementoBinario.IsCompatible(list.ListOfWhat()))
					{
						//si es de un tipo compatible lo añado
						elementos.Add(ElementoBinario.GetElementoBinario(list));
					}
				}
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
