using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlService;


public class SqlTableColumn : ISqlTableColumn
{
    [Required][MaxLength(128)] public string Name { get; set; }

    [JsonConverter(typeof(SqlDataTypeConverter))]
    public ISqlDataType SqlDataType { get; set; }
    public int? Position { get; set; }
    public int? PkPosition { get; set; }
    public bool IsNullable { get; set; }

    // Identity
    public bool IsIdentity { get; set; }
    public int? IdentitySeed { get; set; }
    public int? IdentityIncrement { get; set; }

    // Computed
    public bool IsComputed { get; set; }
    public bool IsPersisted { get; set; }
    public string? ComputedLogic { get; set; }

    // Default
    public bool HasDefault { get; set; }
    public string? DefaultLogic { get; set; }


    public static ISqlTableColumn CreateFromJson(string json) => JsonConvert.DeserializeObject<SqlTableColumn>(json);
    public string ToJsonString() => JsonConvert.SerializeObject(this);


    public bool Equals(ISqlTableColumn? other)
    {
        if (other == null) return false;
        if (Name != other.Name) return false;
        if (Position != other.Position) return false;
        if (PkPosition != other.PkPosition) return false;
        if (IsNullable != other.IsNullable) return false;
        if (IsIdentity != other.IsIdentity) return false;
        if (IdentitySeed != other.IdentitySeed) return false;
        if (IdentityIncrement != other.IdentityIncrement) return false;
        if (IsComputed != other.IsComputed) return false;
        if (IsPersisted != other.IsPersisted) return false;
        if (ComputedLogic != other.ComputedLogic) return false;
        if (HasDefault != other.HasDefault) return false;
        if (DefaultLogic != other.DefaultLogic) return false;
        if ((!SqlDataType?.Equals(other.SqlDataType) ?? other.SqlDataType == null)) return false;
        return true;
    }
    public override bool Equals(object? other)
    {
        if (!(other is ISqlTableColumn)) return false;
        else return Equals((ISqlTableColumn)other);
    }
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(SqlDataType);
        hash.Add(Position);
        hash.Add(PkPosition);
        hash.Add(IsNullable);
        hash.Add(IsIdentity);
        hash.Add(IdentitySeed);
        hash.Add(IdentityIncrement);
        hash.Add(IsComputed);
        hash.Add(IsPersisted);
        hash.Add(ComputedLogic);
        hash.Add(HasDefault);
        hash.Add(DefaultLogic);
        return hash.ToHashCode();
    }
}
