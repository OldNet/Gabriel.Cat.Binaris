using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
    /// <summary>
    /// Es una clase para crear formatos propios
    /// </summary>
    public class Formato
    {
        //el archivo sera tipoDatos -> int,string,Bitmap,int,int,...etc...
        //no se dan pistas sobre lo que es...
        //se usara long para definir la Longitud de los IEnumerable(que seran pasados a Array)
        //entonces si pasan una string escribire long,byte[](con los bytes de la string)
        //tengo una pila de cosas a leer int,long,string,Bitmap y los bytes del archivo, entonces para cada Elemento tengo que leer sus bytes
        public static readonly byte[] firmaDefault = {
			0x06,
			0xBD,
			0xFB,
			0x97,
			0xAB,
			0xF8
		};
        byte[] firma;
        Llista<ElementoBinario> elementosArchivo;
        public Formato()
            : this(firmaDefault)
        {
        }
        public Formato(IEnumerable<ElementoBinario> elementos)
            : this(firmaDefault, elementos)
        {

        }
        public Formato(byte[] firma, IEnumerable<ElementoBinario> elementos)
            : this(firma)
        {
            elementosArchivo.AddRange(elementos);
        }
        public Formato(byte[] firma)
        {
            Firma = firma;
            elementosArchivo = new Llista<ElementoBinario>();
        }
        /// <summary>
        /// Firma para reconocer el tipo de archivo facilmente, si es null no se pondra firma
        /// </summary>
        public byte[] Firma
        {
            get
            {
                return firma;
            }
            set
            {
                if (value == null)
                    firma = new byte[0];
                else
                    firma = value;
            }
        }
        public Llista<ElementoBinario> ElementosArchivo
        {
            
            get { return elementosArchivo; }
            private set { elementosArchivo = value; }
        }
        public Object[] GetPartsOfObject(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            object[] objs = GetPartsOfObject(ms);
            ms.Close();
            return objs;
        }
        public Object[] GetPartsOfObject(MemoryStream st)
        {
            List<Object> objs = new List<object>();
            if ((Hex)st.Read(firma.Length) != (Hex)firma)
                throw new Exception("La stream no pertenece al formato, contiene una firma no esperada...");
            for (int i = 0; i < elementosArchivo.Count; i++)
            {
                objs.Add(elementosArchivo[i].GetObject(st));
            }
            return objs.ToArray();
        }
        public byte[] GetBytes(IEnumerable<object> parts)
        {
            List<Object> objsList = new List<object>(parts);
            List<byte> bytes = new List<byte>();
            bytes.AddRange(firma);
              for (int i = 0; i < elementosArchivo.Count; i++)
                 bytes.AddRange(elementosArchivo[i].GetBytes(objsList[i]));
            return bytes.ToArray();
        }

    }
    
    
    
    

    //clases concretas pasadas al sistema
    
    
    //Excepcion propia
    

    

}
