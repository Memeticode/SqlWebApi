using System.ComponentModel.DataAnnotations;
using System.Data;
using Dapper;

namespace SqlService;

public class SqlConnection : ISqlConnection
{
    [Required][MaxLength(128)] public string Server { get; set; }
    [Required][MaxLength(128)] public string Database { get; set; }
    protected string _ConnectionString => $"Server={Server};Initial Catalog={Database};Integrated Security=True";
    protected System.Data.SqlClient.SqlConnection _Connection { get; set; }


    // Lifecycle - public
    public SqlConnection(ISqlTableReference tableReference) : this(tableReference.Server, tableReference.Database) { }
    public SqlConnection(string server, string database)
    {
        if (server is null) throw new ArgumentNullException(nameof(server));
        if (database is null) throw new ArgumentNullException(nameof(database));
        Server = server;
        Database = database;
        _Connection = new System.Data.SqlClient.SqlConnection(_ConnectionString);
    }
    public void Dispose()
    {
        _Connection?.Close();
        _Connection?.Dispose();
    }

    // Lifecycle - protected
    protected async Task OpenConnectionAsync()
    {
        if (_Connection.State == ConnectionState.Closed)
        {
            await _Connection.OpenAsync();
        }
    }
    protected void ValidateTableReference(ISqlTableReference table)
    {
        if (table.Server != Server) throw new ArgumentException($"Specified table's server name ({table.Server}) does not match connection server name ({Server}).");
        if (table.Database != Database) throw new ArgumentException($"Specified table's database name ({table.Database}) does not match connection database name ({Database}).");
    }

