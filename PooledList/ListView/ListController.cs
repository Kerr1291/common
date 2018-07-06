using UnityEngine;
using System.Collections;

namespace nv
{
    public abstract class ListController : MonoBehaviour
    {
        public ListViewBase listView;

        protected abstract void HandleInput();
    }

    public abstract class ListViewScroller : ListController
    {
        protected abstract void StartScrolling();
        protected abstract void Scroll();
        protected abstract void StopScrolling();
    }
}