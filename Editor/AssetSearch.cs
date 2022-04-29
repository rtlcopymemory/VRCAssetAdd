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

        private void RecursiveSearch(Transform original, Transform targetAvatar, ref Stack<string> path, ref List<AssetDifference> result)
        {
            path.Push(targetAvatar.name);

            if (original.childCount < targetAvatar.childCount)
            {
                // Find which indeces have been added
                var newObjs = new List<int>();
                for (int i = 0; i < targetAvatar.childCount; i++)
                {
                    var child = targetAvatar.GetChild(i);
                    bool found = false;
                    for (int j = 0; j < original.childCount; j++)
                    {
                        if (child.name == original.GetChild(j).name)
                            found = true;
                    }

                    if (!found)
                        newObjs.Add(i);
                }

                string currPath = string.Join("/", path.Reverse());
                foreach (int i in newObjs)
                {
                    var child = Util.FindDescent(asset, targetAvatar.GetChild(i).name);
                    if (child == null)
                    {
                        throw new VRCAddException($"Could not find '{targetAvatar.GetChild(i).name}' in '{asset.name}'");
                    }

                    result.Add(new AssetDifference()
                    {
                        ModelPath = currPath,
                        AssetPath = GetPathFromTransform(child)
                    });
                }
            }

            for (int i = 0; i < original.childCount; i++)
            {
                RecursiveSearch(original.GetChild(i), targetAvatar.GetChild(i), ref path, ref result);
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
