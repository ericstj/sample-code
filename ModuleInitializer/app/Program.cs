using System;
using System.Reflection;

// See https://aka.ms/new-console-template for more information
var testMod = Assembly.LoadFile(Path.Combine(AppContext.BaseDirectory, "module.dll"));

Console.WriteLine($"loaded {testMod}");

var type = testMod.GetExportedTypes().Single();

Console.WriteLine($"got {type}");

var method = type.GetMethod("Test");

Console.WriteLine($"got {method}");

method.Invoke(null, null);