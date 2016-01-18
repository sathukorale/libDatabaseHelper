using libDatabaseHelper.classes.sqlce;

namespace libDatabaseHelper.classes.generic.entities
{
    public class ApplicationError : DatabaseEntity
    {
        [TableColumn(false, true)]
        public string ExceptionString;

        [TableColumn(false, true)]
        public DatabaseEntity Entity;
    }
}
