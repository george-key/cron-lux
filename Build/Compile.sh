#!/usr/bin/env bash
set -e

msbuild /t:Restore Neo.Lux.sln
msbuild /p:Configuration=Release Neo.Lux.sln
