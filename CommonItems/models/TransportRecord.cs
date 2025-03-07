namespace CommonItems.Models;

public class TransportRecord
{
    public string Id { get; set; }
    public TransportType Type { get; set; } = TransportType.Unknown;
    public LocationCode From { get; set; } = LocationCode.Default;
    public LocationCode To { get; set; } = LocationCode.Default;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public TransportRecord() {
        Id = CreateId();
    }
    public TransportRecord(TransportType type, LocationCode from, LocationCode to) {
        Id = CreateId();
        Type = type;
        From = from;
        To = to;
    }

    private string CreateId() => Guid.NewGuid().ToString();
}