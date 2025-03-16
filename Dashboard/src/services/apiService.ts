import axios from "axios";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:8080";

export enum SensorType {
  Environmental = "Environmental",
  AirQuality = "AirQuality",
  Water = "Water",
  Energy = "Energy",
  Motion = "Motion",
  Light = "Light",
}

export interface SensorData {
  id: number;
  sensorId: string;
  sensorType: SensorType;
  timestamp: string;
  processed: boolean;

  // Environmental sensor data
  temperature?: number;
  humidity?: number;
  pressure?: number;

  // Air quality sensor data
  cO2?: number;
  voc?: number;
  pM25?: number;
  pM10?: number;

  // Water sensor data
  ph?: number;
  turbidity?: number;
  dissolvedOxygen?: number;
  conductivity?: number;

  // Energy sensor data
  voltage?: number;
  current?: number;
  powerConsumption?: number;

  // Motion sensor data
  accelerationX?: number;
  accelerationY?: number;
  accelerationZ?: number;
  vibration?: number;

  // Light sensor data
  illuminance?: number;
  uvIndex?: number;
  colorTemperature?: number;
}

export interface SensorSummary {
  sensorId: string;
  sensorType: SensorType;
  latestTimestamp: string;

  // Environmental data
  latestTemperature?: number;
  latestHumidity?: number;
  latestPressure?: number;

  // Air quality data
  latestCO2?: number;
  latestVOC?: number;
  latestPM25?: number;
  latestPM10?: number;

  // Water data
  latestPH?: number;
  latestTurbidity?: number;
  latestDissolvedOxygen?: number;
  latestConductivity?: number;

  // Energy data
  latestVoltage?: number;
  latestCurrent?: number;
  latestPowerConsumption?: number;

  // Motion data
  latestAccelerationX?: number;
  latestAccelerationY?: number;
  latestAccelerationZ?: number;
  latestVibration?: number;

  // Light data
  latestIlluminance?: number;
  latestUVIndex?: number;
  latestColorTemperature?: number;
}

export interface PaginatedResponse<T> {
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  data: T[];
}

export const fetchLatestSensorData = async (): Promise<SensorData[]> => {
  try {
    const response = await axios.get<SensorData[]>(
      `${API_URL}/api/SensorData/latest`
    );
    return response.data;
  } catch (error) {
    console.error("Error fetching latest sensor data:", error);
    return [];
  }
};

export const fetchSensorDataById = async (
  sensorId: string
): Promise<SensorData[]> => {
  try {
    const response = await axios.get<SensorData[]>(
      `${API_URL}/api/SensorData/bySensor/${sensorId}`
    );
    return response.data;
  } catch (error) {
    console.error(`Error fetching sensor data for ${sensorId}:`, error);
    return [];
  }
};

export const fetchSensorDataByType = async (
  sensorType: SensorType
): Promise<SensorData[]> => {
  try {
    const response = await axios.get<SensorData[]>(
      `${API_URL}/api/SensorData/byType/${sensorType}`
    );
    console.log(response.data);
    return response.data;
  } catch (error) {
    console.error(`Error fetching sensor data for type ${sensorType}:`, error);
    return [];
  }
};

export const fetchAllSensorData = async (): Promise<SensorData[]> => {
  try {
    const response = await axios.get<SensorData[]>(`${API_URL}/api/SensorData`);
    return response.data;
  } catch (error) {
    console.error("Error fetching all sensor data:", error);
    return [];
  }
};

export const fetchSensorSummary = async (
  page: number = 1,
  pageSize: number = 6,
  sensorType?: SensorType
): Promise<PaginatedResponse<SensorSummary>> => {
  try {
    let url = `${API_URL}/api/SensorData/summary?page=${page}&pageSize=${pageSize}`;

    if (sensorType) {
      url += `&sensorType=${sensorType}`;
    }

    const response = await axios.get<PaginatedResponse<SensorSummary>>(url);
    return response.data;
  } catch (error) {
    console.error("Error fetching sensor summary:", error);
    return {
      totalCount: 0,
      totalPages: 0,
      currentPage: page,
      pageSize: pageSize,
      data: [],
    };
  }
};
