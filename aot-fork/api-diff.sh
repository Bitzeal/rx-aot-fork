#!/bin/sh
# Prints the difference between the public API of two System.Reactive.dll files; exit 1 if they differ.
# Usage: api-diff.sh <upstream System.Reactive.dll> <fork System.Reactive.dll>
set -eu
here="$(cd "$(dirname "$0")" && pwd)"
out="$(mktemp -d)"
dotnet run -c Release --project "$here/api-diff" -- "$1" > "$out/upstream.cs"
dotnet run -c Release --project "$here/api-diff" -- "$2" > "$out/fork.cs"
echo "api-diff: $(wc -l < "$out/upstream.cs") lines of public API"
diff -u "$out/upstream.cs" "$out/fork.cs" && echo "api-diff: identical"
