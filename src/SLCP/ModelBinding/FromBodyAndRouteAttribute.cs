using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SLCP.API.ModelBinding;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class FromBodyAndRouteAttribute : Attribute, IBindingSourceMetadata
{
	public BindingSource BindingSource => BodyAndRouteBindingSource.BodyAndRoute;
}