using System;
using System.Linq;

using libDatabaseHelper.classes.sqlce;

namespace libDatabaseHelper.classes.generic.entities
{
    public class User : DatabaseEntity, IComboBoxItem
    {
        [TableColumn(true, true, 30, IsAutogen = false)]
        public String Username;

        [TableColumn(false, true, IsAuditVisible = false, IsRetrievable = false)]
        public String Password;

        [TableColumn(false, true, DisplayName = "First Name")]
        public String FirstName;

        [TableColumn(false, true, DisplayName = "Last Name")]
        public String LastName;

        [TableColumn(false, true, DisplayName = "Is Administrator")]
        public bool IsAdmin;

        private static User _loggedInUser;

        public bool ValidateUser()
        {
            var found = DatabaseManager.Select<User>(new[] {new Selector("Username", Username), new Selector("Password", Password), new Selector("CanLogin", true)});
            if (! found.Any()) 
                return false;
            _loggedInUser = (User)found.First();
            AuditEntry.AddAuditEntry(_loggedInUser, "Logged into the system...");
            Password = null;
            return true;
        }

        public static User GetLoggedInUser()
        {
            return _loggedInUser;
        }

        public object GetID()
        {
            return Username;
        }

        public string GetSelectQueryItems()
        {
            return "concat(concat([OBJ].FirstName, \" \"), [OBJ].LastName)";
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }

    public class AuditEntry : DatabaseEntity
    {
        [TableColumn(PrimaryKey = true, IsAutogen = true)]
        public long AuditId;

        [TableColumn()] 
        public DateTime TimeOfOccurence;

        [TableColumn(ReferencedClass = typeof(User), ReferencedField = "Username")]
        public string Username;

        [TableColumn()]
        public string Message;

        public static AuditEntry AddAuditEntry(DatabaseEntity entity, string message)
        {
            var entry = new AuditEntry
            {
                Message = message + " : " + entity.ToContentString(),
                TimeOfOccurence = DateTime.Now,
                Username = User.GetLoggedInUser() == null ? "Application" : User.GetLoggedInUser().Username
            };
            try
            {
                entry.Add();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return entry;
        }
    }
}
