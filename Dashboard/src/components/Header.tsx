import { AppBar, Toolbar, Typography } from "@mui/material";
import SensorsIcon from "@mui/icons-material/Sensors";

const Header = () => {
  return (
    <AppBar position="static">
      <Toolbar>
        <SensorsIcon sx={{ mr: 2 }} />
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          Sensor Data Dashboard
        </Typography>
      </Toolbar>
    </AppBar>
  );
};

export default Header;
