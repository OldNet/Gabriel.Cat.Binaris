using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public abstract class ElementoBinario
	{
		public const byte NULL=0x0;
		
		public byte[] GetBytes()
		{
			return GetBytes(this);
		}
		public abstract byte[] GetBytes(object obj);

		public object GetObject(byte[] bytes)
		{
			return GetObject(new MemoryStream(bytes));
		}

		public abstract object GetObject(MemoryStream bytes);

		public static ElementoBinario ElementosTipoAceptado(Serializar.TiposAceptados tipo)
		{
			ElementoBinario elemento;
			switch (tipo) {
				case Serializar.TiposAceptados.String:
					elemento = new StringBinario();
					break;
				case Serializar.TiposAceptados.Bitmap:
					elemento = new BitmapBinario();
					break;
				default:
					elemento = new ElementoBinarioTamañoFijo(tipo);
					break;
			}
			return elemento;
		}

		public static bool IsCompatible(Type tipo)
		{
			bool compatible=tipo.GetInterface(typeof(IElementoBinarioComplejo).Name)!=null;
			if(!compatible){
				try{
					Serializar.AssemblyToEnumTipoAceptado(tipo.AssemblyQualifiedName);
					compatible=true;
				}catch{compatible=false;}
			}
			return compatible;
		}
		/// <summary>
		/// Obtiene el Serializador del tipo indicado como parametro
		/// </summary>
		/// <param name="tipo">tipo con constructor sin parametros si es IElementoBinarioComplejo</param>
		/// <returns>devuelve null si no es compatible</returns>
		public static ElementoBinario GetElementoBinario(Type tipo)
		{
			ElementoBinario elementoBinario;
			if(IsCompatible(tipo))
			{
				if(tipo.GetInterface(typeof(IElementoBinarioComplejo).Name)!=null)
				{
					try{
					elementoBinario=((IElementoBinarioComplejo)tipo.GetConstructor(Type.EmptyTypes).Invoke(null)).Serialitzer;
					}catch{
						throw new ArgumentException(String.Format("El tipo tiene que tener un constructor publico sin parametros y la propiedad de {0} tener valor.",typeof(IElementoBinarioComplejo).Name));
					
					}
				}else{
				
				elementoBinario=ElementosTipoAceptado(Serializar.GetType(tipo));
				}
			}else elementoBinario=null;
			return elementoBinario;
		}
		/// <summary>
		/// Devuelve el serializador del objeto pasado como parametro
		/// </summary>
		/// <param name="obj">se tendrá en cuenta si implementa IElementoBinarioComplejo.</param>
		/// <returns>si no es compatible es null</returns>
		public static ElementoBinario GetElementoBinario(object obj)
		{
			IElementoBinarioComplejo serializador=obj as IElementoBinarioComplejo;
			ElementoBinario elemento=serializador!=null?serializador.Serialitzer:null;
			if(elemento==null){
				try{
					elemento=ElementosTipoAceptado(Serializar.GetType(obj));
				}catch{}
			}
			return elemento;
		}
	}
	public interface IElementoBinarioComplejo
	{
		ElementoBinario Serialitzer
		{
			get;
		}
	}
}


