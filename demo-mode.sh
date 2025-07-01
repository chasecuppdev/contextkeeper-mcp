#!/bin/bash
# Enter demo mode for ContextKeeper
echo "🎬 Activating ContextKeeper Demo Mode..."

# Set environment variables
export CONTEXTKEEPER_DEMO_MODE=true
export CONTEXTKEEPER_HISTORY_PATH=.contextkeeper-demo
export CONTEXTKEEPER_DEMO_CONTEXT_SIZE=50

# Create demo directory with minimal history
mkdir -p .contextkeeper-demo/snapshots

# Copy a few small snapshots for demo (if they exist)
if [ -f ".contextkeeper/snapshots/SNAPSHOT_2025-06-24_manual_phase2-context-capture.md" ]; then
    cp .contextkeeper/snapshots/SNAPSHOT_2025-06-24_manual_phase2-context-capture.md .contextkeeper-demo/snapshots/ 2>/dev/null || true
    echo "✓ Copied initial snapshot for demo"
fi

if [ -f ".contextkeeper/snapshots/SNAPSHOT_2025-06-24_manual_phase3-complete-autocompaction-mcp.md" ]; then
    cp .contextkeeper/snapshots/SNAPSHOT_2025-06-24_manual_phase3-complete-autocompaction-mcp.md .contextkeeper-demo/snapshots/ 2>/dev/null || true
    echo "✓ Copied MCP integration snapshot for demo"
fi

echo ""
echo "✅ Demo mode activated!"
echo "📁 History will be saved to: .contextkeeper-demo/"
echo "🔍 Search results will be truncated for performance"
echo ""
echo "To deactivate demo mode, run: source demo-cleanup.sh"
echo ""
echo "🚀 You can now run the demo safely without affecting your real history!"