using Microsoft.Data.Sqlite;
using NLog;
using NLog.Targets;

namespace SQLModel.Tests
{
    public class SqliteProviderTests
    {
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
        [Fact]
        public async void SqliteTests()
        {
            // deleting the logs and database of the last test
            if (File.Exists("orm.log"))
            {
                File.Delete("orm.log");
            }
            if (File.Exists("test.db"))
            {
                File.Delete("test.db");
            }

            // creating an instance of logger
            Logger log = LogManager.GetCurrentClassLogger();

            // setting up the logger
            var config = new NLog.Config.LoggingConfiguration();
            var logFile = new FileTarget("logFile") { FileName = $"orm.log" };
            var logconsole = new ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logFile);
            LogManager.Configuration = config;

            // creating an instance of core
            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", log, dropErrors: true);

            // creating tables
            core.Metadata.CreateAll();

            // start a new session
            using (var session = core.CreateSession())
            {
                // creating an instance of ProfilesTable
                ProfilesTable profile = new ProfilesTable()
                {
                    Name = "Name",
                    Description = "Description",
                };

                // add element to the table
                session.Add(profile);

                // creating an instance of LoginsTable
                var login = new LoginsTable()
                {
                    ProfileId = 1,
                };

                // add element to the table
                session.Add(login);
            }

            using (var session = core.CreateSession())
            {
                // an attempt to get an object that does not exist
                LoginsTable login1 = session.GetById<LoginsTable>(2);
                Assert.Null(login1);

                // get an existing element
                LoginsTable login = session.GetById<LoginsTable>(1);
                Assert.NotNull(login);

                ProfilesTable profile = session.GetById<ProfilesTable>(login.ProfileId);

                // update the fields
                profile.Name = "new name";
                profile.Description = "new description";

                // save changes
                session.Update(profile);
            }

            using (var session = core.CreateSession())
            {
                List<LoginsTable> listLogins = session.GetAll<LoginsTable>();
                Assert.Single(listLogins);

                List<ProfilesTable> listProfiles = session.GetAll<ProfilesTable>();
                Assert.Single(listProfiles);

                Assert.Equal("new name", listProfiles[0].Name);
                Assert.Equal("new description", listProfiles[0].Description);

                // an error due to a foreign key constraint
                Assert.Throws<SqliteException>(() =>
                {
                    session.Delete(listProfiles[0]);
                });

                session.Delete(listLogins[0]);
                session.Delete(listProfiles[0]);
            }


            // the same test with the AsyncSession
            using (var session = await core.CreateAsyncSession())
            {
                ProfilesTable profile = new ProfilesTable()
                {
                    Name = "Name",
                    Description = "Description",
                };

                await session.Add(profile);

                var login = new LoginsTable()
                {
                    ProfileId = 2,
                };

                await session.Add(login);
            }

            using (var session = await core.CreateAsyncSession())
            {
                LoginsTable login1 = await session.GetById<LoginsTable>(3);
                Assert.Null(login1);

                LoginsTable login = await session.GetById<LoginsTable>(2);
                Assert.NotNull(login);

                ProfilesTable profile = await session.GetById<ProfilesTable>(login.ProfileId);

                profile.Name = "new name";
                profile.Description = "new description";

                await session.Update(profile);
            }

            using (var session = await core.CreateAsyncSession())
            {
                List<LoginsTable> listLogins = await session.GetAll<LoginsTable>();
                Assert.Single(listLogins);

                List<ProfilesTable> listProfiles = await session.GetAll<ProfilesTable>();
                Assert.Single(listProfiles);

                Assert.Equal("new name", listProfiles[0].Name);
                Assert.Equal("new description", listProfiles[0].Description);

                await Assert.ThrowsAsync<SqliteException>(async () =>
                {
                    await session.Delete(listProfiles[0]);
                });

                await session.Delete(listLogins[0]);
                await session.Delete(listProfiles[0]);
            }
        }
    }
}