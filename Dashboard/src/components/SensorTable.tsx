import React from "react";
import {
  TableContainer,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  Chip,
} from "@mui/material";
import { SensorData, SensorType } from "../services/apiService";
import { getSensorTypeColor } from "../utils/sensorUtils";

interface SensorTableProps {
  data: SensorData[];
  formatTimestamp: (timestamp: string) => string;
}

const SensorTable: React.FC<SensorTableProps> = ({ data, formatTimestamp }) => {
  return (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Sensor ID</TableCell>
            <TableCell>Type</TableCell>
            <TableCell>Timestamp</TableCell>
            <TableCell>Readings</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {data.map((item) => (
            <TableRow key={`${item.sensorId}-${item.timestamp}`}>
              <TableCell>{item.sensorId}</TableCell>
              <TableCell>
                <Chip
                  label={item.sensorType}
                  size="small"
                  sx={{
                    backgroundColor: getSensorTypeColor(item.sensorType),
                    color: "white",
                  }}
                />
              </TableCell>
              <TableCell>{formatTimestamp(item.timestamp)}</TableCell>
              <TableCell>{renderSensorReadings(item)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

const renderSensorReadings = (data: SensorData) => {
  switch (data.sensorType) {
    case SensorType.Environmental:
      return (
        <>
          {data?.temperature && `Temp: ${data.temperature.toFixed(2)}°C, `}
          {data?.humidity && `Humidity: ${data.humidity.toFixed(2)}%, `}
          {data?.pressure && `Pressure: ${data.pressure.toFixed(2)} hPa`}
        </>
      );
    case SensorType.AirQuality:
      return (
        <>
          {data?.cO2 && `CO2: ${data.cO2.toFixed(2)} ppm, `}
          {data?.voc && `VOC: ${data.voc.toFixed(2)} ppb, `}
          {data?.pM25 && `PM2.5: ${data.pM25.toFixed(2)} µg/m³, `}
          {data?.pM10 && `PM10: ${data.pM10.toFixed(2)} µg/m³`}
        </>
      );
    case SensorType.Water:
      return (
        <>
          {data?.ph && `pH: ${data.ph.toFixed(2)}, `}
          {data?.turbidity && `Turbidity: ${data.turbidity.toFixed(2)} NTU, `}
          {data?.dissolvedOxygen &&
            `DO: ${data.dissolvedOxygen.toFixed(2)} mg/L, `}
          {data?.conductivity &&
            `Conductivity: ${data.conductivity.toFixed(2)} µS/cm`}
        </>
      );
    case SensorType.Energy:
      return (
        <>
          {data?.voltage && `Voltage: ${data.voltage.toFixed(2)} V, `}
          {data?.current && `Current: ${data.current.toFixed(2)} A, `}
          {data?.powerConsumption &&
            `Power: ${data.powerConsumption.toFixed(2)} W`}
        </>
      );
    case SensorType.Motion:
      return (
        <>
          {data?.accelerationX && `X: ${data.accelerationX.toFixed(2)} m/s², `}
          {data?.accelerationY && `Y: ${data.accelerationY.toFixed(2)} m/s², `}
          {data?.accelerationZ && `Z: ${data.accelerationZ.toFixed(2)} m/s², `}
          {data?.vibration && `Vibration: ${data.vibration.toFixed(2)} Hz`}
        </>
      );
    case SensorType.Light:
      return (
        <>
          {data?.illuminance &&
            `Illuminance: ${data.illuminance.toFixed(2)} lux, `}
          {data?.uvIndex && `UV: ${data.uvIndex.toFixed(2)}, `}
          {data?.colorTemperature &&
            `Color Temp: ${data.colorTemperature.toFixed(2)} K`}
        </>
      );
    default:
      return null;
  }
};

export default SensorTable;
