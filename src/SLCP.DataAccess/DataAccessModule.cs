using Autofac;
using SLCP.DataAccess.Repositories.Contracts;

namespace SLCP.DataAccess;

public class DataAccessModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterAssemblyTypes(typeof(IUserRepository).Assembly)
			.PublicOnly()
			.AsImplementedInterfaces()
			.InstancePerDependency();
	}
}