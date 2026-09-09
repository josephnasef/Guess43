using Xunit;

namespace Guess43.IntegrationTests;

[CollectionDefinition(nameof(ApiCollection))]
public sealed class ApiCollection : ICollectionFixture<Guess43WebFactory>;
