﻿using System.Security.Claims;
using BackOffice.Application.Appointement;
using BackOffice.Application.Logs;
using BackOffice.Domain.Appointement;
using BackOffice.Domain.Logs;
using BackOffice.Domain.OperationRequest;
using BackOffice.Domain.Shared;
using BackOffice.Domain.Users;
using BackOffice.Infraestructure.OperationRequest;
using BackOffice.Infrastructure;
using BackOffice.Infrastructure.Staff;
using Healthcare.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.Application.OperationRequest
{
    public class OperationRequestService
    {
        private readonly IAppointementRepository _appointementRepository;
        private readonly BackOfficeDbContext _context;
        private readonly IOperationRequestRepository _operationRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SurgeryRoomService _surgeryRoomService;

        public OperationRequestService(IOperationRequestRepository operationRequestRepository,
            IAppointementRepository appointementRepository, IUnitOfWork unitOfWork, BackOfficeDbContext context, 
            IUserRepository userRepository, IStaffRepository staffRepository, IHttpContextAccessor httpContextAccessor, SurgeryRoomService surgeryRoomService)
        {
            _operationRequestRepository = operationRequestRepository;
            _appointementRepository = appointementRepository;
            _unitOfWork = unitOfWork;
            _context = context;
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _httpContextAccessor = httpContextAccessor;
            _surgeryRoomService = surgeryRoomService;
        }

        public async Task<OperationRequestDataModel> CreateOperationRequestAsync(OperationRequestDto operationRequest)
        {

            var requestDto = new OperationRequestDto(
                Guid.NewGuid(),
                operationRequest.DeadLine,
                operationRequest.Priority,
                operationRequest.RecordNumber,
                operationRequest.StaffId,
                Status.StatusType.PENDING.ToString(),
                operationRequest.OperationTypeName
            );

            Console.WriteLine($"RecordNumber in DTO: {requestDto.RecordNumber}"); // Before conversion

            var request = OperationRequestMapper.ToDomain(requestDto);
            Console.WriteLine($"RecordNumber in Domain: {request.Patient.AsString()}"); // After conversion
            if(request == null)
            {
                Console.WriteLine("Error mapping");
                throw new Exception("Request is null");
            }

            try
            {
                return await _operationRequestRepository.AddAsync(request);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error adding to repo: {e.Message}");
                throw;
            }

        }
      
    public async Task<IEnumerable<OperationRequestDataModel>> GetFilteredRequestAsync(FilteredRequestDto filteredRequest)
{
    var query = _context.OperationRequests.AsQueryable();

    if (!string.IsNullOrWhiteSpace(filteredRequest.Priority))
    {
        query = query.Where(r => r.Priority.Contains(filteredRequest.Priority));
    }

    if (!string.IsNullOrWhiteSpace(filteredRequest.Status))
    {
        query = query.Where(r => r.Status.Contains(filteredRequest.Status));
    }

    if (!string.IsNullOrWhiteSpace(filteredRequest.StaffId))
    {
        query = query.Where(r => r.StaffId.Contains(filteredRequest.StaffId));
    }

    var result = await query.ToListAsync();
    return result;
}



    public async Task<OperationRequestDto> UpdateAsync(OperationRequestDto updatedRequestDto)
        {
            var existingRequest = await _context.OperationRequests
                .FirstOrDefaultAsync(r => r.RequestId == updatedRequestDto.RequestId);

            if (existingRequest == null)
            {
                throw new Exception("Operation request not found.");
            }

          /*  var doctor = await _context.Staff
                .FirstOrDefaultAsync(s => s.StaffId == updatedRequestDto.StaffId);

            if (doctor == null)
            {
                throw new Exception("Doctor not found.");
            }

            var loggedInUserEmail = GetLoggedInUserEmail();
            var loggedInUserId = loggedInUserEmail.Split('@')[0];

           if (!loggedInUserId.Equals(existingRequest.StaffId, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Only the requesting doctor can update this operation request.");
            } */

            bool isUpdated = false;

            if (updatedRequestDto.DeadLine != existingRequest.DeadLine)
            {
                existingRequest.DeadLine = updatedRequestDto.DeadLine;
                isUpdated = true;
            }

            if (updatedRequestDto.Priority != existingRequest.Priority)
            {
                existingRequest.Priority = updatedRequestDto.Priority;
                isUpdated = true;
            }
            if (updatedRequestDto.Status != existingRequest.Status)
            {
                existingRequest.Status= updatedRequestDto.Status;
                isUpdated = true;
            }

            if (isUpdated)
            {
           //     await LogUpdateOperation(loggedInUserEmail, updatedRequestDto);
                await _context.SaveChangesAsync();
            }


            var updatedRequest = OperationRequestMapper.ToDomain(updatedRequestDto);
            return updatedRequestDto;
        }

    private string GetLoggedInUserEmail()
        {
            var claimsIdentity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var emailClaim = claimsIdentity.FindFirst(ClaimTypes.Email);
                if (emailClaim != null)
                {
                    Console.WriteLine(emailClaim.Value);
                    return emailClaim.Value;
                }
            }

            throw new Exception("User email not found in token.");
        }

// Método para registrar a atualização
private async Task LogUpdateOperation(string staffEmail, OperationRequestDto updatedRequestDto)
{
    var log = new Log(
        new LogId(Guid.NewGuid().ToString()),
        new ActionType(ActionTypeEnum.Update),
        new Email(staffEmail),
        new Text($"Operation request {updatedRequestDto.RequestId} updated by doctor {staffEmail} at {DateTime.UtcNow}.")
    );

    var logDataModel = LogMapper.ToDataModel(log);
    await _context.Logs.AddAsync(logDataModel);
    await _context.SaveChangesAsync();
}

public async Task<DeleteRequestDto> DeleteOperationRequestAsync(DeleteRequestDto updatedRequestDto)
{
    var existingRequest = await _context.OperationRequests
        .FirstOrDefaultAsync(r => r.RequestId == updatedRequestDto.RequestId);

    if (existingRequest == null)
    {
        throw new Exception("Operation request not found.");
    }


    var doctor =  await _context.Staff
        .FirstOrDefaultAsync(s => s.StaffId == existingRequest.StaffId);

    if (doctor == null)
    {
        throw new Exception("Doctor not found.");
    }
    
    var doctorEmail = await _context.Users
    .Where(u => u.Id == doctor.Email)
    .Select(u => u.Id)
    .FirstOrDefaultAsync();

    var truncatedDoctorEmail = doctorEmail?.Split('@').FirstOrDefault();

    if (!existingRequest.StaffId.Equals(truncatedDoctorEmail, StringComparison.OrdinalIgnoreCase))
    {
        throw new Exception("Only the requesting doctor can delete this operation request.");
    }
    var apointment = await _context.Appointements
        .FirstOrDefaultAsync(a => a.Request == updatedRequestDto.RequestId.ToString());

    if (apointment != null){
        throw new Exception("Operation request cannot be deleted because it is associated with an appointment.");
    }

    await LogDeleteOperation(doctorEmail, updatedRequestDto);
    _context.OperationRequests.Remove(existingRequest);
    await _context.SaveChangesAsync();

    return updatedRequestDto;
}

private async Task LogDeleteOperation(string doctorEmail, DeleteRequestDto updatedRequestDto){
    var log = new Log(
        new LogId(Guid.NewGuid().ToString()),
        new ActionType(ActionTypeEnum.Delete),
        new Email(doctorEmail),
        new Text($"Operation request {updatedRequestDto.RequestId} deleted by doctor {doctorEmail} at {DateTime.UtcNow}.")
    );

    var logDataModel = LogMapper.ToDataModel(log);
    await _context.Logs.AddAsync(logDataModel);
    await _context.SaveChangesAsync();
}
    
    }
}
