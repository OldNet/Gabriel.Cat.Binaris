using System;
using System.Collections.Generic;
using System.Linq;
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
			elementosArchivo.AfegirMolts(elementos);
		}
		public Formato(byte[] firma)
		{
			Firma = firma;
			elementosArchivo = new Llista<ElementoBinario>();
		}
		/// <summary>
		/// Firma para reconocer el tipo de archivo facilmente, si es null no se pondra firma
		/// </summary>
		public byte[] Firma {
			get {
				return firma;
			}
			set {
				if (value == null)
					firma = new byte[0];
				else
					firma = value;
			}
		}
		public Llista<ElementoBinario> ElementosArchivo {
			get { return elementosArchivo; }
			private set { elementosArchivo = value; }
		}
		public Object[] GetObjects(byte[] bytes)
		{
			MemoryStream ms = new MemoryStream(bytes);
			object[] objs = GetObjects(ms);
			ms.Close();
			return objs;
		}
		public Object[] GetObjects(Stream st)
		{
			List<Object> objs = new List<object>();
			if (st.Read(firma.Length).ToHex() != firma.ToHex())
				throw new Exception("La stream no pertenece al formato, contiene una firma no esperada...");
			for (int i = 0; i < elementosArchivo.Count; i++) {
                objs.Add(elementosArchivo[i].GetObject(st));
			}
			return objs.ToArray();
		}

		public byte[] GetBytes(IEnumerable<object> objetos)
		{
			List<Object> objsList = new List<object>(objetos);
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
        { return new MemoryStream(bytes); }
		public abstract Object GetObject(Stream bytes);
		public static ElementoBinario ElementosTipoAceptado(Serializar.TiposAceptados tipo)
		{
			ElementoBinario elemento = null;
			switch (tipo) {
				case Serializar.TiposAceptados.Null:
				case Serializar.TiposAceptados.Byte:
				case Serializar.TiposAceptados.Bool:
				case Serializar.TiposAceptados.Char:
					elemento = new ElementoSimpleBinario(tipo, 1);
					break;
				case Serializar.TiposAceptados.Short:
				case Serializar.TiposAceptados.UShort:
					elemento = new ElementoSimpleBinario(tipo, 2);
					break;
				case Serializar.TiposAceptados.Int:
				case Serializar.TiposAceptados.UInt:
					elemento = new ElementoSimpleBinario(tipo, 4);
					break;
				case Serializar.TiposAceptados.Long:
				case Serializar.TiposAceptados.ULong:
				case Serializar.TiposAceptados.Double:
				case Serializar.TiposAceptados.Float:
					elemento = new ElementoSimpleBinario(tipo, 8);
					break;
					
				case Serializar.TiposAceptados.DateTime:
					elemento = new DateTimeBinario();
					break;
				case Serializar.TiposAceptados.String:
					elemento = new StringBinario();
					break;
				case Serializar.TiposAceptados.Bitmap:
					elemento = new BitmapBinario();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return elemento;
		}

	}

	public class ElementoSimpleBinario : ElementoBinario
	{
		//char,int,long,...basicos
		Serializar.TiposAceptados tipoDatos;
		long numeroBytes;


		public ElementoSimpleBinario(Serializar.TiposAceptados tipo, long numeroBytes)
		{
			TipoDatos = tipo;
			NumeroBytes = numeroBytes;
		}
		public Serializar.TiposAceptados TipoDatos {
			get { return tipoDatos; }
			set {
				if (value == Serializar.TiposAceptados.String || value == Serializar.TiposAceptados.Bitmap)
					throw new TipoException(String.Format("el tipo {0} no es un tipo simple", value.ToString()));
				tipoDatos = value;
			}
		}
		public long NumeroBytes {
			get { return numeroBytes; }
			set {
				if (value < 1)
					throw new TipoException("No puede tener una longitud negativa de bytes");
				numeroBytes = value;
			}
		}
		public override byte[] GetBytes(object obj)
		{
			return Serializar.GetBytes(obj);
		}

		public override object GetObject(Stream bytes)
		{
			return Serializar.ToTipoAceptado(tipoDatos, bytes.Read(NumeroBytes));
		}
	}
	public abstract class ElementoComplejoBinario : ElementoBinario
	{
		Llista<ElementoBinario> partesElemento;
		public ElementoComplejoBinario()
		{
			partesElemento = new Llista<ElementoBinario>();
		}
		public Llista<ElementoBinario> PartesElemento {
			get { return partesElemento; }
			private set { partesElemento = value; }
		}
		public override object GetObject(Stream bytes)
		{
			object[] parts = new object[partesElemento.Count];
			long numNull = 0;
			for (int i = 0; i < parts.Length; i++) {
				parts[i] = partesElemento[i].GetObject(bytes);
				if (parts[i] == null)
					numNull++;
			}
			if (numNull != parts.Length)
				return GetObject(parts);
			else
				return null;
		}
		public abstract object GetObject(Object[] parts);
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
		byte longitudByte;
		ushort longitudUShort;
		uint longitudUInt;
		long longitudLong;
		ElementoBinario elemento;
		private ElementoIEnumerableBinario(ElementoBinario elemento, LongitudBinaria longitud)
		{
			Elemento = elemento;
			Longitud = longitud;
			MarcaFin = null;
		}
		public ElementoIEnumerableBinario(ElementoBinario elemento, byte[] marcaFin)
			: this(elemento, LongitudBinaria.MarcaFin)
		{
			MarcaFin = marcaFin;
		}
		public ElementoIEnumerableBinario(ElementoBinario elemento, byte longitud)
			: this(elemento, LongitudBinaria.Byte)
		{
			LongitudByte = longitud;
		}
		public ElementoIEnumerableBinario(ElementoBinario elemento, ushort longitud)
			: this(elemento, LongitudBinaria.UShort)
		{
			LongitudUShort = longitud;
		}
		public ElementoIEnumerableBinario(ElementoBinario elemento, uint longitud)
			: this(elemento, LongitudBinaria.UInt)
		{
			LongitudUInt = longitud;
		}
		public ElementoIEnumerableBinario(ElementoBinario elemento, long longitud)
			: this(elemento, LongitudBinaria.Long)
		{
			LongitudLong = longitud;
		}
		/// <summary>
		/// Sirve para acabar la lectura sin saber cuantos elementos abran, si es null la marcaFin es 0x00
		/// </summary>
		public byte[] MarcaFin {
			get { return marcaFin; }
			set {
				if (value == null)
					value = new byte[] { 0x00 };
				marcaFin = value;
                Longitud = LongitudBinaria.MarcaFin;
			}
		}
		public LongitudBinaria Longitud {
			get { return longitud; }
			set { longitud = value; }
		}
		public long LongitudLong {
			get { return longitudLong; }
			set { longitudLong = value;
            Longitud = LongitudBinaria.Long;
            }
		}
		public uint LongitudUInt {
			get { return longitudUInt; }
            set { longitudUInt = value; Longitud = LongitudBinaria.UInt; }
		}
		public ushort LongitudUShort {
			get { return longitudUShort; }
            set { longitudUShort = value; Longitud = LongitudBinaria.UShort; }
		}
		public byte LongitudByte {
			get { return longitudByte; }
            set { longitudByte = value; Longitud = LongitudBinaria.Byte; }
		}

		public ElementoBinario Elemento {
			get { return elemento; }
			set {
				if (value == null)
					throw new ArgumentNullException();
				elemento = value;
			}
		}

		public override byte[] GetBytes(object obj)
		{
			if (!(obj is IEnumerable))
				throw new TipoException("El objeto no es IEnumerable<object>");
			List<byte> bytesObj = new List<byte>();
			long numItems = 0;
			foreach (object partObj in (IEnumerable)obj) {
				numItems++;
				bytesObj.AddRange(Elemento.GetBytes(partObj));
			}
			switch (Longitud) {
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
					bytesObj.AddRange(MarcaFin);
					break;
			}
			return bytesObj.ToArray();
		}

		public override object GetObject(Stream bytes)
		{
			//la marca fin y la longitud Que Se usara  y el elemento es el minimo...
			long numItems = -1;
			List<object> objects = new List<object>();
            Llista<byte> compruebaBytes = new Llista<byte>();
			List<byte> bytesElementoMarcaFin = new List<byte>();
			bool continua = true;
			Object objHaPoner = null;
            String marcaFiHex;
            Object[] partes = null;
			switch (Longitud) {
				case LongitudBinaria.Byte:
					numItems = bytes.ReadByte();
					break;
				case LongitudBinaria.UShort:
					numItems = Serializar.ToUShort(bytes.Read(2));
					break;
				case LongitudBinaria.UInt:
					numItems = Serializar.ToUShort(bytes.Read(4));
					;
					break;
				case LongitudBinaria.Long:
					numItems = Serializar.ToUShort(bytes.Read(8));
					break;
			}
			if (numItems >= 0) {
				for (long i = 0; i < numItems; i++) {
					objHaPoner = Elemento.GetObject(bytes);
					if (objHaPoner != null)
						objects.Add(objHaPoner);
				}
                if (objects.Count == 0)
                    objHaPoner = null;
			} else {
				//usa marca fin
                marcaFiHex = MarcaFin.ToHex();
				//pongo el byte en la cola
				//miro si coincide con la marca fin
				//si no coincide cojo el primer byte
				//si coincide dejo de añadir bytes
				do {
					compruebaBytes.Afegir((byte)bytes.ReadByte());
					if (compruebaBytes.Count == marcaFin.Length) {
						continua = compruebaBytes.ToHex() != marcaFiHex;
						if (continua)
							bytesElementoMarcaFin.Add(compruebaBytes.Pop());
					}
				} while (continua && bytes.CanRead);
				//ahora tengo los bytes tengo que obtener los elementos
				bytes = new MemoryStream(bytesElementoMarcaFin.ToArray());
               
				do {
					objHaPoner = Elemento.GetObject(bytes);
					if (objHaPoner != null)
						objects.Add(objHaPoner);
				} while (objHaPoner != null&&!bytes.EndOfStream());
                if (objects.Count == 0)
                    objHaPoner = null;

			}
			if (objHaPoner != null)
				partes= objects.ToArray();
            return partes;

		}
	}

	//clases concretas pasadas al sistema
	public class BitmapBinario : ElementoComplejoBinario
	{
		public BitmapBinario()
		{
			PartesElemento.Afegir(new ElementoIEnumerableBinario(ElementosTipoAceptado(Serializar.TiposAceptados.Byte),(uint) 0));
		}

		public override object GetObject(object[] parts)
		{
			Bitmap bmp = null;
            if (parts.Length == 1 && parts[0] is object[])
                bmp = new Bitmap(new MemoryStream(((object[])parts[0]).Casting<byte>().ToArray()));
			return bmp;
		}

		public override byte[] GetBytes(object obj)
		{
            List<byte> bytes=new List<byte>();
            if (obj != null)
            {
                bytes.AddRange( ((Bitmap)obj).ToStream(System.Drawing.Imaging.ImageFormat.Jpeg).GetAllBytes());
                bytes.InsertRange(0, Serializar.GetBytes(Convert.ToUInt32(bytes.Count)));
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
		}
		public StringBinario(byte[] marcaFin)
			: base(ElementosTipoAceptado(Serializar.TiposAceptados.Char), marcaFin)
		{
		}
		public override object GetObject(Stream bytes)
		{
			object[] caracteres = (object[])base.GetObject(bytes);
			StringBuilder str = new StringBuilder();
			for (int i = 0; i < caracteres.Length; i++)
				str.Append(caracteres[i].ToString());
			return str.ToString();
		}
        public override byte[] GetBytes(object obj)
        {
            List<object> caracteres = new List<object>();
            string str = obj as string;
            for (int i = 0; i < str.Length; i++)
                caracteres.Add(str[i]);
            return base.GetBytes(caracteres);
        }

		
	}
	public class DateTimeBinario:ElementoSimpleBinario
	{
		public DateTimeBinario()
			: base(Serializar.TiposAceptados.Long, 8)
		{
		}
		public override object GetObject(Stream bytes)
		{
			return new DateTime((long)base.GetObject(bytes));
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

}
