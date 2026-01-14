#!/bin/bash
# Build ShieldPrompt for all platforms
# Usage: ./scripts/build-all-platforms.sh [version]

set -e  # Exit on error

VERSION=${1:-"1.0.0"}
OUTPUT_DIR="./publish"

echo "🚀 Building ShieldPrompt v${VERSION} for all platforms..."
echo ""

# Clean previous builds
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Run tests first
echo "📝 Running tests..."
dotnet test --configuration Release --verbosity quiet
TEST_RESULT=$?

if [ $TEST_RESULT -ne 0 ]; then
    echo "❌ Tests failed! Aborting build."
    exit 1
fi

echo "✅ All tests passed!"
echo ""

# Windows x64
echo "🪟 Building Windows x64..."
dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -p:Version="$VERSION" \
    -o "$OUTPUT_DIR/win-x64"

# Windows ARM64
echo "🪟 Building Windows ARM64..."
dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
    -c Release \
    -r win-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -p:Version="$VERSION" \
    -o "$OUTPUT_DIR/win-arm64"

# macOS ARM64 (Apple Silicon)
echo "🍎 Building macOS ARM64..."
dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
    -c Release \
    -r osx-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:Version="$VERSION" \
    -o "$OUTPUT_DIR/osx-arm64"

# macOS x64 (Intel)
echo "🍎 Building macOS x64..."
dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
    -c Release \
    -r osx-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:Version="$VERSION" \
    -o "$OUTPUT_DIR/osx-x64"

# Linux x64
echo "🐧 Building Linux x64..."
dotnet publish src/ShieldPrompt.App/ShieldPrompt.App.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:Version="$VERSION" \
    -o "$OUTPUT_DIR/linux-x64"

# Create portable archives
echo "📦 Creating portable archives..."

# Windows portable
cd "$OUTPUT_DIR/win-x64"
zip "../ShieldPrompt-${VERSION}-win-x64-portable.zip" ShieldPrompt.exe
cd ../..

# macOS portable (if on macOS)
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "Creating macOS universal binary..."
    lipo -create -output "$OUTPUT_DIR/ShieldPrompt-universal" \
        "$OUTPUT_DIR/osx-x64/ShieldPrompt" \
        "$OUTPUT_DIR/osx-arm64/ShieldPrompt"
    chmod +x "$OUTPUT_DIR/ShieldPrompt-universal"
fi

# Linux portable
cd "$OUTPUT_DIR/linux-x64"
tar -czf "../ShieldPrompt-${VERSION}-linux-x64.tar.gz" ShieldPrompt
cd ../..

# Generate checksums
echo "🔐 Generating checksums..."
cd "$OUTPUT_DIR"
find . -type f \( -name "*.exe" -o -name "*.zip" -o -name "*.tar.gz" \) -exec sha256sum {} \; > SHA256SUMS
cd ..

echo ""
echo "✅ Build complete!"
echo ""
echo "📦 Artifacts in $OUTPUT_DIR:"
ls -lh "$OUTPUT_DIR" | grep -E "\.(exe|zip|tar.gz)$|SHA256"
echo ""
echo "🎉 Ready for distribution!"

