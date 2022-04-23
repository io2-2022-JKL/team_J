import axios from 'axios';

export async function getFreeTimeSlots(city, virus, dateFrom, dateTo) {

    let response;
    try {
        response = await axios({
            method: 'post',
            url: 'https://systemszczepien.azurewebsites.net/patient/timeSlots/Filter',

            data: {
                city: "Warszawa",
                dateFrom: "01-01-2022 10:00",
                dateTo: "01-10-2022 10:00",
                virus: "Koronawirus"
            }
        });
        console.log({
            response,
        })
    } catch (error) {
        console.error(error.message);
    }
}
