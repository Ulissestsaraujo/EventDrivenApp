import React from "react";
import {
  Paper,
  Typography,
  Box,
  List,
  ListItem,
  Chip,
  Divider,
} from "@mui/material";
import ErrorIcon from "@mui/icons-material/Error";
import { SensorError } from "../services/apiService";
import { getSensorTypeColor } from "../utils/sensorUtils";

interface SensorErrorsPanelProps {
  errorData: SensorError[];
  formatTimestamp: (timestamp: string) => string;
}

const SensorErrorsPanel: React.FC<SensorErrorsPanelProps> = ({
  errorData,
  formatTimestamp,
}) => {
  return (
    <Paper sx={{ p: 2, mb: 3 }}>
      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          mb: 2,
          gap: 1,
        }}
      >
        <ErrorIcon color="error" />
        <Typography variant="h6">Sensors with Most Errors</Typography>
      </Box>

      {errorData.length === 0 ? (
        <Typography variant="body1" color="text.secondary">
          No sensor errors detected
        </Typography>
      ) : (
        <List>
          {errorData.map((error, index) => (
            <React.Fragment key={error.sensorId}>
              {index > 0 && <Divider component="li" />}
              <ListItem
                alignItems="flex-start"
                sx={{
                  borderLeft: `4px solid ${getSensorTypeColor(
                    error.sensorType
                  )}`,
                  pl: 2,
                }}
              >
                <Box sx={{ width: "100%" }}>
                  <Box
                    sx={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                      mb: 1,
                    }}
                  >
                    <Typography variant="subtitle1">
                      {error.sensorId}
                    </Typography>
                    <Chip
                      label={`${error.errorCount} errors`}
                      color="error"
                      size="small"
                    />
                  </Box>

                  <Box sx={{ display: "flex", gap: 1, mt: 0.5 }}>
                    <Chip
                      label={error.sensorType}
                      size="small"
                      sx={{
                        backgroundColor: getSensorTypeColor(error.sensorType),
                        color: "white",
                      }}
                    />
                    <Typography
                      component="span"
                      variant="body2"
                      color="text.secondary"
                    >
                      Last error: {formatTimestamp(error.lastErrorTimestamp)}
                    </Typography>
                  </Box>

                  <Typography
                    component="div"
                    variant="body2"
                    color="error"
                    sx={{ mt: 0.5 }}
                  >
                    {error.lastErrorMessage}
                  </Typography>
                </Box>
              </ListItem>
            </React.Fragment>
          ))}
        </List>
      )}
    </Paper>
  );
};

export default SensorErrorsPanel;
