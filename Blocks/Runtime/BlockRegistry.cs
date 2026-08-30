using System;
using System.Collections.Generic;

namespace MineWorld.Blocks.Runtime;

public sealed record BlockDefinition(
    string Id,
    string Material,
    float Hardness,
    string[] Tags,
    IReadOnlyDictionary<string, string> DefaultState);

public readonly record struct BlockState(int RuntimeId, string BlockId, IReadOnlyDictionary<string, string> Properties);

public sealed class BlockRegistry
{
    private readonly Dictionary<string, BlockDefinition> _definitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _runtimeIds = new(StringComparer.Ordinal);
    private bool _frozen;

    public int Count => _definitions.Count;
    public bool IsFrozen => _frozen;

    public void Register(BlockDefinition definition)
    {
        if (_frozen) throw new InvalidOperationException("Block registry is frozen.");
        Validate(definition);
        if (!_definitions.TryAdd(definition.Id, definition))
            throw new InvalidOperationException($"Block '{definition.Id}' is already registered.");
    }

    public void Freeze()
    {
        if (_frozen) return;
        var ids = new List<string>(_definitions.Keys);
        ids.Sort(StringComparer.Ordinal);
        for (var i = 0; i < ids.Count; i++) _runtimeIds[ids[i]] = i;
        _frozen = true;
    }

    public BlockDefinition GetDefinition(string id) =>
        _definitions.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Unknown block '{id}'.");

    public int GetRuntimeId(string id)
    {
        EnsureFrozen();
        return _runtimeIds.TryGetValue(id, out var runtimeId)
            ? runtimeId
            : throw new KeyNotFoundException($"Unknown block '{id}'.");
    }

    public BlockState CreateDefaultState(string id)
    {
        EnsureFrozen();
        var definition = GetDefinition(id);
        return new BlockState(GetRuntimeId(id), id,
            new Dictionary<string, string>(definition.DefaultState, StringComparer.Ordinal));
    }

    private void EnsureFrozen()
    {
        if (!_frozen) throw new InvalidOperationException("Block registry must be frozen before runtime use.");
    }

    private static void Validate(BlockDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Id) || !definition.Id.Contains(':'))
            throw new ArgumentException("Block id must use the namespace:id format.");
        if (definition.Hardness < 0) throw new ArgumentOutOfRangeException(nameof(definition.Hardness));
        if (definition.DefaultState is null) throw new ArgumentNullException(nameof(definition.DefaultState));
    }
}
