#!/bin/sh

# Define log file names
LOG1="/myapp/log01.txt"
LOG2="/myapp/log02.txt"

cd /myapp

# Check if log01.txt exists and is larger than 1MB
if [ -f "$LOG1" ] && [ "$(stat -c%s "$LOG1")" -gt 1048576 ]; then
    mv -f "$LOG1" "$LOG2"
fi

echo "Start $(date '+%Y-%m-%d %H:%M:%S')" >> "$LOG1"

# Upgrade all packages via apk
apk update >> "$LOG1" 2>&1
apk upgrade --no-cache >> "$LOG1" 2>&1

# Run crawler script
echo "pwsh script:" >> "$LOG1"
export __SuppressAnsiEscapeSequences=1
export NO_COLOR=1
pwsh -Command "&{ \$PSStyle.OutputRendering = 'PlainText'; \$ErrorView = 'NormalView'; /myapp/SummaryCrawler.ps1 }" -WorkingDirectory /myapp -NoLogo -NonInteractive -NoProfile >> "$LOG1" 2>&1

echo "done." >> "$LOG1"
