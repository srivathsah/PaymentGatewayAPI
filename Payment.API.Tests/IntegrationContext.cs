using Alba;
using LanguageExt;
using LanguageExt.Common;
using Orleans.Concurrency;
using Xunit;
using static LanguageExt.Prelude;

namespace Payment.API.Tests;

public abstract class IntegrationContext : IClassFixture<AppFixture>
{
    protected IntegrationContext(AppFixture fixture)
    {
        Fixture = fixture;
    }

    public AppFixture Fixture { get; }

    /// <summary>
    /// Runs Alba HTTP scenarios through your ASP.Net Core system
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    protected Task<IScenarioResult> Scenario(Action<Scenario> configure)
    {
        return Fixture.AlbaHost.Scenario(configure);
    }

    // The Alba system
    protected IAlbaHost System => Fixture.AlbaHost;

    // Just a convenience because you use it pretty often
    // in tests to get at application services
    protected IServiceProvider Services => Fixture.AlbaHost.Services;

    protected static Immutable<Validation<Error, IEnumerable<TEvent>>> Success<TEvent>() => new(Success<Error, IEnumerable<TEvent>>(new List<TEvent>()));
    protected static Immutable<Validation<Error, IEnumerable<TEvent>>> Error<TEvent>(string error) => new(Fail<Error, IEnumerable<TEvent>>(LanguageExt.Common.Error.New(error)));
}
