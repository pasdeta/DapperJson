using System.Data;
using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using static Dapper.SqlMapper;

public class JsonTypeHandler<T> : TypeHandler<T>
{
	public override T Parse(object value)
	{
		string json = (string)value;

		return JsonSerializer.Deserialize<T>(json)!;
	}

	public override void SetValue(IDbDataParameter parameter, T value)
	{
		var c = (NpgsqlParameter) parameter;
		c.NpgsqlDbType = NpgsqlDbType.Jsonb;
		c.Value = JsonSerializer.Serialize(value);
	}
}

public class NpgsqlRangeQueryParameter : ICustomQueryParameter
{
	private readonly NpgsqlRange<DateTime> _range;

	public NpgsqlRangeQueryParameter(NpgsqlRange<DateTime> range)
	{
		_range = range;
	}

	public void AddParameter(IDbCommand command, string name)
	{
		var parameter = new NpgsqlParameter(name, parameterType: NpgsqlDbType.TimestampRange)
        {
            Value = _range,
        };
		command.Parameters.Add(parameter);
	}
}