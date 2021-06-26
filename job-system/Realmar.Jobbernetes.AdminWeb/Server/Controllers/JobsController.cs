using System;
using Microsoft.AspNetCore.Mvc;
using Realmar.Jobbernetes.AdminWeb.Server.Services;
using Realmar.Jobbernetes.AdminWeb.Shared.Models;

namespace Realmar.Jobbernetes.AdminWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly JobService _jobService;

        public JobsController(JobService jobService) => _jobService = jobService;

        [HttpPut]
        public void Put(AddJobModel model) => _jobService.AddJob(model);

        [HttpDelete]
        public void Delete([FromBody] Guid guid) => _jobService.DeleteJob(guid);
    }
}
