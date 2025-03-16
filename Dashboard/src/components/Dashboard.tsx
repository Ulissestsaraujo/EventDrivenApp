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
  SensorData,
  SensorSummary,
  SensorType,
} from "../services/apiService";

import SensorSummarySection from "./SensorSummarySection";
import SensorTable from "./SensorTable";
import SensorChartsSection from "./SensorChartsSection";

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

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [latest, summaryResponse] = await Promise.all([
          fetchLatestSensorData(),
          fetchSensorSummary(
            summaryPagination.currentPage,
            summaryPagination.pageSize,
            summaryFilterType
          ),
        ]);

        setLatestData(latest);
        setSensorSummary(summaryResponse.data);
        setSummaryPagination({
          totalCount: summaryResponse.totalCount,
          totalPages: summaryResponse.totalPages,
          currentPage: summaryResponse.currentPage,
          pageSize: summaryResponse.pageSize,
        });
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
          <SensorChartsSection
            tabValue={tabValue}
            typeData={typeData}
            formatTimestamp={formatTimestamp}
            onTabChange={handleTabChange}
          />
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;
