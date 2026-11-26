namespace IntegrationTests.TestHost;

/// <summary>All integration tests share one API and one database container.</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
