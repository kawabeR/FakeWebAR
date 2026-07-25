mergeInto(LibraryManager.library, {

  DeviceOrientationBridge_NeedsPermission: function () {
    if (typeof DeviceOrientationEvent !== 'undefined' &&
        typeof DeviceOrientationEvent.requestPermission === 'function') {
      return 1;
    }
    return 0;
  },

  DeviceOrientationBridge_RequestPermission: function (gameObjectNamePtr) {
    var goName = UTF8ToString(gameObjectNamePtr);
    DeviceOrientationEvent.requestPermission().then(function (state) {
      SendMessage(goName, 'OnOrientationPermissionResult', state === 'granted' ? '1' : '0');
    }).catch(function () {
      SendMessage(goName, 'OnOrientationPermissionResult', '0');
    });
  },

  DeviceOrientationBridge_StartListening: function () {
    if (window.__fakeARDeviceOrientationHandler) {
      return;
    }
    window.__fakeARAlpha = 0;
    window.__fakeARBeta = 0;
    window.__fakeARGamma = 0;
    window.__fakeARDeviceOrientationHandler = function (event) {
      window.__fakeARAlpha = event.alpha || 0;
      window.__fakeARBeta = event.beta || 0;
      window.__fakeARGamma = event.gamma || 0;
    };
    window.addEventListener('deviceorientation', window.__fakeARDeviceOrientationHandler, true);
  },

  DeviceOrientationBridge_GetAlpha: function () {
    return window.__fakeARAlpha || 0;
  },

  DeviceOrientationBridge_GetBeta: function () {
    return window.__fakeARBeta || 0;
  },

  DeviceOrientationBridge_GetGamma: function () {
    return window.__fakeARGamma || 0;
  },

  DeviceOrientationBridge_GetScreenOrientationAngle: function () {
    if (typeof window.orientation === 'number') {
      return window.orientation;
    }
    if (window.screen && window.screen.orientation && typeof window.screen.orientation.angle === 'number') {
      return window.screen.orientation.angle;
    }
    return 0;
  }

});
