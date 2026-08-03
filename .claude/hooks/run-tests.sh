#!/usr/bin/env bash
# Stop: Claude cevabını bitirmeden önce unit testleri koştur.
# Kırmızıysa exit 2 → Claude durmaz, düzeltmeye devam eder.
set -uo pipefail
cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0
[ -d tests ] || exit 0

# Unit test projesi adı proje adından bağımsız olarak son ekten bulunur.
UNIT_DIR=$(ls -d tests/*UnitTests 2>/dev/null | head -1)
[ -n "$UNIT_DIR" ] || exit 0

OUT=$(dotnet test "$UNIT_DIR" --nologo -v q 2>&1)
if [ $? -ne 0 ]; then
  echo "UNIT TESTLER KIRMIZI — bitirmeden önce düzelt:" >&2
  echo "$OUT" | tail -40 >&2
  exit 2
fi
echo "unit tests: yeşil"
exit 0
