using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
using System.IO;
namespace Gabriel.Cat.Binaris
{
    /// <summary>
    /// Sirve para crar archivos y cargarlos sin saber como estan hechos (guarda mas informacion)
    /// </summary>
   public class FormatoSimple:IElementoBinario
    {
       byte[] firma;
       ElementoBinarioDescriptivo este;
       public FormatoSimple() : this(Formato.firmaDefault) { }
       public FormatoSimple(byte[] firma)
       {
           this.firma = firma;

           este = new ElementoBinarioDescriptivo(this);
       }
       public object[] Objects
       {
           get { return este.Objectes; }
       }
       public void Vaciar()
       {
           este.Vaciar();
       }
       public void Añadir(IEnumerable<object> objs)
       {
           este.Añadir(objs);
       }
       public void Añadir(object obj)
       {
           este.Añadir(obj);

       }
       public void Quitar(IEnumerable<object> objs)
       {
           if (objs != null)
               foreach (object obj in objs)
                   Quitar(obj);
       }
       public void Quitar(object obj)
       {
           este.Quitar(obj);
       }
       #region Miembros de IElementoBinario

       public byte[] GetBytes()
       {
           List<byte> bytes = new List<byte>(firma); 
           bytes.AddRange(este.GetBytes());
           return bytes.ToArray();
       }
       public byte[] GetBytes(IEnumerable<Object> objs)
       {
           IEnumerable<Object> objsAnt = this.este.Objectes;
           byte[] bytesObjs;
           Vaciar();
           Añadir(objs);
           bytesObjs = GetBytes();
           Vaciar();
           Añadir(objsAnt);
           return bytesObjs;

       }
       public void SetBytes(byte[] bytes)
       {
           MemoryStream ms = new MemoryStream(bytes);
           SetBytes(ms);
           ms.Close();
        
       }
       public void SetBytes(Stream stream)
       {
           if (firma.ToHex() != stream.Read(firma.Length).ToHex())
               throw new TipoException("La firma no coincide");
           este.SetBytes(stream);
       }
       public Object[] GetObjects(Stream stream)
       {
           SetBytes(stream);
           return this.Objects;
       }

       #endregion
    }
   public class ElementoBinarioDescriptivo
   {
       public static bool ComprobarAssemblyName = true;
       StringBinario assemblyName;
       ElementoIEnumerableBinario datos;
       IElementoBinario objecte;
       List<object> objs;
       const string ASSEMBLYNULL = "NULL";
       public ElementoBinarioDescriptivo(IElementoBinario obj)
       {
           Objecte = obj;
           assemblyName = new StringBinario();
           datos = new ElementoIEnumerableBinario(ElementoBinario.ElementosTipoAceptado(Serializar.TiposAceptados.Byte), (long)0);
           objs=new List<object>();
       }
       public IElementoBinario Objecte
       {
           get { return objecte; }
           set { objecte = value; }
       }
       public Object[] Objectes
       {
           get{return objs.ToArray();}
       }
       public void Vaciar()
       {
           objs.Clear();
       }
       public void Añadir(IEnumerable<object> objs)
       {
           foreach(object obj in objs)
               Añadir(obj);
       }
       public void Añadir(object obj)
       {
           if(obj!=null&&!(obj is IElementoBinario || Serializar.AsseblyQualifiedNameTiposMicrosoft.Contains(obj.GetType().AssemblyQualifiedName)))
               throw new TipoException("El objeto no se puede serializar con el sistema actual!");
          objs.Add(obj);

       }
       public void Quitar(IEnumerable<object> objs)
       {
           if(objs!=null)
           foreach(object obj in objs)
               Quitar(obj);
       }
       public void Quitar(object obj)
       {
           objs.Remove(obj);
       }
       public  byte[] GetBytes()
       {
           List<byte> bytesObj = new List<byte>();
           bytesObj.AddRange(assemblyName.GetBytes(Objecte.GetType().AssemblyQualifiedName));
           if(objs!=null)
               foreach (object obj in objs)
               {
                   if (obj != null)
                   {
                       bytesObj.AddRange(assemblyName.GetBytes(obj.GetType().AssemblyQualifiedName));//nombre
                       if (!(obj is IElementoBinario))//datos
                           bytesObj.AddRange(datos.GetBytes(Serializar.GetBytes(obj)));
                       else
                           bytesObj.AddRange(datos.GetBytes(((IElementoBinario)obj).GetBytes()));
                   }
                   else
                   {
                       bytesObj.AddRange(assemblyName.GetBytes(ASSEMBLYNULL));//nombre
                   }
               }
           return bytesObj.ToArray();
       }

       public  void SetBytes(System.IO.Stream bytes)
       {
           //Assembly name objecte
           string assemblyNameObjecte = assemblyName.GetObject(bytes) as string;
           if (ComprobarAssemblyName && Objecte.GetType().AssemblyQualifiedName != assemblyNameObjecte)
               throw new TipoException("El objeto ha deserializar no es el que se serializo...");
           //Objectes->AssembyName,byte[]
           string assemblyNamePartObjecte;
           byte[] bytesPartObject;
           Type tipo;
           IElementoBinario parteObj;
           objs.Clear();
           while (!bytes.EndOfStream())
           {
               //AssemblyName
               assemblyNamePartObjecte = assemblyName.GetObject(bytes) as string;
               if (assemblyNamePartObjecte != ASSEMBLYNULL)
               {
                   //bytes
                   bytesPartObject = ((object[])datos.GetObject(bytes)).Casting<byte>().ToArray();
                   //si es tipoAceptado
                   if (Serializar.AsseblyQualifiedNameTiposMicrosoft.Contains(assemblyNamePartObjecte))
                   {
                       objs.Add(Serializar.ToTipoAceptado(assemblyNamePartObjecte, bytesPartObject));
                   }
                   else
                   {
                       tipo = Type.GetType(assemblyNamePartObjecte);
                       parteObj = Activator.CreateInstance(tipo) as IElementoBinario;
                       if (parteObj != null)
                       {
                           parteObj.SetBytes(bytesPartObject);
                           objs.Add(parteObj);
                       }
                   }
               }
               else objs.Add(null);

           }
           


       }

   }
   public interface IElementoBinario
   {
       byte[] GetBytes();
       void SetBytes(byte[] bytes);
   }
}
