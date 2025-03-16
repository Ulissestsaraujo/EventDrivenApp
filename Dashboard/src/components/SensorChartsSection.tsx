import React from "react";
import { Box, Tabs, Tab, Paper } from "@mui/material";
import { SensorData, SensorType } from "../services/apiService";
import SensorChartPanel from "./SensorChartPanel";

interface SensorChartsSectionProps {
  tabValue: number;
  typeData: SensorData[];
  formatTimestamp: (timestamp: string) => string;
  onTabChange: (event: React.SyntheticEvent, newValue: number) => void;
}

const SensorChartsSection: React.FC<SensorChartsSectionProps> = ({
  tabValue,
  typeData,
  formatTimestamp,
  onTabChange,
}) => {
  return (
    <Paper sx={{ p: 2 }}>
      <Box sx={{ borderBottom: 1, borderColor: "divider", mb: 2 }}>
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
    </Paper>
  );
};

export default SensorChartsSection;
