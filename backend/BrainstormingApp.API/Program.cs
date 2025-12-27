using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BrainstormingApp.API.Hubs;
using BrainstormingApp.API.Middleware;
using BrainstormingApp.Application.Interfaces;
using BrainstormingApp.Application.Services;
using BrainstormingApp.Core.Interfaces;
using BrainstormingApp.Infrastructure.Data;
using BrainstormingApp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Brainstorming API",
        Version = "v1",
        Description = "API for managing brainstorming sessions with 6-3-5 methodology"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token"
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
            Array.Empty<string>()
        }
    });
});

// Configure Database - Using InMemory for development
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("BrainstormingDB"));
    // options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSecret = builder.Configuration["JWT:Secret"]
    ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "BrainstormingApp";
var jwtAudience = builder.Configuration["JWT:Audience"] ?? "BrainstormingApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };

    // For SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174",
                "http://127.0.0.1:5175"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

// Register SignalR
builder.Services.AddSignalR();

// Register Dependencies
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IIdeaService, IdeaService>();
builder.Services.AddScoped<IChatGPTService, ChatGPTService>();
builder.Services.AddScoped<IReportingService, ReportingService>();

// Register HttpClient for ChatGPT service
builder.Services.AddHttpClient<IChatGPTService, ChatGPTService>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Seed test data for development
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedTestData(context);
}

// Global error handling middleware
app.UseErrorHandling();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Brainstorming API V1");
    });
}

// app.UseHttpsRedirection(); // Disabled for development with HTTP frontend

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<BrainstormingHub>("/hubs/brainstorming");

app.Run();

// Seed data method for development/testing
static async Task SeedTestData(ApplicationDbContext context)
{
    // Check if already seeded
    if (context.Users.Any()) return;

    // Create test users with BCrypt hashed password "123456"
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword("123456");

    var eventManager = new BrainstormingApp.Core.Entities.User
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Email = "manager@test.com",
        PasswordHash = hashedPassword,
        FirstName = "Event",
        LastName = "Manager",
        Role = BrainstormingApp.Core.Enums.UserRole.EventManager,
        CreatedAt = DateTime.UtcNow
    };

    var teamLeader = new BrainstormingApp.Core.Entities.User
    {
        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Email = "leader@test.com",
        PasswordHash = hashedPassword,
        FirstName = "Team",
        LastName = "Leader",
        Role = BrainstormingApp.Core.Enums.UserRole.TeamLeader,
        CreatedAt = DateTime.UtcNow
    };

    var member1 = new BrainstormingApp.Core.Entities.User
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Email = "member1@test.com",
        PasswordHash = hashedPassword,
        FirstName = "Member",
        LastName = "One",
        Role = BrainstormingApp.Core.Enums.UserRole.TeamMember,
        CreatedAt = DateTime.UtcNow
    };

    var member2 = new BrainstormingApp.Core.Entities.User
    {
        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        Email = "member2@test.com",
        PasswordHash = hashedPassword,
        FirstName = "Member",
        LastName = "Two",
        Role = BrainstormingApp.Core.Enums.UserRole.TeamMember,
        CreatedAt = DateTime.UtcNow
    };

    var member3 = new BrainstormingApp.Core.Entities.User
    {
        Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
        Email = "member3@test.com",
        PasswordHash = hashedPassword,
        FirstName = "Member",
        LastName = "Three",
        Role = BrainstormingApp.Core.Enums.UserRole.TeamMember,
        CreatedAt = DateTime.UtcNow
    };

    context.Users.AddRange(eventManager, teamLeader, member1, member2, member3);

    // Create test event
    var testEvent = new BrainstormingApp.Core.Entities.Event
    {
        Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
        Name = "Test Brainstorming Event",
        Description = "This is a test event for development",
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(30),
        Status = BrainstormingApp.Core.Enums.EventStatus.Active,
        CreatedById = eventManager.Id,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    context.Events.Add(testEvent);

    // Create test topic
    var testTopic = new BrainstormingApp.Core.Entities.Topic
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        EventId = testEvent.Id,
        Title = "How to improve team productivity?",
        Description = "Brainstorm ideas for improving team productivity and collaboration",
        Status = BrainstormingApp.Core.Enums.TopicStatus.Open,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    context.Topics.Add(testTopic);

    // Create test team
    var testTeam = new BrainstormingApp.Core.Entities.Team
    {
        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        EventId = testEvent.Id,
        Name = "Alpha Team",
        Description = "Test team for brainstorming",
        LeaderId = teamLeader.Id,
        MaxMembers = 6,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    context.Teams.Add(testTeam);

    // Add team members (including event manager for testing)
    var teamMembers = new[]
    {
        new BrainstormingApp.Core.Entities.TeamMember { Id = Guid.NewGuid(), TeamId = testTeam.Id, UserId = eventManager.Id, JoinedAt = DateTime.UtcNow },
        new BrainstormingApp.Core.Entities.TeamMember { Id = Guid.NewGuid(), TeamId = testTeam.Id, UserId = teamLeader.Id, JoinedAt = DateTime.UtcNow },
        new BrainstormingApp.Core.Entities.TeamMember { Id = Guid.NewGuid(), TeamId = testTeam.Id, UserId = member1.Id, JoinedAt = DateTime.UtcNow },
        new BrainstormingApp.Core.Entities.TeamMember { Id = Guid.NewGuid(), TeamId = testTeam.Id, UserId = member2.Id, JoinedAt = DateTime.UtcNow },
        new BrainstormingApp.Core.Entities.TeamMember { Id = Guid.NewGuid(), TeamId = testTeam.Id, UserId = member3.Id, JoinedAt = DateTime.UtcNow }
    };

    context.TeamMembers.AddRange(teamMembers);

    await context.SaveChangesAsync();

    Console.WriteLine("=== TEST DATA SEEDED ===");
    Console.WriteLine("Users (password: 123456):");
    Console.WriteLine("  - manager@test.com (EventManager)");
    Console.WriteLine("  - leader@test.com (TeamLeader)");
    Console.WriteLine("  - member1@test.com (TeamMember)");
    Console.WriteLine("  - member2@test.com (TeamMember)");
    Console.WriteLine("  - member3@test.com (TeamMember)");
    Console.WriteLine("Event: Test Brainstorming Event");
    Console.WriteLine("Topic: How to improve team productivity?");
    Console.WriteLine("Team: Alpha Team (5 members)");
    Console.WriteLine("========================");
}
