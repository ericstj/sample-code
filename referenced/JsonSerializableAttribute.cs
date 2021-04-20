using System;
using System.Text.Json.Serialization;

namespace System.Text.Json.Serialization
{
  public class JsonSerializableAttribute : Attribute 
  {
      public JsonSerializableAttribute(Type targetType) {}
  }
}

