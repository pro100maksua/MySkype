namespace MySkype.Server.Controllers
{
    //[Route("api/users")]
    //[Authorize]
    //[ApiController]
    //public class RoomsController : ControllerBase
    //{
    //    private readonly IRoomsRepository _roomsRepository;

    //    public RoomsController(IRoomsRepository roomsRepository)
    //    {
    //        _roomsRepository = roomsRepository;
    //    }

    //    [HttpGet("{userId}/rooms")]
    //    public async Task<IActionResult> GetAllAsync(Guid userId)
    //    {
    //        var rooms = await _roomsRepository.GetRoomsByUserIdAsync(userId);

    //        return Ok(rooms);
    //    }

    //    [HttpPost("/rooms")]
    //    public async Task<IActionResult> PostAsync([FromBody] RoomDto roomDto)
    //    {
    //        var room = new Room
    //        {
    //            Id = Guid.NewGuid(),
    //            Name = roomDto.Name,
    //            UserIds = new List<Guid> { roomDto.UserId },
    //            Photo = new Photo { FileName = "default.jpg", Id = Guid.NewGuid() }
    //        };

    //        await _roomsRepository.AddAsync(room);

    //        return Ok(room);
    //    }
    //}
}
