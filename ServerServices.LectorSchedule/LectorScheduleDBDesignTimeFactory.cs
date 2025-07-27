//using MathCore.Annotations;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;

//using ServerServices.LectorSchedule;

//namespace ServerServices.Data;

//[PublicAPI]
//file class LectorScheduleDBDesignTimeDbContextFactory : IDesignTimeDbContextFactory<LectorScheduleDB>
//{
//    public LectorScheduleDB CreateDbContext(string[] args)
//    {
//        var opts = new ConfigurationBuilder()
//            .AddJsonFile("appsettings.json", true)
//            .AddEnvironmentVariables()
//            .AddCommandLine(args)
//            .Build();

//        var db_settings = opts.GetSection("db:LectorSchedule");
//        var db_type = db_settings["Type"] ?? "Sqlite";
//        var connection_string = db_settings.GetConnectionString(db_type);

//        var db_opts = new DbContextOptionsBuilder<LectorScheduleDB>();

//        if (db_type.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
//            db_opts.UseSqlite(connection_string, o => o.MigrationsAssembly(typeof(LectorScheduleDB).Assembly.GetName().Name));
//        else if (db_type.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
//            db_opts.UseSqlServer(connection_string, o => o.MigrationsAssembly(typeof(LectorScheduleDB).Assembly.GetName().Name));

//        return new(db_opts.Options);
//    }
//}
