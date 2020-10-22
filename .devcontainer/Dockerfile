FROM mcr.microsoft.com/dotnet/sdk:5.0

# GPG
RUN apt-get update
RUN apt-get install -y gnupg

# NodeJS
RUN curl -sL https://deb.nodesource.com/gpgkey/nodesource.gpg.key | apt-key add - 2>/dev/null
RUN echo "deb https://deb.nodesource.com/node_13.x buster main" | tee /etc/apt/sources.list.d/nodesource.list
RUN echo "deb-src https://deb.nodesource.com/node_13.x buster main" | tee -a /etc/apt/sources.list.d/nodesource.list
RUN apt-get update -y
RUN apt-get install -y nodejs

# Gulp
RUN npm install --global gulp-cli
