using System.Security.Cryptography;
using System.Text;

namespace Domain.EventSourcing;

public static class Utils
{
    private static readonly MD5 _md5 = MD5.Create();

    public static Guid ToGuid(this string value) => new Guid(_md5.ComputeHash(Encoding.Default.GetBytes(value)));
}