    // Sql service base
    public async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? param = null)
    {
        await OpenConnectionAsync();
        return await _Connection.QueryAsync<TResult>(sql, param, null, null, CommandType.Text);
    }
    public async Task<TResult> QuerySingleAsync<TResult>(string sql, object? param = null)
    {
        await OpenConnectionAsync();
        return await _Connection.QuerySingleAsync<TResult>(sql, param, null, null, CommandType.Text);
    }
    public async Task ExecuteAsync(string sql, object? param = null)
    {
        await OpenConnectionAsync();
        await _Connection.ExecuteAsync(sql, param, null, null, CommandType.Text);
    }

    // Table metadata - public
    public async Task<bool> CheckTableExistsAsync(ISqlTableReference table)
    {
        ValidateTableReference(table);
        string sql = @"
            if exists(
	            select 1
                from sys.schemas s
                join sys.tables o on s.schema_id = o.schema_id
	            where s.[name] = @schemaName
	            and o.[name] = @tableName
            )
            begin 
	            select 1; 
            end
            else begin
	            select 0;
            end;";
        return await QuerySingleAsync<bool>(sql, new { databaseName = table.Database, schemaName = table.Schema, tableName = table.Name });
    }
    public async Task<ISqlTableSchema> GetSqlTableSchemaAsync(ISqlTableReference table)
    {
        ValidateTableReference(table);

        string sql = @"
            select c.name as [Name]
	            , typ.name							as [SqlDbType]
	            , c.max_length						as [MaxLength]
	            , c.precision						as [Precision]
	            , c.scale							as [Scale]
	            , c.collation_name					as [Collation]
	            , c.column_id						as Position
	            , pk.index_column_id				as PkPosition
	            , isnull(c.is_nullable,0)			as IsNullable
	            , isnull(c.is_identity,0)			as IsIdentity
	            , id.seed_value						as IdentitySeed
	            , id.increment_value				as IdentityIncrement	
	            , isnull(cc.is_computed	,0)		    as IsComputed
	            , isnull(cc.is_persisted,0)			as IsPersisted
	            , cc.definition						as ComputedLogic
	            , iif(dc.definition is null, 0, 1)	as HasDefault
	            , dc.definition						as [DefaultLogic]
            from sys.schemas s
            join sys.tables o on s.schema_id = o.schema_id
            join sys.columns c on o.object_id = c.object_id
            join sys.types typ on c.user_type_id = typ.user_type_id
            left join sys.indexes i on o.object_id = i.object_id and i.is_primary_key = 1
            left join sys.index_columns pk on o.object_id = pk.object_id and i.index_id = pk.index_id and c.column_id = pk.column_id
            left join sys.identity_columns id on o.object_id = id.object_id and c.column_id = id.column_id
            left join sys.computed_columns cc on o.object_id = cc.object_id and c.column_id = cc.column_id
            left join sys.default_constraints dc on c.default_object_id = dc.object_id
            where s.name = @schemaName
            and o.name = @tableName;";

        var colRes = await QueryAsync<SqlTableColumnQueryResult>(sql, new { schemaName = table.Schema, tableName = table.Name });
        ISqlTableColumn[] columns = colRes?.Select(o => o.ToSqlTableColumn()).ToArray() ?? new ISqlTableColumn[] { };
        return SqlTableSchema.CreateFrom(columns);

    }

    // Table crud - public
    public async Task<ISqlDataRecord[]> GetSqlTableRecordsAsync(ISqlTableReference table, ISqlTableSchema schema)
    {
        ValidateTableReference(table);
        string query = $"SELECT {string.Join(", ", schema.Columns.Select(c => c.Name))} FROM [{table.Schema}].[{table.Name}]";
        var records = await _Connection.QueryAsync<dynamic>(query);
        return records.Select(record =>
        {
            var dataRecord = new SqlDataRecord();
            var recordDict = record as IDictionary<string, object>; // Convert dynamic to dictionary
            if (recordDict != null)
            {
                foreach (var column in schema.Columns)
                {
                    dataRecord.Add(column.Name, recordDict[column.Name]); // Access the dictionary by key
                }
            }
            return dataRecord as ISqlDataRecord;
        }).ToArray();
    }


    public Task<ISqlDataRecord> CreateSqlTableRecordAsync(ISqlTableReference table, ISqlTableSchema schema, ISqlDataRecord record)
    {
        ValidateTableReference(table);
        throw new NotImplementedException();
    }
    public Task<ISqlDataRecord> UpdateSqlTableRecordAsync(ISqlTableReference table, ISqlTableSchema schema, ISqlDataRecord currentRecord, ISqlDataRecord updateRecord)
    {
        throw new NotImplementedException();
    }
    public Task<ISqlDataRecord> DeleteSqlTableRecordAsync(ISqlTableReference table, ISqlTableSchema schema, ISqlDataRecord record)
    {
        ValidateTableReference(table);
        throw new NotImplementedException();
    }


    #region Query Helper Models
    internal class SqlTableColumnQueryResult
    { 
        public string Name { get; set; }

        public SqlDbType SqlDbType { get; set; }
        public int MaxLength { get; set; } = 0;
        public int Precision { get; set; } = 0;
        public int Scale { get; set; } = 0;
        public string? Collation { get; set; } = null;
        public int Position { get; set; }
        public int? PkPosition { get; set; } = null;
        public bool IsNullable { get; set; } = true;
        public bool IsIdentity { get; set; } = false;
        public int? IdentitySeed { get; set; }
        public int? IdentityIncrement { get; set; }
        public bool IsComputed { get; set; } = false;
        public bool IsPersisted { get; set; } = false;
        public string? ComputedLogic { get; set; }

        public bool HasDefault { get; set; } = false;
        public string? DefaultLogic { get; set; }

        public ISqlTableColumn ToSqlTableColumn()
        {
            return new SqlTableColumn()
            {
                Name = this.Name,
                SqlDataType = new SqlDataType()
                {
                    SqlDbType = this.SqlDbType,
                    MaxLength = this.MaxLength,
                    Precision = this.Precision,
                    Scale = this.Scale,
                    Collation = this.Collation,
                },
                Position = this.Position,
                PkPosition = this.PkPosition,
                IsNullable = this.IsNullable,
                IsIdentity = this.IsIdentity,
                IdentitySeed = this.IdentitySeed,
                IdentityIncrement = this.IdentityIncrement,
                IsComputed = this.IsComputed,
                IsPersisted = this.IsPersisted,
                ComputedLogic = this.ComputedLogic,
                HasDefault = this.HasDefault,
                DefaultLogic = this.DefaultLogic,
            };
        }
    }
    #endregion
}
