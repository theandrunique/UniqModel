# dotnet orm
Improves code readability and removes basic queries from the code.
## Requirements
The project was built on the .NET Framework 4.8 platform, and I currently have no ideas on how to make it compatible with all versions. However, it also works on newer versions such as NET 6.0 and above.
## Installing
To install the library, you can use NuGet and find the library named **theandru-dotnet-orm**.

### Installation from source

1. Clone the project
```
git clone https://github.com/theandrunique/dotnet_orm.git
```
2. Navigate to the directory
```
cd dotnet_orm/
```
3. Run the solution file **SQLModel.sln**
4. Compile the library
5. Add a link to the created binary file in you project

## Usage

### Table Representation

```
// All models should inherit from BaseModel
class AccountTable : BaseModel
{
    // A primary key
    [PrimaryKey()]
    public int Id { get; set; }
    // Other fields
    [Field()]
    public string Nickname { get; set; }
    [Field()]
    public string NicknameShortcut { get; set; }
}
```

In the database, it would be something like this:
 - Table AccountTable
 - Id - int, primary key, auto-increment
 - Nickname - text
 - NicknameShortcut - text

#### Interact with the database

```
int main()
{
    // Create an instance of Core
    Core dbcore = Core(DatabaseEngine.Sqlite, "Data Source=database.db");
    // Create a session
    // You should use a context manager
    using (Session session = dbcore.CreateSession())
    {
        // At this line, you already have a connection to the database and a transaction

        // Create an instance of the AccountTable class
        AccountTable account = new AccountTable()
        {
            Nickname = "a nickname",
            NicknameShortcut = "a shortcut",
        };

        // Add the object to the table
        session.Add(account);
    }
    // At this line, a new record has been added to the database
    // The connection is closed, and the transaction is committed
}
```
