namespace Co_working_Space.Services;

// ponytail: bộ nhớ trong, nâng cấp DB sequence khi scale
public static class IdGenerator
{
    private static readonly object _lock = new();
    private static readonly Dictionary<string, int> _counters = new();

    public static string Next(string prefix)
    {
        lock (_lock)
        {
            _counters.TryGetValue(prefix, out int current);
            _counters[prefix] = ++current;
            if (prefix.StartsWith("BKG-"))
            {
                var today = DateTime.UtcNow.ToString("yyyyMMdd");
                return $"{prefix}{today}-{current:D3}";
            }
            return $"{prefix}{current:D4}";
        }
    }

    public const string User = "USR-";
    public const string Staff = "STF-";
    public const string Admin = "ADM-";
    public const string RoomSmall = "RM-S-";
    public const string RoomMedium = "RM-M-";
    public const string RoomLarge = "RM-L-";
    public const string RoomVip = "RM-V-";
    public const string Booking = "BKG-";
    public const string Approval = "APR-";
    public const string EquipProjector = "EQ-PROJ-";
    public const string EquipTV = "EQ-TV-";
    public const string EquipMicrophone = "EQ-MIC-";
    public const string EquipWhiteboard = "EQ-WB-";
    public const string EquipSpeaker = "EQ-SPK-";
    public const string EquipCamera = "EQ-CAM-";
    public const string EquipCapture = "EQ-CAP-";
}
