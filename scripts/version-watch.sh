#!/usr/bin/env bash

set -e

ARTIFACTS_DIR="./artifacts"

echo " Nettoyage de $ARTIFACTS_DIR..."
rm -rf "$ARTIFACTS_DIR"
mkdir -p "$ARTIFACTS_DIR"

echo "🔨 Build de la solution..."
dotnet build -c Release

echo "📦 Pack de la solution..."
dotnet pack -c Release --no-build -o "$ARTIFACTS_DIR"

echo ""
echo "✅ Terminé !"
echo "📦 Packages générés :"

find "$ARTIFACTS_DIR" -type f \( -name "*.nupkg" -o -name "*.snupkg" \) -print
