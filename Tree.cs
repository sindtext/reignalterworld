using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace BehaviorTree
{
    public abstract class Tree : NetworkBehaviour
    {
        public Node _root = null;

        protected void Start()
        {
            firstSpwan();
        }

        private void Update()
        {
            if (_root != null)
                _root.Evaluate();
        }

        protected abstract Node SetupTree();

        protected abstract void firstSpwan();

        protected abstract void RPC_reSpawn(RpcInfo info = default);

        protected abstract void RPC_vlnDamage(NetworkObject sender, float damage, RpcInfo info = default);

        protected abstract void RPC_setDuplicate(RpcInfo info = default);
    }

}