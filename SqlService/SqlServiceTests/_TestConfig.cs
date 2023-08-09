
using SqlService;

namespace SqlServiceTests;

public static class TestConfig
{

    public static string TestServer = @"DESKTOP-BTN3TLD\WSQLEXPRESS";
    public static string TestDatabase = "TestDb";

    public static ISqlConnectionFactory GetConnectionFactory() => new SqlConnectionFactory();
    public static ISqlTableReference GetTestTableReference() => new SqlTableReference()
    {
        Server = TestServer,
        Database = TestDatabase,
        Schema = "dbo",
        Name = "TestTable01",
    };

    public static ISqlTableSchema GetTestTableSchema() 
    {
        var schemaJson = TestResources.TestTable01_Schema;
        return SqlTableSchema.CreateFromJson(schemaJson);
    }
}
