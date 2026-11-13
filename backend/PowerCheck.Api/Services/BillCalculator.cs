// The bill maths. Every endpoint uses this, so the numbers always match.

public static class BillCalculator
{
    public static BillResponse Calculate(decimal monthlyKwh, List<TariffBandDto> bands, decimal fixedChargeRs, string source)
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

        var total = Round(lines.Sum(l => l.AmountRs) + fixedChargeRs);
        return new BillResponse(monthlyKwh, lines, fixedChargeRs, total, source);
    }

    // Money is always rounded to 2 decimal places
    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}