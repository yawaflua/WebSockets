using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Routing;

namespace yawaflua.WebSockets.Attributes;

/// <summary>
/// Attribute that marks WebSocket endpoint handlers. 
/// Applied to classes or methods to define WebSocket route templates.
/// 
/// Usage examples:
/// [WebSocket("/chat")] - basic route
/// [WebSocket("/notifications/{userId}")] - route with parameter
/// [WebSocket("/game/{roomId}/players")] - complex route
/// </summary>
/// <remarks>
/// Inherits from ASP.NET Core's RouteAttribute to leverage standard routing syntax.
/// When applied to a class, defines the base path for all WebSocket endpoints in controller.
/// When applied to methods, defines specific sub-routes (requires class-level base path).
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
[ApiExplorerSettings(IgnoreApi = true)]
public class WebSocketAttribute : RouteAttribute, IRouteTemplateProvider, IApiDescriptionVisibilityProvider
{
    /// <summary>
    /// Original route template specified in attribute
    /// </summary>
    public new string Template { get; }

    public new int? Order { get; } = 0;
    public new string? Name { get; }

    /// <summary>
    /// Creates WebSocket route definition
    /// </summary>
    /// <param name="path">Route template using ASP.NET Core syntax:
    /// - Static: "/status"
    /// - Parameters: "/user/{id}"
    /// - Constraints: "/file/{name:alpha}"
    /// - Optional: "/feed/{category?}"</param>
    public WebSocketAttribute([RouteTemplate]string path) : base(path)
    {
        Template = path;
        Name = path;
    }

    public bool IgnoreApi => true;
}