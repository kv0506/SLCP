using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Microsoft.Extensions.Configuration;
using SLCP.Business;
using SLCP.Business.Request;
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

		containerBuilder.RegisterModule(new DataAccessModule());
		containerBuilder.RegisterModule(new BusinessModule());

		var cosmosSettings = new CosmosSettings();
		builder.Configuration.GetSection(CosmosSettings.SectionName).Bind(cosmosSettings);
		containerBuilder.RegisterInstance(cosmosSettings).As<CosmosSettings>();

		containerBuilder.RegisterType<CosmosService>().As<ICosmosService>().InstancePerDependency();
	});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Services.GetService<IDataSeeder>().SeedData(CancellationToken.None);

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();