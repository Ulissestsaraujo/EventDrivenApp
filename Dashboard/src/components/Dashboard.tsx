import { useState, useEffect } from "react";
import {
  Paper,
  Typography,
  Box,
  CircularProgress,
  SelectChangeEvent,
} from "@mui/material";
import Grid from "@mui/material/Grid2";
import {
  fetchLatestSensorData,
  fetchSensorSummary,
  fetchSensorDataByType,
  fetchSensorErrors,
  SensorData,
  SensorSummary,
  SensorType,
  SensorError,
} from "../services/apiService";

import SensorSummarySection from "./SensorSummarySection";
import SensorTable from "./SensorTable";
import SensorChartsSection from "./SensorChartsSection";
import SensorErrorsPanel from "./SensorErrorsPanel";

const Dashboard = () => {
  const [latestData, setLatestData] = useState<SensorData[]>([]);
  const [sensorSummary, setSensorSummary] = useState<SensorSummary[]>([]);
  const [summaryPagination, setSummaryPagination] = useState({
    totalCount: 0,
    totalPages: 0,
    currentPage: 1,
    pageSize: 6,
  });
  const [summaryFilterType, setSummaryFilterType] = useState<
    SensorType | undefined
  >(undefined);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tabValue, setTabValue] = useState(0);
  const [selectedType, setSelectedType] = useState<SensorType>(
    SensorType.Environmental
  );
  const [typeData, setTypeData] = useState<SensorData[]>([]);
  const [sensorErrors, setSensorErrors] = useState<SensorError[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [latest, summaryResponse, errors] = await Promise.all([
          fetchLatestSensorData(),
          fetchSensorSummary(
            summaryPagination.currentPage,
            summaryPagination.pageSize,
            summaryFilterType
          ),
          fetchSensorErrors(),
        ]);

        setLatestData(latest);
        setSensorSummary(summaryResponse.data);
        setSummaryPagination({
          totalCount: summaryResponse.totalCount,
          totalPages: summaryResponse.totalPages,
          currentPage: summaryResponse.currentPage,
          pageSize: summaryResponse.pageSize,
        });
        setSensorErrors(errors);
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
  }, [
    summaryPagination.currentPage,
    summaryPagination.pageSize,
    summaryFilterType,
  ]);

  const fetchTypeData = async () => {
    try {
      const data = await fetchSensorDataByType(selectedType);
      setTypeData(data);
      setError(null);
    } catch (err) {
      setError(`Error fetching data for type ${selectedType}`);
      console.error(err);
    }
  };

  useEffect(() => {
    fetchTypeData();
  }, [selectedType]);

  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString();
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
    setSelectedType(Object.values(SensorType)[newValue]);
  };

  const handlePageChange = (
    _event: React.ChangeEvent<unknown>,
    page: number
  ) => {
    setSummaryPagination((prev) => ({
      ...prev,
      currentPage: page,
    }));
  };

  const handleFilterChange = (event: SelectChangeEvent) => {
    const value = event.target.value;
    setSummaryFilterType(value === "all" ? undefined : (value as SensorType));
    setSummaryPagination((prev) => ({
      ...prev,
      currentPage: 1,
    }));
  };

  const handleRefreshChartData = () => {
    fetchTypeData();
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
        {/* Sensor Error Panel */}
        <Grid size={{ xs: 12 }}>
          <SensorErrorsPanel
            errorData={sensorErrors.slice(0, 3)}
            formatTimestamp={formatTimestamp}
          />
        </Grid>

        {/* Sensor Summary Section */}
        <Grid size={{ xs: 12 }}>
          <SensorSummarySection
            sensorSummary={sensorSummary}
            formatTimestamp={formatTimestamp}
            pagination={summaryPagination}
            filterType={summaryFilterType}
            onPageChange={handlePageChange}
            onFilterChange={handleFilterChange}
          />
        </Grid>

        {/* Latest Sensor Readings Section */}
        <Grid size={{ xs: 12 }}>
          <Paper sx={{ p: 2, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Latest Sensor Readings
            </Typography>
            <SensorTable data={latestData} formatTimestamp={formatTimestamp} />
          </Paper>
        </Grid>

        {/* Sensor Charts Section */}
        <Grid size={{ xs: 12 }}>
          <Paper sx={{ p: 2 }}>
            <SensorChartsSection
              tabValue={tabValue}
              typeData={typeData}
              formatTimestamp={formatTimestamp}
              onTabChange={handleTabChange}
              onRefresh={handleRefreshChartData}
            />
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;
