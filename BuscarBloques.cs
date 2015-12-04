using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat.Binaris
{
    /// <summary>
    /// Sirve para encontrar un conjunto de bytes iguales seguidos
    /// </summary>
    public  static class BuscarBloques
    {
        /// <summary>
        /// busca la posicion donde se encuentra el bloque de bytes iguales a la muestra
        /// </summary>
        /// <param name="pathArchivo">direccion del archivo</param>
        /// <param name="bytesAEncontrarIgualLaMuestra">cantidad de bytes seguidos a encontrar iguales a la muestra</param>
        /// <param name="byteMuestra">byte muestra para encontrar</param>
        /// <returns>devuelve la posicion del inicio del primer bloque que encuentra de ese tamaño, si no encuentra devuelve -1</returns>
        public static long BuscaBloque(string pathArchivo, long bytesAEncontrarIgualLaMuestra,long inicio=0, byte byteMuestra = 0xFF)
        {
            return BuscaBloque(new FileInfo(pathArchivo), bytesAEncontrarIgualLaMuestra,inicio, byteMuestra);
        }
        /// <summary>
        /// busca la posicion donde se encuentra el bloque de bytes iguales a la muestra
        /// </summary>
        /// <param name="archivo">archivo para buscar los bytes</param>
        /// <param name="bytesAEncontrarIgualLaMuestra">cantidad de bytes seguidos a encontrar iguales a la muestra</param>
        /// <param name="byteMuestra">byte muestra para encontrar</param>
        /// <returns>devuelve la posicion del inicio del primer bloque que encuentra de ese tamaño, si no encuentra devuelve -1</returns>
        public static long BuscaBloque(this FileInfo archivo, long bytesAEncontrarIgualLaMuestra, long inicio = 0, byte byteMuestra = 0xFF)
        {
           FileStream stream= archivo.GetStream();
           long posicionInicioBloque = BuscaBloque(stream, bytesAEncontrarIgualLaMuestra,inicio, byteMuestra);
           stream.Close();
           return posicionInicioBloque;
        }
        public static long BuscaBloque(this IEnumerable<Byte> archivo, long bytesAEncontrarIgualLaMuestra, long inicio = 0, byte byteMuestra = 0xFF)
        {
            Stream stream = new MemoryStream(archivo.ToArray());
            long posicionInicioBloque = BuscaBloque(stream, bytesAEncontrarIgualLaMuestra,inicio, byteMuestra);
            stream.Close();
            return posicionInicioBloque;
        }
        public static long BuscaBloque(this Stream stream, long bytesAEncontrarIgualLaMuestra, long inicio = 0, byte byteMuestra = 0xFF)
        {
            if (!stream.CanRead)
                throw new IOException("No se puede leer");
            long posicionStream = stream.Position;
            long posicionInicioBloque = -1;//si no encuntra devuelve -1
            long contador = 0;
            if (!stream.CanRead)
                throw new System.IO.IOException("Imposible de leer");
            stream.Position = inicio;
            while (posicionInicioBloque == -1 && !stream.EndOfStream())
            {
                if (stream.ReadByte().Equals(byteMuestra))
                    contador++;
                else
                    contador = 0;

                if (contador == bytesAEncontrarIgualLaMuestra)
                {
                    posicionInicioBloque = stream.Position - bytesAEncontrarIgualLaMuestra;
                }

            }
            stream.Position = posicionStream;
            return posicionInicioBloque;
        }
    }
}
