using JoliDay.Config;
using JoliDay.Models;
using JoliDay.Services;
using JoliDay.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//configuration 
var builderConfig = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();


IConfiguration config = builderConfig.Build();
var connectionString = config.GetConnectionString("default");


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JoliDay",
        Version = "v0.1"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Placer un token au format :  Bearer monToken"
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
            new string[] { }
        }
    });
    options.EnableAnnotations();
});
builder.Services.AddDbContext<JoliDayContext>(options => { options.UseSqlServer(connectionString); });

//config Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(option =>
    {
        option.SaveToken = true;
        option.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    })
    .AddGoogle(googleOptions =>
    {
        //googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
        googleOptions.ClientId = builder.Configuration["Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Google:ClientSecret"];
    });
builder.Services.AddAuthorization();

//autoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

//automatise le ModelState 
builder.Services.AddMvc().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var modelState = actionContext.ModelState.Values
            .SelectMany(option => option.Errors)
            .Select(option => option.ErrorMessage);
        return new BadRequestObjectResult(new Error()
            { Code = StatusCodes.Status400BadRequest, Message = modelState.First() });
    };
});

builder.Services.AddCors();

builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddErrorDescriber<ErrorDescriber>()
    .AddEntityFrameworkStores<JoliDayContext>()
    .AddDefaultTokenProviders();


//ajout des service custom
builder.Services.AddTransient<IServiceToken, ServiceToken>();
builder.Services.AddTransient<IServiceEmail, ServiceEmail>();
builder.Services.AddTransient<IServiceValidator, ServiceValidator>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    RoleCreator.Instance(roleManager).CreateRole();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<JoliDayContext>();
    UserCreator.Instance(userManager, dbContext).CreateUsers();
}

app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyOrigin();
    options.SetIsOriginAllowed(origin => true);
    options.AllowAnyMethod();
});

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();