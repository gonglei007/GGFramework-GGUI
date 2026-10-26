using UnityEngine;

namespace GGFramework.GGUI.NGUI
{
    /// <summary>
    /// 基于 NGUI UIGrid 的布局组实现。
    /// </summary>
    public class NGUIGrid : IUILayoutGroup
    {
        private readonly UIGrid grid;

        public NGUIGrid(UIGrid grid)
        {
            this.grid = grid;
        }

        public float cellWidth
        {
            get { return grid != null ? grid.cellWidth : 0f; }
            set { if (grid != null) { grid.cellWidth = value; } }
        }

        public void Reposition()
        {
            if (grid != null)
            {
                grid.repositionNow = true;
            }
        }

        public GameObject gameObject
        {
            get { return grid != null ? grid.gameObject : null; }
        }
    }
}
