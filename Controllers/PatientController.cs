using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BackOffice.Application.Patients;
using BackOffice.Domain.Patients;

namespace BackOffice.Controllers
{
    [ApiController]
    [Route("patient")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly PatientService _patientService;

        public PatientController(IPatientRepository patientRepository, PatientService patientService)
        {
            _patientRepository = patientRepository;
            _patientService = patientService;
        }

        [HttpPost("create")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePatientAsync([FromBody] PatientDto patientDto)
        {
            if (patientDto == null)
            {
                return BadRequest(new { success = false, message = "Patient details are required." });
            }

            try
            {
                // Use the PatientService to create the patient
                var patientDataModel = await _patientService.CreatePatientAsync(patientDto);
                return Ok(new { success = true, patient = patientDataModel });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetAllPatientsAsync(
            [FromQuery] string? userId = null,
            [FromQuery] string? firstName = null,
            [FromQuery] string? lastName = null,
            [FromQuery] string? fullName = null)
        {
            try
            {
                // Create a filter DTO from the query parameters
                var filterDto = new PatientFilterDto(userId, firstName, lastName, fullName);

                var patients = await _patientService.GetFilteredPatientsAsync(filterDto); // Filter based on dto
                return Ok(new { success = true, patients });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdatePatientAsync([FromBody] PatientDto patientDto)
        {
            if (patientDto == null || string.IsNullOrWhiteSpace(patientDto.RecordNumber))
            {
                return BadRequest(new { success = false, message = "Patient details including RecordNumber are required." });
            }

            try
            {
                var updatedPatient = await _patientService.UpdateAsync(patientDto);
                return Ok(new { success = true, patient = updatedPatient });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("markToDelete/{recordNumber}")]
        public async Task<IActionResult> MarkPatientToDeleteAsync([FromRoute] string recordNumber)
        {
            try
            {
                // Convert the string recordNumber to the RecordNumber domain object
                var recordNumObj = new RecordNumber(recordNumber);

                // Use the PatientService to mark the patient for deletion
                var patientDataModel = await _patientService.MarkToDelete(recordNumObj);
                return Ok(new { success = true, patient = patientDataModel });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeletePatientAsync([FromBody] string recordNumber)
        {
            if (string.IsNullOrWhiteSpace(recordNumber))
            {
                return BadRequest(new { success = false, message = "Patient details are required." });
            }

            try
            {
                // Convert the string recordNumber to the RecordNumber domain object
                var recordNumObj = new RecordNumber(recordNumber);

                // Use the PatientService to delete the patient
                var patientDataModel = await _patientService.DeletePatientAsync(recordNumObj);
                return Ok(new { success = true, patient = patientDataModel });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
