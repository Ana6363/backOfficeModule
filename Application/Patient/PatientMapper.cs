using BackOffice.Domain.Patients;
using BackOffice.Domain.Shared;
using BackOffice.Domain.Users;
using BackOffice.Infrastructure.Patients;

namespace BackOffice.Application.Patients
{
    public static class PatientMapper
    {
        
        public static PatientDto ToDto(Patient patient)
        {
            if (patient == null) 
                throw new ArgumentNullException(nameof(patient), "Patient cannot be null.");

            return new PatientDto(
                patient.RecordNumber.AsString(), 
                patient.DateOfBirth.Value, 
                patient.PhoneNumber.Number, 
                patient.EmergencyContact.Number,
                patient.Gender?.ToString() ?? "UNKNOWN",
                patient.UserId.AsString()
            );
        }

       public static Patient ToDomain(PatientDto patientDto)
{
    if (patientDto == null) 
        throw new ArgumentNullException(nameof(patientDto), "PatientDto cannot be null.");

    if (!Enum.TryParse<Gender.GenderType>(patientDto.Gender, true, out var genderType))
    {
        throw new ArgumentException("Invalid gender", nameof(patientDto.Gender));
    }

    return new Patient(
        new RecordNumber(patientDto.RecordNumber),
        new UserId(patientDto.UserId),
        new DateOfBirth(patientDto.DateOfBirth),
        new PhoneNumber(patientDto.PhoneNumber),
        new PhoneNumber(patientDto.EmergencyContact),
        new Gender(genderType)
    );
}


        
        public static PatientDataModel ToDataModel(Patient patient)
        {
            if (patient == null) 
                throw new ArgumentNullException(nameof(patient), "Patient cannot be null.");

            return new PatientDataModel
            {
                RecordNumber = patient.Id.AsString(),
                UserId = patient.UserId.AsString(),
                DateOfBirth = patient.DateOfBirth.Value,
                PhoneNumber = patient.PhoneNumber.Number,
                EmergencyContact = patient.EmergencyContact.Number,
                Gender = patient.Gender?.ToString() ?? "UNKNOWN" // This will now return "MALE" or "FEMALE"
            };
        }

        public static Patient ToDomain(PatientDataModel dataModel)
        {
            if (dataModel == null) 
                throw new ArgumentNullException(nameof(dataModel), "PatientDataModel cannot be null.");

            // Validate and parse the gender
            if (!Enum.TryParse<Gender.GenderType>(dataModel.Gender, true, out var genderType))
            {
                throw new ArgumentException("Invalid gender in data model", nameof(dataModel.Gender));
            }

            return new Patient(
                new RecordNumber(dataModel.RecordNumber),
                new UserId(dataModel.UserId), // Assuming UserId comes from data model
                new DateOfBirth(dataModel.DateOfBirth),
                new PhoneNumber(dataModel.PhoneNumber), // Using PhoneNumber for PhoneNumber
                new PhoneNumber(dataModel.EmergencyContact), // Using PhoneNumber for EmergencyContact
                new Gender(genderType) // Create a new Gender instance with the parsed gender type
            );
        }
    }
}
