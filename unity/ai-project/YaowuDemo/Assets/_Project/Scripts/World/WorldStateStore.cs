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

    public void ApplyFinalChoice(string choice)
    {
        if (Current == null || Current.ghost == null)
        {
            Current = new PersistentWorldState();
        }

        Current.lastChoice = choice;
        switch (choice)
        {
            case "seal":
                Current.corruption = Mathf.Max(0, Current.corruption - 25);
                Current.ghost.trust -= 10;
                AddUnique(Current.scars, "red_seal");
                break;
            case "symbiosis":
                Current.cycle += 1;
                Current.ghost.trust += 25;
                AddUnique(Current.ghost.memories, "与神明共生");
                AddUnique(Current.scars, "cyan_guard");
                break;
            case "transfer":
                Current.cycle += 1;
                Current.corruption = Mathf.Max(0, Current.corruption - 10);
                Current.ghost.trust += 5;
                Current.ghost.shell = "player_vessel";
                AddUnique(Current.ghost.memories, "代价转移");
                break;
            default:
                return;
        }

        Save();
        Changed?.Invoke(Current);
    }

    private static void AddUnique(List<string> values, string value)
    {
        if (!values.Contains(value))
        {
            values.Add(value);
        }
    }
}
