// ----------------------------------------------------------------------------
// MIT License
//
// Copyright (c) 2018 Ryan Hipple
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// Unite 2017 - Game Architecture with Scriptable Objects
//
// Original author: Ryan Hipple, https://github.com/roboryantron/Unite2017
// ----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

namespace MeasVRe.Events
{
    public class InputEventListener : MonoBehaviour
    {
        [Tooltip("Event to register to.")]
        public InputEvent inputEvent;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent<GameObject> Response;

        private void OnEnable()
        {
            inputEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            inputEvent.UnregisterListener(this);
        }

        /// <summary> Invoke the response. </summary>
        /// <param name="obj"> The object that raised the event. </param>
        public void OnEventRaised(GameObject obj)
        {
            Response.Invoke(obj);
        }
    }
}
