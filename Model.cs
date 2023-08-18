using NpgsqlTypes;

public enum FeeType {
	Test = 1,
	Test2 = 2
}

public record BillingModel(
	FeeType fee_type, 
	decimal fee_value
);

public record Data(
	int vendor_id, 
	BillingModel value, 
	NpgsqlRange<DateTime> during
);


public record ConfigWithLevel(string key, string value);