using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using SLCP.API;
using SLCP.API.Middleware;
using SLCP.API.Security.Attributes;
using SLCP.Business;
using SLCP.Business.Request;
using SLCP.Business.Services;
using SLCP.DataAccess;
using SLCP.DataAccess.CosmosService;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
	.ConfigureContainer<ContainerBuilder>(containerBuilder =>
	{
		var configuration = MediatRConfigurationBuilder
			.Create(typeof(LoginCommand).Assembly)
			.WithAllOpenGenericHandlerTypesRegistered()
			.WithRegistrationScope(RegistrationScope.Scoped)
			.Build();

		containerBuilder.RegisterMediatR(configuration);

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