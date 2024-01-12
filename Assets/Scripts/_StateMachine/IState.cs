using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface IState<T>
    {
        void OnStateEnter(T t);
        void OnStateUpdate(T t);
        void OnStateExit(T t);
    }
}