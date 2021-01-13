#!/bin/sh

nohup sudo dotnet run --project ../Gatekeeper.Server.Web/Gatekeeper.Server.Web.csproj &
while ! nc -z localhost 80; do   
  sleep 0.1
done
npm install
npx percy exec -- node snapshots.js
