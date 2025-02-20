using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewiredKeyPadComposite
{
    private Vector2 leftValue = new Vector2(-1, 0);
    private Vector2 rightValue = new Vector2(1, 0);
    private Vector2 downValue = new Vector2(0, -1);
    private Vector2 upValue = new Vector2(0, 1);

    public Vector2 Value => _stateStack.Count > 0 ? _stateStack.Last() : Vector2.zero;

    private List<Vector2> _stateStack = new List<Vector2>();

    public Vector2 ReadInput(Rewired.InputActionEventData data)
    {
        var upPressed = data.player.GetButton("MoveUp");
        var downPressed = data.player.GetButton("MoveDown");
        var leftPressed = data.player.GetButton("MoveLeft");
        var rightPressed = data.player.GetButton("MoveRight");

        if (leftPressed && !_stateStack.Contains(leftValue)) _stateStack.Add(leftValue);
        if (rightPressed && !_stateStack.Contains(rightValue)) _stateStack.Add(rightValue);
        if (downPressed && !_stateStack.Contains(downValue)) _stateStack.Add(downValue);
        if (upPressed && !_stateStack.Contains(upValue)) _stateStack.Add(upValue);

        if (!leftPressed) _stateStack.RemoveAll(s => s == leftValue);
        if (!rightPressed) _stateStack.RemoveAll(s => s == rightValue);
        if (!downPressed) _stateStack.RemoveAll(s => s == downValue);
        if (!upPressed) _stateStack.RemoveAll(s => s == upValue);

        return Value;
    }
}