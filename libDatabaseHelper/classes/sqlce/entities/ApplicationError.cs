using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.classes.sqlce.entities
{
    public class ApplicationError : DatabaseEntity
    {
        [TableColumn(false, true)]
        public string ExceptionString;

        [TableColumn(false, true)]
        public DatabaseEntity Entity;
    }
}
