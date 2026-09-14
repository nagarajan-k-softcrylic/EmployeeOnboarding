using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeOnboarding.Function.Services;

public class EmployeeRecordUpdater : IEmployeeRecordUpdater
{
    private readonly string _connectionString;

    public EmployeeRecordUpdater(IConfiguration configuration)
    {
        _connectionString = configuration["SqlConnectionString"]
            ?? throw new InvalidOperationException("SqlConnectionString is not configured.");
    }

    public async Task UpdateWelcomeLetterUrlAsync(int employeeId, string welcomeLetterUrl, CancellationToken ct = default)
    {
        const string sql = @"UPDATE Employees
                              SET WelcomeLetterUrl = @WelcomeLetterUrl, ModifiedDate = @ModifiedDate
                              WHERE Id = @Id";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@WelcomeLetterUrl", welcomeLetterUrl);
        command.Parameters.AddWithValue("@ModifiedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@Id", employeeId);

        await command.ExecuteNonQueryAsync(ct);
    }
}
