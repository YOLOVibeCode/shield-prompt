using FluentAssertions;
using ShieldPrompt.Infrastructure.Services;

namespace ShieldPrompt.Tests.Unit.Application.Formats;

/// <summary>
/// Tests for IFormatMetadataRepository implementation.
/// TDD: Tests written BEFORE implementation.
/// </summary>
public class FormatMetadataRepositoryTests
{
    private readonly StaticFormatMetadataRepository _sut;
    
    public FormatMetadataRepositoryTests()
    {
        _sut = new StaticFormatMetadataRepository();
    }
    
    [Fact]
    public void GetAllFormats_ReturnsAllDefinedFormats()
    {
        // Act
        var formats = _sut.GetAllFormats();
        
        // Assert
        formats.Should().NotBeNull();
        formats.Should().NotBeEmpty();
    }
    
    [Fact]
    public void GetAllFormats_ReturnsAtLeast4Formats()
    {
        // Act
        var formats = _sut.GetAllFormats();
        
        // Assert
        formats.Count.Should().BeGreaterThanOrEqualTo(4, 
            "we should have at least 4 formats: Markdown, XML, JSON, Plain Text");
    }
    
    [Fact]
    public void GetById_WithValidId_ReturnsFormat()
    {
        // Act
        var markdown = _sut.GetById("markdown");
        
        // Assert
        markdown.Should().NotBeNull();
        markdown!.Id.Should().Be("markdown");
    }
    
    [Fact]
    public void GetById_WithInvalidId_ReturnsNull()
    {
        // Act
        var format = _sut.GetById("non-existent-format");
        
        // Assert
        format.Should().BeNull();
    }
    
    [Fact]
    public void AllFormats_HaveRequiredProperties_NotNullOrEmpty()
    {
        // Act
        var formats = _sut.GetAllFormats();
        
        // Assert
        foreach (var format in formats)
        {
            format.Id.Should().NotBeNullOrWhiteSpace($"Format has no ID");
            format.Name.Should().NotBeNullOrWhiteSpace($"Format {format.Id} has no Name");
            format.Description.Should().NotBeNullOrWhiteSpace($"Format {format.Id} has no Description");
            format.Pros.Should().NotBeEmpty($"Format {format.Id} has no Pros");
            format.Cons.Should().NotBeEmpty($"Format {format.Id} has no Cons");
            format.RecommendedFor.Should().NotBeNullOrWhiteSpace($"Format {format.Id} has no RecommendedFor");
        }
    }
    
    [Fact]
    public void AllFormats_HaveUniqueIds()
    {
        // Act
        var formats = _sut.GetAllFormats();
        var ids = formats.Select(f => f.Id).ToList();
        
        // Assert
        ids.Should().OnlyHaveUniqueItems("each format must have a unique ID");
    }
    
    [Fact]
    public void AllFormats_HaveAtLeast3Pros()
    {
        // Act
        var formats = _sut.GetAllFormats();
        
        // Assert
        foreach (var format in formats)
        {
            format.Pros.Count.Should().BeGreaterThanOrEqualTo(3, 
                $"Format {format.Name} should have at least 3 pros");
        }
    }
    
    [Fact]
    public void AllFormats_HaveAtLeast2Cons()
    {
        // Act
        var formats = _sut.GetAllFormats();
        
        // Assert
        foreach (var format in formats)
        {
            format.Cons.Count.Should().BeGreaterThanOrEqualTo(2, 
                $"Format {format.Name} should have at least 2 cons");
        }
    }
    
    [Fact]
    public void GetById_Markdown_HasExpectedProperties()
    {
        // Act
        var markdown = _sut.GetById("markdown");
        
        // Assert
        markdown.Should().NotBeNull();
        markdown!.Name.Should().Be("Markdown");
        markdown.Pros.Should().Contain(p => p.Contains("LLM") || p.Contains("natural"));
        markdown.Cons.Should().NotBeEmpty();
    }
    
    [Fact]
    public void GetById_XML_HasExpectedProperties()
    {
        // Act
        var xml = _sut.GetById("xml");
        
        // Assert
        xml.Should().NotBeNull();
        xml!.Name.Should().Be("XML");
        xml.Pros.Should().Contain(p => p.Contains("structured") || p.Contains("schema"));
    }
    
    [Fact]
    public void GetById_JSON_HasExpectedProperties()
    {
        // Act
        var json = _sut.GetById("json");
        
        // Assert
        json.Should().NotBeNull();
        json!.Name.Should().Be("JSON");
        json.Pros.Should().Contain(p => p.Contains("structured") || p.Contains("machine"));
    }
    
    [Fact]
    public void GetById_PlainText_HasExpectedProperties()
    {
        // Act
        var plainText = _sut.GetById("plaintext");
        
        // Assert
        plainText.Should().NotBeNull();
        plainText!.Name.Should().Be("Plain Text");
        plainText.Pros.Should().Contain(p => p.Contains("simple") || p.Contains("Simple"));
    }
}

