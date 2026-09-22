using System;
using UnityEngine;

public sealed class KeyboardSkillInput : MonoBehaviour
{
    public event Action<SkillType, string> SkillRequested;
    public event Action ResetRequested;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SkillRequested?.Invoke(SkillType.SwordQi, "键盘剑气");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SkillRequested?.Invoke(SkillType.FireTalisman, "键盘火符");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SkillRequested?.Invoke(SkillType.DeitySummon, "键盘请神");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetRequested?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SimpleEventBus.RaiseRecognitionStatusChanged("已退出光标锁定，保留当前演示流程");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
