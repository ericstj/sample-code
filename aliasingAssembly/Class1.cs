extern alias referenced;
using System;
using referenced.System.Text.Json.Serialization;

[assembly:JsonSerializableAttribute(typeof(aliasingAssembly.Class1))]

namespace aliasingAssembly
{
    public class Class1
    {
    }
}
