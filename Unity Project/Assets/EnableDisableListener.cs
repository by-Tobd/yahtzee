using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class EnableDisableListener : MonoBehaviour
{
    public UnityEvent isEnabledEvent;

    void Start()
    {
        if (isEnabledEvent == null)
            isEnabledEvent = new UnityEvent();
    }

    private void OnEnable()
    {
        isEnabledEvent.Invoke();
    }
}
