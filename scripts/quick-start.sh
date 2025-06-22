#!/bin/bash
# ContextKeeper Quick Start Script

set -e

echo "🚀 ContextKeeper Quick Start"
echo ""

# Check if contextkeeper is installed
if ! command -v contextkeeper &> /dev/null; then
    echo "❌ ContextKeeper not found. Installing..."
    curl -sSL https://raw.githubusercontent.com/chasecupp43/contextkeeper-mcp/main/scripts/install.sh | bash
    source ~/.bashrc 2>/dev/null || source ~/.zshrc 2>/dev/null || true
fi

# Initialize project
echo "📁 Initializing ContextKeeper in current directory..."
contextkeeper init

# Create first snapshot
echo ""
echo "📸 Let's create your first snapshot!"
echo "Enter a milestone description (kebab-case, e.g., 'initial-setup'):"
read -r milestone

if [ -n "$milestone" ]; then
    contextkeeper snapshot "$milestone"
    echo "✅ First snapshot created!"
fi

# Show next steps
echo ""
echo "🎉 You're all set! Here's what you can do next:"
echo ""
echo "  📸 Create snapshots:     contextkeeper snapshot <milestone-name>"
echo "  🔍 Search history:       contextkeeper search <term>"
echo "  📊 Check compaction:     contextkeeper check"
echo "  🔄 Track evolution:      contextkeeper evolution <component>"
echo "  📋 Compare snapshots:    contextkeeper compare <file1> <file2>"
echo ""
echo "For AI integration, add to Claude MCP:"
echo "  claude mcp add contextkeeper -- $(which contextkeeper)"
echo ""