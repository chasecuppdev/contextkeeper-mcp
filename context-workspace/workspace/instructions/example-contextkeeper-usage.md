# ContextKeeper Usage Instructions

## For This Session

When working with ContextKeeper, please follow these guidelines:

### Snapshot Creation
- Create snapshots for significant milestones only
- Use descriptive milestone names (e.g., "auth-implementation", not "update")
- Snapshots are automatic on git commits - no need to create manual ones for every change

### Code Style
- Follow existing patterns in the codebase
- Maintain Native AOT compatibility (no dynamic code generation)
- Use source-generated JSON serialization
- Keep async/await patterns consistent

### Testing
- Run tests after significant changes
- Ensure new features have corresponding tests
- Use TestBase for proper test isolation

### Documentation
- Update CLAUDE.md for major architectural decisions
- Keep this workspace updated with current requirements
- Document breaking changes clearly

## Current Focus

Working on v2.0 features:
- [x] Comprehensive context capture
- [x] Auto-compaction
- [x] Enhanced MCP tools
- [x] User workspace
- [ ] Visual timeline interface (next priority)

## Notes

- The .contextkeeper directory contains automatic snapshots - don't modify
- This workspace directory is for manual documentation only
- All workspace files are included in snapshots automatically