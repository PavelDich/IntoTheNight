using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public interface IDamageble
    {
        public void Damage(NetworkIdentity shooter, float damage);
    }
}