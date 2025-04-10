using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public class AnimatorChanger : MonoBehaviour
    {
        public Animator Animator;
        public string KeyName;
        public void SetKeyValue(int value)
        {
            Animator.SetInteger(KeyName, value);
        }
        public void SetKeyValue(float value)
        {
            Animator.SetFloat(KeyName, value);
        }
        public void SetKeyValue(bool value)
        {
            Animator.SetBool(KeyName, value);
        }
    }
}