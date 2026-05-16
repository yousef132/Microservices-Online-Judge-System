using System.Reflection;
using Community.API.Common.Abstractions;

namespace Community.API.Common.Extensions;

public static class EndpointExtensions
{
    public static void MapEndpoints(this WebApplication app, Assembly? assembly = null)
    {
        var endpointTypes = (assembly ?? Assembly.GetExecutingAssembly())
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)Activator.CreateInstance(type)!;
            endpoint.MapEndpoint(app);
        }
    }
}
