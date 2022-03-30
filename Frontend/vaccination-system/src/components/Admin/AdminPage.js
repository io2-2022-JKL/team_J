import * as React from 'react';
import Button from '@mui/material/Button';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import { Container, CssBaseline } from '@mui/material';
import AccountBoxRoundedIcon from '@mui/icons-material/AccountBoxRounded';
import Typography from '@mui/material/Typography';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import { useNavigate } from "react-router-dom";

const theme = createTheme();

export default function AdminPage() {
    const name = "Jan";
    const surname = "Kowalski"
    const navigate = useNavigate();
    return (
        <ThemeProvider theme={theme}>
            <Container component="main" maxWidth="xs">
                <CssBaseline>
                    <Box
                        sx={{
                            marginTop: 8,
                            display: 'flex',
                            flexDirection: 'column',
                            alignItems: 'center',
                        }}
                    >
                        <Avatar sx={{ m: 1, bgcolor: 'secondary.main' }}>
                            <AccountBoxRoundedIcon />
                        </Avatar>
                        <Typography component="h1" variant="h4">
                            {name} {surname}
                        </Typography>
                        <Typography component="h1" variant="h6">
                            Administrator
                        </Typography>
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                            onClick={() => { navigate("/admin/patients") }}
                        >
                            Pacjenci
                        </Button>
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                            onClick={() => { navigate("/admin/doctors") }}
                        >
                            Doktorzy
                        </Button>
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                            onClick={() => { navigate("/admin/vaccinationCenters") }}
                        >
                            Centra Szczepień
                        </Button>
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                            onClick={() => { navigate("/admin/vaccines") }}
                        >
                            Szczepionki
                        </Button>
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                            onClick={() => { navigate("/signin") }}
                        >
                            Wyloguj się
                        </Button>
                    </Box>
                </CssBaseline>
            </Container>
        </ThemeProvider>
    );
}