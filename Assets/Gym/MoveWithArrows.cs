using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    public class MoveWithArrows : MonoBehaviour
    {
        public float speed = 5f;

        private readonly List<KeyCode> inputList = new();

        private void Update()
        {
            UpdateInput(KeyCode.UpArrow);
            UpdateInput(KeyCode.DownArrow);
            UpdateInput(KeyCode.RightArrow);
            UpdateInput(KeyCode.LeftArrow);

            if (inputList.Count > 0)
            {
                var currentInput = inputList[^1];

                switch (currentInput)
                {
                    case KeyCode.UpArrow:
                        transform.position += speed * Time.deltaTime * transform.forward;
                        break;

                    case KeyCode.DownArrow:
                        transform.position -= speed * Time.deltaTime * transform.forward;
                        break;

                    case KeyCode.RightArrow:
                        transform.position += speed * Time.deltaTime * transform.right;
                        break;

                    case KeyCode.LeftArrow:
                        transform.position -= speed * Time.deltaTime * transform.right;
                        break;

                    default: break;
                }
            }
        }

        private void UpdateInput(KeyCode keyCode)
        {
            if (Input.GetKeyDown(keyCode))
            {
                inputList.Add(keyCode);
            }
            else if (!Input.GetKey(keyCode))
            {
                inputList.Remove(keyCode);
            }
        }
    }
}
