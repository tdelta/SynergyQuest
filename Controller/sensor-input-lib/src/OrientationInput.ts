/**
 * This file is part of the "Synergy Quest" game
 * (github.com/tdelta/SynergyQuest).
 *
 * Copyright (c) 2020
 *   Marc Arnold     (m_o_arnold@gmx.de)
 *   Martin Kerscher (martin_x@live.de)
 *   Jonas Belouadi  (jonas.belouadi@posteo.net)
 *   Anton W Haubner (anton.haubner@outlook.de)
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation; either version 3 of the License, or (at your option) any
 * later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program; if not, see <https://www.gnu.org/licenses>.
 *
 * Additional permission under GNU GPL version 3 section 7 apply,
 * see `LICENSE.md` at the root of this source code repository.
 */

import { boundClass } from 'autobind-decorator';
import { RelativeOrientationSensor } from 'motion-sensors-polyfill';

/**
 * A 4-dimensional vector that can express rotations
 */
type Quaternion = [number, number, number, number];

/**
 * Angles which describe orientation in 3d space,
 * see:
 * https://en.wikipedia.org/wiki/Euler_angles
 * https://en.wikipedia.org/wiki/Aircraft_principal_axes
 *
 * The Generic Sensor Web API supplies us the controller orientation as a
 * quaternion, however, for our purposes this representation is more suited, see
 * also `OrientationInput.quaternionToTaitBryan`.
 */
interface TaitBryanAngle {
  yaw: number;
  pitch: number;
  roll: number;
}

/**
 * Library which allows to easily interpret orientation of a device in 3d space
 * as 2d directional input using the Generic Sensor Web API:
 * https://developer.mozilla.org/en-US/docs/Web/API/Sensor_APIs
 *
 * We interpret roll as a value on a vertical axis in [-1; 1] and pitch as a
 * value on a horizontal axis in [-1; 1], see also `TaitBryanAngle`.
 *
 * Be aware, that this library only works in a secure context:
 * https://developer.mozilla.org/en-US/docs/Web/Security/Secure_Contexts/features_restricted_to_secure_contexts
 *
 * E.g. all connections must be SSL encrypted.
 *
 * Set the callbacks `onWarning`, `onFatelError`, `onActivated` and
 * `onInputChange` to be informed about changes in the sensor input automatically.
 */
@boundClass
export class OrientationInput {
  /**
   * Minimum absolute values for roll and pitch before we start interpreting them.
   * This way we filter out small unintentional movements of the device.
   */
  private static readonly PitchThreshold = 0.1;
  private static readonly RollThreshold = 0.05;
  /**
   * Maximum absolute values for pitch and roll. Any value above this is
   * interpreted as 1 or -1 on the horizontal and vertical 2d axes.
   */
  private static readonly PitchMax = 0.2;
  private static readonly RollMax = 0.3;

  private sensor?: RelativeOrientationSensor;

  /**
   * Interpreted values on the 2d axes
   * @private
   */
  private vertical: number = 0;
  private horizontal: number = 0;
  /**
   * Raw sensor input
   * @private
   */
  private rawQuaternion: Quaternion = [0, 0, 0, 0];
  /**
   * Almost raw sensor input, interpreted as Tait-Bryan / Euler angles instead of
   * an quaternion
   * @private
   */
  private rawAngle: TaitBryanAngle = {
    yaw: 0,
    pitch: 0,
    roll: 0,
  };

  public onWarning: (message: string) => any = _ => {};
  public onFatalError: (message: string) => any = _ => {};
  public onActivated: () => any = () => {};
  public onInputChange: (vertical: number, horizontal: number) => any = _ => {};

  /**
   * Acquire the necessary permissions and start reading sensor values.
   * Set the above callbacks before calling this method.
   *
   * Remember to call `stop` when no more input is required.
   */
  public start() {
    (async () => {
      if (await this.checkPermissions()) {
        this.initSensor();
      } else {
        this.handleFatalError(
          undefined,
          'Could not gain required permissions.'
        );
      }
    })();
  }

  /**
   * Stop reading sensor values.
   */
  public stop() {
    this.sensor?.stop();
  }

  /**
   * 3d orientation (roll) interpreted as value on a single axis in [-1; 1].
   *
   * Imagine holding the phone in both hands in any orientation
   * (primary landscape, secondary landscape, portrait).
   * Tilting the phone so that the upper part goes away from you gives you
   * increasingly positive values on this axis.
   * Tilting the upper part towards you gives you negative values.
   */
  public getVertical(): number {
    return this.vertical;
  }

  /**
   * 3d orientation (roll) interpreted as value on a single axis in [-1; 1].
   *
   * Imagine holding the phone in both hands in any orientation
   * (primary landscape, secondary landscape, portrait).
   * Tilting the phone so that the left part goes up and the right part goes
   * down gives you increasingly positive values on this axis.
   * The reverse gives you negative values.
   */
  public getHorizontal(): number {
    return this.horizontal;
  }

  public getRawAngle(): TaitBryanAngle {
    return this.rawAngle;
  }

