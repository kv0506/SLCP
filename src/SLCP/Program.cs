using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR.Extensions.FluentValidation.AspNetCore;
using SLCP.API;
using SLCP.API.Middleware;
using SLCP.API.Security.Attributes;
using SLCP.Business;
using SLCP.Business.Services;
using SLCP.Business.Validator;
using SLCP.DataAccess;
using SLCP.DataAccess.CosmosService;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
	.ConfigureContainer<ContainerBuilder>(containerBuilder =>
	{
		containerBuilder.RegisterModule(new ApiModule());
		containerBuilder.RegisterModule(new DataAccessModule());
		containerBuilder.RegisterModule(new BusinessModule());

		var cosmosSettings = new CosmosSettings();
		builder.Configuration.GetSection(CosmosSettings.SectionName).Bind(cosmosSettings);
		containerBuilder.RegisterInstance(cosmosSettings).As<CosmosSettings>();

		var accessTokenServiceSettings = new AccessTokenServiceSettings();
		builder.Configuration.GetSection(AccessTokenServiceSettings.SectionName).Bind(accessTokenServiceSettings);
		containerBuilder.RegisterInstance(accessTokenServiceSettings).As<AccessTokenServiceSettings>();

		containerBuilder.RegisterType<CosmosService>().As<ICosmosService>().InstancePerDependency();
	});

builder.Services.AddControllers(options =>
{
	options.Filters.Add(typeof(ProtectAttribute));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidation(new[] { typeof(LoginCommandValidator).GetTypeInfo().Assembly });

var app = builder.Build();

app.UseAppExceptionHandler();

// used for setting up data for testing
//app.Services.GetService<IDataSeeder>().SeedData(CancellationToken.None);

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();