using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace SLCP.API.ModelBinding;

public class BodyAndRouteModelBinderProvider : IModelBinderProvider
{
	private readonly BodyModelBinderProvider _bodyModelBinderProvider;

	public BodyAndRouteModelBinderProvider(BodyModelBinderProvider bodyModelBinderProvider)
	{
		this._bodyModelBinderProvider = bodyModelBinderProvider;
	}

	public IModelBinder GetBinder(ModelBinderProviderContext context)
	{
		var bodyBinder = this._bodyModelBinderProvider.GetBinder(context);

		if (context.BindingInfo.BindingSource != null
		    && context.BindingInfo.BindingSource.CanAcceptDataFrom(BodyAndRouteBindingSource.BodyAndRoute))
		{
			return new BodyAndRouteModelBinder(bodyBinder);
		}

		return null;
	}
}