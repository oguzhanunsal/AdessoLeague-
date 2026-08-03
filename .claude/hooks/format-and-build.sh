#!/usr/bin/env bash
# PostToolUse: her C# düzenlemesinden sonra formatla + derle.
# Derleme kırıldıysa stderr'e yaz + exit 2 → Claude hatayı görüp kendi düzeltir.
set -uo pipefail

INPUT=$(cat)
FILE=$(printf '%s' "$INPUT" | jq -r '.tool_input.file_path // empty')

case "$FILE" in
  *.cs) ;;
  *) exit 0 ;;
esac

cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0
[ -n "$(ls -1 ./*.sln 2>/dev/null)" ] || exit 0

dotnet format --include "$FILE" --verbosity quiet >/dev/null 2>&1

BUILD_OUT=$(dotnet build --nologo -clp:NoSummary -v q 2>&1)
if [ $? -ne 0 ]; then
  echo "BUILD FAILED — düzeltmeden devam etme:" >&2
  echo "$BUILD_OUT" | grep -E "error|warning as error" | head -30 >&2
  exit 2
fi
exit 0
