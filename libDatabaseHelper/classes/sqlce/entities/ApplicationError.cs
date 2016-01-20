using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.classes.sqlce.entities
{
    [TableProperties(Registration = TableProperties.RegistrationType.RegisterOnDatabaseManager)]
    public class ApplicationError : DatabaseEntity
    {
        [TableColumn(false, true)]
        public string ExceptionString;

        [TableColumn(false, true)]
        public DatabaseEntity Entity;
    }
}
