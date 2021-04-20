using System;
using System.Text.Json.Serialization;

[assembly:JsonSerializableAttribute(typeof(aliasingAssembly.Class2))]

namespace aliasingAssembly
{
    public class Class2
    {
    }
}
