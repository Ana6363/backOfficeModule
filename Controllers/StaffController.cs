using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BackOffice.Domain.Staff;
using BackOffice.Infrastructure.Staff;
using BackOffice.Application.StaffService;
using Microsoft.Extensions.Configuration;

namespace BackOffice.Controllers
{
    [ApiController]
    [Route("staff")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffRepository _staffRepository;
        private readonly StaffService _staffService;
        private readonly IConfiguration _configuration;

        public StaffController(IStaffRepository staffRepository, StaffService staffService, IConfiguration configuration)
        {
            _staffRepository = staffRepository;
            _staffService = staffService;
            _configuration = configuration;
        }

        [HttpPost("create")]
            public async Task<IActionResult> CreateStaffAsync([FromBody] StaffDto staffDto)
            {
                if (staffDto == null)
                {
                    return BadRequest(new { success = false, message = "Staff details are required." });
                }

                try
                {
                    var staffDataModel = await _staffService.CreateStaffAsync(staffDto, _configuration);
                    return Ok(new { success = true, staff = staffDataModel });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }

        [HttpGet("filter")]
            public async Task<IActionResult> GetAllStaffAsync(
                [FromQuery] int? phoneNumber = null,
                [FromQuery] string? firstName = null,
                [FromQuery] string? lastName = null,
                [FromQuery] string? fullName = null)
            {
                try
                {
                    var filterDto = new StaffFilterDto(phoneNumber, firstName, lastName, fullName);

                    var staffMembers = await _staffService.GetFilteredStaffAsync(filterDto);
                    return Ok(new { success = true, staffMembers });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }

        [HttpPut("deactivate")]
        public async Task<IActionResult> DeactivateStaffAsync([FromBody] LicenseNumber licenseNumber)
        {
            if (licenseNumber == null)
            {
                return BadRequest(new { success = false, message = "Staff lincense number is required." });
            }

            try
            {
                var staffDataModel = await _staffService.DeactivateStaff(licenseNumber);
                return Ok(new { success = true, staff = staffDataModel });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

    }
}