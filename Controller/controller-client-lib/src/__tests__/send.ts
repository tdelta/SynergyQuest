import { ControllerClient, Button } from '../index';

// These tests try to connect to a server at 127.0.0.1 and send some nonsense
// input.

let controllerClient: ControllerClient;

const sleep = (waitTimeInMs: number) =>
  new Promise(resolve => setTimeout(resolve, waitTimeInMs));

beforeAll(() => {
  return new Promise(resolve => {
    controllerClient = new ControllerClient();

    controllerClient.onReady = resolve;

    controllerClient.connect('MaxMuster', '127.0.0.1');
  });
});

test('Set Attack button to true, wait a second and then set it to false', () => {
  controllerClient.setButton(Button.Attack, true);

  sleep(1000).then(() => {
    controllerClient.setButton(Button.Attack, false);
  });
});

test('Set Pull button to true, wait a second and then set it to false', () => {
  controllerClient.setButton(Button.Pull, true);

  sleep(1000).then(() => {
    controllerClient.setButton(Button.Pull, false);
  });
});

test('Set vertical and horizontal joystick axes to random values', () => {
  var intervalId: any;
  var times = 0;

  intervalId = setInterval(() => {
    controllerClient.setJoystickPosition(Math.random(), Math.random());
    times++;

    if (times > 4) {
      clearInterval(intervalId);
    }
  }, 1500);
});
