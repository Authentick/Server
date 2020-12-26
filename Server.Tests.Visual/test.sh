#!/bin/sh

nohup dotnet run --project ../Server/AuthServer.Server.csproj &
while ! nc -z localhost 80; do   
  sleep 0.1
done
npm install
npx percy exec -- node snapshots.js
