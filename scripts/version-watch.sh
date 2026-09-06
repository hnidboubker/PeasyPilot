#!/usr/bin/env bash
set -o pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARTIFACTS_DIR="$ROOT_DIR/artifacts"
SOLUTION_FILE="$ROOT_DIR/easy-peasy.slnx"
STAMP_FILE="$(mktemp)"
DOTNET="${DOTNET:-dotnet}"
DEBOUNCE_SECONDS=1

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

cleanup() {
    echo -e "${CYAN}[CLEANUP] Cleaning up...${NC}"
    rm -f "$STAMP_FILE" 2>/dev/null || true
    echo -e "${CYAN}[CLEANUP] Done.${NC}"
}

trap cleanup EXIT

# Find dotnet
if ! command -v "$DOTNET" >/dev/null 2>&1; then
    if [[ -x "/c/Program Files/dotnet/dotnet.exe" ]]; then
        DOTNET="/c/Program Files/dotnet/dotnet.exe"
    elif [[ -x "$HOME/.dotnet/dotnet" ]]; then
        DOTNET="$HOME/.dotnet/dotnet"
    else
        echo -e "${RED}[ERROR] dotnet SDK not found. Add it to PATH or set DOTNET env var.${NC}" >&2
        exit 1
    fi
fi

echo -e "${CYAN}[INFO] dotnet: $DOTNET${NC}"
echo -e "${CYAN}[INFO] solution: $SOLUTION_FILE${NC}"
echo -e "${CYAN}[INFO] artifacts: $ARTIFACTS_DIR${NC}"
echo ""

build_and_pack() {
    local start_time=$(date +%s%N)

    echo -e "\n${GREEN}[BUILD] Starting build and pack...${NC}"
    echo -e "${CYAN}[BUILD] Executing: $DOTNET build \"$SOLUTION_FILE\" -c Release${NC}"

    if ! "$DOTNET" build "$SOLUTION_FILE" -c Release; then
        echo -e "${RED}[ERROR] Build failed${NC}"
        echo -e "${YELLOW}[INFO] Watching for changes...${NC}"
        return 1
    fi

    echo -e "${CYAN}[CLEAN] Removing existing artifacts...${NC}"
    rm -rf "$ARTIFACTS_DIR" 2>/dev/null || true
    mkdir -p "$ARTIFACTS_DIR"

    echo -e "${CYAN}[PACK] Executing: $DOTNET pack \"$SOLUTION_FILE\" -c Release -o \"$ARTIFACTS_DIR\"${NC}"

    if ! "$DOTNET" pack "$SOLUTION_FILE" -c Release -o "$ARTIFACTS_DIR" --no-build; then
        echo -e "${RED}[ERROR] Pack failed${NC}"
        echo -e "${YELLOW}[INFO] Watching for changes...${NC}"
        return 1
    fi

    # List packages
    echo -e "\n${GREEN}[SUCCESS] Packages created:${NC}"
    find "$ARTIFACTS_DIR" -type f \( -name "*.nupkg" -o -name "*.snupkg" \) | while read -r pkg; do
        size=$(du -h "$pkg" | cut -f1)
        echo -e "${GREEN}  ✓ $(basename "$pkg") ($size)${NC}"
    done

    local end_time=$(date +%s%N)
    local duration=$(( (end_time - start_time) / 1000000 ))
    echo -e "\n${GREEN}[SUCCESS] Build and pack completed in ${duration}ms${NC}"
    echo -e "${YELLOW}[INFO] Watching for changes...${NC}"

    return 0
}

has_changes() {
    find "$ROOT_DIR" \
        \( -path "$ROOT_DIR/.git" -o \
           -path "$ROOT_DIR/.vs" -o \
           -path "$ROOT_DIR/artifacts" -o \
           -path '*/bin' -o \
           -path '*/obj' -o \
           -path '*/.idea' -o \
           -path '*/packages' \) -prune -o \
        -type f \
        \( -name '*.cs' -o -name '*.csproj' -o -name '*.props' -o \
           -name '*.targets' -o -name '*.slnx' -o -name '*.json' -o \
           -name '*.md' -o -name '*.png' -o -name '*.sh' -o -name '*.ps1' \) \
        -newer "$STAMP_FILE" -print -quit | grep -q . 2>/dev/null || false
}

echo -e "${GREEN}[WATCH] Monitoring $ROOT_DIR (press Ctrl+C to stop)${NC}\n"

# Initial build
build_and_pack
touch "$STAMP_FILE"

while true; do
    if has_changes; then
        # Debounce: wait for changes to settle
        sleep "$DEBOUNCE_SECONDS"

        if has_changes; then
            echo -e "\n${YELLOW}[WATCH] Change detected, rebuilding...${NC}"
            build_and_pack
            touch "$STAMP_FILE"
        fi
    fi

    sleep 0.5
done
