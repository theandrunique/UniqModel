using Microsoft.Data.Sqlite;

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

            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", true, true, dropErrors: true);
        }
        [Fact]
        public void CheckTables()
        {
            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", false, true, dropErrors: true);
            using (var session = core.CreateSession())
            {
                session.ExecuteNonQuery("select * from logins");
                session.ExecuteNonQuery("select * from profiles");
            }
        }
        [Fact]
        public void CrudOperationsTests()
        {
            Core core = new Core(DatabaseEngine.Sqlite, "Data Source=test.db", false, true, dropErrors: true);
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

                Assert.Throws<SqliteException>(() =>
                {
                    session.Delete(session.GetById<ProfilesTable>(1));
                });

            }
        }
        [Table("logins")]
        public class LoginsTable : BaseModel
        {
            [PrimaryKey("id")]
            public int Id { get; set; }
            [ForeignKey("profile_id", "int", "profiles.id", typeof(ProfilesTable))]
            public int Profile_id { get; set; }
        }
        [Table("profiles")]
        public class ProfilesTable : BaseModel
        {
            [PrimaryKey("id")]
            public int Id { get; set; }
            [Field("name", "text")]
            public string Name { get; set; }
            [Field("description", "text")]
            public string Description { get; set; }
        }
    }
}