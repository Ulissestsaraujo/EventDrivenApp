import React from "react";
import { Typography } from "@mui/material";
import { SensorData, SensorType } from "../services/apiService";
import SensorDataChart from "./SensorDataChart";
import TabPanel from "./TabPanel";

interface SensorChartPanelProps {
  index: number;
  tabValue: number;
  data: SensorData[];
  sensorType: SensorType;
  formatTimestamp: (timestamp: string) => string;
}

const SensorChartPanel: React.FC<SensorChartPanelProps> = ({
  index,
  tabValue,
  data,
  sensorType,
  formatTimestamp,
}) => {
  return (
    <TabPanel value={tabValue} index={index}>
      <Typography variant="h6" gutterBottom>
        {getTitle(sensorType)}
      </Typography>
      <SensorDataChart
        data={data}
        lines={getChartLines(sensorType)}
        formatTimestamp={formatTimestamp}
      />
    </TabPanel>
  );
};

const getTitle = (sensorType: SensorType) => {
  switch (sensorType) {
    case SensorType.Environmental:
      return "Environmental Sensors";
    case SensorType.AirQuality:
      return "Air Quality Sensors";
    case SensorType.Water:
      return "Water Quality Sensors";
    case SensorType.Energy:
      return "Energy Sensors";
    case SensorType.Motion:
      return "Motion Sensors";
    case SensorType.Light:
      return "Light Sensors";
    default:
      return "Sensor Data";
  }
};

const getChartLines = (sensorType: SensorType) => {
  switch (sensorType) {
    case SensorType.Environmental:
      return [
        { dataKey: "temperature", name: "Temperature (°C)", color: "#ff5722" },
        { dataKey: "humidity", name: "Humidity (%)", color: "#2196f3" },
        { dataKey: "pressure", name: "Pressure (hPa)", color: "#4caf50" },
      ];
    case SensorType.AirQuality:
      return [
        { dataKey: "cO2", name: "CO2 (ppm)", color: "#ff5722" },
        { dataKey: "voc", name: "VOC (ppb)", color: "#2196f3" },
        { dataKey: "pM25", name: "PM2.5 (µg/m³)", color: "#4caf50" },
        { dataKey: "pM10", name: "PM10 (µg/m³)", color: "#9c27b0" },
      ];
    case SensorType.Water:
      return [
        { dataKey: "ph", name: "pH", color: "#ff5722" },
        { dataKey: "turbidity", name: "Turbidity (NTU)", color: "#2196f3" },
        {
          dataKey: "dissolvedOxygen",
          name: "Dissolved Oxygen (mg/L)",
          color: "#4caf50",
        },
        {
          dataKey: "conductivity",
          name: "Conductivity (µS/cm)",
          color: "#9c27b0",
        },
      ];
    case SensorType.Energy:
      return [
        { dataKey: "voltage", name: "Voltage (V)", color: "#ff5722" },
        { dataKey: "current", name: "Current (A)", color: "#2196f3" },
        {
          dataKey: "powerConsumption",
          name: "Power (W)",
          color: "#4caf50",
        },
      ];
    case SensorType.Motion:
      return [
        {
          dataKey: "accelerationX",
          name: "X-Axis (m/s²)",
          color: "#ff5722",
        },
        {
          dataKey: "accelerationY",
          name: "Y-Axis (m/s²)",
          color: "#2196f3",
        },
        {
          dataKey: "accelerationZ",
          name: "Z-Axis (m/s²)",
          color: "#4caf50",
        },
        { dataKey: "vibration", name: "Vibration (Hz)", color: "#9c27b0" },
      ];
    case SensorType.Light:
      return [
        { dataKey: "illuminance", name: "Illuminance (lux)", color: "#ff5722" },
        { dataKey: "uvIndex", name: "UV Index", color: "#2196f3" },
        {
          dataKey: "colorTemperature",
          name: "Color Temperature (K)",
          color: "#4caf50",
        },
      ];
    default:
      return [];
  }
};

export default SensorChartPanel;
