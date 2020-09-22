// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

﻿using System;
using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Can display a dissolving / teleporting effect of a sprite with different colors using shaders.
 * E.g. players are teleported away using this shader when starting to control a platform, <see cref="PlayerControlledPlatform"/>.
 *
 * You should not directly assign this component to a game object. Instead call the <see cref="TeleportIn"/> and
 * <see cref="TeleportOut"/> methods with the object to apply the effect to any object with a compatible material /
 * shader. (Child objects will also be animated.)
 *
 * The implementation of the shaders used by this material is based on these videos:
 * <list type="bullet">
 *   <item><description><a href="https://www.youtube.com/watch?v=taMp1g1pBeE">Video by Brackeys</a></description></item>
 *   <item><description><a href="https://www.youtube.com/watch?v=dImQy_K5zuk">Video by The Game Dev Shack</a></description></item>
 * </list>
 *
 * This requires the <see cref="SpriteRenderer"/> of the object using this component and / or its child objects to use a
 * material which supports the following properties:
 * <list type="bullet">
 *   <item><description><c>_TeleportColor</c></description></item>
 *   <item><description><c>_TeleportProgress</c></description></item>
 * </list>
 *
 * This can easily be implemented by using the <c>Shaders/SubGraphs/TeleportEffect</c> shader sub-graph in the shader of
 * the material.
 * </summary>
 */
[RequireComponent(typeof(SpriteRenderer))]
public class Teleport : MonoBehaviour
{
    private float _speed = 1.0f;
    private float _timer = 0.0f;
    private bool _isTeleportingOut;
    private Action _onTeleportCompletedCallback = null;

    // The renderers and their material property blocks of this game object and all child objects
    private (Renderer, MaterialPropertyBlock)[] _renderersAndMaterialProperties = null;
    
    private static readonly int ColorProperty = Shader.PropertyToID("_TeleportColor");
    /**
     * This shader property controls the progress of the animation.
     * It ranges from 0 to 1.
     */
    private static readonly int ProgressProperty = Shader.PropertyToID("_TeleportProgress");

    void Awake()
    {
        if (!SupportsTeleportEffect(this.gameObject))
        {
            Debug.LogError(
                "Trying to apply teleport effect to sprite with a material which does not support teleport effects.");
        }

        // Retrieve the renderers of this game object and all child objects.
        // Pair each of them up with a material property block, which we can use later to input parameters to the shader
        _renderersAndMaterialProperties = GetComponentsInChildren<Renderer>()
            .Select(renderer =>
                (
                    renderer,
                    new MaterialPropertyBlock()
                )
            )
            .ToArray();

        // While teleporting, we might loose contact to safe terrain, so we register this behavior as terrain unsafe 
        // for respawning
        foreach (var spawnable in GetComponentsInChildren<Spawnable>())
        {
            spawnable.RegisterTouchingUnsafeTerrain(this);
        }
    }

    /**
     * <summary>
     * Check if the given game object has a renderer with a material / shader which supports this effect.
     * It does not check child objects!
     * </summary>
     */
    public static bool SupportsTeleportEffect(GameObject obj)
    {
        if (obj.TryGetComponent<Renderer>(out var renderer))
        {
            var material = renderer.material;

            return material.HasProperty(ColorProperty) && material.HasProperty(ProgressProperty);
        }

        else return false;
    }

    /**
     * <summary>
     * Play the teleport effect on an visible object to make it disappear.
     * The objects renderer must fulfill the requirements discussed in the class description.
     *
     * Furthermore, the object will be made unable to move by itself and permanently invisible after the effect finishes
     * playing, <see cref="BehaviourExtensions.Freeze"/> and <see cref="BehaviourExtensions.MakeInvisible"/>.
     * </summary>
     * <param name="go">Object which shall be teleported away</param>
     * <param name="color">Color of the teleport effect.</param>
     * <param name="onTeleportCompletedCallback">
     *     If not set to null, will be invoked as soon as the teleport effect finishes.
     * </param>
     * <param name="speed">
     *   Speed modifier for the teleport effect.
     *   E.g. use 2.0f for twice the speed and 0.5f for half the speed.
     * </param>
     */
    public static void TeleportOut(
        GameObject go,
        Color color,
        Action onTeleportCompletedCallback = null,
        float speed = 0.75f
    )
    {
        go.Freeze();

        // Is there already a teleport going on?
        if (go.TryGetComponent(out Teleport instance))
        {
            // If so, we can reuse the component.
            // However, we might need to inverse the progress timer, if the old teleport component played the effect in
            // the opposite direction.
            if (!instance._isTeleportingOut)
            {
                instance._timer = 1.0f - instance._timer;
            }
        }
        
        else
        {
            // Otherwise, we need to create a new instance
            instance = go.AddComponent<Teleport>();
            instance.SetProgress(0.0f);
        }
        
        instance._speed = speed;
        instance._isTeleportingOut = true;
        instance._onTeleportCompletedCallback = onTeleportCompletedCallback;
        instance.SetColor(color);
    }
    
