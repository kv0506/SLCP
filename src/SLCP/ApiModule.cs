using Autofac;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using MediatR.Extensions.Autofac.DependencyInjection;
using SLCP.API.Security.Authentication;
using SLCP.API.Security.Authorization;
using SLCP.Business.Request;
using SLCP.Business.Services;

namespace SLCP.API;

public class ApiModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		var configuration = MediatRConfigurationBuilder
			.Create(typeof(LoginCommand).Assembly)
			.WithAllOpenGenericHandlerTypesRegistered()
			.WithRegistrationScope(RegistrationScope.Scoped)
			.Build();

		builder.RegisterMediatR(configuration);

		builder.RegisterType<AuthenticationProvider>().As<IAuthenticationProvider>().InstancePerLifetimeScope();
		builder.RegisterType<AuthorizationProvider>().As<IAuthorizationProvider>().InstancePerLifetimeScope();
		builder.RegisterType<RequestContext>().As<IRequestContext>().InstancePerLifetimeScope();
	}
}