using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class GhostMemoryState
{
    public int trust;
    public string shell = "shanxiao_shell";
    public List<string> memories = new List<string>();
}

[Serializable]
public sealed class PersistentWorldState
{
    public int cycle;
    public int corruption;
    public GhostMemoryState ghost = new GhostMemoryState();
    public List<string> scars = new List<string>();
    public string lastChoice = "";
}

public sealed class WorldStateStore : MonoBehaviour
{
    private const string SaveKey = "ghost_transfer_world_state";
    public PersistentWorldState Current { get; private set; } = new PersistentWorldState();
    public event Action<PersistentWorldState> Changed;

    public void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            Current = JsonUtility.FromJson<PersistentWorldState>(PlayerPrefs.GetString(SaveKey));
        }
        if (Current == null || Current.ghost == null)
        {
            Current = new PersistentWorldState();
        }
        Changed?.Invoke(Current);
    }

    public void LoadPreset(string preset)
    {
        Current = new PersistentWorldState();
        if (preset == "kind")
        {
            Current.cycle = 7;
            Current.ghost.trust = 60;
            Current.ghost.memories.Add("连续共生");
            Current.scars.Add("cyan_guard");
            Current.lastChoice = "Symbiosis";
        }
        else
        {
            Current.cycle = 7;
            Current.corruption = 70;
            Current.ghost.trust = -60;
            Current.ghost.memories.Add("反复封存");
            Current.scars.Add("red_seal");
            Current.lastChoice = "Seal";
        }
        Save();
        Changed?.Invoke(Current);
    }

    public void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Current));
        PlayerPrefs.Save();
    }
}
