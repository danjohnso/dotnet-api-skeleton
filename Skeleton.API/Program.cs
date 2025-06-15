using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using Skeleton.API.BackgroundServices.Scheduled;
using Skeleton.API.Core;
using Skeleton.API.Core.Auth;
using Skeleton.API.Core.ProblemDetails;
using Skeleton.API.Logging;
using Skeleton.API.v1.Requests;
using Skeleton.Business;
using Skeleton.Core;
using System.Reflection;
using System.Text.Json.Serialization;

Bootstrapper.Run(args, () =>
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    bool reloadOnChange = builder.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);
    if (builder.Environment.IsLocal() && builder.Environment.ApplicationName is { Length: > 0 })
    {
        Assembly appAssembly = Assembly.Load(new AssemblyName(builder.Environment.ApplicationName));
        if (appAssembly is not null)
        {
            builder.Configuration.AddUserSecrets(appAssembly, optional: true, reloadOnChange: reloadOnChange);
        }
    }
    else
    {
        string keyVaultUrl = builder.Configuration["KeyVault:Url"] ?? throw new NullReferenceException($"Unable to find Key Vault Url in config under key 'KeyVault:Url'");
        TimeSpan? reloadInterval = builder.Configuration.GetValue<TimeSpan?>("KeyVault:RefreshTimeSpan");

        builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUrl),
                new DefaultAzureCredential(),
                new AzureKeyVaultConfigurationOptions()
                {
                    ReloadInterval = reloadInterval,
                    Manager = new KeyVaultSecretManager()
                }
            );
    }

    IdentityModelEventSource.ShowPII = builder.Configuration.GetValue<bool?>("EnableEventSourcePII") ?? false;

    builder.Host.ConfigureLogging();
    builder.Host.UseDefaultServiceProvider((host, options) =>
    {
        options.ValidateOnBuild = host.HostingEnvironment.IsDevelopmentOrLocal();
        options.ValidateScopes = host.HostingEnvironment.IsDevelopmentOrLocal();
    });

    builder.Services.AddTransient<ActionLogger>();
    builder.Services.AddAntiforgery(_ => _.HeaderName = Constants.AntiforgeryHeader);

    builder.Services.AddCors(_ =>
    {
        _.AddPolicy(Constants.DefaultCorsPolicy,
            builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
    });

    builder.Services.AddRouting(_ => _.LowercaseUrls = true);

    IMvcBuilder mvcBuilder = builder.Services.AddControllers();
    if (builder.Environment.IsDevelopmentOrLocal())
    {
        mvcBuilder.AddControllersAsServices();
    }

    mvcBuilder.AddJsonOptions(_ =>
    {
        _.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services.Configure<ApiBehaviorOptions>(_ =>
    {
        _.InvalidModelStateResponseFactory = actionContext =>
        {
            ILoggerFactory loggerFactory = actionContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(actionContext.ActionDescriptor.DisplayName ?? "Unknown Action");

            Dictionary<string, List<Problem>> errors = actionContext.ModelState
                .Where(e => e.Key != "request" && e.Value?.Errors.Count > 0)
                .Select(e => new KeyValuePair<string, string>(e.Key, e.Value?.Errors.First().ErrorMessage ?? string.Empty))
                .ToDictionary(x => x.Key, x => new List<Problem> { new(Problems.UnprocessableEntity.Code, x.Value) });

            logger.LogWarning("Invalid request for {Path}, returning ValidationProblemDetails: {Errors}", actionContext.HttpContext.Request.Path, errors);

            ProblemDetails problemDetails = new()
            {
                Type = ProblemDetailsConstants.Status422Uri,
                Title = "Invalid Request",
                Status = StatusCodes.Status422UnprocessableEntity,
                Instance = $"{actionContext.HttpContext.Request.Method} {actionContext.HttpContext.Request.Path}",
            };

            problemDetails.Extensions.Add(ProblemDetailsConstants.ErrorsKey, errors);

            string? traceId = actionContext.HttpContext.GetTraceId();
            if (traceId != null)
            {
                problemDetails.Extensions.Add(ProblemDetailsConstants.TraceIdKey, traceId);
            }

            return new UnprocessableEntityObjectResult(problemDetails)
            {
                ContentTypes = { ProblemDetailsConstants.ContentType }
            };
        };
    });

    // register versioning
    builder.Services.AddApiVersioning(_ =>
    {
        _.ApiVersionSelector = new CurrentImplementationApiVersionSelector(_);
        _.ReportApiVersions = true;
    }).AddApiExplorer(_ =>
    {
        _.GroupNameFormat = "'v'VVV";
        _.SubstituteApiVersionInUrl = true;
    });

    if (builder.Environment.IsLocal())
    {
        builder.Services.AddSwagger(builder.Configuration);
    }

    JwtOptions jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
        ?? throw new NullReferenceException("Unable to find configuration section name 'Jwt'!");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.Authority = jwtOptions.Authority;
        options.Audience = jwtOptions.Audience;
    });

    builder.Services.AddValidatorsFromAssemblyContaining<FileUploadRequest>();

    builder.Services.AddBusiness(builder.Configuration);

    builder.Services.AddHostedService<SampleTimedHostedService>();

    return builder;
}, (host) =>
{
    IApiVersionDescriptionProvider apiVersionDescriptionProvider = host.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    //exception handling should be #1 in the pipeline
    host.UseGlobalExceptionMiddleware();

    if (host.Environment.IsLocal())
    {
        host.UseSwagger(apiVersionDescriptionProvider);
    }

    //make sure calls are to the right endpoints and valid
    host.UseHttpsRedirection();
    host.UseCors(Constants.DefaultCorsPolicy);

    //routing > auth > authz
    host.UseRouting();
    host.UseAuthentication();
    host.UseAuthorization();

    host.MapControllers().RequireAuthorization();
    host.MapVersions(apiVersionDescriptionProvider);
});