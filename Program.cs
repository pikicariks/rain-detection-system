using Microsoft.EntityFrameworkCore;
using RainDetectionApp.Data;
using RainDetectionApp.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHttpClient<NodeMCUService>();


builder.Services.AddScoped<NodeMCUService>();
builder.Services.AddScoped<RainSystemService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    context.Database.EnsureCreated();
}

app.Run();

//   "Kestrel": {
//   "Endpoints": {
//     "Http": {
//       "Url": "http://0.0.0.0:5179"
//     }
//   }
// }