using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat.Binaris
{
    public static class EditarBinarios
    {

        public static void ModificaBytes(this FileInfo archivo, long posicionInicio, byte[] bytesHaPoner)
        {
            byte[] bytesArchivo = archivo.ModificaBytesCopy(posicionInicio, bytesHaPoner);
            string pathArchivo = archivo.FullName;
            archivo.Delete();
            bytesHaPoner.Save(pathArchivo);
        }
        /// <summary>
        /// Sustituye los bytes que encuentra por los nuevos empezando por la posicion indicada
        /// </summary>
        /// <param name="archivo"></param>
        /// <param name="posicion"></param>
        /// <param name="bytesNuevos"></param>
        /// <param name="añadeSiHaceFalta">hace el archivo mas grande</param>
        /// <returns></returns>
        public static Byte[] ModificaBytesCopy(this FileInfo archivo, long posicion, Byte[] bytesNuevos,bool añadeSiHaceFalta=false)
        {
            Stream st=archivo.GetStream();
            Byte[] bytesArchivoNuevo= ModificaBytesCopy(st,posicion,bytesNuevos,añadeSiHaceFalta);
            st.Close();
            return bytesArchivoNuevo;
        }
        public static Byte[] ModificaBytesCopy(this IEnumerable<Byte> archivo, long posicion, Byte[] bytesNuevos, bool añadeSiHaceFalta = false)
        {
            Stream st = new MemoryStream(archivo.ToArray());
            Byte[] bytesArchivoNuevo = ModificaBytesCopy(st, posicion, bytesNuevos,añadeSiHaceFalta);
            st.Close();
            return bytesArchivoNuevo;
        }
        public static Byte[] ModificaBytesCopy(this Stream archivo, long posicion, Byte[] bytesNuevos, bool añadeSiHaceFalta = false)
        {
            //valido los parametros
            if (posicion < 0)
                throw new ArgumentException("La posicion no puede ser negativa");
            if (!archivo.CanRead)
                throw new IOException("No se puede leer de la stream");
            if (!añadeSiHaceFalta && archivo.Length < posicion)
                throw new ArgumentOutOfRangeException("La posicion esta fuera del archivo");
            if (bytesNuevos == null)
                throw new ArgumentNullException("no hay bytes nuevos...hay null");
            if (archivo.Length < posicion + bytesNuevos.Length&&!añadeSiHaceFalta)
                throw new ArgumentOutOfRangeException("Los bytesNuevos no caben en el archivo sin modificar su tamaño");

            List<Byte> archivoModificado = new List<Byte>();
            //si añaden si la posicion cae fuera se pondra 0x00 hasta la posicion y luego se pondran los bytes nuevos :)
            long posicionActual = 0;
            long posicionArray=0;
            long posicionStream = archivo.Position;
            //pongo los bytes en la lista
            while (archivo.Position < posicion && !archivo.EndOfStream())
            {
                archivoModificado.Add((Byte)archivo.ReadByte());
                posicionActual++;
            }
            while (posicionActual < posicion)
                archivoModificado.Add((Byte)0x00);//pongo bytes en blanco hasta llegar a la posicion (pasa cuando se tiene que hacer mas grande)

            while ((añadeSiHaceFalta||!archivo.EndOfStream())&&posicionArray<bytesNuevos.LongLength)
            {
                if (!archivo.EndOfStream())
                    archivo.ReadByte();//sustituyo los bytes asi que no necesito el viejo
                archivoModificado.Add(bytesNuevos[posicionArray++]);
            }

            while (!archivo.EndOfStream())
                archivoModificado.Add((Byte)archivo.ReadByte());//pongo los que faltan del archivo
            archivo.Position = posicionStream;
            return archivoModificado.ToArray();//devuelvo el archivo con los bytes modificados
        }

        public static Byte[] AñadeBytesCopy(this FileInfo archivo, long posicion, Byte[] bytesNuevos)
        {
            Stream st = archivo.GetStream();
            Byte[] bytesArchivoNuevo = AñadeBytesCopy(st, posicion, bytesNuevos);
            st.Close();
            return bytesArchivoNuevo;
        }
        public static Byte[] AñadeBytesCopy(this IEnumerable<Byte> archivo, long posicion, Byte[] bytesNuevos)
        {
            Stream st = new MemoryStream(archivo.ToArray());
            Byte[] bytesArchivoNuevo = AñadeBytesCopy(st, posicion, bytesNuevos);
            st.Close();
            return bytesArchivoNuevo;
        }
        /// <summary>
        /// Añade los bytes en la posicion indicada  BytesViejos[BytesNuevos]BytesViejos
        /// </summary>
        /// <param name="archivo"></param>
        /// <param name="posicion"></param>
        /// <param name="bytesNuevos"></param>
        /// <returns>devuelve los bytes con los nuevos añadidos donde toca</returns>
        public static Byte[] AñadeBytesCopy(this Stream archivo, long posicion, Byte[] bytesNuevos)
        {
            //valido los parametros
            if (posicion < 0)
                throw new ArgumentException("La posicion no puede ser negativa");
            if (!archivo.CanRead)
                throw new IOException("No se puede leer de la stream");
            if (bytesNuevos == null)
                throw new ArgumentNullException("no hay bytes a añadir hay null");
            List<Byte> archivoModificado = new List<Byte>();
            //si añaden si la posicion cae fuera se pondra 0x00 hasta la posicion y luego se pondran los bytes nuevos :)
            long posicionActual = 0;
            long posicionStream = archivo.Position;
            //pongo los bytes en la lista
            while (archivo.Position < posicion && !archivo.EndOfStream())
            {
                archivoModificado.Add((Byte)archivo.ReadByte());
            }
            while (posicionActual < posicion)
                archivoModificado.Add((Byte)0x00);//pongo bytes en blanco hasta llegar a la posicion (pasa cuando se tiene que hacer mas grande)
            for (long i = 0; i < bytesNuevos.LongLength; i++)
                archivoModificado.Add(bytesNuevos[i]);
            if (!archivo.EndOfStream())
                archivoModificado.Add((Byte)archivo.ReadByte());//pongo los que faltan del archivo
            archivo.Position = posicionStream;
            return archivoModificado.ToArray();//devuelvo el archivo con los bytes modificados
        }
    }
}
