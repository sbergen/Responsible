#!/bin/bash

# Intended for running test inside the GameCI docker image
# assuming that repository is mounted at /project
# and a license file is at /project/unity-licesnse

mkdir -p /project/TestResults
unity-editor -batchmode -logFile /dev/stdout -manualLicenseFile /project/unity-license
unity-editor -batchmode -nographics -disable-assembly-updater -enableCodeCoverage -debugCodeOptimization -executeMethod Responsible.EditorSetup.TestRunner.RunTests -logFile /dev/stdout -projectPath /project/ResponsibleUnity -coverageResultsPath /project/TestResults -coverageOptions assemblyFilters:+Responsible,+Responsible.Editor
