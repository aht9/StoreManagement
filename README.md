# StoreManagement

[![Build Status](https://img.shields.io/github/actions/workflow/status/YourUserName/StoreManagement/ci.yml?branch=main)](https://github.com/YourUserName/StoreManagement/actions)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)  
[![Version](https://img.shields.io/github/v/release/YourUserName/StoreManagement)](https://github.com/YourUserName/StoreManagement/releases)

A modern WPF desktop application for small retail businesses, built with  
Domain-Driven Design, CQRS/MediatR, EF Core, AutoMapper and Material Design in XAML.

---
## Table of Contents

1. [Features](#features)  
2. [Prerequisites](#prerequisites)  
3. [Installation](#installation)  
4. [Configuration](#configuration)  
5. [Usage](#usage)  
6. [Sample Code](#sample-code)  
7. [Running Tests](#running-tests)  
8. [Contributing](#contributing)  
9. [License](#license)  
10. [Contact & Acknowledgements](#contact--acknowledgements)

---

## Features

- Full **Domain-Driven** architecture  
- **CQRS** + **MediatR** for commands & queries  
- **EF Core** with Specification pattern  
- **AutoMapper** for DTO ⇆ Entity mapping  
- **Material Design** UI (MaterialDesignInXamlToolkit)  
- Dependency Injection via Microsoft.Extensions.DependencyInjection  
- Dialog & notification service abstraction  
- Unit- & integration- test friendly

---

## Prerequisites

- .NET 6 SDK or higher  
- Visual Studio 2022 / Rider / VS Code  
- SQL Server (LocalDB or full)  
- (Optional) Docker for containerized databases  

---

## Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/aht9/StoreManagement.git
   cd StoreManagement
   
2. **Restore NuGet packages**

   ```bash
   dotnet restore

4. **Build the solution**
 
    ```bash
    cd ../../
    dotnet build

    
## Configuration

- MediatR handlers and repositories are auto‐registered by scanning assemblies.

## Contributing

**We welcome contributions! Please follow these steps:**
1. Fork the repo

2. Create a feature branch:
   
    ```bash
    git checkout -b feature/YourFeature

3. Commit your changes
4. Push to your fork and open a Pull Request against main
5. Fill out the PR template, link any relevant issues
6. Ensure all CI checks (build + tests) pass

**Please respect the existing code style and add tests for any new functionality.**


