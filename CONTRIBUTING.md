# Contributing to Gatekeeper

Thanks for contributing to Gatekeeper :star:. 
This document will provide you the first pointers on how to get started.

#### Table Of Contents

[**How Can I Contribute?**](#how-can-i-contribute)
  * [Reporting Bugs](#reporting-bugs)
  * [Suggesting Enhancements](#suggesting-enhancements)
  * [Code Contributions](#code-contributions)

[**Development Environment**](#development-environment)
  * [Setup](#setup)
     * [Locally](#locally)
     * [GitHub Codespaces](#github-codespaces)
  * [Services](#services)
     * [Web UIs](#web-uis)
     * [Backend](#backend)
  * [Build and run Gatekeeper](#build-and-run-gatekeeper)
     * [Web Server](#web-server)
     * [Build SCSS](#build-scss)
     * [Run tests locally](#run-tests-locally)

# How Can I Contribute?

## Reporting Bugs

Open an issue at https://github.com/GetGatekeeper/Server/issues for any issues you encounter.

## Suggesting Enhancements

Please file enhancement requests at https://github.com/GetGatekeeper/FeatureRequests and not in this repository.

## Code Contributions

We're looking forward to any code contributions. Please note that upon submission of your Pull Request you will have to [sign our CAA](https://cla-assistant.io/GetGatekeeper/).

In case of any questions, feel free to file an issue :-)

# Development Environment

## Setup

You can get a running development environment in less than 5 minutes by using Visual Studio Code Containers. You can do so either locally or using [GitHub codespaces](https://github.com/features/codespaces) (which does offer a Cloud IDE).

### Locally

Follow the guide at https://code.visualstudio.com/docs/remote/containers#_quick-start-open-a-git-repository-or-github-pr-in-an-isolated-container-volume and as GitHub URL type "GetGatekeeper/Server". This will setup all docker containers up locally and configure your development environment.

### GitHub Codespaces

Just start a new codespace from the "main" branch. This will setup all containers in the cloud for you and offer you an IDE accessibly via browser.

![Start new Codespace](https://user-images.githubusercontent.com/878997/99189373-2c17cb80-2761-11eb-8858-48f3b0ebdfb3.png)

## Services

### Web UIs
The containers will start several services for you that can help you whilst developing. The forwarded ports are:

- 8025: [MailHog](https://github.com/mailhog/MailHog)
- 8080: [Adminer](https://www.adminer.org/)

### Backend
The following backend services will be available:

- 5432: [PostgreSQL](https://www.postgresql.org/)

## Build and run Gatekeeper

### Web Server

Start the web server by running:

```bash
cd Server/ && dotnet watch run
```

This will start the web server accessible at port 5001. 

To access it either go to http://localhost:5001 (locally) or for a Codespace environment, use the UI to get the hostname:

![Open in Codespace](https://user-images.githubusercontent.com/878997/99189512-cd9f1d00-2761-11eb-8d7d-ab54fb4bc4c9.png)

### Build SCSS

To build the SCSS, execute the following:

```bash
cd Client/ && gulp
```

### Run tests locally

```bash
cd Client.Tests/ && dotnet test
cd Server.Tests/ && dotnet test
```
