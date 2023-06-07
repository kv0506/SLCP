using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace SLCP.API.Security;

public static class ActionDescriptorExtensions
{
    public static bool IsDefinedOnActionOrController<T>(this ActionDescriptor actionDescriptor, bool inherit)
        where T : Attribute
    {
        if (actionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return controllerActionDescriptor.MethodInfo.IsDefined(typeof(T), inherit) ||
                   controllerActionDescriptor.ControllerTypeInfo.IsDefined(typeof(T), inherit);
        }

        return false;
    }

    public static T GetCustomAttributeFromActionOrController<T>(this ActionDescriptor actionDescriptor,
        bool inherit)
        where T : Attribute
    {
        if (actionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return controllerActionDescriptor.GetCustomAttributeFromActionOrController<T>(inherit);
        }

        return default;
    }

    public static T GetCustomAttributeFromActionOrController<T>(this ControllerActionDescriptor controllerActionDescriptor,
        bool inherit)
        where T : Attribute
    {
        var actionAttributes = (T[])controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(T), inherit);
        if (actionAttributes.Length > 0)
        {
            return actionAttributes[0];
        }

        var controllerAttributes = (T[])controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(T), inherit);
        if (controllerAttributes.Length > 0)
        {
            return controllerAttributes[0];
        }

        return default;
    }
}