namespace FluentHttp.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class HeaderAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}