using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BECamp_T13_HW2_Aspnet_AI.Data;
using BECamp_T13_HW2_Aspnet_AI.Models;
using BECamp_T13_HW2_Aspnet_AI.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration["MySQL:BECampT13HW2"] ?? throw new InvalidOperationException("Connection string 'UserContextConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();

// To create a connection with MySQL by using EF Core context.
builder.Services.AddDbContext<UserContext>(options =>
    options.UseMySQL(connectionString));

// To add Identity services to the container.
builder.Services.AddAuthorization();

// To activate Identity APIs.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<UserContext>();

// To confirm the email address and enables the user to log in.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
});

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<Email>(builder.Configuration.GetSection("Email"));

// Use Interface to achieve dependency injection.
builder.Services.AddScoped<IAIServices, OpenAIServices>();

builder.Services.AddAuthentication()
    .AddGoogle(options => 
    {
        IConfigurationSection googleAuthSection = configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthSection["ClientId"];
        options.ClientSecret = googleAuthSection["ClientSecret"];
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BECamp_T13_HW2_Aspnet-AI",
        Version = "v1",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // UseSwaggerUI is called only in Development.
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Map identity route.
app.MapIdentityApi<IdentityUser>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    
app.MapRazorPages();

app.Run();
