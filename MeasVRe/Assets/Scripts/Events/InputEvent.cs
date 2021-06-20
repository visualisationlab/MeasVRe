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

using System.Collections.Generic;
using UnityEngine;

namespace MeasVRe.Events
{
    [CreateAssetMenu(fileName = "InputEvent", menuName = "MeasVRe/InputEvent", order = 0)]

    public class InputEvent : ScriptableObject
    {
        public List<InputEventListener> eventListeners = new List<InputEventListener>();

        /// <summary> Invoke the response event of each listener. </summary>
        /// <param name="obj">The object that raised the event.</param>
        public void Raise(GameObject obj)
        {
            foreach (InputEventListener listener in eventListeners)
                listener.OnEventRaised(obj);
        }

        /// <summary> Register a listener to this event. </summary>
        /// <param name="listener"> The new listener. </param>
        public void RegisterListener(InputEventListener listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        /// <summary> Unregister a listener from this event. </summary>
        /// <param name="listener"> The listener to unregister. </param>
        public void UnregisterListener(InputEventListener listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }
}
