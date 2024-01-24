# dotnet orm
Improves code readability and removes basic queries from the code.
## Installing
To install the library, you can use NuGet and find the library named [**theandru-dotnet-orm**](https://www.nuget.org/packages/theandru-dotnet-orm).

### .NET CLI
```
dotnet add package theandru-dotnet-orm
```

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

```csharp
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
```

## License
This project is licensed under [MIT license](https://mit-license.org/)