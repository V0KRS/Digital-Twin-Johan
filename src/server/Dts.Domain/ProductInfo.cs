using System.Reflection;

namespace Dts.Domain;

/// <summary>Static product identity. Domain layer: no I/O.</summary>
public static class ProductInfo
{
    public const string Name = "Industrial Digital Twin Studio";
    public const int ConfigSchemaVersion = 1;

    public static string Version =>
        typeof(ProductInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0";
}
