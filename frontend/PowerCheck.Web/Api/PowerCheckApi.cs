using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PowerCheck.Web.Api;

// Calls the Backend Lead's API with HttpClient.
public class PowerCheckApi(HttpClient http) : IPowerCheckApi
{
    public Task<TariffsResponse> GetTariffsAsync(string? tariffCode = null) =>
        SendAsync<TariffsResponse>(() => http.GetAsync(
            tariffCode is null ? "api/tariffs" : $"api/tariffs?tariffCode={Uri.EscapeDataString(tariffCode)}"));

    public Task<BillResponse> CalculateBillAsync(BillRequest request) =>
        SendAsync<BillResponse>(() => http.PostAsJsonAsync("api/bill/calculate", request));

    public Task<SolarResponse> EstimateSolarAsync(SolarRequest request) =>
        SendAsync<SolarResponse>(() => http.PostAsJsonAsync("api/solar/estimate", request));

    public Task<ReadingDto> SaveReadingAsync(ReadingRequest request) =>
        SendAsync<ReadingDto>(() => http.PostAsJsonAsync("api/readings", request));

    public Task<ReadingsResponse> GetReadingsAsync() =>
        SendAsync<ReadingsResponse>(() => http.GetAsync("api/readings"));

    public Task<AppliancesResponse> GetAppliancesAsync() =>
    SendAsync<AppliancesResponse>(() => http.GetAsync("api/cyclone/appliances"));

    public Task<BackupResponse> CalculateBackupAsync(BackupRequest request) =>
        SendAsync<BackupResponse>(() => http.PostAsJsonAsync("api/cyclone/backup", request));

    // One place that sends the request and turns every failure into a plain message.
    private static async Task<T> SendAsync<T>(Func<Task<HttpResponseMessage>> call)
    {
        HttpResponseMessage response;
        try
        {
            response = await call();
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Can't reach the ReVolt server. Check your connection and try again.");
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("The server took too long to answer. Try again.");
        }

        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<T>())!;

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            ProblemResponse? problem = null;
            try { problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(); }
            catch (JsonException) { /* not the usual error format: use the general message below */ }

            var messages = problem?.Errors?.SelectMany(e => e.Value).ToList();
            if (messages is { Count: > 0 }) throw new ApiException(string.Join(" ", messages));
            if (!string.IsNullOrWhiteSpace(problem?.Detail)) throw new ApiException(problem.Detail);
        }

        throw new ApiException("Something went wrong, try again.");
    }
}