using System.Net.Http.Json;
using CalSystem.Web.Models;

namespace CalSystem.Web.Services;

public class OrderApiService(HttpClient http)
{
    public Task<List<OrderDto>?> GetByStatusAsync(string status) =>
        http.GetFromJsonAsync<List<OrderDto>>($"/api/orders?status={status}");

    public Task<HttpResponseMessage> CreateAsync(CreateOrderRequest request) =>
        http.PostAsJsonAsync("/api/orders", request);

    public Task<HttpResponseMessage> AssignAsync(Guid id, AssignTechnicianRequest request) =>
        http.PutAsJsonAsync($"/api/orders/{id}/assign", request);

    public Task<HttpResponseMessage> CloseAsync(Guid id, CloseOrderRequest request) =>
        http.PutAsJsonAsync($"/api/orders/{id}/close", request);
}
