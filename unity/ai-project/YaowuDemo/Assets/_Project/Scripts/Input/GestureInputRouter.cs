using UnityEngine;

public enum ProtocolInputAction
{
    Point,
    Confirm
}

public sealed class GestureInputRouter : MonoBehaviour
{
    private SkillManager skillManager;
    private KeyboardSkillInput keyboardInput;
    private MouseGestureInput mouseInput;
    private MediaPipeHandInput mediaPipeInput;

    public void Initialize(
        SkillManager skillManagerRef,
        KeyboardSkillInput keyboard,
        MouseGestureInput mouse,
        MediaPipeHandInput mediaPipe)
    {
        skillManager = skillManagerRef;
        keyboardInput = keyboard;
        mouseInput = mouse;
        mediaPipeInput = mediaPipe;

        keyboardInput.SkillRequested += HandleKeyboardSkill;
        keyboardInput.ResetRequested += HandleReset;
        mouseInput.SkillRequested += HandleMouseSkill;
        mediaPipeInput.SkillRequested += HandleMediaPipeSkill;
        mediaPipeInput.ProtocolActionRequested += HandleMediaPipeProtocolAction;
        keyboardInput.ProtocolActionRequested += HandleKeyboardProtocolAction;
    }

    private void OnDestroy()
    {
        if (keyboardInput != null)
        {
            keyboardInput.SkillRequested -= HandleKeyboardSkill;
            keyboardInput.ResetRequested -= HandleReset;
        }

        if (mouseInput != null)
        {
            mouseInput.SkillRequested -= HandleMouseSkill;
        }

        if (mediaPipeInput != null)
        {
            mediaPipeInput.SkillRequested -= HandleMediaPipeSkill;
            mediaPipeInput.ProtocolActionRequested -= HandleMediaPipeProtocolAction;
        }

        if (keyboardInput != null)
        {
            keyboardInput.ProtocolActionRequested -= HandleKeyboardProtocolAction;
        }
    }

    private void HandleKeyboardSkill(SkillType skill, string label)
    {
        Dispatch(skill, "键盘", label);
    }

    private void HandleMouseSkill(SkillType skill, string label)
    {
        Dispatch(skill, "鼠标画符", label);
    }

    private void HandleMediaPipeSkill(SkillType skill, string label)
    {
        Dispatch(skill, "摄像头手势", label);
    }

    private void HandleMediaPipeProtocolAction(ProtocolInputAction action, string label)
    {
        DispatchProtocolAction(action, "摄像头手势", label);
    }

    private void HandleKeyboardProtocolAction(ProtocolInputAction action, string label)
    {
        DispatchProtocolAction(action, "键盘", label);
    }

    private void HandleReset()
    {
        SimpleEventBus.RaiseRecognitionStatusChanged("重置战斗");
        DemoFlowController.RequestReset();
    }

    private void Dispatch(SkillType skill, string inputMode, string label)
    {
        SimpleEventBus.RaiseInputModeChanged(inputMode);
        SimpleEventBus.RaiseRecognitionStatusChanged("已识别: " + label);
        skillManager.CastSkill(skill);
    }

    private static void DispatchProtocolAction(ProtocolInputAction action, string inputMode, string label)
    {
        SimpleEventBus.RaiseInputModeChanged(inputMode);
        SimpleEventBus.RaiseRecognitionStatusChanged("已识别: " + label);
        SimpleEventBus.RaiseProtocolInput(action);
    }
}
