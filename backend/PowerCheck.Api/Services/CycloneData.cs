// Appliance watts and battery figures for the cyclone battery planner.
// Every value has a source. Fridge and battery from the Data Lead (14 Nov 2026).

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
        new("fridge",        "Fridge", 35.50m,
            "Samsung RB29FERNDSA 290 L, sold by Courts Mammouth Mauritius: energy label 311 kWh a year = 35.50 W on average (courtsmammouth.mu, read 14 Nov 2026)"),
    };

    // Huawei LUNA2000-5-E1 lists 5 kWh usable; we keep 80% to allow for inverter losses and ageing (team decision)
    public const decimal UsableBatteryPercent = 80m;
    public const string UsableBatterySource =
        "conservative: Huawei LUNA2000 lists 5 kWh usable; we allow 20% for inverter losses and battery ageing";

    public const decimal MaxBatteryKwh = 50m;
}