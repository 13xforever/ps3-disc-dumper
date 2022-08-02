using System.Text;
using System.Text.Json;

namespace Ps3DiscDumper;

public class SnakeCasePolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        var result = new StringBuilder(name.Length + 3);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsLower(c))
                result.Append(c);
            else
            {
                c = char.ToLower(c);
                if (i > 0)
                    result.Append('_').Append(c);
                else
                    result.Append(c);
            }
        }
        return result.ToString();
    }
}