using Microsoft.AspNetCore.Authentication.Cookies;
using Probel.Survey.Application.Anketler;
using Probel.Survey.Application.Kullanicilar;
using Probel.Survey.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);// Oracle bağlantısı + repository kaydı
builder.Services.AddScoped<IAnketService, AnketService>();// Application servisi kaydı
builder.Services.AddScoped<IKullaniciService, KullaniciService>();
builder.Services.AddControllersWithViews();// MVC'yi etkinleştir
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Hesap/Giris";
        opt.AccessDeniedPath = "/Hesap/ErisimYok";
        opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseAuthentication();   // ← YENİ — Authorization'dan ÖNCE olmalı
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Anket}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();