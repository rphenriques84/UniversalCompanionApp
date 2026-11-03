using Companion.Frontend.Features.ServiceAuth;
using Microsoft.Extensions.Configuration;

namespace Companion.Frontend.Tests.Features.ServiceAuth;

public sealed class LocalSecretsTests : IDisposable
{
    private const string _privateKeyName = "ServiceAuth:PrivateKey";

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("companion-secrets-");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void AddLocalSecrets_FileInParentDirectory_LoadsOwnKeyWithBase64Padding()
    {
        var project = _root.CreateSubdirectory("src").CreateSubdirectory("project");
        WriteSecrets("# comment\r\nSERVICE_TOKEN_PRIVATE_KEY=abc==\r\n");

        var configuration = new ConfigurationBuilder();
        configuration.AddLocalSecrets(project.FullName);

        Assert.Equal("abc==", configuration.Build()[_privateKeyName]);
    }

    [Fact]
    public void AddLocalSecrets_OtherSecretsInFile_LoadsOnlyOwnKey()
    {
        WriteSecrets("SERVICE_TOKEN_PUBLIC_KEY=public\n");

        var configuration = new ConfigurationBuilder();
        configuration.AddLocalSecrets(_root.FullName);

        Assert.Empty(configuration.Build().AsEnumerable());
    }

    [Fact]
    public void AddLocalSecrets_ValueFromOtherSource_OtherSourceWins()
    {
        WriteSecrets("SERVICE_TOKEN_PRIVATE_KEY=from-file\n");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([KeyValuePair.Create<string, string?>(_privateKeyName, "from-other-source")]);
        configuration.AddLocalSecrets(_root.FullName);

        Assert.Equal("from-other-source", configuration.Build()[_privateKeyName]);
    }

    private void WriteSecrets(string content) => File.WriteAllText(Path.Combine(_root.FullName, ".env"), content);
}
