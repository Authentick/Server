FROM mcr.microsoft.com/dotnet/sdk:5.0

# GPG
RUN apt-get update
RUN apt-get install -y gnupg

# VIM
RUN apt-get install -y vim

# NodeJS
RUN curl -sL https://deb.nodesource.com/gpgkey/nodesource.gpg.key | apt-key add - 2>/dev/null
RUN echo "deb https://deb.nodesource.com/node_13.x buster main" | tee /etc/apt/sources.list.d/nodesource.list
RUN echo "deb-src https://deb.nodesource.com/node_13.x buster main" | tee -a /etc/apt/sources.list.d/nodesource.list
RUN apt-get update -y
RUN apt-get install -y nodejs

# Gulp
RUN npm install --global gulp-cli

# .NET
RUN dotnet tool install --global dotnet-ef --version 5.0.0-rc.2.20475.6
RUN echo "export PATH=\"$PATH:/root/.dotnet/tools\"" >> ~/.bash_profile

# Networking tools
RUN apt-get install -y iputils-ping dnsutils telnet netcat

# Docker
RUN apt-get -y install \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg-agent \
    software-properties-common
RUN curl -fsSL https://download.docker.com/linux/debian/gpg | apt-key add -
RUN add-apt-repository \
   "deb [arch=amd64] https://download.docker.com/linux/debian \
   $(lsb_release -cs) \
   stable"
RUN apt-get update
RUN apt-get install -y docker-ce docker-ce-cli containerd.io
RUN ln -s /var/run/docker-host.sock /var/run/docker.sock 
