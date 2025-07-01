#!/bin/bash
# Exit demo mode for ContextKeeper
echo "🎬 Deactivating ContextKeeper Demo Mode..."

# Unset environment variables
unset CONTEXTKEEPER_DEMO_MODE
unset CONTEXTKEEPER_HISTORY_PATH
unset CONTEXTKEEPER_DEMO_CONTEXT_SIZE

# Optional: Remove demo data
echo ""
read -p "❓ Remove demo data from .contextkeeper-demo? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    rm -rf .contextkeeper-demo
    echo "✓ Demo data removed"
else
    echo "ℹ️  Demo data preserved in .contextkeeper-demo/"
fi

echo ""
echo "✅ Demo mode deactivated!"
echo "📁 Your real history in .contextkeeper/ is untouched"
echo ""
echo "Thank you for demoing ContextKeeper!"