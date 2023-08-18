using Npgsql;
using Dapper;
using NpgsqlTypes;
using System.Text.Json;

// docker run --rm -P -p 127.0.0.1:5432:5432 -e POSTGRES_PASSWORD="1234" --name pg postgres:15.2

SqlMapper.AddTypeHandler(new JsonTypeHandler<BillingModel>());

var connectionString = "Host=127.0.0.1;Username=postgres;Password=1234;Database=test;";
await using var dataSource = NpgsqlDataSource.Create(connectionString);
var conn = new NpgsqlConnection(connectionString);
// await conn.ExecuteAsync(@"CREATE DATABASE test");


// =============== Vendor Billing =============== //
await conn.ExecuteAsync(@"
	DROP TABLE IF EXISTS vendor_billing_models;
	CREATE TABLE IF NOT EXISTS vendor_billing_models (
		vendor_id integer,
		value jsonb,
		during tsrange
	);
");

var data = new Data(
	vendor_id: 1,
	value: new BillingModel(
		fee_type: FeeType.Test2,
		fee_value: 100
	),
	during: new NpgsqlRange<DateTime>(
		lowerBound: DateTime.Parse("2023-08-07"),
		lowerBoundIsInclusive: true,
		lowerBoundInfinite: false,
		upperBound: DateTime.Parse("2023-08-09"),
		upperBoundInfinite: true,
		upperBoundIsInclusive: false
	)
);
await conn.ExecuteAsync(
	"INSERT INTO vendor_billing_models (vendor_id, value, during) VALUES (@vendor_id, @value, @during)",
	new
	{
		data.vendor_id,
		data.value,
		during = new NpgsqlRangeQueryParameter(data.during)
	}
);
var a = await conn.QueryAsync<Data>("SELECT * FROM vendor_billing_models");
var b = a;

// ======================================================= //
// =============== Config With Levels ==================== //

await conn.ExecuteAsync(@"
	DROP TABLE IF EXISTS config_with_levels;
	CREATE TABLE IF NOT EXISTS config_with_levels (
		key varchar,
		value text
	);
");

await conn.ExecuteAsync(
	"INSERT INTO config_with_levels VALUES (@key, @value)",
	new
	{
		key = "BillingModel",
		value = new BillingModel(FeeType.Test2, fee_value: 10)
	}
);

await conn.ExecuteAsync(
	"INSERT INTO config_with_levels VALUES (@key, @value)",
	new
	{
		key = "IntKey",
		value = 10
	}
);

static async Task<T> FetchKey<T>(NpgsqlConnection conn, string key)
{
	var type = typeof(T);
	var a = await conn.QueryFirstAsync<ConfigWithLevel>("SELECT * FROM config_with_levels where key = @key", new { key });

	if (type.IsPrimitive || type.Equals(typeof(string)))
	{
		
		return (T) Convert.ChangeType(a.value, type);
	}

	return JsonSerializer.Deserialize<T>(a.value)!;
}

var d = await FetchKey<BillingModel>(conn, "BillingModel");
var t = await FetchKey<int>(conn, "IntKey");

Console.WriteLine("Done");
