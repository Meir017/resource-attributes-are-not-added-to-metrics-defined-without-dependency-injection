using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<CustomMetersWithDI>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", ([FromServices] CustomMetersWithDI customMeters) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    CustomMetersWithoutDI.ReportMetrics();
    customMeters.ReportMetrics();

    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class CustomMetersWithoutDI
{
    public static readonly Meter _meter = new("Meir-service", "1.0.0");

    private static readonly Counter<long> _counter = _meter.CreateCounter<long>("custom_counter", "1.0.0", "A custom counter for demonstration purposes");
    private static readonly Histogram<double> _histogram = _meter.CreateHistogram<double>("custom_histogram", "1.0.0", "A custom histogram for demonstration purposes");
    private static readonly UpDownCounter<long> _upDownCounter = _meter.CreateUpDownCounter<long>("custom_updown_counter", "1.0.0", "A custom up-down counter for demonstration purposes");
    private static readonly ObservableGauge<double> _observableGauge = _meter.CreateObservableGauge<double>("custom_observable_gauge", () => Random.Shared.NextDouble() * 100, "A custom observable gauge for demonstration purposes");
    private static readonly ObservableCounter<long> _observableCounter = _meter.CreateObservableCounter<long>("custom_observable_counter", () => Random.Shared.Next(1, 100), "A custom observable counter for demonstration purposes");
    private static readonly ObservableUpDownCounter<long> _observableUpDownCounter = _meter.CreateObservableUpDownCounter<long>("custom_observable_updown_counter", () => Random.Shared.Next(-50, 50), "A custom observable up-down counter for demonstration purposes");

    public static void ReportMetrics()
    {
        _counter.Add(1);
        _histogram.Record(Random.Shared.NextDouble() * 100);
        _upDownCounter.Add(1);
    }
}

public class CustomMetersWithDI
{
    private readonly Counter<long> _counter;
    private readonly Histogram<double> _histogram;
    private readonly UpDownCounter<long> _upDownCounter;
    private readonly ObservableGauge<double> _observableGauge;
    private readonly ObservableCounter<long> _observableCounter;
    private readonly ObservableUpDownCounter<long> _observableUpDownCounter;


    public CustomMetersWithDI(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(new MeterOptions("Meir-service-with-di"));

        _counter = meter.CreateCounter<long>("custom_counter", "1.0.0", "A custom counter for demonstration purposes");
        _histogram = meter.CreateHistogram<double>("custom_histogram", "1.0.0", "A custom histogram for demonstration purposes");
        _upDownCounter = meter.CreateUpDownCounter<long>("custom_updown_counter", "1.0.0", "A custom up-down counter for demonstration purposes");
        _observableGauge = meter.CreateObservableGauge<double>("custom_observable_gauge", () => Random.Shared.NextDouble() * 100, "A custom observable gauge for demonstration purposes");
        _observableCounter = meter.CreateObservableCounter<long>("custom_observable_counter", () => Random.Shared.Next(1, 100), "A custom observable counter for demonstration purposes");
        _observableUpDownCounter = meter.CreateObservableUpDownCounter<long>("custom_observable_updown_counter", () => Random.Shared.Next(-50, 50), "A custom observable up-down counter for demonstration purposes");

    }

    public void ReportMetrics()
    {
        _counter.Add(1);
        _histogram.Record(Random.Shared.NextDouble() * 100);
        _upDownCounter.Add(1);
    }
}