# Changelog

All notable changes to ContextKeeper will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial release of ContextKeeper
- Core snapshot management functionality
- Search engine for history exploration
- Evolution tracking for components
- Compaction engine with LSM-tree inspired approach
- Multi-profile support with auto-detection
- CLAUDE.md workflow as default profile
- README and custom workflow profiles
- Native MCP server implementation
- CLI with intuitive commands
- Installation scripts for Linux, macOS, and Windows
- Comprehensive documentation
- Native AOT compilation for fast startup

### Technical Details
- Built with .NET 9
- Binary size: ~5.6MB
- Native AOT enabled
- Model Context Protocol compatible

## [1.0.0] - 2024-01-XX (Planned)

First stable release.

---

## Development History

### Origin Story
ContextKeeper was extracted from the CodeCartographerAI project where it proved invaluable for maintaining development context across AI sessions. The CLAUDE.md workflow pattern emerged from real-world needs and has been battle-tested through months of development.

### Key Milestones
- **2024-01-15** - Initial concept in CodeCartographerAI
- **2024-01-20** - MCP server implementation
- **2024-01-22** - Extraction as standalone tool
- **2024-01-XX** - First public release

### Acknowledgments
- Inspired by LSM-tree data structures
- Built for the Claude MCP ecosystem
- Thanks to the CodeCartographerAI project for proving the concept