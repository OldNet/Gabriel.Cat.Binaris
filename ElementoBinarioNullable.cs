/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 07/02/2018
 * Hora: 22:21
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat.Binaris
{
	/// <summary>
	/// Description of ElementoBinarioNullable.
	/// </summary>
	public abstract class ElementoBinarioNullable:ElementoBinario
	{
		#region implemented abstract members of ElementoBinario
		public override byte[] GetBytes(object obj)
		{
			byte[] bytesObj;
			if(obj==null)
				bytesObj=new byte[]{ElementoBinario.NULL};
			else bytesObj=IGetBytes(obj);
			
			return bytesObj;
		}
		protected abstract byte[] IGetBytes(object obj);
		
		public override object GetObject(System.IO.MemoryStream bytes)
		{
			object obj;
			if(bytes.ReadByte()==ElementoBinario.NULL)
			{
				obj=null;
			}else{
				bytes.Position--;
				obj=IGetObject(bytes);
				
			}
			return obj;
		}
		protected abstract object IGetObject(System.IO.MemoryStream bytes);
		#endregion
		
	}
}
