#!/usr/bin/env bash

set -u

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARTIFACTS_DIR="$ROOT_DIR/artifacts"
STAMP_FILE="$(mktemp)"
DOTNET="${DOTNET:-dotnet}"

cleanup() {
	rm -f "$STAMP_FILE"
}

trap cleanup EXIT

if ! command -v "$DOTNET" >/dev/null 2>&1; then
	if [[ -x "/c/Program Files/dotnet/dotnet.exe" ]]; then
		DOTNET="/c/Program Files/dotnet/dotnet.exe"
	else
		echo "Erreur : dotnet SDK est introuvable. Ajoutez-le au PATH ou définissez DOTNET."
		exit 1
	fi
fi

build_and_pack() {
	echo "🔨 Build de la solution..."
	if ! "$DOTNET" build "$ROOT_DIR/easy-peasy.slnx" -c Release; then
		echo "❌ Le build a échoué. Surveillance maintenue."
		return
	fi

	echo "📦 Pack de la solution..."
	rm -rf "$ARTIFACTS_DIR"
	mkdir -p "$ARTIFACTS_DIR"

	if ! "$DOTNET" pack "$ROOT_DIR/easy-peasy.slnx" -c Release --no-build -o "$ARTIFACTS_DIR"; then
		echo "❌ Le packaging a échoué. Surveillance maintenue."
		return
	fi

	echo "✅ Packages générés :"
	find "$ARTIFACTS_DIR" -type f \( -name "*.nupkg" -o -name "*.snupkg" \) -print
}

has_changes() {
	find "$ROOT_DIR" \
		\( -path "$ROOT_DIR/.git" -o -path "$ROOT_DIR/artifacts" -o -path '*/bin' -o -path '*/obj' \) -prune -o \
		-type f \
		\( -name '*.cs' -o -name '*.csproj' -o -name '*.props' -o -name '*.targets' -o -name '*.slnx' -o -name '*.json' -o -name '*.md' -o -name '*.png' -o -name '*.sh' \) \
		-newer "$STAMP_FILE" -print -quit | grep -q .
}

echo "👀 Surveillance de $ROOT_DIR (Ctrl+C pour arrêter)"
build_and_pack
touch "$STAMP_FILE"

while true; do
	if has_changes; then
		echo "📝 Modification détectée. Relance du build et du package..."
		build_and_pack
		touch "$STAMP_FILE"
	fi

	sleep 2
done
