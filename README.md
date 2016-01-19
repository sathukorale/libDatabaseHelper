# libDatabaseHelper
A simple C# .NET library to quicken the development of database accessing/related applications. With the use of this library you can make the management of the database tables and the corresponding code respresenting the tables much easier, faster and manageable. For example if you have multiple classes, each representing a table on the database, you do not have to write the same code on thoses classes for doing the same thing(for example, the add, remove or update methods). You can even make the creation of windows forms, for editing or displaying entities much easier (the included generator will literally generate a generic form for you). In summary, the library is developed in the thought of making interactions with different databases much easier and friendlier.

## Example Usage :
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

## TODO List : 
* Implement the same for MySQL (a work in progress), SQL and Oracle
* Implementing auto-generated primary keys.
* Identifying column data type changes (especially when checking the table structure for any modifications)
* Capability to store encrypted password values. 
* On Update both the previous and the updated should be returned. 

## Issues :
* *Pending...*

## Current Build and Tests Status
Latest Build Status (via Snap CI): [![Build Status](https://snap-ci.com/qJdv1q8E_fvKesyStj8nyhtXGwsk8nEbV0h_6g317Es/build_image)](https://snap-ci.com/sathukorale/libDatabaseHelper/branch/master)
