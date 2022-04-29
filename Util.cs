using System.Collections.Generic;
using UnityEngine;

namespace Assets.VRCAssetAdd.Editor
{
    public class Util
    {
        static public Transform FindDescent(Transform start, string key)
        {
            var stack = new Stack<Transform>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                var curr = stack.Pop();
                if (curr.name == key) return curr;
                for (int i = 0; i < curr.childCount; ++i)
                    stack.Push(curr.GetChild(i));
            }
            return null;
        }
    }
}
