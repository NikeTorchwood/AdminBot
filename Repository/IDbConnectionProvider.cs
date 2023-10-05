using System.Data;

namespace AdminBot.Repository;

public interface IDbConnectionProvider
{
    public IDbConnection GetConnection();
}