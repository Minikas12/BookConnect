using System.Text;
using APIs.Config;
using APIs.Repositories;
using APIs.Repositories.Interfaces;
using APIs.Services;
using APIs.Services.Interfaces;
using APIs.Services.Payment;
using APIs.Utils.Hubs;
using BusinessObjects;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(); //enable Cross-Origin Resource Sharing (CORS)
builder.Services.AddSession();
builder.Services.AddOData();
builder.Services.AddControllers()
    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//SignalR
//builder.Services.AddSignalR();
//builder.Services.AddCors(opt =>
//{
//    opt.AddPolicy("CORSPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((host) => true));
//});


//Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IBookGroupService, BookGroupService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ITradeService, TradeService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<ISocialMediaService, SocialMediaService>();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Repositories
builder.Services.AddScoped<ICartRepository, CartRepository>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "BookConnectAPI", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
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
            }, new string[]{}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>();


builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWT:Pepper").Value)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/Chat")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
    //options.Authority = "https://localhost:7138";
    //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

builder.Services.AddMvc();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();
builder.Services.Configure<VnPayConfig>(builder.Configuration.GetSection("VnPay"));
builder.Services.Configure<string>(builder.Configuration.GetSection("OCR_API"));
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.EnableDetailedErrors = true;
    //hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(15);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



//static IEdmModel GetEdmModel()
//{
//    var builder = new ODataConventionModelBuilder();

//    builder.EntitySet<TransactionRecord>("TransactionRecords");

//    return builder.GetEdmModel();
//}

app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader().WithExposedHeaders("X-Pagination");
});

//app.UseCors("CORSPolicy");

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.EnableDependencyInjection();
app.Select().Expand().Filter().OrderBy().Count();
//app.MapODataRoute("odata", "odata", GetEdmModel());



app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/Chat");
app.MapControllers();

app.Run();

