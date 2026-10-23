using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CYHDocBrandData;

public static class DbInitializer
{
    public static async Task InitializeAsync(string connectionString, ILogger? logger = null)
    {
        try
        {
            await using var con = new SqlConnection(connectionString);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "SEED_INITIAL_DATA");

            await cmd.ExecuteNonQueryAsync();
            logger?.LogInformation("Database initialized successfully via ManageDoctor_CYH Stored Procedure.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to initialize database via Stored Procedure.");
        }
    }
}
