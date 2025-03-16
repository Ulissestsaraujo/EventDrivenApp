import { SensorType } from "../services/apiService";

export const getSensorTypeColor = (type: SensorType) => {
  switch (type) {
    case SensorType.Environmental:
      return "#2e7d32"; // Darker Green
    case SensorType.AirQuality:
      return "#1565c0"; // Darker Blue
    case SensorType.Water:
      return "#0277bd"; // Darker Light Blue
    case SensorType.Energy:
      return "#e65100"; // Darker Orange
    case SensorType.Motion:
      return "#7b1fa2"; // Darker Purple
    case SensorType.Light:
      return "#f57f17"; // Amber instead of Yellow
    default:
      return "#424242"; // Darker Grey
  }
};
