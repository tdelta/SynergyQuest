import { ControllerInputSender, Button } from "../src";

// These tests try to connect to a server at 127.0.0.1 and send some nonsense
// input.

let controllerInputSender: ControllerInputSender;

const sleep = (waitTimeInMs) => new Promise(resolve => setTimeout(resolve, waitTimeInMs));

beforeAll(() => {
  return ControllerInputSender.connect("127.0.0.1", 4242)
    .then(sender => controllerInputSender = sender);
});

test("Set Attack button to true, wait a second and then set it to false", () => {
  controllerInputSender.setButton(Button.Attack, true);

  sleep(1000).then(() => {
    controllerInputSender.setButton(Button.Attack, false);
  });
});

test("Set Pull button to true, wait a second and then set it to false", () => {
  controllerInputSender.setButton(Button.Pull, true);

  sleep(1000).then(() => {
    controllerInputSender.setButton(Button.Pull, false);
  });
});

test("Set vertical and horizontal joystick axes to random values", () => {
  var intervalId;
  var times = 0;

  intervalId = setInterval(() => {
    controllerInputSender.setJoystickPosition(Math.random(), Math.random());
    times++;
    
    if (times > 4) {
      clearInterval(intervalId);
    }
  }, 1500);
});
