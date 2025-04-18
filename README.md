# 🎮 .NET Microservices Project – Game Play Store

This is a simple microservices-based project built using **.NET**, inspired by a tutorial series from [Julio Casal](https://www.youtube.com/@juliocasal). The project simulates a backend system for a Game Play Store.

## 🧩 Microservices Included

- **Catalog Service** – Handles game catalog data.
- **Inventory Service** – Manages stock levels and availability of games.

## 🧠 What I Learned

### 🔁 Communication Between Services
- **Synchronous Communication**: Implemented using `HttpClient`.
- **Asynchronous Communication**: Implemented using `RabbitMQ` and `MassTransit`.

### 💾 Data & Storage
- Used **MongoDB** for data persistence in both microservices.

### 🐳 Containerization
- Used **Docker** and `docker-compose` to:
  - Run MongoDB
  - Run RabbitMQ
  - Manage service containers for development and testing

### ♻️ Shared Libraries
- Created **reusable NuGet packages** to share common code (like DTOs and settings) across microservices.

## 🛠️ Technologies Used

- [.NET 5](https://dotnet.microsoft.com/)
- [MongoDB](https://www.mongodb.com/)
- [RabbitMQ](https://www.rabbitmq.com/)
- [MassTransit](https://masstransit-project.com/)
- [Docker](https://www.docker.com/)




------------------------------------


# To run docker through VSCode

`docker run -d --rm --name mongo123 -p 27017:27017 -v mongodbdata:/data/db`

# To run docker through VS

Visual Studio doesn't have a direct UI to run single docker run commands like VS Code terminal, but you can use a docker-compose.yml file in your solution to run MongoDB easily.

### 🔹 Step-by-Step

#### 1. Add a Docker-Compose Project to Your Solution

If you don't already have it:

- Right-click your **solution**
- Go to **Add → Add Container Orchestration Support**
- Choose **Docker Compose**
- Target: **Linux** (Mongo runs best on Linux containers)

This will create a `docker-compose` project with a `docker-compose.yml`.

---

#### 2. Edit `docker-compose.override.yml` or `docker-compose.yml`

Replace or add the MongoDB service like this:

```yaml
version: "3.8"

services:
  mongo:
    image: mongo
    container_name: mongo
    ports:
      - "27017:27017"
    volumes:
      - mongodbdata:/data/db
    restart: unless-stopped

volumes:
  mongodbdata:
```

Note: Even though you're exposing port 27011 externally, the internal MongoDB port is always 27017.

---

#### 3. Set Docker-Compose as Startup Project

- Right-click the docker-compose project
- Click Set as Startup Project
- Press F5 (or Ctrl+F5 if you don’t need debugging)

Visual Studio will:

- Pull the mongo image
- Run it in a container
- Set up the volume and port mappings

# To run docker through VS - Alternative way

add above code to docker-compose.yml file

Then use terminal (view -> Terminal) to run the docker image

- To Run - `docker-compose up -d`

- To stop the container: - `docker-compose down`
