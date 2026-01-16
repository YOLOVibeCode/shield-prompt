using FluentAssertions;
using ShieldPrompt.Application.Services;
using System;
using System.Xml.Linq;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

/// <summary>
/// Tests for XmlPromptBuilder - ensures type-safe XML generation.
/// </summary>
public class XmlPromptBuilderTests
{
    private readonly XmlPromptBuilder _sut;

    public XmlPromptBuilderTests()
    {
        _sut = new XmlPromptBuilder();
    }

    #region BuildResponseContract Tests

    [Fact]
    public void BuildResponseContract_GeneratesValidXml()
    {
        // Act
        var xml = _sut.BuildResponseContract();
        
        // Assert - Should parse without errors
        var act = () => XDocument.Parse(xml);
        act.Should().NotThrow("generated XML must be well-formed");
    }

    [Fact]
    public void BuildResponseContract_HasCodeChangesRoot()
    {
        // Act
        var xml = _sut.BuildResponseContract();
        var doc = XDocument.Parse(xml);
        
        // Assert
        doc.Root.Should().NotBeNull();
        doc.Root!.Name.LocalName.Should().Be("code_changes");
    }

    [Fact]
    public void BuildResponseContract_ContainsAllOperationTypes()
    {
        // Act
        var xml = _sut.BuildResponseContract();
        
        // Assert
        xml.Should().Contain("<file_operation>UPDATE</file_operation>");
        xml.Should().Contain("<file_operation>CREATE</file_operation>");
        xml.Should().Contain("<file_operation>DELETE</file_operation>");
    }

    [Fact]
    public void BuildResponseContract_DeleteExampleHasNoFileCode()
    {
        // Act
        var xml = _sut.BuildResponseContract();
        var doc = XDocument.Parse(xml);
        
        // Assert
        var deleteFile = doc.Descendants("changed_file")
            .FirstOrDefault(cf => cf.Element("file_operation")?.Value == "DELETE");
        
        deleteFile.Should().NotBeNull();
        deleteFile!.Element("file_code").Should().BeNull("DELETE operations should not have file_code");
    }

    [Fact]
    public void BuildResponseContract_UpdateAndCreateHaveFileCode()
    {
        // Act
        var xml = _sut.BuildResponseContract();
        var doc = XDocument.Parse(xml);
        
        // Assert
        var updateFile = doc.Descendants("changed_file")
            .FirstOrDefault(cf => cf.Element("file_operation")?.Value == "UPDATE");
        var createFile = doc.Descendants("changed_file")
            .FirstOrDefault(cf => cf.Element("file_operation")?.Value == "CREATE");
        
        updateFile!.Element("file_code").Should().NotBeNull();
        createFile!.Element("file_code").Should().NotBeNull();
    }

    [Fact]
    public void BuildResponseContract_FileCodeUsesCDATA()
    {
        // Act
        var xml = _sut.BuildResponseContract();
        
        // Assert
        xml.Should().Contain("<![CDATA[", "file_code should use CDATA sections");
    }

    #endregion

    #region CreateFileExample Tests

    [Fact]
    public void CreateFileExample_WithUpdate_ContainsAllRequiredElements()
    {
        // Act
        var element = _sut.CreateFileExample("UPDATE", "src/App.cs", "Added logging", "public class App {}");
        
        // Assert
        element.Name.LocalName.Should().Be("changed_file");
        element.Element("file_path")!.Value.Should().Be("src/App.cs");
        element.Element("file_summary")!.Value.Should().Be("Added logging");
        element.Element("file_operation")!.Value.Should().Be("UPDATE");
        element.Element("file_code").Should().NotBeNull();
    }

    [Fact]
    public void CreateFileExample_WithDelete_OmitsFileCode()
    {
        // Act
        var element = _sut.CreateFileExample("DELETE", "src/OldFile.cs", "Removed deprecated class");
        
        // Assert
        element.Element("file_code").Should().BeNull("DELETE should not have file_code");
    }

    [Fact]
    public void CreateFileExample_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var summary = "Fix <bug> & \"issue\" in 'code'";
        var code = "var x = \"<xml>\" & '<tag>';";
        
        // Act
        var element = _sut.CreateFileExample("UPDATE", "src/Test.cs", summary, code);
        var xml = element.ToString();
        
        // Assert - Should parse without errors
        var act = () => XDocument.Parse($"<root>{xml}</root>");
        act.Should().NotThrow("special characters should be escaped");
        
