# Unit Tests for Event-Driven Application

This directory contains unit tests for the key components of the event-driven application.

## Test Coverage

The tests cover the following key functionalities:

### EventProducer Tests
- `GenerateSensorData()` - Tests the data generation logic to ensure it produces valid sensor readings within expected ranges.
- `SaveSensorDataAsync()` - Tests database persistence for sensor data.

### EventConsumer Tests
- `SensorDataConsumer.Consume()` - Tests processing and storing sensor data received from the message queue.
- `SensorDataController` methods - Tests API endpoints that retrieve and aggregate sensor data.

## Running the Tests

To run all the unit tests, use the following command from the root of the project:

```bash
cd /Users/ulissestsaraujo/Git/ifolor-deep/EventDrivenApp
dotnet test Tests/UnitTests
```

To run specific test classes:

```bash
dotnet test Tests/UnitTests --filter FullyQualifiedName~WorkerTests
dotnet test Tests/UnitTests --filter FullyQualifiedName~SensorDataControllerTests
dotnet test Tests/UnitTests --filter FullyQualifiedName~SensorDataConsumerTests
```

## Test Architecture

These tests are designed as true unit tests, which means:

1. They test individual units of code in isolation.
2. They use mocking frameworks (Moq) to replace external dependencies.
3. They use in-memory databases instead of real databases to ensure tests are fast and reliable.
4. Each test is independent and can run in any order.

## Design Decisions

- **InMemory Database**: Used for testing database operations without requiring a real database.
- **Mocking**: External dependencies are mocked to isolate the code being tested.
- **Test Data**: Test data is generated specifically for each test to ensure predictable test results.
- **Visibility Modification**: Made some methods `internal` and added `InternalsVisibleTo` attribute to allow testing of otherwise private methods.

## Test Categories

1. **Functional Tests**: Verify that components behave as expected (e.g., `GenerateSensorData_ShouldReturnValidData`).
2. **Data Validation Tests**: Ensure data is correctly validated (e.g., range checks).
3. **Error Handling Tests**: Verify proper behavior during error conditions. 