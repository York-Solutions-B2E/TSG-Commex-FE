# Communication Lifecycle App - Frontend

Blazor Server application for managing communication documents (EOBs, EOPs, ID Cards) through their lifecycle.

## 🚀 Quick Start

```bash
# Run the frontend application
dotnet run
```

Application will be available at: `https://localhost:7018`

## 🔗 Related Projects

* **Backend API**: [TSG-Commex-BE](https://github.com/York-Solutions-B2E/TSG-Commex-BE)
* **Docker Compose**: Located in parent directory

## 🛠️ Tech Stack

* Blazor Server (.NET 8)
* Bootstrap CSS Framework
* Integration with backend REST API

## 📋 Features

* Document lifecycle tracking
* Real-time updates via SignalR
* Responsive web interface
* Authentication integration

## 🐳 Docker Development

Use the docker-compose.yml in the parent directory to run the full stack:

```bash
# From the parent directory
docker-compose up -d
``` 