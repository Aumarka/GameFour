using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerInputLockReasons
{
    Dialogue,
    PauseMenu,
    Cutscene,
    Inventory
}

public static class PlayerInputLock
{
    private static readonly HashSet<PlayerInputLockReasons> Locks = new();

    public static bool IsLocked => Locks.Count > 0;

    public static event Action<bool> LockStateChanged;

    public static void Lock(PlayerInputLockReasons reason)
    {
        if (Locks.Add(reason))
            LockStateChanged?.Invoke(IsLocked);
    }

    public static void Unlock(PlayerInputLockReasons reason)
    {
        if (Locks.Remove(reason))
            LockStateChanged?.Invoke(IsLocked);
    }
}
