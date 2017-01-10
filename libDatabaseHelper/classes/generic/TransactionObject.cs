using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Data.SqlServerCe;

namespace libDatabaseHelper.classes.generic
{
    public class TransactionObject
    {
        private DbCommand _command;
        private DbConnection _connection;
        protected DbTransaction _transaction;
        protected bool _isCommitted;
        protected bool _isRegularCommitAllowed;

        protected TransactionObject(DbCommand command)
        {
            _command                = command;
            _connection             = command.Connection;
            _transaction            = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
            _isCommitted            = false;
            _isRegularCommitAllowed = true;

            _command.Transaction = _transaction;
        }

        protected TransactionObject(DbConnection connection)
        {
            _command                = connection.CreateCommand();
            _connection             = connection;
            _transaction            = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
            _isCommitted            = false;
            _isRegularCommitAllowed = true;

            _command.Transaction = _transaction;
        }

        public DbCommand GetCommand() 
        {
            _command.CommandText = "";
            _command.Parameters.Clear();

            return _command;
        }

        public DbConnection GetConnection() { return _connection; }

        public bool HasCommitted() { return _isCommitted; }

        public void EnableRegularCommit(bool enableRegularCommit)
        {
            _isRegularCommitAllowed = enableRegularCommit;
        }

        public virtual void Commit(bool forceCommit = false)
        {
            if (_isCommitted)
            {
                throw new InvalidOperationException("Unable to commit transaction that was already committed.");
            }

            if (forceCommit == false && _isRegularCommitAllowed == false)
            {
                return;
            }

            _isCommitted = true;
            _transaction.Commit();
        }

        public virtual void Rollback()
        {
            _transaction.Rollback();
        }

        public static TransactionObject CreateTransactionObject(DatabaseType database_type, DbCommand command)
        {
            if (database_type == DatabaseType.MySQL)
            {
                return new TransactionObject(command);
            }
            else if (database_type == DatabaseType.SqlCE)
            {
                return new SqlCETransactionObject(command);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type. Currently only 'MySQL' and 'SqlCE' are supported.");
            }
        }

        public static TransactionObject CreateTransactionObject(DatabaseType database_type, DbConnection connection)
        {
            if (database_type == DatabaseType.MySQL)
            {
                return new TransactionObject(connection);
            }
            else if (database_type == DatabaseType.SqlCE)
            {
                return new SqlCETransactionObject(connection);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type. Currently only 'MySQL' and 'SqlCE' are supported.");
            }
        }
    }

    public class SqlCETransactionObject : TransactionObject
    {
        public SqlCETransactionObject(DbCommand command) : base(command) { }
        public SqlCETransactionObject(DbConnection connection) : base(connection) { }

        public override void Commit(bool forceCommit = false)
        {
            if (_isCommitted)
            {
                throw new InvalidOperationException("Unable to commit transaction that was already committed.");
            }

            if (forceCommit == false && _isRegularCommitAllowed == false)
            {
                return;
            }

            (_transaction as SqlCeTransaction).Commit(CommitMode.Immediate);
            _isCommitted = true;
        }
    }
}
