using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OsiguranjApp;
using OsiguranjApp.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(policy =>
{
    // Demo klijent je staticki HTML/JS otvoren direktno u browseru ili preko lokalnog
    // dev servera (Live Server i sl.) - poreklo (origin) nije unapred poznato, pa je CORS
    // ovde namerno permisivan (isto kao u primeru sa vezbi). Ne koristimo kolacice/kredencijale,
    // vec Bearer token u headeru, pa AllowAnyOrigin ne otvara CSRF rizik.
    policy.AddPolicy("KlijentDemo", options =>
    {
        options.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Osiguravajuća kompanija — Web API",
        Version = "v1",
        Description = "CRUD API nad NHibernate slojem osiguravajuće kuće (III projekat)."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nalepi token dobijen sa POST /api/nalozi/prijava (bez reči 'Bearer')."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TokenService.IzdavalacValue,
            ValidateAudience = true,
            ValidAudience = TokenService.IzdavalacValue,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TokenService.PotpisniKljuc
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Osiguravajuca kompanija Web API v1"));

if (app.Environment.IsDevelopment())
    app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex switch
        {
            NeovlascenPristupException => StatusCodes.Status403Forbidden,
            NHibernate.ObjectNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = ex?.Message ?? "Došlo je do greške.",
        };
        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseCors("KlijentDemo");

// Fotografije uz stete se cuvaju na disku servera i vracaju kao staticki fajlovi (/uploads/...).
// Folder "Uploads" (ne "wwwroot") - dotnet watch po defaultu prati samo *.cs/*.csproj/*.resx i
// wwwroot/**/*.config/*.json (https://learn.microsoft.com/aspnet/core/tutorials/dotnet-watch),
// pa slike ovde nikad ne okidaju lazni "izmena izvornog koda" restart/crash.
string uploadsPath = Path.Combine(app.Environment.ContentRootPath, "Uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Demo klijent (Client/*.html) servira i backend na /client, radi lakseg pokretanja
// (nema potrebe za dupli-klik na fajl ili poseban Live Server). api.js i dalje gadja
// isti origin (localhost:5000), pa ovo radi i bez CORS-a.
string clientPath = Path.Combine(app.Environment.ContentRootPath, "Client");
var clientFileProvider = new PhysicalFileProvider(clientPath);
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = clientFileProvider,
    RequestPath = "/client"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = clientFileProvider,
    RequestPath = "/client"
});

app.UseAuthentication();

// Iz validiranog JWT tokena (ako postoji) puni SesijaKorisnik.TrenutniNalog za trenutni
// zahtev — DTOManager.ProveriOvlascenje/ImaUlogu i dalje čitaju isto mesto kao i pre,
// samo sad ono ispravno odražava korisnika ČIJI je ovo zahtev, ne globalno deljeno stanje.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var nalogIdText = context.User.FindFirst("nalogId")?.Value;
        if (int.TryParse(nalogIdText, out var nalogId))
        {
            SesijaKorisnik.TrenutniNalog = new NalogPregled
            {
                NalogId = nalogId,
                KorisnickoIme = context.User.Identity!.Name,
                Uloga = context.User.FindFirst(ClaimTypes.Role)?.Value,
                ImeOsoblja = context.User.FindFirst("imeOsoblja")?.Value,
                PrezimeOsoblja = context.User.FindFirst("prezimeOsoblja")?.Value,
                TipOsoblja = context.User.FindFirst("tipOsoblja")?.Value,
                StatusNaloga = "ODOBREN"
            };
        }
    }
    await next();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
