using System;
using UnityEngine;

namespace Ulko.Data
{
    public class SixSidedDie<T>
    {
        /* Array representation:
         * 
         *    [0]
         * [2][1][3]
         *    [5]
         *    [4]
         */

        public T TopFace => Faces[TopFaceIndex];
        public T BottomFace => Faces[5 - TopFaceIndex];

        public int TopFaceIndex
        {
            get { return topFaceIndex; }
            set {  topFaceIndex = Mathf.Clamp(value, 0, 6); }
        }
        private int topFaceIndex;

        public T[] Faces = new T[6];

        public event Action<T> OnRolled;

        public SixSidedDie(T[] faces, int topFaceIndex = 0)
        {
            Debug.Assert(faces.Length == 6);
            Debug.Assert(topFaceIndex >= 0 && topFaceIndex < 6);

            Faces = faces;
            TopFaceIndex = topFaceIndex;
        }

        public T Roll()
        {
            TopFaceIndex = UnityEngine.Random.Range(0, 6);
            OnRolled?.Invoke(TopFace);

            return TopFace;
        }
    }
}
