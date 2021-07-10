#!/bin/bash

ROOT="../../.."
GRGEN_BIN="$ROOT/ExternalTools/grgen/engine-net-2/bin"
GRGEN="$GRGEN_BIN/GrGen"

TARGET_DIR="$ROOT/SynergyQuest/Assets/Plugins"

cd src
$GRGEN RewriteRules.grg

cp *.dll "$TARGET_DIR"
cp "$GRGEN_BIN/libGr.dll" "$TARGET_DIR"
cp "$GRGEN_BIN/lgspBackend.dll" "$TARGET_DIR"
