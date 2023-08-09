using SqlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServiceTests;


[TestClass]
public class SqlConnectionTests
{
    protected ISqlConnectionFactory _connectionFactory { get; set; }
    protected ISqlTableReference _tableReference { get; set; }
    protected ISqlTableSchema _tableSchema { get; set; }

    protected ISqlConnection GetConnection() => _connectionFactory.CreateConnection(_tableReference);


    [TestInitialize]
    public void Setup()
    {
        _connectionFactory = TestConfig.GetConnectionFactory();
        _tableReference = TestConfig.GetTestTableReference();
        _tableSchema = TestConfig.GetTestTableSchema();
    }

    
    [TestMethod]
    public void CheckTableExistsAsyncTest()
    {
        using (var conn = GetConnection())
        {
            var exists = conn.CheckTableExistsAsync(_tableReference).GetAwaiter().GetResult();
            Assert.IsTrue(exists);
        }
    }


    [TestMethod]
    public void GetSqlTableSchemaAsyncTest()
    {
        using (var conn = GetConnection())
        {
            var schema = conn.GetSqlTableSchemaAsync(_tableReference).GetAwaiter().GetResult();
            Assert.IsNotNull(schema);
            Assert.IsTrue(_tableSchema.Equals(schema));
        }
    }



    [TestMethod]
    public void GetSqlTableRecordsAsync()
    {
        using (var conn = GetConnection())
        {
            var records = conn.GetSqlTableRecordsAsync(_tableReference, _tableSchema).GetAwaiter().GetResult();
            Assert.IsNotNull(records);
            Assert.IsTrue(records.Any());
        }
    }
}
