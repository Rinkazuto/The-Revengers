// The bill maths. Every endpoint uses this, so the numbers always match.

public static class BillCalculator
{
    public static BillResponse Calculate(decimal monthlyKwh, List<TariffBandDto> bands, decimal minimumChargeRs, string source)
    {
        var lines = new List<BillLineDto>();

        // Go through each price band, cheapest first
        foreach (var band in bands.OrderBy(b => b.FromKwh))
        {
            // Where this band stops: its end, or the user's usage if that is lower
            var bandEnd = band.ToKwh ?? decimal.MaxValue;
            var usedUpTo = Math.Min(monthlyKwh, bandEnd);

            // How many kWh fall inside this band
            var kwhInBand = usedUpTo - band.FromKwh;
            if (kwhInBand <= 0) break;   // usage doesn't reach this band, so stop

            var amount = Round(kwhInBand * band.PricePerKwh);
            lines.Add(new BillLineDto(band.FromKwh, band.ToKwh, kwhInBand, band.PricePerKwh, amount));
        }

        // The bill is the band total, but never less than the minimum charge
        var bandTotal = lines.Sum(l => l.AmountRs);
        var total = Round(Math.Max(bandTotal, minimumChargeRs));

        return new BillResponse(monthlyKwh, lines, minimumChargeRs, total, source);
    }

    // Money is always rounded to 2 decimal places
    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}