        element.Element("file_summary")!.Value.Should().Be(summary);
        element.Element("file_code")!.Value.Should().Be(code);
    }

    [Fact]
    public void CreateFileExample_WithNullCode_CreatesElementWithoutFileCode()
    {
        // Act
        var element = _sut.CreateFileExample("UPDATE", "src/Test.cs", "Summary", code: null);
        
        // Assert
        element.Element("file_code").Should().BeNull();
    }

    [Fact]
    public void CreateFileExample_WithEmptyCode_CreatesElementWithEmptyCDATA()
    {
        // Act
        var element = _sut.CreateFileExample("UPDATE", "src/Test.cs", "Summary", code: "");
        
        // Assert
        element.Element("file_code").Should().NotBeNull();
        element.Element("file_code")!.Value.Should().BeEmpty();
    }

    #endregion

    #region AppendResponseInstructions Tests

    [Fact]
    public void AppendResponseInstructions_AppendsToExistingPrompt()
    {
        // Arrange
        var existingPrompt = "# Your Task\nAnalyze this code.";
        
        // Act
        var result = _sut.AppendResponseInstructions(existingPrompt);
        
        // Assert
        result.Should().StartWith(existingPrompt);
        result.Should().Contain("⚠️ CRITICAL: OUTPUT FORMAT");
    }

    [Fact]
    public void AppendResponseInstructions_ContainsResponseContract()
    {
        // Arrange
        var existingPrompt = "Existing prompt";
        
        // Act
        var result = _sut.AppendResponseInstructions(existingPrompt);
        
        // Assert
        result.Should().Contain("<code_changes>");
        result.Should().Contain("<changed_file>");
    }

    [Fact]
    public void AppendResponseInstructions_ContainsAllRules()
    {
        // Act
        var result = _sut.AppendResponseInstructions("Test");
        
        // Assert
        result.Should().Contain("Respond ONLY with valid XML");
        result.Should().Contain("No markdown, no explanations");
        result.Should().Contain("One <changed_file> per file");
        result.Should().Contain("For DELETE: omit <file_code>");
        result.Should().Contain("Use relative paths");
    }

    [Fact]
    public void AppendResponseInstructions_WithEmptyPrompt_GeneratesValidOutput()
    {
        // Act
        var result = _sut.AppendResponseInstructions("");
        
        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("<code_changes>");
    }

    #endregion

    #region IsWellFormedXml Tests

    [Fact]
    public void IsWellFormedXml_WithValidXml_ReturnsTrue()
    {
        // Arrange
        var validXml = "<code_changes><changed_file><file_path>test.cs</file_path></changed_file></code_changes>";
        
        // Act
        var result = _sut.IsWellFormedXml(validXml, out var error);
        
        // Assert
        result.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void IsWellFormedXml_WithInvalidXml_ReturnsFalse()
    {
        // Arrange
        var invalidXml = "<code_changes><changed_file>unclosed";
        
        // Act
        var result = _sut.IsWellFormedXml(invalidXml, out var error);
        
        // Assert
        result.Should().BeFalse();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IsWellFormedXml_WithMalformedXml_ReturnsErrorMessage()
    {
        // Arrange
        var malformedXml = "<root><unclosed></root>";
        
        // Act
        var result = _sut.IsWellFormedXml(malformedXml, out var error);
        
        // Assert
        result.Should().BeFalse();
        error.Should().Contain("unclosed", "error message should indicate the problem");
    }

    [Fact]
    public void IsWellFormedXml_WithEmptyString_ReturnsFalse()
    {
        // Act
        var result = _sut.IsWellFormedXml("", out var error);
        
        // Assert
        result.Should().BeFalse();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IsWellFormedXml_WithNullString_ReturnsFalse()
    {
        // Act
        var result = _sut.IsWellFormedXml(null!, out var error);
        
        // Assert
        result.Should().BeFalse();
        error.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void BuildResponseContract_OutputCanBeParsedAndRebuilt()
    {
        // Act
        var originalXml = _sut.BuildResponseContract();
        var doc = XDocument.Parse(originalXml);
        var rebuiltXml = doc.ToString(SaveOptions.DisableFormatting);
        
        // Assert - Should be able to round-trip
        var isValid = _sut.IsWellFormedXml(rebuiltXml, out _);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void CreateFileExample_WithCDATAContent_ParsesCorrectly()
    {
        // Arrange
        var code = "public class Test { /* <xml> & \"quotes\" */ }";
        
        // Act
        var element = _sut.CreateFileExample("UPDATE", "test.cs", "Test", code);
        var xml = element.ToString();
        var parsed = XElement.Parse(xml);
        
        // Assert
        parsed.Element("file_code")!.Value.Should().Be(code, "CDATA should preserve content exactly");
    }

    #endregion
}

