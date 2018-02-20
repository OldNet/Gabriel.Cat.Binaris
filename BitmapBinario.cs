using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public class BitmapBinario : ElementoComplejoBinarioNullable
	{
		public BitmapBinario()
		{
			Partes.Add(new ElementoIListBinario<byte>(ElementosTipoAceptado(Serializar.TiposAceptados.Byte)));
		}
		

		#region implemented abstract members of ElementoComplejoBinarioNullable


		protected override IList GetPartsObject(object obj)
		{
			Bitmap bmp=obj as Bitmap;
			if(bmp==null)
				throw new Exception(string.Format("Se ha intentado leer {0} como System.Drawing.Bitmap",obj.GetType().FullName));
			return new object[]{Serializar.GetBytes(bmp)};
		}


		#endregion
		protected override object IGetObject(MemoryStream bytes)
		{
		
			byte[] bytesBmp=(byte[]) ((base.GetPartsObject(bytes))[0]);
			Bitmap bmp;
			try{
				bmp=Serializar.ToBitmap(bytesBmp);
			}catch{
				
				throw new Exception("El formato no coincide con un Bitmap");
			}
			return bmp;
		}
		public override string ToString()
		{
			return "TipoDatos=Bitmap";
		}
		
	}
}


