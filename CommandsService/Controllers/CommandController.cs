using System.Runtime.CompilerServices;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsController
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo _repository;

        public CommandsController(ICommandRepo repo)
        {
            _repository = repo;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"--> Hit GetCommandsForPlatformL {platformId}");
            if (_repository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var commands = _repository.GetCommandsForPlatform(platformId);

            var commandsReadDtos = commands.Select(x => new CommandReadDto
            {
                Id = x.Id,
                HowTo = x.HowTo,
                CommandLine = x.CommandLine,
                PlatformId = x.PlatformId

            });
            return Ok(commandsReadDtos);
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            Console.WriteLine($"--> hit GetCommandForPlatform: {platformId} / {commandId}");

            if (!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var command = _repository.GetCommand(platformId, commandId);
            if (command == null)
            {
                return NotFound();
            }
            var commandReadDto = new CommandReadDto
            {
                Id = command.Id,
                PlatformId = command.PlatformId,
                HowTo = command.HowTo,
                CommandLine = command.CommandLine
            };

            return Ok(commandReadDto);
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandDto)
        {
            Console.WriteLine($"--> hit CreateCommandForPlatform: {platformId}");

            if (!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var command = new Command
            {
                PlatformId = platformId,
                HowTo = commandDto.HowTo,
                CommandLine = commandDto.CommandLine
            };
            _repository.CreateCommand(platformId, command);
            _repository.SaveChanges();

            var commandReadDto = new CommandReadDto
            {
                CommandLine = command.CommandLine,
                HowTo = command.HowTo,
                Id = command.Id,
                PlatformId = command.PlatformId
            };

            return CreatedAtRoute(nameof(GetCommandForPlatform),
                new { platformId = platformId, commandId = commandReadDto.Id }, commandReadDto);
        }
    }
}