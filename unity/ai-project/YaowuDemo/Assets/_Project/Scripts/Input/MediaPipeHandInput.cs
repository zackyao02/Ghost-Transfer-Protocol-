using System;
using UnityEngine;

public sealed class MediaPipeHandInput : MonoBehaviour
{
    public event Action<SkillType, string> SkillRequested;

    public bool IsAvailable => false;

    private void Start()
    {
        SimpleEventBus.RaiseRecognitionStatusChanged("MediaPipe 未接入，已启用稳定兜底输入");
        SimpleEventBus.RaiseInputModeChanged("键盘 / 鼠标");
    }

    public void SimulateSkill(SkillType skill, string label)
    {
        SkillRequested?.Invoke(skill, label);
    }
}