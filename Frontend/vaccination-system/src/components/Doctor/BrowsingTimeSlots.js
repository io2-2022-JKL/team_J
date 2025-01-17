import * as React from 'react';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import { Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { FixedSizeList } from 'react-window';
import Button from '@mui/material/Button';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import { Container, CssBaseline } from '@mui/material';
import Typography from '@mui/material/Typography';
import { deleteTimeSlots, getTimeSlots } from './DoctorApi';
import CircularProgress from '@mui/material/CircularProgress';
import { blue } from '@mui/material/colors';
import SummarizeIcon from '@mui/icons-material/Summarize';
import Avatar from '@mui/material/Avatar';
import { ErrorSnackbar, SuccessSnackbar } from '../Snackbars';

const theme = createTheme();

export default function BrowsingTimeSLots() {
    const navigate = useNavigate();
    const [data, setData] = React.useState([]);
    const [loading, setLoading] = React.useState(false);
    const [errorMessage, setError] = React.useState('');
    const [errorState, setErrorState] = React.useState(false);
    const [cancelError, setErrorCancel] = React.useState('');
    const [errorCancelState, setErrorCancelState] = React.useState(false);
    const [success, setSuccess] = React.useState(false);
    let userID = localStorage.getItem('doctorID');

    React.useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            let [doctorData, err] = await getTimeSlots(userID);
            if (doctorData != null) {
                setData(doctorData);
                console.log(doctorData)
            }
            else {
                setData([]);
                setError(err);
                setErrorState(true);
            }
            setLoading(false);
        }
        fetchData();
        console.log("run useEffect")
    }, [cancelError]);

    function renderRow(props) {
        const { index, style, data } = props;
        const item = data[index];
        let dateFrom = item.from.substring(0, 10)
        let from = item.from.substring(11, 16)
        let dateTo = item.to.substring(0, 10)
        let to = item.to.substring(11, 16)
        return (
            <ListItem style={style} key={index} component="div" disablePadding divider>
                <ListItemText primary={"Data: " + dateFrom + " godziny: " + from + " - " + to} secondary={"Wolne: " + (item.isFree ? "Tak" : "Nie")} />
                <Button
                    onClick={async () => {
                        let timeSlotId = item.id;
                        const result = window.confirm("Czy na pewno chcesz usunąć okno?", confirmOptionsInPolish);
                        if (result) {
                            console.log("You click yes!");
                            let err = await deleteTimeSlots(userID, [timeSlotId]);
                            setErrorCancel(err);
                            if (err !== '200') {
                                setErrorCancelState(true);
                            }
                            else
                                setSuccess(true);
                            return;
                        }
                        else
                            console.log("You click No!");
                    }}
                >
                    Usuń okno
                </Button>
                <Button
                    onClick={async () => {
                        let timeSlotId = item.id;
                        navigate("/doctor/timeSlots/modify", { state: { doctorId: userID, timeSlotId: timeSlotId, dateFrom: dateFrom, timeFrom: from, dateTo: dateTo, timeTo: to, action: "modify" } })
                    }}
                >
                    Modyfiukuj okno
                </Button>
            </ListItem>
        );
    }
    return (
        <ThemeProvider theme={theme}>
            <Container component="main" maxWidth="lg">
                <CssBaseline>
                    <Box
                        sx={{
                            marginTop: 2,
                            display: 'flex',
                            flexDirection: 'column',
                            alignItems: 'center',
                        }}
                    >
                        <Avatar sx={{ m: 1, bgcolor: 'secondary.main' }}>
                            <SummarizeIcon />
                        </Avatar>
                        <Typography component="h1" variant='h5'>
                            Okna godzinowe
                        </Typography>

                        <Box sx={{ marginTop: 2, }} />

                        <FixedSizeList
                            height={Math.min(window.innerHeight - 200, data.length * 100)}
                            width="70%"
                            itemSize={100}
                            itemCount={data.length}
                            overscanCount={5}
                            itemData={data}
                        >
                            {renderRow}
                        </FixedSizeList>
                        <Button
                            type="submit"
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                            onClick={() => { navigate("/doctor/timeSlots/create", { state: { doctorId: userID, action: "create" } }) }}
                        >
                            Dodaj nowe okna godzinowe
                        </Button>
                        <Button
                            type="submit"
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                            onClick={() => { navigate("/doctor/redirection", { state: { page: "doctor" } }) }}
                        >
                            Powrót
                        </Button>
                        {
                            loading &&
                            (
                                <CircularProgress
                                    size={24}
                                    sx={{
                                        color: blue,
                                        position: 'relative',
                                        alignSelf: 'center',
                                        left: '50%'
                                    }}
                                />
                            )
                        }
                        <ErrorSnackbar
                            error={errorMessage}
                            errorState={errorState}
                            setErrorState={setErrorState}
                        />
                        <ErrorSnackbar
                            error={cancelError}
                            errorState={errorCancelState}
                            setErrorState={setErrorCancelState}
                        />
                        <SuccessSnackbar
                            success={success}
                            setSuccess={setSuccess}
                        />
                    </Box>
                </CssBaseline>
            </Container>
        </ThemeProvider>
    );
}

const confirmOptionsInPolish = {
    labels: {
        confirmable: "Tak",
        cancellable: "Nie"
    }
}