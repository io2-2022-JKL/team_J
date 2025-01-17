import * as React from 'react';
import { useState } from 'react';
import Button from '@mui/material/Button';
import CssBaseline from '@mui/material/CssBaseline';
import TextField from '@mui/material/TextField';
import Grid from '@mui/material/Grid';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Container from '@mui/material/Container';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import { useLocation, useNavigate } from "react-router-dom";
import { editPatient } from './AdminApi';
import ValidationHelpers from '../../tools/ValidationHelpers';
import { activeOptions } from '../../tools/ActiveOptions';
import { ErrorSnackbar, SuccessSnackbar } from '../Snackbars';

const theme = createTheme();

export default function EditPatient() {
    const navigate = useNavigate();
    const location = useLocation();

    const [peselError, setPeselError] = useState('')
    const [peselErrorState, setPeselErrorState] = useState(false)
    const [firstNameError, setFirstNameError] = useState('')
    const [firstNameErrorSate, setFirstNameErrorState] = useState(false)
    const [lastNameError, setLastNameError] = useState('')
    const [lastNameErrorState, setLastNameErrorState] = useState(false)
    const [mailError, setMailError] = useState('')
    const [mailErrorState, setMailErrorState] = useState(false)
    //const [dateOfBirth, setDateOfBirth] = useState(location.state != null ? location.state.dateOfBirth ? location.state.dateOfBirth.replaceAll('-', '/') : '' : '')
    const [dateOfBirthError, setDateOfBirthError] = useState('')
    const [dateOfBirthErrorState, setDateOfBirthErrorState] = useState(false)
    const [phoneNumberError, setPhoneNumberError] = useState('')
    const [phoneNumberErrorState, setPhoneNumberErrorState] = useState(false)
    const [operationError, setOperationError] = useState('');
    const [operationErrorState, setOperationErrorState] = useState(false);
    const [activeOption, setActiveOption] = React.useState(location.state != null ? location.state.active ? 'aktywny' : 'nieaktywny' : '');
    const [success, setSuccess] = useState(false);

    const handleSubmit = async (event) => {
        event.preventDefault();
        const data = new FormData(event.currentTarget);

        let error = await editPatient(location.state.id, data.get('pesel'), data.get('firstName'), data.get('lastName'),
            data.get('mail'), data.get('dateOfBirth'), data.get('phoneNumber'), data.get('active'));
        setOperationError(error);
        if (error != '200')
            setOperationErrorState(true);
        else
            setSuccess(true);
    };

    const handleChange = (event) => {
        setActiveOption(event.target.value);
    };

    return (
        <ThemeProvider theme={theme}>
            <Container component="main" maxWidth="xs">
                <CssBaseline />
                <Box
                    sx={{
                        marginTop: 8,
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                    }}
                >
                    <Typography component="h1" variant="h5">
                        Wpisz dane pacjenta
                    </Typography>
                    <Box component="form" noValidate onSubmit={handleSubmit} sx={{ mt: 3 }}>
                        <Grid container spacing={2}>
                            <Grid item xs={12}>
                                <TextField
                                    defaultValue={location.state != null ? location.state.pesel : null}
                                    required
                                    fullWidth
                                    name="pesel"
                                    label="PESEL"
                                    id="pesel"
                                    onChange={(e) => {
                                        ValidationHelpers.validatePESEL(e, setPeselError, setPeselErrorState)
                                    }}
                                    helperText={peselError}
                                    error={peselErrorState}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    defaultValue={location.state != null ? location.state.firstName : null}
                                    required
                                    fullWidth
                                    name="firstName"
                                    label="Imię"
                                    id="firstName"
                                    onChange={(e) => {
                                        ValidationHelpers.validateName(e, setFirstNameError, setFirstNameErrorState)
                                    }}
                                    helperText={firstNameError}
                                    error={firstNameErrorSate}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    defaultValue={location.state != null ? location.state.lastName : null}
                                    required
                                    fullWidth
                                    name="lastName"
                                    label="Nazwisko"
                                    id="lastName"
                                    onChange={(e) => {
                                        ValidationHelpers.validateLastName(e, setLastNameError, setLastNameErrorState)
                                    }}
                                    helperText={lastNameError}
                                    error={lastNameErrorState}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    defaultValue={location.state != null ? location.state.mail : null}
                                    required
                                    fullWidth
                                    name="mail"
                                    label="E-Mail"
                                    id="mail"
                                    onChange={(e) => {
                                        ValidationHelpers.validateEmail(e, setMailError, setMailErrorState)
                                    }}
                                    helperText={mailError}
                                    error={mailErrorState}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    defaultValue={location.state != null ? location.state.dateOfBirth : null}
                                    required
                                    fullWidth
                                    name="dateOfBirth"
                                    label="Data urodzenia"
                                    id="dateOfBirth"
                                    onChange={(e) => {
                                        ValidationHelpers.validateDate(e, setDateOfBirthError, setDateOfBirthErrorState)
                                    }}
                                    helperText={dateOfBirthError}
                                    error={dateOfBirthErrorState}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    defaultValue={location.state != null ? location.state.phoneNumber : null}
                                    required
                                    fullWidth
                                    name="phoneNumber"
                                    label="Numer telefonu"
                                    id="phoneNumber"
                                    onChange={(e) => {
                                        ValidationHelpers.validatePhoneNumber(e, setPhoneNumberError, setPhoneNumberErrorState)
                                    }}
                                    helperText={phoneNumberError}
                                    error={phoneNumberErrorState}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    fullWidth
                                    id="active"
                                    select
                                    label="Aktywny"
                                    name="active"
                                    value={activeOption}
                                    onChange={handleChange}
                                    SelectProps={{
                                        native: true,
                                    }}
                                >
                                    {activeOptions.map((option) => (
                                        <option key={option.value} value={option.value}>
                                            {option.label}
                                        </option>
                                    ))}
                                </TextField>
                            </Grid>
                        </Grid>
                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                        >
                            Zatwierdź
                        </Button>
                    </Box>
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        sx={{ mt: 3, mb: 2 }}
                        onClick={() => { navigate("/admin/patients") }}
                    >
                        Powrót
                    </Button>
                    <ErrorSnackbar
                        error={operationError}
                        errorState={operationErrorState}
                        setErrorState={setOperationErrorState}
                    />
                    <SuccessSnackbar
                        success={success}
                        setSuccess={setSuccess}
                    />
                </Box>
            </Container>
        </ThemeProvider>

    )
}