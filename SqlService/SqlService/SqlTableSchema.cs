using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SqlService;

public class SqlTableSchema : ISqlTableSchema
{
    public ISqlTableColumn[] Columns {
        get => _Columns;
        set
        {
            _Columns = value ?? new ISqlTableColumn[0];
            _UpdateInternal();
            _UpdateColumnPositions();
        }
    }
    public string[] PkColumnNames { get => _PkColumnNames; set => _PkColumnNames = value; }

    public static ISqlTableSchema CreateFromJson(string json)
    {
        var jArray = JArray.Parse(json);
        var columns = new List<ISqlTableColumn>();

        foreach (var jColumn in jArray)
        {
            var column = new SqlTableColumn
            {
                Name = jColumn["Name"]?.ToString(),
                SqlDataType = new SqlDataType
                {
                    SqlDbType = Enum.Parse<SqlDbType>(jColumn["SqlDataType"]["SqlDbType"]?.ToString() ?? string.Empty),
                    MaxLength = (int)jColumn["SqlDataType"]["MaxLength"],
                    Precision = (int)jColumn["SqlDataType"]["Precision"],
                    Scale = (int)jColumn["SqlDataType"]["Scale"],
                    Collation = (string?)jColumn["SqlDataType"]["Collation"]
                },
                Position = (int?)jColumn["Position"],
                PkPosition = (int?)jColumn["PkPosition"],
                IsNullable = (bool)jColumn["IsNullable"],
                IsIdentity = (bool)jColumn["IsIdentity"],
                IdentitySeed = (int?)jColumn["IdentitySeed"],
                IdentityIncrement = (int?)jColumn["IdentityIncrement"],
                IsComputed = (bool)jColumn["IsComputed"],
                IsPersisted = (bool)jColumn["IsPersisted"],
                ComputedLogic = (string?)jColumn["ComputedLogic"],
                HasDefault = (bool)jColumn["HasDefault"],
                DefaultLogic = (string?)jColumn["DefaultLogic"],
            };
            columns.Add(column);
        }

        return SqlTableSchema.CreateFrom(columns.ToArray());
    }


    public string ToJsonString()
    {
        return System.Text.Json.JsonSerializer.Serialize(Columns);
    }


    // Constructor

    public static ISqlTableSchema CreateFrom(ISqlTableColumn[] columns)
    {
        var schema = new SqlTableSchema();
        schema.Columns = columns;
        return schema;
    }



    // Backing fields and methods

