namespace WebApiCore_Ado.Services
{
    public interface IJwtService
    {
        string CreateToken(string userId, string role);
    }
}