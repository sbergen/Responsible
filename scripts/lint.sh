set -e

dotnet tool restore
dotnet jb inspectcode -o=inspect.xml $1