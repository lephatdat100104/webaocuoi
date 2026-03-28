using Microsoft.AspNetCore.Mvc;
using WebCUOI.Data;
using WebCUOI.Models;
using Microsoft.EntityFrameworkCore;

namespace WebCUOI.Controllers.Api
{
    
        [Route("api/[controller]")]
        [ApiController]
        public class AoCuoiController : ControllerBase
        {
            private readonly ApplicationDbContext _context; // Thay YourDbContext bằng tên DbContext thật của bạn

            public AoCuoiController(ApplicationDbContext context)
            {
                _context = context;
            }

            // GET: api/aocuoi
            [HttpGet]
            public async Task<ActionResult<IEnumerable<AoCuoi>>> GetAll(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 12,
                [FromQuery] string? search = null)
            {
                var query = _context.AoCuoi.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(a =>
                        a.TenAoCuoi.ToLower().Contains(search) ||
                        (a.MoTa != null && a.MoTa.ToLower().Contains(search)));
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(a => a.NgayNhapKho)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

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

            // GET: api/aocuoi/5
            [HttpGet("{id}")]
            public async Task<ActionResult<AoCuoi>> Get(int id)
            {
                var ao = await _context.AoCuoi.FindAsync(id);
                if (ao == null)
                    return NotFound(new { success = false, message = "Không tìm thấy áo cưới" });

                return Ok(new { success = true, data = ao });
            }
        }

    }

