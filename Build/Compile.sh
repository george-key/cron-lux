#!/usr/bin/env bash
set -e

msbuild /t:Restore /p:TargetFrameworkVersion="v4.6.1" Neo.Lux.sln
msbuild /p:Configuration=Release /p:TargetFrameworkVersion="v4.6.1" Neo.Lux.sln
