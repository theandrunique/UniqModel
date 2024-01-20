

using NLog;
using NLog.Targets;

namespace SQLModel.Tests
{
    public class SqliteProviderTests
    {
        Logger log = LogManager.GetCurrentClassLogger();

        [Fact]
        public void CreateTablesSqliteTest()
        {
            if (File.Exists("orm.log"))
            {
                File.Delete("orm.log");
            }
            if (File.Exists("test.db"))
            {
                File.Delete("test.db");
            }
            if (File.Exists("testasync.db"))
            {
                File.Delete("testasync.db");
            }

            var config = new NLog.Config.LoggingConfiguration();
            var logFile = new FileTarget("logFile") { FileName = $"orm.log" };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
            LogManager.Configuration = config;

            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", log, dropErrors: true);

            core.Metadata.CreateAll();
        }
        [Fact]
        public void CheckTables()
        {
            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", log, dropErrors: true);
            using (var session = core.CreateSession())
            {
                session.ExecuteNonQuery("select * from logins");
                session.ExecuteNonQuery("select * from profiles");
            }
            Assert.Equal<int>(2, Metadata.TableClasses.Count);
        }
        [Fact]
        public void CrudOperationsTests()
        {
            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", log, dropErrors: true);
            using (var session = core.CreateSession())
            {
                var profile = new ProfilesTable()
                {
                    Name = "Name",
                    Description = "Description",
                };
                session.Add(profile);

                var login = new LoginsTable()
                {
                    ProfileId = 1,
                };
                session.Add(login);
            }

            using (var session = core.CreateSession())
            {
                var login1 = session.GetById<LoginsTable>(2);

                Assert.Equal(0, login1.Id);

                var login = session.GetById<LoginsTable>(1);

                Assert.NotNull(login);

                var profile = session.GetById<ProfilesTable>(login.ProfileId);

                profile.Name = "new name";

                profile.Description = "new description";

                session.Update(profile);
            }
        }
        [Fact]
        public async void CrudAsyncOperationsTests()
        {
            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=testasync.db", log, dropErrors: true);

            core.Metadata.CreateAll();

            using (var session = await core.CreateAsyncSession())
            {
                var profile = new ProfilesTable()
                {
                    Name = "Name",
                    Description = "Description",
                };
                await session.Add(profile);

                var login = new LoginsTable()
                {
                    ProfileId = 1,
                };
                await session.Add(login);
            }

            using (var session = await core.CreateAsyncSession())
            {
                var login1 = await session.GetById<LoginsTable>(2);

                Assert.Equal(0, login1.Id);

                var login = await session.GetById<LoginsTable>(1);

                Assert.NotNull(login);

                var profile = await session.GetById<ProfilesTable>(login.ProfileId);

                profile.Name = "new name";

                profile.Description = "new description";

                await session.Update(profile);
            }
        }
        public class LoginsTable : BaseModel
        {
            public static new string Tablename = "logins";
            [PrimaryKey()]
            public int Id { get; set; }
            [ForeignKey("profiles.Id")]
            public int ProfileId { get; set; }
        }
        public class ProfilesTable : BaseModel
        {
            public static new string Tablename = "profiles";
            [PrimaryKey()]
            public int Id { get; set; }
            [Field()]
            public string? Name { get; set; }
            [Field()]
            public string? Description { get; set; }
        }
    }
}