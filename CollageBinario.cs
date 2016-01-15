using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Binaris
{
   public class CollageBinario:ElementoIEnumerableBinario
    {
        public CollageBinario() : base(new ImageFragmentBinario(),null)
        {

        }
        public override byte[] GetBytes(object obj)
        {
            if (obj is Collage)
            {
                LongitudUInt= Convert.ToUInt32((obj as Collage).Count);
            }
            return base.GetBytes(obj);
        }
    }
    public class ImageFragmentBinario : ElementoBinario
    {

        public override byte[] GetBytes(object obj)
        {
            List<byte> bytesObj = new List<byte>();
            ImageFragment fragment= obj as ImageFragment;
            if (fragment!=null)
            {
                bytesObj.AddRange(Serializar.GetBytes(fragment.Location));
                bytesObj.AddRange(Serializar.GetBytes(fragment.Image));
            }
            else {
                bytesObj.Add(0x00);
            }
            return bytesObj.ToArray();
        }

        public override object GetObject(Stream bytes)
        {
            PointZ location = Serializar.DameObjetoAceptado(Serializar.TiposAceptados.PointZ, bytes) as PointZ;
            Bitmap bmp = null;
            ImageFragment fragment=null;
            if (location != null)
                bmp = Serializar.DameObjetoAceptado(Serializar.TiposAceptados.Bitmap, bytes) as Bitmap;
            if (bmp!=null)
            fragment = new ImageFragment(bmp, location);
            return fragment;
        }
    }
}
