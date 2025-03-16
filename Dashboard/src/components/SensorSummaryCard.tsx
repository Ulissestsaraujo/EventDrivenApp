import React from "react";
import { Paper, Typography, Box, Chip } from "@mui/material";
import { SensorSummary, SensorType } from "../services/apiService";
import { getSensorTypeColor } from "../utils/sensorUtils";

interface SensorSummaryCardProps {
  sensor: SensorSummary;
  formatTimestamp: (timestamp: string) => string;
}

const SensorSummaryCard: React.FC<SensorSummaryCardProps> = ({
  sensor,
  formatTimestamp,
}) => {
  return (
    <Paper
      key={`${sensor.sensorId}-${sensor.sensorType}`}
      elevation={3}
      sx={{
        p: 2,
        minWidth: 280,
        flex: "1 1 280px",
        maxWidth: 350,
        borderLeft: `4px solid ${getSensorTypeColor(sensor.sensorType)}`,
      }}
    >
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 1,
        }}
      >
        <Typography variant="h6">{sensor.sensorId}</Typography>
        <Chip
          label={sensor.sensorType}
          size="small"
          sx={{
            backgroundColor: getSensorTypeColor(sensor.sensorType),
            color: "white",
          }}
        />
      </Box>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Last updated: {formatTimestamp(sensor.latestTimestamp)}
      </Typography>
      <Box sx={{ mt: 1 }}>{renderSensorSpecificData(sensor)}</Box>
    </Paper>
  );
};

const renderSensorSpecificData = (sensor: SensorSummary) => {
  switch (sensor.sensorType) {
    case SensorType.Environmental:
      return (
        <>
          {sensor.latestTemperature !== undefined &&
            sensor.latestTemperature !== null && (
              <Typography variant="body1">
                Temperature: {sensor.latestTemperature.toFixed(1)} °C
              </Typography>
            )}
          {sensor.latestHumidity !== undefined &&
            sensor.latestHumidity !== null && (
              <Typography variant="body1">
                Humidity: {sensor.latestHumidity.toFixed(1)} %
              </Typography>
            )}
          {sensor.latestPressure !== undefined &&
            sensor.latestPressure !== null && (
              <Typography variant="body1">
                Pressure: {sensor.latestPressure.toFixed(1)} hPa
              </Typography>
            )}
        </>
      );
    case SensorType.AirQuality:
      return (
        <>
          {sensor.latestCO2 !== undefined && sensor.latestCO2 !== null && (
            <Typography variant="body1">
              CO2: {sensor.latestCO2.toFixed(1)} ppm
            </Typography>
          )}
          {sensor.latestPM25 !== undefined && sensor.latestPM25 !== null && (
            <Typography variant="body1">
              PM2.5: {sensor.latestPM25.toFixed(1)} µg/m³
            </Typography>
          )}
        </>
      );
    case SensorType.Water:
      return (
        <>
          {sensor.latestPH !== undefined && sensor.latestPH !== null && (
            <Typography variant="body1">
              pH: {sensor.latestPH.toFixed(1)}
            </Typography>
          )}
          {sensor.latestDissolvedOxygen !== undefined &&
            sensor.latestDissolvedOxygen !== null && (
              <Typography variant="body1">
                Dissolved Oxygen: {sensor.latestDissolvedOxygen.toFixed(1)} mg/L
              </Typography>
            )}
        </>
      );
    case SensorType.Energy:
      return (
        <>
          {sensor.latestPowerConsumption !== undefined &&
            sensor.latestPowerConsumption !== null && (
              <Typography variant="body1">
                Power: {sensor.latestPowerConsumption.toFixed(1)} W
              </Typography>
            )}
          {sensor.latestVoltage !== undefined &&
            sensor.latestVoltage !== null && (
              <Typography variant="body1">
                Voltage: {sensor.latestVoltage.toFixed(1)} V
              </Typography>
            )}
        </>
      );
    case SensorType.Motion:
      return (
        <>
          {sensor.latestVibration !== undefined &&
            sensor.latestVibration !== null && (
              <Typography variant="body1">
                Vibration: {sensor.latestVibration.toFixed(1)} Hz
              </Typography>
            )}
          {sensor.latestAccelerationX !== undefined &&
            sensor.latestAccelerationX !== null && (
              <Typography variant="body1">
                X-Axis: {sensor.latestAccelerationX.toFixed(1)} m/s²
              </Typography>
            )}
        </>
      );
    case SensorType.Light:
      return (
        <>
          {sensor.latestIlluminance !== undefined &&
            sensor.latestIlluminance !== null && (
              <Typography variant="body1">
                Illuminance: {sensor.latestIlluminance.toFixed(1)} lux
              </Typography>
            )}
          {sensor.latestUVIndex !== undefined &&
            sensor.latestUVIndex !== null && (
              <Typography variant="body1">
                UV Index: {sensor.latestUVIndex.toFixed(1)}
              </Typography>
            )}
        </>
      );
    default:
      return null;
  }
};

export default SensorSummaryCard;
