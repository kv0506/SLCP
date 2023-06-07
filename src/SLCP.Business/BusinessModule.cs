using Autofac;
using SLCP.Business.Services;

namespace SLCP.Business;

public class BusinessModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<AccessTokenService>().As<IAccessTokenService>().InstancePerDependency();
	}
}