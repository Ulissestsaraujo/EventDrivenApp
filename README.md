# Sensor Dashboard Application

A modern dashboard application for monitoring various sensor data types, including environmental conditions, air quality, water metrics, energy consumption, motion detection, and light measurements.

## Getting Started

### Prerequisites

- Docker and Docker Compose installed on your system
- .NET SDK (for backend local development)
- Node.js (for frontend local development)

## Running the Application

### Using Docker Compose (Recommended)

The easiest way to run the entire application stack is using Docker Compose:

```bash
docker compose up -d
```

This will start all the necessary services, including:
- RabbitMQ message broker
- .NET backend services
- React frontend dashboard

Once the services are up and running, you can access the dashboard at:

```
http://localhost
```

### For Local Development

If you're developing and want to run services individually:

1. Start the RabbitMQ message broker using Docker:

```bash
docker compose up rabbitmq -d
```

2. Start individual .NET backend services as needed:

3. Start the dashboard:

```bash
# Navigate to the dashboard directory
cd Dashboard

# Install dependencies
npm install

# Start the development server
npm run dev
```

The dashboard will be available at:

```
http://localhost:5173
```

## Stopping the Application

To stop all services when using Docker Compose:

```bash
docker compose down
```

To remove volumes as well (clears all data):

```bash
docker compose down -v
```

## Architecture

This application follows an event-driven architecture with:
- RabbitMQ for message passing between services
- Individual .NET microservices for Event Producing and Consuming (API contained in the consumer for simplicity purposes)
- React dashboard for visualization
