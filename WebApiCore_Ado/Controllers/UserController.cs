using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using WebApiCore_Ado.Models;
using BCrypt.Net;
using WebApiCore_Ado.Services;
using Microsoft.AspNetCore.Authorization;

namespace WebApiCore_Ado.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="admin")]
    public class UserController : ControllerBase
    {
        private readonly IDBLayer db;
        private readonly ILogger<UserController> _logger;
        private readonly IImageResizer _ImageResize;
        private readonly IWebHostEnvironment _env;
        private readonly IJwtService _jwt;

        public UserController(IDBLayer _db, ILogger<UserController> logger, IImageResizer ImageResize, IWebHostEnvironment env, IJwtService jwt)
        {
            db = _db;
            _logger = logger;
            _ImageResize = ImageResize;
            _env = env;
            _jwt = jwt;
        }

        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            DataTable dt = await db.Table("UserCRUD", new SqlParameter[]
            {
                new SqlParameter("@action" , "selectAll")
            });

            return Ok(dt);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOneUser(int id)
        {
            DataTable dt = await db.Table("UserCRUD", new SqlParameter[]
            {
                new SqlParameter("@action" , "selectOne"),
                new SqlParameter("@uId" , (object)id ?? (object)DBNull.Value),
            });

            return Ok(dt);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOneUser(int id)
        {
            try
            {
                SqlParameter msg = new SqlParameter("@msg", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };

                int res = await db.ExecuteAsync("UserCRUD", new SqlParameter[]
                {
                    new SqlParameter("@action" , "deleteOne"),
                    new SqlParameter("@uId" , (object)id ?? (object)DBNull.Value),
                    msg
                });

                string? resMsg = msg?.Value.ToString() ?? "";

                if (res > 0 || resMsg == "success")
                {
                    return Ok(new { success = true, message = "User Deleted" });
                }

                _logger.LogError($"Database Error : {resMsg}" , "User Not Found");
                return BadRequest(new { success = false, message = "User Not Found"});
            }
            catch (Exception ex)
            {
                _logger.LogError($"Database Error : {ex.Message}", "Database Error");
                return StatusCode(500, new {success=false , message = "Something went wrong" });
            }
             
        }

        [HttpPost]
        [RequestSizeLimit(10*1024*1024)]
        [AllowAnonymous]
        public async Task<ActionResult> AddUser([FromForm] addUserDTO u)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "fill Required Fields" });
                }

                string? HashPass = null;
                if (u.pass != null && u.pass.Length > 0)
                {
                    HashPass = BCrypt.Net.BCrypt.HashPassword(u.pass);
                }

                if(u.img!=null && u.img.Length > 10 * 1024 * 1024)
                {
                    return StatusCode(413, new { success = false, message = "Image size must be less than 10 MB" });
                }

                string? fileName = null;

                if (u.img!=null && u.img.Length > 0)
                {
                   
                    string? filePath = Path.Combine(_env.WebRootPath, "Uploads");

                    fileName = await _ImageResize.ResizeImage(u.img, filePath);

                }

                if (fileName == "fileNotSupported")
                {
                    return BadRequest(new { success = false, message = "File Type Not Supported , Only JPEG, JPG, PNG File type supported" });
                }

                SqlParameter msg = new SqlParameter("@msg", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };

                int res = await db.ExecuteAsync("UserCRUD", new SqlParameter[]
                {
                    new SqlParameter("@action" , "add"),
                    new SqlParameter("@name" , u.name ?? (object)DBNull.Value),
                    new SqlParameter("@email" , u.email ?? (object)DBNull.Value),
                    new SqlParameter("@pass" , HashPass ?? (object)DBNull.Value),
                    new SqlParameter("@img" , fileName ?? (object)DBNull.Value),
                    new SqlParameter("@gender" , u.gender ?? (object)DBNull.Value),
                    new SqlParameter("@hobby" , u.hobby!=null? string.Join("," , u.hobby) : (object)DBNull.Value),
                    new SqlParameter("@address" , u.address ?? (object)DBNull.Value),
                    new SqlParameter("@profession" , u.profession ?? (object)DBNull.Value),
                    new SqlParameter("@isActive" , "1"),
                    new SqlParameter("@dob" , u.dob ?? (object)DBNull.Value),
                    new SqlParameter("@role" , "user"),
                    msg
                });

                string resMsg = msg.Value?.ToString() ?? "";

                return resMsg switch
                {
                    "success" => StatusCode(201, new { success = true, message = "User added" }),
                    "userExists" => Conflict(new { success = false, message = "User already exists" }),
                    _ => BadRequest(new { success = false, message = "Could not add user" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "AddUser failed");
                return StatusCode(500, new { success = false, message = "Something went wrong" });
            }

        }

        [HttpPatch]
        public async Task<ActionResult> EditUser([FromForm] EditUserDTO e)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "fill required fields" });
                }

                SqlParameter msg = new SqlParameter("@msg", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };

                string fileName = null;

                if (e.img != null)
                {
                    string filePath = Path.Combine(_env.WebRootPath, "Uploads");
                    fileName = await _ImageResize.ResizeImage(e.img, filePath);
                }
                int res = await db.ExecuteAsync("UserCRUD", new SqlParameter[]
                {
                    new SqlParameter("action" , "edit"),
                    new SqlParameter("@uId" , e.id ?? (object)DBNull.Value),
                    new SqlParameter("@name" , e.name ?? (object)DBNull.Value),
                    new SqlParameter("@img" , fileName ?? (object)DBNull.Value),
                    new SqlParameter("@gender" , e.gender ?? (object)DBNull.Value),
                    new SqlParameter("@hobby" , e.hobby!=null? string.Join("," , e.hobby) : (object)DBNull.Value),
                    new SqlParameter("@address" , e.address ?? (object)DBNull.Value),
                    new SqlParameter("@profession" , e.profession ?? (object)DBNull.Value),
                    new SqlParameter("@isActive" , e.isActive),
                    new SqlParameter("@dob" , e.dob ?? (object)DBNull.Value),
                    new SqlParameter("@role" , e.role ?? (object)DBNull.Value),
                    msg

                });

                string? resMsg = msg?.Value.ToString() ?? "";

                return resMsg switch
                {
                    "success" => Ok(new { success = true, message = "User updated" }), 
                    _ => BadRequest(new { success = false, message = "Could not update user" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Database Error ");
                return StatusCode(500, new { success = false, message = "Something went wrong" });
            }
        }

        [HttpPut]
        public async Task<ActionResult> EditUserAllDetils([FromForm] EditUserDTO e)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "fill required fields" });
                }

                SqlParameter msg = new SqlParameter("@msg", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };

                string fileName = null;

                if (e.img != null)
                {
                    string filePath = Path.Combine(_env.WebRootPath, "Uploads");
                    fileName = await _ImageResize.ResizeImage(e.img, filePath);
                }
                int res = await db.ExecuteAsync("UserCRUD", new SqlParameter[]
                {
                    new SqlParameter("action" , "edit"),
                    new SqlParameter("@uId" , e.id ?? (object)DBNull.Value),
                    new SqlParameter("@name" , e.name ?? (object)DBNull.Value),
                    new SqlParameter("@img" , fileName ?? (object)DBNull.Value),
                    new SqlParameter("@gender" , e.gender ?? (object)DBNull.Value),
                    new SqlParameter("@hobby" , e.hobby!=null? string.Join("," , e.hobby) : (object)DBNull.Value),
                    new SqlParameter("@address" , e.address ?? (object)DBNull.Value),
                    new SqlParameter("@profession" , e.profession ?? (object)DBNull.Value),
                    new SqlParameter("@isActive" , e.isActive),
                    new SqlParameter("@dob" , e.dob ?? (object)DBNull.Value),
                    new SqlParameter("@role" , e.role ?? (object)DBNull.Value),
                    msg

                });

                string? resMsg = msg?.Value.ToString() ?? "";

                return resMsg switch
                {
                    "success" => Ok(new { success = true, message = "User updated" }), 
                    _ => BadRequest(new { success = false, message = "Could not update user" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Database Error ");
                return StatusCode(500, new { success = false, message = "Something went wrong" });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login(loginDTO l)
        {
            try
            {
                string token = null;

                DataTable dt = await db.Table("UserCRUD", new SqlParameter[]
                {
                    new SqlParameter("@action" , "Login"),
                    new SqlParameter("@email" , l.email),
                });

                if (dt.Rows.Count > 0)
                {
                    bool isValid = BCrypt.Net.BCrypt.Verify(l.pass, dt.Rows[0]["pass"].ToString());
                    if (!isValid)
                    {
                        return Unauthorized(new { success = false, message = "Invalid email or password" });
                    }

                    token = _jwt.CreateToken(dt.Rows[0]["uId"].ToString()!, dt.Rows[0]["role"].ToString()!);
                    return Ok(new { success = true, token = token });
                }
                 
                return Ok(new { success = false, message = "Invalid email or password" });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, "Database Error");
                return StatusCode(500, "Something went wrong");
            }
        }
    }
}
