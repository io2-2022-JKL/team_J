﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VaccinationSystem.DTO.DoctorDTOs;
using VaccinationSystem.DTO;
using VaccinationSystem.Config;
using VaccinationSystem.Models;
using System.Data.Entity;
using VaccinationSystem.DTO.Errors;

namespace VaccinationSystem.Controllers
{
    [ApiController]
    [Route("doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly VaccinationSystemDbContext _context;
        private readonly string _dateTimeFormat = "dd-MM-yyyy HH\\:mm";
        private readonly string _dateFormat = "dd-MM-yyyy";

        public DoctorController(VaccinationSystemDbContext context)
        {
            _context = context;
        }
        [HttpGet("info/{doctorId}")]
        public ActionResult<GetDoctorInfoResponse> GetDoctorInfo(string doctorId)
        {
            // TODO: Token verification for 401 and 403 error codes
            GetDoctorInfoResponse result;
            try
            {
                result = FetchDoctorPatientId(doctorId);
            }
            catch(BadRequestException)
            {
                return BadRequest();
            }
            if (result == null) return NotFound();
            return Ok(result);
        }
        private GetDoctorInfoResponse FetchDoctorPatientId(string doctorId)
        {
            Guid docId;
            try
            {
                docId = Guid.Parse(doctorId);
            }
            catch (FormatException)
            {
                throw new BadRequestException();
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            var doctorAccount = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).Include(doc => doc.VaccinationCenter).SingleOrDefault();
            if (doctorAccount == null) return null;
            Guid patientAccountId = doctorAccount.PatientId;
            GetDoctorInfoResponse result = new GetDoctorInfoResponse()
            {
                patientId = patientAccountId.ToString(),
                vaccinationCenterId = doctorAccount.VaccinationCenterId.ToString(),
                vaccinationCenterCity = doctorAccount.VaccinationCenter.City,
                vaccinationCenterName = doctorAccount.VaccinationCenter.Name,
                vaccinationCenterStreet = doctorAccount.VaccinationCenter.Address,
            };
            return result;
        }
        [HttpGet("timeSlots/{doctorId}")]
        public ActionResult<IEnumerable<ExistingTimeSlotDTO>> GetExistingTimeSlots(string doctorId)
        {
            // TODO: Token verification for 401 and 403 error codes
            IEnumerable<ExistingTimeSlotDTO> result;
            try
            {
                result = fetchExistingTimeSlots(doctorId);
            }
            catch(BadRequestException)
            {
                return BadRequest();
            }
            if (result == null || result.Count() == 0) return NotFound();
            return Ok(result);
        }
        private IEnumerable<ExistingTimeSlotDTO> fetchExistingTimeSlots(string doctorId)
        {
            Guid docId;
            try
            {
                docId = Guid.Parse(doctorId);
            }
            catch(FormatException)
            {
                throw new BadRequestException();
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return null;
            List<ExistingTimeSlotDTO> result = new List<ExistingTimeSlotDTO>();
            var timeSlots = _context.TimeSlots.Where(ts => ts.DoctorId == docId && ts.Active == true).ToList();
            foreach(TimeSlot timeSlot in timeSlots)
            {
                ExistingTimeSlotDTO existingTimeSlotDTO = new ExistingTimeSlotDTO();
                existingTimeSlotDTO.id = timeSlot.Id.ToString();
                existingTimeSlotDTO.from = timeSlot.From.ToString(_dateTimeFormat);
                existingTimeSlotDTO.to = timeSlot.To.ToString(_dateTimeFormat);
                existingTimeSlotDTO.isFree = timeSlot.IsFree;
                result.Add(existingTimeSlotDTO);
            }
            return result;
        }
        [HttpPost("timeSlots/create/{doctorId}")]
        public IActionResult CreateTimeSlots(string doctorId, CreateNewVisitsRequestDTO createNewVisitsRequestDTO)
        {
            // TODO: Token verification for 401 and 403 error codes
            if (createNewVisitsRequestDTO.timeSlotDurationInMinutes == 0) return BadRequest();
            bool result;
            try
            {
                result = createNewTimeSlots(doctorId, createNewVisitsRequestDTO);
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            if (result == false) return BadRequest();
            return Ok();
        }
        private bool createNewTimeSlots(string doctorId, CreateNewVisitsRequestDTO createNewVisitsRequestDTO)
        {
            int addedTimeSlotsCount = 0;
            Guid docId;
            DateTime currentFrom, currentTo, endTo;
            TimeSpan increment;
            try
            {
                docId = Guid.Parse(doctorId);
                currentFrom = DateTime.ParseExact(createNewVisitsRequestDTO.windowBegin, _dateTimeFormat, null);
                increment = TimeSpan.FromMinutes(createNewVisitsRequestDTO.timeSlotDurationInMinutes);
                endTo = DateTime.ParseExact(createNewVisitsRequestDTO.windowEnd, _dateTimeFormat, null);
            }
            catch(FormatException)
            {
                throw new BadRequestException();
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            Doctor doctor = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).SingleOrDefault();
            if (doctor == null) return false;
            currentTo = currentFrom + increment;
            var existingTimeSlots = _context.TimeSlots.Where(ts => ts.Active == true && ts.DoctorId == docId).ToList(); 
            while (currentTo <= endTo)
            {
                var tempResult = existingTimeSlots.Where(ts => (ts.From <= currentFrom && currentFrom < ts.To) ||
                                 (ts.From < currentTo && currentTo <= ts.To) ||
                                 (currentFrom <= ts.From && ts.To <= currentTo) ||
                                 (ts.From <= currentFrom && currentTo <= ts.To)).Count();
                if (tempResult == 0) // no colliding time slots
                {
                    var newTimeSlot = new TimeSlot();
                    newTimeSlot.Id = Guid.NewGuid();
                    newTimeSlot.From = currentFrom;
                    newTimeSlot.To = currentTo;
                    newTimeSlot.Doctor = doctor;
                    newTimeSlot.DoctorId = docId;
                    newTimeSlot.IsFree = true;
                    newTimeSlot.Active = true;
                    _context.TimeSlots.Add(newTimeSlot);
                    addedTimeSlotsCount++;
                }
                currentTo += increment;
                currentFrom += increment;
            }
            if(addedTimeSlotsCount > 0)
            {
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        [HttpPost("timeSlots/delete/{doctorId}")]
        public IActionResult DeleteTimeSlot(string doctorId, IEnumerable<string> ids)
        {
            // TODO: Token verification for 401 and 403 error codes
            bool result;
            try
            {
                result = tryDeleteTimeSlot(doctorId, ids);
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            if (result == false) return NotFound();
            return Ok();
        }

        private bool tryDeleteTimeSlot(string doctorId, IEnumerable<string> ids)
        {
            // Disallow deleting timeSlots that already passed?
            int changedTimeSlots = 0;
            List<Guid> parsedIDs = new List<Guid>();
            Guid docId;
            try
            {
                docId = Guid.Parse(doctorId);
                foreach(string id in ids)
                {
                    Guid newGuid = Guid.Parse(id);
                    parsedIDs.Add(newGuid);
                }
            }
            catch (FormatException)
            {
                throw new BadRequestException();
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return false;
            foreach (Guid id in parsedIDs)
            {
                var tempTimeSlot = _context.TimeSlots.Where(ts => ts.DoctorId == docId && ts.Id == id && ts.Active == true).SingleOrDefault();
                if (tempTimeSlot == null) continue;
                var possibleAppointment = this._context.Appointments.Where(a => a.TimeSlotId == tempTimeSlot.Id && a.State == Models.AppointmentState.Planned).SingleOrDefault();
                if (possibleAppointment != null)
                {
                    possibleAppointment.State = Models.AppointmentState.Cancelled;
                    // TODO: Take care of patient assigned to the appointment (email)
                }
                tempTimeSlot.Active = false;
                this._context.SaveChanges();
                changedTimeSlots++;
            }
            return changedTimeSlots > 0;
        }

        [HttpPost("timeSlots/modify/{doctorId}/{timeSlotId}")]
        public IActionResult ModifyAppointment(string doctorId, string timeSlotId, ModifyTimeSlotRequestDTO modifyVisitRequestDTO)
        {
            // TODO: Token verification for 401 and 403 error codes
            bool result;
            try
            {
                result = tryModifyAppointment(doctorId, timeSlotId, modifyVisitRequestDTO);
            }
            catch(BadRequestException)
            {
                return BadRequest();
            }
            if (result == false) return NotFound();
            return Ok();
        }
        private bool tryModifyAppointment(string doctorId, string timeSlotId, ModifyTimeSlotRequestDTO modifyVisitRequestDTO)
        {
            Guid docId, tsId;
            DateTime newFrom, newTo;
            try
            {
                docId = Guid.Parse(doctorId);
                tsId = Guid.Parse(timeSlotId);
                newFrom = DateTime.ParseExact(modifyVisitRequestDTO.timeFrom, _dateTimeFormat, null);
                newTo = DateTime.ParseExact(modifyVisitRequestDTO.timeTo, _dateTimeFormat, null);
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            catch (FormatException)
            {
                throw new BadRequestException();
            }

            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return false;

            // Find the time slot to change
            var timeSlotToChange = _context.TimeSlots.Where(ts => ts.Id == tsId && ts.DoctorId == docId && ts.Active == true).SingleOrDefault();
            if (timeSlotToChange == null) return false;

            // Check if there is a collision
            var collidingTimeSlot = _context.TimeSlots.Where(ts => ts.DoctorId == docId && ts.Active == true && 
                                ((ts.From <= newFrom && newFrom < ts.To) ||
                                 (ts.From < newTo && newTo <= ts.To) ||
                                 (newFrom <= ts.From && ts.To <= newTo) ||
                                 (ts.From <= newFrom && newTo <= ts.To)) && ts.Id != tsId).ToList();
            // All time slots which are active, belong to this doctor, collide with new time slot start and end times and are NOT the time slot we're changing
            if (collidingTimeSlot == null || collidingTimeSlot.Count == 0) throw new BadRequestException(); // There are collisions

            timeSlotToChange.From = newFrom;
            timeSlotToChange.To = newTo;
            _context.SaveChanges();
            return true;
        }
        [HttpGet("formerAppointments/{doctorId}")]
        public ActionResult<IEnumerable<DoctorFormerAppointmentDTO>> GetFormerAppointments(string doctorId)
        {
            // TODO: Token verification for 401 and 403 error codes
            IEnumerable<DoctorFormerAppointmentDTO> result;
            try
            {
                result = fetchFormerAppointments(doctorId);
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            if (result == null || result.Count() == 0) return NotFound();
            return Ok(result);
        }
        private IEnumerable<DoctorFormerAppointmentDTO> fetchFormerAppointments(string doctorId)
        {
            Guid docId;
            try
            {
                docId = Guid.Parse(doctorId);
            }
            catch(FormatException)
            {
                throw new BadRequestException();
            }
            catch(ArgumentNullException)
            {
                throw new BadRequestException();
            }

            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return null;

            List<DoctorFormerAppointmentDTO> result = new List<DoctorFormerAppointmentDTO>();
            var appointments = _context.Appointments.Where(ap => ap.State != Models.AppointmentState.Planned).Include(ap => ap.TimeSlot).Include(ap => ap.Patient).Include(ap => ap.Vaccine).ToList();
            foreach (Appointment appointment in appointments)
            {
                TimeSlot timeSlot = appointment.TimeSlot;
                Patient patient = appointment.Patient;
                Vaccine vaccine = appointment.Vaccine;
                if(timeSlot == null)
                {
                    timeSlot = _context.TimeSlots.Where(ts => ts.Id == appointment.TimeSlotId && ts.Active == true).SingleOrDefault();
                    if (timeSlot == null) continue;
                }
                if (patient == null)
                {
                    patient = _context.Patients.Where(pt => pt.Id == appointment.PatientId && pt.Active == true).SingleOrDefault();
                    if (patient == null) continue;
                }
                if (vaccine == null)
                {
                    vaccine = _context.Vaccines.Where(vc => vc.Id == appointment.VaccineId && vc.Active == true).SingleOrDefault();
                    if (vaccine == null) continue;
                }
                if (timeSlot.Active == false || timeSlot.DoctorId != docId) continue;
                DoctorFormerAppointmentDTO doctorFormerAppointmentDTO = new DoctorFormerAppointmentDTO();
                doctorFormerAppointmentDTO.vaccineName = vaccine.Name;
                doctorFormerAppointmentDTO.vaccineCompany = vaccine.Company;
                doctorFormerAppointmentDTO.vaccineVirus = vaccine.Virus.ToString();
                doctorFormerAppointmentDTO.whichVaccineDose = appointment.WhichDose;
                doctorFormerAppointmentDTO.appointmentId = appointment.Id.ToString();
                doctorFormerAppointmentDTO.patientFirstName = patient.FirstName;
                doctorFormerAppointmentDTO.patientLastName = patient.LastName;
                doctorFormerAppointmentDTO.PESEL = patient.PESEL;
                doctorFormerAppointmentDTO.state = appointment.State.ToString();
                doctorFormerAppointmentDTO.batchNumber = appointment.VaccineBatchNumber;
                doctorFormerAppointmentDTO.from = timeSlot.From.ToString(_dateTimeFormat);
                doctorFormerAppointmentDTO.to = timeSlot.To.ToString(_dateTimeFormat);
                doctorFormerAppointmentDTO.certifyState = appointment.CertifyState.ToString();
                result.Add(doctorFormerAppointmentDTO);
            }
            return result;
        }
        [HttpGet("incomingAppointments/{doctorId}")]
        public ActionResult<IEnumerable<DoctorIncomingAppointmentDTO>> GetIncomingAppointments(string doctorId)
        {
            // TODO: Token verification for 401 and 403 error codes
            IEnumerable<DoctorIncomingAppointmentDTO> result;
            try
            {
                result = fetchIncomingAppointments(doctorId);
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            if (result == null || result.Count() == 0) return NotFound();
            return Ok(result);
        }
        private IEnumerable<DoctorIncomingAppointmentDTO> fetchIncomingAppointments(string doctorId)
        {
            Guid docId;
            try
            {
                docId = Guid.Parse(doctorId);
            }
            catch (FormatException)
            {
                throw new BadRequestException();
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return null;

            List<DoctorIncomingAppointmentDTO> result = new List<DoctorIncomingAppointmentDTO>();
            var appointments = _context.Appointments.Where(ap => ap.State == AppointmentState.Planned).Include(ap => ap.TimeSlot).Include(ap => ap.Patient).Include(ap => ap.Vaccine).ToList();
            foreach (Appointment appointment in appointments)
            {
                TimeSlot timeSlot = appointment.TimeSlot;
                Patient patient = appointment.Patient;
                Vaccine vaccine = appointment.Vaccine;
                if (timeSlot == null)
                {
                    timeSlot = _context.TimeSlots.Where(ts => ts.Id == appointment.TimeSlotId && ts.Active == true).SingleOrDefault();
                    if (timeSlot == null) continue;
                }
                if (patient == null)
                {
                    patient = _context.Patients.Where(pt => pt.Id == appointment.PatientId && pt.Active == true).SingleOrDefault();
                    if (patient == null) continue;
                }
                if (vaccine == null)
                {
                    vaccine = _context.Vaccines.Where(vc => vc.Id == appointment.VaccineId && vc.Active == true).SingleOrDefault();
                    if (vaccine == null) continue;
                }
                if (timeSlot.Active == false || timeSlot.DoctorId != docId ||
                    patient.Active == false || vaccine.Active == false) continue;
                DoctorIncomingAppointmentDTO doctorFormerAppointmentDTO = new DoctorIncomingAppointmentDTO();
                doctorFormerAppointmentDTO.vaccineName = vaccine.Name;
                doctorFormerAppointmentDTO.vaccineCompany = vaccine.Company;
                doctorFormerAppointmentDTO.vaccineVirus = vaccine.Virus.ToString();
                doctorFormerAppointmentDTO.whichVaccineDose = appointment.WhichDose;
                doctorFormerAppointmentDTO.appointmentId = appointment.Id.ToString();
                doctorFormerAppointmentDTO.patientFirstName = patient.FirstName;
                doctorFormerAppointmentDTO.patientLastName = patient.LastName;
                doctorFormerAppointmentDTO.from = timeSlot.From.ToString(_dateTimeFormat);
                doctorFormerAppointmentDTO.to = timeSlot.To.ToString(_dateTimeFormat);
                result.Add(doctorFormerAppointmentDTO);
            }
            return result.AsEnumerable();
        }

        [HttpGet("vaccinate/{doctorId}/{appointmentId}")]
        public ActionResult<DoctorMarkedAppointmentResponseDTO> GetIncomingAppointment(string doctorId, string appointmentId)
        {
            // TODO: 401/403
            DoctorMarkedAppointmentResponseDTO result;
            try
            {
                result = fetchIncomingAppointment(doctorId, appointmentId);
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            if (result == null) return NotFound();
            return Ok(result);
        }
        private DoctorMarkedAppointmentResponseDTO fetchIncomingAppointment(string doctorId, string appointmentId)
        {
            Guid docId, apId;
            try
            {
                docId = Guid.Parse(doctorId);
                apId = Guid.Parse(appointmentId);
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            catch (FormatException)
            {
                throw new BadRequestException();
            }
            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return null;

            var appointment = _context.Appointments.Where(ap => ap.Id == apId && ap.State == AppointmentState.Planned)
                .Include(ap => ap.Patient).Include(ap => ap.TimeSlot).Include(ap => ap.Vaccine).SingleOrDefault();
            if (appointment == null) return null;
            if (appointment.Patient.Active == false || appointment.TimeSlot.Active == false || 
                appointment.Vaccine.Active == false || appointment.TimeSlot.DoctorId != docId) return null;
            DoctorMarkedAppointmentResponseDTO result = new DoctorMarkedAppointmentResponseDTO()
            {
                vaccineName = appointment.Vaccine.Name,
                vaccineCompany = appointment.Vaccine.Company,
                numberOfDoses = appointment.Vaccine.NumberOfDoses,
                minDaysBetweenDoses = appointment.Vaccine.MinDaysBetweenDoses,
                maxDaysBetweenDoses = appointment.Vaccine.MaxDaysBetweenDoses,
                virusName = appointment.Vaccine.Virus.ToString(),
                minPatientAge = appointment.Vaccine.MinPatientAge,
                maxPatientAge = appointment.Vaccine.MaxPatientAge,
                whichVaccineDose = appointment.WhichDose,
                patientFirstName = appointment.Patient.FirstName,
                patientLastName = appointment.Patient.LastName,
                PESEL = appointment.Patient.PESEL,
                dateOfBirth = appointment.Patient.DateOfBirth.ToString(_dateFormat),
                from = appointment.TimeSlot.From.ToString(_dateTimeFormat),
                to = appointment.TimeSlot.To.ToString(_dateTimeFormat),
            };
            return result;
        }

        [HttpPost("vaccinate/confirmVaccination/{doctorId}/{appointmentId}/{batchId}")]
        public ActionResult<DoctorConfirmVaccinationResponseDTO> ConfirmVaccination(string doctorId, string appointmentId, string batchId)
        {
            DoctorConfirmVaccinationResponseDTO result;
            try
            {
                result = tryConfirmVaccination(doctorId, appointmentId, batchId);
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            if (result == null) return NotFound();
            return Ok(result);
        }
        private DoctorConfirmVaccinationResponseDTO tryConfirmVaccination(string doctorId, string appointmentId, string batchId)
        {
            Guid docId, apId;
            try
            {
                docId = Guid.Parse(doctorId);
                apId = Guid.Parse(appointmentId);
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            catch (FormatException)
            {
                throw new BadRequestException();
            }
            if (batchId == null) throw new BadRequestException();
            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return null;

            var appointment = _context.Appointments.Where(ap => ap.Id == apId && ap.State == AppointmentState.Planned).Include(ap => ap.TimeSlot).Include(ap => ap.Vaccine).SingleOrDefault();
            if (appointment == null || appointment.TimeSlot.Active == false || appointment.Vaccine.Active == false || appointment.TimeSlot.DoctorId != docId) return null;

            DoctorConfirmVaccinationResponseDTO result = new DoctorConfirmVaccinationResponseDTO();
            if (appointment.WhichDose == appointment.Vaccine.NumberOfDoses) // That was the last dose for that vaccine
            {
                result.canCertify = true;
                appointment.CertifyState = CertifyState.LastNotCertified;
            }
            else
            {
                result.canCertify = false;
                appointment.CertifyState = CertifyState.NotLast;
            }

            appointment.State = AppointmentState.Finished;
            appointment.VaccineBatchNumber = batchId;
            _context.SaveChanges();
            return result;
        }

        [HttpPost("vaccinate/vaccinationDidNotHappen/{doctorId}/{appointmentId}")]
        public IActionResult VaccinationDidNotHappen(string doctorId, string appointmentId)
        {
            bool result;
            try
            {
                result = tryVaccinationDidNotHappen(doctorId, appointmentId);
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            if (result == false) return NotFound();
            return Ok();
        }
        private bool tryVaccinationDidNotHappen(string doctorId, string appointmentId)
        {
            Guid docId, apId;
            try
            {
                docId = Guid.Parse(doctorId);
                apId = Guid.Parse(appointmentId);
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            catch (FormatException)
            {
                throw new BadRequestException();
            }
            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return false;

            var appointment = _context.Appointments.Where(ap => ap.Id == apId && ap.State == AppointmentState.Planned)
                .Include(ap => ap.TimeSlot).Include(ap => ap.Vaccine).SingleOrDefault();
            if (appointment == null || appointment.TimeSlot.Active == false ||
                appointment.Vaccine.Active == false || appointment.TimeSlot.DoctorId != docId) return false;
            appointment.State = AppointmentState.Cancelled; // At least I assume so
            _context.SaveChanges();
            return true;
        }

        [HttpPost("vaccinate/certify/{doctorId}/{appointmentId}")]
        public IActionResult Certify(string doctorId, string appointmentId)
        {
            bool result;
            try
            {
                result = tryCertify(doctorId, appointmentId);
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            if (result == false) return NotFound();
            return Ok();
        }
        private bool tryCertify(string doctorId, string appointmentId)
        {
            Guid docId, apId;
            try
            {
                docId = Guid.Parse(doctorId);
                apId = Guid.Parse(appointmentId);
            }
            catch (ArgumentNullException)
            {
                throw new BadRequestException();
            }
            catch (FormatException)
            {
                throw new BadRequestException();
            }
            var checkIfDoctorActive = _context.Doctors.Where(doc => doc.Id == docId && doc.Active == true).FirstOrDefault();
            if (checkIfDoctorActive == null) return false;

            var appointment = _context.Appointments.Where(ap => ap.Id == apId && ap.State == AppointmentState.Finished && ap.CertifyState == CertifyState.LastNotCertified
            && ap.VaccineBatchNumber != null).Include(ap => ap.TimeSlot).Include(ap => ap.Patient).Include(ap => ap.Vaccine).SingleOrDefault();
            if (appointment == null || appointment.TimeSlot.Active == false || appointment.Patient.Active == false ||
                appointment.Vaccine.Active == false || appointment.TimeSlot.DoctorId != docId) return false;
            Certificate newCert = new Certificate()
            {
                Id = Guid.NewGuid(),
                Patient = appointment.Patient,
                PatientId = appointment.PatientId,
                Vaccine = appointment.Vaccine,
                VaccineId = appointment.VaccineId,
                Url = "randomFakeUrl", // to change to something proper once we get there
            };
            _context.Certificates.Add(newCert);
            appointment.CertifyState = CertifyState.Certified;
            _context.SaveChanges();
            return true;
        }
    }
}
