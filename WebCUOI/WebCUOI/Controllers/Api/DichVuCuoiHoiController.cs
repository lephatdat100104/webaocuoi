using Microsoft.AspNetCore.Mvc;
using WebCUOI.Data;
using Microsoft.EntityFrameworkCore;
using WebCUOI.Models;

namespace WebCUOI.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DichVuCuoiHoiController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Thay tên DbContext thật của bạn

        public DichVuCuoiHoiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/dichvucuoihoi
        [HttpGet]
        public async Task<ActionResult<object>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? search = null,
            [FromQuery] string? loai = null,
            [FromQuery] bool? khacDung = null)
        {
            var query = _context.DichVuCuoiHoi.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(d =>
                    d.TenDichVu.ToLower().Contains(search) ||
                    (d.MoTa != null && d.MoTa.ToLower().Contains(search)));
            }

            // Lọc theo loại (ví dụ: "Chụp ảnh cưới", "Trang trí nhà", "Xe hoa",...)
            if (!string.IsNullOrWhiteSpace(loai))
                query = query.Where(d => d.LoaiSanPham == loai);

            // Lọc theo tình trạng khả dụng
            if (khacDung.HasValue)
                query = query.Where(d => d.KhacDung == khacDung.Value);

            var total = await query.CountAsync();

            var items = (await query
     .OrderBy(d => d.TenDichVu)
     .Skip((page - 1) * pageSize)
     .Take(pageSize)
     .ToListAsync())
     .Select(d => new
     {
         d.ID,
         d.TenDichVu,
         d.MoTa,
         d.GiaDichVu,
         d.ThoiLuong,
         d.KhacDung,
         d.HinhAnhURL,
         d.LoaiSanPham,
         NgayBatDauThue = d.NgayBatDauThue?.ToString("dd/MM/yyyy"),
         NgayKetThucThue = d.NgayKetThucThue?.ToString("dd/MM/yyyy")
     })
     .ToList();

            return Ok(new
            {
                success = true,
                data = items,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        // GET: api/dichvucuoihoi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var dv = await _context.DichVuCuoiHoi.FindAsync(id);

            if (dv == null)
                return NotFound(new { success = false, message = "Không tìm thấy dịch vụ!" });

            return Ok(new
            {
                success = true,
                data = new
                {
                    dv.ID,
                    dv.TenDichVu,
                    dv.MoTa,
                    dv.GiaDichVu,
                    dv.ThoiLuong,
                    dv.KhacDung,
                    dv.HinhAnhURL,
                    dv.LoaiSanPham,
                    NgayBatDauThue = dv.NgayBatDauThue?.ToString("dd/MM/yyyy"),
                    NgayKetThucThue = dv.NgayKetThucThue?.ToString("dd/MM/yyyy")
                }
            });
        }

        // POST, PUT, DELETE (dành cho admin – bạn dùng sau)
        // Mình để sẵn, bạn chỉ cần bỏ comment khi cần
        /*
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] DichVuCuoiHoi dv) { ... }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DichVuCuoiHoi dv) { ... }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) { ... }
        */
    }
}
