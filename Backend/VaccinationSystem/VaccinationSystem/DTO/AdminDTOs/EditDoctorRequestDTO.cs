﻿namespace VaccinationSystem.DTO.AdminDTOs
{
    public class EditDoctorRequestDTO
    {
        public string patientID { get; set; }
        public string PESEL { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string mail { get; set; }
        public string dateOfBirth { get; set; }
        public string phoneNumber { get; set; }
        public string vaccinationCenterId { get; set; }
    }
}