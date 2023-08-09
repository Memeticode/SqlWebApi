using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SqlService;

public class SqlTableReference : ISqlTableReference
{
    [Required][MaxLength(128)] public string Server { get; set; }
    [Required][MaxLength(128)] public string Database { get; set; }
    [Required][MaxLength(128)] public string Schema { get; set; }
    [Required][MaxLength(128)] public string Name { get; set; }

    public static ISqlTableReference CreateFromJson(string json) => JsonConvert.DeserializeObject<SqlTableReference>(json);
    public string ToJsonString() => JsonConvert.SerializeObject(this);

    public bool Equals(ISqlTableReference? other)
    {
        if (other == null) return false;
        if (Server != other.Server) return false;
        if (Database != other.Database) return false;
        if (Schema != other.Schema) return false;
        if (Name != other.Name) return false;
        return true;

    }
    public override bool Equals(object? other)
    {
        if (!(other is ISqlTableReference)) return false;
        else return Equals((ISqlTableReference)other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Server, Database, Schema, Name);
    }
}