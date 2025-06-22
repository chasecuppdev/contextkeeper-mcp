# Contributing to ContextKeeper

Thank you for your interest in contributing to ContextKeeper! This document provides guidelines and instructions for contributing.

## Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct:
- Be respectful and inclusive
- Welcome newcomers and help them get started
- Focus on constructive criticism
- Respect differing opinions and experiences

## How to Contribute

### Reporting Issues

1. **Check existing issues** - Ensure the issue hasn't already been reported
2. **Use issue templates** - Select the appropriate template (bug, feature, etc.)
3. **Provide details** - Include:
   - ContextKeeper version
   - Operating system
   - Steps to reproduce
   - Expected vs actual behavior
   - Error messages or logs

### Suggesting Features

1. **Check the roadmap** - See if it's already planned
2. **Open a discussion** - Start with a GitHub Discussion
3. **Provide use cases** - Explain why the feature would be valuable
4. **Consider implementation** - Suggest how it might work

### Contributing Code

#### Setup Development Environment

```bash
# Fork and clone the repository
git clone https://github.com/YOUR_USERNAME/contextkeeper-mcp.git
cd contextkeeper-mcp

# Create a feature branch
git checkout -b feature/your-feature-name

# Install .NET 9 SDK
# See https://dotnet.microsoft.com/download

# Build the project
dotnet build

# Run tests
dotnet test
```

#### Development Workflow

1. **Create a feature branch** - Use descriptive names like `feature/add-timeline-view`
2. **Write tests first** - TDD is encouraged
3. **Implement your feature** - Follow existing patterns
4. **Run all tests** - Ensure nothing is broken
5. **Update documentation** - Include relevant docs
6. **Submit a pull request** - Use the PR template

#### Code Style

Follow these C# coding conventions:

```csharp
// Use PascalCase for public members
public class SnapshotManager
{
    // Use camelCase for private fields
    private readonly ILogger<SnapshotManager> _logger;
    
    // Use async/await properly
    public async Task<SnapshotResult> CreateSnapshotAsync(string milestone)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(milestone);
        
        // Use meaningful variable names
        var validationResult = ValidateMilestone(milestone);
        
        // Handle errors gracefully
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid milestone: {Milestone}", milestone);
            return new SnapshotResult { Success = false };
        }
        
        // Document complex logic
        // Create snapshot using LSM-tree inspired approach
        return await CreateSnapshotInternalAsync(milestone);
    }
}
```

#### Testing Guidelines

Write tests for:
- All public methods
- Edge cases and error conditions
- Integration scenarios

Example test:

```csharp
[Fact]
public async Task CreateSnapshot_WithValidMilestone_CreatesSnapshot()
{
    // Arrange
    var manager = new SnapshotManager(_mockLogger.Object);
    var milestone = "test-feature";
    
    // Act
    var result = await manager.CreateSnapshotAsync(milestone);
    
    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.SnapshotPath);
    Assert.Contains(milestone, result.SnapshotPath);
}
```

### Documentation

#### When to Update Docs

Update documentation when you:
- Add new features
- Change existing behavior
- Add configuration options
- Fix documentation errors

#### Documentation Standards

- Use clear, concise language
- Include code examples
- Add diagrams where helpful
- Keep README focused on users
- Put technical details in /docs

### Pull Request Process

1. **Update your branch** - Rebase on main if needed
2. **Run all tests** - `dotnet test`
3. **Update CHANGELOG** - Add your changes
4. **Create PR** - Use the template
5. **Address feedback** - Respond to review comments
6. **Squash commits** - Keep history clean

#### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] CHANGELOG updated
```

## Development Guidelines

### Architecture Principles

1. **Modularity** - Keep components focused and independent
2. **Testability** - Design for easy testing
3. **Extensibility** - Make it easy to add new features
4. **Performance** - Consider performance implications
5. **Security** - No network operations, validate all inputs

### Adding New Features

#### New Workflow Profile

1. Create profile JSON in `/profiles`
2. Add detection logic if needed
3. Document in CONFIGURATION.md
4. Add tests for profile loading

#### New Tool/Command

1. Add method to `ContextKeeperService`
2. Add CLI command in `Program.cs`
3. Register in MCP protocol handler
4. Update tool list documentation
5. Add comprehensive tests

#### New Compaction Strategy

1. Extend `CompactionEngine`
2. Add strategy selection logic
3. Document strategy behavior
4. Test with various scenarios

## Release Process

### Version Numbering

We use semantic versioning: `MAJOR.MINOR.PATCH`

- **MAJOR** - Breaking changes
- **MINOR** - New features, backward compatible
- **PATCH** - Bug fixes

### Release Checklist

1. Update version in `.csproj`
2. Update CHANGELOG.md
3. Run full test suite
4. Build release binaries
5. Create GitHub release
6. Update installation scripts

## Getting Help

### Resources

- **Documentation** - Start with /docs
- **Discussions** - Ask questions on GitHub
- **Issues** - Report bugs or request features
- **Discord** - Join our community (coming soon)

### Maintainer Response Time

- **Issues** - Within 48 hours
- **PRs** - Within 72 hours
- **Security** - Within 24 hours

## Recognition

Contributors are recognized in:
- CHANGELOG.md
- GitHub contributors page
- Release notes
- Special thanks in README

Thank you for contributing to ContextKeeper!