// backend/src/FlowDesk.Application/Interfaces/IJwtProvider.cs
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Interfaces;

public interface IJwtProvider
{
    string Generate(User user);
}