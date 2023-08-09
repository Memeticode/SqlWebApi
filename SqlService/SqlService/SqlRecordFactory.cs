using SqlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlService;

public class SqlDataRecordManager : ISqlDataRecordManager
{
    public ISqlDataRecord CreateNewRecord(ISqlTableSchema schema)
    {
        var dict = new Dictionary<string, object>();
        foreach (var col in schema.Columns)
        {
            if (col.IsIdentity || col.IsComputed)
                continue;
            dict.Add(col.Name, null);
        }
        return SqlDataRecord.CreateFromDictionary(dict);
    }

    public ISqlDataRecord CreateUpdateRecord(ISqlTableSchema schema, ISqlDataRecord record)
    {
        var dict = new Dictionary<string, object>();
        foreach (var col in schema.Columns)
        {
            if (col.IsIdentity || col.IsComputed)
                continue;
            dict.Add(col.Name, null);
        }
        return SqlDataRecord.CreateFromDictionary(dict);
    }

}
