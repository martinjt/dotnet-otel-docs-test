using System.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder => 
        tracerProviderBuilder
            .AddSource(DiagnosticsConfig.ActivitySource.Name)
            .ConfigureResource(resource => resource
                .AddService(DiagnosticsConfig.ServiceName))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter()
            .AddConsoleExporter())
    .WithMetrics(metricsProviderBuilder => 
        metricsProviderBuilder
            .AddMeter(DiagnosticsConfig.Meter.Name)
            .ConfigureResource(resource => resource
                .AddService(DiagnosticsConfig.ServiceName))
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

public static class DiagnosticsConfig
{
    public const string ServiceName = "MyService";
    public static ActivitySource ActivitySource = new(ServiceName);
    public static Meter Meter = new(ServiceName);
    public static Counter<long> RequestCounter = 
        Meter.CreateCounter<long>("app.request_counter");
    
}