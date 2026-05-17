// backend/src/FlowDesk.Application/Interfaces/IPasswordHasher.cs
namespace FlowDesk.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}