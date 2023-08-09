using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlService;

public class SqlConnectionFactory: ISqlConnectionFactory
{
    public ISqlConnection CreateConnection(ISqlTableReference tableReference)
    {
        return new SqlConnection(tableReference);
    }
}

