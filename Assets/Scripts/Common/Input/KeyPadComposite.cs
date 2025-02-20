using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
[Preserve]
[DisplayStringFormat("{up}/{left}/{down}/{right}")]
public class KeyPadComposite : InputBindingComposite<Vector2>
{
    // NOTE: This is a modified copy of Vector2Composite

    [InputControl(layout = "Button")]
    public int up = 0;
    [InputControl(layout = "Button")]
    public int down = 0;
    [InputControl(layout = "Button")]
    public int left = 0;
    [InputControl(layout = "Button")]
    public int right = 0;

    private Vector2 leftValue = new Vector2(-1, 0);
    private Vector2 rightValue = new Vector2(1, 0);
    private Vector2 downValue = new Vector2(0, -1);
    private Vector2 upValue = new Vector2(0, 1);

    private List<Vector2> _stateStack = new List<Vector2>();

    public override Vector2 ReadValue(ref InputBindingCompositeContext context)
    {
        var upPressed = context.ReadValueAsButton(up);
        var downPressed = context.ReadValueAsButton(down);
        var leftPressed = context.ReadValueAsButton(left);
        var rightPressed = context.ReadValueAsButton(right);

        if (leftPressed && !_stateStack.Contains(leftValue)) _stateStack.Add(leftValue);
        if (rightPressed && !_stateStack.Contains(rightValue)) _stateStack.Add(rightValue);
        if (downPressed && !_stateStack.Contains(downValue)) _stateStack.Add(downValue);
        if (upPressed && !_stateStack.Contains(upValue)) _stateStack.Add(upValue);

        if (!leftPressed) _stateStack.RemoveAll(s => s == leftValue);
        if (!rightPressed) _stateStack.RemoveAll(s => s == rightValue);
        if (!downPressed) _stateStack.RemoveAll(s => s == downValue);
        if (!upPressed) _stateStack.RemoveAll(s => s == upValue);

        if(_stateStack.Count > 0)
            return _stateStack.Last();

        return Vector2.zero;
    }

    public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
    {
        var value = ReadValue(ref context);
        return value.magnitude;
    }

#if UNITY_EDITOR
    static KeyPadComposite()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Initialize()
    {
        InputSystem.RegisterBindingComposite<KeyPadComposite>();
    }
}