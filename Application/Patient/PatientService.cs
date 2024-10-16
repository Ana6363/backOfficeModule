using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackOffice.Application.Patients;
using BackOffice.Domain.Patients;
using BackOffice.Application.Users;
using BackOffice.Domain.Users;
using BackOffice.Infrastructure.Patients;
using BackOffice.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Application.Patients
{
    public class PatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly UserService _userService;
        private readonly BackOfficeDbContext _dbContext;

        public PatientService(IPatientRepository patientRepository, UserService userService, BackOfficeDbContext dbContext)
        {
            _patientRepository = patientRepository;
            _userService = userService;
            _dbContext = dbContext;
        }

public async Task<PatientDataModel> CreatePatientAsync(PatientDto patientDto)
{
    var existingUser = await _userService.GetByIdAsync(new UserId(patientDto.UserId));
    if (existingUser != null)
    {
        throw new Exception("User is already registered in the database.");
    }
    
    var existingPatientWithPhoneNumber = await _dbContext.Patients
        .FirstOrDefaultAsync(p => p.PhoneNumber == patientDto.PhoneNumber);

    if (existingPatientWithPhoneNumber != null)
    {
        throw new Exception("A patient with this phone number already exists.");
    }

    var userDto = new UserDto
    {
        Id = patientDto.UserId,
        Role = "Patient",
        Active = false,
        ActivationToken = null,
        TokenExpiration = null,
        FirstName = patientDto.FirstName,
        LastName = patientDto.LastName,
        FullName = patientDto.FullName
    };

    await _userService.AddAsync(userDto);

    // Generate the next sequential RecordNumber
        var latestPatient = await _dbContext.Patients
            .OrderByDescending(p => p.RecordNumber)
            .FirstOrDefaultAsync();

        int nextRecordNumber = latestPatient != null
            ? int.Parse(latestPatient.RecordNumber) + 1
            : 1;

        string generatedRecordNumber = nextRecordNumber.ToString("D5"); // Always 5 digits

        // Ensure the generated RecordNumber is set
        patientDto.RecordNumber = generatedRecordNumber;

    var patient = PatientMapper.ToDomain(patientDto);

    if (patient == null)
    {
        Console.WriteLine("Mapped patient object is null.");
        throw new Exception("Error mapping the patient object.");
    }

    try
    {
        return await _patientRepository.AddAsync(patient);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error adding patient: {ex.Message}");
        throw;
    }
}



    public async Task<IEnumerable<PatientDataModel>> GetAllPatientsAsync()
        {
            return await _patientRepository.GetAllAsync();
        }
    }
}