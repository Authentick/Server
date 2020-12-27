#!/bin/sh

npm install
dotnet test --collect:"XPlat Code Coverage" -r ../TestResults/ &
