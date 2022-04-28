using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VRCAssetAdd.Editor
{
    internal class AssetSearch
    {
        private readonly Transform root;
        private readonly Transform asset;
        private readonly Transform original;

        public AssetSearch(Transform root, Transform asset, Transform original)
        {
            this.root = root;
            this.asset = asset;
            this.original = original;
        }

        /**
         * I will assume that adding the asset requires only adding things to the model
         * I think this is a fair assumption
         */
        public List<AssetDifference> Search()
        {
            var result = new List<AssetDifference>();
            var path = new Stack<string>();

            RecursiveSearch(original, root, ref path, ref result);

            return result;
        }

        private void RecursiveSearch(Transform original, Transform root, ref Stack<string> path, ref List<AssetDifference> result)
        {
            path.Push(root.name);

            if (original.childCount < root.childCount)
            {
                int start = original.childCount;
                string currPath = string.Join("/", path.Reverse());
                for (int i = start; i < root.childCount; i++)
                {
                    result.Add(new AssetDifference()
                    {
                        ModelPath = currPath,
                        AssetPath = GetPathFromTransform(Util.FindDescent(asset, root.GetChild(i).name))
                    });
                }
            }

            for (int i = 0; i < original.childCount; i++)
            {
                RecursiveSearch(original.GetChild(i), root.GetChild(i), ref path, ref result);
            }
            path.Pop();
        }

        private string GetPathFromTransform(Transform target)
        {
            var reversePath = new List<string>
            {
                target.name
            };

            while (target.name != asset.name)
            {
                target = target.parent;
                reversePath.Add(target.name);
            }

            reversePath.Reverse();
            return string.Join("/", reversePath);
        }
    }
}
