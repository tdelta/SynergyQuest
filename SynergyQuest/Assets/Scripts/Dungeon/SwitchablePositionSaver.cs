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

using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Saves the positions of objects persistently across scenes, depending on the state of switches.
 * For example, with this, one can save the positions of Sokoban boxes only, iff the Sokoban was solved correctly.
 *
 * It requires the saved objects to have the <see cref="Guid"/> component.
 * This component only triggers the save/removal of positions. The actual lookup of saved positions is performed by the
 * <see cref="Guid"/> component.
 * </summary>
 */
[RequireComponent(typeof(Switchable))]
public class SwitchablePositionSaver : MonoBehaviour
{
    /**
     * Objects whose position shall be saved, when this switchable is activated
     */
    [SerializeField] private Guid[] objectsToSave = new Guid[0];
    
    /**
     * Automatically find all objects in the scene who have a tag of the given name and save their positions, iff
     * the switchable is activated
     */
    [SerializeField] private string[] autoDiscoveredTags = new string[0];
    
    private Switchable _switchable;

    private void Awake()
    {
        _switchable = GetComponent<Switchable>();

        objectsToSave = autoDiscoveredTags
            .SelectMany(GameObject.FindGameObjectsWithTag)
            .Select(gameObject => gameObject.GetComponent<Guid>())
            .Where(maybeGuid =>
            {
                if (maybeGuid is null)
                {
                    Debug.LogError(
                        "All objects for which a position shall be saved must have a Guid component. However, while searching for objects to save by the given tags, there was one included without a Guid component");
                    return false;
                }

                return true;
            })
            .Concat(objectsToSave)
            .ToArray();
    }

    private void Start()
    {
        // The Switchable component does not trigger this callback by itself for the initial activation when loading the
        // scene. Hence, we look the initial value up ourselves
        OnActivationChanged(this._switchable.Activation);
    }

    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void OnActivationChanged(bool activation)
    {
        foreach (var guid in objectsToSave)
        {
            if (activation)
            {
                DungeonDataKeeper.Instance.SavePosition(guid);
            }

            else
            {
                DungeonDataKeeper.Instance.RemovePosition(guid);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Make spawner visible in the editor by displaying an icon
        Gizmos.DrawIcon(transform.position, "floppy.png", true);
    }
}
