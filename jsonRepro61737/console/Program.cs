using ClassLibrary1;

MyModel model1 = new MyModel();

string json1 = Serializer.Serialize(model1);
string json2 = Serializer.SerializeWithGenerics(model1);

Console.WriteLine(json1);
Console.WriteLine(json2);
Console.ReadKey();