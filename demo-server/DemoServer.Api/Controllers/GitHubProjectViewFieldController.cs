using DemoServer.Services.Interfaces;
using DemoServer.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace DemoServer.Api.Controllers
{
    [ApiController]
    [Route("api/github/project-view-fields")]
    public class GitHubProjectViewFieldController : ControllerBase
    {
        private readonly IGitHubProjectService _projectService;

        public GitHubProjectViewFieldController(IGitHubProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Get all fields for a project view
        /// </summary>
        /// <param name="viewId">The ID of the project view</param>
        /// <returns>List of field information</returns>
        [HttpGet("{viewId}/fields")]
        public async Task<ActionResult<List<ProjectViewFieldInfo>>> GetProjectViewFields(string viewId)
        {
            try
            {
                var fields = await _projectService.GetProjectViewFieldsAsync(viewId);
                return Ok(fields);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Add a field to a project view
        /// </summary>
        /// <param name="request">Field configuration request</param>
        /// <returns>Created field information</returns>
        [HttpPost("add")]
        public async Task<ActionResult<ProjectViewFieldInfo>> AddProjectViewField([FromBody] CreateProjectViewFieldRequest request)
        {
            try
            {
                var fieldInfo = await _projectService.AddProjectViewFieldAsync(request);
                if (fieldInfo == null)
                {
                    return BadRequest(new { error = "Failed to add field to project view" });
                }
                return CreatedAtAction(nameof(GetProjectViewFields), new { viewId = request.ViewId }, fieldInfo);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update a field configuration in a project view
        /// </summary>
        /// <param name="request">Field update request</param>
        /// <returns>Updated field information</returns>
        [HttpPut("update")]
        public async Task<ActionResult<ProjectViewFieldInfo>> UpdateProjectViewField([FromBody] UpdateProjectViewFieldRequest request)
        {
            try
            {
                var fieldInfo = await _projectService.UpdateProjectViewFieldAsync(request);
                if (fieldInfo == null)
                {
                    return NotFound(new { error = "Field not found or update failed" });
                }
                return Ok(fieldInfo);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a field from a project view
        /// </summary>
        /// <param name="viewId">The ID of the project view</param>
        /// <param name="fieldId">The ID of the field to remove</param>
        /// <returns>Success status</returns>
        [HttpDelete("{viewId}/fields/{fieldId}")]
        public async Task<ActionResult<bool>> RemoveProjectViewField(string viewId, string fieldId)
        {
            try
            {
                var success = await _projectService.RemoveProjectViewFieldAsync(viewId, fieldId);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to remove field from project view" });
                }
                return Ok(success);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Set sorting for a project view
        /// </summary>
        /// <param name="request">Sort configuration request</param>
        /// <returns>Success status</returns>
        [HttpPost("sort")]
        public async Task<ActionResult<bool>> SetProjectViewSort([FromBody] CreateProjectViewSortRequest request)
        {
            try
            {
                var success = await _projectService.SetProjectViewSortAsync(request);
                return Ok(success);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update sorting for a project view
        /// </summary>
        /// <param name="request">Sort update request</param>
        /// <returns>Success status</returns>
        [HttpPut("sort")]
        public async Task<ActionResult<bool>> UpdateProjectViewSort([FromBody] UpdateProjectViewSortRequest request)
        {
            try
            {
                var success = await _projectService.UpdateProjectViewSortAsync(request);
                return Ok(success);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Clear sorting for a project view
        /// </summary>
        /// <param name="viewId">The ID of the project view</param>
        /// <returns>Success status</returns>
        [HttpDelete("{viewId}/sort")]
        public async Task<ActionResult<bool>> ClearProjectViewSort(string viewId)
        {
            try
            {
                var success = await _projectService.ClearProjectViewSortAsync(viewId);
                return Ok(success);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Set grouping for a project view
        /// </summary>
        /// <param name="request">Group configuration request</param>
        /// <returns>Success status</returns>
        [HttpPost("group")]
        public async Task<ActionResult<bool>> SetProjectViewGroup([FromBody] CreateProjectViewGroupRequest request)
        {
            try
            {
                var success = await _projectService.SetProjectViewGroupAsync(request);
                return Ok(success);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update grouping for a project view
        /// </summary>
        /// <param name="request">Group update request</param>
        /// <returns>Success status</returns>
        [HttpPut("group")]
        public async Task<ActionResult<bool>> UpdateProjectViewGroup([FromBody] UpdateProjectViewGroupRequest request)
        {
            try
            {
                var success = await _projectService.UpdateProjectViewGroupAsync(request);
                return Ok(success);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Clear grouping for a project view
        /// </summary>
        /// <param name="viewId">The ID of the project view</param>
        /// <returns>Success status</returns>
        [HttpDelete("{viewId}/group")]
        public async Task<ActionResult<bool>> ClearProjectViewGroup(string viewId)
        {
            try
            {
                var success = await _projectService.ClearProjectViewGroupAsync(viewId);
                return Ok(success);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
