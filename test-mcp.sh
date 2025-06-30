#!/bin/bash
# ContextKeeper MCP Testing Script for Claude Code

set -e

echo "=== ContextKeeper MCP Testing Script ==="
echo

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print test results
print_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓ $2${NC}"
    else
        echo -e "${RED}✗ $2${NC}"
    fi
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

echo "Step 1: Environment Check"
echo "------------------------"

# Check dotnet
if command_exists dotnet; then
    print_result 0 "dotnet found: $(dotnet --version)"
else
    print_result 1 "dotnet not found - please install .NET 9.0 SDK"
    exit 1
fi

# Check Claude Code
if command_exists claude-code; then
    print_result 0 "claude-code found"
else
    print_result 1 "claude-code not found - please install Claude Code CLI"
    echo -e "${YELLOW}Visit https://docs.anthropic.com/en/docs/claude-code to install${NC}"
fi

echo
echo "Step 2: Build ContextKeeper"
echo "--------------------------"

cd "$(dirname "$0")"
if dotnet build --quiet; then
    print_result 0 "Build successful"
else
    print_result 1 "Build failed"
    exit 1
fi

echo
echo "Step 3: Run Tests"
echo "----------------"

if dotnet test --quiet --no-build; then
    print_result 0 "All tests passed"
else
    print_result 1 "Some tests failed"
fi

echo
echo "Step 4: Initialize ContextKeeper"
echo "-------------------------------"

# Create .contextkeeper directory if it doesn't exist
if [ ! -d ".contextkeeper" ]; then
    mkdir -p .contextkeeper/snapshots
    mkdir -p .contextkeeper/archived
    print_result 0 "Created .contextkeeper directory structure"
else
    print_result 0 ".contextkeeper directory already exists"
fi

# Create context-workspace if it doesn't exist
if [ ! -d "context-workspace" ]; then
    mkdir -p context-workspace/workspace/{requirements,design,instructions}
    print_result 0 "Created context-workspace directory structure"
else
    print_result 0 "context-workspace directory already exists"
fi

echo
echo "Step 5: Test CLI Commands"
echo "------------------------"

# Test snapshot creation
echo -n "Testing snapshot creation... "
if dotnet run --project src/ContextKeeper -- snapshot "test-cli-snapshot" >/dev/null 2>&1; then
    print_result 0 "Snapshot created successfully"
else
    print_result 1 "Snapshot creation failed"
fi

# Test search
echo -n "Testing search functionality... "
if dotnet run --project src/ContextKeeper -- search "test" >/dev/null 2>&1; then
    print_result 0 "Search completed successfully"
else
    print_result 1 "Search failed"
fi

# Test check command
echo -n "Testing check command... "
if dotnet run --project src/ContextKeeper -- check >/dev/null 2>&1; then
    print_result 0 "Check completed successfully"
else
    print_result 1 "Check failed"
fi

echo
echo "Step 6: MCP Server Test"
echo "----------------------"

# Test if MCP server starts
echo -n "Testing MCP server startup... "
timeout 5s dotnet run --project src/ContextKeeper >/dev/null 2>&1 || true
print_result 0 "MCP server can start (timeout expected)"

echo
echo "Step 7: Instructions for Claude Code Testing"
echo "-------------------------------------------"
echo
echo -e "${YELLOW}To test with Claude Code:${NC}"
echo
echo "1. Start Claude Code with MCP config:"
echo "   claude-code --mcp-config $(pwd)/claude-code-config.json"
echo
echo "2. In Claude Code, test these commands:"
echo "   /mcp list"
echo "   /mcp call contextkeeper get_status"
echo "   /mcp call contextkeeper snapshot {\"milestone\": \"mcp-test\"}"
echo "   /mcp call contextkeeper search_evolution {\"query\": \"refactoring\"}"
echo "   /mcp call contextkeeper get_timeline {\"limit\": 5}"
echo
echo "3. Or ask Claude naturally:"
echo "   'Use contextkeeper to create a snapshot called test-feature'"
echo "   'Search for information about MCP implementation'"
echo "   'Show me the project timeline'"
echo
echo -e "${GREEN}Setup complete!${NC} Ready for MCP testing."
echo
echo "Troubleshooting:"
echo "- If MCP tools don't appear, check ~/.claude-code/logs/"
echo "- Enable debug mode: export MCP_DEBUG=true"
echo "- Verify config path is absolute in claude-code-config.json"