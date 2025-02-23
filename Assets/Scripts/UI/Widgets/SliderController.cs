using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class SliderController : MonoBehaviour, IMoveHandler
    {
        public float step = 1f;
        public Slider slider;

        public void Increase()
        {
            slider.value += step;
        }

        public void Decrease()
        {
            slider.value -= step;
        }

        public void OnMove(AxisEventData eventData)
        {
            if (eventData.moveDir == MoveDirection.Left)
            {
                Decrease();
            }
            else if (eventData.moveDir == MoveDirection.Right)
            {
                Increase();
            }
        }
    }
}
