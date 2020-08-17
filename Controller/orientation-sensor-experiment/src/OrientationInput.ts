import { boundClass } from 'autobind-decorator';
import { RelativeOrientationSensor } from 'motion-sensors-polyfill';

type Quaternion = [number, number, number, number];

interface TaitBryanAngle {
  yaw: number;
  pitch: number;
  roll: number;
}

@boundClass
export class OrientationInput {
  private static readonly PitchThreshold = 0.1;
  private static readonly RollThreshold = 0.05;
  private static readonly PitchMax = 0.20;
  private static readonly RollMax = 0.30;

  private sensor?: RelativeOrientationSensor;

  private vertical: number = 0;
  private horizontal: number = 0;
  private rawQuaternion: Quaternion = [0, 0, 0, 0];
  private rawAngle: TaitBryanAngle = {
    yaw: 0, pitch: 0, roll: 0
  };

  public onWarning: (message: string) => any = _ => {}
  public onFatalError: (message: string) => any = _ => {}
  public onActivated: () => any = () => {}
  public onInputChange: (vertical: number, horizontal: number) => any = _ => {}

  public start() {
    (async () => {
      if (await this.checkPermissions()) {
        this.initSensor()
      }

      else {
        this.handleFatalError(undefined, 'Could not gain required permissions.')
      }
    })();
  }

  public stop() {
    this.sensor?.stop();
  }

  public getVertical(): number {
    return this.vertical;
  }

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
    this.sensor = new RelativeOrientationSensor({ frequency: 60, referenceFrame: 'screen'});

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
        this.handleFatalError(event.error, 'Required sensors are not present.')
      }

      else {
        this.handleWarning(event.error)
      }
    }

    else {
      this.handleWarning();
    }
  }

  handleSensorReading(_: Event) {
    if (this.sensor != null) {
      this.rawQuaternion = this.sensor.quaternion;
      this.rawAngle = OrientationInput.quaternionToTaitBryan(this.rawQuaternion);

      const newVertical = OrientationInput.axisValueFromAngle(this.rawAngle.roll, OrientationInput.RollThreshold, OrientationInput.RollMax);
      const newHorizontal = OrientationInput.axisValueFromAngle(this.rawAngle.pitch, OrientationInput.PitchThreshold, OrientationInput.PitchMax);

      if (this.vertical !== newVertical || this.horizontal !== newHorizontal) {
        this.vertical = newVertical;
        this.horizontal = newHorizontal;

        this.onInputChange(this.vertical, this.horizontal);
      }
    }

    else {
      this.handleWarning(undefined, 'Tried to process sensor reading, but sensor is not fully initialized. This should never happen and is a programming error.');
    }
  }

  private handleFatalError(error?: {message: string}, additionalMessage?: string) {
    if (additionalMessage != null) {
      this.onFatalError(additionalMessage);
    }

    else if (error != null) {
      this.onFatalError(error.message);
    }

    else {
      this.onFatalError('Some fatal error occured and sensor input will not work correctly.')
    }
  }

  private handleWarning(error?: {message: string}, additionalMessage?: string) {
    if (additionalMessage != null) {
      this.onWarning(additionalMessage);
    }

    else if (error != null) {
      this.onWarning(error.message);
    }

    else {
      this.onWarning('Some error occured.')
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
        this.handleWarning(error, 'Error when attempting to gain necessary permissions. Will still continue though.');
      }
    } else {
      this.handleWarning(undefined, 'No Permissions API. Will still continue though.');
    }

    return true;
  }

  private static quaternionToTaitBryan(q: Quaternion): TaitBryanAngle {
    // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Quaternion_to_Euler_Angles_Conversion
    // Slightly adapted, so that roll is 0 when phone is flat on table with screen upwards
    return {
      yaw: Math.atan2(2 * (q[0] * q[1] + q[2] * q[3]), 1 - 2 * (q[1] * q[1] + q[2] * q[2])),
      pitch: Math.asin(2 * (q[0] * q[2] - q[3] * q[1])),
      roll: Math.atan2(2 * (q[0] * q[3] + q[1] * q[2]), 2 * (q[2] * q[2] + q[3] * q[3]) - 1),
    };
  }

  private static axisValueFromAngle(angle: number, threshold: number, max: number): number {
    const absAngle = Math.abs(angle);

    if (absAngle > threshold) {
      if (angle > max) {
        return -1.0; // negative, since mathematical positive rotation direction is anti-clockwise
      }

      else if (angle < -max) {
        return 1.0
      }

      else {
        return -Math.sign(angle) * (absAngle - threshold) / (max - threshold);
      }
    }

    else {
      return 0;
    }
  }
}