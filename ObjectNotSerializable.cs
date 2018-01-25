using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using Gabriel.Cat.Extension;
using System.Collections;
namespace Gabriel.Cat.Binaris
{
	public class ObjectNotSerializable : Exception
	{
		public ObjectNotSerializable(Type typeSerializable) : base("Only can serialitze " + typeSerializable.AssemblyQualifiedName)
		{
		}
	}
}


