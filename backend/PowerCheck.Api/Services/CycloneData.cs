// Appliance watts and battery figures for the cyclone battery planner.
// Sources researched by the Integrator. Two values are PLACEHOLDERS until the Data Lead sends a source.

public static class CycloneData
{
    // Typical power use of common appliances during a power cut
    public static readonly List<ApplianceDto> Appliances = new()
    {
        new("led_light",     "LED light (one bulb)", 9m,
            "FTC and ENERGY STAR: an 800-lumen LED uses about 9 W"),
        new("wifi_router",   "Wi-Fi router", 10m,
            "Router makers' published measurements (about 6–10 W), rounded up"),
        new("ceiling_fan",   "Ceiling fan", 120m,
            "Virginia Tech, Estimating Appliance and Home Electronic Energy Use, BSE-301NP (2020), DOE table"),
        new("tv",            "TV (flat screen)", 120m,
            "Virginia Tech BSE-301NP (2020), DOE table"),
        new("laptop",        "Laptop", 50m,
            "Virginia Tech BSE-301NP (2020), DOE table"),
        new("phone_charger", "Phone charger", 7.5m,
            "USB Battery Charging spec 1.2 maximum (5 V × 1.5 A), safe side"),
        // PLACEHOLDER: replace with the annual kWh from an EEMO energy label ÷ 365 ÷ 24 (Data Lead)
        new("fridge",        "Fridge", 50m,
            "PENDING SOURCE: average of about 1.2 kWh a day (a fridge cycles on and off); to be replaced with an EEMO energy label value"),
    };

    // PLACEHOLDER: replace with the depth of discharge from a real battery datasheet (Data Lead)
    public const decimal UsableBatteryPercent = 80m;
    public const string UsableBatterySource =
        "PENDING SOURCE: conservative assumption (home lithium batteries are usually 80–95% usable)";

    public const decimal MaxBatteryKwh = 50m;
}