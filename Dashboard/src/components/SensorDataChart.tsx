import React from "react";
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
import { SensorData } from "../services/apiService";

interface ChartLine {
  dataKey: string;
  name: string;
  color: string;
}

interface SensorDataChartProps {
  data: SensorData[];
  lines: ChartLine[];
  formatTimestamp: (timestamp: string) => string;
}

const SensorDataChart: React.FC<SensorDataChartProps> = ({
  data,
  lines,
  formatTimestamp,
}) => {

  const chronologicalData = [...data].reverse();

  return (
    <ResponsiveContainer width="100%" height={300}>
      <LineChart data={chronologicalData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis
          dataKey="timestamp"
          tickFormatter={(timestamp) =>
            new Date(timestamp).toLocaleTimeString("de-DE")
          }
        />
        <YAxis />
        <Tooltip labelFormatter={(label) => formatTimestamp(label as string)} />
        <Legend />
        {lines.map((line, index) => (
          <Line
            key={index}
            type="monotone"
            dataKey={line.dataKey}
            name={line.name}
            stroke={line.color}
            dot={false}
            activeDot={false}
          />
        ))}
      </LineChart>
    </ResponsiveContainer>
  );
};

export default SensorDataChart;
