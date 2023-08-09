using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SqlService;

public class SqlDataRecord : ISqlDataRecord
{
    private readonly Dictionary<string, object> _data;

    public SqlDataRecord()
    {
        _data = new Dictionary<string, object>();
    }

    public ICollection<string> Keys => _data.Keys;
    public ICollection<object> Values => _data.Values;
    public int Count => _data.Count;
    public bool IsReadOnly => false;

    public object this[string key]
    {
        get => _data[key];
        set => _data[key] = value;
    }

    public static ISqlDataRecord CreateFromJson(string json)
    {
        var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        var record = new SqlDataRecord();
        foreach (var item in data)
        {
            record[item.Key] = item.Value;
        }
        return record;
    }
    public string ToJsonString() => ToJsonObject().ToString();
    public JsonObject ToJsonObject()
    {
        var jsonObject = new JsonObject();
        foreach (var item in _data)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(item.Value);
            JsonNode jsonNode = JsonNode.Parse(json);
            jsonObject.Add(item.Key, jsonNode);
        }
        return jsonObject;
    }


    public void Clear() => _data.Clear();

    public bool Contains(KeyValuePair<string, object> item) => _data.Contains(item);
    public bool ContainsKey(string key) => _data.ContainsKey(key);
    public bool TryGetValue(string key, out object value) => _data.TryGetValue(key, out value);

    public void Add(string key, object value) => _data.Add(key, value);
    public void Add(KeyValuePair<string, object> item) => _data.Add(item.Key, item.Value);

    public bool Remove(string key) => _data.Remove(key);
    public bool Remove(KeyValuePair<string, object> item) => _data.Remove(item.Key);


    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();


    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < _data.Count)
            throw new ArgumentException("The number of elements in the source dictionary is greater than the available space from arrayIndex to the end of the destination array.");

        foreach (var item in _data)
        {
            array[arrayIndex++] = new KeyValuePair<string, object>(item.Key, item.Value);
        }
    }


}
