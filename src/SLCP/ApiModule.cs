using Autofac;
using SLCP.API.Security.Authentication;
using SLCP.API.Security.Authorization;
using SLCP.Business.Services;

namespace SLCP.API;

public class ApiModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<AuthenticationProvider>().As<IAuthenticationProvider>().InstancePerLifetimeScope();
		builder.RegisterType<AuthorizationProvider>().As<IAuthorizationProvider>().InstancePerLifetimeScope();
		builder.RegisterType<RequestContext>().As<IRequestContext>().InstancePerLifetimeScope();
	}
}