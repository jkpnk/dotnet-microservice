using System;
using System.Collections;
using CommandsService.Data;
using CommandsService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controller
{
    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly ICommandRepo _repository;

        public PlatformsController(ICommandRepo repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("==> Getting Platforms from CommandsService");
            var platformItems = _repository.GetAllPlatforms();
            var platformReadDto = platformItems.Select(x => new PlatformReadDto
            {
                Name = x.Name,
                MyProperty = x.ExternalID
            });
            return Ok(platformItems);
        }
        [HttpPost]
        public ActionResult TestInboundConnection()
        {
            Console.WriteLine("--> Inbound Post # Command Service");

            return Ok("Inbound test of from Platforms Controller");
        }
    }
}