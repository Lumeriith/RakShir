﻿using System;
using UnityEngine;

namespace FancyScrollView.Example05
{
    public class Context
    {
        public int SelectedIndex = -1;

        // Cell -> ScrollView
        public Action<int> OnCellClicked;

        // ScrollView -> Cell
        public Action UpdateCellState;

        // xy = cell position, z = data index, w = select animation
        public Vector4[] CellState = new Vector4[1];

        public void SetCellState(int cellIndex, int dataIndex, float x, float y, float selectAnimation)
        {
            var size = cellIndex + 1;
            if (size > CellState.Length)
            {
                Array.Resize(ref CellState, size);
            }

            CellState[cellIndex].x = x;
            CellState[cellIndex].y = y;
            CellState[cellIndex].z = dataIndex;
            CellState[cellIndex].w = selectAnimation;
        }
    }
}
