import axios from 'axios';
import Moment from 'moment';
import { SYSTEM_SZCZEPIEN_URL } from '../../api/Api';

export async function getFreeTimeSlots(city, dateFrom, dateTo, virus) {

    //console.log("dateTo", Moment(dateTo).format('DD-MM-YYYY hh:mm'))
    //console.log("dateFrom", Moment(dateFrom).format('DD-MM-YYYY hh:mm'))

    let response;
    let errCode = '200'
    try {
        response = await axios({
            method: 'get',
            url: SYSTEM_SZCZEPIEN_URL + '/patient/timeSlots/Filter',

            params: {
                city: city,
                dateFrom: Moment(dateFrom).format('DD-MM-YYYY hh:mm'),
                dateTo: Moment(dateTo).format('DD-MM-YYYY hh:mm'),
                virus: virus
                //city: city,
                //dateFrom: Moment(dateFrom).format('DD-MM-YYYY'),
                //dateTo: Moment(dateTo).format('DD-MM-YYYY'),
                //virus: virus
            }
        });
        console.log(
            "request succueeded"
        )
        return [response.data, errCode];
    } catch (error) {
        console.error(error.message);
        if (error.response != null)
            return [response, error.response.status.toString()];
        return [response, error.code];
    }

    //return response;
}

export async function bookTimeSlot(timeSlot, vaccine) {

    let patientId = localStorage.getItem('userID')
    let timeSlotId = timeSlot.timeSlotId
    let vaccineId = vaccine.vaccineId

    console.log(patientId, timeSlotId, vaccineId)

    let errCode = '200'

    try {
        await axios({
            method: 'post',
            url: SYSTEM_SZCZEPIEN_URL + '/patient/timeSlots/Book/' + patientId + '/' + timeSlotId + '/' + vaccineId,
        });
        return [errCode];
    } catch (error) {
        console.error(error.message);
        if (error.response != null)
            return error.response.status.toString();
        return error.code;
    }
}

export async function getFormerAppointments(patientId) {

    let response;
    let err = '200';
    try {
        response = await axios({
            method: 'get',
            url: SYSTEM_SZCZEPIEN_URL + '/patient/appointments/formerAppointments/' + patientId,
        });
        /*
        console.log({
            data: response.data,
        })
        */
        return [response.data, err];
    } catch (error) {
        console.error(error.message);
        if (error.response != null)
            return [response, error.response.status.toString()];
        return [response, error.code];
    }
}

export async function getIncomingAppointments(patientId) {

    let response;
    let err = '200'
    try {
        response = await axios({
            method: 'get',
            url: SYSTEM_SZCZEPIEN_URL + '/patient/appointments/incomingAppointments/' + patientId,
        });
        /*
        console.log({
            data: response.data,
        })
        */
        return [response.data, err];
    } catch (error) {
        console.error(error.message);
        if (error.response != null)
            return [response, error.response.status.toString()];
        return [response, error.code];
    }
}

export async function cancelAppointment(patientId, appointmentId) {
    let err = '200'
    try {
        await axios({
            method: 'delete',
            url: SYSTEM_SZCZEPIEN_URL + '/patient/appointments/incomingAppointments/cancelAppointments/' + patientId + '/' + appointmentId,
        });
        return err;
    } catch (error) {
        console.error(error.message);
        if (error.response != null)
            return error.response.status.toString();
        return error.code;
    }

}

export async function getCertificates(patientId) {

    let response;
    let error = '200';
    try {
        response = await axios({
            method: 'get',
            url: SYSTEM_SZCZEPIEN_URL + '/patient/certificates/' + patientId,
        });
        return [response.data, error];
    } catch (error) {
        console.error(error.message);
        if (error.response != null)
            return [response, error.response.status.toString()];
        return [response, error.code];
    }
}

export async function getPatientInfo(patientId) {
    let response;
    let errCode = '200';
    try {
        response = await axios({
            method: 'get',
            url: SYSTEM_SZCZEPIEN_URL + '/patient/info/' + patientId,
        });
        return [response.data, errCode];
    } catch (error) {
        console.error(error.message);
        if (error.response != null)
            return [response, error.response.status.toString()];
        return [response, error.code];
    }
}
