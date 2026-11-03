using Companion.Backend.Api.Features.ServiceAuth;
using Microsoft.Extensions.Configuration;

namespace Companion.Backend.Tests.Features.ServiceAuth;

public sealed class LocalSecretsTests : IDisposable
{
    private const string _publicKeyName = "ServiceAuth:CurrentPublicKey";
    private const string _previousKeyName = "ServiceAuth:PreviousPublicKey";

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("companion-secrets-");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void AddLocalSecrets_FileInParentDirectory_LoadsOwnKeysWithBase64Padding()
    {
        var project = _root.CreateSubdirectory("src").CreateSubdirectory("project");
        WriteSecrets("# comment\r\nSERVICE_TOKEN_PUBLIC_KEY=abc==\r\nSERVICE_TOKEN_PREVIOUS_PUBLIC_KEY=def=\r\n");

        var configuration = new ConfigurationBuilder();
        configuration.AddLocalSecrets(project.FullName);

        var result = configuration.Build();
        Assert.Equal("abc==", result[_publicKeyName]);
        Assert.Equal("def=", result[_previousKeyName]);
    }

    [Fact]
    public void AddLocalSecrets_PrivateKeyInFile_DoesNotLoadIt()
    {
        WriteSecrets("SERVICE_TOKEN_PRIVATE_KEY=private\n");

        var configuration = new ConfigurationBuilder();
        configuration.AddLocalSecrets(_root.FullName);

        Assert.Empty(configuration.Build().AsEnumerable());
    }

    [Fact]
    public void AddLocalSecrets_ValueFromOtherSource_OtherSourceWins()
    {
        WriteSecrets("SERVICE_TOKEN_PUBLIC_KEY=from-file\n");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([KeyValuePair.Create<string, string?>(_publicKeyName, "from-other-source")]);
        configuration.AddLocalSecrets(_root.FullName);

        Assert.Equal("from-other-source", configuration.Build()[_publicKeyName]);
    }

    private void WriteSecrets(string content) => File.WriteAllText(Path.Combine(_root.FullName, ".env"), content);
}
