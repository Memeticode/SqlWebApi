using System.Data;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using System.Xml.XPath;
using System.Data.Common;
using System.Text.Json.Nodes;
using System.ComponentModel.DataAnnotations;

namespace SqlService;

public interface IJsonSerializable
{
    string ToJsonString();
}

public interface ISqlTableReference: IEquatable<ISqlTableReference>, IJsonSerializable
{
    [Required][MaxLength(128)] string Server { get; set; }
    [Required][MaxLength(128)] string Database { get; set; }
    [Required][MaxLength(128)] string Schema { get; set; }
    [Required][MaxLength(128)] string Name { get; set; }
}

public interface ISqlDataRecord: IDictionary<string, object>, ICloneable, IJsonSerializable
{
    JsonObject ToJsonObject();
}

public interface ISqlTableSchema : IEquatable<ISqlTableSchema>, IDictionary<string, ISqlTableColumn>, IOrderedDictionary, IJsonSerializable
{
    ISqlTableColumn[] Columns { get; set; }
    string[] PkColumnNames { get; set; }
}

public interface ISqlTableColumn : IEquatable<ISqlTableColumn>, IJsonSerializable
{
    [Required][MaxLength(128)] string Name { get; set; }

    [JsonConverter(typeof(SqlDataTypeConverter))]
    [Required] ISqlDataType SqlDataType { get; set; }
    int? Position { get; set; }
    int? PkPosition { get; set; }
    [Required] bool IsNullable { get; set; }

    // Identity
    bool IsIdentity { get; set; }
    int? IdentitySeed { get; set; }
    int? IdentityIncrement { get; set; }

    // Computed
    bool IsComputed { get; set; }
    bool IsPersisted { get; set; }
    string? ComputedLogic { get; set; }

    // Default
    bool HasDefault { get; set; }
    string? DefaultLogic { get; set; }
}

public interface ISqlDataType : IEquatable<ISqlDataType>, IJsonSerializable
{
    [Required] SqlDbType SqlDbType { get; set; }
    int MaxLength { get; set; }
    int Precision { get; set; }
    int Scale { get; set; }
    string? Collation { get; set; }
}




//// "Under the hood" interfaces
public interface ISqlDataRecordManager
{
    ISqlDataRecord CreateNewRecord(ISqlTableSchema schema);
}


public interface ISqlConnectionFactory
{
    ISqlConnection CreateConnection(ISqlTableReference tableReference);
}

public interface ISqlConnection: IDisposable
{
    [Required][MaxLength(128)] string Server { get; set; }
    [Required][MaxLength(128)] string Database { get; set; }

    Task<TResult> QuerySingleAsync<TResult>(string sql, object? param = null);
    Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? param = null);
    Task ExecuteAsync(string sql, object? param = null);

    Task<bool> CheckTableExistsAsync(ISqlTableReference table);
    Task<ISqlTableSchema> GetSqlTableSchemaAsync(ISqlTableReference table);
    //Task<ISqlTableColumn[]> GetSqlTableColumns(ISqlTableReference table);
    //Task<bool> CheckTableHasPk(ISqlTableReference table);
    //Task<string[]> GetSqlTablePkColumnNames(ISqlTableReference table);

    Task<ISqlDataRecord[]> GetSqlTableRecordsAsync(ISqlTableReference table, ISqlTableSchema schema);
    //Task<ISqlDataRecord[]> GetSqlTableRecordsAsync(ISqlTableReference table, ISqlTableSchema schema, string[] columnNames, ISqlTableFilter[] filters);
    Task<ISqlDataRecord> CreateSqlTableRecordAsync(ISqlTableReference table, ISqlTableSchema schema, ISqlDataRecord record);
    Task<ISqlDataRecord> UpdateSqlTableRecordAsync(ISqlTableReference table, ISqlTableSchema schema, ISqlDataRecord currentRecord, ISqlDataRecord updateRecord);
    Task<ISqlDataRecord> DeleteSqlTableRecordAsync(ISqlTableReference table, ISqlTableSchema schema, ISqlDataRecord record);
}



