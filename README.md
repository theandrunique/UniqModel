# UniqModel
Improves code readability and removes basic queries from the code.
## Installing
To install the library, you can use NuGet and find the library named [**UniqModel**](https://www.nuget.org/packages/UniqModel/).

### .NET CLI
```
dotnet add package UniqModel
```

### Installation from source

1. Clone the project

```
git clone https://github.com/theandrunique/UniqModel.git
```

2. Navigate to the directory

```
cd UniqModel/
```
3. Compile the library

- Run the solution file **UniqModel.sln** and compile the library
- or use .NET CLI to compile
```
dotnet build
```

4. Add a link to the created binary file in you project

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
Core dbcore = new Core(DatabaseEngine.Sqlite, "Data Source=database.db");
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