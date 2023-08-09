using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SqlService;

public class SqlDataType : ISqlDataType
{
    [Required] public SqlDbType SqlDbType { get; set; }
    public int MaxLength { get; set; }
    public int Precision { get; set; }
    public int Scale { get; set; }
    public string? Collation { get; set; }

    public static ISqlDataType CreateFromJson(string json) => JsonConvert.DeserializeObject<SqlDataType>(json);
    public string ToJsonString() => JsonConvert.SerializeObject(this);
    
    public bool Equals(ISqlDataType? other)
    {
        if (other is null) return false;
        if (SqlDbType != other.SqlDbType) return false;
        if (MaxLength != other.MaxLength) return false;
        if (Precision != other.Precision) return false;
        if (Scale != other.Scale) return false;
        if (Collation != other.Collation) return false;
        return true;
    }

    public override bool Equals(object? other)
    {
        if (!(other is ISqlDataType)) return false;
        else return Equals((ISqlDataType)other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(SqlDbType, MaxLength, Precision, Scale, Collation);
    }

}


public class SqlDataTypeConverter : JsonConverter<ISqlDataType>
{
    public override ISqlDataType ReadJson(JsonReader reader, Type objectType, ISqlDataType existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        return serializer.Deserialize<SqlDataType>(reader);
    }

    public override void WriteJson(JsonWriter writer, ISqlDataType value, Newtonsoft.Json.JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}

