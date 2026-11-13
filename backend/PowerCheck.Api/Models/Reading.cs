namespace PowerCheck.Api.Models;

// One month of usage typed in by the user ("My readings"). One row per month.
// Fields follow the API contract (POST /api/readings). Agree any change with the Backend Lead.
public class Reading
{
    public int Id { get; set; }
    public string Month { get; set; } = "";     // "YYYY-MM", e.g. "2026-10"; text sorts in date order
    public decimal Kwh { get; set; }            // 0 to 5000
    public DateTime UpdatedAtUtc { get; set; }  // last time this month was saved
}
