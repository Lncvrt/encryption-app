#!/bin/bash
set -e

cfg=Release
tfm=net9.0
outdir=bin/$cfg
zipdir=$(pwd)/builds
rids=(
  linux-x64 linux-arm linux-arm64 linux-musl-x64 linux-musl-arm64
  win-x86 win-x64 win-arm64
  osx-x64 osx-arm64
)

projfile=$(ls *.csproj | head -1)
if [[ -z "$projfile" ]]; then
  echo "No .csproj file found, aborting."
  exit 1
fi

version=$(grep -oPm1 "(?<=<Version>)[^<]+" "$projfile")
if [[ -z "$version" ]]; then
  version="0.0.0"
fi

rm -rf "$zipdir"
mkdir -p "$zipdir"

name=$(basename "$projfile" .csproj)

for rid in "${rids[@]}"; do
  dotnet publish "$projfile" -c $cfg -r $rid --self-contained false -o "$outdir/$rid/publish"
  7z a -tzip "$zipdir/${name}-${version}-${rid}.zip" "$outdir/$rid/publish/*"
done
