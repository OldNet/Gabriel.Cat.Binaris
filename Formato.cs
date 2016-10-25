using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat;
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
    public abstract class ElementoBinario
    {
        public abstract byte[] GetBytes(Object obj);
        public Object GetObject(byte[] bytes)
        { return GetObject(new MemoryStream(bytes)); }
        public abstract Object GetObject(MemoryStream bytes);
        public static ElementoBinario ElementosTipoAceptado(Serializar.TiposAceptados tipo)
        {
            ElementoBinario elemento;
            if (tipo == Serializar.TiposAceptados.String)
                elemento = new StringBinario();
            else if (tipo == Serializar.TiposAceptados.Bitmap)
                elemento = new BitmapBinario();
            else if (tipo == Serializar.TiposAceptados.DateTime)
                elemento = new DateTimeBinario();
            else
                elemento = new ElementoSimpleBinario(tipo);
            return elemento;
        }

    }

    public class ElementoSimpleBinario : ElementoBinario
    {
        //char,int,long,...basicos
        Serializar.TiposAceptados tipoDatos;

        public ElementoSimpleBinario(Serializar.TiposAceptados tipo)
        {
            TipoDatos = tipo;
        }
        public Serializar.TiposAceptados TipoDatos
        {
            get { return tipoDatos; }
            set
            {
                if (value == Serializar.TiposAceptados.String || value == Serializar.TiposAceptados.Bitmap)
                    throw new TipoException(String.Format("el tipo {0} no es un tipo simple", value.ToString()));
                tipoDatos = value;
            }
        }

        public override byte[] GetBytes(object obj)
        {
            return Serializar.GetBytes(obj);
        }

        public override object GetObject(MemoryStream bytes)
        {
            return Serializar.ToObjetoAceptado(TipoDatos, bytes);
        }
    }
    public abstract class ElementoComplejoBinario : ElementoBinario
    {
        Llista<ElementoBinario> partesElemento;
        public ElementoComplejoBinario()
        {
            partesElemento = new Llista<ElementoBinario>();
        }
        public Llista<ElementoBinario> PartesElemento
        {
            get { return partesElemento; }
            private set { partesElemento = value; }
        }
        public override object GetObject(MemoryStream bytes)
        {
            object[] parts = new object[partesElemento.Count];
            long numNull = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = partesElemento[i].GetObject(bytes);
                if (parts[i] == null)
                    numNull++;
            }
            if (numNull != parts.Length)
                return GetObject(parts);
            else
                return null;
        }
        protected abstract object GetObject(Object[] parts);
    }
    public class ElementoIEnumerableBinario : ElementoBinario
    {
        public enum LongitudBinaria
        {
            Byte,
            UShort,
            UInt,
            Long,
            MarcaFin
        }
        byte[] marcaFin;
        LongitudBinaria longitud;
        ElementoBinario elemento;
        public ElementoIEnumerableBinario(ElementoBinario elemento, LongitudBinaria unidadCantidadElementos)
        {
            Elemento = elemento;
            MarcaFin = null;
            Longitud = unidadCantidadElementos;
            
        }
        public ElementoIEnumerableBinario(ElementoBinario elemento, byte[] marcaFin)
            : this(elemento, LongitudBinaria.MarcaFin)
        {
            MarcaFin = marcaFin;
        }
      
        /// <summary>
        /// Sirve para acabar la lectura sin saber cuantos elementos abran, si es null la marcaFin es 0x00
        /// </summary>
        public byte[] MarcaFin
        {
            get { return marcaFin; }
            set
            {
                if (value == null)
                    value = new byte[] { 0x00 };
                marcaFin = value;
                Longitud = LongitudBinaria.MarcaFin;
            }
        }
        public LongitudBinaria Longitud
        {
            get { return longitud; }
            set { longitud = value; }
        }

        public ElementoBinario Elemento
        {
            get { return elemento; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                elemento = value;
            }
        }
        /// <summary>
        /// Obtiene los bytes de la lista de elementos
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override byte[] GetBytes(object obj)
        {
            if (!(obj is IEnumerable))
                throw new TipoException("El objeto no es IEnumerable<object>");
            List<byte> bytesObj = new List<byte>();
            long numItems = 0;
            foreach (object partObj in (IEnumerable)obj)
            {
                numItems++;
                bytesObj.AddRange(Elemento.GetBytes(partObj));
            }
            switch (Longitud)
            {
                case LongitudBinaria.Byte:
                    bytesObj.InsertRange(0, Serializar.GetBytes(Convert.ToByte(numItems)));
                    break;
                case LongitudBinaria.UShort:
                    bytesObj.InsertRange(0, Serializar.GetBytes(Convert.ToUInt16(numItems)));
                    break;
                case LongitudBinaria.UInt:
                    bytesObj.InsertRange(0, Serializar.GetBytes(Convert.ToUInt32(numItems)));
                    break;
                case LongitudBinaria.Long:
                    bytesObj.InsertRange(0, Serializar.GetBytes(Convert.ToInt64(numItems)));
                    break;
                case LongitudBinaria.MarcaFin:
                    if (bytesObj.Contains(MarcaFin))
                        throw new Exception("Se ha encontrado los bytes de la marca de fin en los bytes a guardar");
                    bytesObj.AddRange(MarcaFin);
                    break;
            }
            return bytesObj.ToArray();
        }

        public override object GetObject(MemoryStream bytes)
        {
           
            //la marca fin y la longitud Que Se usara  y el elemento es el minimo...
            long numItems = -1;
            List<object> objects = new List<object>();
            Llista<byte> compruebaBytes = new Llista<byte>();
            List<byte> bytesElementoMarcaFin = new List<byte>();
            byte[] bufferStreamBytes,bytesObj;
            ByteArrayStream bs = bytes;
            Object objHaPoner = null;
           // String marcaFiHex;
            Object[] partes = null;
            switch (Longitud)
            {
                case LongitudBinaria.Byte:
                    numItems = bytes.ReadByte();
                    break;
                case LongitudBinaria.UShort:
                    numItems = Serializar.ToUShort(bytes.Read(2));
                    break;
                case LongitudBinaria.UInt:
                    numItems = Serializar.ToUInt(bytes.Read(4));
                    ;
                    break;
                case LongitudBinaria.Long:
                    numItems = Serializar.ToLong(bytes.Read(8));
                    break;
            }
            if (numItems >= 0)
            {
                partes = new Object[numItems];
                for (long i = 0; i < numItems; i++)
                {
                    partes[i] = Elemento.GetObject(bytes);
                   
                }

            }
            else
            {
                //usa marca fin
                //pongo el byte en la cola
                //miro si coincide con la marca fin
                //si no coincide cojo el primer byte
                //si coincide dejo de añadir bytes
                bufferStreamBytes = bytes.GetBuffer();
                try
                {
                    bytesObj = bufferStreamBytes.SubArray((Hex)bytes.Position, bufferStreamBytes.BuscarArray((Hex)bytes.Position, marcaFin));

                    //ahora tengo los bytes tengo que obtener los elementos
                    bytes = new MemoryStream(bytesObj);

                    do
                    {
                        objHaPoner = Elemento.GetObject(bytes);
                        if (objHaPoner != null)
                            objects.Add(objHaPoner);
                    } while (objHaPoner != null && !bytes.EndOfStream());
                    if (objects.Count != 0)
                        partes = objects.ToArray();
                }catch
                {
                    throw new FormatException("No se ha encontrado la marca de fin");
                }
            }
            if (partes == null)
                partes = new object[0];   
            return partes;

        }
    }

    //clases concretas pasadas al sistema
    public class BitmapBinario : ElementoComplejoBinario
    {
        public BitmapBinario()
        {
            PartesElemento.Add(new ElementoIEnumerableBinario(ElementosTipoAceptado(Serializar.TiposAceptados.Byte), ElementoIEnumerableBinario.LongitudBinaria.Long));
        }
        public override object GetObject(MemoryStream bytes)
        {
            Object obj = null;
            if (bytes.ReadByte() != (byte)0x00)
            {
                bytes.Position--;
                obj= base.GetObject(bytes);
            }
            return obj;
        }
        protected override object GetObject(object[] parts)
        {
            Bitmap bmp = null;
            if (parts.Length == 1 && parts[0] is object[])
                bmp = Serializar.ToBitmap(((object[])parts[0]).CastingToByte());
            return bmp;
        }



        public override byte[] GetBytes(object obj)
        {
            List<byte> bytes = new List<byte>();
            Bitmap bmp = obj as Bitmap;
            if (bmp!=null)
            {
                bytes.AddRange(Serializar.GetBytes(bmp));
                bytes.InsertRange(0, Serializar.GetBytes(Convert.ToInt64(bytes.Count)));
            }
            else
                bytes.Add((byte)0x0);
            return bytes.ToArray();
        }
    }
    public class StringBinario : ElementoIEnumerableBinario
    {
        public StringBinario()
            : this(null)
        {
        	Longitud=LongitudBinaria.Long;
        }
        public StringBinario(byte[] marcaFin)
            : base(ElementosTipoAceptado(Serializar.TiposAceptados.Char), marcaFin)
        {
        }
        public override object GetObject(MemoryStream bytes)
        {
            object[] caracteres = (object[])base.GetObject(bytes);
            StringBuilder str = new StringBuilder();
            if(caracteres!=null)
              for (int i = 0; i < caracteres.Length; i++)
                str.Append(caracteres[i].ToString());
            return str.ToString();
        }
        public override byte[] GetBytes(object obj)
        {
            List<object> caracteres = new List<object>();
            string str = obj as string;
            if (str != null)
                for (int i = 0; i < str.Length; i++)
                    caracteres.Add(str[i]);
            return base.GetBytes(caracteres);
        }


    }
    public class DateTimeBinario : ElementoSimpleBinario
    {
        public DateTimeBinario()
            : base(Serializar.TiposAceptados.Long)
        {
        }
        public override object GetObject(MemoryStream bytes)
        {
            object objTime = base.GetObject(bytes);
            if (objTime != null)
                return new DateTime((long)objTime);
            else return null;
        }
    }
    //Excepcion propia
    public class TipoException : Exception
    {
        public TipoException()
            : base()
        {
        }
        public TipoException(string message)
            : base(message)
        {
        }
    }

    public class ObjectNotSerializable:Exception
    {
        public ObjectNotSerializable(Type typeSerializable):base("Only can serialitze "+typeSerializable.AssemblyQualifiedName)
        {}
    }

}
