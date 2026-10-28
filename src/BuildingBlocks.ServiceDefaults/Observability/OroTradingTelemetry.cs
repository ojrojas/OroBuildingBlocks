using System.Diagnostics.Metrics;

namespace BuildingBlocks.ServiceDefaults.Observability;

/// <summary>Low-cardinality operational measurements for durable processing components.</summary>
public static class OroTradingTelemetry
{
    public const string MeterName = "OroTrading.Foundation";

    private static readonly Meter Meter = new(MeterName, "1.0.0");
    private static readonly Histogram<double> OldestOutboxAge =
        Meter.CreateHistogram<double>("orotrading.outbox.oldest_age", "s");
    private static readonly Counter<long> InboxDuplicates =
        Meter.CreateCounter<long>("orotrading.inbox.duplicates");
    private static readonly Counter<long> WorkerLeaseLosses =
        Meter.CreateCounter<long>("orotrading.worker.lease_losses");

    public const string MarketDataMeterName = "OroTrading.MarketData";
    public const string MarketDataVenueTag = "venue";
    public const string MarketDataMarketTypeTag = "market_type";
    public const string MarketDataStreamTypeTag = "stream_type";

    /// <summary>
    /// Meter for the market protection engine and its five guards. Registered so that
    /// instruments declared by the MarketProtection module are exported without the
    /// composition root needing to know which module declared them.
    /// </summary>
    public const string ProtectionMeterName = "OroTrading.Protection";

    // Protection tag vocabulary. Reason codes are per-guard and bounded; nothing here may
    // carry a symbol, an order id or an account, because a tag set that varies per symbol
    // multiplies series cardinality until the collector rejects them.
    public const string ProtectionGuardTag = "guard";
    public const string ProtectionVerdictTag = "verdict";
    public const string ProtectionScopeTag = "scope";
    public const string ProtectionReasonTag = "reason_code";
    public const string ProtectionActionTag = "action_kind";
    public const string ProtectionVenueTag = "venue";
    public const string ProtectionHealthStateTag = "state";

    public static void RecordOldestOutboxAge(TimeSpan age)
    {
        OldestOutboxAge.Record(Math.Max(0, age.TotalSeconds));
    }

    public static void RecordInboxDuplicate()
    {
        InboxDuplicates.Add(1);
    }

    public static void RecordWorkerLeaseLoss()
    {
        WorkerLeaseLosses.Add(1);
    }
}
