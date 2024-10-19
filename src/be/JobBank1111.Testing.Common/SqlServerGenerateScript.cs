using Microsoft.Data.SqlClient;

namespace JobBank1111.Testing.Common;

internal class SqlServerGenerateScript
{
    public static string ClearAllRecord()
    {
        return @"
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
EXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; DELETE FROM ?'
EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
";
    }

    public static void OnlySupportLocal(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.Compare(builder.DataSource, "localhost", StringComparison.InvariantCultureIgnoreCase) != 0)
        {
            throw new NotSupportedException($"伺服器只支援 localhost，目前連線字串為 {connectionString}");
        }
    }
}