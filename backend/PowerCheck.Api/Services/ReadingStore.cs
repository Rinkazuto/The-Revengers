// TEMPORARY in-memory storage for readings.
// Data is lost when the API restarts. Replace with the Data Lead's Readings table (AppDbContext).

public static class ReadingStore
{
    // One saved month: id, month ("2026-10") and kWh
    public class StoredReading
    {
        public int Id { get; set; }
        public string Month { get; set; } = "";
        public decimal Kwh { get; set; }
    }

    private static readonly List<StoredReading> Readings = new();
    private static int _nextId = 1;
    private static readonly object Lock = new();   // stops two requests changing the list at the same time

    // Save a month. Returns the reading and whether it was NEW (true) or OVERWRITTEN (false).
    public static (StoredReading Reading, bool IsNew) Save(string month, decimal kwh)
    {
        lock (Lock)
        {
            var existing = Readings.FirstOrDefault(r => r.Month == month);
            if (existing is not null)
            {
                existing.Kwh = kwh;   // one reading per month: overwrite
                return (existing, false);
            }

            var reading = new StoredReading { Id = _nextId++, Month = month, Kwh = kwh };
            Readings.Add(reading);
            return (reading, true);
        }
    }

    // All saved months, oldest first
    public static List<StoredReading> GetAll()
    {
        lock (Lock)
        {
            return Readings.OrderBy(r => r.Month).ToList();
        }
    }
}