  public getRawQuaternion(): Quaternion {
    return this.rawQuaternion;
  }

  private initSensor() {
    this.sensor = new RelativeOrientationSensor({
      frequency: 60,
      referenceFrame: 'screen', // let the Sensor API automatically handle flipped screen orientations etc.
    });

    this.sensor.onerror = this.handleSensorError;
    this.sensor.onreading = this.handleSensorReading;
    this.sensor.onactivate = this.handleSensorActivation;

    this.sensor.start();
  }

  handleSensorActivation() {
    this.onActivated();
  }

  handleSensorError(event: Event) {
    if (event instanceof ErrorEvent) {
      if (event.error.name === 'NotReadableError') {
        this.handleFatalError(event.error, 'Required sensors are not present.');
      } else {
        this.handleWarning(event.error);
      }
    } else {
      this.handleWarning();
    }
  }

  handleSensorReading(_: Event) {
    if (this.sensor != null) {
      // Acquire raw readings
      this.rawQuaternion = this.sensor.quaternion;
      // Make them more easily interpretable by converting to euler angles
      this.rawAngle = OrientationInput.quaternionToTaitBryan(
        this.rawQuaternion
      );

      // Interpret roll and pitch as 2d axis values, see `GetVertical` and `GetHorizontal`
      const newVertical = OrientationInput.axisValueFromAngle(
        this.rawAngle.roll,
        OrientationInput.RollThreshold,
        OrientationInput.RollMax
      );
      const newHorizontal = OrientationInput.axisValueFromAngle(
        this.rawAngle.pitch,
        OrientationInput.PitchThreshold,
        OrientationInput.PitchMax
      );

      if (this.vertical !== newVertical || this.horizontal !== newHorizontal) {
        this.vertical = newVertical;
        this.horizontal = newHorizontal;

        this.onInputChange(this.vertical, this.horizontal);
      }
    } else {
      this.handleWarning(
        undefined,
        'Tried to process sensor reading, but sensor is not fully initialized. This should never happen and is a programming error.'
      );
    }
  }

  private handleFatalError(
    error?: { message: string },
    additionalMessage?: string
  ) {
    if (additionalMessage != null) {
      this.onFatalError(additionalMessage);
    } else if (error != null) {
      this.onFatalError(error.message);
    } else {
      this.onFatalError(
        'Some fatal error occured and sensor input will not work correctly.'
      );
    }
  }

  private handleWarning(
    error?: { message: string },
    additionalMessage?: string
  ) {
    if (additionalMessage != null) {
      this.onWarning(additionalMessage);
    } else if (error != null) {
      this.onWarning(error.message);
    } else {
      this.onWarning('Some error occured.');
    }
  }

  private async checkPermissions(): Promise<boolean> {
    // Based on https://github.com/intel/generic-sensor-demos/tree/master/orientation-phone
    if (navigator.permissions) {
      // https://w3c.github.io/orientation-sensor/#model
      try {
        const results = await Promise.all([
          navigator.permissions.query({ name: 'accelerometer' }),
          navigator.permissions.query({ name: 'magnetometer' }),
          navigator.permissions.query({ name: 'gyroscope' }),
        ]);

        return results.every(result => result.state === 'granted');
      } catch (error) {
        this.handleWarning(
          error,
          'Error when attempting to gain necessary permissions. Will still continue though.'
        );
      }
    } else {
      this.handleWarning(
        undefined,
        'No Permissions API. Will still continue though.'
      );
    }

    return true;
  }

  /**
   * https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Quaternion_to_Euler_Angles_Conversion
   * Slightly adapted, so that roll is flipped around and equal to 0 when phone is flat on table with screen upwards
   *
   * @private
   */
  private static quaternionToTaitBryan(q: Quaternion): TaitBryanAngle {
    return {
      yaw: Math.atan2(
        2 * (q[0] * q[1] + q[2] * q[3]),
        1 - 2 * (q[1] * q[1] + q[2] * q[2])
      ),
      pitch: Math.asin(2 * (q[0] * q[2] - q[3] * q[1])),
      roll: Math.atan2(
        2 * (q[0] * q[3] + q[1] * q[2]),
        2 * (q[2] * q[2] + q[3] * q[3]) - 1
      ),
    };
  }

  /**
   * Interprets an euler angle as a value in [-1; 1]
   *
   * @param angle angle to be interpreted
   * @param threshold the absolute angle must be greater than this to get a result other than 0 (this filters noise)
   * @param max if the absolute angle is greater or equal than this, you get a maximum / minimum result value (1 or -1) depending on the sign on the angle
   * @private
   */
  private static axisValueFromAngle(
    angle: number,
    threshold: number,
    max: number
  ): number {
    const absAngle = Math.abs(angle);

    if (absAngle > threshold) {
      if (angle > max) {
        return -1.0; // negative, since mathematical positive rotation direction is anti-clockwise
      } else if (angle < -max) {
        return 1.0;
      } else {
        return (-Math.sign(angle) * (absAngle - threshold)) / (max - threshold);
      }
    } else {
      return 0;
    }
  }
}
