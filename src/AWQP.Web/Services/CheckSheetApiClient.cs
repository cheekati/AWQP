using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AWQP.Application.DTOs;

namespace AWQP.Web.Services;

public sealed class CheckSheetApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiTokenProvider _tokenProvider;
    private readonly IConfiguration _configuration;

    public CheckSheetApiClient(IHttpClientFactory httpClientFactory, ApiTokenProvider tokenProvider, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
        _configuration = configuration;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        var apiBase = (_configuration["ApiBaseUrl"] ?? "http://localhost:5080").TrimEnd('/');
        client.BaseAddress = new Uri(apiBase + "/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.CreateToken());
        return client;
    }

    public async Task<List<CheckPointMasterDto>> ListCheckPointsAsync(CancellationToken ct = default)
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<CheckPointMasterDto>>("api/ehs/check-points", JsonOptions, ct) ?? new();
    }

    public async Task<(bool Ok, CheckPointMasterDto? Value, string? Error)> CreateCheckPointAsync(CreateCheckPointMasterRequest request, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/ehs/check-points", request, JsonOptions, ct);
        return await ReadResultAsync<CheckPointMasterDto>(response, ct);
    }

    public async Task<(bool Ok, CheckPointMasterDto? Value, string? Error)> UpdateCheckPointAsync(Guid id, UpdateCheckPointMasterRequest request, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"api/ehs/check-points/{id}", request, JsonOptions, ct);
        return await ReadResultAsync<CheckPointMasterDto>(response, ct);
    }

    public async Task<(bool Ok, CheckPointMasterDto? Value, string? Error)> UpdateSpecsAsync(Guid id, UpdateCheckPointSpecsRequest request, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"api/ehs/check-points/{id}/specs", request, JsonOptions, ct);
        return await ReadResultAsync<CheckPointMasterDto>(response, ct);
    }

    public async Task<(bool Ok, string? Error)> DeleteCheckPointAsync(Guid id, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"api/ehs/check-points/{id}", ct);
        if (response.IsSuccessStatusCode) return (true, null);
        var err = await ReadErrorAsync(response, ct);
        return (false, err);
    }

    public async Task<List<CheckSheetLookupDto>> ListDepartmentsAsync(CancellationToken ct = default)
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<CheckSheetLookupDto>>("api/ehs/checksheets/departments", JsonOptions, ct) ?? new();
    }

    public async Task<List<CheckSheetLookupDto>> ListSectionsAsync(string departmentCode, CancellationToken ct = default)
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<CheckSheetLookupDto>>(
            $"api/ehs/checksheets/sections?departmentCode={Uri.EscapeDataString(departmentCode)}", JsonOptions, ct) ?? new();
    }

    public async Task<List<string>> ListMachinesAsync(string departmentCode, string sectionCode, CancellationToken ct = default)
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<string>>(
            $"api/ehs/checksheets/machines?departmentCode={Uri.EscapeDataString(departmentCode)}&sectionCode={Uri.EscapeDataString(sectionCode)}",
            JsonOptions, ct) ?? new();
    }

    public async Task<List<string>> ListFrequenciesAsync(string departmentCode, string sectionCode, string machineName, CancellationToken ct = default)
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<string>>(
            $"api/ehs/checksheets/frequencies?departmentCode={Uri.EscapeDataString(departmentCode)}&sectionCode={Uri.EscapeDataString(sectionCode)}&machineName={Uri.EscapeDataString(machineName)}",
            JsonOptions, ct) ?? new();
    }

    public async Task<List<CheckSheetGridRowDto>> GetGridAsync(
        string departmentCode, string sectionCode, string machineName, string frequency, DateTime checkingDate, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var q = $"api/ehs/checksheets/grid?departmentCode={Uri.EscapeDataString(departmentCode)}&sectionCode={Uri.EscapeDataString(sectionCode)}&machineName={Uri.EscapeDataString(machineName)}&frequency={Uri.EscapeDataString(frequency)}&checkingDate={checkingDate:yyyy-MM-dd}";
        return await client.GetFromJsonAsync<List<CheckSheetGridRowDto>>(q, JsonOptions, ct) ?? new();
    }

    public async Task<List<DateTime>> GetCheckedDatesAsync(
        string departmentCode, string sectionCode, string machineName, string frequency, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var q = $"api/ehs/checksheets/dates?departmentCode={Uri.EscapeDataString(departmentCode)}&sectionCode={Uri.EscapeDataString(sectionCode)}&machineName={Uri.EscapeDataString(machineName)}&frequency={Uri.EscapeDataString(frequency)}";
        return await client.GetFromJsonAsync<List<DateTime>>(q, JsonOptions, ct) ?? new();
    }

    public async Task<(bool Ok, CheckSheetGridRowDto? Value, string? Error)> SaveRowAsync(SaveCheckSheetRowRequest request, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/ehs/checksheets/rows", request, JsonOptions, ct);
        return await ReadResultAsync<CheckSheetGridRowDto>(response, ct);
    }

    public async Task<(bool Ok, string? Path, string? Error)> UploadImageAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        // Upload goes to the Web host (not API) so files land under wwwroot/uploads.
        using var client = _httpClientFactory.CreateClient();
        using var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", fileName);

        // Relative to current web app — caller should pass absolute when needed.
        var response = await client.PostAsync("/CheckSheet/api/upload", form, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var path = doc.RootElement.TryGetProperty("path", out var p) ? p.GetString() : null;
        return (true, path, null);
    }

    private static async Task<(bool Ok, T? Value, string? Error)> ReadResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        if (response.IsSuccessStatusCode)
        {
            var value = JsonSerializer.Deserialize<T>(body, JsonOptions);
            return (true, value, null);
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var err))
                return (false, default, err.GetString());
        }
        catch
        {
            // ignore parse errors
        }

        return (false, default, string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var err))
                return err.GetString() ?? body;
        }
        catch
        {
            // ignore
        }
        return string.IsNullOrWhiteSpace(body) ? (response.ReasonPhrase ?? "Request failed") : body;
    }
}
