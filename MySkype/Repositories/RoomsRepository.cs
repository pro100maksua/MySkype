namespace MySkype.Server.Repositories
{
    //public class RoomsRepository : IRoomsRepository
    //{
    //    private readonly MongoContext _context;

    //    public RoomsRepository(MongoContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<IEnumerable<Room>> GetRoomsByUserIdAsync(Guid userId)
    //    {
    //        return await _context.Rooms
    //            .AsQueryable()
    //            .Where(r => r.UserIds.Contains(userId))
    //            .ToListAsync();
    //    }

    //    public async Task<Room> GetRoomAsync(Guid id)
    //    {
    //        return await _context.Rooms
    //            .AsQueryable()
    //            .FirstOrDefaultAsync(u => u.Id == id);
    //    }


    //    public async Task AddAsync(Room room)
    //    {
    //        await _context.Rooms.InsertOneAsync(room);
    //    }
    //}
}