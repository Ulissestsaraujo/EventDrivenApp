import React from "react";
import {
  Box,
  Tabs,
  Tab,
  IconButton,
  Tooltip as MuiTooltip,
} from "@mui/material";
import RefreshIcon from "@mui/icons-material/Refresh";
import { SensorData, SensorType } from "../services/apiService";
import SensorChartPanel from "./SensorChartPanel";

interface SensorChartsSectionProps {
  tabValue: number;
  typeData: SensorData[];
  formatTimestamp: (timestamp: string) => string;
  onTabChange: (event: React.SyntheticEvent, newValue: number) => void;
  onRefresh: () => void;
}

const SensorChartsSection: React.FC<SensorChartsSectionProps> = ({
  tabValue,
  typeData,
  formatTimestamp,
  onTabChange,
  onRefresh,
}) => {
  return (
    <>
      <Box
        sx={{
          borderBottom: 1,
          borderColor: "divider",
          mb: 2,
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
        }}
      >
        <Tabs
          value={tabValue}
          onChange={onTabChange}
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
        <MuiTooltip title="Refresh chart data">
          <IconButton onClick={onRefresh} color="primary" size="medium">
            <RefreshIcon />
          </IconButton>
        </MuiTooltip>
      </Box>

      {Object.values(SensorType).map((type, index) => (
        <SensorChartPanel
          key={type}
          index={index}
          tabValue={tabValue}
          data={typeData}
          sensorType={type}
          formatTimestamp={formatTimestamp}
        />
      ))}
    </>
  );
};

export default SensorChartsSection;
