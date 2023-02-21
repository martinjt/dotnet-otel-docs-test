using System.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(new DiagnosticsConfig());
builder.Services
    .ConfigureOpenTelemetryTracerProvider((sp, tracerProviderBuilder) =>
        tracerProviderBuilder.AddSource(
            sp.GetRequiredService<DiagnosticsConfig>().ActivitySource.Name
        ))
    .ConfigureOpenTelemetryMeterProvider((sp, meterProviderBuilder) =>
        meterProviderBuilder.AddMeter(
            sp.GetRequiredService<DiagnosticsConfig>().Meter.Name
        ));
;

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(DiagnosticsConfig.ServiceName))
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter()
            .AddConsoleExporter())
    .WithMetrics(metricsProviderBuilder =>
        metricsProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter()
            .AddConsoleExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class DiagnosticsConfig
{
    private readonly Lazy<Counter<long>> _requestCounter = null!;

    public const string ServiceName = "MyService";
    public ActivitySource ActivitySource = new(ServiceName);
    public Meter Meter = new(ServiceName);
    public Counter<long> RequestCounter => _requestCounter.Value;

    public DiagnosticsConfig()
    { 
        _requestCounter = new Lazy<Counter<long>>(() => 
            Meter.CreateCounter<long>("app.request_counter")); 
    }  
}