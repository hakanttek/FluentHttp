namespace FluentHttp.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class QueryAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}