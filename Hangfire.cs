    <PackageReference Include="hangfire" Version="1.7.33" />
    <PackageReference Include="hangfire.core" Version="1.7.33" />
    <PackageReference Include="hangfire.sqlserver" Version="1.7.33" />

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
GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(@"Server=.\SQLEXPRESS; Database=SchoolDB; Integrated Security=True;", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true
                });
//var jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget!"));
//RecurringJob.AddOrUpdate(
//    "myrecurringjob",
//    () => Console.WriteLine("Recurring!"),
//    Cron.Minutely);
//var res = BackgroundJob.ContinueJobWith(
//    jobId,
//    () => Console.WriteLine("Continuation!"));
//Console.WriteLine($"**{res}**"); // does not print???
//var jobId2 = BackgroundJob.Schedule(
//    () => Console.WriteLine("Delayed!"),
//    TimeSpan.FromMinutes(2));
using (var server = new BackgroundJobServer())
{
    Console.ReadLine();
}
				