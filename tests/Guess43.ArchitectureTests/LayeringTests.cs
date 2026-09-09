using FluentAssertions;
using Guess43.Application.Common;
using Guess43.Domain.Games;
using Guess43.Infrastructure.Persistence;
using NetArchTest.Rules;
using Xunit;

namespace Guess43.ArchitectureTests;

public class LayeringTests
{
    private const string DomainNamespace = "Guess43.Domain";
    private const string ApplicationNamespace = "Guess43.Application";
    private const string InfrastructureNamespace = "Guess43.Infrastructure";
    private const string ApiNamespace = "Guess43.Api";

    private static readonly System.Reflection.Assembly Domain = typeof(GameSession).Assembly;
    private static readonly System.Reflection.Assembly Application = typeof(Result).Assembly;
    private static readonly System.Reflection.Assembly Infrastructure = typeof(AppDbContext).Assembly;

    [Fact]
    public void Domain_should_not_depend_on_other_layers()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    [Fact]
    public void Domain_should_not_depend_on_entity_framework()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_api()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    [Fact]
    public void Application_should_not_depend_on_entity_framework()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        var result = Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildMessage(result));
    }

    private static string BuildMessage(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Offending types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
