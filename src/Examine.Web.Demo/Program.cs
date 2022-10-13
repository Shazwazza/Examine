using Examine.Web.Demo;
using Examine;
using Examine.Web.Demo.Controllers;
using Examine.Web.Demo.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Adds Examine Core services
builder.Services.AddExamine();

// A custom extension method to create custom indexes
builder.Services.CreateIndexes();

// Custom services for the demo
builder.Services.AddTransient<BogusDataService>();
builder.Services.AddTransient<IndexService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
