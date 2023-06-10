﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.Reflection;

namespace SLCP.API.ModelBinding;

public class BodyAndRouteModelBinder : IModelBinder
{
	private readonly IModelBinder _bodyBinder;

	public BodyAndRouteModelBinder(IModelBinder bodyBinder)
	{
		this._bodyBinder = bodyBinder;
	}

	public async Task BindModelAsync(ModelBindingContext bindingContext)
	{
		await this._bodyBinder.BindModelAsync(bindingContext);

		if (!bindingContext.Result.IsModelSet)
		{
			return;
		}

		var routeDataValues = bindingContext.ActionContext.RouteData.Values;
		var routeParams = routeDataValues.Except(routeDataValues.Where(v => v.Key == "controller"))
			.ToDictionary(x => x.Key, x => x.Value.ToString());

		var queryStringParams = QueryStringValues(bindingContext.HttpContext.Request);
		var allUriParams = routeParams.Union(queryStringParams)
			.ToDictionary(pair => pair.Key, pair => pair.Value);

		foreach (var key in allUriParams.Keys)
		{
			var prop = bindingContext.ModelType.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
			if (prop == null)
			{
				continue;
			}

			var typeConverter = TypeDescriptor.GetConverter(prop.PropertyType);
			if (typeConverter.CanConvertFrom(typeof(string)))
			{
				prop.SetValue(bindingContext.Result.Model, typeConverter.ConvertFromString(allUriParams[key]));
			}
		}

		if (bindingContext.Result.IsModelSet)
		{
			bindingContext.Model = bindingContext.Result.Model;
		}
	}

	private static IDictionary<string, string> QueryStringValues(HttpRequest request)
	{
		return request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
	}
}