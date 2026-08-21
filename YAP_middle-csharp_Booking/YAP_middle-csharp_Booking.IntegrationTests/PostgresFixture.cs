using Testcontainers.PostgreSql;

namespace YAP_middle_csharp_Booking.IntegrationTests
{
    public class PostgresFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_booking_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        public async Task InitializeAsync()
        {
            await Container.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await Container.DisposeAsync();
        }
    }

    [CollectionDefinition("PostgresCollection")]
    public class PostgresCollection : ICollectionFixture<PostgresFixture> { }

}
