import React from "react";
import {
  Box,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  SelectChangeEvent,
  Pagination,
} from "@mui/material";
import { SensorSummary, SensorType } from "../services/apiService";
import SensorSummaryCard from "./SensorSummaryCard";

interface SensorSummarySectionProps {
  sensorSummary: SensorSummary[];
  formatTimestamp: (timestamp: string) => string;
  pagination: {
    totalCount: number;
    totalPages: number;
    currentPage: number;
    pageSize: number;
  };
  filterType?: SensorType;
  onPageChange: (event: React.ChangeEvent<unknown>, page: number) => void;
  onFilterChange: (event: SelectChangeEvent) => void;
}

const SensorSummarySection: React.FC<SensorSummarySectionProps> = ({
  sensorSummary,
  formatTimestamp,
  pagination,
  filterType,
  onPageChange,
  onFilterChange,
}) => {
  return (
    <>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 2,
        }}
      >
        <Typography variant="h6">Sensor Summary</Typography>
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <FormControl sx={{ minWidth: 120 }} size="small">
            <InputLabel id="filter-type-label">Filter Type</InputLabel>
            <Select
              labelId="filter-type-label"
              value={filterType || "all"}
              label="Filter Type"
              onChange={onFilterChange}
            >
              <MenuItem value="all">All Types</MenuItem>
              {Object.values(SensorType).map((type) => (
                <MenuItem key={type} value={type}>
                  {type}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
      </Box>

      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 2 }}>
        {sensorSummary.map((sensor) => (
          <SensorSummaryCard
            key={`${sensor.sensorId}-${sensor.sensorType}`}
            sensor={sensor}
            formatTimestamp={formatTimestamp}
          />
        ))}
      </Box>

      {pagination.totalPages > 1 && (
        <Box sx={{ display: "flex", justifyContent: "center", mt: 2 }}>
          <Pagination
            count={pagination.totalPages}
            page={pagination.currentPage}
            onChange={onPageChange}
            color="primary"
          />
        </Box>
      )}
    </>
  );
};

export default SensorSummarySection;
