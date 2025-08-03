using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace SQLite.Net.Interop
{
    public enum DatabaseType
    {
        File,
        InMemory,
        Temporary
    }

    public class SQLiteConnectionOptions
    {
        public SQLiteConnectionOptions(string databasePath, DatabaseType databaseType = DatabaseType.File, bool storeDateTimeAsTicks = true)
        {
            DatabasePath = databasePath;
            DatabaseType = databaseType;
            StoreDateTimeAsTicks = storeDateTimeAsTicks;
        }

        public string DatabasePath { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public bool StoreDateTimeAsTicks { get; set; }
    }

    public interface ISQLiteConnectionFactory
    {
        ISQLiteConnection Create(string databasePath);
        ISQLiteConnection CreateInMemory();
        ISQLiteConnection CreateTemporary();
    }

    public interface ISQLiteConnectionFactoryEx
    {
        ISQLiteConnection Create(SQLiteConnectionOptions options);
    }

    [Flags]
    public enum CreateFlags
    {
        None = 0x000,
        ImplicitPK = 0x001,
        AutoIncPK = 0x002,
        ImplicitIndex = 0x004,
        AllImplicit = ImplicitPK | AutoIncPK | ImplicitIndex,
        FullTextSearch3 = 0x100,
        FullTextSearch4 = 0x200,
        Nullable = 0x800
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrementAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class IndexedAttribute : Attribute
    {
        public IndexedAttribute(string name = null, int order = -1, bool unique = false)
        {
            Name = name;
            Order = order;
            Unique = unique;
        }

        public string Name { get; }
        public int Order { get; }
        public bool Unique { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class CollationAttribute : Attribute
    {
        public CollationAttribute(string collation)
        {
            Value = collation;
        }

        public string Value { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class MaxLengthAttribute : Attribute
    {
        public MaxLengthAttribute(int length)
        {
            Value = length;
        }

        public int Value { get; }
    }

    public interface ISQLiteConnection : IDisposable
    {
        void Trace(bool enabled);

        int Execute(string sql, params object[] args);
        List<T> Query<T>(string sql, params object[] args) where T : new();
        T Find<T>(object primaryKey) where T : new();
        T Get<T>(object primaryKey) where T : new();
        T Get<T>(Expression<Func<T, bool>> predicate) where T : new();

        int Insert(object item);
        int InsertAll(IEnumerable<object> items);
        int Update(object item);
        int Delete(object item);

        void CreateTable<T>(CreateFlags createFlags = CreateFlags.None);
        void DropTable<T>();
        TableMapping GetMapping(Type type);
        void BeginTransaction();
        void Commit();
        void Rollback();

        TableQuery<T> Table<T>() where T : new();
    }

    public abstract class MvxBaseSQLiteConnectionFactory : ISQLiteConnectionFactory, ISQLiteConnectionFactoryEx
    {
        public ISQLiteConnection Create(string databasePath)
        {
            return Create(new SQLiteConnectionOptions(databasePath));
        }

        public ISQLiteConnection CreateInMemory()
        {
            return Create(new SQLiteConnectionOptions(null, DatabaseType.InMemory));
        }

        public ISQLiteConnection CreateTemporary()
        {
            return Create(new SQLiteConnectionOptions(null, DatabaseType.Temporary));
        }

        public ISQLiteConnection Create(SQLiteConnectionOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            switch (options.DatabaseType)
            {
                case DatabaseType.File:
                    return CreateSQLiteConnection(options.DatabasePath, options.StoreDateTimeAsTicks);
                case DatabaseType.InMemory:
                    return CreateSQLiteConnection(":memory:", options.StoreDateTimeAsTicks);
                case DatabaseType.Temporary:
                    return CreateSQLiteConnection("", options.StoreDateTimeAsTicks);
                default:
                    throw new NotSupportedException($"Unsupported database type: {options.DatabaseType}");
            }
        }

        protected abstract ISQLiteConnection CreateSQLiteConnection(string databasePath, bool storeDateTimeAsTicks);
    }
}
