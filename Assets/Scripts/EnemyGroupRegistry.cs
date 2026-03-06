using System;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyGroupRegistry
{
    public struct GroupState
    {
        public int aliveCount;
        public int remainingSpawnCount;

        public bool IsClear => aliveCount <= 0 && remainingSpawnCount <= 0;
    }

    public static event Action<string, GroupState> OnGroupStateChanged;

    private static readonly Dictionary<string, GroupState> groups = new();

    public static GroupState GetState(string groupTag)
    {
        if (string.IsNullOrWhiteSpace(groupTag))
            return default;

        if (groups.TryGetValue(groupTag, out var state))
            return state;

        return default;
    }

    public static void RegisterAlive(string groupTag)
    {
        if (string.IsNullOrWhiteSpace(groupTag))
            return;

        var state = GetState(groupTag);
        state.aliveCount++;
        groups[groupTag] = state;
        Notify(groupTag);
    }

    public static void UnregisterAlive(string groupTag)
    {
        if (string.IsNullOrWhiteSpace(groupTag))
            return;

        var state = GetState(groupTag);
        state.aliveCount = Mathf.Max(0, state.aliveCount - 1);
        groups[groupTag] = state;
        Notify(groupTag);
    }

    public static void AddPlannedSpawns(string groupTag, int amount)
    {
        if (string.IsNullOrWhiteSpace(groupTag) || amount <= 0)
            return;

        var state = GetState(groupTag);
        state.remainingSpawnCount += amount;
        groups[groupTag] = state;
        Notify(groupTag);
    }

    public static void ConsumePlannedSpawn(string groupTag, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(groupTag) || amount <= 0)
            return;

        var state = GetState(groupTag);
        state.remainingSpawnCount = Mathf.Max(0, state.remainingSpawnCount - amount);
        groups[groupTag] = state;
        Notify(groupTag);
    }

    public static void RemovePlannedSpawns(string groupTag, int amount)
    {
        if (string.IsNullOrWhiteSpace(groupTag) || amount <= 0)
            return;

        var state = GetState(groupTag);
        state.remainingSpawnCount = Mathf.Max(0, state.remainingSpawnCount - amount);
        groups[groupTag] = state;
        Notify(groupTag);
    }

    private static void Notify(string groupTag)
    {
        if (!groups.TryGetValue(groupTag, out var state))
            state = default;

        OnGroupStateChanged?.Invoke(groupTag, state);
    }
}