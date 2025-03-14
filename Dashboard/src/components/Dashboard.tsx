import { useState, useEffect } from "react";
import {
  Paper,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
  Tabs,
  Tab,
  Chip,
} from "@mui/material";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from "recharts";
import Grid from "@mui/material/Grid2";
import {
  fetchLatestSensorData,
  fetchSensorSummary,
  fetchSensorDataByType,
  SensorData,
  SensorSummary,
  SensorType,
} from "../services/apiService";

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`sensor-tabpanel-${index}`}
      aria-labelledby={`sensor-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

const Dashboard = () => {
  const [latestData, setLatestData] = useState<SensorData[]>([]);
  const [sensorSummary, setSensorSummary] = useState<SensorSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tabValue, setTabValue] = useState(0);
  const [selectedType, setSelectedType] = useState<SensorType>(
    SensorType.Environmental
  );
  const [typeData, setTypeData] = useState<SensorData[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [latest, summary] = await Promise.all([
          fetchLatestSensorData(),
          fetchSensorSummary(),
        ]);
        console.info(latest);
        setLatestData(latest);
        setSensorSummary(summary);
        setError(null);
      } catch (err) {
        setError("Error fetching data from the API");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
    const interval = setInterval(fetchData, 5000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const fetchTypeData = async () => {
      try {
        setLoading(true);
        const data = await fetchSensorDataByType(selectedType);
        setTypeData(data);
        setError(null);
      } catch (err) {
        setError(`Error fetching data for type ${selectedType}`);
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchTypeData();
  }, [selectedType]);

  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString();
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
    setSelectedType(Object.values(SensorType)[newValue]);
  };

  const getSensorTypeColor = (type: SensorType) => {
    switch (type) {
      case SensorType.Environmental:
        return "#4caf50"; // Green
      case SensorType.AirQuality:
        return "#2196f3"; // Blue
      case SensorType.Water:
        return "#03a9f4"; // Light Blue
      case SensorType.Energy:
        return "#ff9800"; // Orange
      case SensorType.Motion:
        return "#9c27b0"; // Purple
      case SensorType.Light:
        return "#ffeb3b"; // Yellow
      default:
        return "#757575"; // Grey
    }
  };

  if (loading && !latestData.length) {
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "100vh",
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Sensor Data Dashboard
      </Typography>

      {error && (
        <Paper
          sx={{
            p: 2,
            mb: 3,
            backgroundColor: "#ffebee",
            color: "#c62828",
          }}
        >
          <Typography>{error}</Typography>
        </Paper>
      )}

      <Grid container spacing={3}>
        {/* Sensor Summary Cards */}
        <Grid size={{ xs: 12 }}>
          <Typography variant="h6" gutterBottom>
            Sensor Summary
          </Typography>
          <Box sx={{ display: "flex", flexWrap: "wrap", gap: 2 }}>
            {sensorSummary.slice(0, 6).map((sensor) => (
              <Paper
                key={`${sensor.sensorId}-${sensor.sensorType}`}
                elevation={3}
                sx={{
                  p: 2,
                  minWidth: 280,
                  flex: "1 1 280px",
                  maxWidth: 350,
                  borderLeft: `4px solid ${getSensorTypeColor(
                    sensor.sensorType
                  )}`,
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
                <Box sx={{ mt: 1 }}>
                  {sensor.sensorType === SensorType.Environmental && (
                    <>
                      {sensor.latestTemperature !== undefined &&
                        sensor.latestTemperature !== null && (
                          <Typography variant="body1">
                            Temperature: {sensor.latestTemperature.toFixed(1)}{" "}
                            °C
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
                  )}

                  {sensor.sensorType === SensorType.AirQuality && (
                    <>
                      {sensor.latestCO2 !== undefined &&
                        sensor.latestCO2 !== null && (
                          <Typography variant="body1">
                            CO2: {sensor.latestCO2.toFixed(1)} ppm
                          </Typography>
                        )}
                      {sensor.latestPM25 !== undefined &&
                        sensor.latestPM25 !== null && (
                          <Typography variant="body1">
                            PM2.5: {sensor.latestPM25.toFixed(1)} µg/m³
                          </Typography>
                        )}
                    </>
                  )}

                  {sensor.sensorType === SensorType.Water && (
                    <>
                      {sensor.latestPH !== undefined &&
                        sensor.latestPH !== null && (
                          <Typography variant="body1">
                            pH: {sensor.latestPH.toFixed(1)}
                          </Typography>
                        )}
                      {sensor.latestDissolvedOxygen !== undefined &&
                        sensor.latestDissolvedOxygen !== null && (
                          <Typography variant="body1">
                            Dissolved Oxygen:{" "}
                            {sensor.latestDissolvedOxygen.toFixed(1)} mg/L
                          </Typography>
                        )}
                    </>
                  )}

                  {sensor.sensorType === SensorType.Energy && (
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
                  )}

                  {sensor.sensorType === SensorType.Motion && (
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
                  )}

                  {sensor.sensorType === SensorType.Light && (
                    <>
                      {sensor.latestIlluminance !== undefined &&
                        sensor.latestIlluminance !== null && (
                          <Typography variant="body1">
                            Illuminance: {sensor.latestIlluminance.toFixed(1)}{" "}
                            lux
                          </Typography>
                        )}
                      {sensor.latestUVIndex !== undefined &&
                        sensor.latestUVIndex !== null && (
                          <Typography variant="body1">
                            UV Index: {sensor.latestUVIndex.toFixed(1)}
                          </Typography>
                        )}
                    </>
                  )}
                </Box>
              </Paper>
            ))}
          </Box>
        </Grid>

        <Grid size={{ xs: 12 }}>
          <Paper sx={{ p: 2, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Latest Sensor Readings
            </Typography>
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
                  {latestData.slice(0, 5).map((data) => (
                    <TableRow key={`${data.sensorId}-${data.timestamp}`}>
                      <TableCell>{data.sensorId}</TableCell>
                      <TableCell>
                        <Chip
                          label={data.sensorType}
                          size="small"
                          sx={{
                            backgroundColor: getSensorTypeColor(
                              data.sensorType
                            ),
                            color: "white",
                          }}
                        />
                      </TableCell>
                      <TableCell>{formatTimestamp(data.timestamp)}</TableCell>
                      <TableCell>
                        {data.sensorType === SensorType.Environmental && (
                          <>
                            {data?.temperature &&
                              `Temp: ${data.temperature.toFixed(2)}°C, `}
                            {data?.humidity &&
                              `Humidity: ${data.humidity.toFixed(2)}%, `}
                            {data?.pressure &&
                              `Pressure: ${data.pressure.toFixed(2)} hPa`}
                          </>
                        )}
                        {data.sensorType === SensorType.AirQuality && (
                          <>
                            {data?.co2 && `CO2: ${data.co2.toFixed(2)} ppm, `}
                            {data?.voc && `VOC: ${data.voc.toFixed(2)} ppb, `}
                            {data?.pm25 &&
                              `PM2.5: ${data.pm25.toFixed(2)} µg/m³, `}
                            {data?.pm10 &&
                              `PM10: ${data.pm10.toFixed(2)} µg/m³`}
                          </>
                        )}
                        {data.sensorType === SensorType.Water && (
                          <>
                            {data?.ph && `pH: ${data.ph.toFixed(2)}, `}
                            {data?.turbidity &&
                              `Turbidity: ${data.turbidity.toFixed(2)} NTU, `}
                            {data?.dissolvedOxygen &&
                              `DO: ${data.dissolvedOxygen.toFixed(2)} mg/L, `}
                            {data?.conductivity &&
                              `Conductivity: ${data.conductivity.toFixed(
                                2
                              )} µS/cm`}
                          </>
                        )}
                        {data.sensorType === SensorType.Energy && (
                          <>
                            {data?.voltage &&
                              `Voltage: ${data.voltage.toFixed(2)} V, `}
                            {data?.current &&
                              `Current: ${data.current.toFixed(2)} A, `}
                            {data?.powerConsumption &&
                              `Power: ${data.powerConsumption.toFixed(2)} W`}
                          </>
                        )}
                        {data.sensorType === SensorType.Motion && (
                          <>
                            {data?.accelerationX &&
                              `X: ${data.accelerationX.toFixed(2)} m/s², `}
                            {data?.accelerationY &&
                              `Y: ${data.accelerationY.toFixed(2)} m/s², `}
                            {data?.accelerationZ &&
                              `Z: ${data.accelerationZ.toFixed(2)} m/s², `}
                            {data?.vibration &&
                              `Vibration: ${data.vibration.toFixed(2)} Hz`}
                          </>
                        )}
                        {data.sensorType === SensorType.Light && (
                          <>
                            {data?.illuminance &&
                              `Illuminance: ${data.illuminance.toFixed(
                                2
                              )} lux, `}
                            {data?.uvIndex &&
                              `UV: ${data.uvIndex.toFixed(2)}, `}
                            {data?.colorTemperature &&
                              `Color Temp: ${data.colorTemperature.toFixed(
                                2
                              )} K`}
                          </>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </Grid>

        <Grid size={{ xs: 12 }}>
          <Paper sx={{ p: 2 }}>
            <Box sx={{ borderBottom: 1, borderColor: "divider", mb: 2 }}>
              <Tabs
                value={tabValue}
                onChange={handleTabChange}
                aria-label="sensor type tabs"
              >
                {Object.values(SensorType).map((type, index) => (
                  <Tab
                    key={type}
                    label={type}
                    id={`sensor-tab-${index}`}
                    aria-controls={`sensor-tabpanel-${index}`}
                  />
                ))}
              </Tabs>
            </Box>

            {/* Environmental Sensors Tab */}
            <TabPanel value={tabValue} index={0}>
              <Typography variant="h6" gutterBottom>
                Environmental Sensors
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={typeData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(timestamp) =>
                      new Date(timestamp).toLocaleTimeString()
                    }
                  />
                  <YAxis />
                  <Tooltip
                    labelFormatter={(label) => formatTimestamp(label as string)}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="temperature"
                    name="Temperature (°C)"
                    stroke="#ff5722"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="humidity"
                    name="Humidity (%)"
                    stroke="#2196f3"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="pressure"
                    name="Pressure (hPa)"
                    stroke="#4caf50"
                    activeDot={{ r: 8 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </TabPanel>

            {/* Air Quality Sensors Tab */}
            <TabPanel value={tabValue} index={1}>
              <Typography variant="h6" gutterBottom>
                Air Quality Sensors
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={typeData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(timestamp) =>
                      new Date(timestamp).toLocaleTimeString()
                    }
                  />
                  <YAxis />
                  <Tooltip
                    labelFormatter={(label) => formatTimestamp(label as string)}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="co2"
                    name="CO2 (ppm)"
                    stroke="#ff5722"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="voc"
                    name="VOC (ppb)"
                    stroke="#2196f3"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="pm25"
                    name="PM2.5 (µg/m³)"
                    stroke="#4caf50"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="pm10"
                    name="PM10 (µg/m³)"
                    stroke="#9c27b0"
                    activeDot={{ r: 8 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </TabPanel>

            {/* Water Sensors Tab */}
            <TabPanel value={tabValue} index={2}>
              <Typography variant="h6" gutterBottom>
                Water Quality Sensors
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={typeData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(timestamp) =>
                      new Date(timestamp).toLocaleTimeString()
                    }
                  />
                  <YAxis />
                  <Tooltip
                    labelFormatter={(label) => formatTimestamp(label as string)}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="ph"
                    name="pH"
                    stroke="#ff5722"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="turbidity"
                    name="Turbidity (NTU)"
                    stroke="#2196f3"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="dissolvedOxygen"
                    name="Dissolved Oxygen (mg/L)"
                    stroke="#4caf50"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="conductivity"
                    name="Conductivity (µS/cm)"
                    stroke="#9c27b0"
                    activeDot={{ r: 8 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </TabPanel>

            {/* Energy Sensors Tab */}
            <TabPanel value={tabValue} index={3}>
              <Typography variant="h6" gutterBottom>
                Energy Sensors
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={typeData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(timestamp) =>
                      new Date(timestamp).toLocaleTimeString()
                    }
                  />
                  <YAxis />
                  <Tooltip
                    labelFormatter={(label) => formatTimestamp(label as string)}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="voltage"
                    name="Voltage (V)"
                    stroke="#ff5722"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="current"
                    name="Current (A)"
                    stroke="#2196f3"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="powerConsumption"
                    name="Power (W)"
                    stroke="#4caf50"
                    activeDot={{ r: 8 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </TabPanel>

            {/* Motion Sensors Tab */}
            <TabPanel value={tabValue} index={4}>
              <Typography variant="h6" gutterBottom>
                Motion Sensors
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={typeData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(timestamp) =>
                      new Date(timestamp).toLocaleTimeString()
                    }
                  />
                  <YAxis />
                  <Tooltip
                    labelFormatter={(label) => formatTimestamp(label as string)}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="accelerationX"
                    name="X-Axis (m/s²)"
                    stroke="#ff5722"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="accelerationY"
                    name="Y-Axis (m/s²)"
                    stroke="#2196f3"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="accelerationZ"
                    name="Z-Axis (m/s²)"
                    stroke="#4caf50"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="vibration"
                    name="Vibration (Hz)"
                    stroke="#9c27b0"
                    activeDot={{ r: 8 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </TabPanel>

            {/* Light Sensors Tab */}
            <TabPanel value={tabValue} index={5}>
              <Typography variant="h6" gutterBottom>
                Light Sensors
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={typeData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(timestamp) =>
                      new Date(timestamp).toLocaleTimeString()
                    }
                  />
                  <YAxis />
                  <Tooltip
                    labelFormatter={(label) => formatTimestamp(label as string)}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="illuminance"
                    name="Illuminance (lux)"
                    stroke="#ff5722"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="uvIndex"
                    name="UV Index"
                    stroke="#2196f3"
                    activeDot={{ r: 8 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="colorTemperature"
                    name="Color Temperature (K)"
                    stroke="#4caf50"
                    activeDot={{ r: 8 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </TabPanel>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;
