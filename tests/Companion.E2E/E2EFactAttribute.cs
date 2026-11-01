namespace Companion.E2E;

/// <summary>Skips the test unless COMPANION_E2E=1, because it builds and starts the Docker stack.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class E2EFactAttribute : FactAttribute
{
    public E2EFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("COMPANION_E2E") != "1")
        {
            Skip = "Set COMPANION_E2E=1 to run tests that need Docker.";
        }
    }
}
