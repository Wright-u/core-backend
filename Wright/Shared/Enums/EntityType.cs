namespace Wright.Shared.Enums;

public enum IdentifierType
{
    APP,
    FUNCTION,
    RELATION,
    CLASS,
    UNKNOWN
}

public static class IdentifierTypeExtensions
{
    public static string ToIdentifierTypeString(this IdentifierType identifierType) => identifierType.ToString().ToLower();
    public static IdentifierType ToIdentifierType(this string identifierTypeString) => Enum.Parse<IdentifierType>(identifierTypeString);
}