    protected ISqlTableColumn[] _Columns { get; set; }
    protected HashSet<string> _ColumnNames { get; set; }
    protected string[] _PkColumnNames { get; set; }
    protected void _UpdateInternal()
    {
        // should add method to validate column position and unique names
        _ColumnNames = _Columns.Select(c => c.Name).ToHashSet();
        _PkColumnNames = _Columns.Where(o => o.PkPosition is not null).OrderBy(o => o.PkPosition).Select(o => o.Name).ToArray();
    }
    protected void _UpdateColumnPositions()
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            Columns[i].Position = i;
        }
    }
    protected void _AddOrUpdateColumnByName(string name, ISqlTableColumn column)
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            if (Columns[i].Name == name)
            {
                Columns[i] = column;
                _UpdateInternal();
                return;
            }
        }
        Columns.Append(column);
        _UpdateInternal();
    }
    protected void _AddOrUpdateColumnByPosition(int position, ISqlTableColumn column)
    {
        Columns[position] = column;
        _UpdateInternal();
        _UpdateColumnPositions();
    }

    
    //// Interfaces ////

    // IEquatable<ISqlTableSchema>

    public bool Equals(ISqlTableSchema? other)
    {
        if (other is null) return false;
        if (Columns.Length != other.Columns.Length) return false;

        // Check all columns are present and equivalent in other schema
        foreach (var col in Columns)
        {
            if (other.TryGetValue(col.Name, out ISqlTableColumn otherCol))
            {
                if (!col.Equals(otherCol))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }
    public override bool Equals(object? other)
    {
        if (!(other is ISqlTableSchema)) return false;
        else return Equals((ISqlTableSchema)other);
    }
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var col in Columns) { hash.Add(col.GetHashCode()); }
        return hash.ToHashCode();
    }


    //// IDictionary<string, ISqlTableColumn>, IOrderedDictionary

    public int Count => Columns.Length;
    public bool IsReadOnly => false;
    public bool IsFixedSize => false;
    public bool IsSynchronized => false;
    public object SyncRoot => false;
    public ISqlTableColumn this[string key] 
    { 
        get => Columns.Where(o => o.Name == key).First();
        set => _AddOrUpdateColumnByName(key, (ISqlTableColumn)value);
    }
    public object? this[int index] { 
        get => Columns[index];
        set => _AddOrUpdateColumnByPosition(index, (ISqlTableColumn)value);
    }
    public object? this[object key] { 
        get => this[key.ToString()];
        set => _AddOrUpdateColumnByName(key.ToString(), (ISqlTableColumn)value);
    }

    public ICollection<string> Keys => Columns.OrderBy(o => o.Position).Select(o => o.Name).ToArray();
    public ICollection<ISqlTableColumn> Values => Columns;


    ICollection IDictionary.Keys => Columns.OrderBy(o => o.Position).Select(o => o.Name).ToArray();
    ICollection IDictionary.Values => Columns;

    public bool Contains(KeyValuePair<string, ISqlTableColumn> item)
    {
        if (item.Key != item.Value.Name)
            throw new ArgumentException($"Specified column name key ({item.Key}) does not match specified column name ({item.Value.Name}).");
        if (!_ColumnNames.Contains(item.Key)) return false;
        if (!_Columns.Contains(item.Value)) return false;
        return true;
    }
    public bool Contains(object key) => _ColumnNames.Contains(key.ToString());
    public bool ContainsKey(string key) => _ColumnNames.Contains(key.ToString());
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out ISqlTableColumn value)
    {
        value = null;
        if (ContainsKey(key))
        {
            value = this[key];
            return true;
        }
        return false;
    }


    public void Add(string key, ISqlTableColumn value) => _AddOrUpdateColumnByName(key, value);
    public void Add(KeyValuePair<string, ISqlTableColumn> item) => _AddOrUpdateColumnByName(item.Key, item.Value);
    public void Add(object key, object? value) => _AddOrUpdateColumnByName(key.ToString(), (ISqlTableColumn)value);

    public void Insert(int index, object key, object? value)
    {
        if (!(value is ISqlTableColumn)) 
            throw new ArgumentException("Specified valus does not implement ISqlTableColumn.");

        var k = key.ToString();
        var v = (ISqlTableColumn)value;
        if (k == v.Name)
            throw new ArgumentException($"Specified column name key ({k}) does not match specified column name ({v.Name}).");

        if (_ColumnNames.Contains(k))
            throw new ArgumentException($"Column with name {k} already exists in schema.");

        var cols = _Columns.ToList();
        cols.Insert(index, v);
        Columns = cols.ToArray();
    }
    public void Remove(object key) => Remove(key.ToString());
    public bool Remove(string key)
    {
        if (!_ColumnNames.Contains(key)) 
            return false;
        for (int i = 0; i < Columns.Length; i++)
        {
            if (Columns[i].Name == key)
            {
                RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    public bool Remove(KeyValuePair<string, ISqlTableColumn> item)
    {

        if (item.Key != item.Value.Name)
            throw new ArgumentException($"Specified column name key ({item.Key}) does not match specified column name ({item.Value.Name}).");
        if (!_ColumnNames.Contains(item.Key)) return false;
        if (!_Columns.Contains(item.Value)) return false;
        Remove(item.Key);
        return true;
    }
    public void RemoveAt(int index)
    {
        if (index >= Columns.Length) throw new IndexOutOfRangeException();
        var cols = Columns.ToList();
        cols.RemoveAt(index);
        Columns = cols.ToArray();
    }

    public void Clear() => Columns = new ISqlTableColumn[0];


    public IEnumerator<KeyValuePair<string, ISqlTableColumn>> GetEnumerator() => _Columns.Select(c => new KeyValuePair<string, ISqlTableColumn>(c.Name, c)).GetEnumerator();
    IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
    {
        OrderedDictionary orderedDictionary = new OrderedDictionary();
        foreach (var column in _Columns)
        {
            orderedDictionary.Add(column.Name, column);
        }
        return orderedDictionary.GetEnumerator();
    }
    IDictionaryEnumerator IDictionary.GetEnumerator() => new Hashtable(_Columns.ToDictionary(c => c.Name, c => (object)c)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _Columns.GetEnumerator();

    public void CopyTo(KeyValuePair<string, ISqlTableColumn>[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < _Columns.Length)
            throw new ArgumentException("The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

        foreach (var column in _Columns)
        {
            array[arrayIndex++] = new KeyValuePair<string, ISqlTableColumn>(column.Name, column);
        }
    }
    public void CopyTo(Array array, int index)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (index < 0 || index > array.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (array.Length - index < _Columns.Length)
            throw new ArgumentException("The number of elements in the source collection is greater than the available space from index to the end of the destination array.");

        foreach (var column in _Columns)
        {
            array.SetValue(new KeyValuePair<string, ISqlTableColumn>(column.Name, column), index++);
        }
    }


}