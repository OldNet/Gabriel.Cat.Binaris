using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
using System.IO;
using System.Collections;

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
       public FormatoSimple(byte[] firma) : this(null, firma) { }
       public FormatoSimple(IElementoBinario parent) : this(parent,Formato.firmaDefault) { }
       public FormatoSimple(IElementoBinario parent,byte[] firma)
       {
           if(parent==null)
             parent=this;
           this.firma = firma;
           este = new ElementoBinarioDescriptivo(parent);
       }
       public IElementoBinario Parent
       {
           get { return este.Objecte; }
           set { este.Objecte = value; }
       }
       public object[] Objects
       {
           get { return este.Objectes; }
           set { este.Objectes = value; }
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

       public void SetBytes(MemoryStream stream)
       {
           string firmaLeidaHex= (Hex)stream.Read(firma.Length);
           if ((Hex)firma !=firmaLeidaHex )
               throw new TipoException("La firma no coincide");
           este.SetBytes(stream);
       }
       public Object[] GetObjects(byte[] bytes)
       {
           this.SetBytes(bytes);
           return this.Objects;
       }
       public Object[] GetObjects(MemoryStream stream)
       {
           SetBytes(stream);
           return this.Objects;
       }

       #endregion

       #region Miembros de IElementoBinario


       public void SetObjects(object[] objs)
       {
           este.Vaciar();
           este.Añadir(objs);
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
           get { return objs.ToArray(); }
           set { Vaciar(); Añadir(value); }
           
       }
       public void Vaciar()
       {
           objs.Clear();
       }
       public void Añadir(IEnumerable<object> objs)
       {
           if(objs!=null)
           foreach(object obj in objs)
               Añadir(obj);
       }
       public void Añadir(object obj)
       {
           if(obj!=null&&!(obj is IElementoBinario || ((IList<string>)Serializar.AsseblyQualifiedNameTiposMicrosoft).Contains(obj.GetType().AssemblyQualifiedName)))
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
           List<object> enumObjs = new List<object>(objs);
           bytesObj.AddRange(assemblyName.GetBytes(Objecte.GetType().AssemblyQualifiedName));
           if(objs!=null)
               foreach (object obj in enumObjs)
               {
                   if (obj != null)
                   {
                       bytesObj.AddRange(assemblyName.GetBytes(obj.GetType().AssemblyQualifiedName));//nombre
                       if (!(obj  is IElementoBinario))//datos
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

       public  void SetBytes(MemoryStream bytes)
       {
           //Assembly name objecte
           string assemblyNameObjecte = assemblyName.GetObject(bytes) as string;
           if (ComprobarAssemblyName && Objecte.GetType().AssemblyQualifiedName != assemblyNameObjecte)
               throw new TipoException("El objeto ha deserializar no es el que se serializo...");
           objecte = Activator.CreateInstance(Type.GetType(assemblyNameObjecte)) as IElementoBinario;
           //Objectes->AssembyName,byte[]
           string assemblyNamePartObjecte;
           byte[] bytesPartObject;
           Type tipo;
           IElementoBinario parteObj;
           Object[] partsObjIElementoBinario;
           ElementoBinarioDescriptivo subElemento;

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
                   if (((IList<string>)Serializar.AsseblyQualifiedNameTiposMicrosoft).Contains(assemblyNamePartObjecte))
                   {
                       objs.Add(Serializar.ToTipoAceptado(assemblyNamePartObjecte, bytesPartObject));
                   }
                   else
                   {
                       tipo = Type.GetType(assemblyNamePartObjecte);
                       parteObj = Activator.CreateInstance(tipo) as IElementoBinario;
                       if (parteObj != null)
                       {
                           subElemento = new ElementoBinarioDescriptivo(parteObj); 
                           subElemento.SetBytes(bytesPartObject);
                           partsObjIElementoBinario = subElemento.Objectes;
                           parteObj.SetObjects(partsObjIElementoBinario);
                           objs.Add(parteObj);
                       }
                   }
               }
               else objs.Add(null);

           }

           objecte.SetObjects(objs.ToArray());

       }

       public void SetBytes(byte[] bytes)
       {
           SetBytes(new MemoryStream(bytes));
       }

   }
   public interface IElementoBinario
   {
       byte[] GetBytes();
       void SetBytes(MemoryStream bytesFile);
       void SetObjects(Object[] objs);
   }
}
