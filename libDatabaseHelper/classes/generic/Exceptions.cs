using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace libDatabaseHelper.classes.generic
{
    public class SelectorException : Exception
    {
    }

    public class DatabaseEntityException : Exception
    {
        private string _message;
        public DatabaseEntityException(string message)
        {
            _message = message;
        }

        public string GetMessage()
        {
            return _message;
        }
    }

    public class ValidationException : Exception
    {
        private readonly string _message;

        public ValidationException(Control control, string message)
        {
            control.Focus();
            _message = message;
        }

        public string GetErrorMessage()
        {
            return _message;
        }
    }

    public class DatabaseConnectionException : Exception
    {
        public enum ConnectionErrorType
        { 
            InvalidConnectionString,
            NoConnectionStringFound,
            NoConnectionManagerFound
        }

        private readonly ConnectionErrorType _errorType;

        public DatabaseConnectionException(ConnectionErrorType errorType)
        {
            _errorType = errorType;
        }

        public ConnectionErrorType GetErrorType()
        {
            return _errorType;
        }
    }

    public class DatabaseException : Exception
    {
        [Flags]
        public enum ErrorType
        {
            NoColumnsFound = 32,
            NoPrimaryKeyColumnsFound = 16,
            ReferenceKeyViolation = 8,
            RecordAlreadyExists = 4,
            AlreadyExistingUnqiueField = 2,
            NonExistingRecord = 1,
        }

        private readonly object _additionalData;
        private readonly ErrorType _errorType;

        public DatabaseException(ErrorType type)
        {
            _errorType = type;
        }

        public DatabaseException(ErrorType type, object additionalData)
        {
            _errorType = type;
            _additionalData = additionalData;
        }

        public ErrorType GetErrorType()
        {
            return _errorType;
        }

        public object GetAdditionalData()
        {
            return _additionalData;
        }
    }
}
