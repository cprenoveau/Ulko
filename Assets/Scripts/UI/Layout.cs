using System;
using System.Collections;
using Ulko.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public static class Layout
    {
        public static void SetupGrid(GridLayoutGroup grid, Vector2 anchoredPosition)
        {
            grid.StartCoroutine(SetupNavigationDelayed(grid, anchoredPosition));
        }

        private static IEnumerator SetupNavigationDelayed(GridLayoutGroup grid, Vector2 anchoredPosition)
        {
            yield return null;

            var autoScroll = grid.GetComponentInParent<AutoScroll>();
            if (autoScroll != null)
                autoScroll.Reset(anchoredPosition);

            SetupGridCellSize(grid);
            SetupWrapAroundGridNavigation(grid);
        }

        private static void SetupGridCellSize(GridLayoutGroup grid)
        {
            float width = grid.GetComponent<RectTransform>().rect.width - (grid.constraintCount - 1) * grid.spacing.x - grid.padding.left - grid.padding.right;
            grid.cellSize = new Vector2(width / grid.constraintCount, grid.cellSize.y);
        }

        private static void SetupWrapAroundGridNavigation(GridLayoutGroup grid)
        {
            if (grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
            {
                Debug.LogWarning("SetupWrapAroundGridNavigation only works with grids with a fixed column count");
                return;
            }

            int lastColIndex = GetLastColumnIndex(grid);

            for (int i = 0; i < grid.transform.childCount; ++i)
            {
                var coord = GetCoordinates(i, grid);

                var selectable = GetSelectableAt(i, grid.transform);
                var nav = new Navigation() { mode = Navigation.Mode.Explicit };

                if (coord.colIndex == 0)
                    nav.selectOnLeft = GetSelectableAt(GetIndex(lastColIndex, coord.rowIndex, grid), grid.transform);
                else
                    nav.selectOnLeft = GetSelectableAt(GetIndex(coord.colIndex - 1, coord.rowIndex, grid), grid.transform);

                if (coord.colIndex == lastColIndex)
                    nav.selectOnRight = GetSelectableAt(GetIndex(0, coord.rowIndex, grid), grid.transform);
                else
                    nav.selectOnRight = GetSelectableAt(GetIndex(coord.colIndex + 1, coord.rowIndex, grid), grid.transform);

                int lastRowIndex = GetLastRowIndex(coord.colIndex, grid);

                if (coord.rowIndex == 0)
                    nav.selectOnUp = GetSelectableAt(GetIndex(coord.colIndex, lastRowIndex, grid), grid.transform);
                else
                    nav.selectOnUp = GetSelectableAt(GetIndex(coord.colIndex, coord.rowIndex - 1, grid), grid.transform);

                if (coord.rowIndex == lastRowIndex)
                    nav.selectOnDown = GetSelectableAt(GetIndex(coord.colIndex, 0, grid), grid.transform);
                else
                    nav.selectOnDown = GetSelectableAt(GetIndex(coord.colIndex, coord.rowIndex + 1, grid), grid.transform);

                selectable.navigation = nav;
            }
        }

        public static void SetupVertical(Transform parent, Vector2 anchoredPosition)
        {
            parent.GetComponent<LayoutGroup>().StartCoroutine(SetupVerticalDelayed(parent, anchoredPosition));
        }

        private static IEnumerator SetupVerticalDelayed(Transform parent, Vector2 anchoredPosition)
        {
            yield return null;

            var autoScroll = parent.GetComponentInParent<AutoScroll>();
            if (autoScroll != null)
                autoScroll.Reset(anchoredPosition);

            SetupVerticalNavigation(parent);
        }

        private static void SetupVerticalNavigation(Transform parent)
        {
            for (int i = 0; i < parent.childCount; ++i)
            {
                var child = GetSelectableAt(i, parent);
                var nav = new Navigation() { mode = Navigation.Mode.Explicit };

                if (i == 0)
                {
                    nav.selectOnUp = GetSelectableAt(parent.childCount - 1, parent);
                }
                else
                {
                    nav.selectOnUp = GetSelectableAt(i - 1, parent);
                }

                if (i == parent.childCount - 1)
                {
                    nav.selectOnDown = GetSelectableAt(0, parent);
                }
                else
                {
                    nav.selectOnDown = GetSelectableAt(i + 1, parent);
                }

                child.navigation = nav;
            }
        }

        private static Selectable GetSelectableAt(int index, Transform parent)
        {
            return parent.GetChild(index).GetComponentInChildren<Selectable>();
        }

        private static (int colIndex, int rowIndex) GetCoordinates(int index, GridLayoutGroup grid)
        {
            int col = index % grid.constraintCount;
            int row = index / grid.constraintCount;

            return (col, row);
        }

        private static int GetIndex(int colIndex, int rowIndex, GridLayoutGroup grid)
        {
            int maxRowIndex = GetLastRowIndex(colIndex, grid);
            rowIndex = Mathf.Clamp(rowIndex, 0, maxRowIndex);

            return rowIndex * grid.constraintCount + colIndex;
        }

        private static int GetLastColumnIndex(GridLayoutGroup grid)
        {
            if (grid.transform.childCount < grid.constraintCount)
                return grid.transform.childCount - 1;

            return grid.constraintCount - 1;
        }

        private static int GetLastRowIndex(int colIndex, GridLayoutGroup grid)
        {
            int remaining = grid.transform.childCount % grid.constraintCount;

            if (remaining == 0 || colIndex >= remaining)
                return grid.transform.childCount / grid.constraintCount - 1;
            else
                return grid.transform.childCount / grid.constraintCount;
        }
    }
}
