using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly ICommandDataClient _commandDataClient;

        public PlatformsController(IPlatformRepo repository, ICommandDataClient commandDataClient)
        {
            _repository = repository;
            _commandDataClient = commandDataClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            var platforms = _repository.GetAllPlatforms();
            var platformDtos = new List<PlatformReadDto>();
            foreach (var p in platforms)
            {
                platformDtos.Add(new PlatformReadDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Publisher = p.Publisher,
                    Cost = p.Cost
                });
            }

            return Ok(platformDtos);
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatformById(int id)
        {
            var platform = _repository.GetPlatformById(id);
            if (platform != null)
                return Ok(new PlatformReadDto()
                {
                    Id = platform.Id,
                    Name = platform.Name,
                    Publisher = platform.Publisher,
                    Cost = platform.Cost
                });

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            var platformModel = new Platform()
            {
                Name = platformCreateDto.Name,
                Publisher = platformCreateDto.Publisher,
                Cost = platformCreateDto.Cost
            };

            _repository.CreatePlatform(platformModel);
            _repository.SaveChanges();

            var platformReadDto = new PlatformReadDto()
            {
                Id = platformModel.Id,
                Publisher = platformModel.Publisher,
                Name = platformModel.Name,
                Cost = platformModel.Cost
            };

            try
            {
                await _commandDataClient.SendPlatFormToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send synchronouslyu: {ex.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
        }
    }
}