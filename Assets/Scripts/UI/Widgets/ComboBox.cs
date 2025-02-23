using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ulko.UI
{
    public class ComboBox : MonoBehaviour, IMoveHandler
    {
        public int itemCount = 10;

        public int Index
        {
            get { return index; }
            set
            {
                int newIndex = Mathf.Clamp(value, 0, itemCount - 1);
                if(newIndex != index)
                {
                    index = newIndex;
                    OnValueChanged?.Invoke(index);
                }
            }
        }

        private int index;

        public event Action<int> OnValueChanged;

        public void Next()
        {
            if (itemCount == 0)
                return;

            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            Index = (index + 1) % itemCount;
        }

        public void Previous()
        {
            if (itemCount == 0)
                return;

            Audio.Player.PlayUISound(Audio.UISoundId.MenuBlip);

            if (index == 0)
                Index = itemCount - 1;
            else
                Index = index - 1;
        }

        public void OnMove(AxisEventData eventData)
        {
            if (eventData.moveDir == MoveDirection.Left)
            {
                Previous();
            }
            else if (eventData.moveDir == MoveDirection.Right)
            {
                Next();
            }
        }
    }
}
