using System;
using System.Text.Json.Serialization;

[assembly:JsonSerializableAttribute(typeof(sameAssembly.Class1))]

namespace System.Text.Json.Serialization
{
  public class JsonSerializableAttribute : Attribute 
  {
      public JsonSerializableAttribute(Type targetType) {}
  }
}

namespace sameAssembly
{
    public class Class1
    {
    }
}
