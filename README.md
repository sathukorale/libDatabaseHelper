# libDatabaseHelper ([![Build status](https://ci.appveyor.com/api/projects/status/0iepmbwxl4uonvr5/branch/master?svg=true)](https://ci.appveyor.com/project/sathukorale1379/libdatabasehelper/branch/master), [![Coverity Status](https://scan.coverity.com/projects/7734/badge.svg)](https://scan.coverity.com/projects/sathukorale-libdatabasehelper), [![Coverage Status](https://coveralls.io/repos/github/sathukorale/libDatabaseHelper/badge.svg?branch=master)](https://coveralls.io/github/sathukorale/libDatabaseHelper?branch=master)) 
A simple C# .NET library to quicken the development of database accessing/related applications. With the use of this library you can make the management of the database tables and the corresponding code respresenting the tables much easier, faster and manageable. For example if you have multiple classes, each representing a table on the database, you do not have to write the same code on thoses classes for doing the same thing(for example, the add, remove or update methods). You can even make the creation of windows forms, for editing or displaying entities much easier (the included generator will literally generate a generic form for you). In summary, the library is developed in the thought of making interactions with different databases much easier and friendlier.

## Example :

Take this class for instance. The class _TableName_ will repsent the table _TableName_ on the database, and fields (with TableColumn attribute on top) represents the columns.
```cs
public class TableName : DatabaseEntity
{
  [TableColumn(true, true, 30, IsAutogenerated = false, GridDisplayName="Primary Key")]
  public String PrimaryKeyField; // A primary key field that is of type string(varchar) and of maximum length 30

  [TableColumn(false, true, IsAuditVisible = false, IsRetrievableFromDatabase = false)]
  public String OtherField1; // A field that won't be displayed on the audit trail will not be retrieved from the database on select query

  [TableColumn(false, true, GridDisplayName = "Other Field 2")]
  public int OtherField2; // A normal integer field
}
```
You can create the above table using the following lines. However the lines below, should come before any code that deals with the table or the class _TableName_.

```cs
GenericConnectionManager.RegisterConnectionManager<libDatabaseHelper.classes.sqlce.ConnectionManager>(); // Installing the ConnectionManager that will be handling the SQL CE connections
GenericDatabaseManager.RegisterDatabaseManager<libDatabaseHelper.classes.sqlce.DatabaseManager>(); // Installing the DatabaseManager that will be handling SQL CE DatabaseEntities.

var dbFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\libDBHelderSampleFolder1\\SampleDatabase1.sdf";

// Setting the default connection string for any sql ce databases (Alternatively you can set for one specific type)
GenericConnectionManager.GetConnectionManager(DatabaseType.SqlCE).SetConnectionString("Data Source=" + dbFile + ";Persist Security Info=False;");

// Creating the table in the specified database
GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<TableName>();
```

Which will generate the following table for you,

| *           | PrimaryKeyField | OtherField1 | OtherField2 |
|-------------|:---------------:|:-----------:|:-----------:|
| Primary Key | True            | False       | False       |
| Data Type   | NVarChar        | NVarChar    | Integer     |

The code below should give a brief example as to how you can work with _TableName_ in retrieving, updating, modifying and deleting data.

```cs
var entry = new TableName { PrimaryKeyField = "KeyValue1", OtherField1 = "NormalValue1", OtherField2 = 1 };
entry.Add(); // Adding the entry into the database

var selectors = new[] { new Selector("PrimaryKeyField", entry.PrimaryKeyField) }; // Filters, that usually go after the where clause of a select statement
var fetched_entries = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<TableName>(selectors); // Selected entries from the database
var fetched_entry = fetched_entries.FirstOrDefault() as TableName;

fetched_entry.OtherField1 = "Updated Value";
fetched_entry.Update(); // Updating the entry on the database

fetched_entry.Remove(); // Removing the entry from the database
```

## TODO List : 
* Extend the library to support MS SQL, Oracle and SQLite
* Implement the functionality to have auto-generated primary keys.
* Update the library to detect column data-type chanegs so that the tables can be modified, to suit the data type specified.
* The library does not support, storing encrypted data/passwords (however the developer can go around and develop his own extension). The capability to store encrypted data should be introduced.
* Most of the database exceptions are not captured via the library, even though the code attempts to prevent some causes of database exceptions (checking constraints and references, etc). Meastures should be applied to either capture these exceptions or prevent them prior execution
* The DatabaseEntityFormGenerator needs to be updated to generate customized forms (themes etc.)

## Requirements : 
* Windows (atleast for the moment, due to certain limitations)
* .NET 4.0 (The oldest version supported is .NET 4.0)
* The following list libraries will depend on the supported database list
  * SQL Compact Edition 4.0
