using Microsoft.Management.Infrastructure;

ulong sizeInKB = 0;         
EnumInstances("SELECT * FROM Win32_PhysicalMemory", instance => sizeInKB += Convert.ToUInt64(instance.CimInstanceProperties["Capacity"].Value) / 1024);
Console.WriteLine(sizeInKB);

EnumInstances("SELECT * FROM Win32_OperatingSystem", os => Console.WriteLine($"Last boot time: {os.CimInstanceProperties["LastBootUpTime"].Value}"));


void EnumInstances(string query, Action<CimInstance> action)
{
    using var session = CimSession.Create(null);   
    foreach (CimInstance instance in session.QueryInstances(@"root\cimv2", "WQL", query))
    {
        using (instance)
        {
            action(instance);
        }
    }
}