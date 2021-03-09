#!/bin/bash

cd "$( dirname "${BASH_SOURCE[0]}" )"
mkdir -p bin
curl https://github.com/dotnet/docfx/releases/download/v2.56.7/docfx.zip -L --output bin/docfx.zip
cd bin
unzip docfx.zip
