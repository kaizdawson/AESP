using AESP.API.Helpers;

using AESP.Common.DTOs;
using AESP.Realtime.Hubs;
using AESP.Realtime.Interfaces;
using AESP.Realtime.Services;
using AESP.Repository.Contract;
using AESP.Repository.DB;
using AESP.Repository.Implementation;
using AESP.Repository.Models;
using AESP.Repository.Repositories;
using AESP.Service.BackgroundJobs;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using CloudinaryDotNet;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!)
        ),
        ClockSkew = TimeSpan.Zero,
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha512 }
    };

    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // SignalR sends token via query string or access_token parameter
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // If this is a SignalR hub request and token is in query string
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            // Also check for token in Authorization header (standard way)
            else if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\": \"Bạn chưa đăng nhập hoặc token không hợp lệ\"}");
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\": \"Không đủ quyền hạn để truy cập\"}");
        }
    };
});




// Add services to the container.

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IQuestionAssessmentService, QuestionAssessmentService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IAdminReviewerService, AdminReviewerService>();
builder.Services.AddScoped<IReviewerProfileService, ReviewerProfileService>();  
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IAssessmentDetailService, AssessmentDetailService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminFeedbackService, AdminFeedbackService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();  
builder.Services.AddScoped<IAdminLearnerService, AdminLearnerService>();
builder.Services.AddScoped<IAdminManagerService, AdminManagerService>();
builder.Services.AddScoped<IAdminReviewerIncomeService, AdminReviewerIncomeService>();
builder.Services.AddScoped<IAuthQueryRepository, AuthQueryRepository>();
builder.Services.AddScoped<IAuthQueryService, AuthQueryService>();
builder.Services.AddScoped<ICoinService, CoinService>();
builder.Services.AddScoped<ILearnerCourseService, LearnerCourseService>();
builder.Services.AddScoped<ILearningPathCourseService, LearningPathCourseService>();
builder.Services.AddScoped<IAvatarService, AvatarService>();
builder.Services.AddScoped<IQuestionMediaService, QuestionMediaService>();
builder.Services.AddScoped<ILearnerQuestionService, LearnerQuestionService>();
builder.Services.AddScoped<ILearnerAnswerService, LearnerAnswerService>();
builder.Services.AddScoped<ILearningPathExerciseService, LearningPathExerciseService>();
builder.Services.AddScoped<ILearnerReviewRequestService, LearnerReviewRequestService>();
builder.Services.AddScoped<ILearningPathQuestionService, LearningPathQuestionService>();
builder.Services.AddScoped<IAdminWithdrawalService, AdminWithdrawalService>();
builder.Services.AddScoped<IAdminPurchaseService, AdminPurchaseService>();

builder.Services.AddScoped<ILearningPathChapterService, LearningPathChapterService>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IServicePackageService, ServicePackageService>();
builder.Services.AddScoped<IAIConversationChargeService, AIConversationChargeService>();

builder.Services.AddSignalR();
builder.Services.AddScoped<IReviewerReviewService, ReviewerReviewService>();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddScoped<IAdminReviewFeeService, AdminReviewFeeService>();
builder.Services.AddScoped<ILearnerBuyReview, LearnerBuyReview>();
builder.Services.AddScoped<IRecordService, RecordService>();
builder.Services.AddScoped<IRecordCategoryService, RecordCategoryService>();

builder.Services.AddScoped<IProgressAnalyticsService, ProgressAnalyticsService>(); 
builder.Services.AddScoped<IProgressAnalyticsQueryService, ProgressAnalyticsQueryService>(); 
builder.Services.AddHostedService<ProgressAnalyticsBackgroundService>();


builder.Services.AddHttpClient<PayOSService>();
builder.Services.Configure<PayOSConfig>(builder.Configuration.GetSection("PayOS"));


var cloudinaryConfig = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
if (cloudinaryConfig == null)
    throw new Exception("❌ CloudinarySettings is missing in appsettings.json.");

Account account = new Account(
    cloudinaryConfig.CloudName,
    cloudinaryConfig.ApiKey,
    cloudinaryConfig.ApiSecret
);

Cloudinary cloudinary = new Cloudinary(account);
cloudinary.Api.Secure = true;

// Đăng ký đối tượng Cloudinary làm Singleton
builder.Services.AddSingleton(cloudinary);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AESP.API",
        Version = "1.0.0"
    });
    c.DescribeAllParametersInCamelCase();
    // Không cần nhập chữ Bearer nha mấy đứa
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,   
        Scheme = "bearer",                                         
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste your JWT token here (no need to add 'Bearer')"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    c.OperationFilter<AESP.API.Helpers.CookieParameterOperationFilter>();
    c.OperationFilter<FileUploadOperationFilter>();
});


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
                "https://fa-25-se-161-ai-english-speaking-pr.vercel.app",
                "https://localhost:3000",
                "https://aespwithai.com",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
       
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });



var firebaseConfig = builder.Configuration.GetSection("Firebase").Get<Dictionary<string, object>>();

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromJson(JsonSerializer.Serialize(firebaseConfig))
});
var app = builder.Build();





app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AESP.API v1");
    c.RoutePrefix = string.Empty; 

});


app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UpdateLastActiveMiddleware>();
app.MapControllers();
app.MapHub<ReviewerHub>("/api/hubs/reviewer");

app.Run();
