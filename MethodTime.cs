<PackageReference Include="MethodTimer.fody" Version="3.2.1" />
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services //.AddScoped<DataProtectionSample>()
        .AddMediatR(typeof(Program)) // does not work after next line
        .AddHangfire(x => x.UseSqlServerStorage("<connection string>")) //??
        .AddHangfireServer() //??
        .AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("c:/temp_keys")) // without this, the provider.GetDataProtector() will throw exception: System.InvalidOperationException: 'The 'IXmlRepository' instance could not be found. 
        //.SetDefaultKeyLifetime(TimeSpan.FromDays(10)) // at least one week; defaults to 90 days
        .ProtectKeysWithDpapi(protectToLocalMachine: true) // when defaulted to false: only the current Windows user account can decipher the persisted key ring
        .SetApplicationName($"MyAmazingApp-{appName}")
        )
    .Build();

public static class MethodTimeLogger
{
    public static void Log(MethodBase methodBase, long milliseconds, string message)
    {
        Console.WriteLine($"[{methodBase.DeclaringType.Name}.{methodBase.Name}] {message} {milliseconds} ms");
    }
}
				