using AuthService.Models;
using System.Collections.Concurrent;

namespace AuthService.Services;

public static class CodeStore
{
    private static ConcurrentDictionary<string, CodeStoreData> codes = new();

    public static void Save(string code, User user, string clientId, string scope)
        => codes[code] = new CodeStoreData { User = user, ClientId = clientId, Scope = scope };

    public static CodeStoreData? Get(string code)
        => codes.TryGetValue(code, out var entry) ? entry : null;
}