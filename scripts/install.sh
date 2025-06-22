#!/bin/bash
# ContextKeeper Installation Script

set -e

INSTALL_DIR="${CONTEXTKEEPER_INSTALL_DIR:-$HOME/.contextkeeper}"
RELEASE_URL="https://github.com/chasecupp43/contextkeeper-mcp/releases/latest"
BIN_NAME="contextkeeper"

echo "🚀 Installing ContextKeeper..."
echo "Installation directory: $INSTALL_DIR"

# Create installation directory
mkdir -p "$INSTALL_DIR"

# Detect OS and architecture
OS=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)

case "$ARCH" in
    x86_64) ARCH="x64" ;;
    aarch64|arm64) ARCH="arm64" ;;
    *) echo "❌ Unsupported architecture: $ARCH"; exit 1 ;;
esac

case "$OS" in
    linux) ;;
    darwin) OS="osx" ;;
    *) echo "❌ Unsupported OS: $OS"; exit 1 ;;
esac

BINARY_NAME="${BIN_NAME}-${OS}-${ARCH}"

# Download latest release
echo "📥 Downloading ContextKeeper for $OS-$ARCH..."
DOWNLOAD_URL="${RELEASE_URL}/download/${BINARY_NAME}.tar.gz"

if command -v wget >/dev/null 2>&1; then
    wget -qO- "$DOWNLOAD_URL" | tar -xz -C "$INSTALL_DIR"
elif command -v curl >/dev/null 2>&1; then
    curl -sL "$DOWNLOAD_URL" | tar -xz -C "$INSTALL_DIR"
else
    echo "❌ Neither wget nor curl found. Please install one of them."
    exit 1
fi

# Make binary executable
chmod +x "$INSTALL_DIR/$BIN_NAME"

# Add to PATH
SHELL_CONFIG=""
if [ -n "$BASH_VERSION" ]; then
    SHELL_CONFIG="$HOME/.bashrc"
elif [ -n "$ZSH_VERSION" ]; then
    SHELL_CONFIG="$HOME/.zshrc"
else
    SHELL_CONFIG="$HOME/.profile"
fi

PATH_LINE="export PATH=\"\$HOME/.contextkeeper:\$PATH\""
if ! grep -q "contextkeeper" "$SHELL_CONFIG" 2>/dev/null; then
    echo "" >> "$SHELL_CONFIG"
    echo "# ContextKeeper" >> "$SHELL_CONFIG"
    echo "$PATH_LINE" >> "$SHELL_CONFIG"
    echo "✅ Added ContextKeeper to PATH in $SHELL_CONFIG"
else
    echo "ℹ️  ContextKeeper already in PATH"
fi

# Initialize in current directory if it looks like a project
if [ -f "CLAUDE.md" ] || [ -f "README.md" ] || [ -d "docs" ]; then
    echo ""
    echo "📁 Detected project in current directory"
    read -p "Initialize ContextKeeper here? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        "$INSTALL_DIR/$BIN_NAME" init
    fi
fi

echo ""
echo "✅ ContextKeeper installed successfully!"
echo ""
echo "To get started:"
echo "  1. Reload your shell: source $SHELL_CONFIG"
echo "  2. Run: contextkeeper --help"
echo "  3. Initialize a project: contextkeeper init"
echo ""
echo "For MCP integration with Claude:"
echo "  claude mcp add contextkeeper -- $INSTALL_DIR/$BIN_NAME"
echo ""