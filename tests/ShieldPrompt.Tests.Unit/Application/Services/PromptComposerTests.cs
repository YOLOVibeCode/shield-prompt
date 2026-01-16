using FluentAssertions;
using NSubstitute;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class PromptComposerTests
{
    private readonly ITokenCountingService _tokenCountingService;
    private readonly IXmlPromptBuilder _xmlPromptBuilder;
    private readonly PromptComposer _sut;

    public PromptComposerTests()
    {
        _tokenCountingService = Substitute.For<ITokenCountingService>();
        _tokenCountingService.CountTokens(Arg.Any<string>()).Returns(100);
        
        _xmlPromptBuilder = new XmlPromptBuilder();
        _sut = new PromptComposer(_tokenCountingService, _xmlPromptBuilder);
    }

    [Fact]
    public void Compose_IncludesCodeChangesResponseContract()
    {
        // Arrange
        var template = new PromptTemplate(
            Id: "test",
            Name: "Test Template",
            Icon: "🧪",
            Description: "Test",
            SystemPrompt: "Review the code.",
            FocusOptions: Array.Empty<string>(),
            RequiresCustomInput: false);

        var file = new FileNode("/repo/Test.cs", "Test.cs", false)
        {
            Content = "public class Test {}"
        };

        var options = new PromptOptions(
            CustomInstructions: "Be concise.",
            SelectedFocusAreas: null,
            SelectedRole: null,
            IncludeFilePaths: true,
            IncludeLineNumbers: false);

        // Act
        var composed = _sut.Compose(template, new[] { file }, options);

        // Assert
        composed.FullPrompt.Should().Contain("<code_changes>");
        composed.FullPrompt.Should().Contain("<file_operation>CREATE|UPDATE|DELETE</file_operation>");
        composed.FullPrompt.Should().Contain("START YOUR RESPONSE WITH: <code_changes");
    }
}