    /**
     * <summary>
     * Play the teleport effect on an invisible object to make it appear.
     * The objects renderer must fulfill the requirements discussed in the class description.
     * 
     * Furthermore, the object will be made able to move by itself (again) and permanently visible,
     * <see cref="BehaviourExtensions.UnFreeze"/> and <see cref="BehaviourExtensions.MakeVisible"/>.
     * </summary>
     * <param name="go">Object which shall be teleported in</param>
     * <param name="color">Color of the teleport effect.</param>
     * <param name="onTeleportCompletedCallback">
     *     If not set to null, will be invoked as soon as the teleport effect finishes.
     * </param>
     * <param name="speed">
     *   Speed modifier for the teleport effect.
     *   E.g. use 2.0f for twice the speed and 0.5f for half the speed.
     * </param>
     */
    public static void TeleportIn(
        GameObject go,
        Color color,
        Action onTeleportCompletedCallback = null,
        float speed = 0.75f
    )
    {
        // Is there already a teleport going on?
        if (go.TryGetComponent(out Teleport instance))
        {
            // If so, we can reuse the component.
            // However, we might need to inverse the progress timer, if the old teleport component played the effect in
            // the opposite direction.
            if (instance._isTeleportingOut)
            {
                instance._timer = 1.0f - instance._timer;
            }
        }

        else
        {
            // Otherwise, we need to create a new instance
            instance = go.AddComponent<Teleport>();
            instance.SetProgress(1.0f);
        }
        
        instance._speed = speed;
        instance._isTeleportingOut = false;
        instance._onTeleportCompletedCallback = onTeleportCompletedCallback;
        instance.SetColor(color);
        
        go.MakeVisible();
    }

    private void Update()
    {
        // advance the animation:
        _timer = Mathf.Min(1.0f, _timer + Time.deltaTime * _speed);
        
        // If the animation is still running...
        // (it is completed when the `ProgressProperty` reaches 1.0f)
        if (_timer < 1.0f)
        {
            SetProgress(
                _isTeleportingOut ?
                    _timer :
                    1.0f - _timer // reverse the effect when teleporting an object in instead of out
            );
        }

        else
        {
            // Make object permanently invisible, if this is the teleporting out effect
            if (_isTeleportingOut)
            {
                this.gameObject.MakeInvisible();
            }

            // Otherwise, if they are reappearing / teleporting in, make them able to move again.
            else
            {
                this.gameObject.UnFreeze();
            }
            
            // Users can set a callback to be triggered when the teleport effect finishes.
            // If present, we cache it now before destroying this behavior to invoke it last:
            var onTeleportCompletedCallback = _onTeleportCompletedCallback;
            _onTeleportCompletedCallback = null;
            
            // While teleporting, we might loose contact to safe terrain, so we registered this behavior as terrain unsafe 
            // for respawning.
            // Now that the teleport has finished, we can unregister it again.
            foreach (var spawnable in GetComponentsInChildren<Spawnable>())
            {
                spawnable.UnregisterTouchingUnsafeTerrain(this);
            }

            // Destroy this behaviour when the effect finished
            DestroyImmediate(this);
            
            onTeleportCompletedCallback?.Invoke();
        }
    }

    /**
     * <summary>
     * Tells the shader how far the animation has progressed for every child object.
     * </summary>
     * <param name="progress">How far the animation progressed. Must be in the interval [0; 1]</param>
     */
    private void SetProgress(float progress)
    {
        foreach (var (renderer, materialProperties) in _renderersAndMaterialProperties)
        {
            renderer.GetPropertyBlock(materialProperties);
            materialProperties.SetFloat(ProgressProperty, progress);
            
            renderer.SetPropertyBlock(materialProperties);
        }
    }
    
    /**
     * <summary>
     * Tells the shader how to color the animation for every child object.
     * </summary>
     */
    private void SetColor(Color color)
    {
        foreach (var (renderer, materialProperties) in _renderersAndMaterialProperties)
        {
            renderer.GetPropertyBlock(materialProperties);
            materialProperties.SetColor(ColorProperty, color);
            
            renderer.SetPropertyBlock(materialProperties);
        }
    }
}
