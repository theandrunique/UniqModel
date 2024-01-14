

namespace SQLModel.Tests
{
    public class SqliteProviderTests
    {
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

            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", true, dropErrors: true);

            core.Metadata.CreateAll();
        }
        [Fact]
        public void CheckTables()
        {
            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", true, dropErrors: true);
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
            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", true, dropErrors: true);
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
                    Profile_id = 1,
                };
                session.Add(login);
            }

            using (var session = core.CreateSession())
            {
                var login1 = session.GetById<LoginsTable>(2);

                Assert.Equal(0, login1.Id);

                var login = session.GetById<LoginsTable>(1);

                Assert.NotNull(login);

                var profile = session.GetById<ProfilesTable>(login.Profile_id);

                profile.Name = "new name";

                profile.Description = "new description";

                session.Update(profile);

            }
        }
        public class LoginsTable : BaseModel
        {
            public static new string Tablename = "logins";
            [PrimaryKey("id")]
            public int Id { get; set; }
            [ForeignKey("profile_id", "int", "profiles.id")]
            public int Profile_id { get; set; }
        }
        public class ProfilesTable : BaseModel
        {
            public static new string Tablename = "profiles";
            [PrimaryKey("id")]
            public int Id { get; set; }
            [Field("name", "text")]
            public string? Name { get; set; }
            [Field("description", "text")]
            public string? Description { get; set; }
        }
    }
}