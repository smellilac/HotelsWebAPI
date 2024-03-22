public class HotelRepository : IHotelRepository
{
    private readonly HotelDb _context;

    public HotelRepository(HotelDb context)
    {
        _context = context;
    }

    public Task<List<Hotel>> GetHotelsAsync() => _context.Hotels.ToListAsync();


    public async Task<Hotel> GetHotelAsync(int hotelId) =>
        await _context.Hotels.FindAsync(new object[]{hotelId});

    public async Task  InsertHotelAsync(Hotel hotel) => await _context.Hotels.AddAsync(hotel);
    


    public async Task  UpdateHotelAsync(Hotel hotel)
    {
        var entity = await _context.Hotels.FindAsync(new object[] {hotel.Id});
        if (entity == null ) return;
        entity.Longitude = hotel.Longitude;
        entity.Name = hotel.Name;
        entity.Latitude = hotel.Latitude;
    }

    public async Task DeleteHotelAsync(int hotelId)
    {
        var entity = await _context.Hotels.FindAsync(new object[] {hotelId});
        if (entity == null ) return;
        _context.Hotels.Remove(entity);
    }

    public async Task  SaveAsync() => await _context.SaveChangesAsync();

    private bool _disposed = false;
    public virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
        _disposed = true;
        // _disposed = true; in anyway of code executing
        // maybe later I need to fix that 
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}