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

using CameraUtils;
using UnityEngine;

/**
 * <summary>
 * Platform which is controlled by the 3d orientation of the controller of a single player.
 * 
 * It is activated by a <see cref="ColorSwitch"/> and removes the avatar of the player activating the switch as long
 * as they are controlling the platform. While controlling the platform, the 3D orientation of the controller of the
 * player will move this platform.
 *
 * The control is deactivated by the controller <see cref="Button.Exit"/> button.
 *
 * It should be used in conjunction with the <see cref="Platform"/> behavior (and the other components mentioned in the
 * <c>RequireComponent</c> annotations.
 * </summary>
 * <seealso cref="Platform"/>
 */
[RequireComponent(typeof(ColorReplacer))]
[RequireComponent(typeof(Switchable))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Platform))]
[RequireComponent(typeof(CameraTracked))]
public class PlayerControlledPlatform : MonoBehaviour
{
    /**
     * Maximum speed of this platform
     */
    [SerializeField] private float speed = 3.0f;
    /**
     * The switch which will activate this platform
     */
    [SerializeField] private ColorSwitch colorSwitch = default;
    
    private Switchable _switchable;
    private Rigidbody2D _body;
    private ColorReplacer _replacer;
    private CameraTracked _cameraTracked;

    // Player currently controlling this platform (is null, if no player is controlling the platform)
    private PlayerController _player;
    // If active, the platform will be recolored with the color of the player controlling it.
    // We cache the original color of the platform here, so that we can restore it, when the player stops controlling
    // this platform
    private Color _originalColor;

    /**
     * <summary>
     * The player currently controlling this platform. May be <c>null</c>, if no player is currently controlling the
     * platform.
     *
     * When being (un)set, it will take care of adjusting the settings of the controller of the player and recoloring
     * the platform in the color of the player when being controlled.
     * </summary>
     */
    public PlayerController Player
    {
        get => _player;
        set
        {
            // If a player is already currently controlling this platform it will now be replaced / removed.
            // In this case, we have to adjust some controller settings etc.
            if (!ReferenceEquals(_player, null) && value != _player)
            {
                // set the InputMode back to normal (it is set to IMUOrientation when controlling the platform).
                _player.Input.InputMode = InputMode.Normal;
                // exit button is no longer needed
                _player.Input.EnableButtons((Button.Exit, false));
                
                // the previous player shall appear again at the spot where they vanished: The color switch
                _player.PhysicsEffects.Teleport(colorSwitch.transform.position);
                _player.FaceDirection(Direction.Down);

                var oldPlayer = _player;
                Teleport.TeleportIn(
                    _player.gameObject,
                    _player.Color.ToRGB(),
                    () =>
                    {
                        // Stop blocking registering respawn points
                        // See comments further below.
                        if (oldPlayer.TryGetComponent<Spawnable>(out var spawnable))
                        {
                            spawnable.UnregisterTouchingUnsafeTerrain(this);
                        }
                    }
                );
            }
            
            _player = value;
            // If a player is assigned to control this platform (value is not null),
            // we have to adjust some of their controller settings etc.
            if (!ReferenceEquals(_player, null))
            {
                // The platform is to be controlled by the 3d orientation of the controller, hence we adjust the InputMode
                _player.Input.InputMode = InputMode.IMUOrientation;
                // The player should have a button to stop controlling the platform
                _player.Input.EnableButtons((Button.Exit, true));

                // The player does not leave the chasm while controlling the platform, though they do disappear.
                // Hence, we have to temporarily tell Spawnable, that the player is not on safe terrain and should not
                // safe respawn points.
                if (_player.TryGetComponent<Spawnable>(out var spawnable))
                {
                    spawnable.RegisterTouchingUnsafeTerrain(this);
                }

                // the player shall disappear, as long as they are controlling the platform
                Teleport.TeleportOut(_player.gameObject, _player.Color.ToRGB());
            }
            
            // Adjust the color of this platform to the color of the controlling player, or back to the original color
            // when no player is controlling it now
            _replacer.ReplacementColor = value?.Color.ToRGB() ?? _originalColor;
        }
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _replacer = GetComponent<ColorReplacer>();
        _switchable = GetComponent<Switchable>();
        _cameraTracked = GetComponent<CameraTracked>();

        // Make sure, the ColorSwitch activating this platform is registered in the Switchable component which
        // ultimately observes the switch state
        _switchable.AddSwitches(colorSwitch.GetComponent<Switch>());

        _originalColor = _replacer.ReplacementColor;
    }
    
    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void Start()
    {
        // The Switchable component does not trigger this callback by itself for the initial activation when loading the
        // scene. Hence, we look the initial value up ourselves
        OnActivationChanged(_switchable.Activation);
    }

    private void Update()
    {
        // If a player is controlling this platform currently...
        if (!ReferenceEquals(Player, null))
        {
            // ...we move according to their controller orientation
            var delta = new Vector2(
                Player.Input.GetIMUOrientationHorizontal(),
                Player.Input.GetIMUOrientationVertical()
            ) * (speed * Time.deltaTime);
            
            _body.MovePosition(_body.position + delta);

            // The player can stop controlling the platform by pressing the Exit button
            if (Player.Input.GetButtonUp(Button.Exit))
            {
                // if the platform will no longer be controlled, we must reenable the switches which can activate it again.
                // However, we prevent the color switch triggering this platform from immediately reacting to the
                // controlling player who will now appear again.
                colorSwitch.IgnoreContactOnce = Player.gameObject;
                _switchable.enabled = true;
                
                Teleport.TeleportIn(colorSwitch.gameObject, colorSwitch.Color.ToRGB());
                
                Player = null;
                
                // if the platform is no longer being controlled, the camera can stop following it
                _cameraTracked.Tracking = false;
            }
        }
    }
    
    /**
     * <summary>
     * Invoked when the platform control is being activated.
     * Note, that the <see cref="Switchable"/> can not deactivate the platform, this is decided by the player
     * controlling the platform, <see cref="Update"/>.
     * </summary>
     * <seealso cref="Switchable"/>
     */
    void OnActivationChanged(bool activation)
    {
        // If the platform is activated (by the ColorSwitch)...
        if (activation)
        {
            // And we can determine the player who did it...
            if (!ReferenceEquals(colorSwitch.ActivatingPlayer, null))
            {
                // We can deactivate the switch system triggering the platform activation (since the player controls the deactivation now)
                _switchable.enabled = false;
                
                // We set the player to the one how activated the platform
                Player = colorSwitch.ActivatingPlayer;
                // And let the activating switch disappear
                Teleport.TeleportOut(colorSwitch.gameObject, colorSwitch.Color.ToRGB());

                // Also the camera should now follow this platform as it is a player controlled entity now
                _cameraTracked.Tracking = true;
            }

            else
            {
                Debug.LogError("A player controlled platform has been activated, but the activating player could not be determined. This should not be possible and is a programming error.");
            }
        }
    }
}
