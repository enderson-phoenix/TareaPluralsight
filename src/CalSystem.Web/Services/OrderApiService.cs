using System.Net.Http.Json;
using CalSystem.Web.Models;

namespace CalSystem.Web.Services;

public class OrderApiService(HttpClient http)
{
    public Task<List<OrderDto>?> GetByStatusAsync(string status) =>
        http.GetFromJsonAsync<List<OrderDto>>($"/api/service-orders?status={status}");

    public Task<HttpResponseMessage> CreateAsync(CreateOrderRequest request) =>
        http.PostAsJsonAsync("/api/service-orders", request);

    public Task<HttpResponseMessage> AssignAsync(Guid id, AssignTechnicianRequest request) =>
        http.PutAsJsonAsync($"/api/service-orders/{id}/assign", request);

    public Task<HttpResponseMessage> CloseAsync(Guid id, CloseOrderRequest request) =>
        http.PutAsJsonAsync($"/api/service-orders/{id}/close", request);

    public Task<List<TechnicianDto>?> GetTechniciansAsync() =>
        http.GetFromJsonAsync<List<TechnicianDto>>("/api/technicians");

    public Task<HttpResponseMessage> CreateTechnicianAsync(CreateTechnicianRequest request) =>
        http.PostAsJsonAsync("/api/technicians", request);
}
