#!/bin/bash

set -e
cd "$(dirname "${BASH_SOURCE[0]}")"

dotnet jb inspectcode -a -s=WARNING -o=inspect.xml Responsible.sln

# grep -q is not available on MacOS
# shellcheck disable=SC2143
if [[ $(grep "<Issue " inspect.xml) ]]; then
    echo "::error ::Found issues with code style :("
    exit 1
else
    echo "No issues found \o/"
fi
