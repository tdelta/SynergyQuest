#!/bin/bash

if ! which msbuild 2>/dev/null; then
  echo "You need to install msbuild."
  exit 127
fi

git submodule init
git submodule update

cd grgen

cd frontend
make
cd ..

cd engine-net-2

cd src/libGr
bash genparser.sh
cd ../..

cd src/GrShell
bash genparser.sh
cd ../..

msbuild GrGen.sln -t:GrShell,GrGen,lgspBackend,libGr -p:Configuration=Release

cd ..
