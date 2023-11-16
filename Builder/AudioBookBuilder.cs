using System.Security.Cryptography.X509Certificates;
using MujRozhlas.Data;
using MujRozhlas.Database;

namespace MujRozhlas.Builder;

public class AudioBookBuilder
{
    private readonly IDatabase database;

    public AudioBookBuilder(IDatabase database)
    {
        this.database = database;
    }

    public void BuildBook(Serial serial)
    {

    }
}